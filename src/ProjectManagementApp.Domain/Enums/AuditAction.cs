namespace ProjectManagementApp.Domain.Enums;

// One shared enum carrying every feature's audit action values (001–006).
// Later features consume values here; none defines its own enum.
public enum AuditAction
{
    // 001 — Auth & RBAC
    UserRegistered,
    UserLoggedIn,
    UserLoggedOut,
    TokenRefreshed,
    UserDeactivated,
    UserSeeded,
    UserRoleChanged,
    UserReactivated,

    // 002 — Project Management
    ProjectCreated,
    ProjectUpdated,
    ProjectDeleted,
    ProjectOwnerChanged,

    // 003 — Task Management
    TaskCreated,
    TaskUpdated,
    TaskStatusChanged,
    TaskReassigned,
    TaskDeleted,

    // 004 — Team Management
    TeamMemberAdded,
    TeamMemberRemoved,

    // 006 — Reports
    ReportGenerated
}
