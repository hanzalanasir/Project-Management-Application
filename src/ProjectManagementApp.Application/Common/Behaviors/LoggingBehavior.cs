using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProjectManagementApp.Application.Common.Behaviors;

// Opens a logging scope with request name, correlation id, and elapsed ms.
// MUST NOT log the request itself — RegisterCommand/LoginCommand carry plaintext passwords (Constitution V.3).
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
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
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);
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
}
