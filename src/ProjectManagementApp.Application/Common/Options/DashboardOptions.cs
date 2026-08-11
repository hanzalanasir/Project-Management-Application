namespace ProjectManagementApp.Application.Common.Options;

// Declared in Application, not Api/Configuration (tasks.md T005's literal location) — same
// relocation 002/003/004 already made for ProjectsOptions/TasksOptions/TeamOptions and for the
// identical reason: every value here is consumed by a slice handler and Application MUST NOT
// reference Api (Constitution II.2, enforced by LayerDependencyTests). Api still owns the
// services.Configure<DashboardOptions>(...) binding call in Program.cs; only the class itself
// moved.
//
// OverdueBoundary scopes to the date COMPARISON only (strictly-before-today). The timezone is
// NOT a knob here — it is fixed to UTC in MetricDefinitions.IsOverdue, because a configurable
// timezone would let a deployment break 006's NFR-002 cross-feature value-parity requirement
// (research R-2).
public class DashboardOptions
{
    public ActivityOptions Activity { get; set; } = new();
    public string OverdueBoundary { get; set; } = "StrictlyBeforeToday";

    public class ActivityOptions
    {
        public int DefaultPageSize { get; set; } = 20;
        public int MaxPageSize { get; set; } = 100;
    }
}
