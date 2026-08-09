using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Tasks.ListTasks;

/// <summary>
/// Serves BOTH list routes (research R-4) — one handler, one predicate, so their scoping can
/// never drift. Fixed composition order (data-model.md §5): scope -> filters -> CountAsync (on the
/// scoped+filtered query) -> whitelisted sort -> Skip/Take (clamped). Deviation here is a security
/// bug, not a style choice: counting before scoping would leak system-wide existence through
/// <c>totalCount</c>. Only <c>Assignee</c> is Include'd — <c>TaskSummaryDto.ProjectId</c> is the
/// scalar FK, not a navigation, so no join to Project is needed for the list (no N+1 either way).
/// </summary>
public class ListTasksQueryHandler : IRequestHandler<ListTasksQuery, Result<PagedResult<TaskSummaryDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITaskAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly TasksOptions _options;

    public ListTasksQueryHandler(
        IApplicationDbContext db,
        ITaskAccessPolicy accessPolicy,
        ICurrentUserService currentUserService,
        IOptions<TasksOptions> options)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _currentUserService = currentUserService;
        _options = options.Value;
    }

    public async Task<Result<PagedResult<TaskSummaryDto>>> Handle(ListTasksQuery request, CancellationToken cancellationToken)
    {
        var caller = _currentUserService.Current;

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? _options.DefaultPageSize : Math.Min(request.PageSize, _options.MaxPageSize);
        var sort = string.IsNullOrWhiteSpace(request.Sort) ? TaskSortMap.Default : request.Sort;

        IQueryable<TaskItem> query = _accessPolicy.ApplyScope(_db.Tasks.AsNoTracking(), caller);

        if (request.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == request.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ProjectManagementApp.Domain.Enums.TaskStatus>(request.Status, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (request.AssigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == request.AssigneeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search}%";
            query = query.Where(t => EF.Functions.ILike(t.Title, term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pageOfTasks = await TaskSortMap.Apply(query, sort)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.Assignee)
            .ToListAsync(cancellationToken);

        var items = pageOfTasks.Select(t => t.ToSummaryDto()).ToList();
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return Result<PagedResult<TaskSummaryDto>>.Success(
            new PagedResult<TaskSummaryDto>(items, page, pageSize, totalCount, totalPages));
    }
}
