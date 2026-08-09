using MediatR;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.GetUserById;

namespace ProjectManagementApp.Application.Features.Auth.ChangeUserRole;

public sealed record ChangeUserRoleCommand(Guid CallerId, Guid TargetId, string Role, uint IfMatchVersion)
    : IRequest<Result<AdminUserDetail>>;
