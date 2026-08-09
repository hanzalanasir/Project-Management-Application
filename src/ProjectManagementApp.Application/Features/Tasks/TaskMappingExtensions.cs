namespace ProjectManagementApp.Application.Features.Tasks;

// Manual static mapping — no AutoMapper (ADR-0005), same convention as 002's
// ProjectMappingExtensions. Both methods require TaskItem.Project and, when AssigneeId is set,
// TaskItem.Assignee to be loaded (projected or Include'd) by the caller; neither method touches
// the database itself.
public static class TaskMappingExtensions
{
    public static UserRefDto ToUserRefDto(this ProjectManagementApp.Domain.Entities.ApplicationUser user) =>
        new(user.Id, user.FullName, user.IsActive);

    public static TaskSummaryDto ToSummaryDto(this ProjectManagementApp.Domain.Entities.TaskItem task) =>
        new(task.Id, task.ProjectId, task.Title, task.Status.ToString(), task.Priority.ToString(), task.DueDate,
            task.Assignee?.ToUserRefDto());

    public static TaskDetailDto ToDetailDto(this ProjectManagementApp.Domain.Entities.TaskItem task) =>
        new(task.Id, task.ProjectId, task.Title, task.Description, task.Status.ToString(), task.Priority.ToString(),
            task.DueDate, task.Assignee?.ToUserRefDto(), task.ClosedAt, task.CreatedAt, task.UpdatedAt, task.Version);
}
