using FluentAssertions;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Tasks.UpdateTaskStatus;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;

namespace ProjectManagementApp.Application.Tests.Features.Tasks;

[Collection(PostgresCollection.Name)]
public class UpdateTaskStatusCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public UpdateTaskStatusCommandHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Handle_UnknownId_ReturnsNotFound()
    {
        await using var db = _fixture.CreateDbContext();
        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(Guid.NewGuid(), "caller@example.com", "Admin", "Caller"));
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new UpdateTaskStatusCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new UpdateTaskStatusCommand(Guid.NewGuid(), "Done", 0), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_TeamMemberNotAssignee_ReturnsForbidden()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm@status-handler-test.com").Build();
        var assignee = new ApplicationUserBuilder().WithEmail("assignee@status-handler-test.com").Build();
        var outsider = new ApplicationUserBuilder().WithEmail("outsider@status-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskBuilder().WithProject(project).WithAssignee(assignee).Build();
        db.Users.AddRange(pm, assignee, outsider);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(outsider.Id, outsider.Email!, "TeamMember", outsider.FullName));
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new UpdateTaskStatusCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new UpdateTaskStatusCommand(task.Id, "InProgress", task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
    }

    [Fact]
    public async Task Handle_AssigneeChangesStatus_AuditsFromToTransition_NoSeparateClosedAtEvent()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm2@status-handler-test.com").Build();
        var assignee = new ApplicationUserBuilder().WithEmail("assignee2@status-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskBuilder().WithProject(project).WithAssignee(assignee).WithStatus(ProjectManagementApp.Domain.Enums.TaskStatus.ToDo).Build();
        db.Users.AddRange(pm, assignee);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(assignee.Id, assignee.Email!, "TeamMember", assignee.FullName));
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new UpdateTaskStatusCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new UpdateTaskStatusCommand(task.Id, "Done", task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Done");
        result.Value!.ClosedAt.Should().NotBeNull();

        // Exactly one audit call, TaskStatusChanged, recording the from -> to transition and the
        // closed_at effect in its summary — no separate audit action for the closed_at side effect
        // (spec B.7).
        await activityLog.Received(1).LogAsync(
            assignee.Id, "TaskStatusChanged", "Task", task.Id.ToString(),
            Arg.Is<string>(s => s!.Contains("ToDo") && s.Contains("Done")),
            Arg.Any<CancellationToken>(), task.ProjectId);
    }

    [Fact]
    public async Task Handle_InvalidStatusValue_ReturnsValidationError()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm3@status-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskBuilder().WithProject(project).Build();
        db.Users.Add(pm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(pm.Id, pm.Email!, "ProjectManager", pm.FullName));
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new UpdateTaskStatusCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new UpdateTaskStatusCommand(task.Id, "NotARealStatus", task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
    }
}
