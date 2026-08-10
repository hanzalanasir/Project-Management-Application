using System.Net;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// research R-4: a project team is bounded and human-scale, so the ">50 items" PagedResult trigger
// never fires here. The response is a plain JSON array — no envelope, no ?page/?pageSize, and an
// empty team is 200 with [] (never 404 — an easy place to accidentally conflate "no rows" with
// "not found").
[Collection(ApiTestCollection.Name)]
public class GetProjectTeamShapeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public GetProjectTeamShapeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task GetProjectTeam_ResponseIsPlainArray_NotPagedResultEnvelope()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Shape Array", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));

        var response = await GetTeamAsync(client, pmToken, projectId);

        var body = await ReadJsonAsync(response);
        body.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetProjectTeam_IgnoresPageAndPageSizeQueryParameters()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Shape No Paging", null, "2026-08-01", null, null, null));
        var tmId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));
        await AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(tmId));

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{projectId}/team?page=2&pageSize=1");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", pmToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.ValueKind.Should().Be(JsonValueKind.Array);
        // Full roster, unaffected by the ignored page/pageSize params — still just the one member.
        body.GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task GetProjectTeam_EmptyTeam_Returns200_WithEmptyArray_NeverA404()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Shape Empty", null, "2026-08-01", null, null, null));

        var response = await GetTeamAsync(client, pmToken, projectId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.ValueKind.Should().Be(JsonValueKind.Array);
        body.GetArrayLength().Should().Be(0);
    }
}
