using System.Security.Claims;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Api.Services;

// Materialized from the validated JWT via IHttpContextAccessor — never from a request body or
// query string (shared-contracts.md §2). Registered scoped; only meaningful behind [Authorize].
//
// Reads the literal JWT claim names ("sub", "email", "role") TokenService wrote, not ClaimTypes.*
// URIs — Program.cs sets MapInboundClaims = false specifically so these are not remapped.
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser Current
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User
                ?? throw new InvalidOperationException("No HttpContext is available.");

            var userIdClaim = user.FindFirstValue("sub")
                ?? throw new InvalidOperationException("Current request has no authenticated user.");

            var email = user.FindFirstValue("email") ?? string.Empty;
            var role = user.FindFirstValue("role") ?? string.Empty;
            var fullName = user.FindFirstValue("full_name") ?? string.Empty;

            return new CurrentUser(Guid.Parse(userIdClaim), email, role, fullName);
        }
    }
}
