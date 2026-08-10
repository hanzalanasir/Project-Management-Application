using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Tasks.DeleteTask;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using NSubstitute;

namespace ProjectManagementApp.Application.Tests.Features.Tasks;

// Uses the real TaskAccessPolicy against real Postgres — CanMutateAsync(Delete) is the same
// mutation-kind gate every other write goes through (belt-and-braces, spec T.2).
[Collection(PostgresCollection.Name)]
public class DeleteTaskCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public DeleteTaskCommandHandlerTests(PostgresFixture fixture)
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

        var handler = new DeleteTaskCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new DeleteTaskCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_TeamMemberAssignee_ReturnsForbidden_AndTaskSurvives()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm@delete-task-handler-test.com").Build();
        var assignee = new ApplicationUserBuilder().WithEmail("assignee@delete-task-handler-test.com").Build();
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

        var handler = new DeleteTaskCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new DeleteTaskCommand(task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
        (await db.Tasks.AnyAsync(t => t.Id == task.Id)).Should().BeTrue();
        await activityLog.DidNotReceive().LogAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidDelete_AuditsBeforeRemoval_AndTaskIsGone()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm2@delete-task-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskBuilder().WithProject(project).WithTitle("Doomed Task").Build();
        db.Users.Add(pm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(pm.Id, pm.Email!, "ProjectManager", pm.FullName));
        var activityLog = Substitute.For<IActivityLogService>();
        // Asserting call ORDER: LogAsync must fire while the task is still tracked/present —
        // the same ordering precedent as DeleteProjectCommandHandlerTests (002).
        var taskStillPresentWhenLogged = false;
        activityLog
            .When(x => x.LogAsync(Arg.Any<Guid?>(), "TaskDeleted", "Task", task.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>()))
            .Do(_ => taskStillPresentWhenLogged = db.Tasks.Local.Any(t => t.Id == task.Id));

        var handler = new DeleteTaskCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new DeleteTaskCommand(task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        taskStillPresentWhenLogged.Should().BeTrue();
        await activityLog.Received(1).LogAsync(pm.Id, "TaskDeleted", "Task", task.Id.ToString(), Arg.Is<string>(s => s!.Contains("Doomed Task")), Arg.Any<CancellationToken>());
        (await db.Tasks.AnyAsync(t => t.Id == task.Id)).Should().BeFalse();
    }
}
