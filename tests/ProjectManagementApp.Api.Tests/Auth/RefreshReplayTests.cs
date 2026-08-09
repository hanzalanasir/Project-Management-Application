using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class RefreshReplayTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RefreshReplayTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task Refresh_ReplayingAnAlreadyRotatedToken_Returns401()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Replay User", "replay-test@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("replay-test@example.com", "S3cure-P@ss!"));
        loginResponse.Headers.TryGetValues("Set-Cookie", out var loginCookies);
        var originalRefreshCookie = ExtractCookiePair(loginCookies!.Single(c => c.StartsWith("refresh_token=")));
        var originalXsrfCookie = ExtractCookiePair(loginCookies!.Single(c => c.StartsWith("XSRF-TOKEN=")));
        var xsrfToken = CsrfTestHelper.ExtractXsrfToken(loginResponse);

        // First refresh rotates the cookie (uses the shared client's own cookie jar).
        using var firstRefreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithCsrfHeader(xsrfToken);
        var firstRefresh = await client.SendAsync(firstRefreshRequest);
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

        // Replay the PRE-rotation cookie on a fresh client (no jar interference) — this is what
        // an attacker replaying a stolen, already-rotated token would do. The matching XSRF
        // cookie+header pair travels with it (CSRF and refresh-token replay are independent checks).
        using var replayRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithCsrfHeader(xsrfToken);
        replayRequest.Headers.Add("Cookie", $"{originalRefreshCookie}; {originalXsrfCookie}");
        var replayResponse = await _fixture.CreateClient().SendAsync(replayRequest);

        replayResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static string ExtractCookiePair(string setCookieHeader) => setCookieHeader.Split(';')[0];
}
