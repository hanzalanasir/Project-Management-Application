using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Projects.ListProjects;

// Matches the /projects GET query parameters in contracts/projects.v1.yaml. PageSize of 0/negative
// means "not supplied" — the handler substitutes the configured default (research R-3, data-model §5).
public sealed record ListProjectsQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Status,
    string? Sort) : IRequest<Result<PagedResult<ProjectSummaryDto>>>;
