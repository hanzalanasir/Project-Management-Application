using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Api.Common;
using ProjectManagementApp.Api.Configuration;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.GetCurrentUser;
using ProjectManagementApp.Application.Features.Auth.Login;
using ProjectManagementApp.Application.Features.Auth.Logout;
using ProjectManagementApp.Application.Features.Auth.Refresh;
using ProjectManagementApp.Application.Features.Auth.Register;

namespace ProjectManagementApp.Api.Controllers;

/// <summary>
/// Registration, session lifecycle (login/refresh/logout), and current-user lookup.
/// Every action is a thin <see cref="MediatR.IMediator.Send{TResponse}"/> wrapper — no business
/// logic lives here (Constitution II.2).
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly CsrfOptions _csrfOptions;
    private readonly RefreshCookieOptions _refreshCookieOptions;

    public AuthController(
        IMediator mediator, ICurrentUserService currentUserService,
        IOptions<CsrfOptions> csrfOptions, IOptions<RefreshCookieOptions> refreshCookieOptions)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _csrfOptions = csrfOptions.Value;
        _refreshCookieOptions = refreshCookieOptions.Value;
    }

    /// <summary>The body of a successful login/refresh. The refresh token is deliberately absent — it is cookie-only (FR-016).</summary>
    public sealed record AuthTokensResponse(string AccessToken, DateTimeOffset ExpiresAt, UserDto User);

    /// <summary>The body of a successful refresh. Same rationale as <see cref="AuthTokensResponse"/> — the rotated refresh token is cookie-only.</summary>
    public sealed record AccessTokenResponse(string AccessToken, DateTimeOffset ExpiresAt);

    /// <summary>Self-registration. Always assigns TeamMember; any client-supplied role is ignored.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.ToActionResult(StatusCodes.Status201Created, GetLocation(result));
    }

    /// <summary>
    /// Exchanges credentials for a token pair: the access token in the body, the refresh token
    /// only as an httpOnly Secure SameSite cookie — never in the response body (FR-016).
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            return result.ToActionResult();
        }

        var tokens = result.Value!;
        Response.SetRefreshCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAt, _refreshCookieOptions);
        CsrfProtection.IssueToken(Response, _csrfOptions); // Refresh/Logout require this cookie+header pair

        // The refresh token is deliberately excluded here — cookie only, never the body (FR-016).
        return Ok(new AuthTokensResponse(tokens.AccessToken, tokens.ExpiresAt, tokens.User));
    }

    /// <summary>
    /// Rotates the single-use refresh token (cookie-only, CSRF-checked) for a new access/refresh
    /// pair. Anonymous by necessity — it is what issues a token once the caller's JWT has expired.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        if (!CsrfProtection.IsValid(Request, _csrfOptions))
        {
            return BadRequest();
        }

        var presentedRefreshToken = Request.Cookies[_refreshCookieOptions.Name];
        if (string.IsNullOrEmpty(presentedRefreshToken))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new RefreshCommand(presentedRefreshToken), ct);

        if (!result.IsSuccess)
        {
            return result.ToActionResult();
        }

        var tokens = result.Value!;
        Response.SetRefreshCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAt, _refreshCookieOptions);
        CsrfProtection.IssueToken(Response, _csrfOptions);

        return Ok(new AccessTokenResponse(tokens.AccessToken, tokens.ExpiresAt));
    }

    /// <summary>The caller's identity, resolved from the validated token only — zero DB round-trips (NFR-002).</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), ct);
        return result.ToActionResult();
    }

    /// <summary>Revokes the presented refresh token server-side (idempotent) and clears the cookie.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (!CsrfProtection.IsValid(Request, _csrfOptions))
        {
            return BadRequest();
        }

        var presentedRefreshToken = Request.Cookies[_refreshCookieOptions.Name];
        var command = new LogoutCommand(_currentUserService.Current.UserId, presentedRefreshToken);
        var result = await _mediator.Send(command, ct);

        Response.ClearRefreshCookie(_refreshCookieOptions);
        return result.ToActionResult(StatusCodes.Status204NoContent);
    }

    private string? GetLocation(Result<UserDto> result) =>
        result.IsSuccess ? $"/api/users/{result.Value!.Id}" : null;
}
