using MediatR;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.Login;

namespace ProjectManagementApp.Application.Features.Auth.Refresh;

// The presented refresh token comes from the cookie, never the body.
public sealed record RefreshCommand(string PresentedRefreshToken) : IRequest<Result<AuthTokens>>;
