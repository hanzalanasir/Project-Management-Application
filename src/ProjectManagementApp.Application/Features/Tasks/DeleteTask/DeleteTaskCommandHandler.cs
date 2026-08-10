using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Tasks.DeleteTask;

/// <summary>
/// The <c>TaskDeleted</c> audit row is written BEFORE <c>Tasks.Remove</c>, in the same
/// <c>SaveChangesAsync</c> — same ordering precedent as 002's DeleteProjectCommandHandler. Delete
/// is not in a TeamMember's mutation set at all (unlike StatusChange), so even the assignee is
/// refused by <c>CanMutateAsync(Delete)</c>.
/// </summary>
public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ITaskAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLog;

    public DeleteTaskCommandHandler(
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

    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var task = await _db.Tasks.Include(t => t.Project).SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (task is null)
        {
            return Result.Failure(new Error(ErrorKind.NotFound, "Task not found"));
        }

        var decision = await _accessPolicy.CanMutateAsync(task, TaskMutation.Delete, caller, cancellationToken);
        if (!decision.Allowed)
        {
            return Result.Failure(new Error(ErrorKind.Forbidden, decision.Reason ?? "You do not have access to delete this task."));
        }

        // Audit BEFORE removal — the snapshot summary is written first, in the same unit of work,
        // so the TaskDeleted row survives even though the task row is gone by the time this call
        // returns (spec US-003-06).
        await _activityLog.LogAsync(
            caller.UserId, nameof(AuditAction.TaskDeleted), "Task", task.Id.ToString(),
            $"Task '{task.Title}' deleted", cancellationToken);

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
