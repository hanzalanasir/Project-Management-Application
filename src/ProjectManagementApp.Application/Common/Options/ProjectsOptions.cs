namespace ProjectManagementApp.Application.Common.Options;

// Declared in Application, not Api/Configuration (tasks.md T005's literal location), because
// every value here is consumed by a slice handler (ListProjectsQueryHandler,
// CreateProjectCommandHandler, etc.) and Application MUST NOT reference Api (Constitution II.2,
// enforced by LayerDependencyTests). Api still owns the services.Configure<ProjectsOptions>(...)
// binding call in Program.cs; only the class itself moved — the same pattern 001 used relocating
// JwtOptions to Infrastructure when Application-layer code needed it directly.
public class ProjectsOptions
{
    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;
    public string DefaultStatus { get; set; } = "Planning";
    public bool MaskOutOfScopeAs404 { get; set; }
    public bool AllowOwnershipTransfer { get; set; } = true;
    public int MaxNameLength { get; set; } = 200;
    public int MaxDescriptionLength { get; set; } = 2000;
}
