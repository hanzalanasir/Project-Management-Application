using ProjectManagementApp.Application.Features.Auth.Register;

namespace ProjectManagementApp.Application.Features.Auth.Login;

// RefreshToken here is the RAW opaque value, for the controller to place in Set-Cookie only —
// it must never be serialized into an HTTP response body (FR-016). The controller is
// responsible for stripping it before returning JSON.
public sealed record AuthTokens(
    string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt, UserDto User);
