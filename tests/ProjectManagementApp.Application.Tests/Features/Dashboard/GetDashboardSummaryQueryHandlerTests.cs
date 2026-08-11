using FluentAssertions;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Dashboard;
using ProjectManagementApp.Application.Features.Dashboard.GetSummary;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Features.Dashboard;

// T023: each aggregate is composed against the scope subquery (never executed against the whole
// table then filtered), and the values are correct for each role — including the TeamMember
// personal-view rule (T046).
[Collection(PostgresCollection.Name)]
public class GetDashboardSummaryQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public GetDashboardSummaryQueryHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static GetDashboardSummaryQueryHandler CreateHandler(
        ProjectManagementApp.Infrastructure.Persistence.ApplicationDbContext db, ApplicationUser caller, string role) =>
        new(db, new ProjectAccessPolicy(db), new TaskAccessPolicy(db), CurrentUserFor(caller, role));

    private static ICurrentUserService CurrentUserFor(ApplicationUser user, string role)
    {
        var svc = Substitute.For<ICurrentUserService>();
        svc.Current.Returns(new CurrentUser(user.Id, user.Email!, role, user.FullName));
        return svc;
    }

    [Fact]
    public async Task Handle_Admin_TaskCounts_ComposedAgainstFullScope()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new DashboardScenario();
        await scenario.SeedAsync(db);

        var handler = CreateHandler(db, scenario.Team.Tasks.Projects.Admin, "Admin");
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!;
        dto.VisibleProjectCount.Should().Be(2);
        dto.TasksByStatus.ToDo.Should().Be(5);
        dto.TasksByStatus.InProgress.Should().Be(1);
        dto.TasksByStatus.InReview.Should().Be(1);
        dto.TasksByStatus.Done.Should().Be(1);
        dto.TasksByStatus.Blocked.Should().Be(1);
        dto.OverdueTaskCount.Should().Be(2);
        dto.BlockedTaskCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ProjectManager_ScopedToOwnedProjectOnly()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new DashboardScenario();
        await scenario.SeedAsync(db);

        var handler = CreateHandler(db, scenario.Team.Tasks.Projects.Pm, "ProjectManager");
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        result.Value!.VisibleProjectCount.Should().Be(1);
        result.Value!.TasksByStatus.ToDo.Should().Be(5);
    }

    [Fact]
    public async Task Handle_TeamMember_TileCountsAreThePersonalSlice_NotProjectWide()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new DashboardScenario();
        await scenario.SeedAsync(db);

        var handler = CreateHandler(db, scenario.Team.Tasks.Projects.Tm, "TeamMember");
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        var dto = result.Value!;
        // Tm's own assigned tasks: Task1(ToDo), DToDo(ToDo), DInProgress(InProgress), DDone(Done) — 4 total.
        dto.TasksByStatus.ToDo.Should().Be(2);
        dto.TasksByStatus.InProgress.Should().Be(1);
        dto.TasksByStatus.Done.Should().Be(1);
        dto.TasksByStatus.InReview.Should().Be(0);
        dto.TasksByStatus.Blocked.Should().Be(0);
        dto.OverdueTaskCount.Should().Be(1);

        // T042: literally the same numbers as personalTasks, not merely equal by coincidence.
        dto.TasksByStatus.Should().BeEquivalentTo(dto.PersonalTasks.ByStatus);
        dto.OverdueTaskCount.Should().Be(dto.PersonalTasks.OverdueCount);
    }

    [Fact]
    public async Task Handle_UnassignedTeamMember_PersonalTasksAreZero_NotAffectingColleaguesCounts()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new DashboardScenario();
        await scenario.SeedAsync(db);

        var handler = CreateHandler(db, scenario.Team.Tasks.Tm2, "TeamMember");
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        var dto = result.Value!;
        // Tm2 is a member of A and assigned Task2 only — not any of Tm's/unassigned D* tasks.
        dto.TasksByStatus.ToDo.Should().Be(1);
        dto.TasksByStatus.InProgress.Should().Be(0);
        dto.TasksByStatus.Done.Should().Be(0);
        dto.PersonalTasks.AssignedTotal.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Lonely_ReturnsAllZeros_ForEveryTile()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new DashboardScenario();
        await scenario.SeedAsync(db);

        var handler = CreateHandler(db, scenario.Lonely, "TeamMember");
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!;
        dto.VisibleProjectCount.Should().Be(0);
        dto.TasksByStatus.Should().BeEquivalentTo(new TaskStatusCountsDto(0, 0, 0, 0, 0));
        dto.OverdueTaskCount.Should().Be(0);
        dto.CompletionRate.Should().Be(0);
        dto.PersonalTasks.AssignedTotal.Should().Be(0);
    }
}
