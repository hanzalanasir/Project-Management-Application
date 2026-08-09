using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Projects.DeleteProject;

/// <summary>
/// The <c>ProjectDeleted</c> audit row is written BEFORE <c>Projects.Remove</c>, in the same
/// <c>SaveChangesAsync</c> — order matters here because the row (and its cascaded dependents) is
/// gone by the time this call returns, so the snapshot summary must be captured first (spec
/// US-002-05, FR-012).
/// </summary>
public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLog;

    public DeleteProjectCommandHandler(
        IApplicationDbContext db,
        IProjectAccessPolicy accessPolicy,
        ICurrentUserService currentUserService,
        IActivityLogService activityLog)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _currentUserService = currentUserService;
        _activityLog = activityLog;
    }

    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var project = await _db.Projects.SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (project is null)
        {
            return Result.Failure(new Error(ErrorKind.NotFound, "Project not found"));
        }

        var canMutate = await _accessPolicy.CanMutateAsync(project, caller, cancellationToken);
        if (!canMutate.Allowed)
        {
            return Result.Failure(new Error(ErrorKind.Forbidden, "You do not have access to this project."));
        }

        // Audit BEFORE removal — the snapshot summary is written first, in the same unit of work,
        // so the ProjectDeleted row survives even though the project row (and its cascaded
        // dependents) is gone by the time this call returns (spec US-002-05, FR-012).
        await _activityLog.LogAsync(
            caller.UserId, nameof(AuditAction.ProjectDeleted), "Project", project.Id.ToString(),
            $"Project '{project.Name}' deleted", cancellationToken);

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
