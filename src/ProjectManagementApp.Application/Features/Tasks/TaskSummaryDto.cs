namespace ProjectManagementApp.Application.Features.Tasks;

// Matches contracts/tasks.v1.yaml#/components/schemas/TaskSummary.
public sealed record TaskSummaryDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Status,
    string Priority,
    DateOnly? DueDate,
    UserRefDto? Assignee);
