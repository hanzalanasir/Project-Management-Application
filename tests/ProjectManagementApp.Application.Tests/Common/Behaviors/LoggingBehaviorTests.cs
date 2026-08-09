using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using ProjectManagementApp.Application.Common.Behaviors;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Tests.Common.Behaviors;

public class LoggingBehaviorTests
{
    public sealed record FakeLoginCommand(string Email, string Password) : IRequest<Result<string>>;

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            Messages.Add(state?.ToString() ?? string.Empty);
            return NullDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }

    [Fact]
    public async Task Handle_NeverLogsRequestBody_EvenWhenRequestCarriesAPassword()
    {
        var logger = new CapturingLogger<LoggingBehavior<FakeLoginCommand, Result<string>>>();
        var behavior = new LoggingBehavior<FakeLoginCommand, Result<string>>(logger);

        const string secret = "Sup3rSecretPassword!";
        var request = new FakeLoginCommand("user@example.com", secret);

        var result = await behavior.Handle(request, _ => Task.FromResult(Result<string>.Success("ok")), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Messages.Should().NotContain(m => m.Contains(secret));
        logger.Messages.Should().NotContain(m => m.Contains(request.Email));
    }

    [Fact]
    public async Task Handle_LogsRequestName()
    {
        var logger = new CapturingLogger<LoggingBehavior<FakeLoginCommand, Result<string>>>();
        var behavior = new LoggingBehavior<FakeLoginCommand, Result<string>>(logger);

        await behavior.Handle(new FakeLoginCommand("a@b.com", "x"), _ => Task.FromResult(Result<string>.Success("ok")), CancellationToken.None);

        logger.Messages.Should().Contain(m => m.Contains(nameof(FakeLoginCommand)));
    }
}
