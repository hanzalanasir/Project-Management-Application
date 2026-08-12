using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Features.Reports.GetTaskCompletion;

/// <summary>Matches <c>docs/contracts/reports.v1.yaml</c>'s <c>TaskCompletionBucket</c> exactly.</summary>
public sealed record TaskCompletionBucketDto(DateOnly PeriodStart, string PeriodLabel, int CompletedCount);

public sealed record TaskCompletionTotalsDto(int Completed);

public sealed record TaskCompletionReportDto(
    string ReportType,
    DateTimeOffset GeneratedAt,
    string Scope,
    ReportWindowDto Window,
    string TimeZone,
    string GroupBy,
    IReadOnlyList<TaskCompletionBucketDto> Buckets,
    TaskCompletionTotalsDto Totals);
