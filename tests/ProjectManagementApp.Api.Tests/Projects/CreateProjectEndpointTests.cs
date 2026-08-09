using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

[Collection(ApiTestCollection.Name)]
public class CreateProjectEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CreateProjectEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task CreateProject_AsProjectManager_Returns201_WithLocationAndETag_OwnedByCaller()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var meResponse = await client.SendAsync(meRequest);
        var meBody = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
        var pmId = meBody.GetProperty("id").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/projects")
        {
            Content = JsonContent.Create(new CreateProjectRequest("Apollo Rollout", "Regional launch", "2026-08-01", "2026-11-30", "Planning", null)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("/api/projects/");
        response.Headers.ETag.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Apollo Rollout");
        body.GetProperty("owner").GetProperty("id").GetString().Should().Be(pmId);
    }
}
