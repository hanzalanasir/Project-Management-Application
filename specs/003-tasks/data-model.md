# Phase 1 Data Model: 003 Task Management

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Research**: [research.md](research.md)
**Source of truth**: spec 003 §Consolidated Data Model + B.1. This file adds the CLR shape, the graduated
authorization matrix, and the migration plan — it introduces no column the spec did not commit to.

EF Core 10 + Npgsql · PostgreSQL 18 · snake_case (VIII.2) · `timestamptz` UTC · calendar dates as `date`.

---

## 1. What already exists vs. what 003 adds

Per [research R-7](research.md), the `tasks` table — **including `closed_at`** — already exists from 001's
`InitialCreate`. 003 does **not** create it.

| Artifact | Created by | 003's action |
|---|---|---|
| `tasks` table, columns (incl. `closed_at`), FKs | 001 `InitialCreate` | — already present |
| `tasks.project_id` → `projects` **CASCADE** | 001 config | prove by test |
| `tasks.assignee_id` → `users` **RESTRICT** | 001 config | prove by test |
| `xmin` row-version mapping | 001 config | contended on **three** `PUT`s (R-5) |
| `pg_trgm` extension | **002** `AddProjectIndexes` | reused — not re-issued |
| Six indexes on `tasks` | — | ✅ **`AddTaskIndexes`** |
| `TaskMutation` enum | ✅ **001** (T020, fixed 2026-07-31) | consumed here — see [research R-1](research.md) |
| Business rules, API, UI | — | ✅ all of 003 |

> ✅ Spec 003 B.1 previously named this migration `AddTasksTable`; **corrected to `AddTaskIndexes` on
> 2026-07-31** (see R-7).

---

## 2. `TaskItem` entity (`ProjectManagementApp.Domain/Entities/TaskItem.cs`)

**CLR name is `TaskItem`, not `Task`** — deliberately, to avoid colliding with
`System.Threading.Tasks.Task` throughout an async codebase (spec §Consolidated Data Model). It maps to the
`tasks` table.

| Property | Type | Null | Notes |
|---|---|---|---|
| `Id` | `Guid` | ✘ | PK |
| `ProjectId` | `Guid` | ✘ | FK → `projects`, **CASCADE**. From the **route**, never the body; **immutable** after create (FR-003, OQ-003-05) |
| `Title` | `string` | ✘ | varchar(200) |
| `Description` | `string?` | ✔ | varchar(2000) |
| `Status` | `TaskStatus` | ✘ | varchar(20), default `ToDo`, stored **as string** |
| `Priority` | `TaskPriority` | ✘ | varchar(20), default `Medium`, stored **as string** |
| `DueDate` | `DateOnly?` | ✔ | `date`; app-level rule — within the parent project's window when both set |
| `AssigneeId` | `Guid?` | ✔ | FK → `users`, **RESTRICT**. Nullable — an unassigned task is legal |
| `ClosedAt` | `DateTimeOffset?` | ✔ | **derived, never bindable** — set on → `Done`, cleared on re-open (R-3) |
| `CreatedAt` / `UpdatedAt` | `DateTimeOffset` | ✘ | UTC |
| `Version` | `uint` | ✘ | `xmin`, `IsRowVersion()` — stale write → **409** |
| `Project` | `Project` | — | navigation — **required by the PM scope predicate** |
| `Assignee` | `ApplicationUser?` | — | navigation |

**`Project` navigation is load-bearing.** The ProjectManager scope predicate is
`t => t.Project.OwnerId == caller.UserId` — it reaches ownership *through* the navigation, so EF can
translate it to a join rather than a second query. Without it the predicate cannot fold into the
`IQueryable`.

### Enums (spec B.2)
- **`TaskStatus`** — `ToDo` (default) · `InProgress` · `InReview` · `Done` · `Blocked`.
  `Done` is the **only** completion state: it drives `closed_at`, the overdue rule, and every 005/006 metric.
- **`TaskPriority`** — `Low` · `Medium` (default) · `High` · `Critical`.
- **`TaskMutation`** — `Create` · `FullEdit` · `StatusChange` · `Reassign` · `Delete`.
  **Authorization input, never persisted.** Lives in `.Application/Common/Models/` beside `AccessDecision`,
  and **must be created in 001** because 001 authors the interface that references it (research R-1).

**No status workflow in v1.** Any status may move to any other, including *out of* `Done`, uniformly for
every role (OQ-003-03, Clarifications 2026-07-22), behind `Tasks:EnforceStatusWorkflow` (default `false`).

---

## 3. The graduated authorization matrix (the heart of this feature)

Resolved in one place — `TaskAccessPolicy.CanMutateAsync` — and nowhere else.

### Scope predicates (`ApplyScope`)

| Role | Predicate |
|---|---|
| `Admin` | *(none)* — all rows |
| `ProjectManager` | `t => t.Project.OwnerId == caller.UserId` |
| `TeamMember` | `t => t.AssigneeId == caller.UserId` |

Note the TeamMember predicate is **by assignment, not membership** — a TeamMember on a project's team sees
only the tasks assigned *to them*, not every task on the project.

### Mutation matrix (`CanMutateAsync`)

| `TaskMutation` | Admin | ProjectManager (owns parent) | TeamMember (is assignee) |
|---|---|---|---|
| `Create` | allow | allow | **deny** |
| `FullEdit` | allow | allow | **deny** |
| `StatusChange` | allow | allow | **allow** ← the graduated cell |
| `Reassign` | allow | allow | **deny** |
| `Delete` | allow | allow | **deny** |

**The defining test**: the *same* TeamMember on the *same* row gets **403** on `PUT /api/tasks/{id}` and
**200** on `PUT /api/tasks/{id}/status`. All 15 cells are covered by one table-driven xUnit test (DoD #3).

### Two enforcement mechanisms, both required (R-2)
1. **Structural** — `PUT /api/tasks/{id}/status` binds a **status-only** command; a widened payload has no
   property to bind `title` or `assigneeId` to. Escalation by extra field is impossible, not merely rejected.
2. **Behavioural** — `CanMutateAsync` is consulted regardless, so the rule survives any future slice that
   reuses the policy.

---

## 4. Migration `AddTaskIndexes`

```
CREATE INDEX ix_tasks_project_id          ON tasks (project_id);
CREATE INDEX ix_tasks_assignee_id         ON tasks (assignee_id);
CREATE INDEX ix_tasks_status              ON tasks (status);
CREATE INDEX ix_tasks_project_id_status   ON tasks (project_id, status);
CREATE INDEX ix_tasks_assignee_id_status  ON tasks (assignee_id, status);
CREATE INDEX ix_tasks_title_trgm          ON tasks USING gin (title gin_trgm_ops);
```

| Index | Serves |
|---|---|
| `(project_id)` | nested list route; cascade cleanup |
| `(assignee_id)` | **TeamMember scope predicate** — the hottest filter for the most numerous role |
| `(status)` | `?status=` filter; 005's tasks-by-status aggregate |
| `(project_id, status)` | a manager filtering one project's board |
| `(assignee_id, status)` | **005's personal slice** and 006's throughput — "my tasks by status" |
| GIN trigram on `title` | `?search=` substring matching (002 R-3's mechanism) |

`pg_trgm` is already enabled by 002's migration; migrations apply in order, so 003 does not re-issue it.

**Tasks is the highest-volume table in the product** — this index set is where NFR-005 is genuinely
exercised, not the earlier features.

---

## 5. Query composition order (fixed — deviation is a bug)

Identical discipline to 002 §5, because the same leak is possible:

1. `db.Tasks` → base `IQueryable`
2. **`ApplyScope(query, caller)`** ← scope first, always
3. Route/query filters: `projectId`, `status`, `assigneeId`, `search` (`ILIKE` on title)
4. **`CountAsync()`** → `totalCount`, on the scoped **and** filtered query
5. `?sort=` → whitelist-mapped `OrderBy` (default: due date ascending)
6. `Skip/Take` → `pageSize` **clamped** to max 100, never rejected
7. Project to `TaskSummaryDto` and materialize — **one** round trip

**A filter narrows; it never widens.** A TeamMember passing `?assigneeId=<colleague>` gets an **empty
page** — the predicates are `AND`-ed. Returning 403 there would confirm the colleague's task exists.

---

## 6. State transition: `closed_at`

The only state machine in the feature, and it is a **side effect**, not a user-settable field:

| Transition | `closed_at` |
|---|---|
| any status → `Done` | set to `now (UTC)` |
| `Done` → any other status (re-open) | cleared to `null` |
| `Done` → `Done` (no-op) | **unchanged** |
| any non-`Done` → any non-`Done` | untouched |

Computed inside `UpdateTaskStatusCommandHandler`; present in no request command (R-3). This is what lets
006 Reports treat `closed_at` as a trustworthy completion time — unlike `updated_at`, a later edit to a
finished task will not move it.

**No new audit action**: the `TaskStatusChanged` entry's `change_summary` records the transition, including
the `closed_at` effect (spec B.7 explicitly forbids a separate event).

---

## 7. Transactional invariants

1. **Write + audit commit together** in one `SaveChangesAsync` (IV.4) — create, full edit, status change,
   reassignment, delete.
2. **Delete audits before removal**; the `TaskDeleted` row survives, and survives the parent project's
   cascade too.
3. **Status change and `closed_at` are one atomic write** — a task can never be `Done` with a stale
   `closed_at`, or re-opened while retaining one.
4. **Reassignment audits from → to**, so transfers of access are reconstructable (spec B.7).

---

## 8. Deliberate non-goals

- **No `progress_percent`** — `status` is the progress signal (OQ-003-02).
- **No task movement between projects** — `project_id` is immutable after create (OQ-003-05).
- **No status-transition enforcement** in v1 (OQ-003-03).
- **No sub-tasks, dependencies, comments, attachments, time tracking** — bonus scope (I.2).
- **No new shared-kernel abstraction.** 003 *implements* `ITaskAccessPolicy` (shared-contracts §3) and
  *consumes* `IApplicationDbContext` (§7) and `IActivityLogService` (§6). It declares nothing new —
  **except** `TaskMutation`, which belongs to the shared kernel and must be created in 001 (research R-1).
