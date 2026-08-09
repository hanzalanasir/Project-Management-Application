using FluentValidation;

namespace ProjectManagementApp.Application.Features.Auth.Register;

// Invoked automatically by ValidationBehavior — never called by the handler (ADR-0005).
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
        RuleFor(c => c.ConfirmPassword).Equal(c => c.Password).WithMessage("Passwords do not match.");
    }
}
