using FluentValidation;

namespace ProjectManagementApp.Application.Features.Team.AddTeamMember;

// Invoked automatically by ValidationBehavior — never called by the handler (ADR-0005). Database-
// dependent rules (project existence, user existence/active, already-a-member) live in the handler
// instead, since they need the loaded parent Project (research.md R-1/R-2/R-3).
public class AddTeamMemberCommandValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
    }
}
