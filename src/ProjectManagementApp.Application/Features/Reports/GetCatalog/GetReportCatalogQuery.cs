using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Reports.GetCatalog;

/// <summary>No parameters — the descriptor set is role-annotated from the caller, never passed in.</summary>
public sealed record GetReportCatalogQuery : IRequest<Result<IReadOnlyList<ReportDescriptor>>>;
