using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Tasks.ListTasks;

// Serves BOTH routes (research R-4): the nested route pre-populates ProjectId; the flat route
// leaves it null unless the caller supplies ?projectId=. PageSize of 0/negative means "not
// supplied" (same convention as 002's ListProjectsQuery).
public sealed record ListTasksQuery(
    int Page,
    int PageSize,
    Guid? ProjectId,
    string? Status,
    Guid? AssigneeId,
    string? Search,
    string? Sort) : IRequest<Result<PagedResult<TaskSummaryDto>>>;
