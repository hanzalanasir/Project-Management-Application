using FluentAssertions;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.GetProjectProgress;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using Microsoft.Extensions.Options;

namespace ProjectManagementApp.Application.Tests.Features.Reports;

// T037: scope is composed BEFORE aggregation (a PM never even queries B's tasks), and the shared
// counting rules (T014/ReportCountingRules) are what the handler relies on for the re-open
// exclusion — not a second, handler-local implementation of "is this task closed".
[Collection(PostgresCollection.Name)]
public class GetProjectProgressQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public GetProjectProgressQueryHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static CurrentUser CurrentUserFor(Domain.Entities.ApplicationUser user, string role) =>
        new(user.Id, user.Email!, role, user.FullName);

    private static GetProjectProgressQueryHandler MakeHandlerFor(
        ProjectManagementApp.Infrastructure.Persistence.ApplicationDbContext db, CurrentUser caller, IActivityLogService activityLogService)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.Current.Returns(caller);
        return new GetProjectProgressQueryHandler(db, new ProjectAccessPolicy(db), currentUserService, activityLogService,
            Options.Create(new ReportsOptions()));
    }

    [Fact]
    public async Task Handle_ProjectManager_NeverReceivesRowsFromAnUnownedProject()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ReportsScenario();
        await scenario.SeedAsync(db);

        var caller = CurrentUserFor(scenario.Team.Tasks.Projects.Pm, "ProjectManager");
        var activityLogService = Substitute.For<IActivityLogService>();
        var handler = MakeHandlerFor(db, caller, activityLogService);

        var result = await handler.Handle(
            new GetProjectProgressQuery(ReportsScenario.WindowFrom, ReportsScenario.WindowTo, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Rows.Should().OnlyContain(r => r.ProjectId == scenario.Team.Tasks.Projects.ProjectA.Id);
    }

    [Fact]
    public async Task Handle_ReopenedTask_ExcludedFromClosedTasks_SameRuleAsTaskCompletion()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ReportsScenario();
        await scenario.SeedAsync(db);

        var caller = CurrentUserFor(scenario.Team.Tasks.Projects.Admin, "Admin");
        var activityLogService = Substitute.For<IActivityLogService>();
        var handler = MakeHandlerFor(db, caller, activityLogService);

        var result = await handler.Handle(
            new GetProjectProgressQuery(ReportsScenario.WindowFrom, ReportsScenario.WindowTo, scenario.Team.Tasks.Projects.ProjectA.Id.ToString()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var row = result.Value!.Rows.Should().ContainSingle().Subject;

        // RClosedEarlyInWindow, RClosedMidWindow, RClosedLateWindow_Tm2 are Done+closed_at set — 3
        // closed. RReopened is InProgress+closed_at cleared — must NOT count as closed, by the same
        // ReportCountingRules.IsClosed rule Task Completion's bucketing will use in stage 3.
        row.ClosedTasks.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ScopeFailure_PropagatesAsFailureResult_NoRowsComputed()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ReportsScenario();
        await scenario.SeedAsync(db);

        var caller = CurrentUserFor(scenario.Team.Tasks.Projects.Pm, "ProjectManager");
        var activityLogService = Substitute.For<IActivityLogService>();
        var handler = MakeHandlerFor(db, caller, activityLogService);

        var result = await handler.Handle(
            new GetProjectProgressQuery(ReportsScenario.WindowFrom, ReportsScenario.WindowTo, scenario.Team.Tasks.Projects.ProjectB.Id.ToString()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
        await activityLogService.DidNotReceiveWithAnyArgs().LogAsync(default, default!, default!, default!, default!, default);
    }
}
