using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Reports.GetProjectProgress;

public sealed record GetProjectProgressQuery(
    DateOnly? From,
    DateOnly? To,
    string? ProjectScope) : IRequest<Result<ProjectProgressReportDto>>;
