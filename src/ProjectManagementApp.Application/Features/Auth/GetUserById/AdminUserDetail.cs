using System.Text.Json.Serialization;

namespace ProjectManagementApp.Application.Features.Auth.GetUserById;

// The row version travels as the ETag/If-Match header, never a body property (ADR-0007 §3) —
// JsonIgnore keeps Version out of the wire format while still available to the controller.
public sealed record AdminUserDetail(
    Guid Id, string FullName, string Email, string Role, bool IsActive,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
    [property: JsonIgnore] uint Version);
