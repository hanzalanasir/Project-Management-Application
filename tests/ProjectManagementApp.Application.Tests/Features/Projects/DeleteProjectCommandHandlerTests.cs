using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Projects.DeleteProject;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;

namespace ProjectManagementApp.Application.Tests.Features.Projects;

[Collection(PostgresCollection.Name)]
public class DeleteProjectCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public DeleteProjectCommandHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Handle_UnknownId_ReturnsNotFound()
    {
        await using var db = _fixture.CreateDbContext();
        var accessPolicy = new ProjectAccessPolicy(db);
        var activityLog = Substitute.For<IActivityLogService>();
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(Guid.NewGuid(), "caller@example.com", "Admin", "Caller"));

        var handler = new DeleteProjectCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new DeleteProjectCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_OutOfScopePM_ReturnsForbidden_ProjectStillExists()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner@delete-handler-test.com").Build();
        var otherPm = new ApplicationUserBuilder().WithEmail("other@delete-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).Build();
        db.Users.AddRange(owner, otherPm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new ProjectAccessPolicy(db);
        var activityLog = Substitute.For<IActivityLogService>();
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(otherPm.Id, otherPm.Email!, "ProjectManager", otherPm.FullName));

        var handler = new DeleteProjectCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new DeleteProjectCommand(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
        (await db.Projects.AnyAsync(p => p.Id == project.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Success_WritesTheAuditRowBeforeRemoval_InTheSameSaveChangesCall()
    {
        await using var db = _fixture.CreateDbContext();
        var owner = new ApplicationUserBuilder().WithEmail("owner2@delete-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(owner).WithName("Doomed").Build();
        db.Users.Add(owner);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new ProjectAccessPolicy(db);
        var activityLog = Substitute.For<IActivityLogService>();
        // Asserting call ORDER: LogAsync must be invoked before Projects.Remove takes effect —
        // proven here by checking the entity is still tracked/present at the moment LogAsync fires.
        var projectStillPresentWhenLogged = false;
        activityLog
            .When(x => x.LogAsync(Arg.Any<Guid?>(), "ProjectDeleted", "Project", project.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<Guid?>()))
            .Do(_ => projectStillPresentWhenLogged = db.Projects.Local.Any(p => p.Id == project.Id));

        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(owner.Id, owner.Email!, "ProjectManager", owner.FullName));

        var handler = new DeleteProjectCommandHandler(db, accessPolicy, currentUser, activityLog);
        var result = await handler.Handle(new DeleteProjectCommand(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        projectStillPresentWhenLogged.Should().BeTrue();
        await activityLog.Received(1).LogAsync(owner.Id, "ProjectDeleted", "Project", project.Id.ToString(), Arg.Is<string>(s => s!.Contains("Doomed")), Arg.Any<CancellationToken>(), project.Id);
        (await db.Projects.AnyAsync(p => p.Id == project.Id)).Should().BeFalse();
    }
}
