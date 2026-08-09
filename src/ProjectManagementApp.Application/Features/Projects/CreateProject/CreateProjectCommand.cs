using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Projects.CreateProject;

// Matches contracts/projects.v1.yaml#/components/schemas/CreateProjectRequest. OwnerId is ignored
// entirely for a ProjectManager caller (owner is taken from the token) — see the handler.
public sealed record CreateProjectCommand(
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Status,
    Guid? OwnerId) : IRequest<Result<ProjectDetailDto>>;
