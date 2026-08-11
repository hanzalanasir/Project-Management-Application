using System.Text.Json.Serialization;

namespace ProjectManagementApp.Application.Features.Dashboard;

/// <summary>Matches the contract's <c>TaskStatusCounts</c> exactly — see <see cref="ProjectStatusCountsDto"/> remarks.</summary>
public sealed record TaskStatusCountsDto(
    [property: JsonPropertyName("ToDo")] int ToDo,
    [property: JsonPropertyName("InProgress")] int InProgress,
    [property: JsonPropertyName("InReview")] int InReview,
    [property: JsonPropertyName("Done")] int Done,
    [property: JsonPropertyName("Blocked")] int Blocked);
