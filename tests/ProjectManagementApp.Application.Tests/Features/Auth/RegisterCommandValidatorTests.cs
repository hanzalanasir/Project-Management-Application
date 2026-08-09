using FluentAssertions;
using FluentValidation.TestHelper;
using ProjectManagementApp.Application.Features.Auth.Register;

namespace ProjectManagementApp.Application.Tests.Features.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyFullName_HasError()
    {
        var result = _validator.TestValidate(new RegisterCommand("", "a@b.com", "Passw0rd!", "Passw0rd!"));
        result.ShouldHaveValidationErrorFor(c => c.FullName);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void Validate_InvalidEmail_HasError(string email)
    {
        var result = _validator.TestValidate(new RegisterCommand("Dana", email, "Passw0rd!", "Passw0rd!"));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_PasswordTooShort_HasError()
    {
        var result = _validator.TestValidate(new RegisterCommand("Dana", "a@b.com", "Sh0rt!", "Sh0rt!"));
        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void Validate_ConfirmPasswordMismatch_HasError()
    {
        var result = _validator.TestValidate(new RegisterCommand("Dana", "a@b.com", "Passw0rd!", "Different1!"));
        result.ShouldHaveValidationErrorFor(c => c.ConfirmPassword);
    }

    [Fact]
    public void Validate_AllFieldsValid_HasNoErrors()
    {
        var result = _validator.TestValidate(new RegisterCommand("Dana Rivera", "dana@example.com", "Passw0rd!", "Passw0rd!"));
        result.IsValid.Should().BeTrue();
    }
}
