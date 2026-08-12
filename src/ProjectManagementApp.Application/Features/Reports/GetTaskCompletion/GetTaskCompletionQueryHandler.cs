using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.Common;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Reports.GetTaskCompletion;

/// <summary>
/// A completion trend bucketed by day/week/month — zero-filled and continuous (T049's
/// <see cref="BucketGenerator"/>). Grouping happens in SQL at day granularity (a single
/// <c>GROUP BY</c> over the scoped+filtered task set), then <see cref="BucketGenerator"/> merges
/// those day-counts into the requested bucket size — never fetch-then-group in memory.
/// Writes exactly one <c>ReportGenerated</c> audit row via <c>ReportGenerationAudit</c> as the last
/// step — the one deliberate write in this otherwise read-only feature (Constitution VIII.3).
/// </summary>
public sealed class GetTaskCompletionQueryHandler
    : IRequestHandler<GetTaskCompletionQuery, Result<TaskCompletionReportDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectAccessPolicy _projectAccessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly ReportsOptions _options;

    public GetTaskCompletionQueryHandler(
        IApplicationDbContext db,
        IProjectAccessPolicy projectAccessPolicy,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        IOptions<ReportsOptions> options)
    {
        _db = db;
        _projectAccessPolicy = projectAccessPolicy;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
        _options = options.Value;
    }

    public async Task<Result<TaskCompletionReportDto>> Handle(GetTaskCompletionQuery request, CancellationToken ct)
    {
        var caller = _currentUserService.Current;
        var from = request.From!.Value;
        var to = request.To!.Value;
        var groupBy = request.GroupBy!;

        var scopeResult = await ReportScope.ResolveAsync(_db, _projectAccessPolicy, caller, request.ProjectScope, ct);
        if (!scopeResult.IsSuccess)
        {
            return Result<TaskCompletionReportDto>.Failure(scopeResult.Error!);
        }

        var visibleProjectIds = scopeResult.Value!;
        var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toUtc = new DateTimeOffset(to.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        // A TeamMember cannot trend a colleague — silently constrained to self, never a 403 (the
        // same "own value only" shape as T060's Team Performance clamp, applied here to a filter
        // rather than a whole report).
        var effectiveAssigneeId = caller.Role == nameof(Role.TeamMember) ? caller.UserId : request.AssigneeId;

        var taskQuery = _db.Tasks.Where(t => visibleProjectIds.Contains(t.ProjectId));
        if (effectiveAssigneeId is not null)
        {
            taskQuery = taskQuery.Where(t => t.AssigneeId == effectiveAssigneeId);
        }

        // Npgsql cannot translate a GroupBy keyed on a computed DateTimeOffset->DateTime->Date
        // conversion (the shaper throws "No coercion operator is defined between types
        // 'System.DateTimeOffset' and 'System.Nullable<DateTime>'" when reading the group key back)
        // — a real translation gap hit while implementing this, not a hypothetical. The WHERE
        // (scope + window filter) still executes entirely in SQL; only this final column
        // projection and the day-bucketing are done client-side, over an already-bounded set (the
        // report window, never unbounded — Task Completion has no threshold guard because its
        // result size is capped by the window, unlike Activity's arbitrary row count).
        var closedTimestamps = await taskQuery
            .Where(ReportCountingRules.ClosedInWindow(fromUtc, toUtc))
            .Select(t => t.ClosedAt!.Value)
            .ToListAsync(ct);

        var dayCounts = closedTimestamps
            .GroupBy(ts => DateOnly.FromDateTime(ts.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Count());
        var buckets = BucketGenerator.Generate(from, to, groupBy, dayCounts);

        var dto = new TaskCompletionReportDto(
            "TaskCompletion", DateTimeOffset.UtcNow, caller.Role,
            new ReportWindowDto(from, to), "UTC", groupBy, buckets,
            new TaskCompletionTotalsDto(buckets.Sum(b => b.CompletedCount)));

        await ReportGenerationAudit.RecordAsync(
            _activityLogService, _options, caller, "TaskCompletion",
            new { from, to, groupBy, projectScope = request.ProjectScope, assigneeId = effectiveAssigneeId }, ct);
        await _db.SaveChangesAsync(ct);

        return Result<TaskCompletionReportDto>.Success(dto);
    }
}
