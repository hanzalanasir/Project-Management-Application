using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class LogoutIdempotencyTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public LogoutIdempotencyTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
    private sealed record LoginRequest(string Email, string Password);

    [Fact]
    public async Task Logout_CalledTwice_StillReturns204_TheSecondTime()
    {
        var client = _fixture.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Logout Twice", "logout-twice@example.com", "S3cure-P@ss!", "S3cure-P@ss!"));
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("logout-twice@example.com", "S3cure-P@ss!"));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginBody.GetProperty("accessToken").GetString();
        var xsrfToken = CsrfTestHelper.ExtractXsrfToken(loginResponse);

        using var firstRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout").WithCsrfHeader(xsrfToken);
        firstRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var firstResponse = await client.SendAsync(firstRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var secondRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout").WithCsrfHeader(xsrfToken);
        secondRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var secondResponse = await client.SendAsync(secondRequest);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
