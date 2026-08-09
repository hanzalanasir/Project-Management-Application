using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Features.Auth.Logout;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Auth;

public class LogoutCommandHandlerTests
{
    private static (IApplicationDbContext db, IActivityLogService activityLog) CreateDependencies(RefreshToken? existingToken)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var tokens = existingToken is null
            ? new List<RefreshToken>()
            : new List<RefreshToken> { existingToken };
        var mockDbSet = tokens.BuildMockDbSet();
        db.RefreshTokens.Returns(mockDbSet);

        return (db, Substitute.For<IActivityLogService>());
    }

    [Fact]
    public async Task Handle_LiveToken_RevokesIt_AndAudits()
    {
        var userId = Guid.NewGuid();
        var token = new RefreshToken { Id = Guid.NewGuid(), UserId = userId, TokenHash = "hash-abc", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), CreatedAt = DateTimeOffset.UtcNow };
        var (db, activityLog) = CreateDependencies(token);
        var tokenService = Substitute.For<ITokenService>();
        tokenService.HashRefreshToken("raw-token").Returns("hash-abc");

        var handler = new LogoutCommandHandler(db, activityLog, tokenService);
        var result = await handler.Handle(new LogoutCommand(userId, "raw-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        token.RevokedAt.Should().NotBeNull();
        await activityLog.Received(1).LogAsync(userId, "UserLoggedOut", "User", userId.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadyRevokedOrAbsentToken_StillSucceeds_Idempotently()
    {
        var userId = Guid.NewGuid();
        var (db, activityLog) = CreateDependencies(existingToken: null);
        var tokenService = Substitute.For<ITokenService>();
        tokenService.HashRefreshToken(Arg.Any<string>()).Returns("hash-does-not-exist");

        var handler = new LogoutCommandHandler(db, activityLog, tokenService);
        var result = await handler.Handle(new LogoutCommand(userId, "already-expired-or-absent"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NullPresentedToken_StillSucceeds_Idempotently()
    {
        var userId = Guid.NewGuid();
        var (db, activityLog) = CreateDependencies(existingToken: null);
        var tokenService = Substitute.For<ITokenService>();

        var handler = new LogoutCommandHandler(db, activityLog, tokenService);
        var result = await handler.Handle(new LogoutCommand(userId, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
