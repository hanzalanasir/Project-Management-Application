using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Features.Reports.GetProjectProgress;

/// <summary>Matches <c>docs/contracts/reports.v1.yaml</c>'s <c>ProjectProgressRow</c> exactly.</summary>
public sealed record ProjectProgressRowDto(
    Guid ProjectId,
    string ProjectName,
    string Status,
    int TotalTasks,
    int OpenTasks,
    int ClosedTasks,
    int OverdueTasks,
    double CompletionPercent,
    DateOnly? ProjectedCompletion);

public sealed record ProjectProgressTotalsDto(int Projects, double AvgCompletionPercent);

/// <summary>Matches <c>ProjectProgressReport</c> — <see cref="ReportEnvelope"/> plus <c>rows</c>/<c>totals</c>.</summary>
public sealed record ProjectProgressReportDto(
    string ReportType,
    DateTimeOffset GeneratedAt,
    string Scope,
    ReportWindowDto Window,
    string TimeZone,
    IReadOnlyList<ProjectProgressRowDto> Rows,
    ProjectProgressTotalsDto Totals);
