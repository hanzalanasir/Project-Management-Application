using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.GetUserById;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Auth.ChangeUserRole;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, Result<AdminUserDetail>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLog;

    public ChangeUserRoleCommandHandler(IApplicationDbContext db, UserManager<ApplicationUser> userManager, IActivityLogService activityLog)
    {
        _db = db;
        _userManager = userManager;
        _activityLog = activityLog;
    }

    public async Task<Result<AdminUserDetail>> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.CallerId == request.TargetId)
        {
            return Result<AdminUserDetail>.Failure(new Error(ErrorKind.Conflict, "You cannot change your own role."));
        }

        var target = await _db.Users.SingleOrDefaultAsync(u => u.Id == request.TargetId, cancellationToken);
        if (target is null)
        {
            return Result<AdminUserDetail>.Failure(new Error(ErrorKind.NotFound, "User not found"));
        }

        if (target.Version != request.IfMatchVersion)
        {
            return Result<AdminUserDetail>.Failure(new Error(ErrorKind.Conflict, "The user has been modified since it was last read."));
        }

        var currentRole = (await _userManager.GetRolesAsync(target)).Single();

        // Independent of the self-check (research.md R-12) — evaluated whenever an Admin is
        // being moved away from Admin, regardless of who is making the change.
        if (currentRole == Role.Admin.ToString() && request.Role != Role.Admin.ToString())
        {
            var admins = await _userManager.GetUsersInRoleAsync(Role.Admin.ToString());
            if (admins.Count <= 1)
            {
                return Result<AdminUserDetail>.Failure(new Error(ErrorKind.Conflict, "At least one Admin must remain."));
            }
        }

        if (currentRole == request.Role)
        {
            return Result<AdminUserDetail>.Failure(new Error(ErrorKind.Validation, $"User already has role {request.Role}."));
        }

        await _userManager.RemoveFromRoleAsync(target, currentRole);
        await _userManager.AddToRoleAsync(target, request.Role);

        await _activityLog.LogAsync(
            actorId: request.CallerId,
            action: nameof(AuditAction.UserRoleChanged),
            entityType: "User",
            entityId: target.Id.ToString(),
            changeSummary: $"Role changed from {currentRole} to {request.Role} for {target.Email}",
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new AdminUserDetail(
            target.Id, target.FullName, target.Email!, request.Role, target.IsActive,
            target.CreatedAt, target.UpdatedAt, target.Version);
        return Result<AdminUserDetail>.Success(dto);
    }
}
