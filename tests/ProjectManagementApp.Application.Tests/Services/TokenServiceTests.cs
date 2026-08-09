using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Identity;

namespace ProjectManagementApp.Application.Tests.Services;

public class TokenServiceTests
{
    private static TokenService CreateService(int accessTokenMinutes = 15) =>
        new(Options.Create(new JwtOptions
        {
            SigningKey = "unit-test-signing-key-at-least-32-characters-long",
            Issuer = "ProjectManagementApp.Tests",
            Audience = "ProjectManagementApp.Tests",
            AccessTokenMinutes = accessTokenMinutes,
            RefreshTokenDays = 7
        }), db: null!);

    [Fact]
    public void CreateAccessToken_IncludesSubEmailAndSingleRoleClaim_WithConfiguredExpiry()
    {
        var service = CreateService(accessTokenMinutes: 15);
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "dana@example.com" };

        var token = service.CreateAccessToken(user, "ProjectManager");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Subject.Should().Be(user.Id.ToString());
        jwt.Claims.Should().ContainSingle(c => c.Type == "email" && c.Value == "dana@example.com");
        jwt.Claims.Where(c => c.Type == "role").Should().ContainSingle().Which.Value.Should().Be("ProjectManager");
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CreateRefreshToken_ReturnsOpaqueHighEntropyString()
    {
        var service = CreateService();

        var token1 = service.CreateRefreshToken();
        var token2 = service.CreateRefreshToken();

        token1.Should().NotBeNullOrEmpty();
        token1.Length.Should().BeGreaterThan(32);
        token1.Should().NotBe(token2, "each refresh token must be unique/high-entropy");
    }
}
