using FluentValidation;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Features.Reports.GetTaskCompletion;

public sealed class GetTaskCompletionQueryValidator : AbstractValidator<GetTaskCompletionQuery>
{
    private static readonly string[] AllowedGroupBy = ["day", "week", "month"];

    public GetTaskCompletionQueryValidator(IOptions<ReportsOptions> options)
    {
        var maxWindowDays = options.Value.MaxWindowDays;

        RuleFor(q => q.From).NotNull().WithMessage("'from' is required.");
        RuleFor(q => q.To).NotNull().WithMessage("'to' is required.");

        RuleFor(q => q.GroupBy)
            .NotNull().WithMessage("'groupBy' is required.")
            .Must(g => g is null || AllowedGroupBy.Contains(g))
            .WithMessage("'groupBy' must be one of: day, week, month.");

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
