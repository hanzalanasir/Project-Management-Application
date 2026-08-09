using MediatR;
using Microsoft.AspNetCore.Identity;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    private static readonly HashSet<string> DuplicateErrorCodes = new() { "DuplicateUserName", "DuplicateEmail" };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLog;
    private readonly IApplicationDbContext _db;

    public RegisterCommandHandler(UserManager<ApplicationUser> userManager, IActivityLogService activityLog, IApplicationDbContext db)
    {
        _userManager = userManager;
        _activityLog = activityLog;
        _db = db;
    }

    public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var isDuplicate = createResult.Errors.Any(e => DuplicateErrorCodes.Contains(e.Code));
            var message = string.Join(" ", createResult.Errors.Select(e => e.Description));

            return Result<UserDto>.Failure(isDuplicate
                ? new Error(ErrorKind.Conflict, "Email already registered.")
                : new Error(ErrorKind.Validation, message));
        }

        // Client-supplied role is never read — self-registration always assigns TeamMember.
        await _userManager.AddToRoleAsync(user, Role.TeamMember.ToString());

        await _activityLog.LogAsync(
            actorId: null,
            action: nameof(AuditAction.UserRegistered),
            entityType: "User",
            entityId: user.Id.ToString(),
            changeSummary: $"User {user.Email} registered as {Role.TeamMember}",
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new UserDto(user.Id, user.FullName, user.Email, Role.TeamMember.ToString(), user.CreatedAt);
        return Result<UserDto>.Success(dto);
    }
}
