namespace ProjectManagementApp.Application.Features.Projects;

// Manual static mapping — no AutoMapper (ADR-0005). Both methods require Project.Owner to be
// loaded (projected or Include'd) by the caller; neither method itself touches the database.
public static class ProjectMappingExtensions
{
    public static OwnerRefDto ToOwnerRefDto(this ProjectManagementApp.Domain.Entities.ApplicationUser owner) =>
        new(owner.Id, owner.FullName, owner.IsActive);

    public static ProjectSummaryDto ToSummaryDto(this ProjectManagementApp.Domain.Entities.Project project) =>
        new(project.Id, project.Name, project.Status.ToString(), project.StartDate, project.EndDate, project.Owner.ToOwnerRefDto());

    public static ProjectDetailDto ToDetailDto(this ProjectManagementApp.Domain.Entities.Project project) =>
        new(project.Id, project.Name, project.Description, project.Status.ToString(), project.StartDate, project.EndDate,
            project.Owner.ToOwnerRefDto(), project.CreatedAt, project.UpdatedAt, project.Version);
}
