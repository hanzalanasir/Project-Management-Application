using System.Net.Http.Headers;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

// T080: proves NFR-002's "no N+1 on owner" against the real SQL executed, not by reading the
// handler and assuming .Include() worked (mirrors StatelessAuthTests' CommandCounter pattern).
//
// The handler issues exactly TWO commands by design (data-model.md §5 step 5 CountAsync, step 8
// materialize) — that is not itself the N+1 concern. The concern is a THIRD (or more) command per
// distinct project owner, which .Include(p => p.Owner) is meant to fold into the step-8 query as a
// JOIN. Proven here by using three DIFFERENT owners: an N+1 implementation would show 2 + 3 = 5+
// commands; the correct one stays at exactly 2 regardless of how many distinct owners exist.
[Collection(ApiTestCollection.Name)]
public class ListProjectsNoN1OnOwnerTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ListProjectsNoN1OnOwnerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ListProjects_WithThreeDistinctOwners_ExecutesExactlyTwoSqlCommands_NotOnePerOwner()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two N1", "pm2-n1@example.com", "S3cure-P@ss!");
        var pm2Token = await LoginAsync(client, "pm2-n1@example.com", "S3cure-P@ss!");

        await CreateProjectAsync(client, pmToken, new CreateProjectRequest("N1 Project A", null, "2026-08-01", null, "Planning", null));
        await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("N1 Project B", null, "2026-08-01", null, "Planning", null));
        await CreateProjectAsync(client, adminToken, new CreateProjectRequest("N1 Project C", null, "2026-08-01", null, "Planning", null));

        _fixture.CommandCounter.Reset();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        _fixture.CommandCounter.Count.Should().Be(2,
            "CountAsync + the owner-joined page fetch is exactly two commands regardless of how many distinct owners the three projects have — a per-owner lookup would push this past 2 (NFR-002)");
    }
}
