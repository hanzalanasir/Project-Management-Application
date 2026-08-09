using FluentAssertions;
using Microsoft.Extensions.Options;
using MockQueryable.NSubstitute;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Tasks.Common;
using ProjectManagementApp.Application.Features.Tasks.CreateTask;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Tests.Features.Tasks;

public class CreateTaskCommandHandlerTests
{
    private static IApplicationDbContext CreateDb(
        List<Project> projects, List<TaskItem>? tasks = null, List<ApplicationUser>? users = null, List<TeamMember>? teamMembers = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var projectSet = projects.BuildMockDbSet();
        var taskSet = (tasks ?? new List<TaskItem>()).BuildMockDbSet();
        var userSet = (users ?? new List<ApplicationUser>()).BuildMockDbSet();
        var teamMemberSet = (teamMembers ?? new List<TeamMember>()).BuildMockDbSet();
        db.Projects.Returns(projectSet);
        db.Tasks.Returns(taskSet);
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

    private static IOptions<TasksOptions> DefaultOptions() => Options.Create(new TasksOptions());

    private static Project MakeProject(ApplicationUser owner, DateOnly? startDate = null, DateOnly? endDate = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Project",
        StartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
        EndDate = endDate,
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
        var accessPolicy = Substitute.For<ITaskAccessPolicy>();
        var currentUser = CreateCurrentUser(Guid.NewGuid(), "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new CreateTaskCommandHandler(db, accessPolicy, currentUser, activityLog, new AssigneeValidator(db), DefaultOptions());

        var result = await handler.Handle(new CreateTaskCommand(Guid.NewGuid(), "Title", null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_AccessPolicyDenies_ReturnsForbidden_AndNothingIsPersisted()
    {
        var pm = MakeUser("owner@example.com");
        var project = MakeProject(pm);
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm });
        var accessPolicy = Substitute.For<ITaskAccessPolicy>();
        accessPolicy.CanMutateAsync(Arg.Any<TaskItem>(), TaskMutation.Create, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(false));
        var currentUser = CreateCurrentUser(Guid.NewGuid(), "TeamMember");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new CreateTaskCommandHandler(db, accessPolicy, currentUser, activityLog, new AssigneeValidator(db), DefaultOptions());

        var result = await handler.Handle(new CreateTaskCommand(project.Id, "Title", null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
        await db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AssigneeNotInProjectPool_ReturnsValidationError_OnAssigneeIdField()
    {
        var pm = MakeUser("owner2@example.com");
        var outsider = MakeUser("outsider@example.com");
        var project = MakeProject(pm);
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm, outsider });
        var accessPolicy = Substitute.For<ITaskAccessPolicy>();
        accessPolicy.CanMutateAsync(Arg.Any<TaskItem>(), TaskMutation.Create, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new CreateTaskCommandHandler(db, accessPolicy, currentUser, activityLog, new AssigneeValidator(db), DefaultOptions());

        var result = await handler.Handle(new CreateTaskCommand(project.Id, "Title", null, null, null, outsider.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
        result.Error!.Fields.Should().ContainKey("assigneeId");
    }

    [Fact]
    public async Task Handle_DueDateOutsideProjectWindow_ReturnsValidationError_OnDueDateField()
    {
        var pm = MakeUser("owner3@example.com");
        var project = MakeProject(pm, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10));
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm });
        var accessPolicy = Substitute.For<ITaskAccessPolicy>();
        accessPolicy.CanMutateAsync(Arg.Any<TaskItem>(), TaskMutation.Create, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new CreateTaskCommandHandler(db, accessPolicy, currentUser, activityLog, new AssigneeValidator(db), DefaultOptions());

        var farDueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(100);
        var result = await handler.Handle(new CreateTaskCommand(project.Id, "Title", null, null, farDueDate, null), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
        result.Error!.Fields.Should().ContainKey("dueDate");
    }

    [Fact]
    public async Task Handle_ValidRequest_PersistsWithConfiguredDefaults_AndWritesTaskCreatedAudit()
    {
        var pm = MakeUser("owner4@example.com");
        var project = MakeProject(pm);
        var db = CreateDb(new List<Project> { project }, users: new List<ApplicationUser> { pm });
        var accessPolicy = Substitute.For<ITaskAccessPolicy>();
        accessPolicy.CanMutateAsync(Arg.Any<TaskItem>(), TaskMutation.Create, Arg.Any<CurrentUser>(), Arg.Any<CancellationToken>())
            .Returns(new AccessDecision(true));
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");
        var activityLog = Substitute.For<IActivityLogService>();
        var handler = new CreateTaskCommandHandler(db, accessPolicy, currentUser, activityLog, new AssigneeValidator(db), DefaultOptions());

        var result = await handler.Handle(new CreateTaskCommand(project.Id, "Draft rollout checklist", null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Draft rollout checklist");
        result.Value!.Status.Should().Be("ToDo");
        result.Value!.Priority.Should().Be("Medium");
        await activityLog.Received(1).LogAsync(pm.Id, "TaskCreated", "Task", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
