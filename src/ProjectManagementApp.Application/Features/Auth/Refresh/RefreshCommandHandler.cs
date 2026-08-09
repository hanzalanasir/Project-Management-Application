using MediatR;
using Microsoft.AspNetCore.Identity;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.Login;
using ProjectManagementApp.Application.Features.Auth.Register;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Auth.Refresh;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, Result<AuthTokens>>
{
    private static readonly Error InvalidToken = new(ErrorKind.Unauthenticated, "Invalid or expired refresh token");

    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IActivityLogService _activityLog;

    public RefreshCommandHandler(
        IApplicationDbContext db, UserManager<ApplicationUser> userManager,
        ITokenService tokenService, IActivityLogService activityLog)
    {
        _db = db;
        _userManager = userManager;
        _tokenService = tokenService;
        _activityLog = activityLog;
    }

    public async Task<Result<AuthTokens>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var oldToken = await _tokenService.ValidateRefreshTokenAsync(request.PresentedRefreshToken, cancellationToken);
        if (oldToken is null)
        {
            return Result<AuthTokens>.Failure(InvalidToken);
        }

        var user = oldToken.User;
        if (!user.IsActive)
        {
            return Result<AuthTokens>.Failure(InvalidToken);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.Single();

        var newRawRefreshToken = _tokenService.CreateRefreshToken();
        var newHash = _tokenService.HashRefreshToken(newRawRefreshToken);
        var now = DateTimeOffset.UtcNow;

        // Revoke-old + insert-new + audit, all staged here and committed in the single
        // SaveChangesAsync below — one transaction (data-model.md §5, T103 proves this).
        oldToken.RevokedAt = now;
        oldToken.ReplacedByToken = newHash;

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAt = now.Add(_tokenService.RefreshTokenLifetime),
            CreatedAt = now
        });

        var accessToken = _tokenService.CreateAccessToken(user, role);

        await _activityLog.LogAsync(
            actorId: user.Id,
            action: nameof(AuditAction.TokenRefreshed),
            entityType: "User",
            entityId: user.Id.ToString(),
            changeSummary: $"Refresh token rotated for {user.Email}",
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(user.Id, user.FullName, user.Email!, role, user.CreatedAt);
        var tokens = new AuthTokens(
            accessToken, newRawRefreshToken,
            now.Add(_tokenService.AccessTokenLifetime),
            now.Add(_tokenService.RefreshTokenLifetime),
            userDto);
        return Result<AuthTokens>.Success(tokens);
    }
}
