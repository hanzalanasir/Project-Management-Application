using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Projects.UpdateProject;

/// <summary>
/// <c>CanMutateAsync</c> is re-evaluated here at write time against a freshly-loaded entity, never
/// trusted from an earlier read (FR-010) — a stale read must not authorize a later mutation. The
/// client's <c>If-Match</c> value drives EF's original <c>xmin</c>, so PostgreSQL (not application
/// code) performs the actual optimistic-concurrency check (research R-2).
/// </summary>
public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result<ProjectDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectAccessPolicy _accessPolicy;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLog;
    private readonly ProjectsOptions _options;

    public UpdateProjectCommandHandler(
        IApplicationDbContext db,
        IProjectAccessPolicy accessPolicy,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        IActivityLogService activityLog,
        IOptions<ProjectsOptions> options)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _activityLog = activityLog;
        _options = options.Value;
    }

    public async Task<Result<ProjectDetailDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var project = await _db.Projects.Include(p => p.Owner).SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (project is null)
        {
            return Result<ProjectDetailDto>.Failure(new Error(ErrorKind.NotFound, "Project not found"));
        }

        // Re-evaluated NOW, never trusted from an earlier read (FR-010) — before any mutation.
        var canMutate = await _accessPolicy.CanMutateAsync(project, caller, cancellationToken);
        if (!canMutate.Allowed)
        {
            return Result<ProjectDetailDto>.Failure(new Error(ErrorKind.Forbidden, "You do not have access to this project."));
        }

        if (!Enum.TryParse<ProjectStatus>(request.Status, out var parsedStatus))
        {
            return Result<ProjectDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["status"] = new[] { $"'{request.Status}' is not a recognized project status." } }));
        }

        var transferringOwnership = request.OwnerId.HasValue && request.OwnerId.Value != project.OwnerId;
        ApplicationUser? newOwner = null;
        if (transferringOwnership)
        {
            if (!_options.AllowOwnershipTransfer)
            {
                return Result<ProjectDetailDto>.Failure(new Error(ErrorKind.Forbidden, "Ownership transfer is disabled."));
            }

            // Admin-only — a ProjectManager's own ownerId is never even reachable here since
            // CanMutateAsync already denied them unless they are the current owner, and this check
            // additionally refuses a PM attempting to hand the project to someone else.
            if (caller.Role != nameof(Role.Admin))
            {
                return Result<ProjectDetailDto>.Failure(new Error(ErrorKind.Forbidden, "Only an Admin may transfer project ownership."));
            }

            newOwner = await _db.Users.SingleOrDefaultAsync(u => u.Id == request.OwnerId!.Value, cancellationToken);
            if (newOwner is null)
            {
                return Result<ProjectDetailDto>.Failure(new Error(
                    ErrorKind.Validation, "Validation failed",
                    new Dictionary<string, string[]> { ["ownerId"] = new[] { "The specified owner does not exist." } }));
            }

            var newOwnerRoles = await _userManager.GetRolesAsync(newOwner);
            if (!newOwnerRoles.Contains(nameof(Role.ProjectManager)) && !newOwnerRoles.Contains(nameof(Role.Admin)))
            {
                return Result<ProjectDetailDto>.Failure(new Error(
                    ErrorKind.Validation, "Validation failed",
                    new Dictionary<string, string[]> { ["ownerId"] = new[] { "The owner must hold the ProjectManager or Admin role." } }));
            }
        }

        var changedFields = new List<string>();
        if (project.Name != request.Name) changedFields.Add("name");
        if (project.Description != request.Description) changedFields.Add("description");
        if (project.StartDate != request.StartDate) changedFields.Add("startDate");
        if (project.EndDate != request.EndDate) changedFields.Add("endDate");
        if (project.Status != parsedStatus) changedFields.Add("status");

        project.Name = request.Name;
        project.Description = request.Description;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.Status = parsedStatus;
        project.UpdatedAt = DateTimeOffset.UtcNow;

        var previousOwnerEmail = project.Owner.Email;
        if (transferringOwnership)
        {
            project.OwnerId = newOwner!.Id;
            project.Owner = newOwner;
        }

        // The client's If-Match value drives the xmin check, not whatever this context happened to
        // load — a stale supplied version must fail even if nothing else raced in between (research R-2).
        _db.Entry(project).Property(p => p.Version).OriginalValue = request.IfMatchVersion;

        var changeSummary = changedFields.Count > 0
            ? $"Project '{project.Name}' updated — changed: {string.Join(", ", changedFields)}"
            : $"Project '{project.Name}' updated — no field changes";

        await _activityLog.LogAsync(caller.UserId, nameof(AuditAction.ProjectUpdated), "Project", project.Id.ToString(), changeSummary, cancellationToken);

        if (transferringOwnership)
        {
            await _activityLog.LogAsync(
                caller.UserId, nameof(AuditAction.ProjectOwnerChanged), "Project", project.Id.ToString(),
                $"Owner changed from {previousOwnerEmail} to {newOwner!.Email}", cancellationToken);
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ProjectDetailDto>.Failure(new Error(ErrorKind.Conflict, "This project was modified by someone else. Reload and try again."));
        }

        return Result<ProjectDetailDto>.Success(project.ToDetailDto());
    }
}
