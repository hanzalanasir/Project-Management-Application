using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Projects.UpdateProject;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Projects;

[Collection(PostgresCollection.Name)]
public class UpdateProjectCommandHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public UpdateProjectCommandHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static ICurrentUserService CreateCurrentUser(Guid userId, string role) =>
        MakeCurrentUserService(userId, role);

    private static ICurrentUserService MakeCurrentUserService(Guid userId, string role)
    {
        var service = Substitute.For<ICurrentUserService>();
        service.Current.Returns(new CurrentUser(userId, "caller@example.com", role, "Caller"));
        return service;
    }

    private static IOptions<ProjectsOptions> DefaultOptions() => Options.Create(new ProjectsOptions());

    [Fact]
    public async Task Handle_ChangedFields_AuditSummaryListsThem()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm@update-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).WithName("Original").WithStatus(Domain.Enums.ProjectStatus.Planning).Build();
        db.Users.Add(pm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new ProjectAccessPolicy(db);
        var userManager = CreateUserManager();
        var activityLog = Substitute.For<IActivityLogService>();
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");

        var handler = new UpdateProjectCommandHandler(db, accessPolicy, userManager, currentUser, activityLog, DefaultOptions());
        var command = new UpdateProjectCommand(project.Id, "Renamed", project.Description, project.StartDate, project.EndDate, "Active", null, project.Version);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Renamed");
        await activityLog.Received(1).LogAsync(
            pm.Id, "ProjectUpdated", "Project", project.Id.ToString(),
            Arg.Is<string>(summary => summary != null && summary.Contains("name", StringComparison.OrdinalIgnoreCase) && summary.Contains("status", StringComparison.OrdinalIgnoreCase)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoOpUpdate_StillRefreshesUpdatedAt_AndAudits()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = new ApplicationUserBuilder().WithEmail("pm2@update-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).WithName("Same Name").Build();
        db.Users.Add(pm);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);
        var originalUpdatedAt = project.UpdatedAt;

        var accessPolicy = new ProjectAccessPolicy(db);
        var userManager = CreateUserManager();
        var activityLog = Substitute.For<IActivityLogService>();
        var currentUser = CreateCurrentUser(pm.Id, "ProjectManager");

        var handler = new UpdateProjectCommandHandler(db, accessPolicy, userManager, currentUser, activityLog, DefaultOptions());
        var command = new UpdateProjectCommand(project.Id, project.Name, project.Description, project.StartDate, project.EndDate, project.Status.ToString(), null, project.Version);

        await Task.Delay(10); // ensure a measurable clock difference
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        await activityLog.Received(1).LogAsync(pm.Id, "ProjectUpdated", "Project", project.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnerChanged_AuditsProjectOwnerChanged_InAdditionToProjectUpdated()
    {
        await using var db = _fixture.CreateDbContext();
        var admin = new ApplicationUserBuilder().WithEmail("admin@update-handler-test.com").Build();
        var pm = new ApplicationUserBuilder().WithEmail("pm3@update-handler-test.com").Build();
        var newOwner = new ApplicationUserBuilder().WithEmail("neowner@update-handler-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        db.Users.AddRange(admin, pm, newOwner);
        db.Projects.Add(project);
        await db.SaveChangesAsync(CancellationToken.None);

        var accessPolicy = new ProjectAccessPolicy(db);
        var userManager = CreateUserManager();
        userManager.GetRolesAsync(newOwner).Returns(new List<string> { "ProjectManager" });
        var activityLog = Substitute.For<IActivityLogService>();
        var currentUser = CreateCurrentUser(admin.Id, "Admin");

        var handler = new UpdateProjectCommandHandler(db, accessPolicy, userManager, currentUser, activityLog, DefaultOptions());
        var command = new UpdateProjectCommand(project.Id, project.Name, project.Description, project.StartDate, project.EndDate, project.Status.ToString(), newOwner.Id, project.Version);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Owner.Id.Should().Be(newOwner.Id);
        await activityLog.Received(1).LogAsync(admin.Id, "ProjectUpdated", "Project", project.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await activityLog.Received(1).LogAsync(admin.Id, "ProjectOwnerChanged", "Project", project.Id.ToString(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
