using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Auth.Register;

public sealed record RegisterCommand(string FullName, string Email, string Password, string ConfirmPassword)
    : IRequest<Result<UserDto>>;
