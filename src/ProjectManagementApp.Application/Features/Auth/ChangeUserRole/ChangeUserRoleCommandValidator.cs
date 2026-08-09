using FluentValidation;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Features.Auth.ChangeUserRole;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(c => c.Role).NotEmpty().Must(r => Enum.TryParse<Role>(r, out _)).WithMessage("Role must be one of Admin, ProjectManager, TeamMember.");
    }
}
