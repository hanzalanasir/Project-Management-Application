using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

[Collection(ApiTestCollection.Name)]
public class UpdateProjectOwnershipTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateProjectOwnershipTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task UpdateProject_OwnershipTransfer_AsProjectManager_Returns403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var pm2Id = await RegisterProjectManagerAsync(client, adminToken, "PM Two Transfer", "pm2-transfer@example.com", "S3cure-P@ss!");
        var (id, etag) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Transfer Attempt", null, "2026-08-01", null, "Planning", null));

        var response = await PutProjectAsync(client, pmToken, id, etag, new UpdateProjectRequest("Transfer Attempt", null, "2026-08-01", null, "Planning", pm2Id));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateProject_OwnershipTransfer_AsAdmin_ToATeamMember_Returns400()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var teamMemberId = await RegisterAndGetIdAsync(client, "TM For Transfer", "tm-transfer@example.com", "S3cure-P@ss!");
        var (id, etag) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Transfer To TM", null, "2026-08-01", null, "Planning", null));

        var response = await PutProjectAsync(client, adminToken, id, etag, new UpdateProjectRequest("Transfer To TM", null, "2026-08-01", null, "Planning", teamMemberId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errors").TryGetProperty("ownerId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProject_OwnershipTransfer_AsAdmin_ToAnEligibleOwner_Succeeds()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var pm2Id = await RegisterProjectManagerAsync(client, adminToken, "PM Two Eligible", "pm2-eligible@example.com", "S3cure-P@ss!");
        var (id, etag) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Transfer To PM2", null, "2026-08-01", null, "Planning", null));

        var response = await PutProjectAsync(client, adminToken, id, etag, new UpdateProjectRequest("Transfer To PM2", null, "2026-08-01", null, "Planning", pm2Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("owner").GetProperty("id").GetString().Should().Be(pm2Id.ToString());
    }
}
