using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T054 — the 🎯 self-only clamp test, the sharpest least-privilege boundary in this feature
// (research R-6, DoD 3). A TeamMember naming a colleague's userId must receive exactly ONE row —
// their own — with status 200, never 403. A 403 would confirm the colleague exists and is out of
// scope, which a peer-comparison report must never leak.
[Collection(ApiTestCollection.Name)]
public class TeamPerformanceSelfOnlyTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public TeamPerformanceSelfOnlyTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task TeamMember_NamingAColleaguesUserId_GetsExactlyOneRow_TheirOwn_Status200_Never403()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        var tm2Id = await RegisterAndGetIdAsync(client, "TM2 SelfOnly", "tm2-selfonly@example.com", "S3cure-P@ss1!");

        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("SelfOnly Project", null, "2026-08-01", null, null, null));
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);
        await AssignTeamMemberAsync(_fixture.Services, projectId, tm2Id);

        var response = await GetTeamPerformanceResponseAsync(client, tmToken, DefaultWindowQuery() + $"&userId={tm2Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK, "a 403 here would confirm the colleague exists and is out of scope");

        var body = await GetTeamPerformanceAsync(client, tmToken, DefaultWindowQuery() + $"&userId={tm2Id}");
        var rows = body.GetProperty("rows").EnumerateArray().ToList();
        rows.Should().ContainSingle();
        rows[0].GetProperty("userId").GetString().Should().Be(tmId.ToString());
    }

    [Fact]
    public async Task TeamMember_NoUserIdSupplied_StillGetsExactlyOwnRow()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);

        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("SelfOnly NoParam Project", null, "2026-08-01", null, null, null));
        await AssignTeamMemberAsync(_fixture.Services, projectId, tmId);

        var body = await GetTeamPerformanceAsync(client, tmToken, DefaultWindowQuery());
        var rows = body.GetProperty("rows").EnumerateArray().ToList();
        rows.Should().ContainSingle();
        rows[0].GetProperty("userId").GetString().Should().Be(tmId.ToString());
    }
}
