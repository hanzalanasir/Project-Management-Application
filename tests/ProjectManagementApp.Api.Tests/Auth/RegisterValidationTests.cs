using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectManagementApp.Api.Tests.Fixtures;
using ProjectManagementApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectManagementApp.Api.Tests.Auth;

[Collection(ApiTestCollection.Name)]
public class RegisterValidationTests : IAsyncLifetime
{
    private readonly ApiTestFixture _fixture;

    public RegisterValidationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);

    [Fact]
    public async Task Register_PolicyFailingPassword_Returns400_WithPerFieldErrors_AndPersistsNothing()
    {
        var client = _fixture.CreateClient();
        var request = new RegisterRequest("Weak Password", "weak@example.com", "weak", "weak");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("errors");

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Users.AnyAsync(u => u.Email == "weak@example.com")).Should().BeFalse();
    }
}
