using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Tasks.GetTaskById;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;

namespace ProjectManagementApp.Application.Tests.Features.Tasks;

// Not-found is checked BEFORE the scope gate (mirrors 002's GetProjectByIdQueryHandler): an
// unknown id is 404 regardless of caller role, and a known-but-out-of-scope id is 403 only once
// existence is established.
[Collection(PostgresCollection.Name)]
public class GetTaskByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public GetTaskByIdQueryHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static IOptions<TasksOptions> Options(bool mask = false) =>
        Microsoft.Extensions.Options.Options.Create(new TasksOptions { MaskOutOfScopeAs404 = mask });

    [Fact]
    public async Task Handle_UnknownId_ReturnsNotFound_RegardlessOfCallerRole()
    {
        await using var db = _fixture.CreateDbContext();
        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(Guid.NewGuid(), "caller@example.com", "Admin", "Caller"));

        var handler = new GetTaskByIdQueryHandler(db, accessPolicy, currentUser, Options());
        var result = await handler.Handle(new GetTaskByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_KnownId_OutOfScope_ReturnsForbidden_AfterExistenceIsEstablished()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner@get-task-test.com").Build();
        var otherPm = new ApplicationUserBuilder().WithEmail("other@get-task-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        var task = new TaskBuilder().WithProject(project).Build();
        db.Users.AddRange(owner, otherPm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(otherPm.Id, otherPm.Email!, "ProjectManager", otherPm.FullName));

        var handler = new GetTaskByIdQueryHandler(db, accessPolicy, currentUser, Options());
        var result = await handler.Handle(new GetTaskByIdQuery(task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
    }

    [Fact]
    public async Task Handle_KnownId_OutOfScope_WithMaskingEnabled_ReturnsNotFound_InsteadOfForbidden()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner2@get-task-test.com").Build();
        var otherPm = new ApplicationUserBuilder().WithEmail("other2@get-task-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        var task = new TaskBuilder().WithProject(project).Build();
        db.Users.AddRange(owner, otherPm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(otherPm.Id, otherPm.Email!, "ProjectManager", otherPm.FullName));

        var handler = new GetTaskByIdQueryHandler(db, accessPolicy, currentUser, Options(mask: true));
        var result = await handler.Handle(new GetTaskByIdQuery(task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_AssigneeWasDeactivated_TaskIsStillReturned_WithAssigneeFlaggedInactive()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm@get-task-test.com").Build();
        var deactivatedAssignee = new ApplicationUserBuilder().WithEmail("deactivated@get-task-test.com").Inactive().Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskBuilder().WithProject(project).WithAssignee(deactivatedAssignee).Build();
        db.Users.AddRange(pm, deactivatedAssignee);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(pm.Id, pm.Email!, "ProjectManager", pm.FullName));

        var handler = new GetTaskByIdQueryHandler(db, accessPolicy, currentUser, Options());
        var result = await handler.Handle(new GetTaskByIdQuery(task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Assignee.Should().NotBeNull();
        result.Value!.Assignee!.IsActive.Should().BeFalse();
    }
}
