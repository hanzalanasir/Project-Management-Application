using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Persistence;

namespace ProjectManagementApp.Infrastructure.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly ApplicationDbContext _db;

    public ActivityLogService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task LogAsync(Guid? actorId, string action, string entityType, string entityId,
        string changeSummary, CancellationToken ct, Guid? projectId = null)
    {
        // Adds to the caller's unit of work; commits in the SAME SaveChangesAsync as the
        // triggering change (Constitution IV.4) — the caller is responsible for calling SaveChangesAsync.
        _db.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            ActorId = actorId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = DateTimeOffset.UtcNow,
            ChangeSummary = changeSummary,
            ProjectId = projectId
        });

        return Task.CompletedTask;
    }

    public async Task<PagedResult<ActivityEntry>> QueryScopedAsync(
        ActivityScope scope, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.ActivityLogs.AsNoTracking().AsQueryable();

        if (!scope.Unscoped)
        {
            // Scope by the ProjectId stamped at write time — NOT by comparing EntityId against
            // visible-project ids (that only ever matched EntityType "Project" rows; every
            // Task/TeamMember row's EntityId is the task/member's own id, never a project id).
            // A row with no ProjectId (e.g. a User event) has no owning project and is therefore
            // never visible to a scoped (non-Admin) reader.
            var visibleProjectIds = scope.VisibleProjectIds.ToHashSet();
            query = query.Where(a => a.ProjectId != null && visibleProjectIds.Contains(a.ProjectId.Value));
        }

        var totalCount = await query.CountAsync(ct);

        var entries = await query
            .OrderByDescending(a => a.Timestamp).ThenByDescending(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .GroupJoin(_db.Users.AsNoTracking(), a => a.ActorId, u => (Guid?)u.Id, (a, users) => new { a, users })
            .SelectMany(x => x.users.DefaultIfEmpty(), (x, u) => new ActivityEntry(
                x.a.Id, x.a.ActorId, u != null ? u.FullName : "System", x.a.Action,
                x.a.EntityType, x.a.EntityId, x.a.Timestamp, x.a.ChangeSummary))
            .ToListAsync(ct);

        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<ActivityEntry>(entries, page, pageSize, totalCount, totalPages);
    }
}
