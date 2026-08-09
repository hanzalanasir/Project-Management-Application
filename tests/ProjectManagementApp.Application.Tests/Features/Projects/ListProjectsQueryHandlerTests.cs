using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Projects.ListProjects;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;

namespace ProjectManagementApp.Application.Tests.Features.Projects;

// Proves the fixed composition order (data-model.md §5): scope -> filter -> count -> sort -> page.
// Runs against real PostgreSQL — the scope predicate must reach SQL (ADR-0007 §2), which an
// EF.Functions.ILike-based search predicate cannot fake against an in-memory provider either.
[Collection(PostgresCollection.Name)]
public class ListProjectsQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public ListProjectsQueryHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static IOptions<ProjectsOptions> DefaultOptions() => Options.Create(new ProjectsOptions());

    [Fact]
    public async Task Handle_CountIsComputedOnTheScopedQuery_NotTheSystemWideOne()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var accessPolicy = new ProjectAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(scenario.Pm.Id, scenario.Pm.Email!, "ProjectManager", scenario.Pm.FullName));

        var handler = new ListProjectsQueryHandler(db, accessPolicy, currentUser, DefaultOptions());
        var result = await handler.Handle(new ListProjectsQuery(1, 20, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(1);
        result.Value!.Items.Should().ContainSingle(p => p.Id == scenario.ProjectA.Id);
    }

    [Fact]
    public async Task Handle_StatusFilter_IsAppliedWithinScope_NeverWidensIt()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var accessPolicy = new ProjectAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        // PM owns only Project A, which defaults to Planning — filtering for Active must yield
        // zero, proving the filter composes on top of scope rather than searching system-wide.
        currentUser.Current.Returns(new CurrentUser(scenario.Pm.Id, scenario.Pm.Email!, "ProjectManager", scenario.Pm.FullName));

        var handler = new ListProjectsQueryHandler(db, accessPolicy, currentUser, DefaultOptions());
        var result = await handler.Handle(new ListProjectsQuery(1, 20, null, "Active", null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PageSizeAboveConfiguredMax_IsClamped()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ProjectsScenario();
        await scenario.SeedAsync(db);

        var accessPolicy = new ProjectAccessPolicy(db);
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(scenario.Admin.Id, scenario.Admin.Email!, "Admin", scenario.Admin.FullName));

        var handler = new ListProjectsQueryHandler(db, accessPolicy, currentUser, DefaultOptions());
        var result = await handler.Handle(new ListProjectsQuery(1, 500, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PageSize.Should().Be(100);
    }
}
