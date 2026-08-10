using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ProjectManagementApp.Infrastructure.Persistence;

/// <summary>
/// Translates a caught <see cref="DbUpdateException"/> wrapping a Postgres unique-violation
/// (SQLSTATE 23505) into a recognizable, expected outcome. This — not the application-level
/// pre-check a handler may also run for a friendlier message — is what makes the concurrent
/// duplicate-add guarantee true: the pre-check alone is a TOCTOU race that would 500 on the
/// losing side of a genuine race (research R-3, ADR-0003).
/// </summary>
public static class UniqueViolationMapper
{
    private const string UniqueViolationSqlState = "23505";

    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: UniqueViolationSqlState };

    public static bool IsUniqueViolation(DbUpdateException exception, string indexName) =>
        exception.InnerException is PostgresException pg
        && pg.SqlState == UniqueViolationSqlState
        && pg.ConstraintName == indexName;
}
