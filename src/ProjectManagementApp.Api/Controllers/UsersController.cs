using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApp.Api.Common;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.ChangeUserRole;
using ProjectManagementApp.Application.Features.Auth.ChangeUserStatus;
using ProjectManagementApp.Application.Features.Auth.GetUserById;
using ProjectManagementApp.Application.Features.Auth.ListUsers;

namespace ProjectManagementApp.Api.Controllers;

/// <summary>
/// Admin-only user administration — list/view every account, change a user's role, and
/// deactivate/reactivate an account (US-001-07/08/09). Unscoped by design: the role gate on the
/// class is the entire authorization surface, so no per-request scope check is layered on top.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public sealed record ChangeUserRoleRequest(string Role);
    public sealed record ChangeUserStatusRequest(bool IsActive);

    /// <summary>Every user in the system, paged — deactivated accounts are included and flagged, never filtered out.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminUserSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListUsersQuery(page, pageSize), ct);
        return result.ToActionResult();
    }

    /// <summary>A single user's detail, with the row version on the <c>ETag</c> header for a subsequent <c>If-Match</c>.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDetail), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);

        if (result.IsSuccess)
        {
            Response.WriteETag(result.Value!.Version);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Changes a user's role. Refused with 409 for self-role-change or if it would leave zero
    /// Admins (both fixed invariants, independent checks). Requires a matching <c>If-Match</c>.
    /// </summary>
    [HttpPut("{id:guid}/role")]
    [ProducesResponseType(typeof(AdminUserDetail), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeRole(Guid id, ChangeUserRoleRequest request, CancellationToken ct)
    {
        if (!Request.TryParseIfMatch(out var ifMatchVersion))
        {
            return BadRequest();
        }

        var command = new ChangeUserRoleCommand(_currentUserService.Current.UserId, id, request.Role, ifMatchVersion);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            Response.WriteETag(result.Value!.Version);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Deactivates or reactivates a user. Deactivating also revokes every active refresh token for
    /// that user (reuses the logout/refresh revocation mechanism — never a second one). Refused
    /// with 409 for self-deactivation. Reactivating does not restore any previously-revoked token.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(AdminUserDetail), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangeUserStatusRequest request, CancellationToken ct)
    {
        if (!Request.TryParseIfMatch(out var ifMatchVersion))
        {
            return BadRequest();
        }

        var command = new ChangeUserStatusCommand(_currentUserService.Current.UserId, id, request.IsActive, ifMatchVersion);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            Response.WriteETag(result.Value!.Version);
        }

        return result.ToActionResult();
    }
}
