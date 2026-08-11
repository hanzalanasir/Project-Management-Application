using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Dashboard.Common;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Dashboard.GetActivity;

/// <summary>
/// Builds the visible-project scope (T008's resolver — same one US1/US3 use) and hands it to
/// <see cref="IActivityLogService.QueryScopedAsync"/>. <b>Never</b> queries <c>db.ActivityLogs</c>
/// directly (FR-006, ADR-0006 addendum) — that read path belongs to 001 alone.
/// </summary>
/// <remarks>Performs no writes. There is no mutation path here — not even to record that the feed
/// was viewed (DoD 8, FR-010; proven by <c>NoWriteGuaranteeTests</c>).</remarks>
public class GetDashboardActivityQueryHandler : IRequestHandler<GetDashboardActivityQuery, Result<PagedResult<ActivityEntryDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectAccessPolicy _projectAccessPolicy;
    private readonly IActivityLogService _activityLogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly DashboardOptions _options;

    public GetDashboardActivityQueryHandler(
        IApplicationDbContext db,
        IProjectAccessPolicy projectAccessPolicy,
        IActivityLogService activityLogService,
        ICurrentUserService currentUserService,
        IOptions<DashboardOptions> options)
    {
        _db = db;
        _projectAccessPolicy = projectAccessPolicy;
        _activityLogService = activityLogService;
        _currentUserService = currentUserService;
        _options = options.Value;
    }

    public async Task<Result<PagedResult<ActivityEntryDto>>> Handle(GetDashboardActivityQuery request, CancellationToken ct)
    {
        var caller = _currentUserService.Current;
        var pageSize = request.PageSize <= 0
            ? _options.Activity.DefaultPageSize
            : Math.Min(request.PageSize, _options.Activity.MaxPageSize);

        // ActivityScope (001's shared-kernel type) needs a materialized collection, not the
        // un-materialized IQueryable<Guid> VisibleProjectScope normally returns — this is the one
        // legitimate exit point where 005 materializes it, since it's crossing into a different
        // abstraction's contract. Skipped entirely for Admin (Unscoped fast path), so the only
        // caller ever paying this cost is a PM/TM, whose visible-project set is bounded, never the
        // whole table.
        var scope = caller.Role == nameof(Role.Admin)
            ? new ActivityScope(Array.Empty<Guid>(), Unscoped: true)
            : new ActivityScope(
                await VisibleProjectScope.Resolve(_db, _projectAccessPolicy, caller).ToListAsync(ct),
                Unscoped: false);

        var paged = await _activityLogService.QueryScopedAsync(scope, request.Page, pageSize, ct);

        var items = paged.Items
            .Select(e => new ActivityEntryDto(e.Id, e.ActorName, e.Action, e.EntityType, e.EntityId, e.Timestamp, e.ChangeSummary))
            .ToList();

        return Result<PagedResult<ActivityEntryDto>>.Success(
            new PagedResult<ActivityEntryDto>(items, paged.Page, paged.PageSize, paged.TotalCount, paged.TotalPages));
    }
}
