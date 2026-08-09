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
public class ChangeUserStatusReactivateTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public ChangeUserStatusReactivateTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record ChangeUserStatusRequest(bool IsActive);

    [Fact]
    public async Task ChangeStatus_Reactivating_Returns200_WritesUserReactivated_AndPreDeactivationTokenStaysRevoked()
    {
        var adminClient = _fixture.CreateClient();
        var adminToken = await LoginAsync(adminClient, AdminEmail, AdminPassword);
        var targetId = await RegisterAndGetIdAsync(adminClient, "Reactivate Target", "reactivate-target@example.com", "S3cure-P@ss!");

        var targetClient = _fixture.CreateClient();
        var targetLoginResponse = await targetClient.PostAsJsonAsync("/api/auth/login", new LoginRequest("reactivate-target@example.com", "S3cure-P@ss!"));
        var targetXsrf = CsrfTestHelper.ExtractXsrfToken(targetLoginResponse);

        // Deactivate.
        using (var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{targetId}"))
        {
            getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var getResponse = await adminClient.SendAsync(getRequest);
            var etag = getResponse.Headers.ETag!.Tag;

            using var deactivateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{targetId}/status")
            {
                Content = JsonContent.Create(new ChangeUserStatusRequest(false))
            };
            deactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            deactivateRequest.Headers.TryAddWithoutValidation("If-Match", etag);
            (await adminClient.SendAsync(deactivateRequest)).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // The pre-deactivation refresh token cannot refresh while deactivated.
        using (var preReactivateRefresh = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithCsrfHeader(targetXsrf))
        {
            (await targetClient.SendAsync(preReactivateRefresh)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Reactivate.
        using (var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{targetId}"))
        {
            getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var getResponse = await adminClient.SendAsync(getRequest);
            var etag = getResponse.Headers.ETag!.Tag;

            using var reactivateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{targetId}/status")
            {
                Content = JsonContent.Create(new ChangeUserStatusRequest(true))
            };
            reactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            reactivateRequest.Headers.TryAddWithoutValidation("If-Match", etag);
            var reactivateResponse = await adminClient.SendAsync(reactivateRequest);
            reactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await reactivateResponse.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("isActive").GetBoolean().Should().BeTrue();
        }

        // The SAME pre-deactivation refresh token remains 401 — reactivation does not restore it.
        using (var postReactivateRefresh = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithCsrfHeader(targetXsrf))
        {
            (await targetClient.SendAsync(postReactivateRefresh)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var activityLogs = await db.ActivityLogs.Where(a => a.EntityId == targetId.ToString()).ToListAsync();
        activityLogs.Should().Contain(a => a.Action == "UserReactivated");
    }
}
