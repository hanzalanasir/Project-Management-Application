using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// GET /api/projects/{id}/tasks and GET /api/tasks?projectId={id} must return IDENTICAL content for
// the same caller — one handler, one predicate (research R-4). Two implementations that could
// drift would be a real security bug waiting to happen.
[Collection(ApiTestCollection.Name)]
public class ListTasksRouteParityTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ListTasksRouteParityTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task NestedRouteAndFlatRouteWithProjectIdFilter_ReturnIdenticalContent_ForTheSameCaller()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);

        var nested = await ReadJsonAsync(await ListTasksByProjectAsync(client, scenario.PmToken, scenario.ProjectAId));
        var flat = await ReadJsonAsync(await ListTasksAsync(client, scenario.PmToken, $"?projectId={scenario.ProjectAId}"));

        nested.GetProperty("totalCount").GetInt32().Should().Be(flat.GetProperty("totalCount").GetInt32());
        Ids(nested).Should().BeEquivalentTo(Ids(flat));

        var nestedTm = await ReadJsonAsync(await ListTasksByProjectAsync(client, scenario.TmToken, scenario.ProjectAId));
        var flatTm = await ReadJsonAsync(await ListTasksAsync(client, scenario.TmToken, $"?projectId={scenario.ProjectAId}"));

        nestedTm.GetProperty("totalCount").GetInt32().Should().Be(flatTm.GetProperty("totalCount").GetInt32());
        Ids(nestedTm).Should().BeEquivalentTo(Ids(flatTm));
    }
}
