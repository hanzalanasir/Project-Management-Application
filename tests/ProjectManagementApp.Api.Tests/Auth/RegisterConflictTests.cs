using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class RegisterConflictTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RegisterConflictTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);

    [Fact]
    public async Task Register_DuplicateEmail_Returns409_EvenWithDifferentCasing()
    {
        var client = _fixture.CreateClient();

        var first = new RegisterRequest("Dana Rivera", "dana@example.com", "S3cure-P@ss!", "S3cure-P@ss!");
        var firstResponse = await client.PostAsJsonAsync("/api/auth/register", first);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicate = new RegisterRequest("Dana Impostor", "DANA@EXAMPLE.COM", "An0ther-P@ss!", "An0ther-P@ss!");
        var duplicateResponse = await client.PostAsJsonAsync("/api/auth/register", duplicate);

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
