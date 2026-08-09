using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectManagementApp.Infrastructure.Persistence;
using Respawn;
using Testcontainers.PostgreSql;

namespace ProjectManagementApp.Application.Tests.Fixtures;

// Real PostgreSQL wherever EF is involved — never EF InMemory (ADR-0007 §2). Application.Tests
// needs its own fixture (distinct from Infrastructure.Tests.Fixtures.PostgresFixture) because test
// projects don't reference one another; ProjectAccessPolicy's scope predicate must be proven to
// reach SQL (data-model.md §3 invariant 1), which is unrepresentable against any in-memory provider.
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("projectmanagementapp_apptest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _respawnConnection = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();

        _respawnConnection = new NpgsqlConnection(_container.GetConnectionString());
        await _respawnConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_respawnConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    public async Task ResetAsync() => await _respawner.ResetAsync(_respawnConnection);

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options);
    }

    public async Task DisposeAsync()
    {
        await _respawnConnection.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
