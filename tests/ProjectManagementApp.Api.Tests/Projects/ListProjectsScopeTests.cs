using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

// The primary acceptance test of 002's headline security property (quickstart V4). Asserts
// totalCount, not just items — a mismatch would mean the scope filter ran after counting,
// leaking system-wide existence through paging metadata (FR-007).
[Collection(ApiTestCollection.Name)]
public class ListProjectsScopeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ListProjectsScopeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ListProjects_ThreeRoleScopeMatrix_ItemsAndTotalCountAreBothScoped()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var pm2Id = await RegisterProjectManagerAsync(client, adminToken, "PM Two", "pm2-list-scope@example.com", "S3cure-P@ss!");
        var pm2Token = await LoginAsync(client, "pm2-list-scope@example.com", "S3cure-P@ss!");
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);

        var (projectAId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Project A", null, "2026-08-01", null, null, null));
        var (projectBId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Project B", null, "2026-08-01", null, null, null));
        await AssignTeamMemberAsync(_fixture.Services, projectAId, tmId);

        var adminList = await GetListAsync(client, adminToken);
        adminList.GetProperty("totalCount").GetInt32().Should().Be(2);
        Ids(adminList).Should().BeEquivalentTo(new[] { projectAId, projectBId });

        var pmList = await GetListAsync(client, pmToken);
        pmList.GetProperty("totalCount").GetInt32().Should().Be(1);
        Ids(pmList).Should().BeEquivalentTo(new[] { projectAId });

        var pm2List = await GetListAsync(client, pm2Token);
        pm2List.GetProperty("totalCount").GetInt32().Should().Be(1);
        Ids(pm2List).Should().BeEquivalentTo(new[] { projectBId });

        var tmList = await GetListAsync(client, tmToken);
        tmList.GetProperty("totalCount").GetInt32().Should().Be(1);
        Ids(tmList).Should().BeEquivalentTo(new[] { projectAId });
    }

    private static IEnumerable<Guid> Ids(JsonElement page) =>
        page.GetProperty("items").EnumerateArray().Select(i => Guid.Parse(i.GetProperty("id").GetString()!));

    private static async Task<JsonElement> GetListAsync(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
