using FluentAssertions;
using NSubstitute;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.Common;

namespace ProjectManagementApp.Application.Tests.Features.Reports;

// T018: exactly one ReportGenerated row per invocation, correct entity_type, the parameters
// captured in change_summary, and the AuditOnGeneration gate respected. This helper takes no
// IApplicationDbContext at all — by construction it has no way to touch a domain entity, which is
// the structural half of "no domain entity is written" (the functional half, proven against a real
// database, is T081/T036 in later stages).
public class ReportGenerationAuditTests
{
    private static readonly CurrentUser Caller = new(Guid.NewGuid(), "pm@example.com", "ProjectManager", "PM");

    [Fact]
    public async Task RecordAsync_WritesExactlyOneRow_WithReportEntityType()
    {
        var activityLog = Substitute.For<IActivityLogService>();
        var options = new ReportsOptions { AuditOnGeneration = true };

        await ReportGenerationAudit.RecordAsync(activityLog, options, Caller, "ProjectProgress",
            new { from = "2026-07-01", to = "2026-07-31" }, CancellationToken.None);

        await activityLog.Received(1).LogAsync(
            Caller.UserId, "ReportGenerated", "Report", Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>(), projectId: null);
    }

    [Fact]
    public async Task RecordAsync_ChangeSummary_CapturesReportTypeAndParameters()
    {
        var activityLog = Substitute.For<IActivityLogService>();
        var options = new ReportsOptions { AuditOnGeneration = true };
        string? capturedSummary = null;
        await activityLog.LogAsync(
            Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string>(s => capturedSummary = s),
            Arg.Any<CancellationToken>(), Arg.Any<Guid?>());

        await ReportGenerationAudit.RecordAsync(activityLog, options, Caller, "TeamPerformance",
            new { userId = "self-clamped" }, CancellationToken.None);

        capturedSummary.Should().Contain("TeamPerformance").And.Contain("self-clamped");
    }

    [Fact]
    public async Task RecordAsync_EachInvocation_UsesADistinctRunId()
    {
        var activityLog = Substitute.For<IActivityLogService>();
        var options = new ReportsOptions { AuditOnGeneration = true };
        var capturedIds = new List<string>();
        await activityLog.LogAsync(
            Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string>(id => capturedIds.Add(id)), Arg.Any<string>(),
            Arg.Any<CancellationToken>(), Arg.Any<Guid?>());

        await ReportGenerationAudit.RecordAsync(activityLog, options, Caller, "Activity", new { }, CancellationToken.None);
        await ReportGenerationAudit.RecordAsync(activityLog, options, Caller, "Activity", new { }, CancellationToken.None);

        capturedIds.Should().HaveCount(2);
        capturedIds[0].Should().NotBe(capturedIds[1]);
    }

    [Fact]
    public async Task RecordAsync_AuditOnGenerationDisabled_NeverCallsLogAsync()
    {
        var activityLog = Substitute.For<IActivityLogService>();
        var options = new ReportsOptions { AuditOnGeneration = false };

        await ReportGenerationAudit.RecordAsync(activityLog, options, Caller, "TaskCompletion", new { }, CancellationToken.None);

        await activityLog.DidNotReceiveWithAnyArgs().LogAsync(
            default, default!, default!, default!, default!, default, default);
    }
}
