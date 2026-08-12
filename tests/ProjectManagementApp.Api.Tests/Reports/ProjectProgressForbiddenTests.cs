using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T033 — named-out-of-scope semantics (FR-002, FR-004, research R-7): a NAMED projectScope outside
// the caller's visibility 403s; "all" silently narrows (never a 403, nothing was named); a named
// list where every id is in scope succeeds covering exactly those; a list mixing in-scope and
// out-of-scope ids 403s the WHOLE request — nothing is ever partially returned.
[Collection(ApiTestCollection.Name)]
public class ProjectProgressForbiddenTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ProjectProgressForbiddenTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static string WindowWithScope(string projectScope) =>
        $"{DefaultWindowQuery()}&projectScope={Uri.EscapeDataString(projectScope)}";

    [Fact]
    public async Task ProjectProgress_PmNamesPm2sProject_Returns403()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "forbid-named");

        var response = await GetProjectProgressResponseAsync(
            client, scenario.PmToken, WindowWithScope(scenario.ProjectBId.ToString()));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ProjectProgress_ProjectScopeAll_SilentlyNarrowsToOwnedProject_200()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "forbid-all");

        var response = await GetProjectProgressResponseAsync(client, scenario.PmToken, WindowWithScope("all"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await GetProjectProgressAsync(client, scenario.PmToken, WindowWithScope("all"));
        var rows = body.GetProperty("rows").EnumerateArray().ToList();
        rows.Should().ContainSingle();
        rows[0].GetProperty("projectId").GetString().Should().Be(scenario.ProjectAId.ToString());
    }

    [Fact]
    public async Task ProjectProgress_NamedListAllInScope_SucceedsCoveringExactlyThoseIds()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "forbid-list-ok");

        var body = await GetProjectProgressAsync(
            client, scenario.AdminToken, WindowWithScope($"{scenario.ProjectAId},{scenario.ProjectBId}"));

        var projectIds = body.GetProperty("rows").EnumerateArray()
            .Select(r => r.GetProperty("projectId").GetString()).ToList();
        projectIds.Should().BeEquivalentTo([scenario.ProjectAId.ToString(), scenario.ProjectBId.ToString()]);
    }

    [Fact]
    public async Task ProjectProgress_ListMixingInAndOutOfScope_Returns403ForTheWholeRequest_NotPartial()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "forbid-list-mixed");

        var response = await GetProjectProgressResponseAsync(
            client, scenario.PmToken, WindowWithScope($"{scenario.ProjectAId},{scenario.ProjectBId}"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "one out-of-scope id in the list must fail the whole request, not silently drop it");
    }

    [Fact]
    public async Task ProjectProgress_MalformedProjectScope_Returns400_NotForbidden()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "forbid-malformed");

        var response = await GetProjectProgressResponseAsync(client, scenario.PmToken, WindowWithScope("not-a-guid"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
