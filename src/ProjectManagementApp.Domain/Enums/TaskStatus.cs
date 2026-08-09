namespace ProjectManagementApp.Domain.Enums;

// Named to match the domain vocabulary; disambiguate from System.Threading.Tasks.TaskStatus
// at usage sites with a fully-qualified reference (ProjectManagementApp.Domain.Enums.TaskStatus).
public enum TaskStatus
{
    ToDo,
    InProgress,
    InReview,
    Done,
    Blocked
}
