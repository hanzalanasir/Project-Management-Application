using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Auth.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<AdminUserDetail>>;
