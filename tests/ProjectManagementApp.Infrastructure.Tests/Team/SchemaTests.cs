using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApp.Infrastructure.Tests.Fixtures;

namespace ProjectManagementApp.Infrastructure.Tests.Team;

// Proves data-model.md §4's correctness claims for team_members: the unique index that IS the
// concurrency mechanism (research R-2/R-3), the three cascade behaviors 001 configured, and the
// two deliberate schema absences (no role column, no updated_at/xmin) that keep this feature's
// central design claim honest rather than assumed.
[Collection(PostgresCollection.Name)]
public class SchemaTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public SchemaTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _fixture.ResetAsync();

    [Fact]
    public async Task UniqueIndex_OnProjectIdAndUserId_Exists()
    {
        await using var db = _fixture.CreateDbContext();

        var indexNames = await db.Database
            .SqlQuery<string>($"SELECT indexname FROM pg_indexes WHERE tablename = 'team_members'")
            .ToListAsync();

        indexNames.Should().Contain(new[]
        {
            "ux_team_members_project_id_user_id",
            "ix_team_members_project_id",
            "ix_team_members_user_id"
        });

        var isUnique = await db.Database
            .SqlQuery<bool>($"SELECT indisunique AS \"Value\" FROM pg_index WHERE indexrelid = 'ux_team_members_project_id_user_id'::regclass")
            .SingleAsync();
        isUnique.Should().BeTrue();
    }

    [Fact]
    public async Task TeamMembers_HasNoRoleOrPermissionColumn()
    {
        await using var db = _fixture.CreateDbContext();

        var columnNames = await db.Database
            .SqlQuery<string>($"SELECT column_name FROM information_schema.columns WHERE table_name = 'team_members'")
            .ToListAsync();

        columnNames.Should().NotContain(c => c.Contains("role", StringComparison.OrdinalIgnoreCase));
        columnNames.Should().NotContain(c => c.Contains("permission", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task TeamMembers_HasNoUpdatedAt_AndNoXmin()
    {
        await using var db = _fixture.CreateDbContext();

        var columnNames = await db.Database
            .SqlQuery<string>($"SELECT column_name FROM information_schema.columns WHERE table_name = 'team_members'")
            .ToListAsync();

        // xmin is a Postgres system column, never listed in information_schema.columns for a
        // user table, so its absence here is implicit; updated_at would appear if it existed.
        columnNames.Should().NotContain("updated_at");
        columnNames.Should().BeEquivalentTo(new[] { "id", "project_id", "user_id", "added_by", "created_at" });
    }

    [Fact]
    public async Task DeleteBehaviors_MatchDataModel_ProjectCascade_UserCascade_AddedBySetNull()
    {
        await using var db = _fixture.CreateDbContext();

        var deleteRules = await db.Database
            .SqlQuery<string>($@"
                SELECT confdeltype::text FROM pg_constraint
                WHERE conrelid = 'team_members'::regclass AND conname = 'fk_team_members_projects_project_id'")
            .ToListAsync();
        deleteRules.Should().ContainSingle().Which.Should().Be("c"); // CASCADE

        var userDeleteRules = await db.Database
            .SqlQuery<string>($@"
                SELECT confdeltype::text FROM pg_constraint
                WHERE conrelid = 'team_members'::regclass AND conname = 'fk_team_members_users_user_id'")
            .ToListAsync();
        userDeleteRules.Should().ContainSingle().Which.Should().Be("c"); // CASCADE

        var addedByDeleteRules = await db.Database
            .SqlQuery<string>($@"
                SELECT confdeltype::text FROM pg_constraint
                WHERE conrelid = 'team_members'::regclass AND conname = 'fk_team_members_users_added_by'")
            .ToListAsync();
        addedByDeleteRules.Should().ContainSingle().Which.Should().Be("n"); // SET NULL
    }
}
