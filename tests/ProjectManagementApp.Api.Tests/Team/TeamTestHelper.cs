using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProjectManagementApp.Api.Tests.Team;

public static class TeamTestHelper
{
    public sealed record AddTeamMemberRequest(Guid UserId);

    public static async Task<HttpResponseMessage> AddTeamMemberAsync(HttpClient client, string token, Guid projectId, object request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/projects/{projectId}/team")
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(httpRequest);
    }

    public static async Task<HttpResponseMessage> GetTeamAsync(HttpClient client, string token, Guid projectId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{projectId}/team");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> RemoveTeamMemberAsync(HttpClient client, string token, Guid projectId, Guid userId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/projects/{projectId}/team/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    /// <summary>Deactivates a user via the real admin endpoint (GET for ETag, then PUT .../status).</summary>
    public static async Task DeactivateUserAsync(HttpClient client, string adminToken, Guid userId)
    {
        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var getResponse = await client.SendAsync(getRequest);
        var etag = getResponse.Headers.ETag!.Tag;

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{userId}/status")
        {
            Content = JsonContent.Create(new { isActive = false }),
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        putRequest.Headers.TryAddWithoutValidation("If-Match", etag);
        await client.SendAsync(putRequest);
    }

    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();
}
