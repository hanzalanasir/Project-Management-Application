using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.ChangeUserStatus;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Auth;

public class ChangeUserStatusCommandHandlerTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static (IApplicationDbContext db, UserManager<ApplicationUser> userManager, IActivityLogService activityLog) CreateDependencies(
        List<ApplicationUser> users, List<RefreshToken>? tokens = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var mockUsers = users.BuildMockDbSet();
        db.Users.Returns(mockUsers);
        var mockTokens = (tokens ?? new List<RefreshToken>()).BuildMockDbSet();
        db.RefreshTokens.Returns(mockTokens);
        var userManager = CreateUserManager();
        foreach (var user in users)
        {
            userManager.GetRolesAsync(user).Returns(new List<string> { "TeamMember" });
        }
        return (db, userManager, Substitute.For<IActivityLogService>());
    }

    [Fact]
    public async Task Handle_DeactivatingADifferentActiveUser_Succeeds_RevokesTokens_AndAudits()
    {
        var caller = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", IsActive = true, Version = 1 };
        var target = new ApplicationUser { Id = Guid.NewGuid(), Email = "member@example.com", IsActive = true, Version = 5 };
        var liveToken = new RefreshToken { Id = Guid.NewGuid(), UserId = target.Id, TokenHash = "h1", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), CreatedAt = DateTimeOffset.UtcNow };
        var (db, userManager, activityLog) = CreateDependencies(new List<ApplicationUser> { caller, target }, new List<RefreshToken> { liveToken });

        var handler = new ChangeUserStatusCommandHandler(db, userManager, activityLog);
        var result = await handler.Handle(new ChangeUserStatusCommand(caller.Id, target.Id, false, target.Version), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        target.IsActive.Should().BeFalse();
        liveToken.RevokedAt.Should().NotBeNull();
        await activityLog.Received(1).LogAsync(caller.Id, "UserDeactivated", "User", target.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DeactivatingSelf_Returns409()
    {
        var caller = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", IsActive = true, Version = 1 };
        var (db, userManager, activityLog) = CreateDependencies(new List<ApplicationUser> { caller });

        var handler = new ChangeUserStatusCommandHandler(db, userManager, activityLog);
        var result = await handler.Handle(new ChangeUserStatusCommand(caller.Id, caller.Id, false, caller.Version), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Conflict);
    }

    [Fact]
    public async Task Handle_Reactivating_Succeeds_AuditsReactivated_AndDoesNotTouchTokens()
    {
        var caller = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", IsActive = true, Version = 1 };
        var target = new ApplicationUser { Id = Guid.NewGuid(), Email = "member@example.com", IsActive = false, Version = 5 };
        var revokedToken = new RefreshToken { Id = Guid.NewGuid(), UserId = target.Id, TokenHash = "h1", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), CreatedAt = DateTimeOffset.UtcNow, RevokedAt = DateTimeOffset.UtcNow.AddHours(-1) };
        var (db, userManager, activityLog) = CreateDependencies(new List<ApplicationUser> { caller, target }, new List<RefreshToken> { revokedToken });

        var handler = new ChangeUserStatusCommandHandler(db, userManager, activityLog);
        var result = await handler.Handle(new ChangeUserStatusCommand(caller.Id, target.Id, true, target.Version), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        target.IsActive.Should().BeTrue();
        var revokedAtBefore = revokedToken.RevokedAt;
        revokedToken.RevokedAt.Should().Be(revokedAtBefore, "reactivation must not restore a previously-revoked token");
        await activityLog.Received(1).LogAsync(caller.Id, "UserReactivated", "User", target.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SameStatusRequested_Returns400()
    {
        var caller = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", IsActive = true, Version = 1 };
        var target = new ApplicationUser { Id = Guid.NewGuid(), Email = "member@example.com", IsActive = true, Version = 5 };
        var (db, userManager, activityLog) = CreateDependencies(new List<ApplicationUser> { caller, target });

        var handler = new ChangeUserStatusCommandHandler(db, userManager, activityLog);
        var result = await handler.Handle(new ChangeUserStatusCommand(caller.Id, target.Id, true, target.Version), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
    }
}
