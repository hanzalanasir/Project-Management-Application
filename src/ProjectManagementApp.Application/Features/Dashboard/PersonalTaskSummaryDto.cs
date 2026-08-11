namespace ProjectManagementApp.Application.Features.Dashboard;

/// <summary>
/// Matches the contract's <c>PersonalTaskSummary</c>. Tasks assigned to the caller within their
/// visible-project set. For a TeamMember this <b>is</b> the number shown in the summary's task
/// tiles (personal view) — see T046 (data-model.md §2).
/// </summary>
public sealed record PersonalTaskSummaryDto(
    int AssignedTotal,
    TaskStatusCountsDto ByStatus,
    int OverdueCount);
