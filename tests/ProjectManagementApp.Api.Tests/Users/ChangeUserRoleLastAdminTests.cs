namespace ProjectManagementApp.Api.Tests.Users;

// T123 asks for an integration test asserting a change that would leave zero Admins returns 409.
// Per research.md R-12, this is NOT constructible as a genuine HTTP-level test distinct from the
// self-check: to call PUT /users/{id}/role at all, the caller must be an Admin. If exactly one
// Admin exists and the caller targets that same last Admin, the caller IS the target — that is
// the self-check (409 via ChangeUserRoleSelfTests), not the last-Admin path. Reaching the
// last-Admin branch via a distinct caller requires a second Admin to exist, at which point
// removing either one leaves at least one Admin remaining — the branch never fires.
// The invariant is proven independently of the self-check at the handler level instead:
// see ChangeUserRoleCommandHandlerTests.Handle_WouldLeaveZeroAdmins_Returns409 (Application.Tests),
// which constructs the scenario directly (a distinct caller demoting the sole remaining Admin)
// without needing an HTTP round trip that cannot actually exist under RBAC's own rules.
