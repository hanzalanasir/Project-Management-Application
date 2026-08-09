using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Users;

[Collection(ApiTestCollection.Name)]
public class ChangeUserStatusSelfTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ChangeUserStatusSelfTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record ChangeUserStatusRequest(bool IsActive);

    [Fact]
    public async Task ChangeStatus_AdminDeactivatingThemselves_Returns409()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var meResponse = await client.SendAsync(meRequest);
        var me = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
        var adminId = me.GetProperty("id").GetString();

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{adminId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var getResponse = await client.SendAsync(getRequest);
        var etag = getResponse.Headers.ETag!.Tag;

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{adminId}/status")
        {
            Content = JsonContent.Create(new ChangeUserStatusRequest(false))
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        putRequest.Headers.TryAddWithoutValidation("If-Match", etag);
        var putResponse = await client.SendAsync(putRequest);

        putResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
