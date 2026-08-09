using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;

namespace ProjectManagementApp.Application.Features.Tasks.GetTaskById;

/// <summary>
/// Existence is resolved BEFORE the scope gate (mirrors 002's GetProjectByIdQueryHandler) — an
/// unknown id is always 404 regardless of caller role, only THEN is out-of-scope evaluated as 403
/// (maskable to 404 via <c>MaskOutOfScopeAs404</c>). A task whose assignee was deactivated is still
/// returned — <see cref="UserRefDto.IsActive"/> flags it rather than hiding the task.
/// </summary>
public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITaskAccessPolicy _accessPolicy;
    private readonly ICurrentUserService _currentUserService;
    private readonly TasksOptions _options;

    public GetTaskByIdQueryHandler(
        IApplicationDbContext db,
        ITaskAccessPolicy accessPolicy,
        ICurrentUserService currentUserService,
        IOptions<TasksOptions> options)
    {
        _db = db;
        _accessPolicy = accessPolicy;
        _currentUserService = currentUserService;
        _options = options.Value;
    }

    public async Task<Result<TaskDetailDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task is null)
        {
            return Result<TaskDetailDto>.Failure(new Error(ErrorKind.NotFound, "Task not found"));
        }

        var caller = _currentUserService.Current;
        var decision = await _accessPolicy.CanReadAsync(task, caller, cancellationToken);
        if (!decision.Allowed)
        {
            var kind = _options.MaskOutOfScopeAs404 ? ErrorKind.NotFound : ErrorKind.Forbidden;
            var message = kind == ErrorKind.NotFound ? "Task not found" : "You do not have access to this task.";
            return Result<TaskDetailDto>.Failure(new Error(kind, message));
        }

        return Result<TaskDetailDto>.Success(task.ToDetailDto());
    }
}
