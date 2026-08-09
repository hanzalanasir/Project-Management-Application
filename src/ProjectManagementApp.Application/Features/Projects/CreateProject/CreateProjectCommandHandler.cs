using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Projects.CreateProject;

/// <summary>
/// Owner is taken from the token for a ProjectManager, never the request body (ownership cannot be
/// forged, spec T.8); an Admin-specified owner is validated by role via <c>UserManager</c>, not a
/// direct query — <see cref="IApplicationDbContext"/> exposes no <c>user_roles</c> join (research R-6).
/// </summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLog;
    private readonly ProjectsOptions _options;

    public CreateProjectCommandHandler(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        IActivityLogService activityLog,
        IOptions<ProjectsOptions> options)
    {
        _db = db;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _activityLog = activityLog;
        _options = options.Value;
    }

    public async Task<Result<ProjectDetailDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        // Ownership can never be forged: for a ProjectManager the owner is always the caller,
        // regardless of what the body says (spec T.8). Only an Admin may set a different owner.
        var ownerId = caller.Role == nameof(Role.ProjectManager) ? caller.UserId : request.OwnerId ?? caller.UserId;

        var owner = await _db.Users.SingleOrDefaultAsync(u => u.Id == ownerId, cancellationToken);
        if (owner is null)
        {
            return Result<ProjectDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["ownerId"] = new[] { "The specified owner does not exist." } }));
        }

        // research R-6: the owner must hold ProjectManager or Admin — checked via UserManager,
        // never a direct user_roles join (IApplicationDbContext exposes no such DbSet).
        var ownerRoles = await _userManager.GetRolesAsync(owner);
        if (!ownerRoles.Contains(nameof(Role.ProjectManager)) && !ownerRoles.Contains(nameof(Role.Admin)))
        {
            return Result<ProjectDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["ownerId"] = new[] { "The owner must hold the ProjectManager or Admin role." } }));
        }

        var status = string.IsNullOrWhiteSpace(request.Status) ? _options.DefaultStatus : request.Status;
        if (!Enum.TryParse<ProjectStatus>(status, out var parsedStatus))
        {
            return Result<ProjectDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["status"] = new[] { $"'{status}' is not a recognized project status." } }));
        }

        var now = DateTimeOffset.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = parsedStatus,
            OwnerId = ownerId,
            Owner = owner,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Projects.Add(project);

        await _activityLog.LogAsync(
            actorId: caller.UserId,
            action: nameof(AuditAction.ProjectCreated),
            entityType: "Project",
            entityId: project.Id.ToString(),
            changeSummary: $"Project '{project.Name}' created, owned by {owner.Email}",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        return Result<ProjectDetailDto>.Success(project.ToDetailDto());
    }
}
