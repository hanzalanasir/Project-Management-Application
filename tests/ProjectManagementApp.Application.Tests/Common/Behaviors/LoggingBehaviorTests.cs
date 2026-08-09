using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ProjectManagementApp.Application.Common.Behaviors;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Tests.Common.Behaviors;

public class LoggingBehaviorTests
{
    public sealed record FakeLoginCommand(string Email, string Password) : IRequest<Result<string>>;

    // Unauthenticated-shaped by default (throws), matching real CurrentUserService's contract for
    // requests like Login/Register — the behavior must tolerate this, not assume a caller exists.
    private static ICurrentUserService FakeCurrentUserService()
    {
        var service = Substitute.For<ICurrentUserService>();
        service.Current.Returns(_ => throw new InvalidOperationException("No authenticated user."));
        return service;
    }

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
        var behavior = new LoggingBehavior<FakeLoginCommand, Result<string>>(logger, FakeCurrentUserService());

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
        var behavior = new LoggingBehavior<FakeLoginCommand, Result<string>>(logger, FakeCurrentUserService());

        await behavior.Handle(new FakeLoginCommand("a@b.com", "x"), _ => Task.FromResult(Result<string>.Success("ok")), CancellationToken.None);

        logger.Messages.Should().Contain(m => m.Contains(nameof(FakeLoginCommand)));
    }

    public sealed record FakeGetProjectByIdQuery(Guid Id) : IRequest<Result<string>>;

    [Fact]
    public async Task Handle_ForbiddenResult_LogsActorId_EntityId_AndReason_NoRequestPayload()
    {
        var logger = new CapturingLogger<LoggingBehavior<FakeGetProjectByIdQuery, Result<string>>>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var actorId = Guid.NewGuid();
        currentUser.Current.Returns(new CurrentUser(actorId, "actor@example.com", "ProjectManager", "Actor"));
        var behavior = new LoggingBehavior<FakeGetProjectByIdQuery, Result<string>>(logger, currentUser);

        var projectId = Guid.NewGuid();
        var request = new FakeGetProjectByIdQuery(projectId);
        var denial = Result<string>.Failure(new Error(ErrorKind.Forbidden, "You do not have access to this project."));

        var result = await behavior.Handle(request, _ => Task.FromResult(denial), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        logger.Messages.Should().Contain(m =>
            m.Contains(actorId.ToString()) &&
            m.Contains(projectId.ToString()) &&
            m.Contains("You do not have access to this project."));
    }

    [Fact]
    public async Task Handle_SuccessfulResult_DoesNotLogAsADenial()
    {
        var logger = new CapturingLogger<LoggingBehavior<FakeGetProjectByIdQuery, Result<string>>>();
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Current.Returns(new CurrentUser(Guid.NewGuid(), "actor@example.com", "Admin", "Actor"));
        var behavior = new LoggingBehavior<FakeGetProjectByIdQuery, Result<string>>(logger, currentUser);

        await behavior.Handle(new FakeGetProjectByIdQuery(Guid.NewGuid()), _ => Task.FromResult(Result<string>.Success("ok")), CancellationToken.None);

        logger.Messages.Should().NotContain(m => m.Contains("Denied"));
    }
}
