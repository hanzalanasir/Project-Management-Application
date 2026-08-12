using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Auth.Refresh;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Identity;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Tokens;

// data-model.md §5: revoke-old + insert-new + audit commit as ONE transaction. A failure mid
// rotation must never leave two live tokens (or a revoked-but-not-replaced token).
[Collection(PostgresCollection.Name)]
public class RotationAtomicityTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private static readonly JwtOptions TestJwtOptions = new()
    {
        SigningKey = "atomicity-test-signing-key-at-least-32-characters",
        Issuer = "test", Audience = "test", AccessTokenMinutes = 15, RefreshTokenDays = 7
    };

    public RotationAtomicityTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private sealed class ThrowingActivityLogService : IActivityLogService
    {
        public Task LogAsync(Guid? actorId, string action, string entityType, string entityId, string changeSummary, CancellationToken ct, Guid? projectId = null)
            => throw new InvalidOperationException("Simulated failure mid-rotation.");

        public Task<PagedResult<ActivityEntry>> QueryScopedAsync(
            ActivityScope scope, int page, int pageSize, CancellationToken ct,
            DateTimeOffset? from = null, DateTimeOffset? to = null,
            Guid? projectId = null, string? entityType = null, Guid? actorId = null)
            => throw new NotImplementedException();
    }

    [Fact]
    public async Task FailureMidRotation_LeavesTheOriginalTokenUntouched_NoPartialState()
    {
        const string rawRefreshToken = "known-raw-value-for-atomicity-test";
        var userId = Guid.NewGuid();

        await using (var seedDb = _fixture.CreateDbContext())
        {
            var hasher = new TokenService(Options.Create(TestJwtOptions), seedDb);
            var user = new ApplicationUser
            {
                Id = userId, UserName = "atomicity@example.com", Email = "atomicity@example.com",
                NormalizedUserName = "ATOMICITY@EXAMPLE.COM", NormalizedEmail = "ATOMICITY@EXAMPLE.COM",
                FullName = "Atomicity Test", IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
            };
            var originalToken = new RefreshToken
            {
                Id = Guid.NewGuid(), UserId = userId, TokenHash = hasher.HashRefreshToken(rawRefreshToken),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7), CreatedAt = DateTimeOffset.UtcNow
            };
            seedDb.Users.Add(user);
            seedDb.RefreshTokens.Add(originalToken);
            await seedDb.SaveChangesAsync();
        }

        await using (var handlerDb = _fixture.CreateDbContext())
        {
            var tokenService = new TokenService(Options.Create(TestJwtOptions), handlerDb);
            var userManager = TestUserManagerFactory.Create(handlerDb);
            var handler = new RefreshCommandHandler(handlerDb, userManager, tokenService, new ThrowingActivityLogService());

            await FluentActions.Awaiting(() => handler.Handle(new RefreshCommand(rawRefreshToken), CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        await using var verifyDb = _fixture.CreateDbContext();
        var tokensAfterFailure = await verifyDb.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();

        tokensAfterFailure.Should().HaveCount(1, "the failed rotation must not have inserted a new token");
        tokensAfterFailure.Single().RevokedAt.Should().BeNull("the failed rotation must not have revoked the original token either — all-or-nothing");
    }
}
