using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.ChangeUserRole;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Auth;

public class ChangeUserRoleCommandHandlerTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static IApplicationDbContext CreateDb(List<ApplicationUser> users)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var mockUsers = users.BuildMockDbSet();
        db.Users.Returns(mockUsers);
        return db;
    }

    [Fact]
    public async Task Handle_PromotingADifferentUser_Succeeds_AndAudits()
    {
        var caller = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", Version = 1 };
        var target = new ApplicationUser { Id = Guid.NewGuid(), Email = "member@example.com", Version = 5 };
        var db = CreateDb(new List<ApplicationUser> { caller, target });

        var userManager = CreateUserManager();
        userManager.GetRolesAsync(target).Returns(new List<string> { "TeamMember" });
        userManager.RemoveFromRoleAsync(target, "TeamMember").Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(target, "ProjectManager").Returns(IdentityResult.Success);

        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new ChangeUserRoleCommandHandler(db, userManager, activityLog);

        var result = await handler.Handle(new ChangeUserRoleCommand(caller.Id, target.Id, "ProjectManager", target.Version), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await userManager.Received(1).RemoveFromRoleAsync(target, "TeamMember");
        await userManager.Received(1).AddToRoleAsync(target, "ProjectManager");
        await activityLog.Received(1).LogAsync(caller.Id, "UserRoleChanged", "User", target.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ChangingOwnRole_Returns409()
    {
        var caller = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", Version = 1 };
        var db = CreateDb(new List<ApplicationUser> { caller });
        var userManager = CreateUserManager();
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new ChangeUserRoleCommandHandler(db, userManager, activityLog);
        var result = await handler.Handle(new ChangeUserRoleCommand(caller.Id, caller.Id, "TeamMember", caller.Version), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Conflict);
    }

    [Fact]
    public async Task Handle_WouldLeaveZeroAdmins_Returns409()
    {
        var caller = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", Version = 1 };
        var lastAdmin = new ApplicationUser { Id = Guid.NewGuid(), Email = "lastadmin@example.com", Version = 3 };
        var db = CreateDb(new List<ApplicationUser> { caller, lastAdmin });

        var userManager = CreateUserManager();
        userManager.GetRolesAsync(lastAdmin).Returns(new List<string> { "Admin" });
        userManager.GetUsersInRoleAsync("Admin").Returns(new List<ApplicationUser> { lastAdmin });

        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new ChangeUserRoleCommandHandler(db, userManager, activityLog);

        var result = await handler.Handle(new ChangeUserRoleCommand(caller.Id, lastAdmin.Id, "ProjectManager", lastAdmin.Version), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Conflict);
        await userManager.DidNotReceive().RemoveFromRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_SameRoleRequested_Returns400()
    {
        var caller = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", Version = 1 };
        var target = new ApplicationUser { Id = Guid.NewGuid(), Email = "pm@example.com", Version = 2 };
        var db = CreateDb(new List<ApplicationUser> { caller, target });

        var userManager = CreateUserManager();
        userManager.GetRolesAsync(target).Returns(new List<string> { "ProjectManager" });

        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new ChangeUserRoleCommandHandler(db, userManager, activityLog);

        var result = await handler.Handle(new ChangeUserRoleCommand(caller.Id, target.Id, "ProjectManager", target.Version), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
    }

    [Fact]
    public async Task Handle_StaleIfMatch_Returns409()
    {
        var caller = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", Version = 1 };
        var target = new ApplicationUser { Id = Guid.NewGuid(), Email = "member@example.com", Version = 9 };
        var db = CreateDb(new List<ApplicationUser> { caller, target });
        var userManager = CreateUserManager();
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new ChangeUserRoleCommandHandler(db, userManager, activityLog);
        var result = await handler.Handle(new ChangeUserRoleCommand(caller.Id, target.Id, "ProjectManager", 1), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Conflict);
    }
}
