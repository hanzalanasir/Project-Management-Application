using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Tasks.CreateTask;

// Matches contracts/tasks.v1.yaml#/components/schemas/CreateTaskRequest. ProjectId comes from the
// ROUTE, never the body — the contract's CreateTaskRequest deliberately has no projectId property
// (FR-003), so a task cannot be smuggled into another project by a widened payload.
public sealed record CreateTaskCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    string? Priority,
    DateOnly? DueDate,
    Guid? AssigneeId) : IRequest<Result<TaskDetailDto>>;
