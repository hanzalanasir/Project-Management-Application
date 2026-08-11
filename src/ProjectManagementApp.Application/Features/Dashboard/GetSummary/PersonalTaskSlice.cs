using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Metrics;
using ProjectManagementApp.Application.Features.Dashboard.Common;

namespace ProjectManagementApp.Application.Features.Dashboard.GetSummary;

/// <summary>
/// The tasks-assigned-to-the-caller slice (US-005-03), scoped to their visible-project set. Both
/// the assignee predicate and the project scope are applied IN the query (T044) — never filtered
/// in memory after fetching. Re-run with the extra <c>assignee_id == caller</c> predicate on top
/// of the same aggregates every other tile uses; zero-seeded identically (data-model.md §3).
/// </summary>
public static class PersonalTaskSlice
{
    public static async Task<PersonalTaskSummaryDto> ComputeAsync(
        IApplicationDbContext db, IQueryable<Guid> visibleProjectIds, Guid callerUserId, DateOnly todayUtc, CancellationToken ct)
    {
        var personalTasks = db.Tasks.Where(t => visibleProjectIds.Contains(t.ProjectId) && t.AssigneeId == callerUserId);

        var statusCounts = await personalTasks
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var byStatus = EnumCountMap.Build(statusCounts.Select(x => (x.Status, x.Count)));

        var overdueCount = await personalTasks.Where(MetricDefinitions.IsOverdue(todayUtc)).CountAsync(ct);
        var assignedTotal = byStatus.Values.Sum();

        return new PersonalTaskSummaryDto(assignedTotal, EnumCountMap.ToTaskStatusCountsDto(byStatus), overdueCount);
    }
}
