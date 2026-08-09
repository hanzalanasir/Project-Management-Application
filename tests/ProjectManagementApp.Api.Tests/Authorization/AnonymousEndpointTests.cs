using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Authorization;

[Collection(ApiTestCollection.Name)]
public class AnonymousEndpointTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public AnonymousEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task Health_SucceedsWithoutAToken()
    {
        var client = _fixture.CreateClient();
        var response = await client.GetAsync("/api/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_SucceedsWithoutAToken()
    {
        var client = _fixture.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Anon Test", "anon-register@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Login_SucceedsWithoutAToken()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Anon Login", "anon-login@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("anon-login@example.com", "S3cure-P@ss!"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_SucceedsWithoutABearerToken()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Anon Refresh", "anon-refresh@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("anon-refresh@example.com", "S3cure-P@ss!"));
        var xsrfToken = CsrfTestHelper.ExtractXsrfToken(loginResponse);

        // No Authorization header attached — only the cookie the login response set.
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh").WithCsrfHeader(xsrfToken);
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCurrentUser_RequiresAToken()
    {
        var client = _fixture.CreateClient();
        var response = await client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
