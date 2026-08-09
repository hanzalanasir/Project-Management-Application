using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Tasks.UpdateTaskStatus;

// EXACTLY ONE bindable property, Status — this narrowness IS the graduated model's structural
// half. Adding a second property here would dismantle it; the contract's UpdateTaskStatusRequest
// has additionalProperties: false for the identical reason. IfMatchVersion carries the parsed
// If-Match header, not a body field (002 R-2).
public sealed record UpdateTaskStatusCommand(
    Guid Id,
    string Status,
    uint IfMatchVersion) : IRequest<Result<TaskDetailDto>>;
