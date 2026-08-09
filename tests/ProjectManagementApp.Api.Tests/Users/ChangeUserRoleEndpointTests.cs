using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Users;

[Collection(ApiTestCollection.Name)]
public class ChangeUserRoleEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ChangeUserRoleEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record ChangeUserRoleRequest(string Role);

    [Fact]
    public async Task ChangeRole_PromotingADifferentUser_Returns200_AndAuditsFromTo()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var targetId = await RegisterAndGetIdAsync(client, "Promote Target", "promote-target@example.com", "S3cure-P@ss!");

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{targetId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var getResponse = await client.SendAsync(getRequest);
        var etag = getResponse.Headers.ETag!.Tag;

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{targetId}/role")
        {
            Content = JsonContent.Create(new ChangeUserRoleRequest("ProjectManager"))
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        putRequest.Headers.TryAddWithoutValidation("If-Match", etag);
        var putResponse = await client.SendAsync(putRequest);

        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await putResponse.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("role").GetString().Should().Be("ProjectManager");
    }
}
