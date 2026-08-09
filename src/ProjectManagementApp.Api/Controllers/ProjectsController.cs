using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApp.Api.Common;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Projects;
using ProjectManagementApp.Application.Features.Projects.CreateProject;
using ProjectManagementApp.Application.Features.Projects.DeleteProject;
using ProjectManagementApp.Application.Features.Projects.GetProjectById;
using ProjectManagementApp.Application.Features.Projects.ListProjects;
using ProjectManagementApp.Application.Features.Projects.UpdateProject;

namespace ProjectManagementApp.Api.Controllers;

/// <summary>
/// Project lifecycle CRUD and role-scoped listing — the anchor domain entity (spec 002). The role
/// gate here is attribute-only (Constitution V.2); the finer ownership/assignment scope gate lives
/// in each slice handler via the shared <c>IProjectAccessPolicy</c> (research R-1).
/// </summary>
/// <remarks>
/// Route stubs only (T021) — each action is wired to its slice's <c>MediatR.Send</c> as that
/// story's phase lands (T033 create, T045 list, T054 detail, T065 update, T074 delete).
/// </remarks>
[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ProjectsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>Role-scoped at the query source — <c>totalCount</c> reflects only what the caller may see, never a system-wide total (FR-007).</summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<ProjectSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListProjectsQuery(page, pageSize, search, status, sort), ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ProjectManager")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProject(CreateProjectCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            Response.WriteETag(result.Value!.Version);
        }

        var location = result.IsSuccess ? $"/api/projects/{result.Value!.Id}" : null;
        return result.ToActionResult(StatusCodes.Status201Created, location);
    }

    // No :guid route constraint here (unlike PUT/DELETE): a malformed id must reach this action and
    // be reported as a 400 (contract), not fall through route matching to a bare 404 (research/quickstart V5).
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectById(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Malformed id", detail: $"'{id}' is not a valid project id.");
        }

        var result = await _mediator.Send(new GetProjectByIdQuery(parsedId), ct);

        if (result.IsSuccess)
        {
            Response.WriteETag(result.Value!.Version);
        }

        return result.ToActionResult();
    }

    public sealed record UpdateProjectRequestBody(string Name, string? Description, DateOnly StartDate, DateOnly? EndDate, string Status, Guid? OwnerId);

    /// <summary>Requires <c>If-Match</c> (absent → 400, stale → 409) — optimistic concurrency is mandatory, never a silent last-write-wins (ADR-0004).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ProjectManager")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectRequestBody body, CancellationToken ct)
    {
        if (!Request.TryParseIfMatch(out var ifMatchVersion))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "If-Match header is required.",
            });
        }

        var command = new UpdateProjectCommand(id, body.Name, body.Description, body.StartDate, body.EndDate, body.Status, body.OwnerId, ifMatchVersion);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            Response.WriteETag(result.Value!.Version);
        }

        return result.ToActionResult();
    }

    /// <summary>Hard delete; cascades to tasks/team-member assignments at the database level. No <c>If-Match</c> — a delete has no lost-update failure mode (ADR-0007 §3).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,ProjectManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteProjectCommand(id), ct);
        return result.ToActionResult(StatusCodes.Status204NoContent);
    }
}
