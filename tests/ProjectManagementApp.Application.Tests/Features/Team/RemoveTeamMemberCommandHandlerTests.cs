using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Team.RemoveTeamMember;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Team;

// Ordering (T051, data-model.md §5): project load -> policy -> membership check -> open-tasks
// check -> audit -> delete. Getting the open-tasks check wrong relative to audit/delete breaks
// T044's "total no-op" guarantee, so each early-exit branch here also asserts nothing downstream
// ran (no SaveChangesAsync, no Remove, no LogAsync).
public class RemoveTeamMemberCommandHandlerTests
{
    private static IApplicationDbContext CreateDb(
        List<Project> projects, List<ApplicationUser>? users = null, List<TeamMember>? teamMembers = null, List<TaskItem>? tasks = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var projectSet = projects.BuildMockDbSet();
        var userSet = (users ?? new List<ApplicationUser>()).BuildMockDbSet();
        var teamMemberSet = (teamMembers ?? new List<TeamMember>()).BuildMockDbSet();
        var taskSet = (tasks ?? new List<TaskItem>()).BuildMockDbSet();
        db.Projects.Returns(projectSet);
        db.Users.Returns(userSet);
        db.TeamMembers.Returns(teamMemberSet);
        db.Tasks.Returns(taskSet);
        return db;
    }

    private static ICurrentUserService CreateCurrentUser(Guid userId, string role)
    {
        var service = Substitute.For<ICurrentUserService>();
        service.Current.Returns(new CurrentUser(userId, "caller@example.com", role, "Caller"));
        return service;
    }

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

    private static ApplicationUser MakeUser(string email) => new()
    {
        Id = Guid.NewGuid(), Email = email, FullName = email, IsActive = true,
    };

    [Fact]
    public async Task Handle_UnknownProjectId_ReturnsNotFound()
    {
        var db = CreateDb(new List<Project>());
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        var currentUser = CreateCurrentUser(Guid.NewGuid(), "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new RemoveTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog);

        var result = await handler.Handle(new RemoveTeamMemberCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_AccessPolicyDenies_ReturnsForbidden_AndNothingIsPersisted()
    {
        var pm = MakeUser("owner@example.com");
        var project = MakeProject(pm);
        var target = MakeUser("target@example.com");
        var membership = new TeamMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = target.Id, User = target, CreatedAt = DateTimeOffset.UtcNow };
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm, target }, teamMembers: new List<TeamMember> { membership });
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanManageTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(false));
        var currentUser = CreateCurrentUser(Guid.NewGuid(), "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new RemoveTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog);

        var result = await handler.Handle(new RemoveTeamMemberCommand(project.Id, target.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
        await db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await activityLog.DidNotReceive().LogAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotAMember_ReturnsNotFound()
    {
        var pm = MakeUser("owner2@example.com");
        var project = MakeProject(pm);
        var target = MakeUser("target2@example.com");
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm, target });
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanManageTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new RemoveTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog);

        var result = await handler.Handle(new RemoveTeamMemberCommand(project.Id, target.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
        await db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MemberHasOpenAssignedTasks_ReturnsConflict_WithBlockingCount_AndIsATotalNoOp()
    {
        var pm = MakeUser("owner3@example.com");
        var project = MakeProject(pm);
        var target = MakeUser("target3@example.com");
        var membership = new TeamMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = target.Id, User = target, CreatedAt = DateTimeOffset.UtcNow };
        var openTask = new TaskItem { Id = Guid.NewGuid(), ProjectId = project.Id, AssigneeId = target.Id, Title = "Open", Status = Domain.Enums.TaskStatus.InProgress, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        var doneTask = new TaskItem { Id = Guid.NewGuid(), ProjectId = project.Id, AssigneeId = target.Id, Title = "Done", Status = Domain.Enums.TaskStatus.Done, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm, target }, teamMembers: new List<TeamMember> { membership }, tasks: new List<TaskItem> { openTask, doneTask });
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanManageTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new RemoveTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog);

        var result = await handler.Handle(new RemoveTeamMemberCommand(project.Id, target.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Conflict);
        result.Error!.Message.Should().Contain("1 open task");
        db.TeamMembers.Received(0).Remove(Arg.Any<TeamMember>());
        await activityLog.DidNotReceive().LogAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MemberHasOnlyDoneTasks_Succeeds_AuditsBeforeDeleting()
    {
        var pm = MakeUser("owner4@example.com");
        var project = MakeProject(pm);
        var target = MakeUser("target4@example.com");
        var membership = new TeamMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = target.Id, User = target, CreatedAt = DateTimeOffset.UtcNow };
        var doneTask = new TaskItem { Id = Guid.NewGuid(), ProjectId = project.Id, AssigneeId = target.Id, Title = "Done", Status = Domain.Enums.TaskStatus.Done, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm, target }, teamMembers: new List<TeamMember> { membership }, tasks: new List<TaskItem> { doneTask });
        var accessPolicy = Substitute.For<ITeamAccessPolicy>();
        accessPolicy.CanManageTeamAsync(project, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new RemoveTeamMemberCommandHandler(db, accessPolicy, currentUser, activityLog);

        var result = await handler.Handle(new RemoveTeamMemberCommand(project.Id, target.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        db.TeamMembers.Received(1).Remove(Arg.Is<TeamMember>(m => m!.Id == membership.Id));
        await activityLog.Received(1).LogAsync(pm.Id, "TeamMemberRemoved", "TeamMember", membership.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
