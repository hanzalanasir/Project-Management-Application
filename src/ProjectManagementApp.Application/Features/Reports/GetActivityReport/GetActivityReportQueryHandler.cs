using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.Common;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Reports.GetActivityReport;

/// <summary>
/// The audit-facing report — the only one that reads through <see cref="IActivityLogService"/>
/// rather than <see cref="IApplicationDbContext"/> (FR-007: never <c>db.ActivityLogs</c> directly),
/// and the only one guarded by <see cref="RowThresholdGuard"/> (research R-5), because it's the only
/// report whose result size isn't already bounded by a project/member count.
/// </summary>
/// <remarks>
/// Fixed order (data-model.md §5, this stage's own instruction): scope → threshold guard (a
/// <c>page=1,pageSize=1</c> probe through the SAME scoped-and-filtered
/// <see cref="IActivityLogService.QueryScopedAsync"/> call the real read would use) → the real
/// scoped read → audit. Getting the guard second would mean it ran AFTER a full page was already
/// materialized, which protects nothing.
/// </remarks>
public sealed class GetActivityReportQueryHandler
    : IRequestHandler<GetActivityReportQuery, Result<ActivityReportDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectAccessPolicy _projectAccessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly ReportsOptions _options;

    public GetActivityReportQueryHandler(
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

    public async Task<Result<ActivityReportDto>> Handle(GetActivityReportQuery request, CancellationToken ct)
    {
        var caller = _currentUserService.Current;
        var from = request.From!.Value;
        var to = request.To!.Value;

        var isAdmin = caller.Role == nameof(Role.Admin);
        ActivityScope scope;

        if (isAdmin)
        {
            scope = new ActivityScope(Array.Empty<Guid>(), Unscoped: true);
        }
        else
        {
            var visibleProjectIds = await _projectAccessPolicy.ApplyScope(_db.Projects, caller)
                .Select(p => p.Id)
                .ToListAsync(ct);

            if (request.ProjectId is not null && !visibleProjectIds.Contains(request.ProjectId.Value))
            {
                return Result<ActivityReportDto>.Failure(new Error(
                    ErrorKind.Forbidden, "That project is outside your visible scope."));
            }

            scope = new ActivityScope(visibleProjectIds, Unscoped: false);
        }

        var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toUtc = new DateTimeOffset(to.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
        var pageSize = request.PageSize <= 0
            ? _options.Activity.DefaultPageSize
            : Math.Min(request.PageSize, _options.Activity.MaxPageSize);

        var guardResult = await RowThresholdGuard.CheckAsync(
            async probeCt =>
            {
                var probe = await _activityLogService.QueryScopedAsync(
                    scope, page: 1, pageSize: 1, probeCt,
                    fromUtc, toUtc, request.ProjectId, request.EntityType, request.ActorId);
                return probe.TotalCount;
            },
            _options.LargeReportRowThreshold, ct);

        if (!guardResult.IsSuccess)
        {
            return Result<ActivityReportDto>.Failure(guardResult.Error!);
        }

        var paged = await _activityLogService.QueryScopedAsync(
            scope, request.Page, pageSize, ct,
            fromUtc, toUtc, request.ProjectId, request.EntityType, request.ActorId);

        var items = paged.Items
            .Select(e => new ActivityReportRowDto(e.Id, e.Timestamp, e.ActorId, e.ActorName, e.Action, e.EntityType, e.EntityId, e.ChangeSummary))
            .ToList();

        var dto = new ActivityReportDto(
            "Activity", DateTimeOffset.UtcNow, caller.Role,
            new ReportWindowDto(from, to), "UTC",
            items, paged.Page, paged.PageSize, paged.TotalCount, paged.TotalPages);

        await ReportGenerationAudit.RecordAsync(
            _activityLogService, _options, caller, "Activity",
            new { from, to, projectId = request.ProjectId, entityType = request.EntityType, actorId = request.ActorId, page = request.Page, pageSize }, ct);
        await _db.SaveChangesAsync(ct);

        return Result<ActivityReportDto>.Success(dto);
    }
}
