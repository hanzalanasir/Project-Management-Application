using FluentValidation;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Features.Reports.GetTeamPerformance;

public sealed class GetTeamPerformanceQueryValidator : AbstractValidator<GetTeamPerformanceQuery>
{
    public GetTeamPerformanceQueryValidator(IOptions<ReportsOptions> options)
    {
        var maxWindowDays = options.Value.MaxWindowDays;

        RuleFor(q => q.From).NotNull().WithMessage("'from' is required.");
        RuleFor(q => q.To).NotNull().WithMessage("'to' is required.");

        RuleFor(q => q)
            .Must(q => q.From is null || q.To is null || ReportWindow.IsOrdered(q.From.Value, q.To.Value))
            .WithMessage("'from' must be on or before 'to'.")
            .WithName("from");

        RuleFor(q => q)
            .Must(q => q.From is null || q.To is null || ReportWindow.WithinMaxSpan(q.From.Value, q.To.Value, maxWindowDays))
            .WithMessage($"The window may not exceed {maxWindowDays} days.")
            .WithName("to");
    }
}
