using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Options;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Common.Options;
using ProjectManagementApp.Application.Features.Reports.GetActivityReport;

namespace ProjectManagementApp.Application.Tests.Features.Reports;

// T068: the threshold guard's probe (page=1, pageSize=1) runs BEFORE the real scoped read (the
// requested pageSize), which runs BEFORE the audit. An Admin caller needs no IApplicationDbContext
// scope resolution at all (Unscoped fast path), so this is a pure NSubstitute unit test — no
// Postgres needed to prove call ordering.
public class GetActivityReportQueryHandlerTests
{
    private static CurrentUser AdminCaller => new(Guid.NewGuid(), "admin@test", "Admin", "Admin");

    private static GetActivityReportQueryHandler MakeHandler(
        IApplicationDbContext db, IActivityLogService activityLogService, ICurrentUserService currentUserService) =>
        new(db, Substitute.For<ProjectManagementApp.Application.Common.Interfaces.IProjectAccessPolicy>(),
            currentUserService, activityLogService, Options.Create(new ReportsOptions { LargeReportRowThreshold = 5 }));

    private static ICurrentUserService CurrentUserFor(CurrentUser caller)
    {
        var service = Substitute.For<ICurrentUserService>();
        service.Current.Returns(caller);
        return service;
    }

    [Fact]
    public async Task Handle_OverThreshold_OnlyCallsTheProbe_NeverTheRealPage_AndNeverAudits()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var activityLogService = Substitute.For<IActivityLogService>();

        // The probe (pageSize=1) reports a scoped total over the threshold (5).
        activityLogService.QueryScopedAsync(
                Arg.Any<ActivityScope>(), 1, 1, Arg.Any<CancellationToken>(),
                Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<Guid?>())
            .Returns(new PagedResult<ActivityEntry>([], 1, 1, TotalCount: 6, TotalPages: 6));

        var handler = MakeHandler(db, activityLogService, CurrentUserFor(AdminCaller));

        var result = await handler.Handle(
            new GetActivityReportQuery(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31), null, null, null, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Kind.Should().Be(ErrorKind.UnprocessableContent);

        // The real page-1 read (the requested pageSize=20) must never have been called.
        await activityLogService.DidNotReceive().QueryScopedAsync(
            Arg.Any<ActivityScope>(), Arg.Any<int>(), 20, Arg.Any<CancellationToken>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<Guid?>());
        await activityLogService.DidNotReceiveWithAnyArgs().LogAsync(default, default!, default!, default!, default!, default);
        await db.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_UnderThreshold_CallsProbeThenRealPageThenAudits_InThatOrder()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var activityLogService = Substitute.For<IActivityLogService>();

        activityLogService.QueryScopedAsync(
                Arg.Any<ActivityScope>(), 1, 1, Arg.Any<CancellationToken>(),
                Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<Guid?>())
            .Returns(new PagedResult<ActivityEntry>([], 1, 1, TotalCount: 2, TotalPages: 2));

        activityLogService.QueryScopedAsync(
                Arg.Any<ActivityScope>(), 1, 20, Arg.Any<CancellationToken>(),
                Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<Guid?>())
            .Returns(new PagedResult<ActivityEntry>([], 1, 20, TotalCount: 2, TotalPages: 1));

        var handler = MakeHandler(db, activityLogService, CurrentUserFor(AdminCaller));

        var result = await handler.Handle(
            new GetActivityReportQuery(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31), null, null, null, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        Received.InOrder(() =>
        {
            activityLogService.QueryScopedAsync(
                Arg.Any<ActivityScope>(), 1, 1, Arg.Any<CancellationToken>(),
                Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<Guid?>());
            activityLogService.QueryScopedAsync(
                Arg.Any<ActivityScope>(), 1, 20, Arg.Any<CancellationToken>(),
                Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<Guid?>());
            activityLogService.LogAsync(
                Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        });
    }
}
