using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Common.Interfaces;

/// <summary>Token signing/validation — used by the Login and Refresh handlers.</summary>
public interface ITokenService
{
    /// <summary>Signs a short-lived JWT carrying <c>sub</c>, <c>email</c>, a single <c>role</c>, and <c>full_name</c>.</summary>
    string CreateAccessToken(ApplicationUser user, string role);

    /// <summary>Generates an opaque, high-entropy refresh token. The caller persists only its hash.</summary>
    string CreateRefreshToken();

    /// <summary>Hashes a raw refresh token (SHA-256) — the only form ever persisted or compared.</summary>
    string HashRefreshToken(string rawToken);

    /// <summary>Looks up a presented refresh token by its hash. Returns null when expired, revoked, or unknown.</summary>
    Task<RefreshToken?> ValidateRefreshTokenAsync(string presented, CancellationToken ct);

    // So handlers can compute an access token's ExpiresAt for the response body without
    // Application needing to depend on Infrastructure's JwtOptions.
    /// <summary>The configured access-token lifetime, for computing a response's <c>expiresAt</c>.</summary>
    TimeSpan AccessTokenLifetime { get; }

    /// <summary>The configured refresh-token lifetime, for computing the refresh cookie's expiry.</summary>
    TimeSpan RefreshTokenLifetime { get; }
}
