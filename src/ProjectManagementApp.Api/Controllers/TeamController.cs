using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApp.Api.Common;
using ProjectManagementApp.Application.Features.Team;
using ProjectManagementApp.Application.Features.Team.AddTeamMember;
using ProjectManagementApp.Application.Features.Team.ListTeam;
using ProjectManagementApp.Application.Features.Team.RemoveTeamMember;

namespace ProjectManagementApp.Api.Controllers;

/// <summary>
/// Project team membership — add, view roster, remove (spec 004). Membership is a link, not a
/// role: the finer ownership/membership scope gate lives in each slice handler via the shared
/// <c>ITeamAccessPolicy</c> (research R-1). No <c>If-Match</c> on any endpoint — a
/// <c>team_members</c> row has no mutable field (research R-2).
/// </summary>
/// <remarks>
/// All three actions use plain <c>[Authorize]</c>, not an attribute-only role gate — a role-only
/// denial (e.g. a TeamMember attempting add/remove) must still reach <c>CanManageTeamAsync</c> so
/// the handler returns a <c>Result</c> the MediatR <c>LoggingBehavior</c> can log with actor,
/// project id, and reason. An attribute-level <c>[Authorize(Roles=...)]</c> denial short-circuits
/// before MediatR.Send is ever called, so nothing gets logged beyond a bare "responded 403" —
/// found and fixed during the 004 polish pass (T060), the same bug 003's T103 found on
/// Delete/Reassign. This is not a security hole: the role decision is still 100% enforced, just
/// one layer later (in <c>CanManageTeamAsync</c>'s unconditional TeamMember denial), and it makes
/// every denial observable, not less.
/// </remarks>
[ApiController]
[Route("api/projects/{projectId:guid}/team")]
public class TeamController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeamController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>View a project's team roster. Admin (any project), owner-or-member PM, or member TM.</summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<TeamMemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectTeam(Guid projectId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ListTeamQuery(projectId), ct);
        return result.ToActionResult();
    }

    public sealed record AddTeamMemberRequestBody(Guid UserId);

    /// <summary>Add any active user to a project's team. Admin, or the owning ProjectManager.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddTeamMember(Guid projectId, AddTeamMemberRequestBody body, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddTeamMemberCommand(projectId, body.UserId), ct);
        var location = result.IsSuccess ? $"/api/projects/{projectId}/team/{body.UserId}" : null;
        return result.ToActionResult(StatusCodes.Status201Created, location);
    }

    /// <summary>Remove a user from a project's team. Blocked with 409 while they hold open tasks in it.</summary>
    [HttpDelete("{userId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTeamMember(Guid projectId, Guid userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveTeamMemberCommand(projectId, userId), ct);
        return result.ToActionResult(StatusCodes.Status204NoContent);
    }
}
