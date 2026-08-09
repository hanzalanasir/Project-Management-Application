using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

[Collection(ApiTestCollection.Name)]
public class DeleteProjectAuthorizationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public DeleteProjectAuthorizationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task DeleteProject_CrossOwnerPM_Returns403_AndNothingIsRemoved()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Delete", "pm2-delete@example.com", "S3cure-P@ss!");
        var pm2Token = await LoginAsync(client, "pm2-delete@example.com", "S3cure-P@ss!");
        var (id, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Not Yours To Delete", null, "2026-08-01", null, "Planning", null));

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/projects/{id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(deleteRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pm2Token);
        var getResponse = await client.SendAsync(getRequest);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteProject_AsAdmin_Succeeds_ForAnyProject()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var (id, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Admin Deletable", null, "2026-08-01", null, "Planning", null));

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/projects/{id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.SendAsync(deleteRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProject_AsTeamMember_Returns403()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var (id, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Not For TM Delete", null, "2026-08-01", null, "Planning", null));

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/projects/{id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tmToken);
        var response = await client.SendAsync(deleteRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
