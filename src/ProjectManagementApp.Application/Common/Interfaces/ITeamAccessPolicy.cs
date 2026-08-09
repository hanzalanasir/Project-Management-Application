using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Common.Interfaces;

// Shared-kernel interface; rules owned/implemented by 004. Deliberately binary — no ApplyScope,
// because every team read is pinned to a single project by the route.
public interface ITeamAccessPolicy
{
    Task<AccessDecision> CanViewTeamAsync(Project project, CurrentUser caller, CancellationToken ct);
    Task<AccessDecision> CanManageTeamAsync(Project project, CurrentUser caller, CancellationToken ct);
}
