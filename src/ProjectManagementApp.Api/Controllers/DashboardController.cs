using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApp.Api.Common;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Dashboard;
using ProjectManagementApp.Application.Features.Dashboard.GetActivity;
using ProjectManagementApp.Application.Features.Dashboard.GetSummary;

namespace ProjectManagementApp.Api.Controllers;

/// <summary>
/// Strictly read-only aggregation over 001–004 (spec 005). No write verbs, and — deliberately,
/// unlike every other controller in this API — no 403/404 anywhere: the dashboard names no
/// resource, so scope shapes content rather than access (research R-3).
/// </summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Role-scoped summary tiles, including the caller's personal task slice.</summary>
    [HttpGet("summary")]
    [Authorize]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardSummaryQuery(), ct);
        return result.ToActionResult();
    }

    /// <summary>Paginated, role-scoped recent-activity feed. The only error statuses are 400 and 401.</summary>
    [HttpGet("activity")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<ActivityEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivity([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDashboardActivityQuery(page, pageSize), ct);
        return result.ToActionResult();
    }
}
