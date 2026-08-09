namespace ProjectManagementApp.Application.Common.Options;

// Declared in Application, not Api/Configuration (tasks.md T005's literal location) — same
// relocation 002 already made for ProjectsOptions and for the identical reason: every value here
// is consumed by a slice handler and Application MUST NOT reference Api (Constitution II.2,
// enforced by LayerDependencyTests). Api still owns the services.Configure<TasksOptions>(...)
// binding call in Program.cs; only the class itself moved.
public class TasksOptions
{
    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;
    public string DefaultStatus { get; set; } = "ToDo";
    public string DefaultPriority { get; set; } = "Medium";
    public bool EnforceStatusWorkflow { get; set; }
    public bool AllowUnassigned { get; set; } = true;
    public bool AllowAssignToInactiveUser { get; set; }
    public bool AllowWritesToTerminalStatusProject { get; set; } = true;
    public bool MaskOutOfScopeAs404 { get; set; }
    public int MaxTitleLength { get; set; } = 200;
    public int MaxDescriptionLength { get; set; } = 2000;
}
