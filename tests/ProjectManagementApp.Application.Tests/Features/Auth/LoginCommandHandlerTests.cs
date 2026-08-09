using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.Login;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static (UserManager<ApplicationUser> userManager, ITokenService tokenService, IActivityLogService activityLog, IApplicationDbContext db)
        CreateDependencies() =>
        (CreateUserManager(), Substitute.For<ITokenService>(), Substitute.For<IActivityLogService>(), Substitute.For<IApplicationDbContext>());

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthTokens_AndAudits()
    {
        var (userManager, tokenService, activityLog, db) = CreateDependencies();
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "dana@example.com", UserName = "dana@example.com", IsActive = true, FullName = "Dana Rivera", CreatedAt = DateTimeOffset.UtcNow };

        userManager.FindByEmailAsync("dana@example.com").Returns(user);
        userManager.CheckPasswordAsync(user, "correct-password").Returns(true);
        userManager.GetRolesAsync(user).Returns(new List<string> { "TeamMember" });
        tokenService.CreateAccessToken(user, "TeamMember").Returns("fake-jwt");
        tokenService.CreateRefreshToken().Returns("fake-refresh-token");

        var handler = new LoginCommandHandler(userManager, tokenService, activityLog, db);
        var result = await handler.Handle(new LoginCommand("dana@example.com", "correct-password"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("fake-jwt");
        result.Value!.RefreshToken.Should().Be("fake-refresh-token");
        result.Value!.User.Role.Should().Be("TeamMember");

        await activityLog.Received(1).LogAsync(user.Id, "UserLoggedIn", "User", user.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsGenericUnauthenticated()
    {
        var (userManager, tokenService, activityLog, db) = CreateDependencies();
        userManager.FindByEmailAsync("nobody@example.com").Returns((ApplicationUser?)null);

        var handler = new LoginCommandHandler(userManager, tokenService, activityLog, db);
        var result = await handler.Handle(new LoginCommand("nobody@example.com", "whatever"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Unauthenticated);
        result.Error!.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsIdenticalGenericError_AsUnknownEmail()
    {
        var (userManager, tokenService, activityLog, db) = CreateDependencies();
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "dana@example.com", IsActive = true };
        userManager.FindByEmailAsync("dana@example.com").Returns(user);
        userManager.CheckPasswordAsync(user, "wrong-password").Returns(false);

        var handler = new LoginCommandHandler(userManager, tokenService, activityLog, db);
        var result = await handler.Handle(new LoginCommand("dana@example.com", "wrong-password"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Unauthenticated);
        result.Error!.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Handle_DeactivatedUser_ReturnsUnauthenticated_EvenWithCorrectPassword()
    {
        var (userManager, tokenService, activityLog, db) = CreateDependencies();
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "dana@example.com", IsActive = false };
        userManager.FindByEmailAsync("dana@example.com").Returns(user);
        userManager.CheckPasswordAsync(user, "correct-password").Returns(true);

        var handler = new LoginCommandHandler(userManager, tokenService, activityLog, db);
        var result = await handler.Handle(new LoginCommand("dana@example.com", "correct-password"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Unauthenticated);
    }
}
