using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Tasks.ReassignTask;

// Matches contracts/tasks.v1.yaml#/components/schemas/ReassignTaskRequest — AssigneeId ONLY
// (nullable: null unassigns). IfMatchVersion carries the parsed If-Match header, not a body field
// (002 R-2) — the controller rejects a missing header with 400 before this command is constructed.
public sealed record ReassignTaskCommand(
    Guid Id,
    Guid? AssigneeId,
    uint IfMatchVersion) : IRequest<Result<TaskDetailDto>>;
