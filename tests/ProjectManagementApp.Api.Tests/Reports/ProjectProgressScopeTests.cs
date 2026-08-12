using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T032 — the three-role scope matrix (spec DoD 2): Admin sees every visible project, a
// ProjectManager only the ones they own, a TeamMember only the ones they are a member of, and a
// caller with nothing visible gets 200 with an empty rows array — never 403 (nothing was named).
[Collection(ApiTestCollection.Name)]
public class ProjectProgressScopeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ProjectProgressScopeTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ProjectProgress_Admin_SeesEveryProject()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "scope-admin");

        var body = await GetProjectProgressAsync(client, scenario.AdminToken, DefaultWindowQuery());

        var projectIds = body.GetProperty("rows").EnumerateArray()
            .Select(r => r.GetProperty("projectId").GetString()).ToList();
        projectIds.Should().BeEquivalentTo([scenario.ProjectAId.ToString(), scenario.ProjectBId.ToString()]);
    }

    [Fact]
    public async Task ProjectProgress_ProjectManager_SeesOnlyOwnedProject()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "scope-pm");

        var body = await GetProjectProgressAsync(client, scenario.PmToken, DefaultWindowQuery());

        var rows = body.GetProperty("rows").EnumerateArray().ToList();
        rows.Should().ContainSingle();
        rows[0].GetProperty("projectId").GetString().Should().Be(scenario.ProjectAId.ToString());
    }

    [Fact]
    public async Task ProjectProgress_SecondProjectManager_SeesOnlyTheirOwnProject_NothingFromA()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "scope-pm2");

        var body = await GetProjectProgressAsync(client, scenario.Pm2Token, DefaultWindowQuery());

        var rows = body.GetProperty("rows").EnumerateArray().ToList();
        rows.Should().ContainSingle();
        rows[0].GetProperty("projectId").GetString().Should().Be(scenario.ProjectBId.ToString());
    }

    [Fact]
    public async Task ProjectProgress_TeamMember_SeesOnlyMemberOfProject()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "scope-tm");

        var body = await GetProjectProgressAsync(client, scenario.TmToken, DefaultWindowQuery());

        var rows = body.GetProperty("rows").EnumerateArray().ToList();
        rows.Should().ContainSingle();
        rows[0].GetProperty("projectId").GetString().Should().Be(scenario.ProjectAId.ToString());
    }

    [Fact]
    public async Task ProjectProgress_CallerWithNoVisibleProjects_Returns200WithEmptyRows_Never403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        await ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper.RegisterProjectManagerAsync(
            client, adminToken, "Lonely PM", "lonely-pm-progress@example.com", "S3cure-P@ss1!");
        var lonelyPmToken = await LoginAsync(client, "lonely-pm-progress@example.com", "S3cure-P@ss1!");

        var response = await GetProjectProgressResponseAsync(client, lonelyPmToken, DefaultWindowQuery());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await GetProjectProgressAsync(client, lonelyPmToken, DefaultWindowQuery());
        body.GetProperty("rows").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task ProjectProgress_RequiresAuthentication_401()
    {
        var client = _fixture.CreateClient();
        var response = await GetProjectProgressResponseAsync(client, "not-a-real-token", DefaultWindowQuery());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
