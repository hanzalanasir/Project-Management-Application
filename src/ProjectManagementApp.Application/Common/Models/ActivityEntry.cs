namespace ProjectManagementApp.Application.Common.Models;

public sealed record ActivityEntry(Guid Id, Guid? ActorId, string ActorName, string Action,
                                   string EntityType, string EntityId,
                                   DateTimeOffset Timestamp, string ChangeSummary);
