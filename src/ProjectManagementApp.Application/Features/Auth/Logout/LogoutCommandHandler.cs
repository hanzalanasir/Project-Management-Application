using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IActivityLogService _activityLog;
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(IApplicationDbContext db, IActivityLogService activityLog, ITokenService tokenService)
    {
        _db = db;
        _activityLog = activityLog;
        _tokenService = tokenService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.PresentedRefreshToken))
        {
            // Absent token — succeed idempotently (US-001-03 edge case).
            return Result.Success();
        }

        var hash = _tokenService.HashRefreshToken(request.PresentedRefreshToken);
        var token = await _db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (token is null || token.RevokedAt is not null)
        {
            // Unknown or already-revoked token — succeed idempotently, no duplicate audit row.
            return Result.Success();
        }

        token.RevokedAt = DateTimeOffset.UtcNow;

        await _activityLog.LogAsync(
            actorId: request.UserId,
            action: nameof(AuditAction.UserLoggedOut),
            entityType: "User",
            entityId: request.UserId.ToString(),
            changeSummary: "User logged out",
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
