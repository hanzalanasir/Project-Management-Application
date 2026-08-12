using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Reports.GetActivityReport;

public sealed record GetActivityReportQuery(
    DateOnly? From,
    DateOnly? To,
    Guid? ProjectId,
    string? EntityType,
    Guid? ActorId,
    int Page,
    int PageSize) : IRequest<Result<ActivityReportDto>>;
