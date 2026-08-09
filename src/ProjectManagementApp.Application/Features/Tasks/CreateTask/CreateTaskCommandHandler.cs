using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Tasks.Common;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Tasks.CreateTask;

/// <summary>
/// Fixed order (data-model.md §3, §6): load the parent project (404) → CanMutateAsync(Create)
/// (403) → assignee pool + due-date window (400) → persist + audit in one SaveChangesAsync
/// (Constitution IV.4). <c>CanMutateAsync</c> is checked against a transient, unsaved TaskItem
/// (the row does not exist yet) so Create shares the exact same policy path every other mutation
/// uses — the behavioural half of the graduated model's belt-and-braces design.
/// </summary>
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITaskAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLog;
    private readonly AssigneeValidator _assigneeValidator;
    private readonly TasksOptions _options;

    public CreateTaskCommandHandler(
        IApplicationDbContext db,
        ITaskAccessPolicy accessPolicy,
        ICurrentUserService currentUserService,
        IActivityLogService activityLog,
        AssigneeValidator assigneeValidator,
        IOptions<TasksOptions> options)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _currentUserService = currentUserService;
        _activityLog = activityLog;
        _assigneeValidator = assigneeValidator;
        _options = options.Value;
    }

    public async Task<Result<TaskDetailDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var project = await _db.Projects.SingleOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Result<TaskDetailDto>.Failure(new Error(ErrorKind.NotFound, "No project exists with that id."));
        }

        var candidate = new TaskItem { ProjectId = project.Id, Project = project };
        var decision = await _accessPolicy.CanMutateAsync(candidate, TaskMutation.Create, caller, cancellationToken);
        if (!decision.Allowed)
        {
            return Result<TaskDetailDto>.Failure(new Error(ErrorKind.Forbidden, decision.Reason ?? "You may not create tasks in this project."));
        }

        if (!await _assigneeValidator.IsEligibleAsync(project.Id, request.AssigneeId, cancellationToken))
        {
            return Result<TaskDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["assigneeId"] = new[] { "User is not an active team member on this project." } }));
        }

        if (!DueDateWindowValidator.IsWithinWindow(request.DueDate, project.StartDate, project.EndDate))
        {
            return Result<TaskDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["dueDate"] = new[] { "Due date must fall within the parent project's start/end window." } }));
        }

        var priority = string.IsNullOrWhiteSpace(request.Priority) ? _options.DefaultPriority : request.Priority;
        if (!Enum.TryParse<TaskPriority>(priority, out var parsedPriority))
        {
            return Result<TaskDetailDto>.Failure(new Error(
                ErrorKind.Validation, "Validation failed",
                new Dictionary<string, string[]> { ["priority"] = new[] { $"'{priority}' is not a recognized task priority." } }));
        }

        Enum.TryParse<ProjectManagementApp.Domain.Enums.TaskStatus>(_options.DefaultStatus, out var defaultStatus);

        ApplicationUser? assignee = null;
        if (request.AssigneeId.HasValue)
        {
            assignee = await _db.Users.SingleOrDefaultAsync(u => u.Id == request.AssigneeId, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Project = project,
            Title = request.Title,
            Description = request.Description,
            Status = defaultStatus,
            Priority = parsedPriority,
            DueDate = request.DueDate,
            AssigneeId = request.AssigneeId,
            Assignee = assignee,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Tasks.Add(task);

        await _activityLog.LogAsync(
            actorId: caller.UserId,
            action: nameof(AuditAction.TaskCreated),
            entityType: "Task",
            entityId: task.Id.ToString(),
            changeSummary: $"Task '{task.Title}' created in project '{project.Name}'",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        return Result<TaskDetailDto>.Success(task.ToDetailDto());
    }
}
