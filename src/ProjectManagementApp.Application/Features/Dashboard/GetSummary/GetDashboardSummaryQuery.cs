using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Dashboard.GetSummary;

/// <summary>No parameters — scope is derived from the caller, never passed in.</summary>
public sealed record GetDashboardSummaryQuery : IRequest<Result<DashboardSummaryDto>>;
