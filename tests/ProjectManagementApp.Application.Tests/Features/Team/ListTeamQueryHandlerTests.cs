using FluentAssertions;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MockQueryable.NSubstitute;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Team.ListTeam;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Team;

public class ListTeamQueryHandlerTests
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

    private static UserManager<ApplicationUser> CreateUserManager(params (ApplicationUser User, string Role)[] usersInRole)
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userManager = Substitute.For<UserManager<ApplicationUser>>(store, null!, null!, null!, null!, null!, null!, null!, null!);
        foreach (var role in usersInRole.Select(u => u.Role).Distinct())
        {
            var inRole = usersInRole.Where(u => u.Role == role).Select(u => u.User).ToList();
            userManager.GetUsersInRoleAsync(role).Returns(inRole);
        }

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
        var currentUser = CreateCurrentUser(Guid.NewGuid(), "Admin");
        var handler = new ListTeamQueryHandler(db, accessPolicy, currentUser, CreateUserManager(), DefaultOptions());

        var result = await handler.Handle(new ListTeamQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_AccessPolicyDenies_ReturnsForbidden_BeforeAnyRowIsRead()
    {
        var owner = MakeUser("owner@example.com");
        var project = MakeProject(owner);
        var member = MakeUser("member@example.com");
        var teamMembers = new List<TeamMember>
        {
            new() { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = member.Id, User = member, CreatedAt = DateTimeOffset.UtcNow },
        };
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { owner, member }, teamMembers: teamMembers);
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanViewTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(false));
        var currentUser = CreateCurrentUser(Guid.NewGuid(), "TeamMember");
        var handler = new ListTeamQueryHandler(db, accessPolicy, currentUser, CreateUserManager(), DefaultOptions());

        var result = await handler.Handle(new ListTeamQuery(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
    }

    [Fact]
    public async Task Handle_Allowed_ReturnsFullRoster_WithGlobalRoles_SingleBoundedRead()
    {
        var owner = MakeUser("owner2@example.com");
        var project = MakeProject(owner);
        var tm = MakeUser("tm@example.com");
        var pm2 = MakeUser("pm2@example.com");
        var teamMembers = new List<TeamMember>
        {
            new() { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = tm.Id, User = tm, CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5) },
            new() { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = pm2.Id, User = pm2, CreatedAt = DateTimeOffset.UtcNow },
        };
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { owner, tm, pm2 }, teamMembers: teamMembers);
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanViewTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(owner.Id, "ProjectManager");
        var userManager = CreateUserManager((tm, "TeamMember"), (pm2, "ProjectManager"));
        var handler = new ListTeamQueryHandler(db, accessPolicy, currentUser, userManager, DefaultOptions());

        var result = await handler.Handle(new ListTeamQuery(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value!.Single(m => m.UserId == tm.Id).Role.Should().Be("TeamMember");
        result.Value!.Single(m => m.UserId == pm2.Id).Role.Should().Be("ProjectManager");
        // Role lookup is batched — exactly one GetUsersInRoleAsync call per Role enum value
        // (3 total, per T014's BuildRoleLookupAsync), never one per roster row.
        await userManager.Received(3).GetUsersInRoleAsync(Arg.Any<string>());
    }
}
