using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MediatR;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Features.Reports.GetProjectProgress;

/// <summary>
/// One row per in-scope project — completion %, open/closed, overdue, and a projected completion
/// date, all computed within scope in SQL. Scope is resolved via <see cref="ReportScope"/> (project
/// membership, not task assignment) so a TeamMember sees the FULL stats of every project they are a
/// member of, not just their own assigned tasks — spec's "member-of projects only" (not
/// <see cref="ITaskAccessPolicy"/>'s assignee-scoped default, which would under-report a TeamMember's
/// own project's totals and break Dashboard/Reports parity for Admin/PM anyway).
/// Writes exactly one <c>ReportGenerated</c> audit row via <c>ReportGenerationAudit</c> as the last
/// step — the one deliberate write in this otherwise read-only feature (Constitution VIII.3).
/// </summary>
public sealed class GetProjectProgressQueryHandler
    : IRequestHandler<GetProjectProgressQuery, Result<ProjectProgressReportDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectAccessPolicy _projectAccessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly ReportsOptions _options;

    public GetProjectProgressQueryHandler(
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

    public async Task<Result<ProjectProgressReportDto>> Handle(GetProjectProgressQuery request, CancellationToken ct)
    {
        var caller = _currentUserService.Current;
        var from = request.From!.Value;
        var to = request.To!.Value;

        var scopeResult = await ReportScope.ResolveAsync(_db, _projectAccessPolicy, caller, request.ProjectScope, ct);
        if (!scopeResult.IsSuccess)
        {
            return Result<ProjectProgressReportDto>.Failure(scopeResult.Error!);
        }

        var visibleProjectIds = scopeResult.Value!;
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toUtc = new DateTimeOffset(to.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
        var windowDays = Math.Max(1, to.DayNumber - from.DayNumber + 1);

        var projects = await _db.Projects
            .Where(p => visibleProjectIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.Status })
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        var rows = new List<ProjectProgressRowDto>(projects.Count);

        foreach (var project in projects)
        {
            var projectTasks = _db.Tasks.Where(t => t.ProjectId == project.Id);

            var total = await projectTasks.CountAsync(ct);
            var closed = await projectTasks.Where(ReportCountingRules.IsClosed).CountAsync(ct);
            var overdue = await projectTasks.Where(ReportCountingRules.IsOverdue(today)).CountAsync(ct);
            var closedInWindow = await projectTasks.Where(ReportCountingRules.ClosedInWindow(fromUtc, toUtc)).CountAsync(ct);
            var open = total - closed;

            var completionPercent = (double)(ReportCountingRules.CompletionRate(closed, total) * 100);
            var avgClosedPerDay = (double)closedInWindow / windowDays;

            DateOnly? projectedCompletion = null;
            if (avgClosedPerDay > 0 && open > 0)
            {
                var daysNeeded = (int)Math.Ceiling(open / avgClosedPerDay);
                projectedCompletion = today.AddDays(daysNeeded);
            }

            rows.Add(new ProjectProgressRowDto(
                project.Id, project.Name, project.Status.ToString(),
                total, open, closed, overdue,
                Math.Round(completionPercent, 2), projectedCompletion));
        }

        var totals = new ProjectProgressTotalsDto(
            rows.Count,
            rows.Count == 0 ? 0 : Math.Round(rows.Average(r => r.CompletionPercent), 2));

        var dto = new ProjectProgressReportDto(
            "ProjectProgress", DateTimeOffset.UtcNow, caller.Role,
            new ReportWindowDto(from, to), "UTC", rows, totals);

        await ReportGenerationAudit.RecordAsync(
            _activityLogService, _options, caller, "ProjectProgress",
            new { from, to, projectScope = request.ProjectScope }, ct);

        // IActivityLogService.LogAsync only stages the row on the caller's unit of work (the
        // Infrastructure implementation's own contract) — every command handler in this codebase
        // commits explicitly, and a read-only query handler has no other SaveChangesAsync in its
        // path, so this is the one write this feature performs, and this is where it must commit.
        await _db.SaveChangesAsync(ct);

        return Result<ProjectProgressReportDto>.Success(dto);
    }
}
