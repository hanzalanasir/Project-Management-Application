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

    /// <summary>
    /// A scoped, paginated read of the audit trail — consumed by 005's feed and 006's Activity
    /// Report. The optional <paramref name="from"/>/<paramref name="to"/>/<paramref name="projectId"/>/
    /// <paramref name="entityType"/>/<paramref name="actorId"/> filters were added for 006's Activity
    /// Report (FR-007): 005's own feed never needed them (it's the caller's whole scoped trail, no
    /// window or entity filter), and this is the ONLY legal read path for either feature — 006's
    /// spec requires per-role scoping plus these filters, and never reading <c>activity_logs</c>
    /// directly, so extending this shared-kernel method is the only way to satisfy both at once
    /// (the same kind of explicit, minimal, backward-compatible extension already made once before,
    /// when this method itself was added to 001 during 005's planning). Every filter narrows the
    /// SAME scoped `WHERE`, including <see cref="PagedResult{T}.TotalCount"/> — so a
    /// <c>page=1, pageSize=1</c> probe call returns an accurate, fully-filtered count without
    /// materializing more than one row, which is what makes an indexed pre-materialization
    /// threshold guard possible without a second, parallel read path.
    /// </summary>
    Task<PagedResult<ActivityEntry>> QueryScopedAsync(
        ActivityScope scope, int page, int pageSize, CancellationToken ct,
        DateTimeOffset? from = null, DateTimeOffset? to = null,
        Guid? projectId = null, string? entityType = null, Guid? actorId = null);
}
