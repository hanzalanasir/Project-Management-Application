using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// T032: default pageSize 20; oversized pageSize clamped to 100 (never rejected, Constitution
// VI.4); an out-of-range page returns an empty page with valid metadata (never an error); a
// negative page is a genuine 400; ordering is newest-first and stable across requests.
[Collection(ApiTestCollection.Name)]
public class ActivityPagingTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ActivityPagingTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Activity_NoPageSizeSupplied_DefaultsToTwenty()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "paging-default");

        var activity = await GetActivityAsync(client, scenario.PmToken);

        activity.GetProperty("pageSize").GetInt32().Should().Be(20);
    }

    [Fact]
    public async Task Activity_PageSizeAboveMaximum_IsClamped_NotRejected()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "paging-clamp");

        var response = await GetActivityResponseAsync(client, scenario.PmToken, "?pageSize=500");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var activity = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        activity.GetProperty("pageSize").GetInt32().Should().Be(100);
    }

    [Fact]
    public async Task Activity_PageFarBeyondTheData_ReturnsEmptyItems_WithValidMetadata_Not404()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "paging-farpage");

        var response = await GetActivityResponseAsync(client, scenario.PmToken, "?page=999");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var activity = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        activity.GetProperty("items").GetArrayLength().Should().Be(0);
        activity.GetProperty("page").GetInt32().Should().Be(999);
        activity.GetProperty("totalCount").GetInt32().Should().Be(11);
    }

    [Fact]
    public async Task Activity_NegativePage_Returns400()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "paging-negative");

        var response = await GetActivityResponseAsync(client, scenario.PmToken, "?page=-1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Activity_Ordering_IsNewestFirst_AndStableAcrossRequests()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "paging-order");

        var first = await GetActivityAsync(client, scenario.PmToken, "?pageSize=100");
        var second = await GetActivityAsync(client, scenario.PmToken, "?pageSize=100");

        var firstIds = first.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetString()).ToList();
        var secondIds = second.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetString()).ToList();
        firstIds.Should().Equal(secondIds);

        var timestamps = first.GetProperty("items").EnumerateArray()
            .Select(i => DateTimeOffset.Parse(i.GetProperty("timestamp").GetString()!))
            .ToList();
        timestamps.Should().BeInDescendingOrder();
    }
}
