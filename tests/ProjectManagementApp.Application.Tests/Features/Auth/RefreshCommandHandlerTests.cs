using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.Refresh;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Auth;

public class RefreshCommandHandlerTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static IApplicationDbContext CreateDb(List<RefreshToken> tokens)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var mockSet = tokens.BuildMockDbSet();
        db.RefreshTokens.Returns(mockSet);
        return db;
    }

    [Fact]
    public async Task Handle_ValidToken_RotatesIt_AndReturnsNewPair()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "dana@example.com", IsActive = true, FullName = "Dana" };
        var oldToken = new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, User = user, TokenHash = "old-hash", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), CreatedAt = DateTimeOffset.UtcNow };
        var db = CreateDb(new List<RefreshToken> { oldToken });

        var userManager = CreateUserManager();
        userManager.GetRolesAsync(user).Returns(new List<string> { "TeamMember" });

        var tokenService = Substitute.For<ITokenService>();
        tokenService.HashRefreshToken("presented-raw").Returns("old-hash");
        tokenService.ValidateRefreshTokenAsync("presented-raw", Arg.Any<CancellationToken>()).Returns(oldToken);
        tokenService.CreateAccessToken(user, "TeamMember").Returns("new-access-token");
        tokenService.CreateRefreshToken().Returns("new-raw-token");
        tokenService.HashRefreshToken("new-raw-token").Returns("new-hash");

        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new RefreshCommandHandler(db, userManager, tokenService, activityLog);

        var result = await handler.Handle(new RefreshCommand("presented-raw"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("new-access-token");
        result.Value!.RefreshToken.Should().Be("new-raw-token");
        oldToken.RevokedAt.Should().NotBeNull();
        oldToken.ReplacedByToken.Should().Be("new-hash");
        await activityLog.Received(1).LogAsync(user.Id, "TokenRefreshed", "User", user.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownOrInvalidToken_ReturnsUnauthenticated()
    {
        var db = CreateDb(new List<RefreshToken>());
        var userManager = CreateUserManager();
        var tokenService = Substitute.For<ITokenService>();
        tokenService.ValidateRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new RefreshCommandHandler(db, userManager, tokenService, activityLog);
        var result = await handler.Handle(new RefreshCommand("garbage"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Unauthenticated);
    }

    [Fact]
    public async Task Handle_DeactivatedUser_ReturnsUnauthenticated_EvenWithValidToken()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "dana@example.com", IsActive = false };
        var token = new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, User = user, TokenHash = "hash", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), CreatedAt = DateTimeOffset.UtcNow };
        var db = CreateDb(new List<RefreshToken> { token });

        var userManager = CreateUserManager();
        var tokenService = Substitute.For<ITokenService>();
        tokenService.ValidateRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(token);
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new RefreshCommandHandler(db, userManager, tokenService, activityLog);
        var result = await handler.Handle(new RefreshCommand("presented"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Unauthenticated);
    }
}
