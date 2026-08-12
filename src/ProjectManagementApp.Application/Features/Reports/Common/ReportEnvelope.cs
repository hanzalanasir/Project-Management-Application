namespace ProjectManagementApp.Application.Features.Reports.Common;

/// <summary>
/// The fields every report DTO shares, matching <c>docs/contracts/reports.v1.yaml</c>'s
/// <c>ReportEnvelope</c> exactly. Embedded via primary-constructor inheritance in each report's own
/// record rather than composed as a nested object, matching the contract's <c>allOf</c> flattening.
/// </summary>
/// <remarks>
/// <see cref="TimeZone"/> is always the literal <c>"UTC"</c> — never a caller- or
/// deployment-configurable value (research R-4). A configurable timezone here would let a
/// deployment silently break NFR-002's value-parity requirement against 005's Dashboard.
/// <see cref="Scope"/> and <see cref="ReportType"/> are plain strings, not C# enums — the app
/// registers no <c>JsonStringEnumConverter</c>, matching every other role/status field in this
/// codebase (validated server-side via whitelist, never bound to a C# enum type).
/// </remarks>
public sealed record ReportEnvelope(
    string ReportType,
    DateTimeOffset GeneratedAt,
    string Scope,
    ReportWindowDto Window,
    string TimeZone = "UTC");

/// <summary>The <c>window</c> object inside <see cref="ReportEnvelope"/> — the resolved, inclusive date range.</summary>
public sealed record ReportWindowDto(DateOnly From, DateOnly To);
