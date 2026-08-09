using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Domain.Entities;

// Named TaskItem, not Task, to avoid colliding with System.Threading.Tasks.Task.
public class TaskItem
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Domain.Enums.TaskStatus Status { get; set; } = Domain.Enums.TaskStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateOnly? DueDate { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public uint Version { get; set; }

    public Project Project { get; set; } = null!;
    public ApplicationUser? Assignee { get; set; }
}
