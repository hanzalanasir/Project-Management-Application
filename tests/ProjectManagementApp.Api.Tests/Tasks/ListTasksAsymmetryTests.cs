using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// The nested route names a project, so it can 403/404; the flat route names no resource, so scope
// only shapes content — never a 403 or 404 (research R-4).
[Collection(ApiTestCollection.Name)]
public class ListTasksAsymmetryTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ListTasksAsymmetryTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task NestedRoute_OutOfScopeProject_Returns403()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);

        var response = await ListTasksByProjectAsync(client, scenario.Pm2Token, scenario.ProjectAId);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NestedRoute_UnknownProject_Returns404()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);

        var response = await ListTasksByProjectAsync(client, scenario.PmToken, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FlatRoute_ProjectIdFilterForAnOutOfScopeProject_ReturnsScopedEmptyContent_Never403Or404()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);

        var response = await ListTasksAsync(client, scenario.Pm2Token, $"?projectId={scenario.ProjectAId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task FlatRoute_ProjectIdFilterForAnUnknownProject_ReturnsEmptyContent_Never404()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);

        var response = await ListTasksAsync(client, scenario.PmToken, $"?projectId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("totalCount").GetInt32().Should().Be(0);
    }
}
