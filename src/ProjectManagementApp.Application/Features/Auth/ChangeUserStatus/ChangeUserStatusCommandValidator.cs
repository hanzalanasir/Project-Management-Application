using FluentValidation;

namespace ProjectManagementApp.Application.Features.Auth.ChangeUserStatus;

public class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
{
    // IsActive is a bool — always "present" once bound; nothing further to validate here.
    // Kept as a distinct validator to match every other command's shape (ADR-0005).
}
