using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Reports.GetTeamPerformance;

public sealed record GetTeamPerformanceQuery(
    DateOnly? From,
    DateOnly? To,
    string? ProjectScope,
    Guid? UserId) : IRequest<Result<TeamPerformanceReportDto>>;
