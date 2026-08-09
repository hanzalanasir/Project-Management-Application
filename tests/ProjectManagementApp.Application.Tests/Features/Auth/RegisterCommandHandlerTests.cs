using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.Register;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesUser_AssignsTeamMemberRole_AndAudits()
    {
        var userManager = CreateUserManager();
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "TeamMember").Returns(IdentityResult.Success);

        var activityLog = Substitute.For<IActivityLogService>();
        var db = Substitute.For<IApplicationDbContext>();

        var handler = new RegisterCommandHandler(userManager, activityLog, db);
        var command = new RegisterCommand("Dana Rivera", "dana@example.com", "S3cure-P@ss!", "S3cure-P@ss!");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Role.Should().Be("TeamMember");
        result.Value!.Email.Should().Be("dana@example.com");

        await userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "TeamMember");
        await activityLog.Received(1).LogAsync(
            actorId: null, action: "UserRegistered", entityType: "User",
            entityId: Arg.Any<string>(), changeSummary: Arg.Any<string>(), Arg.Any<CancellationToken>());
        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsConflict()
    {
        var userManager = CreateUserManager();
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "DuplicateUserName", Description = "Email already registered." }));

        var activityLog = Substitute.For<IActivityLogService>();
        var db = Substitute.For<IApplicationDbContext>();

        var handler = new RegisterCommandHandler(userManager, activityLog, db);
        var command = new RegisterCommand("Dana Rivera", "dana@example.com", "S3cure-P@ss!", "S3cure-P@ss!");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Conflict);
        await userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_OtherCreateFailure_ReturnsValidationError()
    {
        var userManager = CreateUserManager();
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "PasswordTooShort", Description = "Password too short." }));

        var activityLog = Substitute.For<IActivityLogService>();
        var db = Substitute.For<IApplicationDbContext>();

        var handler = new RegisterCommandHandler(userManager, activityLog, db);
        var command = new RegisterCommand("Dana Rivera", "dana@example.com", "weak", "weak");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
    }
}
