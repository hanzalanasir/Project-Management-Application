namespace ProjectManagementApp.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; set; }
    public Guid? ActorId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string ChangeSummary { get; set; } = string.Empty;

    // Stamped at write time so scoped reads (005's feed, 006's Activity Report) don't need to
    // resolve an entity's owning project via a live-table join — which breaks the moment the
    // entity is deleted (TaskDeleted/ProjectDeleted/TeamMemberRemoved rows must still be
    // scope-classifiable after the row they describe is gone; QueryScopedAsync's original
    // implementation compared EntityId directly against visible-project ids, which only ever
    // matched EntityType "Project" rows — every Task/TeamMember row was silently invisible to
    // every non-Admin caller). Null for entity types with no owning project (User events).
    public Guid? ProjectId { get; set; }
}
