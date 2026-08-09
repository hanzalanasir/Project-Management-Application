using MediatR;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.Register;

namespace ProjectManagementApp.Application.Features.Auth.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<Result<UserDto>>;
