using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.GetTeamPerformance;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;

namespace ProjectManagementApp.Application.Tests.Features.Reports;

// T058: the self-clamp is applied in the handler, before any pool lookup, and cannot be bypassed
// by any combination of userId/projectScope a TeamMember caller supplies.
[Collection(PostgresCollection.Name)]
public class GetTeamPerformanceQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public GetTeamPerformanceQueryHandlerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static CurrentUser CurrentUserFor(Domain.Entities.ApplicationUser user, string role) =>
        new(user.Id, user.Email!, role, user.FullName);

    private static GetTeamPerformanceQueryHandler MakeHandlerFor(
        ProjectManagementApp.Infrastructure.Persistence.ApplicationDbContext db, CurrentUser caller, IActivityLogService activityLogService) =>
        new(db, new ProjectAccessPolicy(db), Sub(caller), activityLogService, Options.Create(new ReportsOptions()));

    private static ICurrentUserService Sub(CurrentUser caller)
    {
        var service = Substitute.For<ICurrentUserService>();
        service.Current.Returns(caller);
        return service;
    }

    [Fact]
    public async Task Handle_TeamMember_SuppliesColleaguesUserId_StillGetsOnlyOwnRow_NeverForbidden()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ReportsScenario();
        await scenario.SeedAsync(db);

        var tm = scenario.Team.Tasks.Projects.Tm;
        var tm2 = scenario.Team.Tasks.Tm2;
        var caller = CurrentUserFor(tm, "TeamMember");
        var handler = MakeHandlerFor(db, caller, Substitute.For<IActivityLogService>());

        var result = await handler.Handle(
            new GetTeamPerformanceQuery(ReportsScenario.WindowFrom, ReportsScenario.WindowTo, null, tm2.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var row = result.Value!.Rows.Should().ContainSingle().Subject;
        row.UserId.Should().Be(tm.Id);
    }

    [Fact]
    public async Task Handle_TeamMember_SuppliesColleaguesUserId_WithProjectScopeAlsoSet_StillClamped()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ReportsScenario();
        await scenario.SeedAsync(db);

        var tm = scenario.Team.Tasks.Projects.Tm;
        var tm2 = scenario.Team.Tasks.Tm2;
        var projectA = scenario.Team.Tasks.Projects.ProjectA;
        var caller = CurrentUserFor(tm, "TeamMember");
        var handler = MakeHandlerFor(db, caller, Substitute.For<IActivityLogService>());

        // Even with an explicit (in-scope) projectScope AND a colleague's userId both supplied,
        // the clamp still wins — no parameter combination bypasses it.
        var result = await handler.Handle(
            new GetTeamPerformanceQuery(ReportsScenario.WindowFrom, ReportsScenario.WindowTo, projectA.Id.ToString(), tm2.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Rows.Should().ContainSingle().Which.UserId.Should().Be(tm.Id);
    }

    [Fact]
    public async Task Handle_ProjectManager_NamingAnOutOfScopeUser_ReturnsForbidden_AndNeverAudits()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = new ReportsScenario();
        await scenario.SeedAsync(db);

        var pm = scenario.Team.Tasks.Projects.Pm;
        var admin = scenario.Team.Tasks.Projects.Admin;
        var caller = CurrentUserFor(pm, "ProjectManager");
        var activityLogService = Substitute.For<IActivityLogService>();
        var handler = MakeHandlerFor(db, caller, activityLogService);

        var result = await handler.Handle(
            new GetTeamPerformanceQuery(ReportsScenario.WindowFrom, ReportsScenario.WindowTo, null, admin.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.Forbidden);
        await activityLogService.DidNotReceiveWithAnyArgs().LogAsync(default, default!, default!, default!, default!, default);
    }
}
