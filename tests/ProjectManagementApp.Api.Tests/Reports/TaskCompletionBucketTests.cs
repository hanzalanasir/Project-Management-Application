using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T043: the completion trend is a ZERO-FILLED CONTINUOUS series — a 4-week window with closures in
// only 2 of those weeks must still return exactly 4 buckets, two of them `completedCount: 0`, so a
// chart shows a real gap-free line rather than missing points.
[Collection(ApiTestCollection.Name)]
public class TaskCompletionBucketTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public TaskCompletionBucketTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task TaskCompletion_GroupByWeek_FourWeekWindow_TwoWeeksWithClosures_YieldsFourBuckets_TwoZero()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Bucket Project", null, "2026-08-01", null, null, null));

        // 2026-06-01 is a Monday — from/to below span exactly 4 ISO weeks (06-01..06-28).
        var (t1Id, t1Etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("W1", null, null, null, null));
        await PutTaskStatusAsync(client, pmToken, t1Id, t1Etag, new UpdateTaskStatusRequest("Done"));
        await SetTaskClosedAtAsync(_fixture.Services, t1Id, new DateTimeOffset(2026, 6, 3, 10, 0, 0, TimeSpan.Zero));

        var (t2Id, t2Etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("W3", null, null, null, null));
        await PutTaskStatusAsync(client, pmToken, t2Id, t2Etag, new UpdateTaskStatusRequest("Done"));
        await SetTaskClosedAtAsync(_fixture.Services, t2Id, new DateTimeOffset(2026, 6, 17, 10, 0, 0, TimeSpan.Zero));

        var body = await GetTaskCompletionAsync(client, pmToken, "?from=2026-06-01&to=2026-06-28&groupBy=week&projectScope=" + projectId);

        var buckets = body.GetProperty("buckets").EnumerateArray().ToList();
        buckets.Should().HaveCount(4);

        var counts = buckets.Select(b => b.GetProperty("completedCount").GetInt32()).ToList();
        counts.Should().BeEquivalentTo([1, 0, 1, 0], opts => opts.WithStrictOrdering());
        counts.Count(c => c == 0).Should().Be(2);
    }

    [Fact]
    public async Task TaskCompletion_InvalidGroupBy_Returns400()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var response = await GetTaskCompletionResponseAsync(client, pmToken, "?from=2026-06-01&to=2026-06-28&groupBy=fortnight");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TaskCompletion_MissingGroupBy_Returns400()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);

        var response = await GetTaskCompletionResponseAsync(client, pmToken, "?from=2026-06-01&to=2026-06-28");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
