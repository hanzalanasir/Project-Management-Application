using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Tasks.TasksTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Dashboard;

public static class DashboardTestHelper
{
    // The shared scope-matrix scenario reused across T019-T022, T027, T042, T043: Project A owned
    // by PM (TM + TM2 on its team), Project B owned by PM2 (no team, no tasks — proves an owner
    // with nothing still gets 200 with zeros, not the LONELY case). Six tasks on A, spanning every
    // TaskStatus with a deliberate overdue/Done/no-due-date mix:
    //   Ta ToDo,       assignee TM,  due yesterday  -> overdue
    //   Tb InProgress, assignee TM,  due +7 days    -> not overdue
    //   Tc Done,       assignee TM,  due yesterday  -> Done excludes it regardless
    //   Td ToDo,       assignee TM2, due (none)     -> never overdue; NOT counted for TM's personal view
    //   Te InReview,   unassigned,   due today      -> not overdue (boundary)
    //   Tf Blocked,    unassigned,   due yesterday  -> overdue
    //
    // TM's personal slice (assignee == TM, within A): ToDo=1, InProgress=1, Done=1, total=3, overdue=1.
    // Project-wide (Admin/PM) on A: ToDo=2, InProgress=1, Done=1, InReview=1, Blocked=1, total=6, overdue=2.
    public sealed record ScopeScenario(
        Guid ProjectAId, Guid ProjectBId,
        string AdminToken, string PmToken, string Pm2Token, string TmToken, string Tm2Token,
        Guid TmId, Guid Tm2Id,
        string TaId, string TbId, string TcId, string TdId, string TeId, string TfId);

    public static async Task<ScopeScenario> SeedScopeScenarioAsync(HttpClient client, ApiTestFixture fixture, string suffix)
    {
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Dashboard", $"pm2-dash-{suffix}@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, $"pm2-dash-{suffix}@example.com", "S3cure-P@ss1!");
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        var tm2Id = await RegisterAndGetIdAsync(client, "TM Two Dashboard", $"tm2-dash-{suffix}@example.com", "S3cure-P@ss1!");
        var tm2Token = await LoginAsync(client, $"tm2-dash-{suffix}@example.com", "S3cure-P@ss1!");

        var (projectAId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest($"Dashboard Project A {suffix}", null, "2026-08-01", null, null, null));
        var (projectBId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest($"Dashboard Project B {suffix}", null, "2026-08-01", null, null, null));

        await AssignTeamMemberAsync(fixture.Services, projectAId, tmId);
        await AssignTeamMemberAsync(fixture.Services, projectAId, tm2Id);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1).ToString("yyyy-MM-dd");
        var todayStr = today.ToString("yyyy-MM-dd");
        var nextWeek = today.AddDays(7).ToString("yyyy-MM-dd");

        var (taId, taEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectAId, new CreateTaskRequest("Ta", null, null, yesterday, tmId));

        var (tbId, tbEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectAId, new CreateTaskRequest("Tb", null, null, nextWeek, tmId));
        tbEtag = (await PutTaskStatusAsync(client, pmToken, tbId, tbEtag, new UpdateTaskStatusRequest("InProgress"))).Headers.ETag!.Tag;

        var (tcId, tcEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectAId, new CreateTaskRequest("Tc", null, null, yesterday, tmId));
        tcEtag = (await PutTaskStatusAsync(client, pmToken, tcId, tcEtag, new UpdateTaskStatusRequest("Done"))).Headers.ETag!.Tag;

        var (tdId, tdEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectAId, new CreateTaskRequest("Td", null, null, null, tm2Id));

        var (teId, teEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectAId, new CreateTaskRequest("Te", null, null, todayStr, null));
        teEtag = (await PutTaskStatusAsync(client, pmToken, teId, teEtag, new UpdateTaskStatusRequest("InReview"))).Headers.ETag!.Tag;

        var (tfId, tfEtag) = await CreateTaskAndGetIdEtagAsync(client, pmToken, projectAId, new CreateTaskRequest("Tf", null, null, yesterday, null));
        tfEtag = (await PutTaskStatusAsync(client, pmToken, tfId, tfEtag, new UpdateTaskStatusRequest("Blocked"))).Headers.ETag!.Tag;

        return new ScopeScenario(
            projectAId, projectBId, adminToken, pmToken, pm2Token, tmToken, tm2Token, tmId, tm2Id,
            taId, tbId, tcId, tdId, teId, tfId);
    }

    public static async Task<JsonElement> GetSummaryAsync(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard/summary");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task<HttpResponseMessage> GetSummaryResponseAsync(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard/summary");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> GetActivityResponseAsync(HttpClient client, string token, string? queryString = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/dashboard/activity{queryString}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<JsonElement> GetActivityAsync(HttpClient client, string token, string? queryString = null)
    {
        var response = await GetActivityResponseAsync(client, token, queryString);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
