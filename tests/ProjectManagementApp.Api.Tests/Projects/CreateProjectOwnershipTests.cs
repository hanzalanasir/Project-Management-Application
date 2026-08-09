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
public class CreateProjectOwnershipTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public CreateProjectOwnershipTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task CreateProject_AsProjectManager_SupplyingAnotherUsersOwnerId_IsIgnored_OwnerIsStillCaller()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var meBody = await (await client.SendAsync(meRequest)).Content.ReadFromJsonAsync<JsonElement>();
        var pmId = meBody.GetProperty("id").GetString();

        using var adminMeRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        adminMeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var adminMeBody = await (await client.SendAsync(adminMeRequest)).Content.ReadFromJsonAsync<JsonElement>();
        var adminId = adminMeBody.GetProperty("id").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/projects")
        {
            Content = JsonContent.Create(new CreateProjectRequest("Forged Owner", null, "2026-08-01", null, null, Guid.Parse(adminId!))),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("owner").GetProperty("id").GetString().Should().Be(pmId);
    }

    [Fact]
    public async Task CreateProject_AsAdmin_MaySetAnEligibleOwner()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pm2Id = await RegisterProjectManagerAsync(client, adminToken, "PM Two", "pm2-ownership@example.com", "S3cure-P@ss!");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/projects")
        {
            Content = JsonContent.Create(new CreateProjectRequest("Admin-Assigned", null, "2026-08-01", null, null, pm2Id)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("owner").GetProperty("id").GetString().Should().Be(pm2Id.ToString());
    }

    [Fact]
    public async Task CreateProject_AsAdmin_SettingATeamMemberAsOwner_Returns400()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var teamMemberId = await RegisterAndGetIdAsync(client, "New Member", "new-member-ownership@example.com", "S3cure-P@ss!");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/projects")
        {
            Content = JsonContent.Create(new CreateProjectRequest("Ineligible Owner", null, "2026-08-01", null, null, teamMemberId)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errors").TryGetProperty("ownerId", out _).Should().BeTrue();
    }
}
