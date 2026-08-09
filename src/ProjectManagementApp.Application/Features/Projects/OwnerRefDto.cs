namespace ProjectManagementApp.Application.Features.Projects;

// Matches contracts/projects.v1.yaml#/components/schemas/OwnerRef. isActive is surfaced (not
// hidden) so a deactivated owner is still shown, flagged — never silently omitted (spec US-002-03).
public sealed record OwnerRefDto(Guid Id, string FullName, bool IsActive);
