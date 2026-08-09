using ProjectManagementApp.Domain.Entities;

namespace ProjectManagementApp.Application.Tests.Builders;

// Constitution IX.4 — test data via builders, not inline object literals scattered across files.
public class ApplicationUserBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _email = "user@example.com";
    private string _fullName = "Test User";
    private bool _isActive = true;
    private uint _version;

    public ApplicationUserBuilder WithId(Guid id) { _id = id; return this; }
    public ApplicationUserBuilder WithEmail(string email) { _email = email; return this; }
    public ApplicationUserBuilder WithFullName(string fullName) { _fullName = fullName; return this; }
    public ApplicationUserBuilder Inactive() { _isActive = false; return this; }
    public ApplicationUserBuilder WithVersion(uint version) { _version = version; return this; }

    public ApplicationUser Build()
    {
        var now = DateTimeOffset.UtcNow;
        return new ApplicationUser
        {
            Id = _id,
            UserName = _email,
            Email = _email,
            NormalizedUserName = _email.ToUpperInvariant(),
            NormalizedEmail = _email.ToUpperInvariant(),
            FullName = _fullName,
            IsActive = _isActive,
            CreatedAt = now,
            UpdatedAt = now,
            Version = _version,
        };
    }
}
