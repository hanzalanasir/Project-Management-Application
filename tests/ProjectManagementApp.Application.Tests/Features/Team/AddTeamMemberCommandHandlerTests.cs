using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MockQueryable.NSubstitute;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Team.AddTeamMember;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Tests.Features.Team;

public class AddTeamMemberCommandHandlerTests
{
    private static IApplicationDbContext CreateDb(
        List<Project> projects, List<ApplicationUser>? users = null, List<TeamMember>? teamMembers = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var projectSet = projects.BuildMockDbSet();
        var userSet = (users ?? new List<ApplicationUser>()).BuildMockDbSet();
        var teamMemberSet = (teamMembers ?? new List<TeamMember>()).BuildMockDbSet();
        db.Projects.Returns(projectSet);
        db.Users.Returns(userSet);
        db.TeamMembers.Returns(teamMemberSet);
        return db;
    }

    private static ICurrentUserService CreateCurrentUser(Guid userId, string role)
    {
        var service = Substitute.For<ICurrentUserService>();
        service.Current.Returns(new CurrentUser(userId, "caller@example.com", role, "Caller"));
        return service;
    }

    private static UserManager<ApplicationUser> CreateUserManager(ApplicationUser user, string role)
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userManager = Substitute.For<UserManager<ApplicationUser>>(store, null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.GetRolesAsync(user).Returns(new List<string> { role });
        return userManager;
    }

    private static IOptions<TeamOptions> DefaultOptions() => Options.Create(new TeamOptions());

    private static Project MakeProject(ApplicationUser owner) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Project",
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Owner = owner,
        OwnerId = owner.Id,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    private static ApplicationUser MakeUser(string email, bool isActive = true) => new()
    {
        Id = Guid.NewGuid(), Email = email, FullName = email, IsActive = isActive,
    };

    [Fact]
    public async Task Handle_UnknownProjectId_ReturnsNotFound()
    {
        var db = CreateDb(new List<Project>());
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        var currentUser = CreateCurrentUser(Guid.NewGuid(), "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new AddTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog, CreateUserManager(MakeUser("x@example.com"), "TeamMember"), DefaultOptions());

        var result = await handler.Handle(new AddTeamMemberCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_AccessPolicyDenies_ReturnsForbidden_AndNothingIsPersisted()
    {
        var pm = MakeUser("owner@example.com");
        var project = MakeProject(pm);
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm });
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanManageTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(false));
        var currentUser = CreateCurrentUser(Guid.NewGuid(), "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new AddTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog, CreateUserManager(pm, "ProjectManager"), DefaultOptions());

        var result = await handler.Handle(new AddTeamMemberCommand(project.Id, pm.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
        await db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownUserId_ReturnsNotFound()
    {
        var pm = MakeUser("owner2@example.com");
        var project = MakeProject(pm);
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm });
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanManageTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new AddTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog, CreateUserManager(pm, "ProjectManager"), DefaultOptions());

        var result = await handler.Handle(new AddTeamMemberCommand(project.Id, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_DeactivatedTargetUser_ReturnsValidationError_OnUserIdField()
    {
        var pm = MakeUser("owner3@example.com");
        var inactive = MakeUser("inactive@example.com", isActive: false);
        var project = MakeProject(pm);
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm, inactive });
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanManageTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new AddTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog, CreateUserManager(pm, "ProjectManager"), DefaultOptions());

        var result = await handler.Handle(new AddTeamMemberCommand(project.Id, inactive.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
        result.Error!.Fields.Should().ContainKey("userId");
    }

    [Fact]
    public async Task Handle_AlreadyAMember_ReturnsConflict_PreCheck()
    {
        var pm = MakeUser("owner4@example.com");
        var target = MakeUser("target@example.com");
        var project = MakeProject(pm);
        var existingMembership = new TeamMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = target.Id, CreatedAt = DateTimeOffset.UtcNow };
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm, target }, teamMembers: new List<TeamMember> { existingMembership });
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanManageTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new AddTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog, CreateUserManager(pm, "ProjectManager"), DefaultOptions());

        var result = await handler.Handle(new AddTeamMemberCommand(project.Id, target.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Conflict);
        await db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidRequest_Persists_SetsAddedBy_AndWritesTeamMemberAddedAudit()
    {
        var pm = MakeUser("owner5@example.com");
        var target = MakeUser("target2@example.com");
        var project = MakeProject(pm);
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm, target });
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanManageTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new AddTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog, CreateUserManager(target, "TeamMember"), DefaultOptions());

        var result = await handler.Handle(new AddTeamMemberCommand(project.Id, target.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.UserId.Should().Be(target.Id);
        result.Value!.Role.Should().Be("TeamMember");
        db.TeamMembers.Received(1).Add(Arg.Is<TeamMember>(m => m!.ProjectId == project.Id && m.UserId == target.Id && m.AddedBy == pm.Id));
        await activityLog.Received(1).LogAsync(pm.Id, "TeamMemberAdded", "TeamMember", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>(), project.Id);
        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
