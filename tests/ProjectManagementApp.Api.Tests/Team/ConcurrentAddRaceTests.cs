using System.Linq;
using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Team.TeamTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Team;

// The assertion that separates a real guarantee from a TOCTOU pre-check (quickstart V8, DoD 4/6):
// two genuinely simultaneous adds of the same (project, user) must resolve to exactly one 201 and
// one 409, NEVER a 500 — the losing request's SQLSTATE 23505 must be caught and mapped, not escape
// as an unhandled DbUpdateException. Meaningless on EF InMemory (no unique-constraint enforcement
// there), so this runs against ApiTestFixture's real Testcontainers Postgres.
//
// Placed in Api.Tests rather than the literal Infrastructure.Tests path tasks.md names: asserting
// actual HTTP status codes (201/409, never 500) requires a running API host, and
// Infrastructure.Tests has no project reference to ProjectManagementApp.Api to host one — only
// Api.Tests's ApiTestFixture can produce a real HTTP response to assert against. The DB-level half
// of this guarantee (the 23505 mapping itself) is already covered by Infrastructure.Tests's
// UniqueViolationMapperTests; this test proves the two other pieces together: real concurrent
// contention under load, and that the API layer never lets the loser's exception surface as a 500.
[Collection(ApiTestCollection.Name)]
public class ConcurrentAddRaceTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ConcurrentAddRaceTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task TwoSimultaneousAdds_OfTheSameProjectAndUser_ResolveToOne201AndOne409_NeverA500()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Race Project", null, "2026-08-01", null, null, null));
        var targetId = await GetCurrentUserIdAsync(client, await LoginAsync(client, TeamMemberEmail, TeamMemberPassword));

        var results = await Task.WhenAll(
            AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(targetId)),
            AddTeamMemberAsync(client, pmToken, projectId, new AddTeamMemberRequest(targetId)));

        results.Should().NotContain(r => (int)r.StatusCode >= 500, "the losing side of the race must be caught and mapped to 409, never escape as a 500");
        results.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
        results.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(1);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var rowCount = await db.TeamMembers.CountAsync(m => m.ProjectId == projectId && m.UserId == targetId);
        rowCount.Should().Be(1);
    }
}
