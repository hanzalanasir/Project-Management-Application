using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// A filter narrows a scope; it never widens it. A 403 here would confirm the colleague's task
// exists — the only safe answer to "does TM2 have task X" from TM's perspective is an empty page.
[Collection(ApiTestCollection.Name)]
public class ListTasksFilterTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ListTasksFilterTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task TeamMember_FilteringByAnotherAssigneesId_GetsAnEmptyPage_Not403()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);

        var response = await ListTasksAsync(client, scenario.TmToken, $"?assigneeId={scenario.Tm2Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("totalCount").GetInt32().Should().Be(0);
    }
}
