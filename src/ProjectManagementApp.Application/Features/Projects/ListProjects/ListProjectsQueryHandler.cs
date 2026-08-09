using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Projects.ListProjects;

/// <summary>
/// Scope is composed into the query BEFORE <c>CountAsync</c> — deviation here is a security bug,
/// not a style choice: counting before scoping would leak system-wide existence through
/// <c>totalCount</c> (FR-007, data-model.md §5).
/// </summary>
public class ListProjectsQueryHandler : IRequestHandler<ListProjectsQuery, Result<PagedResult<ProjectSummaryDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly ProjectsOptions _options;

    public ListProjectsQueryHandler(
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

    // Fixed composition order (data-model.md §5) — deviation here is a bug, not a style choice:
    // 1. base query  2. ApplyScope (ALWAYS first)  3. search  4. status filter
    // 5. CountAsync (on the scoped+filtered query)  6. whitelisted sort  7. Skip/Take (clamped)
    public async Task<Result<PagedResult<ProjectSummaryDto>>> Handle(ListProjectsQuery request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? _options.DefaultPageSize : Math.Min(request.PageSize, _options.MaxPageSize);
        var sort = string.IsNullOrWhiteSpace(request.Sort) ? ProjectSortMap.Default : request.Sort;

        IQueryable<Project> query = _accessPolicy.ApplyScope(_db.Projects.AsNoTracking(), caller);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, term) || (p.Description != null && EF.Functions.ILike(p.Description, term)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ProjectStatus>(request.Status, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pageOfProjects = await ProjectSortMap.Apply(query, sort)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Owner)
            .ToListAsync(cancellationToken);

        var items = pageOfProjects.Select(p => p.ToSummaryDto()).ToList();
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return Result<PagedResult<ProjectSummaryDto>>.Success(
            new PagedResult<ProjectSummaryDto>(items, page, pageSize, totalCount, totalPages));
    }
}
