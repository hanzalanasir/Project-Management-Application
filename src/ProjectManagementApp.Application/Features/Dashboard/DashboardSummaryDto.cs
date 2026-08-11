namespace ProjectManagementApp.Application.Features.Dashboard;

/// <summary>
/// Matches <c>docs/contracts/dashboard.v1.yaml</c>'s <c>DashboardSummary</c> exactly — every enum
/// key required, no <c>additionalProperties</c>. A caller with an empty scope receives this same
/// shape, all zeros (data-model.md §4).
/// </summary>
/// <remarks>
/// <c>ProjectsByStatus</c>/<c>TasksByStatus</c> are the fixed-shape <see cref="ProjectStatusCountsDto"/>/
/// <see cref="TaskStatusCountsDto"/> types, not <c>Dictionary&lt;,&gt;</c> — see those types' remarks
/// for why a dictionary fails the contract drift gate despite serializing correctly at runtime.
/// </remarks>
public sealed record DashboardSummaryDto(
    DateTimeOffset GeneratedAt,
    string Scope,
    int VisibleProjectCount,
    ProjectStatusCountsDto ProjectsByStatus,
    TaskStatusCountsDto TasksByStatus,
    int OverdueTaskCount,
    decimal CompletionRate,
    int BlockedTaskCount,
    int VisibleTeamMemberCount,
    PersonalTaskSummaryDto PersonalTasks);
