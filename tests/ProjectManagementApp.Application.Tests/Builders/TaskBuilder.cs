using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Tests.Builders;

// Constitution IX.4 — test data via builders, not inline object literals scattered across files.
// From builders, never the production seeder (ADR-0007 §4).
public class TaskBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _projectId = Guid.NewGuid();
    private Project? _project;
    private string _title = "Test Task";
    private string? _description;
    private ProjectManagementApp.Domain.Enums.TaskStatus _status = ProjectManagementApp.Domain.Enums.TaskStatus.ToDo;
    private TaskPriority _priority = TaskPriority.Medium;
    private DateOnly? _dueDate;
    private Guid? _assigneeId;
    private ApplicationUser? _assignee;
    private DateTimeOffset? _closedAt;
    private uint _version;

    public TaskBuilder WithId(Guid id) { _id = id; return this; }
    public TaskBuilder WithProjectId(Guid projectId) { _projectId = projectId; return this; }
    public TaskBuilder WithProject(Project project) { _project = project; _projectId = project.Id; return this; }
    public TaskBuilder WithTitle(string title) { _title = title; return this; }
    public TaskBuilder WithDescription(string? description) { _description = description; return this; }
    public TaskBuilder WithStatus(ProjectManagementApp.Domain.Enums.TaskStatus status) { _status = status; return this; }
    public TaskBuilder WithPriority(TaskPriority priority) { _priority = priority; return this; }
    public TaskBuilder WithDueDate(DateOnly? dueDate) { _dueDate = dueDate; return this; }
    public TaskBuilder WithAssigneeId(Guid? assigneeId) { _assigneeId = assigneeId; return this; }
    public TaskBuilder WithAssignee(ApplicationUser? assignee) { _assignee = assignee; _assigneeId = assignee?.Id; return this; }
    public TaskBuilder WithClosedAt(DateTimeOffset? closedAt) { _closedAt = closedAt; return this; }
    public TaskBuilder WithVersion(uint version) { _version = version; return this; }

    public TaskItem Build()
    {
        var now = DateTimeOffset.UtcNow;
        var task = new TaskItem
        {
            Id = _id,
            ProjectId = _projectId,
            Title = _title,
            Description = _description,
            Status = _status,
            Priority = _priority,
            DueDate = _dueDate,
            AssigneeId = _assigneeId,
            ClosedAt = _closedAt,
            CreatedAt = now,
            UpdatedAt = now,
            Version = _version,
        };

        if (_project is not null)
        {
            task.Project = _project;
        }

        if (_assignee is not null)
        {
            task.Assignee = _assignee;
        }

        return task;
    }
}
