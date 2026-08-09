using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.GetUserById;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Auth.ChangeUserStatus;

public class ChangeUserStatusCommandHandler : IRequestHandler<ChangeUserStatusCommand, Result<AdminUserDetail>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLog;

    public ChangeUserStatusCommandHandler(IApplicationDbContext db, UserManager<ApplicationUser> userManager, IActivityLogService activityLog)
    {
        _db = db;
        _userManager = userManager;
        _activityLog = activityLog;
    }

    public async Task<Result<AdminUserDetail>> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.CallerId == request.TargetId && !request.IsActive)
        {
            return Result<AdminUserDetail>.Failure(new Error(ErrorKind.Conflict, "You cannot deactivate your own account."));
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

        if (target.IsActive == request.IsActive)
        {
            var word = request.IsActive ? "active" : "inactive";
            return Result<AdminUserDetail>.Failure(new Error(ErrorKind.Validation, $"User is already {word}."));
        }

        target.IsActive = request.IsActive;
        string auditAction;

        if (!request.IsActive)
        {
            // Reuses RefreshToken.RevokedAt — the same field LogoutCommandHandler and
            // RefreshCommandHandler already set, not a new mechanism (research.md R-13).
            var now = DateTimeOffset.UtcNow;
            var activeTokens = await _db.RefreshTokens
                .Where(t => t.UserId == target.Id && t.RevokedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var token in activeTokens)
            {
                token.RevokedAt = now;
            }

            auditAction = nameof(AuditAction.UserDeactivated);
        }
        else
        {
            // Reactivating does NOT restore any previously-revoked token (US-001-09).
            auditAction = nameof(AuditAction.UserReactivated);
        }

        await _activityLog.LogAsync(
            actorId: request.CallerId,
            action: auditAction,
            entityType: "User",
            entityId: target.Id.ToString(),
            changeSummary: $"{(request.IsActive ? "Reactivated" : "Deactivated")} {target.Email}",
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var role = (await _userManager.GetRolesAsync(target)).Single();
        var dto = new AdminUserDetail(
            target.Id, target.FullName, target.Email!, role, target.IsActive,
            target.CreatedAt, target.UpdatedAt, target.Version);
        return Result<AdminUserDetail>.Success(dto);
    }
}
