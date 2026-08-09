using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Auth.Logout;

public sealed record LogoutCommand(Guid UserId, string? PresentedRefreshToken) : IRequest<Result>;
