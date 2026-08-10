using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Common.Authorization;

/// <summary>
/// The binary access policy for 004 (data-model.md §3, research R-1) — two methods, deliberately
/// not one: a ProjectManager can be a MEMBER of a project they do not own (any active user is
/// eligible), and that manager must pass <see cref="CanViewTeamAsync"/> but fail
/// <see cref="CanManageTeamAsync"/>. No <c>ApplyScope</c> — every operation is already pinned to
/// one project by the route, so there is no cross-collection query to scope. Lives in
/// Application, not Infrastructure — authorization rules are business rules (002 R-1).
/// </summary>
public sealed class TeamAccessPolicy : ITeamAccessPolicy
{
    private readonly IApplicationDbContext _db;

    public TeamAccessPolicy(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<AccessDecision> CanViewTeamAsync(Project project, CurrentUser caller, CancellationToken ct)
    {
        if (caller.Role == nameof(Role.Admin))
        {
            return new AccessDecision(true);
        }

        if (caller.Role == nameof(Role.ProjectManager) && project.OwnerId == caller.UserId)
        {
            return new AccessDecision(true);
        }

        // Owner-PM already returned above; a non-owner PM falls through to the same membership
        // check as a TeamMember — queried directly against db.TeamMembers, never a navigation
        // collection, for the same reason ProjectAccessPolicy.CanReadAsync does (research.md R-1).
        var isMember = await _db.TeamMembers
            .AnyAsync(tm => tm.ProjectId == project.Id && tm.UserId == caller.UserId, ct);
        return new AccessDecision(isMember);
    }

    /// <inheritdoc />
    public async Task<AccessDecision> CanManageTeamAsync(Project project, CurrentUser caller, CancellationToken ct)
    {
        if (caller.Role == nameof(Role.Admin))
        {
            return new AccessDecision(true);
        }

        if (caller.Role == nameof(Role.ProjectManager))
        {
            return new AccessDecision(project.OwnerId == caller.UserId);
        }

        // TeamMember can never manage, regardless of membership. This is the ONLY gate now — the
        // controller uses plain [Authorize], not [Authorize(Roles=...)] (T060), so every denial
        // reaches here and gets logged, rather than being silently refused at the attribute.
        await Task.CompletedTask;
        return new AccessDecision(false);
    }
}
