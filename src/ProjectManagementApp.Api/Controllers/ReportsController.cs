using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApp.Api.Common;
using ProjectManagementApp.Application.Features.Reports.GetCatalog;
using ProjectManagementApp.Application.Features.Reports.GetProjectProgress;
using ProjectManagementApp.Application.Features.Reports.GetActivityReport;
using ProjectManagementApp.Application.Features.Reports.GetTaskCompletion;
using ProjectManagementApp.Application.Features.Reports.GetTeamPerformance;

namespace ProjectManagementApp.Api.Controllers;

/// <summary>
/// Parameterized, role-scoped, read-only analytics over 001–004 (spec 006, final module). Five
/// <c>GET</c>s, no write verbs anywhere, and — unlike 005's Dashboard — a report can <b>name</b> a
/// specific project or user, so 403 is a real outcome here (data-model.md §3). No <c>?format</c>
/// parameter and no export route: PDF/CSV are rendered entirely client-side (research R-3).
/// </summary>
/// <remarks>
/// Every action below is a query — the one exception is the single <c>ReportGenerated</c> audit row
/// each of the four data reports (all but <see cref="GetCatalog"/>) writes via
/// <c>ReportGenerationAudit</c> as the last step of its handler (Constitution VIII.3). Exporting to
/// PDF/CSV happens entirely in the browser (<c>ReportExportService</c>, jsPDF + papaparse) over the
/// JSON a report request already returned — it never calls back into this controller, which is why
/// no action here accepts a <c>?format</c> parameter or exposes an export route (research R-3,
/// enforced by <c>ExportArchitectureTests</c>).
/// </remarks>
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>The self-describing report catalog — drives the frontend's dynamic parameter forms. Not audited.</summary>
    [HttpGet("catalog")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ReportDescriptor>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalog(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetReportCatalogQuery(), ct);
        return result.ToActionResult();
    }

    /// <summary>Per-project progress over a date window. Writes one ReportGenerated audit row.</summary>
    [HttpGet("project-progress")]
    [Authorize]
    [ProducesResponseType(typeof(ProjectProgressReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectProgress(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? projectScope,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectProgressQuery(from, to, projectScope), ct);
        return result.ToActionResult();
    }

    /// <summary>Task-completion trend, bucketed by day/week/month. Writes one ReportGenerated audit row.</summary>
    [HttpGet("task-completion")]
    [Authorize]
    [ProducesResponseType(typeof(TaskCompletionReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskCompletion(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? groupBy,
        [FromQuery] string? projectScope,
        [FromQuery] Guid? assigneeId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTaskCompletionQuery(from, to, groupBy, projectScope, assigneeId), ct);
        return result.ToActionResult();
    }

    /// <summary>Per-member throughput/workload/overdue. A TeamMember always receives exactly their own row. Writes one ReportGenerated audit row.</summary>
    [HttpGet("team-performance")]
    [Authorize]
    [ProducesResponseType(typeof(TeamPerformanceReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeamPerformance(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? projectScope,
        [FromQuery] Guid? userId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTeamPerformanceQuery(from, to, projectScope, userId), ct);
        return result.ToActionResult();
    }

    /// <summary>Filtered, paginated activity excerpt. Read only through IActivityLogService. Writes one ReportGenerated audit row. May return 422 if the window is too large.</summary>
    [HttpGet("activity")]
    [Authorize]
    [ProducesResponseType(typeof(ActivityReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivity(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid? projectId,
        [FromQuery] string? entityType,
        [FromQuery] Guid? actorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetActivityReportQuery(from, to, projectId, entityType, actorId, page, pageSize), ct);
        return result.ToActionResult();
    }
}
