using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApp.Api.Common;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Projects.GetProjectById;
using ProjectManagementApp.Application.Features.Tasks;
using ProjectManagementApp.Application.Features.Tasks.CreateTask;
using ProjectManagementApp.Application.Features.Tasks.GetTaskById;
using ProjectManagementApp.Application.Features.Tasks.ListTasks;
using ProjectManagementApp.Application.Features.Tasks.UpdateTask;
using ProjectManagementApp.Application.Features.Tasks.UpdateTaskStatus;

namespace ProjectManagementApp.Api.Controllers;

/// <summary>
/// Task lifecycle within a project, and the graduated authorization model (spec 003): the role gate
/// here is attribute-only (Constitution V.2); the finer scope/mutation-kind gate lives in each
/// slice handler via the shared <c>ITaskAccessPolicy</c> (research R-1). <c>/status</c> and
/// <c>/assignee</c> are nouns, not convenience routes — they are the structural half of the
/// graduated model (data-model.md §3).
/// </summary>
/// <remarks>
/// Route stubs only (T022) — each action is wired to its slice's <c>MediatR.Send</c> as that
/// story's phase lands (T034 create; list/detail/edit/status/delete/reassign land in later stages).
/// </remarks>
[ApiController]
[Route("api")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// The nested route names a project, so it can 403/404 — it pre-populates the <see cref="ListTasksQuery.ProjectId"/>
    /// filter and reuses <see cref="GetProjectByIdQuery"/> for the existence/visibility gate (research R-4).
    /// </summary>
    [HttpGet("projects/{projectId}/tasks")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<TaskSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProjectTasks(
        Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] Guid? assigneeId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
    {
        var projectCheck = await _mediator.Send(new GetProjectByIdQuery(projectId), ct);
        if (!projectCheck.IsSuccess)
        {
            return projectCheck.ToActionResult();
        }

        var result = await _mediator.Send(new ListTasksQuery(page, pageSize, projectId, status, assigneeId, search, sort), ct);
        return result.ToActionResult();
    }

    public sealed record CreateTaskRequestBody(string Title, string? Description, string? Priority, DateOnly? DueDate, Guid? AssigneeId);

    [HttpPost("projects/{projectId}/tasks")]
    [Authorize(Roles = "Admin,ProjectManager")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTask(Guid projectId, CreateTaskRequestBody body, CancellationToken ct)
    {
        var command = new CreateTaskCommand(projectId, body.Title, body.Description, body.Priority, body.DueDate, body.AssigneeId);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            Response.WriteETag(result.Value!.Version);
        }

        var location = result.IsSuccess ? $"/api/tasks/{result.Value!.Id}" : null;
        return result.ToActionResult(StatusCodes.Status201Created, location);
    }

    /// <summary>
    /// The flat route names no resource — scope only shapes content. Never 403/404 (research R-4).
    /// </summary>
    [HttpGet("tasks")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<TaskSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? projectId = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? assigneeId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListTasksQuery(page, pageSize, projectId, status, assigneeId, search, sort), ct);
        return result.ToActionResult();
    }

    // No :guid route constraint (same reasoning as ProjectsController.GetProjectById): a malformed
    // id must reach this action and be reported as 400, not fall through route matching to a bare 404.
    [HttpGet("tasks/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskById(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Malformed id", detail: $"'{id}' is not a valid task id.");
        }

        var result = await _mediator.Send(new GetTaskByIdQuery(parsedId), ct);

        if (result.IsSuccess)
        {
            Response.WriteETag(result.Value!.Version);
        }

        return result.ToActionResult();
    }

    public sealed record UpdateTaskRequestBody(string Title, string? Description, string Priority, DateOnly? DueDate);

    /// <summary>
    /// Deliberately <c>[Authorize]</c>, not role-restricted: the role gate for FullEdit lives
    /// ENTIRELY inside <c>CanMutateAsync</c>, not the attribute (a deviation from this task's
    /// literal text — see stage-3 notes in tasks.md). A TeamMember must reach the handler so its
    /// 403 can name the narrower right they DO have ("...but not its details") — an attribute-level
    /// role rejection returns an empty body (no <c>IAuthorizationMiddlewareResultHandler</c> is
    /// registered in this codebase), which cannot carry that message, and quickstart V2 requires it.
    /// </summary>
    [HttpPut("tasks/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskRequestBody body, CancellationToken ct)
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

        var command = new UpdateTaskCommand(id, body.Title, body.Description, body.Priority, body.DueDate, ifMatchVersion);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            Response.WriteETag(result.Value!.Version);
        }

        return result.ToActionResult();
    }

    [HttpDelete("tasks/{id:guid}")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public IActionResult DeleteTask(Guid id) => StatusCode(StatusCodes.Status501NotImplemented);

    public sealed record UpdateTaskStatusRequestBody(string Status);

    /// <summary>
    /// The one write endpoint permitting all three roles — <c>CanMutateAsync(StatusChange)</c> is
    /// what actually restricts a TeamMember to their own assigned task (the graduated cell).
    /// </summary>
    [HttpPut("tasks/{id:guid}/status")]
    [Authorize]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, UpdateTaskStatusRequestBody body, CancellationToken ct)
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

        var command = new UpdateTaskStatusCommand(id, body.Status, ifMatchVersion);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            Response.WriteETag(result.Value!.Version);
        }

        return result.ToActionResult();
    }

    [HttpPut("tasks/{id:guid}/assignee")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public IActionResult ReassignTask(Guid id) => StatusCode(StatusCodes.Status501NotImplemented);
}
