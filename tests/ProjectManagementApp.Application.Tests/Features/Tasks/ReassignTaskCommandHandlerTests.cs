using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Tasks.Common;
using ProjectManagementApp.Application.Features.Tasks.ReassignTask;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Tasks;

// Uses the real TaskAccessPolicy and AssigneeValidator against real Postgres — CanMutateAsync(Reassign)
// is the graduated model's other belt-and-braces gate: TeamMember is denied even for their own task.
[Collection(PostgresCollection.Name)]
public class ReassignTaskCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public ReassignTaskCommandHandlerTests(PostgresFixture fixture)
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
        var assigneeValidator = new AssigneeValidator(db);

        var handler = new ReassignTaskCommandHandler(db, accessPolicy, currentUser, activityLog, assigneeValidator);
        var result = await handler.Handle(new ReassignTaskCommand(Guid.NewGuid(), null, 0), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_NoOpReassignmentToCurrentAssignee_StillAudits()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm@reassign-handler-test.com").Build();
        var assignee = new ApplicationUserBuilder().WithEmail("assignee@reassign-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskBuilder().WithProject(project).WithAssignee(assignee).WithTitle("No-op Task").Build();
        db.Users.AddRange(pm, assignee);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        db.TeamMembers.Add(new TeamMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = assignee.Id, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(pm.Id, pm.Email!, "ProjectManager", pm.FullName));
        var activityLog = Substitute.For<IActivityLogService>();
        var assigneeValidator = new AssigneeValidator(db);

        var handler = new ReassignTaskCommandHandler(db, accessPolicy, currentUser, activityLog, assigneeValidator);
        var result = await handler.Handle(new ReassignTaskCommand(task.Id, assignee.Id, task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await activityLog.Received(1).LogAsync(pm.Id, "TaskReassigned", "Task", task.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>(), task.ProjectId);
    }

    [Fact]
    public async Task Handle_StaleIfMatch_Returns409()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm2@reassign-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskBuilder().WithProject(project).WithTitle("Concurrent Task").Build();
        db.Users.Add(pm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);
        var staleVersion = task.Version;

        // Simulate a concurrent write from another request via a separate context, bumping xmin.
        await using (var otherDb = _fixture.CreateDbContext())
        {
            var sameTask = await otherDb.Tasks.SingleAsync(t => t.Id == task.Id);
            sameTask.Title = "Changed elsewhere";
            await otherDb.SaveChangesAsync(CancellationToken.None);
        }

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(pm.Id, pm.Email!, "ProjectManager", pm.FullName));
        var activityLog = Substitute.For<IActivityLogService>();
        var assigneeValidator = new AssigneeValidator(db);

        var handler = new ReassignTaskCommandHandler(db, accessPolicy, currentUser, activityLog, assigneeValidator);
        var result = await handler.Handle(new ReassignTaskCommand(task.Id, null, staleVersion), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Conflict);
    }
}
