using MediatR;
using Microsoft.AspNetCore.Identity;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.Register;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthTokens>>
{
    private static readonly Error GenericFailure = new(ErrorKind.Unauthenticated, "Invalid credentials");

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IActivityLogService _activityLog;
    private readonly IApplicationDbContext _db;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IActivityLogService activityLog,
        IApplicationDbContext db)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _activityLog = activityLog;
        _db = db;
    }

    public async Task<Result<AuthTokens>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            return Result<AuthTokens>.Failure(GenericFailure);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return Result<AuthTokens>.Failure(GenericFailure);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.Single();

        var accessToken = _tokenService.CreateAccessToken(user, role);
        var rawRefreshToken = _tokenService.CreateRefreshToken();
        var now = DateTimeOffset.UtcNow;

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(rawRefreshToken),
            ExpiresAt = now.Add(_tokenService.RefreshTokenLifetime),
            CreatedAt = now
        });

        await _activityLog.LogAsync(
            actorId: user.Id,
            action: nameof(AuditAction.UserLoggedIn),
            entityType: "User",
            entityId: user.Id.ToString(),
            changeSummary: $"User {user.Email} logged in",
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(user.Id, user.FullName, user.Email!, role, user.CreatedAt);
        var tokens = new AuthTokens(
            accessToken, rawRefreshToken,
            now.Add(_tokenService.AccessTokenLifetime),
            now.Add(_tokenService.RefreshTokenLifetime),
            userDto);
        return Result<AuthTokens>.Success(tokens);
    }
}
