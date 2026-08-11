using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Common.Interfaces;

/// <summary>The audit trail's only write/read surface — reading <c>activity_logs</c> directly is forbidden elsewhere.</summary>
public interface IActivityLogService
{
    /// <summary>
    /// Writes one audit row into the caller's unit of work, committed in the same <c>SaveChangesAsync</c>
    /// as the triggering change (Constitution IV.4). <paramref name="projectId"/> is the entity's
    /// owning project, stamped at write time so a scoped read (<see cref="QueryScopedAsync"/>)
    /// never needs to resolve it via a live-table join — which would break the moment the entity
    /// is deleted. Leave it <c>null</c> for entity types with no owning project (e.g. <c>User</c>
    /// events); such rows are then only ever visible to an Unscoped (Admin) reader.
    /// </summary>
    Task LogAsync(Guid? actorId, string action, string entityType, string entityId,
                  string changeSummary, CancellationToken ct, Guid? projectId = null);

    /// <summary>A scoped, paginated read of the audit trail — consumed by 005's feed and 006's Activity Report.</summary>
    Task<PagedResult<ActivityEntry>> QueryScopedAsync(
        ActivityScope scope, int page, int pageSize, CancellationToken ct);
}
