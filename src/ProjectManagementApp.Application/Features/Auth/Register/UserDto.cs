using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApp.Application.Features.Auth.Register;

// The safe user projection. Contains no password, no hash, no token — ever.
// CreatedAt is nullable: the contract marks it optional, and GetCurrentUserQueryHandler (T084)
// cannot supply it without a DB round-trip, which NFR-002 forbids for that endpoint (T082).
public sealed record UserDto(Guid Id, string FullName, [property: EmailAddress] string Email, string Role, DateTimeOffset? CreatedAt);
