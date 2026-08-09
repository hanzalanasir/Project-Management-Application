using FluentAssertions;
using ProjectManagementApp.Application.Common.Authorization;
using ProjectManagementApp.Application.Common.Models;
using ProjectManagementApp.Application.Tests.Builders;
using ProjectManagementApp.Application.Tests.Fixtures;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Domain.Enums;

namespace ProjectManagementApp.Application.Tests.Authorization;

// The 15-cell TaskMutation x role matrix (data-model.md §3) — the heart of the feature. Asserts
// StatusChange is the ONLY cell a TeamMember passes (DoD #3): the same TeamMember, on the same
// row, must be denied every other mutation kind. Runs against real PostgreSQL (ADR-0007 §2) since
// the PM/TM branches query db.Projects/AssigneeId, matching the pattern already proven for
// ProjectAccessPolicy.
[Collection(PostgresCollection.Name)]
public class TaskAccessPolicyMatrixTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public TaskAccessPolicyMatrixTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static CurrentUser Caller(Guid userId, Role role) =>
        new(userId, "caller@matrix-test.com", role.ToString(), "Caller");

    public static IEnumerable<object[]> AllCells()
    {
        foreach (TaskMutation mutation in Enum.GetValues<TaskMutation>())
        {
            foreach (Role role in Enum.GetValues<Role>())
            {
                var expectedAllowed = role switch
                {
                    Role.Admin => true,
                    Role.ProjectManager => true,
                    Role.TeamMember => mutation == TaskMutation.StatusChange,
                    _ => throw new ArgumentOutOfRangeException(nameof(role)),
                };
                yield return new object[] { mutation, role, expectedAllowed };
            }
        }
    }

    [Theory]
    [MemberData(nameof(AllCells))]
    public async Task CanMutateAsync_FifteenCellMatrix_MatchesDataModelDecisionTable(
        TaskMutation mutation, Role role, bool expectedAllowed)
    {
        await using var db = _fixture.CreateDbContext();

        var pm = new ApplicationUserBuilder().WithEmail($"pm-{mutation}-{role}@matrix-test.com").Build();
        var assignee = new ApplicationUserBuilder().WithEmail($"assignee-{mutation}-{role}@matrix-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Project = project,
            Title = "Matrix Task",
            AssigneeId = assignee.Id,
            Assignee = assignee,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        db.Users.AddRange(pm, assignee);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TaskAccessPolicy(db);

        // Caller identity for each role: PM must be the owner and TM must be the assignee for the
        // matrix's "owns parent" / "is assignee" preconditions to hold — an Admin's id is
        // irrelevant since Admin is unscoped.
        var callerId = role switch
        {
            Role.Admin => Guid.NewGuid(),
            Role.ProjectManager => pm.Id,
            Role.TeamMember => assignee.Id,
            _ => throw new ArgumentOutOfRangeException(nameof(role)),
        };

        var decision = await policy.CanMutateAsync(task, mutation, Caller(callerId, role), CancellationToken.None);

        decision.Allowed.Should().Be(expectedAllowed, because: $"{role} {(expectedAllowed ? "should" : "should not")} be able to perform {mutation}");
    }

    [Fact]
    public async Task CanMutateAsync_TeamMember_StatusChangeAllowed_ButEveryOtherMutationDenied_OnTheSameRow()
    {
        await using var db = _fixture.CreateDbContext();

        var pm = new ApplicationUserBuilder().WithEmail("pm-graduated@matrix-test.com").Build();
        var assignee = new ApplicationUserBuilder().WithEmail("assignee-graduated@matrix-test.com").Build();
        var project = new ProjectBuilder().WithOwner(pm).Build();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Project = project,
            Title = "Graduated Task",
            AssigneeId = assignee.Id,
            Assignee = assignee,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        db.Users.AddRange(pm, assignee);
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(CancellationToken.None);

        var policy = new TaskAccessPolicy(db);
        var caller = Caller(assignee.Id, Role.TeamMember);

        (await policy.CanMutateAsync(task, TaskMutation.StatusChange, caller, CancellationToken.None)).Allowed.Should().BeTrue();
        (await policy.CanMutateAsync(task, TaskMutation.FullEdit, caller, CancellationToken.None)).Allowed.Should().BeFalse();
        (await policy.CanMutateAsync(task, TaskMutation.Create, caller, CancellationToken.None)).Allowed.Should().BeFalse();
        (await policy.CanMutateAsync(task, TaskMutation.Reassign, caller, CancellationToken.None)).Allowed.Should().BeFalse();
        (await policy.CanMutateAsync(task, TaskMutation.Delete, caller, CancellationToken.None)).Allowed.Should().BeFalse();
    }
}
