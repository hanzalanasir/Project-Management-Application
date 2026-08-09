using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class RefreshEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RefreshEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task Refresh_WithValidCookie_Returns200_NewAccessToken_RotatedCookie_AndLinksOldToNew()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Refresh User", "refresh-test@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("refresh-test@example.com", "S3cure-P@ss!"));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var originalAccessToken = loginBody.GetProperty("accessToken").GetString();
        var xsrfToken = CsrfTestHelper.ExtractXsrfToken(loginResponse);

        // HandleCookies=true on the shared client carries the refresh cookie automatically.
        using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithCsrfHeader(xsrfToken);
        var refreshResponse = await client.SendAsync(refreshRequest);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<JsonElement>();
        var newAccessToken = refreshBody.GetProperty("accessToken").GetString();
        newAccessToken.Should().NotBe(originalAccessToken);

        refreshResponse.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Should().Contain(c => c.StartsWith("refresh_token="));

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == "refresh-test@example.com");
        var tokens = await db.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync();

        tokens.Should().HaveCount(2);
        var oldToken = tokens.Single(t => t.RevokedAt != null);
        var newToken = tokens.Single(t => t.RevokedAt == null);
        oldToken.ReplacedByToken.Should().Be(newToken.TokenHash);
    }
}
