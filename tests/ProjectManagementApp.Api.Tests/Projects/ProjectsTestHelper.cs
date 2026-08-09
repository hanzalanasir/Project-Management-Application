using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Projects;

public static class ProjectsTestHelper
{
    public sealed record CreateProjectRequest(string Name, string? Description, string StartDate, string? EndDate, string? Status, Guid? OwnerId);
    public sealed record UpdateProjectRequest(string Name, string? Description, string StartDate, string? EndDate, string Status, Guid? OwnerId);

    /// <summary>Registers a new user (always provisioned TeamMember) then promotes them to ProjectManager as Admin — the only way to get a SECOND ProjectManager for cross-owner tests (ADR-0007 §4).</summary>
    public static async Task<Guid> RegisterProjectManagerAsync(HttpClient client, string adminToken, string fullName, string email, string password)
    {
        var userId = await RegisterAndGetIdAsync(client, fullName, email, password);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var getResponse = await client.SendAsync(getRequest);
        var etag = getResponse.Headers.ETag!.Tag;

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{userId}/role")
        {
            Content = JsonContent.Create(new { role = "ProjectManager" }),
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        putRequest.Headers.TryAddWithoutValidation("If-Match", etag);
        await client.SendAsync(putRequest);

        return userId;
    }

    public static async Task<(Guid Id, string ETag)> CreateProjectAsync(HttpClient client, string token, CreateProjectRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/projects")
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(httpRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (Guid.Parse(body.GetProperty("id").GetString()!), response.Headers.ETag!.Tag);
    }

    public static async Task<HttpResponseMessage> PutProjectAsync(HttpClient client, string token, Guid id, string ifMatch, UpdateProjectRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/projects/{id}")
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequest.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        return await client.SendAsync(httpRequest);
    }

    public static async Task<Guid> GetCurrentUserIdAsync(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var body = await (await client.SendAsync(request)).Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(body.GetProperty("id").GetString()!);
    }

    /// <summary>
    /// Assigns a user to a project's team directly via the DbContext (004's roster endpoints don't
    /// exist yet in this feature set — ADR-0007 §4 permits direct seeding in tests when no API path
    /// exists to reach the required state).
    /// </summary>
    public static async Task AssignTeamMemberAsync(IServiceProvider services, Guid projectId, Guid userId)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.TeamMembers.Add(new Domain.Entities.TeamMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(CancellationToken.None);
    }
}
