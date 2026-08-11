using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Dashboard.DashboardTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

// T030: Admin sees all activity; PM only owned-project activity; TM only member-of-project
// activity. totalCount itself must be scoped — a TeamMember must never learn the system-wide
// activity volume from paging metadata, even if the individual rows are hidden.
[Collection(ApiTestCollection.Name)]
public class ActivityScopeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ActivityScopeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task Activity_Admin_SeesEverything_AcrossBothProjects_IncludingWhatPmAndPm2SeeIndividually()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "activity-scope-admin");
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);

        // Admin's Unscoped read also picks up global User-entity events (login/register/role
        // change) that scenario setup generates and that a scoped PM/PM2 never sees — so this
        // asserts a lower bound (every project-scoped row must be present), not an exact count.
        var pmActivity = await GetActivityAsync(client, scenario.PmToken, "?pageSize=100");
        var pm2Activity = await GetActivityAsync(client, scenario.Pm2Token, "?pageSize=100");
        var adminActivity = await GetActivityAsync(client, adminToken, "?pageSize=100");

        adminActivity.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(
            pmActivity.GetProperty("totalCount").GetInt32() + pm2Activity.GetProperty("totalCount").GetInt32());

        var adminEntityIds = adminActivity.GetProperty("items").EnumerateArray()
            .Select(i => i.GetProperty("entityId").GetString()).ToHashSet();
        foreach (var item in pmActivity.GetProperty("items").EnumerateArray())
        {
            adminEntityIds.Should().Contain(item.GetProperty("entityId").GetString());
        }
    }

    [Fact]
    public async Task Activity_ProjectManager_SeesOnlyOwnedProjectActivity()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "activity-scope-pm");

        var activity = await GetActivityAsync(client, scenario.PmToken, "?pageSize=100");

        activity.GetProperty("totalCount").GetInt32().Should().Be(11);
        foreach (var item in activity.GetProperty("items").EnumerateArray())
        {
            item.GetProperty("entityType").GetString().Should().BeOneOf("Project", "Task");
        }
    }

    [Fact]
    public async Task Activity_ProjectManager2_SeesOnlyItsOwnProjectCreation_NothingFromProjectA()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "activity-scope-pm2");

        var activity = await GetActivityAsync(client, scenario.Pm2Token, "?pageSize=100");

        // TotalCount itself is scoped, not just the items — PM2 must never learn Project A had 11
        // rows through the paging metadata.
        activity.GetProperty("totalCount").GetInt32().Should().Be(1);
        activity.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task Activity_TeamMember_SeesMemberOfProjectActivity_NotTheOtherProject()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedScopeScenarioAsync(client, _fixture, "activity-scope-tm");

        var activity = await GetActivityAsync(client, scenario.TmToken, "?pageSize=100");

        // TM is a member of A only — sees A's 11 rows, not B's, and never learns B exists via
        // totalCount either.
        activity.GetProperty("totalCount").GetInt32().Should().Be(11);
    }
}
