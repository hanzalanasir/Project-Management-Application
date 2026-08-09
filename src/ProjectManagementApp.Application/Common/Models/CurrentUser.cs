namespace ProjectManagementApp.Application.Common.Models;

// Exactly one role — see 001. FullName appended (not part of shared-contracts.md's original
// three-field declaration) so GetCurrentUserQueryHandler can build the contract's UserDto
// (which requires fullName) from token claims alone — zero DB round-trips (NFR-002, T082).
// Positioned last so any 002-006 code consuming (UserId, Email, Role) positionally is unaffected;
// only 001 constructs this record.
public sealed record CurrentUser(Guid UserId, string Email, string Role, string FullName);
