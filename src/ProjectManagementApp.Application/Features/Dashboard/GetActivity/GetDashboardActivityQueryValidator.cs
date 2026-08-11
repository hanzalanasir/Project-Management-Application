using FluentValidation;

namespace ProjectManagementApp.Application.Features.Dashboard.GetActivity;

// PageSize is deliberately NOT validated — oversized values are clamped by the handler, never
// rejected (Constitution VI.4, same convention as 002-004's list endpoints).
public class GetDashboardActivityQueryValidator : AbstractValidator<GetDashboardActivityQuery>
{
    public GetDashboardActivityQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
    }
}
