using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

// The headline test of this stage (quickstart V5): scope is by ASSIGNMENT, not membership. TM and
// TM2 are both on project A's team, but each sees only the task assigned to them.
[Collection(ApiTestCollection.Name)]
public class ListTasksScopeTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ListTasksScopeTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task ListTasks_ThreeRoleScopeMatrix_TmSeesOnlyT1_Tm2SeesOnlyT2_DespiteBothOnProjectATeam()
    {
        var client = _fixture.CreateClient();
        var scenario = await SeedListScenarioAsync(client, _fixture);

        var adminBody = await ReadJsonAsync(await ListTasksAsync(client, scenario.AdminToken));
        adminBody.GetProperty("totalCount").GetInt32().Should().Be(3);
        Ids(adminBody).Should().BeEquivalentTo(new[] { scenario.Task1Id, scenario.Task2Id, scenario.Task3Id });

        var pmBody = await ReadJsonAsync(await ListTasksAsync(client, scenario.PmToken));
        pmBody.GetProperty("totalCount").GetInt32().Should().Be(3);
        Ids(pmBody).Should().BeEquivalentTo(new[] { scenario.Task1Id, scenario.Task2Id, scenario.Task3Id });

        var pm2Body = await ReadJsonAsync(await ListTasksAsync(client, scenario.Pm2Token));
        pm2Body.GetProperty("totalCount").GetInt32().Should().Be(0);

        var tmBody = await ReadJsonAsync(await ListTasksAsync(client, scenario.TmToken));
        tmBody.GetProperty("totalCount").GetInt32().Should().Be(1);
        Ids(tmBody).Should().BeEquivalentTo(new[] { scenario.Task1Id });

        var tm2Body = await ReadJsonAsync(await ListTasksAsync(client, scenario.Tm2Token));
        tm2Body.GetProperty("totalCount").GetInt32().Should().Be(1);
        Ids(tm2Body).Should().BeEquivalentTo(new[] { scenario.Task2Id });
    }
}
