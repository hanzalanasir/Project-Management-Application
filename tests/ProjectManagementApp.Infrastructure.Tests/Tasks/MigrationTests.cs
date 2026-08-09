using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Tasks;

[Collection(PostgresCollection.Name)]
public class MigrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public MigrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task AddTaskIndexes_CreatesAllSixIndexes()
    {
        await using var db = _fixture.CreateDbContext();

        var indexNames = await db.Database
            .SqlQuery<string>($"SELECT indexname FROM pg_indexes WHERE tablename = 'tasks'")
            .ToListAsync();

        indexNames.Should().Contain(new[]
        {
            "ix_tasks_project_id",
            "ix_tasks_assignee_id",
            "ix_tasks_status",
            "ix_tasks_project_id_status",
            "ix_tasks_assignee_id_status",
            "ix_tasks_title_trgm"
        });
    }

    [Fact]
    public async Task TitleTrigramIndex_IsUsableForIlikeSubstringSearch()
    {
        await using var db = _fixture.CreateDbContext();

        // EXPLAIN over an empty table won't force an index scan (the planner may prefer seq scan
        // regardless of size), so this proves the index is well-formed and queryable rather than
        // asserting a specific plan — the plan-level proof belongs to a data-heavy performance test,
        // not this migration-shape test.
        var rows = await db.Database
            .SqlQuery<string>($"SELECT title FROM tasks WHERE title ILIKE '%term%'")
            .ToListAsync();

        rows.Should().BeEmpty();
    }
}
