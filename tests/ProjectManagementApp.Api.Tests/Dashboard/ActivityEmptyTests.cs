using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// T034: an empty scope (LONELY — on no team anywhere) returns 200 with an empty page, never 404.
[Collection(ApiTestCollection.Name)]
public class ActivityEmptyTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ActivityEmptyTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Activity_LonelyTeamMember_Returns200_WithAnEmptyPage_Never404()
    {
        var client = _fixture.CreateClient();
        await RegisterAndGetIdAsync(client, "Lonely Activity", "lonely-activity@example.com", "S3cure-P@ss1!");
        var lonelyToken = await LoginAsync(client, "lonely-activity@example.com", "S3cure-P@ss1!");

        var response = await GetActivityResponseAsync(client, lonelyToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var activity = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        activity.GetProperty("items").GetArrayLength().Should().Be(0);
        activity.GetProperty("totalCount").GetInt32().Should().Be(0);
        activity.GetProperty("totalPages").GetInt32().Should().Be(0);
        activity.GetProperty("page").GetInt32().Should().Be(1);
    }
}
