using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Team;

// Matches contracts/team.v1.yaml#/components/schemas/TeamMemberDto. `role` is the member's
// GLOBAL role (001), read-only for display — a team_members row carries no role column at all
// (data-model.md §2, "the single most important schema fact of this feature").
public sealed record TeamMemberDto(
    Guid MembershipId,
    Guid UserId,
    string FullName,
    [property: EmailAddress] string Email,
    string Role,
    bool IsActive,
    DateTimeOffset AddedAt);

public static class TeamMemberDtoMappingExtensions
{
    public static TeamMemberDto ToDto(this TeamMember member, string role) =>
        new(member.Id, member.UserId, member.User.FullName, member.User.Email!, role, member.User.IsActive, member.CreatedAt);

    /// <summary>
    /// Builds a userId → role lookup for an entire roster in exactly <see cref="Role"/>'s three
    /// values worth of queries, never one per row (analyze finding G2). <c>IApplicationDbContext</c>
    /// exposes no Roles/UserRoles <c>DbSet</c> (shared-contracts §7), so a join is not available —
    /// <see cref="UserManager{TUser}.GetUsersInRoleAsync"/> per role, inverted into a dictionary, is
    /// the batched alternative.
    /// </summary>
    public static async Task<IReadOnlyDictionary<Guid, string>> BuildRoleLookupAsync(
        this UserManager<ApplicationUser> userManager, CancellationToken ct)
    {
        var lookup = new Dictionary<Guid, string>();
        foreach (var role in Enum.GetValues<Role>())
        {
            ct.ThrowIfCancellationRequested();
            var roleName = role.ToString();
            var usersInRole = await userManager.GetUsersInRoleAsync(roleName);
            foreach (var user in usersInRole)
            {
                lookup[user.Id] = roleName;
            }
        }

        return lookup;
    }
}
