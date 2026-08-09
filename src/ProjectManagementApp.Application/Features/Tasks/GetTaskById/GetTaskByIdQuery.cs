using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Tasks.GetTaskById;

public sealed record GetTaskByIdQuery(Guid Id) : IRequest<Result<TaskDetailDto>>;
