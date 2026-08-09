using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Common.Authorization;

/// <summary>
/// Layer 2 of the two-layer authorization model (spec 002 T.2) — the ownership/assignment gate no
/// role attribute can express. Lives in Application, not Infrastructure: scope rules are business
/// rules (research.md R-1), and <see cref="IApplicationDbContext"/> is a shared-kernel Application
/// abstraction, so no database-layer dependency is needed to reach it.
/// </summary>
public sealed class ProjectAccessPolicy : IProjectAccessPolicy
{
    private readonly IApplicationDbContext _db;

    public ProjectAccessPolicy(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public IQueryable<Project> ApplyScope(IQueryable<Project> source, CurrentUser caller) => caller.Role switch
    {
        nameof(Role.Admin) => source,
        nameof(Role.ProjectManager) => source.Where(p => p.OwnerId == caller.UserId),
        _ => source.Where(p => p.TeamMembers.Any(tm => tm.UserId == caller.UserId)),
    };

    /// <inheritdoc />
    public async Task<AccessDecision> CanReadAsync(Project project, CurrentUser caller, CancellationToken ct)
    {
        if (caller.Role == nameof(Role.Admin))
        {
            return new AccessDecision(true);
        }

        if (caller.Role == nameof(Role.ProjectManager))
        {
            return new AccessDecision(project.OwnerId == caller.UserId);
        }

        // Queried directly against db.TeamMembers, never the project.TeamMembers navigation —
        // a caller that loaded `project` via a plain SingleOrDefaultAsync (the common case in the
        // read/write handlers) would otherwise see an empty, unloaded collection (research.md R-1).
        var isAssigned = await _db.TeamMembers
            .AnyAsync(tm => tm.ProjectId == project.Id && tm.UserId == caller.UserId, ct);
        return new AccessDecision(isAssigned);
    }

    /// <inheritdoc />
    public async Task<AccessDecision> CanMutateAsync(Project project, CurrentUser caller, CancellationToken ct)
    {
        if (caller.Role == nameof(Role.Admin))
        {
            return new AccessDecision(true);
        }

        if (caller.Role == nameof(Role.ProjectManager))
        {
            return new AccessDecision(project.OwnerId == caller.UserId);
        }

        // TeamMember can never mutate, regardless of assignment — also blocked earlier at the
        // role gate (data-model.md §3 decision table).
        await Task.CompletedTask;
        return new AccessDecision(false);
    }
}
