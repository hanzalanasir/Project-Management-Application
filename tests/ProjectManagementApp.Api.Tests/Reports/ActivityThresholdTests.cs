using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Application.Common.Options;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Reports.ReportsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

// T064 — the 🎯 422 guard test (research R-5, DoD 7). A window whose scoped-and-filtered result set
// would exceed Reports:LargeReportRowThreshold must 422 BEFORE anything beyond a single probe row
// is ever materialized — a 422 returned after loading 10,000 rows protects the browser, not the
// server, and does not satisfy this task.
[Collection(ApiTestCollection.Name)]
public class ActivityThresholdTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ActivityThresholdTests(ApiTestFixture fixture) { _fixture = fixture; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task OverThresholdWindow_Returns422_AndNeverMaterializesTheFullPage_NarrowingThenReturns200()
    {
        // A tiny threshold (3) so the test seeds a handful of real rows rather than 10,000 —
        // Reports:LargeReportRowThreshold itself is just a number the guard compares against; the
        // mechanism under test doesn't care what that number is.
        var client = _fixture.CreateClient(services =>
            services.Configure<ReportsOptions>(o => o.LargeReportRowThreshold = 3));

        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        var (projectId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Threshold Project", null, "2026-08-01", null, null, null));

        // The ProjectCreated row above is #1. Five TaskCreated rows push the scoped total to 6 —
        // comfortably over the threshold of 3.
        for (var i = 0; i < 5; i++)
        {
            await CreateTaskAndGetIdEtagAsync(client, pmToken, projectId, new CreateTaskRequest($"T{i}", null, null, null, null));
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var windowQuery = $"?from={today.AddDays(-1):yyyy-MM-dd}&to={today:yyyy-MM-dd}&pageSize=20";

        _fixture.SqlCapture.Reset();
        var response = await GetActivityResponseAsync(client, pmToken, windowQuery);

        response.StatusCode.Should().Be((HttpStatusCode)422);

        // The real page-1 read (pageSize=20) must never have executed — only the guard's own
        // page=1,pageSize=1 probe. Npgsql renders .Take(n) as "LIMIT n"; a "LIMIT 20" anywhere in
        // the captured SQL would mean the guard ran AFTER materializing the full page, which
        // protects nothing.
        _fixture.SqlCapture.CommandTexts.Should().NotContain(sql => sql.Contains("LIMIT 20"),
            "the real paged read (pageSize=20) must never execute once the threshold guard has already failed");

        // Narrow: filtering to just Project-typed rows drops the scoped count to 1 (well under the
        // threshold of 3) — same window, same scope, a real 200.
        var narrowedResponse = await GetActivityResponseAsync(client, pmToken, windowQuery + "&entityType=Project");
        narrowedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
