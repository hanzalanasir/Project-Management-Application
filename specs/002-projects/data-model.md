# Phase 1 Data Model: 002 Project Management

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Research**: [research.md](research.md)
**Source of truth**: spec 002 §Consolidated Data Model + B.1. This file adds the CLR shape, the scope
predicates, and the migration plan — it introduces no column the spec did not already commit to.

EF Core 10 + Npgsql · PostgreSQL 18 · snake_case (VIII.2) · `timestamptz` UTC · calendar dates as `date`.

---

## 1. What already exists vs. what 002 adds — read this first

Per [research.md R-4](research.md), the `projects` **table already exists**: 001's `InitialCreate` builds
all five constitution entities, and 001's EF configuration already declares the columns and delete
behaviors. 002 does **not** create the table.

| Artifact | Created by | 002's action |
|---|---|---|
| `projects` table + columns + FKs | 001 `InitialCreate` | — already present |
| `projects.owner_id` → `users` **RESTRICT** | 001 config | **prove by test** (R-5) |
| `tasks.project_id` → `projects` **CASCADE** | 001 config | **prove by test** (R-5) |
| `team_members.project_id` → `projects` **CASCADE** | 001 config | **prove by test** (R-5) |
| `xmin` row-version mapping on `projects` | 001 config | **first feature to actually contend on it** (R-2) |
| Indexes `(owner_id)`, `(status)`, `(owner_id,status)` | — | ✅ **`AddProjectIndexes`** |
| `pg_trgm` extension + GIN trigram index on `name` | — | ✅ **`AddProjectIndexes`** |
| Business rules, API, UI | — | ✅ all of 002 |

> ✅ Spec 002 B.1 previously named this migration `AddProjectsTable`; **corrected to `AddProjectIndexes`
> on 2026-07-31** (see R-4). If 001 is not yet implemented, fold these indexes into `InitialCreate` and
> 002 adds **no** migration at all.

---

## 2. `Project` entity (`ProjectManagementApp.Domain/Entities/Project.cs`)

The class is authored in 001 as table-only; 002 owns its rules. Final shape:

| Property | Type | Null | Notes |
|---|---|---|---|
| `Id` | `Guid` | ✘ | PK |
| `Name` | `string` | ✘ | varchar(200); **not unique** — duplicates permitted (OQ-002-06) |
| `Description` | `string?` | ✔ | varchar(2000) |
| `StartDate` | `DateOnly` | ✘ | `date` |
| `EndDate` | `DateOnly?` | ✔ | `date`; **CHECK** `end_date IS NULL OR end_date >= start_date` |
| `Status` | `ProjectStatus` | ✘ | varchar(20), default `Planning`, stored **as string** |
| `OwnerId` | `Guid` | ✘ | FK → `users`, **ON DELETE RESTRICT** |
| `Owner` | `ApplicationUser` | — | navigation (IV.3) |
| `CreatedAt` / `UpdatedAt` | `DateTimeOffset` | ✘ | UTC |
| `Version` | `uint` | ✘ | `xmin`, `IsRowVersion()` — stale write → **409** (ADR-0004) |
| `Tasks` | `ICollection<TaskItem>` | — | navigation; cascade on project delete (003) |
| `TeamMembers` | `ICollection<TeamMember>` | — | navigation; **required by `ApplyScope`** (004) |

**`TeamMembers` is not decorative.** The TeamMember scope predicate composes through it
(`p.TeamMembers.Any(tm => tm.UserId == caller.UserId)`), so the navigation must exist for the scoped query
to translate to SQL. This is why 001 creates the table on day one.

### Enum (`ProjectStatus`, spec B.2)
`Planning` (default) · `Active` · `OnHold` · `Completed` · `Cancelled` — persisted as **string** for
readability and immunity to reordering. `Completed`/`Cancelled` are *terminal* only by convention; 002
imposes no transition rules and no cascade on reaching them.

---

## 3. The scope predicates (the heart of the model)

Implemented by `ProjectAccessPolicy` in `.Application` (R-1). These are the **security-critical**
expressions of this feature — everything else is CRUD around them.

| Caller role | `ApplyScope` predicate | Meaning |
|---|---|---|
| `Admin` | *(none)* | all rows |
| `ProjectManager` | `p => p.OwnerId == caller.UserId` | only projects they own |
| `TeamMember` | `p => p.TeamMembers.Any(tm => tm.UserId == caller.UserId)` | only projects they are assigned to |

`CanReadAsync(project, caller)` and `CanMutateAsync(project, caller)` evaluate the same facts for a single
already-loaded entity, at the moment of the operation:

| | Admin | ProjectManager | TeamMember |
|---|---|---|---|
| `CanReadAsync` | allow | allow iff owner | allow iff assigned |
| `CanMutateAsync` | allow | allow iff owner | **deny** (also blocked earlier at the role gate) |

**Two invariants the tests must hold:**
1. **The predicate is composed into the `IQueryable` before `Count()` and before `Skip/Take`.** Out-of-scope
   rows must never be loaded, counted, or reflected in paging metadata (FR-007, NFR-002). This is why the
   no-repository decision is load-bearing (001 R-3, shared-contracts §7).
2. **`CanMutateAsync` is re-evaluated at write time**, never trusted from the read that loaded the entity —
   a stale read must not authorize a later mutation (FR-010, T.8).

---

## 4. Migration `AddProjectIndexes`

```
CREATE EXTENSION IF NOT EXISTS pg_trgm;              -- permitted DDL, not data access (R-3)

CREATE INDEX ix_projects_owner_id         ON projects (owner_id);
CREATE INDEX ix_projects_status           ON projects (status);
CREATE INDEX ix_projects_owner_id_status  ON projects (owner_id, status);
CREATE INDEX ix_projects_name_trgm        ON projects USING gin (name gin_trgm_ops);
```

**Why each exists:**
- `(owner_id)` — the ProjectManager scope predicate; the single hottest filter in the feature.
- `(status)` — the `?status=` filter, and later 005 Dashboard's projects-by-status aggregate.
- `(owner_id, status)` — the common composite: a manager filtering their own projects by status.
- **GIN trigram on `name`** — serves `ILIKE '%term%'`. A B-tree **cannot** (R-3); without this the search
  degrades to a sequential scan.

There is deliberately **no index for the TeamMember scope** on `projects` — that predicate resolves through
`team_members`, whose `(project_id, user_id)` unique index (owned by 004) is what makes it fast.

---

## 5. Query composition order (fixed — deviation is a bug)

`ListProjectsQueryHandler` must build the query in exactly this order:

1. `db.Projects` → base `IQueryable`
2. **`ApplyScope(query, caller)`** ← scope first, always
3. `?search=` → `ILIKE` on name/description
4. `?status=` → equality
5. **`CountAsync()`** → `totalCount`, computed on the scoped **and** filtered query
6. `?sort=` → whitelist-mapped `OrderBy`
7. `Skip/Take` → paging, `pageSize` **clamped** to the configured max, never rejected
8. Project to `ProjectSummaryDto` and materialize — **one** round trip

Steps 2 and 5 are the security-relevant ones: counting before scoping, or paging before counting, would
leak the existence of out-of-scope projects through `totalCount`/`totalPages`.

---

## 6. Transactional invariants

1. **Write + audit commit together.** Every create/update/delete/ownership-change writes its
   `activity_logs` row in the **same** `SaveChangesAsync` (IV.4). A project change cannot exist without its
   audit entry.
2. **Delete audits *before* removal** — the `ProjectDeleted` row (with a pre-removal snapshot summary) is
   written first, so the audit survives the cascade (spec US-002-05, FR-012).
3. **`activity_logs` is never cascaded away**, so a deleted project's history stays queryable by 006.
4. **Concurrency is checked by the database, not the application** — `If-Match` sets EF's original
   `xmin`; PostgreSQL enforces it in the `UPDATE … WHERE xmin = …`.

---

## 7. Deliberate non-goals

- **No soft-delete column.** Hard delete with cascade is resolved (Clarifications 2026-07-22);
  `activity_logs` is the recovery story.
- **No uniqueness on `name`.** Duplicates are permitted and disambiguated by id + owner (OQ-002-06).
- **No `is_archived` / template / clone columns** — bonus scope (Constitution I.2).
- **No new shared-kernel abstraction.** 002 *implements* `IProjectAccessPolicy` (shared-contracts §3) and
  *consumes* `IApplicationDbContext` (§7) and `IActivityLogService` (§6); it declares nothing new.
