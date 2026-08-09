namespace ProjectManagementApp.Application.Common.Models;

// Authorization vocabulary, never persisted. Referenced by ITaskAccessPolicy.CanMutateAsync (003).
public enum TaskMutation
{
    Create,
    FullEdit,
    StatusChange,
    Reassign,
    Delete
}
