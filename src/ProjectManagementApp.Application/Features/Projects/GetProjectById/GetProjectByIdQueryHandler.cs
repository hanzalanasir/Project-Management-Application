using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;

namespace ProjectManagementApp.Application.Features.Projects.GetProjectById;

/// <summary>
/// Existence is resolved BEFORE the scope gate — an unknown id is always 404 regardless of caller
/// role, only THEN is out-of-scope evaluated as 403 (maskable to 404 via <c>MaskOutOfScopeAs404</c>,
/// spec OQ-002-03).
/// </summary>
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly ProjectsOptions _options;

    public GetProjectByIdQueryHandler(
        IApplicationDbContext db,
        IProjectAccessPolicy accessPolicy,
        ICurrentUserService currentUserService,
        IOptions<ProjectsOptions> options)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _currentUserService = currentUserService;
        _options = options.Value;
    }

    public async Task<Result<ProjectDetailDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        // Existence is established BEFORE the scope gate — an unknown id is always 404, regardless
        // of caller role (data-model.md §3).
        var project = await _db.Projects
            .Include(p => p.Owner)
            .SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project is null)
        {
            return Result<ProjectDetailDto>.Failure(new Error(ErrorKind.NotFound, "Project not found"));
        }

        var caller = _currentUserService.Current;
        var decision = await _accessPolicy.CanReadAsync(project, caller, cancellationToken);
        if (!decision.Allowed)
        {
            // Maskable to 404 by configuration (spec OQ-002-03) — a hardening path against
            // resource enumeration; 403 is the default so the distinction is visible unless opted out.
            var kind = _options.MaskOutOfScopeAs404 ? ErrorKind.NotFound : ErrorKind.Forbidden;
            var message = kind == ErrorKind.NotFound ? "Project not found" : "You do not have access to this project.";
            return Result<ProjectDetailDto>.Failure(new Error(kind, message));
        }

        return Result<ProjectDetailDto>.Success(project.ToDetailDto());
    }
}
