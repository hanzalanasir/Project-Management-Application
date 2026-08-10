namespace ProjectManagementApp.Application.Common.Options;

// Declared in Application, not Api/Configuration (tasks.md T005's literal location) — same
// relocation 002/003 already made for ProjectsOptions/TasksOptions and for the identical reason:
// every value here is consumed by a slice handler and Application MUST NOT reference Api
// (Constitution II.2, enforced by LayerDependencyTests). Api still owns the
// services.Configure<TeamOptions>(...) binding call in Program.cs; only the class itself moved.
public class TeamOptions
{
    public bool AllowAddInactiveUser { get; set; }
    public bool AllowManageOnTerminalStatusProject { get; set; } = true;
    public bool IncludeInactiveMembersInRoster { get; set; } = true;
    public bool MaskOutOfScopeAs404 { get; set; }
}
