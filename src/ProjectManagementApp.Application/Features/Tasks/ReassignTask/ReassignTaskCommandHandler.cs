using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Tasks.Common;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Tasks.ReassignTask;

/// <summary>
/// <c>CanMutateAsync(Reassign)</c> denies every TeamMember outright — unlike StatusChange, Reassign
/// is not in their mutation set at all, even for their own task and even reassigning to themselves.
/// <see cref="AssigneeValidator"/> (shared with CreateTask, T019) is reused read-only against
/// <c>team_members</c> — never a 004 handler, never mutating 004's data. The previous assignee's
/// access ends the instant this commits: their next read re-evaluates scope fresh and returns 403
/// (no grace period, no cached access — Clarifications 2026-07-22).
/// </summary>
public class ReassignTaskCommandHandler : IRequestHandler<ReassignTaskCommand, Result<TaskDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITaskAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLog;
    private readonly AssigneeValidator _assigneeValidator;

    public ReassignTaskCommandHandler(
        IApplicationDbContext db,
        ITaskAccessPolicy accessPolicy,
        ICurrentUserService currentUserService,
        IActivityLogService activityLog,
        AssigneeValidator assigneeValidator)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _currentUserService = currentUserService;
        _activityLog = activityLog;
        _assigneeValidator = assigneeValidator;
    }

    public async Task<Result<TaskDetailDto>> Handle(ReassignTaskCommand request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var task = await _db.Tasks.Include(t => t.Project).Include(t => t.Assignee).SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (task is null)
        {
            return Result<TaskDetailDto>.Failure(new Error(ErrorKind.NotFound, "Task not found"));
        }

        var decision = await _accessPolicy.CanMutateAsync(task, TaskMutation.Reassign, caller, cancellationToken);
        if (!decision.Allowed)
        {
            return Result<TaskDetailDto>.Failure(new Error(ErrorKind.Forbidden, decision.Reason ?? "You do not have access to reassign this task."));
        }

        if (!await _assigneeValidator.IsEligibleAsync(task.ProjectId, request.AssigneeId, cancellationToken))
        {
            return Result<TaskDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["assigneeId"] = new[] { "User is not a team member on this project." } }));
        }

        var previousAssigneeLabel = task.Assignee?.FullName ?? "Unassigned";

        var newAssignee = request.AssigneeId.HasValue
            ? await _db.Users.SingleOrDefaultAsync(u => u.Id == request.AssigneeId, cancellationToken)
            : null;
        var newAssigneeLabel = newAssignee?.FullName ?? "Unassigned";

        task.AssigneeId = request.AssigneeId;
        task.Assignee = newAssignee;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        // The client's If-Match value drives the xmin check, not whatever this context happened to
        // load — a stale supplied version must fail even if nothing else raced in between.
        _db.Entry(task).Property(t => t.Version).OriginalValue = request.IfMatchVersion;

        await _activityLog.LogAsync(
            actorId: caller.UserId,
            action: nameof(AuditAction.TaskReassigned),
            entityType: "Task",
            entityId: task.Id.ToString(),
            changeSummary: $"Task '{task.Title}' reassigned from {previousAssigneeLabel} to {newAssigneeLabel}",
            cancellationToken, projectId: task.ProjectId);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<TaskDetailDto>.Failure(new Error(ErrorKind.Conflict, "This task was modified by someone else. Reload and try again."));
        }

        return Result<TaskDetailDto>.Success(task.ToDetailDto());
    }
}
