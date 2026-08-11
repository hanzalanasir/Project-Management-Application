using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Team.RemoveTeamMember;

/// <summary>
/// Fixed order (data-model.md §5): load the parent project (404) → <c>CanManageTeamAsync</c> (403)
/// → confirm membership (404) → <see cref="OpenAssignedTaskCheck"/> (409 with the blocking count —
/// a TOTAL no-op, nothing written) → <c>TeamMemberRemoved</c> audit BEFORE <c>TeamMembers.Remove</c>,
/// both in one <c>SaveChangesAsync</c> (same ordering precedent as 003's DeleteTaskCommandHandler).
/// No <c>If-Match</c> anywhere — a membership row has no mutable field (research R-2).
/// </summary>
public class RemoveTeamMemberCommandHandler : IRequestHandler<RemoveTeamMemberCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ITeamAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLog;

    public RemoveTeamMemberCommandHandler(
        IApplicationDbContext db,
        ITeamAccessPolicy accessPolicy,
        ICurrentUserService currentUserService,
        IActivityLogService activityLog)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _currentUserService = currentUserService;
        _activityLog = activityLog;
    }

    public async Task<Result> Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var project = await _db.Projects.SingleOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Result.Failure(new Error(ErrorKind.NotFound, "No project exists with that id."));
        }

        var decision = await _accessPolicy.CanManageTeamAsync(project, caller, cancellationToken);
        if (!decision.Allowed)
        {
            return Result.Failure(new Error(ErrorKind.Forbidden, decision.Reason ?? "You do not have access to manage this project's team."));
        }

        var membership = await _db.TeamMembers.SingleOrDefaultAsync(
            m => m.ProjectId == project.Id && m.UserId == request.UserId, cancellationToken);
        if (membership is null)
        {
            return Result.Failure(new Error(ErrorKind.NotFound, "This user is not a member of this project."));
        }

        var blockingCount = await OpenAssignedTaskCheck.CountBlockingTasksAsync(_db, project.Id, request.UserId, cancellationToken);
        if (blockingCount > 0)
        {
            // A total no-op: return before touching TeamMembers or ActivityLogs at all — the
            // membership row, and everything else, is exactly as it was before this call.
            return Result.Failure(new Error(
                ErrorKind.Conflict,
                $"Cannot remove: the member has {blockingCount} open task(s) assigned in this project. Reassign or close them first."));
        }

        await _activityLog.LogAsync(
            actorId: caller.UserId,
            action: nameof(AuditAction.TeamMemberRemoved),
            entityType: "TeamMember",
            entityId: membership.Id.ToString(),
            changeSummary: $"User removed from project '{project.Name}'",
            cancellationToken, projectId: project.Id);

        _db.TeamMembers.Remove(membership);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The membership was already deleted by a concurrent request between our read and our
            // delete — the losing side of the race sees "no longer there", i.e. 404, not a 500.
            return Result.Failure(new Error(ErrorKind.NotFound, "This user is not a member of this project."));
        }

        return Result.Success();
    }
}
