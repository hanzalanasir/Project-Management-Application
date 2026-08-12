using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Features.Reports.GetActivityReport;

/// <summary>Matches <c>docs/contracts/reports.v1.yaml</c>'s <c>ActivityReportRow</c> exactly.</summary>
public sealed record ActivityReportRowDto(
    Guid Id,
    DateTimeOffset Timestamp,
    Guid? ActorId,
    string ActorName,
    string Action,
    string EntityType,
    string EntityId,
    string ChangeSummary);

public sealed record ActivityReportDto(
    string ReportType,
    DateTimeOffset GeneratedAt,
    string Scope,
    ReportWindowDto Window,
    string TimeZone,
    IReadOnlyList<ActivityReportRowDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
