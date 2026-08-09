using MediatR;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.GetUserById;

namespace ProjectManagementApp.Application.Features.Auth.ChangeUserStatus;

public sealed record ChangeUserStatusCommand(Guid CallerId, Guid TargetId, bool IsActive, uint IfMatchVersion)
    : IRequest<Result<AdminUserDetail>>;
