using System.Diagnostics;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Common.Behaviors;

// Opens a logging scope with request name, correlation id, and elapsed ms.
// MUST NOT log the request itself — RegisterCommand/LoginCommand carry plaintext passwords (Constitution V.3).
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        using var scope = _logger.BeginScope(
            "RequestName={RequestName} CorrelationId={CorrelationId}", requestName, correlationId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            // A role gate (attribute) never reaches this far — this is only the finer scope gate,
            // decided inside a handler (e.g. IProjectAccessPolicy) and returned as a Result, never
            // thrown. Denials are a WARNING with who/what/why, never the full request (no payloads).
            if (response is Result { IsSuccess: false } result && IsDenial(result.Error!.Kind))
            {
                _logger.LogWarning(
                    "Denied {RequestName} for actor {ActorId} on entity {EntityId}: {Reason} ({ErrorKind}) in {ElapsedMs}ms",
                    requestName, TryGetActorId(), TryGetRequestEntityId(request), result.Error.Message, result.Error.Kind, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMs}ms",
                    requestName, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Faulted {RequestName} after {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private static bool IsDenial(ErrorKind kind) => kind is ErrorKind.Forbidden or ErrorKind.NotFound;

    private object? TryGetActorId()
    {
        try
        {
            return _currentUserService.Current.UserId;
        }
        catch (InvalidOperationException)
        {
            // Unauthenticated requests (Login/Register) never reach a Forbidden/NotFound Result path
            // that this logs, but ICurrentUserService is only meaningful behind [Authorize] regardless.
            return "anonymous";
        }
    }

    // Generic across every feature, not just Projects: any command/query with an "Id" property
    // (GetProjectByIdQuery, UpdateProjectCommand, DeleteProjectCommand, GetUserByIdQuery, ...) gets
    // its target entity id surfaced without each handler needing to log it individually. 004's
    // commands (ListTeamQuery, AddTeamMemberCommand, RemoveTeamMemberCommand) have no "Id" at all —
    // every team operation is scoped by "ProjectId" instead, since there is no single membership id
    // known up front on add, and every action is inherently project-scoped. Falls back to
    // "ProjectId" second so those denials still surface the project rather than always logging
    // entity=null (found live during 004's polish pass, T060 — a real NFR-003 gap, not assumed).
    private static object? TryGetRequestEntityId(TRequest request)
    {
        var type = typeof(TRequest);
        return type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)?.GetValue(request)
            ?? type.GetProperty("ProjectId", BindingFlags.Public | BindingFlags.Instance)?.GetValue(request);
    }
}
