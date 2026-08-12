using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Features.Reports.GetTeamPerformance;

/// <summary>Matches <c>docs/contracts/reports.v1.yaml</c>'s <c>TeamPerformanceRow</c> exactly.</summary>
public sealed record TeamPerformanceRowDto(
    Guid UserId,
    string FullName,
    bool IsActive,
    int Throughput,
    int Workload,
    int OverdueCount);

public sealed record TeamPerformanceReportDto(
    string ReportType,
    DateTimeOffset GeneratedAt,
    string Scope,
    ReportWindowDto Window,
    string TimeZone,
    IReadOnlyList<TeamPerformanceRowDto> Rows);
