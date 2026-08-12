using MediatR;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Reports.GetCatalog;

/// <summary>
/// Returns the static, role-annotated descriptor set. Deliberately does **not** call
/// <c>ReportGenerationAudit</c> — the catalog is metadata about the API surface, not a report
/// generation, and FR-011 requires it to write zero audit rows (T026 proves this with a
/// repeated-call test).
/// </summary>
public sealed class GetReportCatalogQueryHandler
    : IRequestHandler<GetReportCatalogQuery, Result<IReadOnlyList<ReportDescriptor>>>
{
    private readonly ICurrentUserService _currentUserService;

    public GetReportCatalogQueryHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public Task<Result<IReadOnlyList<ReportDescriptor>>> Handle(GetReportCatalogQuery request, CancellationToken ct)
    {
        var caller = _currentUserService.Current;
        var descriptors = ReportCatalog.Describe(caller.Role);
        return Task.FromResult(Result<IReadOnlyList<ReportDescriptor>>.Success(descriptors));
    }
}
