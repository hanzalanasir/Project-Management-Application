using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.Common;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Reports.GetTeamPerformance;

/// <summary>
/// Per-member throughput/workload/overdue over 004's member pool — the sharpest least-privilege
/// boundary in the feature (research R-6). A TeamMember's <c>userId</c> is force-clamped to their
/// own id BEFORE any pool lookup, unconditionally: a colleague's id is silently ignored, never
/// checked against the pool, so no 403 can ever leak whether that colleague is even in scope.
/// Admin/ProjectManager get the opposite treatment — a named out-of-scope <c>userId</c> 403s,
/// because they ARE entitled to know their own scope boundary.
/// Writes exactly one <c>ReportGenerated</c> audit row via <c>ReportGenerationAudit</c> as the last
/// step — the one deliberate write in this otherwise read-only feature (Constitution VIII.3).
/// </summary>
public sealed class GetTeamPerformanceQueryHandler
    : IRequestHandler<GetTeamPerformanceQuery, Result<TeamPerformanceReportDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectAccessPolicy _projectAccessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly ReportsOptions _options;

    public GetTeamPerformanceQueryHandler(
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

    public async Task<Result<TeamPerformanceReportDto>> Handle(GetTeamPerformanceQuery request, CancellationToken ct)
    {
        var caller = _currentUserService.Current;
        var from = request.From!.Value;
        var to = request.To!.Value;

        var scopeResult = await ReportScope.ResolveAsync(_db, _projectAccessPolicy, caller, request.ProjectScope, ct);
        if (!scopeResult.IsSuccess)
        {
            return Result<TeamPerformanceReportDto>.Failure(scopeResult.Error!);
        }

        var visibleProjectIds = scopeResult.Value!;

        List<Guid> memberIds;
        if (caller.Role == nameof(Role.TeamMember))
        {
            // The clamp: no pool lookup, no scope check — a colleague's id is discarded outright.
            memberIds = [caller.UserId];
        }
        else
        {
            var pool = _db.TeamMembers
                .Where(tm => visibleProjectIds.Contains(tm.ProjectId))
                .Select(tm => tm.UserId)
                .Distinct();

            if (request.UserId is not null)
            {
                var inPool = await pool.AnyAsync(id => id == request.UserId.Value, ct);
                if (!inPool)
                {
                    return Result<TeamPerformanceReportDto>.Failure(new Error(
                        ErrorKind.Forbidden, "That user is outside your visible scope."));
                }

                memberIds = [request.UserId.Value];
            }
            else
            {
                memberIds = await pool.ToListAsync(ct);
            }
        }

        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toUtc = new DateTimeOffset(to.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var users = await _db.Users
            .Where(u => memberIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.IsActive })
            .ToListAsync(ct);

        var rows = new List<TeamPerformanceRowDto>(users.Count);
        foreach (var user in users)
        {
            // Scoped to the caller's visible projects (not every task ever assigned to this member
            // system-wide) — the same "within scope" boundary every other 006 aggregate honours.
            var memberTasks = _db.Tasks.Where(t => visibleProjectIds.Contains(t.ProjectId) && t.AssigneeId == user.Id);

            var throughput = await memberTasks.Where(ReportCountingRules.ClosedInWindow(fromUtc, toUtc)).CountAsync(ct);
            var workload = await memberTasks.Where(ReportCountingRules.IsOpenAssignment).CountAsync(ct);
            var overdueCount = await memberTasks.Where(ReportCountingRules.IsOverdue(today)).CountAsync(ct);

            rows.Add(new TeamPerformanceRowDto(user.Id, user.FullName, user.IsActive, throughput, workload, overdueCount));
        }

        rows = rows.OrderBy(r => r.FullName, StringComparer.Ordinal).ToList();

        var dto = new TeamPerformanceReportDto(
            "TeamPerformance", DateTimeOffset.UtcNow, caller.Role,
            new ReportWindowDto(from, to), "UTC", rows);

        await ReportGenerationAudit.RecordAsync(
            _activityLogService, _options, caller, "TeamPerformance",
            new { from, to, projectScope = request.ProjectScope, userId = request.UserId }, ct);
        await _db.SaveChangesAsync(ct);

        return Result<TeamPerformanceReportDto>.Success(dto);
    }
}
