using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using ProjectManagementApp.Application.Common.Behaviors;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Tests.Common.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record FakeCommand(string Name) : IRequest<Result<string>>;

    [Fact]
    public async Task Handle_WhenValidatorFails_ShortCircuitsWithFailedResult_AndNeverThrows()
    {
        var failingValidator = Substitute.For<IValidator<FakeCommand>>();
        failingValidator.ValidateAsync(Arg.Any<ValidationContext<FakeCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var behavior = new ValidationBehavior<FakeCommand, Result<string>>(new[] { failingValidator });

        var nextCalled = false;
        Task<Result<string>> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult(Result<string>.Success("should not reach here"));
        }

        var act = async () => await behavior.Handle(new FakeCommand(""), Next, CancellationToken.None);

        var result = await act.Should().NotThrowAsync();
        result.Subject.IsSuccess.Should().BeFalse();
        result.Subject.Error!.Kind.Should().Be(ErrorKind.Validation);
        result.Subject.Error!.Fields.Should().ContainKey("name");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenValidatorSucceeds_CallsNext()
    {
        var passingValidator = Substitute.For<IValidator<FakeCommand>>();
        passingValidator.ValidateAsync(Arg.Any<ValidationContext<FakeCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<FakeCommand, Result<string>>(new[] { passingValidator });

        var result = await behavior.Handle(new FakeCommand("ok"), _ => Task.FromResult(Result<string>.Success("value")), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
    }
}
