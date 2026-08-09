namespace ProjectManagementApp.Application.Features.Tasks.Common;

// Cross-field rule (ADR-0005): when set, a task's due date MUST fall within its parent project's
// start/end window. Needs the parent project, so it lives beside the handler rather than in
// FluentValidation (which only sees the command, not the loaded Project).
public static class DueDateWindowValidator
{
    public static bool IsWithinWindow(DateOnly? dueDate, DateOnly projectStartDate, DateOnly? projectEndDate)
    {
        if (dueDate is null)
        {
            return true;
        }

        if (dueDate.Value < projectStartDate)
        {
            return false;
        }

        return !projectEndDate.HasValue || dueDate.Value <= projectEndDate.Value;
    }
}
