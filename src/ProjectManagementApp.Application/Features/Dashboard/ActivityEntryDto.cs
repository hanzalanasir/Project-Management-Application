namespace ProjectManagementApp.Application.Features.Dashboard;

/// <summary>Matches the contract's <c>ActivityEntry</c>. Projected from <see cref="Common.Models.ActivityEntry"/>.</summary>
public sealed record ActivityEntryDto(
    Guid Id,
    string? ActorName,
    string Action,
    string EntityType,
    string EntityId,
    DateTimeOffset Timestamp,
    string ChangeSummary);
