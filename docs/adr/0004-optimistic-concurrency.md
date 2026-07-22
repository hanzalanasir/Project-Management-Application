# ADR-0004: Optimistic concurrency via PostgreSQL `xmin`

**Status**: Accepted · **Date**: 2026-07-22 · **Relates to**: Constitution III (EF Core + Npgsql), VI.2

## Context

Multiple users legitimately edit the same row: an Admin and the owning ProjectManager may both edit a
project, and a ProjectManager and an assigned TeamMember may both update a task. Without a
concurrency token, EF Core issues a blind `UPDATE` and the second writer silently discards the first
writer's changes, with no signal to either user.

## Decision

User-editable domain entities (`projects`, `tasks`, `users`) map PostgreSQL's system column **`xmin`**
as an EF Core concurrency token:

```csharp
builder.Property<uint>("xmin").IsRowVersion().HasColumnName("xmin");
```

- No extra column, no extra migration surface — `xmin` already exists on every PostgreSQL row.
- A stale update raises `DbUpdateConcurrencyException`, which the service converts to
  `ErrorKind.Conflict` → **409 Conflict** as ProblemDetails (ADR-0003).
- The API returns the current row version to clients; the UI prompts the user to reload and reapply.

Append-only and system-managed tables (`activity_logs`, `refresh_tokens`) are **excluded** — they are
never concurrently edited.

## Alternatives rejected

- **No concurrency control** — silent data loss on a genuinely reachable path; unacceptable for a tool
  whose whole purpose is coordinating shared work.
- **Explicit `row_version` column** — more portable across database engines, but adds a column and
  maintenance to every entity for portability the locked PostgreSQL 18 stack (Constitution III) does
  not need.

## Consequences

Concurrent edits now fail loudly with 409 instead of losing data. Every feature spec must document
409 in its status-code table, and every update endpoint needs a conflict test. `xmin` is
PostgreSQL-specific: migrating engines would require replacing this with an explicit column.
