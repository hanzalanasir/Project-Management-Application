using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApp.Application.Features.Auth.ListUsers;

// The Admin-only user projection — unlike UserDto, it exposes IsActive (US-001-07).
public sealed record AdminUserSummary(Guid Id, string FullName, [property: EmailAddress] string Email, string Role, bool IsActive, DateTimeOffset CreatedAt);
