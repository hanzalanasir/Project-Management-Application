using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Projects.ProjectsTestHelper;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Tasks;

public static class TasksTestHelper
{
    public sealed record CreateTaskRequest(string Title, string? Description, string? Priority, string? DueDate, Guid? AssigneeId);
    public sealed record UpdateTaskRequest(string Title, string? Description, string Priority, string? DueDate);
    public sealed record UpdateTaskStatusRequest(string Status);
    public sealed record ReassignTaskRequest(Guid? AssigneeId);

    // The standard 003 list-scenario fixture (quickstart.md's own fixture set): project A owned by
    // PM, project B owned by PM2, TM and TM2 both on A's team. T1 assigned TM, T2 assigned TM2, T3
    // unassigned — all on A. TM2 proves scope is by assignment, not membership (ADR-0007 §4).
    public sealed record ListScenario(
        Guid ProjectAId, Guid ProjectBId,
        string AdminToken, string PmToken, string Pm2Token, string TmToken, string Tm2Token,
        Guid TmId, Guid Tm2Id,
        Guid Task1Id, Guid Task2Id, Guid Task3Id);

    public static async Task<ListScenario> SeedListScenarioAsync(HttpClient client, ApiTestFixture fixture)
    {
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var pmToken = await LoginAsync(client, ProjectManagerEmail, ProjectManagerPassword);
        await RegisterProjectManagerAsync(client, adminToken, "PM Two Tasks", "pm2-tasks-list@example.com", "S3cure-P@ss1!");
        var pm2Token = await LoginAsync(client, "pm2-tasks-list@example.com", "S3cure-P@ss1!");
        var tmToken = await LoginAsync(client, TeamMemberEmail, TeamMemberPassword);
        var tmId = await GetCurrentUserIdAsync(client, tmToken);
        var tm2Id = await RegisterAndGetIdAsync(client, "TM Two Tasks", "tm2-tasks-list@example.com", "S3cure-P@ss1!");
        var tm2Token = await LoginAsync(client, "tm2-tasks-list@example.com", "S3cure-P@ss1!");

        var (projectAId, _) = await CreateProjectAsync(client, pmToken, new CreateProjectRequest("Task List Project A", null, "2026-08-01", null, null, null));
        var (projectBId, _) = await CreateProjectAsync(client, pm2Token, new CreateProjectRequest("Task List Project B", null, "2026-08-01", null, null, null));

        await AssignTeamMemberAsync(fixture.Services, projectAId, tmId);
        await AssignTeamMemberAsync(fixture.Services, projectAId, tm2Id);

        var t1 = await ReadJsonAsync(await CreateTaskAsync(client, pmToken, projectAId, new CreateTaskRequest("T1", null, null, null, tmId)));
        var t2 = await ReadJsonAsync(await CreateTaskAsync(client, pmToken, projectAId, new CreateTaskRequest("T2", null, null, null, tm2Id)));
        var t3 = await ReadJsonAsync(await CreateTaskAsync(client, pmToken, projectAId, new CreateTaskRequest("T3", null, null, null, null)));

        return new ListScenario(
            projectAId, projectBId, adminToken, pmToken, pm2Token, tmToken, tm2Token, tmId, tm2Id,
            Guid.Parse(t1.GetProperty("id").GetString()!),
            Guid.Parse(t2.GetProperty("id").GetString()!),
            Guid.Parse(t3.GetProperty("id").GetString()!));
    }

    public static async Task<HttpResponseMessage> CreateTaskAsync(HttpClient client, string token, Guid projectId, CreateTaskRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/projects/{projectId}/tasks")
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(httpRequest);
    }

    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    public static async Task<HttpResponseMessage> ListTasksByProjectAsync(HttpClient client, string token, Guid projectId, string? queryString = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{projectId}/tasks{queryString}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> ListTasksAsync(HttpClient client, string token, string? queryString = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/tasks{queryString}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> GetTaskByIdAsync(HttpClient client, string token, string id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/tasks/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static IEnumerable<Guid> Ids(JsonElement page) =>
        page.GetProperty("items").EnumerateArray().Select(i => Guid.Parse(i.GetProperty("id").GetString()!));

    public static async Task<HttpResponseMessage> PutTaskAsync(HttpClient client, string token, string id, string? ifMatch, UpdateTaskRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/tasks/{id}")
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (ifMatch is not null)
        {
            httpRequest.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        }
        return await client.SendAsync(httpRequest);
    }

    public static async Task<HttpResponseMessage> PutTaskStatusAsync(HttpClient client, string token, string id, string? ifMatch, object request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/tasks/{id}/status")
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (ifMatch is not null)
        {
            httpRequest.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        }
        return await client.SendAsync(httpRequest);
    }

    public static async Task<(string Id, string ETag)> CreateTaskAndGetIdEtagAsync(HttpClient client, string token, Guid projectId, CreateTaskRequest request)
    {
        var response = await CreateTaskAsync(client, token, projectId, request);
        var body = await ReadJsonAsync(response);
        return (body.GetProperty("id").GetString()!, response.Headers.ETag!.Tag);
    }

    public static async Task<HttpResponseMessage> DeleteTaskAsync(HttpClient client, string token, string id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/tasks/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> PutTaskAssigneeAsync(HttpClient client, string token, string id, string? ifMatch, ReassignTaskRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/tasks/{id}/assignee")
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (ifMatch is not null)
        {
            httpRequest.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        }
        return await client.SendAsync(httpRequest);
    }
}
