using System.Text.Json.Serialization;

namespace ProjectManagementApp.Application.Features.Projects;

// Matches contracts/projects.v1.yaml#/components/schemas/ProjectDetail. The row version is
// deliberately not a wire property — it travels as the ETag/If-Match header (research R-2);
// JsonIgnore keeps Version out of the JSON body while still available to the controller
// (same pattern as 001's AdminUserDetail).
public sealed record ProjectDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    OwnerRefDto Owner,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    [property: JsonIgnore] uint Version);
