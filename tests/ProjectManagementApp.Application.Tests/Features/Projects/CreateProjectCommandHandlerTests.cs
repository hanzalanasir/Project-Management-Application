using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MockQueryable.NSubstitute;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Projects.CreateProject;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Projects;

public class CreateProjectCommandHandlerTests
{
    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static IApplicationDbContext CreateDb(List<ApplicationUser> users, List<Project>? projects = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var userSet = users.BuildMockDbSet();
        db.Users.Returns(userSet);
        var projectSet = (projects ?? new List<Project>()).BuildMockDbSet();
        db.Projects.Returns(projectSet);
        return db;
    }

    private static ICurrentUserService CreateCurrentUser(Guid userId, string role)
    {
        var service = Substitute.For<ICurrentUserService>();
        service.Current.Returns(new CurrentUser(userId, "caller@example.com", role, "Caller"));
        return service;
    }

    private static IOptions<ProjectsOptions> DefaultOptions() => Microsoft.Extensions.Options.Options.Create(new ProjectsOptions());

    [Fact]
    public async Task Handle_ProjectManagerCaller_OwnerIsTakenFromToken_IgnoringAnyBodyValue()
    {
        var pm = new ApplicationUser { Id = Guid.NewGuid(), Email = "pm@example.com", FullName = "PM", IsActive = true };
        var someoneElse = new ApplicationUser { Id = Guid.NewGuid(), Email = "other@example.com", FullName = "Other", IsActive = true };
        var db = CreateDb(new List<ApplicationUser> { pm, someoneElse });
        var userManager = CreateUserManager();
        userManager.GetRolesAsync(pm).Returns(new List<string> { "ProjectManager" });
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new CreateProjectCommandHandler(db, userManager, currentUser, activityLog, DefaultOptions());
        var command = new CreateProjectCommand("Apollo", null, DateOnly.FromDateTime(DateTime.UtcNow), null, null, someoneElse.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Owner.Id.Should().Be(pm.Id);
        await activityLog.Received(1).LogAsync(pm.Id, "ProjectCreated", "Project", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<Guid?>());
    }

    [Fact]
    public async Task Handle_AdminCaller_MaySpecifyAnEligibleOwner()
    {
        var admin = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", FullName = "Admin", IsActive = true };
        var otherPm = new ApplicationUser { Id = Guid.NewGuid(), Email = "otherpm@example.com", FullName = "Other PM", IsActive = true };
        var db = CreateDb(new List<ApplicationUser> { admin, otherPm });
        var userManager = CreateUserManager();
        userManager.GetRolesAsync(otherPm).Returns(new List<string> { "ProjectManager" });
        var currentUser = CreateCurrentUser(admin.Id, "Admin");
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new CreateProjectCommandHandler(db, userManager, currentUser, activityLog, DefaultOptions());
        var command = new CreateProjectCommand("Apollo", null, DateOnly.FromDateTime(DateTime.UtcNow), null, null, otherPm.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Owner.Id.Should().Be(otherPm.Id);
    }

    [Fact]
    public async Task Handle_AdminCaller_OwnerIdOmitted_AdminBecomesOwner()
    {
        var admin = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", FullName = "Admin", IsActive = true };
        var db = CreateDb(new List<ApplicationUser> { admin });
        var userManager = CreateUserManager();
        userManager.GetRolesAsync(admin).Returns(new List<string> { "Admin" });
        var currentUser = CreateCurrentUser(admin.Id, "Admin");
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new CreateProjectCommandHandler(db, userManager, currentUser, activityLog, DefaultOptions());
        var command = new CreateProjectCommand("Apollo", null, DateOnly.FromDateTime(DateTime.UtcNow), null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Owner.Id.Should().Be(admin.Id);
    }

    [Fact]
    public async Task Handle_AdminCaller_SpecifiesATeamMemberAsOwner_ReturnsValidationError()
    {
        var admin = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", FullName = "Admin", IsActive = true };
        var teamMember = new ApplicationUser { Id = Guid.NewGuid(), Email = "tm@example.com", FullName = "TM", IsActive = true };
        var db = CreateDb(new List<ApplicationUser> { admin, teamMember });
        var userManager = CreateUserManager();
        userManager.GetRolesAsync(teamMember).Returns(new List<string> { "TeamMember" });
        var currentUser = CreateCurrentUser(admin.Id, "Admin");
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new CreateProjectCommandHandler(db, userManager, currentUser, activityLog, DefaultOptions());
        var command = new CreateProjectCommand("Apollo", null, DateOnly.FromDateTime(DateTime.UtcNow), null, null, teamMember.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
        await activityLog.DidNotReceive().LogAsync(Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<Guid?>());
    }

    [Fact]
    public async Task Handle_AdminCaller_SpecifiesANonExistentOwner_ReturnsValidationError()
    {
        var admin = new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com", FullName = "Admin", IsActive = true };
        var db = CreateDb(new List<ApplicationUser> { admin });
        var userManager = CreateUserManager();
        var currentUser = CreateCurrentUser(admin.Id, "Admin");
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new CreateProjectCommandHandler(db, userManager, currentUser, activityLog, DefaultOptions());
        var command = new CreateProjectCommand("Apollo", null, DateOnly.FromDateTime(DateTime.UtcNow), null, null, Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
    }

    [Fact]
    public async Task Handle_StatusOmitted_DefaultsToConfiguredDefault()
    {
        var pm = new ApplicationUser { Id = Guid.NewGuid(), Email = "pm2@example.com", FullName = "PM", IsActive = true };
        var db = CreateDb(new List<ApplicationUser> { pm });
        var userManager = CreateUserManager();
        userManager.GetRolesAsync(pm).Returns(new List<string> { "ProjectManager" });
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new CreateProjectCommandHandler(db, userManager, currentUser, activityLog, DefaultOptions());
        var command = new CreateProjectCommand("Apollo", null, DateOnly.FromDateTime(DateTime.UtcNow), null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Planning");
    }
}
