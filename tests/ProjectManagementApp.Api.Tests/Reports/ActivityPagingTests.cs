using System.Net;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T067: default page size 20; pageSize=500 clamps to 100 (never rejected); page=-1 -> 400;
// newest-first stable ordering.
[Collection(ApiTestCollection.Name)]
public class ActivityPagingTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ActivityPagingTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static string WindowQuery()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return $"?from={today.AddDays(-1):yyyy-MM-dd}&to={today:yyyy-MM-dd}";
    }

    [Fact]
    public async Task NoPageSizeSupplied_DefaultsToTwenty()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var body = await GetActivityAsync(client, pmToken, WindowQuery());
        body.GetProperty("pageSize").GetInt32().Should().Be(20);
    }

    [Fact]
    public async Task PageSize500_ClampsToMaximumOfOneHundred_NotRejected()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var response = await GetActivityResponseAsync(client, pmToken, WindowQuery() + "&pageSize=500");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await GetActivityAsync(client, pmToken, WindowQuery() + "&pageSize=500");
        body.GetProperty("pageSize").GetInt32().Should().Be(100);
    }

    [Fact]
    public async Task NegativePage_Returns400()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var response = await GetActivityResponseAsync(client, pmToken, WindowQuery() + "&page=-1");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Items_AreOrderedNewestFirst()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Paging Order Project", null, "2026-08-01", null, null, null));

        await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("First", null, null, null, null));
        await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Second", null, null, null, null));
        await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Third", null, null, null, null));

        var body = await GetActivityAsync(client, pmToken, WindowQuery() + "&entityType=Task");
        var timestamps = body.GetProperty("items").EnumerateArray()
            .Select(i => DateTimeOffset.Parse(i.GetProperty("timestamp").GetString()!))
            .ToList();

        timestamps.Should().BeInDescendingOrder();
    }
}
