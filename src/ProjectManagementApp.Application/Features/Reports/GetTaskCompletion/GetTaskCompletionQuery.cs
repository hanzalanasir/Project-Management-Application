using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Reports.GetTaskCompletion;

public sealed record GetTaskCompletionQuery(
    DateOnly? From,
    DateOnly? To,
    string? GroupBy,
    string? ProjectScope,
    Guid? AssigneeId) : IRequest<Result<TaskCompletionReportDto>>;
