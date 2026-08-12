using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T044 — the 🎯 UTC boundary test (research R-2/R-4, same guarantee 005's OverdueTimezoneTests
// proved for the Dashboard). A task closed at 23:30 UTC on a bucket boundary must land in the day
// UTC determines. Two complementary halves, matching 005 T050's precedent exactly:
//   1. A source scan proving no file under Features/Reports/ ever reads the local clock — the
//      mechanism that makes "re-run under a non-UTC process clock" a non-event, because there is no
//      local-clock read anywhere in the path for a timezone change to affect.
//   2. Honesty note: actually flipping the sandbox's OS-level timezone is a system-wide, hard-to-
//      reverse action out of scope for an automated test (verified empirically true for 005's own
//      T050 — TimeZoneInfo.Local on Windows reads the OS registry, not the TZ env var). The
//      guarantee is proven structurally here, not by empirically flipping the host clock.
[Collection(ApiTestCollection.Name)]
public class TaskCompletionTimezoneTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public TaskCompletionTimezoneTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public void NoSourceFileUnderFeaturesReports_ReadsTheLocalClock()
    {
        var repoRoot = FindRepoRoot();
        var reportsDir = Path.Combine(repoRoot, "src", "ProjectManagementApp.Application", "Features", "Reports");
        Directory.Exists(reportsDir).Should().BeTrue();

        var localClockTokens = new[] { "DateTime.Now", "DateTime.Today", "TimeZoneInfo.Local", "TimeZoneInfo.ConvertTime" };

        var offendingFiles = Directory.EnumerateFiles(reportsDir, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllLines(file)
                .Where(line => !line.TrimStart().StartsWith("//"))
                .Any(line => localClockTokens.Any(line.Contains)))
            .ToList();

        offendingFiles.Should().BeEmpty(
            "every bucket boundary and 'today' comparison must be computed from DateTimeOffset.UtcNow/UtcDateTime only — " +
            "any local-clock read here would let a deployment's OS timezone setting silently move a task into a different bucket");
    }

    [Fact]
    public void DateTimeOffsetUtcNow_OffsetIsAlwaysZero_RegardlessOfHostLocalTimezone()
    {
        DateTimeOffset.UtcNow.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task TaskClosedAt2330Utc_OnADayBoundary_LandsInTheUtcDeterminedBucket()
    {
        var client = _fixture.CreateClient();
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Boundary Project", null, "2026-08-01", null, null, null));

        var (taskId, etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest("Boundary", null, null, null, null));
        await PutTaskStatusAsync(client, pmToken, taskId, etag, new UpdateTaskStatusRequest("Done"));
        // 30 minutes before midnight UTC — a naive local-time conversion (e.g. UTC+1 or later)
        // would push this into the NEXT calendar day.
        await SetTaskClosedAtAsync(_fixture.Services, taskId, new DateTimeOffset(2026, 6, 15, 23, 30, 0, TimeSpan.Zero));

        var body = await GetTaskCompletionAsync(client, pmToken, "?from=2026-06-15&to=2026-06-15&groupBy=day&projectScope=" + projectId);

        var buckets = body.GetProperty("buckets").EnumerateArray().ToList();
        buckets.Should().ContainSingle();
        buckets[0].GetProperty("periodStart").GetString().Should().Be("2026-06-15");
        buckets[0].GetProperty("completedCount").GetInt32().Should().Be(1);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ProjectManagementApp.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Could not locate repo root (ProjectManagementApp.slnx not found).");
    }
}
