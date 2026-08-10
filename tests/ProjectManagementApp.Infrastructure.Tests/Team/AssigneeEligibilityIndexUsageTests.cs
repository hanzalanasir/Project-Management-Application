using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Domain.Entities;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Team;

// T059 (NFR-002): 003's AssigneeValidator.IsEligibleAsync filters on
// `tm.ProjectId == projectId && tm.UserId == assigneeId` — exactly the two columns
// `ux_team_members_project_id_user_id` covers. Proves the query PLANNER can and does use that
// index for this exact predicate shape, rather than assuming it from the WHERE clause alone.
// `enable_seqscan = off` is set first: on a freshly-seeded, tiny test table Postgres's cost-based
// planner prefers a sequential scan regardless of available indexes (a well-known EXPLAIN-testing
// pitfall), which would make this test report a false negative at real production scale. Forcing
// seqscan off isolates the real question — "is this index usable for this predicate at all" — from
// "did the planner judge it worthwhile on 2 rows."
[Collection(PostgresCollection.Name)]
public class AssigneeEligibilityIndexUsageTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public AssigneeEligibilityIndexUsageTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    private static ApplicationUser NewUser(string email) => new()
    {
        Id = Guid.NewGuid(),
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        FullName = email,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    private static Project NewProject(string name, ApplicationUser owner) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Owner = owner,
        OwnerId = owner.Id,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task AssigneeEligibilityPredicate_ProjectIdAndUserIdEquality_UsesTheUniqueIndex()
    {
        var owner = NewUser("index-usage-owner@example.com");
        var member = NewUser("index-usage-member@example.com");
        var project = NewProject("Index Usage Project", owner);

        await using (var seed = _fixture.CreateDbContext())
        {
            seed.Users.AddRange(owner, member);
            seed.Projects.Add(project);
            seed.TeamMembers.Add(new TeamMember { Id = Guid.NewGuid(), ProjectId = project.Id, UserId = member.Id, CreatedAt = DateTimeOffset.UtcNow });
            await seed.SaveChangesAsync(CancellationToken.None);
        }

        await using var db = _fixture.CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("SET enable_seqscan = off");

        var planLines = await db.Database
            .SqlQuery<string>($@"
                EXPLAIN SELECT 1 FROM team_members tm
                JOIN users u ON u.id = tm.user_id
                WHERE tm.project_id = {project.Id} AND tm.user_id = {member.Id} AND u.is_active = true")
            .ToListAsync();

        var plan = string.Join('\n', planLines);
        plan.Should().Contain("ux_team_members_project_id_user_id",
            $"the (project_id, user_id) equality predicate should be servable by the unique index, but the plan was:\n{plan}");
    }
}
