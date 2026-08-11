using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Tasks.Common;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Tasks.UpdateTask;

/// <summary>
/// <c>CanMutateAsync(FullEdit)</c> is re-evaluated here at write time against a freshly-loaded
/// entity, never trusted from an earlier read (mirrors 002's UpdateProjectCommandHandler). A
/// TeamMember — even the assignee — is always denied here; the policy's message names the
/// narrower right they DO have (StatusChange), which is the graduated model's acceptance signal
/// (quickstart V2). The client's If-Match value drives EF's original xmin, so PostgreSQL performs
/// the actual optimistic-concurrency check.
/// </summary>
public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TaskDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITaskAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLog;

    public UpdateTaskCommandHandler(
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

    public async Task<Result<TaskDetailDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var task = await _db.Tasks.Include(t => t.Project).SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (task is null)
        {
            return Result<TaskDetailDto>.Failure(new Error(ErrorKind.NotFound, "Task not found"));
        }

        var decision = await _accessPolicy.CanMutateAsync(task, TaskMutation.FullEdit, caller, cancellationToken);
        if (!decision.Allowed)
        {
            return Result<TaskDetailDto>.Failure(new Error(ErrorKind.Forbidden, decision.Reason ?? "You do not have access to edit this task."));
        }

        if (!Enum.TryParse<TaskPriority>(request.Priority, out var parsedPriority))
        {
            return Result<TaskDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["priority"] = new[] { $"'{request.Priority}' is not a recognized task priority." } }));
        }

        // Revalidated at write time — the parent project's window may have changed since the task
        // was created, or the caller may be setting a due date for the first time.
        if (!DueDateWindowValidator.IsWithinWindow(request.DueDate, task.Project.StartDate, task.Project.EndDate))
        {
            return Result<TaskDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["dueDate"] = new[] { "Due date must fall within the parent project's start/end window." } }));
        }

        var changedFields = new List<string>();
        if (task.Title != request.Title) changedFields.Add("title");
        if (task.Description != request.Description) changedFields.Add("description");
        if (task.Priority != parsedPriority) changedFields.Add("priority");
        if (task.DueDate != request.DueDate) changedFields.Add("dueDate");

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = parsedPriority;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        // The client's If-Match value drives the xmin check, not whatever this context happened to
        // load — a stale supplied version must fail even if nothing else raced in between.
        _db.Entry(task).Property(t => t.Version).OriginalValue = request.IfMatchVersion;

        var changeSummary = changedFields.Count > 0
            ? $"Task '{task.Title}' updated — changed: {string.Join(", ", changedFields)}"
            : $"Task '{task.Title}' updated — no field changes";

        await _activityLog.LogAsync(caller.UserId, nameof(AuditAction.TaskUpdated), "Task", task.Id.ToString(), changeSummary, cancellationToken, projectId: task.ProjectId);

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
