using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Common.Interfaces;

/// <summary>The audit trail's only write/read surface — reading <c>activity_logs</c> directly is forbidden elsewhere.</summary>
public interface IActivityLogService
{
    /// <summary>Writes one audit row into the caller's unit of work, committed in the same <c>SaveChangesAsync</c> as the triggering change (Constitution IV.4).</summary>
    Task LogAsync(Guid? actorId, string action, string entityType, string entityId,
                  string changeSummary, CancellationToken ct);

    /// <summary>A scoped, paginated read of the audit trail — consumed by 005's feed and 006's Activity Report.</summary>
    Task<PagedResult<ActivityEntry>> QueryScopedAsync(
        ActivityScope scope, int page, int pageSize, CancellationToken ct);
}
