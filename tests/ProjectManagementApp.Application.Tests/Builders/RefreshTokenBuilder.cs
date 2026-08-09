using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Builders;

public class RefreshTokenBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();
    private string _tokenHash = "test-hash";
    private DateTimeOffset _expiresAt = DateTimeOffset.UtcNow.AddDays(7);
    private DateTimeOffset? _revokedAt;

    public RefreshTokenBuilder ForUser(Guid userId) { _userId = userId; return this; }
    public RefreshTokenBuilder WithTokenHash(string hash) { _tokenHash = hash; return this; }
    public RefreshTokenBuilder Expired() { _expiresAt = DateTimeOffset.UtcNow.AddDays(-1); return this; }
    public RefreshTokenBuilder Revoked() { _revokedAt = DateTimeOffset.UtcNow; return this; }

    public RefreshToken Build() => new()
    {
        Id = _id,
        UserId = _userId,
        TokenHash = _tokenHash,
        ExpiresAt = _expiresAt,
        CreatedAt = DateTimeOffset.UtcNow,
        RevokedAt = _revokedAt,
    };
}
