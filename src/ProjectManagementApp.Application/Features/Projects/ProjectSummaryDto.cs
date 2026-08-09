namespace ProjectManagementApp.Application.Features.Projects;

// Matches contracts/projects.v1.yaml#/components/schemas/ProjectSummary — the list-row shape.
public sealed record ProjectSummaryDto(
    Guid Id,
    string Name,
    string Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    OwnerRefDto Owner);
