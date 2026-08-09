using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class RefreshRevokedTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RefreshRevokedTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task Refresh_ExpiredToken_Returns401()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Expired User", "expired-test@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("expired-test@example.com", "S3cure-P@ss!"));
        var xsrfToken = CsrfTestHelper.ExtractXsrfToken(loginResponse);

        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.SingleAsync(u => u.Email == "expired-test@example.com");
            var token = await db.RefreshTokens.SingleAsync(t => t.UserId == user.Id);
            token.ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();
        }

        // Reuse `client` — its own cookie jar (HandleCookies=true) already carries both the
        // refresh_token and XSRF-TOKEN cookies from login; only the X-XSRF-TOKEN header needs
        // adding manually (that echo is Angular's job in the real app, not a generic HTTP client's).
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithCsrfHeader(xsrfToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_RevokedToken_Returns401()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Revoked User", "revoked-test@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("revoked-test@example.com", "S3cure-P@ss!"));
        var xsrfToken = CsrfTestHelper.ExtractXsrfToken(loginResponse);

        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.SingleAsync(u => u.Email == "revoked-test@example.com");
            var token = await db.RefreshTokens.SingleAsync(t => t.UserId == user.Id);
            token.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithCsrfHeader(xsrfToken);
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_UnknownToken_Returns401()
    {
        // A valid XSRF pair is still required to reach the refresh-token check at all; obtain one
        // via a real login, then overwrite the refresh_token cookie with a garbage value.
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Unknown Token User", "unknown-token@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("unknown-token@example.com", "S3cure-P@ss!"));
        var xsrfToken = CsrfTestHelper.ExtractXsrfToken(loginResponse);
        loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies);
        var xsrfCookiePair = cookies!.Single(c => c.StartsWith("XSRF-TOKEN=")).Split(';')[0];

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithCsrfHeader(xsrfToken);
        request.Headers.Add("Cookie", $"refresh_token=totally-unknown-value; {xsrfCookiePair}");
        var response = await _fixture.CreateClient().SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
