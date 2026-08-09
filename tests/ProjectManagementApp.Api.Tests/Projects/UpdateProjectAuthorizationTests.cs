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
public class UpdateProjectAuthorizationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public UpdateProjectAuthorizationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task UpdateProject_CrossOwnerPM_Returns403_AndProjectBIsUnchanged()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Update", "pm2-update@example.com", "S3cure-P@ss!");
        var pm2Token = await LoginAsync(client, "pm2-update@example.com", "S3cure-P@ss!");
        var (bId, bEtag) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Project B", null, "2026-08-01", null, "Planning", null));

        var response = await PutProjectAsync(client, pmToken, bId, bEtag, new UpdateProjectRequest("Hijacked", null, "2026-08-01", null, "Active", null));
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{bId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pm2Token);
        var body = await (await client.SendAsync(getRequest)).Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Project B");
    }

    [Fact]
    public async Task UpdateProject_AsAdmin_Succeeds_ForAnyProject()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var (id, etag) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Admin Editable", null, "2026-08-01", null, "Planning", null));

        var response = await PutProjectAsync(client, adminToken, id, etag, new UpdateProjectRequest("Admin Edited", null, "2026-08-01", null, "Active", null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateProject_AsTeamMember_Returns403_AtTheRoleGate()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var (id, etag) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Not For TM", null, "2026-08-01", null, "Planning", null));

        var response = await PutProjectAsync(client, tmToken, id, etag, new UpdateProjectRequest("Should Not Apply", null, "2026-08-01", null, "Active", null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
