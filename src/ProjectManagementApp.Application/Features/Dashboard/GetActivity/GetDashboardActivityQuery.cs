using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Dashboard.GetActivity;

/// <summary>Page and PageSize only — scope is derived from the caller, never accepted from the request.</summary>
public sealed record GetDashboardActivityQuery(int Page, int PageSize) : IRequest<Result<PagedResult<ActivityEntryDto>>>;
