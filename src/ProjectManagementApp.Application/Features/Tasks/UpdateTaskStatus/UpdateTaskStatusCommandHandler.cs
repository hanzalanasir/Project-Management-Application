using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Tasks.UpdateTaskStatus;

/// <summary>
/// The graduated cell: <c>CanMutateAsync(StatusChange)</c> is the only mutation kind a TeamMember
/// ever passes, and only as the task's assignee (data-model.md §3). <c>closed_at</c> is derived
/// here as a side effect of crossing the Done boundary — set on entry, cleared on exit, unchanged
/// on a Done-&gt;Done no-op — and rides on the SAME <c>TaskStatusChanged</c> audit entry rather than
/// a separate event (spec B.7).
/// </summary>
public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, Result<TaskDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITaskAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLog;

    public UpdateTaskStatusCommandHandler(
        IApplicationDbContext db,
        ITaskAccessPolicy accessPolicy,
        ICurrentUserService currentUserService,
        IActivityLogService activityLog)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _currentUserService = currentUserService;
        _activityLog = activityLog;
    }

    public async Task<Result<TaskDetailDto>> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var task = await _db.Tasks.Include(t => t.Project).Include(t => t.Assignee).SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (task is null)
        {
            return Result<TaskDetailDto>.Failure(new Error(ErrorKind.NotFound, "Task not found"));
        }

        var decision = await _accessPolicy.CanMutateAsync(task, TaskMutation.StatusChange, caller, cancellationToken);
        if (!decision.Allowed)
        {
            return Result<TaskDetailDto>.Failure(new Error(ErrorKind.Forbidden, decision.Reason ?? "You do not have access to change this task's status."));
        }

        if (!Enum.TryParse<ProjectManagementApp.Domain.Enums.TaskStatus>(request.Status, out var newStatus))
        {
            return Result<TaskDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["status"] = new[] { $"'{request.Status}' is not a recognized task status." } }));
        }

        var previousStatus = task.Status;

        // Derived, never user-settable: entering Done sets it; leaving Done clears it; a
        // Done->Done no-op leaves it untouched; any non-Done->non-Done transition never touches it.
        if (newStatus == ProjectManagementApp.Domain.Enums.TaskStatus.Done && previousStatus != ProjectManagementApp.Domain.Enums.TaskStatus.Done)
        {
            task.ClosedAt = DateTimeOffset.UtcNow;
        }
        else if (newStatus != ProjectManagementApp.Domain.Enums.TaskStatus.Done && previousStatus == ProjectManagementApp.Domain.Enums.TaskStatus.Done)
        {
            task.ClosedAt = null;
        }

        task.Status = newStatus;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        _db.Entry(task).Property(t => t.Version).OriginalValue = request.IfMatchVersion;

        await _activityLog.LogAsync(
            actorId: caller.UserId,
            action: nameof(AuditAction.TaskStatusChanged),
            entityType: "Task",
            entityId: task.Id.ToString(),
            changeSummary: $"Task '{task.Title}' status changed from {previousStatus} to {newStatus}",
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
