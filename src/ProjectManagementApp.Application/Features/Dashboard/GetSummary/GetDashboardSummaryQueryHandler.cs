using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Metrics;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Dashboard.Common;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Dashboard.GetSummary;

/// <summary>
/// Every number scoped at the query source (data-model.md §5): 1. base queryable, 2. scope
/// composed in, 3. metric filter, 4. <c>GroupBy</c>+<c>Count</c> executed in SQL, 5. merge into
/// the zero-seeded map. Live per request — no caching (Clarifications 2026-07-22). Performs no
/// writes — a read of this endpoint modifies no row and creates no audit entry (DoD 8, FR-010;
/// proven by <c>NoWriteGuaranteeTests</c>).
/// </summary>
/// <remarks>
/// For a <b>TeamMember</b>, <c>tasksByStatus</c>/<c>overdueTaskCount</c>/<c>completionRate</c>/
/// <c>blockedTaskCount</c> are sourced directly from the personal slice — not recomputed via
/// <see cref="ITaskAccessPolicy"/> — so they are literally the same numbers as
/// <c>personalTasks</c>, not merely equal by coincidence of predicate (T042, T046). For Admin/PM
/// the tiles reflect the full project-wide scope via <see cref="ITaskAccessPolicy.ApplyScope"/>,
/// and <c>personalTasks</c> is computed separately (typically empty for a manager).
/// </remarks>
public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectAccessPolicy _projectAccessPolicy;
    private readonly ITaskAccessPolicy _taskAccessPolicy;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardSummaryQueryHandler(
        IApplicationDbContext db,
        IProjectAccessPolicy projectAccessPolicy,
        ITaskAccessPolicy taskAccessPolicy,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _projectAccessPolicy = projectAccessPolicy;
        _taskAccessPolicy = taskAccessPolicy;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
    {
        var caller = _currentUserService.Current;
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var visibleProjectIds = VisibleProjectScope.Resolve(_db, _projectAccessPolicy, caller);

        var projectStatusCounts = await _db.Projects
            .Where(p => visibleProjectIds.Contains(p.Id))
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var projectsByStatus = EnumCountMap.Build(projectStatusCounts.Select(x => (x.Status, x.Count)));
        var visibleProjectCount = projectsByStatus.Values.Sum();

        var personalTasks = await PersonalTaskSlice.ComputeAsync(_db, visibleProjectIds, caller.UserId, today, ct);

        TaskStatusCountsDto tasksByStatus;
        int overdueTaskCount;
        int totalTasksInScope;
        int blockedTaskCount;
        int closedTasksInScope;

        if (caller.Role == nameof(Role.TeamMember))
        {
            // Personal-view (data-model.md §2): the tile IS the personal slice, sourced directly
            // rather than recomputed — guarantees T042's tile-consistency by construction, not by
            // two predicates happening to agree.
            tasksByStatus = personalTasks.ByStatus;
            overdueTaskCount = personalTasks.OverdueCount;
            totalTasksInScope = personalTasks.AssignedTotal;
            blockedTaskCount = tasksByStatus.Blocked;
            closedTasksInScope = tasksByStatus.Done;
        }
        else
        {
            var taskScope = _taskAccessPolicy.ApplyScope(_db.Tasks, caller);

            var taskStatusCounts = await taskScope
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct);
            var taskStatusMap = EnumCountMap.Build(taskStatusCounts.Select(x => (x.Status, x.Count)));

            overdueTaskCount = await taskScope.Where(MetricDefinitions.IsOverdue(today)).CountAsync(ct);
            tasksByStatus = EnumCountMap.ToTaskStatusCountsDto(taskStatusMap);
            totalTasksInScope = taskStatusMap.Values.Sum();
            blockedTaskCount = taskStatusMap[ProjectManagementApp.Domain.Enums.TaskStatus.Blocked];
            closedTasksInScope = taskStatusMap[ProjectManagementApp.Domain.Enums.TaskStatus.Done];
        }

        var completionRate = MetricDefinitions.CompletionRate(closedTasksInScope, totalTasksInScope);

        var visibleTeamMemberCount = await _db.TeamMembers
            .Where(tm => visibleProjectIds.Contains(tm.ProjectId))
            .Select(tm => tm.UserId)
            .Distinct()
            .CountAsync(ct);

        var dto = new DashboardSummaryDto(
            now,
            caller.Role,
            visibleProjectCount,
            EnumCountMap.ToProjectStatusCountsDto(projectsByStatus),
            tasksByStatus,
            overdueTaskCount,
            completionRate,
            blockedTaskCount,
            visibleTeamMemberCount,
            personalTasks);

        return Result<DashboardSummaryDto>.Success(dto);
    }
}
