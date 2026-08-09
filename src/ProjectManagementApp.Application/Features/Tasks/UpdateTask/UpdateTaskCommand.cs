using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Tasks.UpdateTask;

// Matches contracts/tasks.v1.yaml#/components/schemas/UpdateTaskRequest — Title/Description/
// Priority/DueDate ONLY. Deliberately excludes status, assigneeId, projectId: each has its own
// endpoint or is immutable. IfMatchVersion carries the parsed If-Match header, not a body field
// (002 R-2) — the controller rejects a missing header with 400 before this command is constructed.
public sealed record UpdateTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    string Priority,
    DateOnly? DueDate,
    uint IfMatchVersion) : IRequest<Result<TaskDetailDto>>;
