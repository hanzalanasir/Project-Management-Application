using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Common.Interfaces;

// The DbContext behind an Application-owned interface — not a repository (shared-contracts.md §7).
// A slice's handler composes LINQ directly against these DbSet<T> properties.
/// <summary>
/// The persistence surface every feature's handlers depend on. Not a repository — it exposes the
/// identical <see cref="IQueryable{T}"/> surface as the concrete DbContext, which is load-bearing
/// for scope predicates (<c>ApplyScope</c>) folding into a live query rather than materializing in memory.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<Project> Projects { get; }
    DbSet<TaskItem> Tasks { get; }
    DbSet<TeamMember> TeamMembers { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);

    // Exposes exactly the EF surface needed to set an entity's ORIGINAL concurrency-token value —
    // required so a client-supplied If-Match version (not whatever this context happens to have
    // loaded) drives the `xmin` check on UPDATE (research R-2, data-model.md §6.4). ApplicationDbContext
    // (which already derives from DbContext) satisfies this implicitly; no new implementation needed.
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
