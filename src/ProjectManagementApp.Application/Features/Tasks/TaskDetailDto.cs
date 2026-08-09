using System.Text.Json.Serialization;

namespace ProjectManagementApp.Application.Features.Tasks;

// Matches contracts/tasks.v1.yaml#/components/schemas/TaskDetail. closedAt appears in responses
// only — it is never bindable in any request command (research R-3, data-model.md §6). The row
// version travels as the ETag/If-Match header (002 R-2), not as a body property — JsonIgnore keeps
// it out of the JSON body while still available to the controller.
public sealed record TaskDetailDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateOnly? DueDate,
    UserRefDto? Assignee,
    DateTimeOffset? ClosedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    [property: JsonIgnore] uint Version);
