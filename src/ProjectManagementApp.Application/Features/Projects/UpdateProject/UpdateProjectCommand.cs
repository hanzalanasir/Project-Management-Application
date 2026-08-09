using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Projects.UpdateProject;

// IfMatchVersion carries the parsed `If-Match` header — NOT a body field (research R-2). The
// controller rejects a missing header with 400 before this command is ever constructed.
public sealed record UpdateProjectCommand(
    Guid Id,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status,
    Guid? OwnerId,
    uint IfMatchVersion) : IRequest<Result<ProjectDetailDto>>;
