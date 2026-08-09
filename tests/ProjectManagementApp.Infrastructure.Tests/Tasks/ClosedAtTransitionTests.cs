using FluentAssertions;
using NSubstitute;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Interfaces;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Features.Tasks.UpdateTaskStatus;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Tasks;

// closed_at is a DERIVED side effect (data-model.md §6), never a user-settable field —
// UpdateTaskStatusCommand structurally has no ClosedAt property, so "a request supplying closedAt
// is ignored" is guaranteed at compile time; these tests prove the actual derivation rule against
// real PostgreSQL.
[Collection(PostgresCollection.Name)]
public class ClosedAtTransitionTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public ClosedAtTransitionTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static ApplicationUser NewUser(string email) => new()
    {
        Id = Guid.NewGuid(), UserName = email, NormalizedUserName = email.ToUpperInvariant(),
        Email = email, NormalizedEmail = email.ToUpperInvariant(), FullName = email, IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow,
    };

    private static Project NewProject(string name, ApplicationUser owner) => new()
    {
        Id = Guid.NewGuid(), Name = name, StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Owner = owner, OwnerId = owner.Id, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow,
    };

    private static ICurrentUserService CallerService(Guid userId, string email, string role) =>
        Substitute.For<ICurrentUserService>().Also(s => s.Current.Returns(new CurrentUser(userId, email, role, "Caller")));

    [Fact]
    public async Task TransitioningToDone_SetsClosedAt()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = NewUser("closedat-pm1@example.com");
        var project = NewProject("ClosedAt Project 1", pm);
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "T", Project = project, ProjectId = project.Id, Status = ProjectManagementApp.Domain.Enums.TaskStatus.ToDo, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        db.Users.Add(pm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateTaskStatusCommandHandler(db, new TaskAccessPolicy(db), CallerService(pm.Id, pm.Email!, "ProjectManager"), Substitute.For<IActivityLogService>());
        var result = await handler.Handle(new UpdateTaskStatusCommand(task.Id, "Done", task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task TransitioningAwayFromDone_ClearsClosedAt()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = NewUser("closedat-pm2@example.com");
        var project = NewProject("ClosedAt Project 2", pm);
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "T", Project = project, ProjectId = project.Id, Status = ProjectManagementApp.Domain.Enums.TaskStatus.Done, ClosedAt = DateTimeOffset.UtcNow.AddDays(-1), CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        db.Users.Add(pm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateTaskStatusCommandHandler(db, new TaskAccessPolicy(db), CallerService(pm.Id, pm.Email!, "ProjectManager"), Substitute.For<IActivityLogService>());
        var result = await handler.Handle(new UpdateTaskStatusCommand(task.Id, "InProgress", task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClosedAt.Should().BeNull();
    }

    [Fact]
    public async Task DoneToDone_NoOp_LeavesClosedAtUnchanged()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = NewUser("closedat-pm3@example.com");
        var project = NewProject("ClosedAt Project 3", pm);
        var originalClosedAt = DateTimeOffset.UtcNow.AddDays(-3);
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "T", Project = project, ProjectId = project.Id, Status = ProjectManagementApp.Domain.Enums.TaskStatus.Done, ClosedAt = originalClosedAt, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        db.Users.Add(pm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateTaskStatusCommandHandler(db, new TaskAccessPolicy(db), CallerService(pm.Id, pm.Email!, "ProjectManager"), Substitute.For<IActivityLogService>());
        var result = await handler.Handle(new UpdateTaskStatusCommand(task.Id, "Done", task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClosedAt.Should().BeCloseTo(originalClosedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task NonDoneToNonDone_ClosedAtStaysUntouched()
    {
        await using var db = _fixture.CreateDbContext();
        var pm = NewUser("closedat-pm4@example.com");
        var project = NewProject("ClosedAt Project 4", pm);
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "T", Project = project, ProjectId = project.Id, Status = ProjectManagementApp.Domain.Enums.TaskStatus.ToDo, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        db.Users.Add(pm);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateTaskStatusCommandHandler(db, new TaskAccessPolicy(db), CallerService(pm.Id, pm.Email!, "ProjectManager"), Substitute.For<IActivityLogService>());
        var result = await handler.Handle(new UpdateTaskStatusCommand(task.Id, "InProgress", task.Version), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClosedAt.Should().BeNull();
    }
}

internal static class SubstituteAlsoExtensions
{
    public static T Also<T>(this T value, Action<T> action)
    {
        action(value);
        return value;
    }
}
