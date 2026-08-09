using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Users;

[Collection(ApiTestCollection.Name)]
public class ChangeUserRoleConcurrencyTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ChangeUserRoleConcurrencyTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record ChangeUserRoleRequest(string Role);

    [Fact]
    public async Task ChangeRole_MissingIfMatch_Returns400()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var targetId = await RegisterAndGetIdAsync(client, "No IfMatch", "no-ifmatch@example.com", "S3cure-P@ss!");

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{targetId}/role")
        {
            Content = JsonContent.Create(new ChangeUserRoleRequest("ProjectManager"))
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.SendAsync(putRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeRole_StaleIfMatch_Returns409()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var targetId = await RegisterAndGetIdAsync(client, "Stale IfMatch", "stale-ifmatch@example.com", "S3cure-P@ss!");

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{targetId}/role")
        {
            Content = JsonContent.Create(new ChangeUserRoleRequest("ProjectManager"))
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        putRequest.Headers.TryAddWithoutValidation("If-Match", "\"999999\"");
        var response = await client.SendAsync(putRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
