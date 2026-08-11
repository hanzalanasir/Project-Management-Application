using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// The 🎯 zero-scope test (T021, DoD 5, FR-007). A caller whose visible-project set is empty
// (LONELY — on no team anywhere) receives 200 with all counts zero and every breakdown present.
// Not 403, not 404, not an empty body — the dashboard names no resource (research R-3).
[Collection(ApiTestCollection.Name)]
public class SummaryEmptyScopeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public SummaryEmptyScopeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Summary_LonelyTeamMember_Returns200_AllZeros_NeverForbiddenOrNotFound()
    {
        var client = _fixture.CreateClient();
        await RegisterAndGetIdAsync(client, "Lonely", "lonely-empty-scope@example.com", "S3cure-P@ss1!");
        var lonelyToken = await LoginAsync(client, "lonely-empty-scope@example.com", "S3cure-P@ss1!");

        var response = await GetSummaryResponseAsync(client, lonelyToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await GetSummaryAsync(client, lonelyToken);
        summary.GetProperty("scope").GetString().Should().Be("TeamMember");
        summary.GetProperty("visibleProjectCount").GetInt32().Should().Be(0);
        summary.GetProperty("overdueTaskCount").GetInt32().Should().Be(0);
        summary.GetProperty("completionRate").GetDouble().Should().Be(0);
        summary.GetProperty("blockedTaskCount").GetInt32().Should().Be(0);
        summary.GetProperty("visibleTeamMemberCount").GetInt32().Should().Be(0);

        foreach (var key in new[] { "Planning", "Active", "OnHold", "Completed", "Cancelled" })
        {
            summary.GetProperty("projectsByStatus").GetProperty(key).GetInt32().Should().Be(0);
        }

        foreach (var key in new[] { "ToDo", "InProgress", "InReview", "Done", "Blocked" })
        {
            summary.GetProperty("tasksByStatus").GetProperty(key).GetInt32().Should().Be(0);
        }

        var personalTasks = summary.GetProperty("personalTasks");
        personalTasks.GetProperty("assignedTotal").GetInt32().Should().Be(0);
        personalTasks.GetProperty("overdueCount").GetInt32().Should().Be(0);
    }
}
