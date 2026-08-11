using FluentAssertions;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Tasks.UpdateTask;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using NSubstitute;

namespace ProjectManagementApp.Application.Tests.Features.Tasks;

// Uses the real TaskAccessPolicy (not a mock) against real Postgres — CanMutateAsync(FullEdit) is
// the graduated model's core, and its exact denial message matters for quickstart V2.
[Collection(PostgresCollection.Name)]
public class UpdateTaskCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public UpdateTaskCommandHandlerTests(PostgresFixture fixture)
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

        var handler = new UpdateTaskCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new UpdateTaskCommand(Guid.NewGuid(), "Title", null, "Medium", null, 0), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_TeamMemberAssignee_ReturnsForbidden_WithTheNarrowerRightMessage()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm@update-task-handler-test.com").Build();
        var assignee = new ApplicationUserBuilder().WithEmail("assignee@update-task-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskBuilder().WithProject(project).WithAssignee(assignee).Build();
        db.Users.AddRange(pm, assignee);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(assignee.Id, assignee.Email!, "TeamMember", assignee.FullName));
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new UpdateTaskCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new UpdateTaskCommand(task.Id, "Renamed", null, "Low", null, task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
        result.Error!.Message.Should().Be("You may update the status of this task, but not its details.");
    }

    [Fact]
    public async Task Handle_DueDateOutsideRevalidatedWindow_ReturnsValidationError()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm2@update-task-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm)
            .WithStartDate(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithEndDate(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10))
            .Build();
        var task = new TaskBuilder().WithProject(project).Build();
        db.Users.Add(pm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(pm.Id, pm.Email!, "ProjectManager", pm.FullName));
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new UpdateTaskCommandHandler(db, accessPolicy, currentUser, activityLog);
        var farDueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(100);
        var result = await handler.Handle(new UpdateTaskCommand(task.Id, "Title", null, "Medium", farDueDate, task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Validation);
        result.Error!.Fields.Should().ContainKey("dueDate");
    }

    [Fact]
    public async Task Handle_ValidEdit_UpdatesFields_AndAuditsChangedFieldSummary()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm3@update-task-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskBuilder().WithProject(project).WithTitle("Old Title").Build();
        db.Users.Add(pm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(pm.Id, pm.Email!, "ProjectManager", pm.FullName));
        var activityLog = Substitute.For<IActivityLogService>();

        var handler = new UpdateTaskCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new UpdateTaskCommand(task.Id, "New Title", null, "High", null, task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("New Title");
        result.Value!.Priority.Should().Be("High");
        await activityLog.Received(1).LogAsync(pm.Id, "TaskUpdated", "Task", task.Id.ToString(), Arg.Is<string>(s => s!.Contains("title") && s.Contains("priority")), Arg.Any<CancellationToken>(), task.ProjectId);
    }
}
