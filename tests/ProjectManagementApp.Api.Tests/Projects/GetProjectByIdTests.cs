using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

[Collection(ApiTestCollection.Name)]
public class GetProjectByIdTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public GetProjectByIdTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task GetProjectById_InScope_Returns200_WithETag()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (id, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Owned", null, "2026-08-01", null, null, null));

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.ETag.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProjectById_OutOfScope_Returns403()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Detail", "pm2-detail@example.com", "S3cure-P@ss!");
        var pm2Token = await LoginAsync(client, "pm2-detail@example.com", "S3cure-P@ss!");
        var (id, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Not Yours", null, "2026-08-01", null, null, null));

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProjectById_UnknownId_Returns404()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectById_MalformedId_Returns400()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects/not-a-guid");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProjectById_AsAdmin_Returns200_ForAnyProject()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var (id, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Someone Else's", null, "2026-08-01", null, null, null));

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
