using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.GetTaskCompletion;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;

namespace ProjectManagementApp.Application.Tests.Features.Reports;

// T047: the grouped aggregate is computed in SQL (a real Postgres GROUP BY over the scoped+filtered
// task set — see GetTaskCompletionQueryHandler's single GroupBy().Select().ToListAsync()), and
// BucketGenerator's zero-fill is a pure, separate, post-materialization step.
[Collection(PostgresCollection.Name)]
public class GetTaskCompletionQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public GetTaskCompletionQueryHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static CurrentUser CurrentUserFor(Domain.Entities.ApplicationUser user, string role) =>
        new(user.Id, user.Email!, role, user.FullName);

    private static GetTaskCompletionQueryHandler MakeHandlerFor(
        ProjectManagementApp.Infrastructure.Persistence.ApplicationDbContext db, CurrentUser caller, IActivityLogService activityLogService)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.Current.Returns(caller);
        return new GetTaskCompletionQueryHandler(db, new ProjectAccessPolicy(db), currentUserService, activityLogService,
            Options.Create(new ReportsOptions()));
    }

    [Fact]
    public async Task Handle_MonthGrouping_SumsAllThreeInWindowClosuresIntoOneBucket_ReopenedExcluded()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ReportsScenario();
        await scenario.SeedAsync(db);

        var caller = CurrentUserFor(scenario.Team.Tasks.Projects.Admin, "Admin");
        var handler = MakeHandlerFor(db, caller, Substitute.For<IActivityLogService>());

        var result = await handler.Handle(
            new GetTaskCompletionQuery(ReportsScenario.WindowFrom, ReportsScenario.WindowTo, "month", null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var bucket = result.Value!.Buckets.Should().ContainSingle().Subject;
        // 3 closed-in-window (RClosedEarlyInWindow, RClosedMidWindow, RClosedLateWindow_Tm2) —
        // RReopened's cleared closed_at excludes it, by the same rule T037 proved for Project
        // Progress's closedTasks.
        bucket.CompletedCount.Should().Be(3);
        result.Value!.Totals.Completed.Should().Be(3);
    }

    [Fact]
    public async Task Handle_DayGrouping_ZeroFillsEveryDayWithNoClosures()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ReportsScenario();
        await scenario.SeedAsync(db);

        var caller = CurrentUserFor(scenario.Team.Tasks.Projects.Admin, "Admin");
        var handler = MakeHandlerFor(db, caller, Substitute.For<IActivityLogService>());

        var from = ReportsScenario.WindowFrom;
        var to = from.AddDays(6);
        var result = await handler.Handle(
            new GetTaskCompletionQuery(from, to, "day", null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // RClosedEarlyInWindow closes on WindowFrom+2 — one non-zero day among 7, six zeros.
        result.Value!.Buckets.Should().HaveCount(7);
        result.Value!.Buckets.Count(b => b.CompletedCount == 0).Should().Be(6);
        result.Value!.Buckets.Single(b => b.PeriodStart == from.AddDays(2)).CompletedCount.Should().Be(1);
    }
}
