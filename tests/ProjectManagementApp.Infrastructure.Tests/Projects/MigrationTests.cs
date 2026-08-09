using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Projects;

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
    public async Task AddProjectIndexes_CreatesAllFourIndexes()
    {
        await using var db = _fixture.CreateDbContext();

        var indexNames = await db.Database
            .SqlQuery<string>($"SELECT indexname FROM pg_indexes WHERE tablename = 'projects'")
            .ToListAsync();

        indexNames.Should().Contain(new[]
        {
            "ix_projects_owner_id",
            "ix_projects_status",
            "ix_projects_owner_id_status",
            "ix_projects_name_trgm"
        });
    }

    [Fact]
    public async Task PgTrgmExtension_IsEnabled()
    {
        await using var db = _fixture.CreateDbContext();

        var extensionNames = await db.Database
            .SqlQuery<string>($"SELECT extname FROM pg_extension WHERE extname = 'pg_trgm'")
            .ToListAsync();

        extensionNames.Should().ContainSingle();
    }
}
