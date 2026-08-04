# Phase 1 Read Model: 005 Dashboard

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Research**: [research.md](research.md)

> **This feature adds no entity, no table, and no migration.** It implements the brief's Dashboard module
> by **aggregation**, not by new persistence (Constitution I.1). There is no `xmin` (nothing is updated)
> and no audit write (nothing is written). What follows is the **read model**: what is read, how it is
> scoped, and what shape comes back.

---

## 1. Sources read (all owned elsewhere)

| Source | Owner | Read for | Access path |
|---|---|---|---|
| `projects` | 002 | Project counts by status; **the visible-project scope** | `IApplicationDbContext` + `IProjectAccessPolicy.ApplyScope` |
| `tasks` | 003 | Task counts by status, overdue, blocked, completion rate, personal slice | `IApplicationDbContext` (+ `ITaskAccessPolicy.ApplyScope` for the personal slice) |
| `team_members` | 004 | TeamMember visible-project scope; distinct headcount | `IApplicationDbContext` |
| `users` | 001 | Actor/member display names | `IApplicationDbContext` |
| `activity_logs` | 001 | The recent-activity feed | **`IActivityLogService.QueryScopedAsync` only — never a direct table query** (FR-006) |

**No migration, no new index.** The aggregates rely on indexes 002/003/004 already declare —
`projects(owner_id, status)`, `tasks(project_id, status)`, `tasks(assignee_id, status)`, a `due_date`
filter, and `team_members(project_id, user_id)`. If profiling later shows a hot aggregate, the index is
added by the **owning** feature's migration, not here.

> ⚠️ `QueryScopedAsync` **does not exist yet** — 001 defines `IActivityLogService` with `LogAsync` only.
> See [research R-1](research.md); it must be added to 001 before this feature can be built.

---

## 2. The scope chain (the security-critical part)

Every number is computed **within the caller's visible-project set**, and that set is produced by reusing
existing predicates — 005 defines **no new scope logic**.

```
visibleProjectIds : IQueryable<Guid>          ← NOT materialized (research R-4)
    = IProjectAccessPolicy.ApplyScope(db.Projects, caller).Select(p => p.Id)
```

| Role | Visible projects | Task tiles counted over |
|---|---|---|
| `Admin` | all | all tasks in scope |
| `ProjectManager` | `owner_id == caller` | all tasks in those projects |
| `TeamMember` | projects with a `team_members` row for the caller | **only tasks assigned to them** (personal-view) |

**The TeamMember row is the subtle one.** Their task and overdue tiles use the **personal** predicate
(`assignee_id == caller`), identical to the US-005-03 slice — one number to compute and test, matching the
"my work" mental model, and never surfacing colleagues' work they cannot act on (Clarifications
2026-07-22).

**Filter at the source, never in memory.** Each aggregate is expressed so EF Core translates the scope
predicate **into the SQL** (`WHERE project_id IN (<scoped subquery>) GROUP BY status`). Materializing the
project ids first would round-trip the entire projects table for an Admin. This is the single most
important implementation rule of the feature (NFR-002, DoD 3).

---

## 3. Metric definitions (must match 006 exactly — 006 NFR-002)

| Metric | Definition |
|---|---|
| `visibleProjectCount` | count of the scoped project set |
| `projectsByStatus` | `GROUP BY status` over scoped projects — **every `ProjectStatus` key present, zeros included** |
| `tasksByStatus` | `GROUP BY status` over scoped tasks — **every `TaskStatus` key present, zeros included** |
| `overdueTaskCount` | `due_date < today (UTC)` **AND** `status != Done`, within scope |
| `completionRate` | `Done ÷ total` tasks in scope; **`0` when total = 0** — never a divide-by-zero |
| `blockedTaskCount` | derived from `tasksByStatus[Blocked]` — **no extra query** |
| `visibleTeamMemberCount` | `COUNT(DISTINCT user_id)` over `team_members` in scope — a user on several visible projects counts **once** |
| `personalTasks` | the same task aggregates re-run with `assignee_id == caller` |

**`Done` is the only completion state** and **today is evaluated in UTC, fixed** ([research R-2](research.md)).
Both are load-bearing: 006 reuses these definitions verbatim and a cross-feature test asserts the numbers
are identical for the same caller.

**Overdue boundary**: a task due *today* is **not** overdue; one due *before* today is. A task with **no**
due date is never overdue.

---

## 4. Response shapes (transient — never persisted)

### `DashboardSummaryDto`
```
generatedAt · scope · visibleProjectCount
projectsByStatus { Planning, Active, OnHold, Completed, Cancelled }   ← all keys, zeros included
tasksByStatus    { ToDo, InProgress, InReview, Done, Blocked }        ← all keys, zeros included
overdueTaskCount · completionRate · blockedTaskCount · visibleTeamMemberCount
personalTasks    { assignedTotal, byStatus, overdueCount }
```

### `ActivityEntryDto`
```
id · actorName · action · entityType · entityId · timestamp · changeSummary
```

**A stable typed contract, not a stat dictionary.** Enum-keyed maps are seeded with every enum value at
zero *before* query results merge in, so a status with no rows appears as `0` rather than vanishing. A
caller with an empty scope receives **the same shape**, all zeros — which is what lets the frontend render
an empty state instead of null-guarding every key.

---

## 5. Query composition order (fixed)

Per metric:
1. Base `IQueryable` (`db.Projects` / `db.Tasks` / `db.TeamMembers`)
2. **Scope predicate composed in** — via `ApplyScope` or `project_id IN (visibleProjectIds)`
3. Metric-specific filter (`due_date < today (UTC)`, `assignee_id == caller`, …)
4. `GroupBy` + `Count` — **executed in SQL**
5. Merge into the zero-seeded enum map

For the feed: `IActivityLogService.QueryScopedAsync(scope, page, pageSize)` → `PagedResult<T>`, `pageSize`
**clamped** to 100, newest first, stable ordering by `(timestamp, id)`.

**Nothing is fetched then filtered, at any step.**

---

## 6. What is deliberately absent

| Absent | Why |
|---|---|
| **Any write path** | Read-only feature. No create/update/delete of anything |
| **`CanMutateAsync`** | Nothing to authorize a mutation for (research R-6) |
| **`xmin` / `ETag` / `If-Match`** | Nothing is updated |
| **Audit rows** | IV.4 audits *writes*; 005 has none. **Intentionally empty catalog** — contrast 006, which writes exactly one `ReportGenerated` row |
| **403 / 404** | Scope shapes *content*, not access — empty scope → 200 with zeros (research R-3) |
| **Paging on the summary** | Fixed-N scalar/enum metrics; only the feed pages (research R-5) |
| **New tables, columns, indexes, migrations** | Aggregation over existing entities only |
| **Caching** | Live per request for v1; the seam sits **inside the query handlers**, so adding a cache later changes no contract |
