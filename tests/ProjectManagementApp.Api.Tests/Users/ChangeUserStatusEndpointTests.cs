using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using static ProjectManagementApp.Api.Tests.Users.UsersTestHelper;

namespace ProjectManagementApp.Api.Tests.Users;

[Collection(ApiTestCollection.Name)]
public class ChangeUserStatusEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ChangeUserStatusEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record ChangeUserStatusRequest(bool IsActive);

    [Fact]
    public async Task ChangeStatus_DeactivatingADifferentActiveUser_Returns200_AndRevokesAllTheirRefreshTokens()
    {
        var client = _fixture.CreateClient();
        var adminToken = await LoginAsync(client, AdminEmail, AdminPassword);
        var targetId = await RegisterAndGetIdAsync(client, "Deactivate Target", "deactivate-target@example.com", "S3cure-P@ss!");
        await LoginAsync(client, "deactivate-target@example.com", "S3cure-P@ss!"); // gives the target a live refresh token

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{targetId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var getResponse = await client.SendAsync(getRequest);
        var etag = getResponse.Headers.ETag!.Tag;

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{targetId}/status")
        {
            Content = JsonContent.Create(new ChangeUserStatusRequest(false))
        };
        putRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        putRequest.Headers.TryAddWithoutValidation("If-Match", etag);
        var putResponse = await client.SendAsync(putRequest);

        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokens = await db.RefreshTokens.Where(t => t.UserId == targetId).ToListAsync();
        tokens.Should().OnlyContain(t => t.RevokedAt != null);
    }
}
