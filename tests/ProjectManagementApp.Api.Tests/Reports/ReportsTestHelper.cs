using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Reports;

public static class ReportsTestHelper
{
    // The shared scope-matrix scenario (T032/T033/T036 and, later, T054-058, T064-068): Project A
    // owned by PM (TM + TM2 on its team), Project B owned by PM2 (no team, no tasks — an owner with
    // nothing still gets 200 with empty rows, never 403). Mirrors DashboardTestHelper's identical
    // shape so the scope semantics stay directly comparable to 005 (V6 parity).
    public sealed record ScopeScenario(
        Guid ProjectAId, Guid ProjectBId,
        string AdminToken, string PmToken, string Pm2Token, string TmToken, string Tm2Token,
        Guid TmId, Guid Tm2Id);

    public static async Task<ScopeScenario> SeedScopeScenarioAsync(HttpClient client, ApiTestFixture fixture, string suffix)
    {
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Reports", $"pm2-rep-{suffix}@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, $"pm2-rep-{suffix}@example.com", "S3cure-P@ss1!");
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        var tm2Id = await RegisterAndGetIdAsync(client, "TM Two Reports", $"tm2-rep-{suffix}@example.com", "S3cure-P@ss1!");
        var tm2Token = await LoginAsync(client, $"tm2-rep-{suffix}@example.com", "S3cure-P@ss1!");

        var (projectAId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest($"Reports Project A {suffix}", null, "2026-08-01", null, null, null));
        var (projectBId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest($"Reports Project B {suffix}", null, "2026-08-01", null, null, null));

        await AssignTeamMemberAsync(fixture.Services, projectAId, tmId);
        await AssignTeamMemberAsync(fixture.Services, projectAId, tm2Id);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1).ToString("yyyy-MM-dd");

        // One overdue-and-open task and one closed (Done "now") task on A — enough for the scope
        // matrix (T032) to see non-zero rows, and for T033's projectScope tests to have real data
        // to compare against. Metric-specific counts (exactly 3-of-12, projected completion, etc.)
        // are seeded per-test in their own dedicated projects, not here.
        var (t1Id, t1Etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectAId, new CreateTaskRequest("RA-Overdue", null, null, yesterday, tmId));
        _ = t1Etag;

        var (t2Id, t2Etag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectAId, new CreateTaskRequest("RA-Closed", null, null, null, tmId));
        await PutTaskStatusAsync(client, pmToken, t2Id, t2Etag, new UpdateTaskStatusRequest("Done"));

        return new ScopeScenario(projectAId, projectBId, adminToken, pmToken, pm2Token, tmToken, tm2Token, tmId, tm2Id);
    }

    public static async Task<HttpResponseMessage> GetCatalogResponseAsync(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports/catalog");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<JsonElement> GetCatalogAsync(HttpClient client, string token)
    {
        var response = await GetCatalogResponseAsync(client, token);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task<HttpResponseMessage> GetProjectProgressResponseAsync(HttpClient client, string token, string queryString)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/reports/project-progress{queryString}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<JsonElement> GetProjectProgressAsync(HttpClient client, string token, string queryString)
    {
        var response = await GetProjectProgressResponseAsync(client, token, queryString);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static string DefaultWindowQuery(DateOnly? from = null, DateOnly? to = null)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var f = from ?? today.AddDays(-6);
        var t = to ?? today;
        return $"?from={f:yyyy-MM-dd}&to={t:yyyy-MM-dd}";
    }

    public static async Task<HttpResponseMessage> GetTaskCompletionResponseAsync(HttpClient client, string token, string queryString)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/reports/task-completion{queryString}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<JsonElement> GetTaskCompletionAsync(HttpClient client, string token, string queryString)
    {
        var response = await GetTaskCompletionResponseAsync(client, token, queryString);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task<HttpResponseMessage> GetTeamPerformanceResponseAsync(HttpClient client, string token, string queryString)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/reports/team-performance{queryString}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<JsonElement> GetTeamPerformanceAsync(HttpClient client, string token, string queryString)
    {
        var response = await GetTeamPerformanceResponseAsync(client, token, queryString);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task<HttpResponseMessage> GetActivityResponseAsync(HttpClient client, string token, string queryString)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/reports/activity{queryString}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<JsonElement> GetActivityAsync(HttpClient client, string token, string queryString)
    {
        var response = await GetActivityResponseAsync(client, token, queryString);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    /// <summary>
    /// Overwrites a task's <c>closed_at</c> directly via the DbContext, bypassing the real-clock
    /// value the status-transition endpoint always stamps. Task Completion's bucketing tests need
    /// deterministic historical closure dates spread across known weeks/months — there is no API
    /// path to backdate a closure, so this is the same ADR-0007 §4 escape hatch already used for
    /// team-membership seeding, applied to a column instead of a whole row. Production code never
    /// does this — 003's status handler is the only writer of `closed_at` at runtime.
    /// </summary>
    public static async Task SetTaskClosedAtAsync(IServiceProvider services, string taskId, DateTimeOffset closedAt)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var task = await db.Tasks.SingleAsync(t => t.Id == Guid.Parse(taskId));
        task.ClosedAt = closedAt;
        await db.SaveChangesAsync();
    }
}
