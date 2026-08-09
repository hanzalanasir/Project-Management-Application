using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

[Collection(ApiTestCollection.Name)]
public class ListTasksPagingTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ListTasksPagingTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ListTasks_PageSizeAboveMax_IsClampedTo100_NotRejected()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);

        var response = await ListTasksAsync(client, scenario.AdminToken, "?page=1&pageSize=500");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("pageSize").GetInt32().Should().Be(100);
    }

    [Fact]
    public async Task ListTasks_NegativePage_Returns400()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);

        var response = await ListTasksAsync(client, scenario.AdminToken, "?page=-1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListTasks_InteriorSubstringTitleSearch_MatchesViaTrigramIndex()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);
        await CreateTaskAsync(client, scenario.PmToken, scenario.ProjectAId, new CreateTaskRequest("Draft rollout checklist", null, null, null, null));

        var response = await ListTasksAsync(client, scenario.AdminToken, "?search=rollout");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("totalCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task ListTasks_UnwhitelistedSort_Returns400()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);

        var response = await ListTasksAsync(client, scenario.AdminToken, "?sort=notAField");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
