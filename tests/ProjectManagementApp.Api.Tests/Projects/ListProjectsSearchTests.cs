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
public class ListProjectsSearchTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ListProjectsSearchTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ListProjects_SearchMatchesInteriorSubstring_ViaTrigramIndex()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Apollo Rollout", null, "2026-08-01", null, null, null));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects?search=pollo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("name").GetString())
            .Should().Contain("Apollo Rollout");
    }

    [Fact]
    public async Task ListProjects_StatusFilter_NarrowsWithinScope()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Planning One", null, "2026-08-01", null, "Planning", null));
        await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Active One", null, "2026-08-01", null, "Active", null));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects?status=Active");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalCount").GetInt32().Should().Be(1);
        body.GetProperty("items")[0].GetProperty("status").GetString().Should().Be("Active");
    }

    [Fact]
    public async Task ListProjects_SearchNeverWidensScope_PM2SearchingPMsProjectFindsNothing()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Search", "pm2-search@example.com", "S3cure-P@ss!");
        var pm2Token = await LoginAsync(client, "pm2-search@example.com", "S3cure-P@ss!");
        await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Apollo Rollout", null, "2026-08-01", null, null, null));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects?search=pollo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pm2Token);
        var response = await client.SendAsync(request);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task ListProjects_UnrecognizedSort_Returns400()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects?sort=DROP+TABLE");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
