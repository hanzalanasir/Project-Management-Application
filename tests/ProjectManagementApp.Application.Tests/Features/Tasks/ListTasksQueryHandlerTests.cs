using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Tasks.ListTasks;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;

namespace ProjectManagementApp.Application.Tests.Features.Tasks;

// Proves the fixed composition order (data-model.md §5): scope -> filter -> count -> sort -> page.
// Runs against real PostgreSQL — the scope predicate must reach SQL (ADR-0007 §2).
[Collection(PostgresCollection.Name)]
public class ListTasksQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public ListTasksQueryHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static IOptions<TasksOptions> DefaultOptions() => Options.Create(new TasksOptions());

    [Fact]
    public async Task Handle_CountIsComputedOnTheScopedQuery_NotTheSystemWideOne()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new TasksScenario();
        await scenario.SeedAsync(db);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(scenario.Tm2.Id, scenario.Tm2.Email!, "TeamMember", scenario.Tm2.FullName));

        var handler = new ListTasksQueryHandler(db, accessPolicy, currentUser, DefaultOptions());
        var result = await handler.Handle(new ListTasksQuery(1, 20, null, null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(1);
        result.Value!.Items.Should().ContainSingle(t => t.Id == scenario.Task2.Id);
    }

    [Fact]
    public async Task Handle_StatusFilter_IsAppliedWithinScope_NeverWidensIt()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new TasksScenario();
        await scenario.SeedAsync(db);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        // PM owns project A (all 3 tasks, default status ToDo) — filtering for Done must yield
        // zero, proving the filter composes on top of scope rather than searching system-wide.
        currentUser.Current.Returns(new CurrentUser(scenario.Projects.Pm.Id, scenario.Projects.Pm.Email!, "ProjectManager", scenario.Projects.Pm.FullName));

        var handler = new ListTasksQueryHandler(db, accessPolicy, currentUser, DefaultOptions());
        var result = await handler.Handle(new ListTasksQuery(1, 20, null, "Done", null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PageSizeAboveConfiguredMax_IsClamped()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new TasksScenario();
        await scenario.SeedAsync(db);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(scenario.Projects.Admin.Id, scenario.Projects.Admin.Email!, "Admin", scenario.Projects.Admin.FullName));

        var handler = new ListTasksQueryHandler(db, accessPolicy, currentUser, DefaultOptions());
        var result = await handler.Handle(new ListTasksQuery(1, 500, null, null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task Handle_ProjectIdFilter_NarrowsWithinScope()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new TasksScenario();
        await scenario.SeedAsync(db);

        var accessPolicy = new TaskAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(scenario.Projects.Admin.Id, scenario.Projects.Admin.Email!, "Admin", scenario.Projects.Admin.FullName));

        var handler = new ListTasksQueryHandler(db, accessPolicy, currentUser, DefaultOptions());
        var result = await handler.Handle(new ListTasksQuery(1, 20, scenario.Projects.ProjectB.Id, null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(0);
    }
}
