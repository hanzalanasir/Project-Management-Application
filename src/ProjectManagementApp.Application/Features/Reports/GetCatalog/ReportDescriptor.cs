namespace ProjectManagementApp.Application.Features.Reports.GetCatalog;

/// <summary>One parameter of a <see cref="ReportDescriptor"/>, matching the contract schema exactly.</summary>
public sealed record ReportParameterDescriptor(string Name, string Type, bool Required);

/// <summary>
/// A single report's self-description — type, title, its ordered parameters, and the client render
/// formats. Drives the frontend's dynamic parameter forms (T030) so a fifth report later needs no
/// new form code. <see cref="Note"/> carries a role annotation (e.g. Team Performance's "self only"
/// for a TeamMember) — <c>null</c> when there is nothing to flag for the caller's role.
/// </summary>
public sealed record ReportDescriptor(
    string Type,
    string Title,
    string? Note,
    IReadOnlyList<ReportParameterDescriptor> Parameters,
    IReadOnlyList<string> Formats);

/// <summary>
/// The four v1 report descriptors (OQ-006-06) — static, role-annotated at read time by
/// <see cref="ReportCatalog.Describe"/>. No database access: this is metadata about the API surface,
/// not a query over it (FR-011 — the catalog is the one report endpoint that is never audited).
/// </summary>
public static class ReportCatalog
{
    private static readonly IReadOnlyList<string> AllFormats = ["json", "pdf", "csv"];

    public static IReadOnlyList<ReportDescriptor> Describe(string callerRole)
    {
        var teamPerformanceNote = callerRole == nameof(Domain.Enums.Role.TeamMember) ? "self only" : null;

        return
        [
            new ReportDescriptor(
                "ProjectProgress",
                "Project Progress",
                null,
                [
                    new ReportParameterDescriptor("from", "date", true),
                    new ReportParameterDescriptor("to", "date", true),
                    new ReportParameterDescriptor("projectScope", "all|projectIds", false),
                ],
                AllFormats),

            new ReportDescriptor(
                "TaskCompletion",
                "Task Completion",
                null,
                [
                    new ReportParameterDescriptor("from", "date", true),
                    new ReportParameterDescriptor("to", "date", true),
                    new ReportParameterDescriptor("groupBy", "day|week|month", true),
                    new ReportParameterDescriptor("projectScope", "all|projectIds", false),
                    new ReportParameterDescriptor("assigneeId", "uuid", false),
                ],
                AllFormats),

            new ReportDescriptor(
                "TeamPerformance",
                "Team Performance",
                teamPerformanceNote,
                [
                    new ReportParameterDescriptor("from", "date", true),
                    new ReportParameterDescriptor("to", "date", true),
                    new ReportParameterDescriptor("projectScope", "all|projectIds", false),
                    new ReportParameterDescriptor("userId", "uuid", false),
                ],
                AllFormats),

            new ReportDescriptor(
                "Activity",
                "Activity",
                null,
                [
                    new ReportParameterDescriptor("from", "date", true),
                    new ReportParameterDescriptor("to", "date", true),
                    new ReportParameterDescriptor("projectId", "uuid", false),
                    new ReportParameterDescriptor("entityType", "User|Project|Task|TeamMember|Report", false),
                    new ReportParameterDescriptor("actorId", "uuid", false),
                    new ReportParameterDescriptor("page", "integer", false),
                    new ReportParameterDescriptor("pageSize", "integer", false),
                ],
                AllFormats),
        ];
    }
}
