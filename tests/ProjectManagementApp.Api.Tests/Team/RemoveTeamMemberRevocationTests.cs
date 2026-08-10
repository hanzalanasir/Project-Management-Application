using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// Mirrors 003's stage-4 T090 access-revocation proof (ReassignTaskRevocationTests): after removal
// the user's very next request re-evaluates scope fresh — no grace period, no cached access.
// Verified live against real Postgres, never mocked.
[Collection(ApiTestCollection.Name)]
public class RemoveTeamMemberRevocationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RemoveTeamMemberRevocationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task RemoveTeamMember_TheirNextGetOnTheProject_Returns403_Immediately_AndTheProjectDisappearsFromTheirList()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Revocation Project", null, "2026-08-01", null, null, null));
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));

        // Sanity: TM can read the project and see it listed before removal.
        (await GetProjectAsync(client, tmToken, projectId)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ListProjectIdsAsync(client, tmToken)).Should().Contain(projectId);

        var removeResponse = await RemoveTeamMemberAsync(client, pmToken, projectId, tmId);
        removeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var postCheck = await GetProjectAsync(client, tmToken, projectId);
        postCheck.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        (await ListProjectIdsAsync(client, tmToken)).Should().NotContain(projectId);
    }

    private static async Task<HttpResponseMessage> GetProjectAsync(HttpClient client, string token, Guid projectId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{projectId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    private static async Task<List<Guid>> ListProjectIdsAsync(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects?pageSize=100");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);
        var body = await ReadJsonAsync(response);
        return body.GetProperty("items").EnumerateArray().Select(p => Guid.Parse(p.GetProperty("id").GetString()!)).ToList();
    }
}
