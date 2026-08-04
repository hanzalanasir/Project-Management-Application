# Phase 1 Read Model: 006 Reports

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Research**: [research.md](research.md)

> **No new domain entity, domain table, or migration against domain tables.** 006 implements the brief's
> Reports module by **aggregation + client-side export**. The **only** persisted output is one
> **audit-only** `activity_logs` row per generation ([research R-1](research.md)) — written through 001's
> service, targeting a logical `Report`, touching no domain entity.

---

## 1. Sources read

| Source | Owner | Read for | Access path |
|---|---|---|---|
| `projects` | 002 | Project Progress rows; the visible-project scope | `IApplicationDbContext` + `IProjectAccessPolicy.ApplyScope` |
| `tasks` | 003 | Completion %, open/closed/overdue, throughput, workload; **`closed_at`** | `IApplicationDbContext` (+ `ITaskAccessPolicy.ApplyScope`) |
| `team_members` | 004 | The member pool for Team Performance; TeamMember scope | `IApplicationDbContext` |
| `users` | 001 | Member/assignee/actor display names | `IApplicationDbContext` |
| `activity_logs` | 001 | Activity Report — **`QueryScopedAsync` only** — **and** the one `ReportGenerated` write | `IActivityLogService` |

**No migration, no new index.** Aggregates reuse indexes 002/003/004 declare — notably
`tasks(assignee_id, status)` for throughput and `tasks(project_id, status)` for progress.

**Dependency on 003's `closed_at`**: created with the `tasks` table in 001's `InitialCreate`, set on entry
to `Done` and cleared on re-open by 003's status handler. 006 **reads** it and never writes it. This is
precisely why `closed_at` exists rather than reusing `updated_at` — a later edit to a finished task must
not move its completion time.

---

## 2. The one write

| | |
|---|---|
| **When** | Each successful report **data** request (not the catalog) |
| **How** | `IActivityLogService.LogAsync` |
| **Row** | `actor_id` = caller · `action` = `ReportGenerated` · `entity_type` = `'Report'` · `entity_id` = generated run id · `timestamp` · `change_summary` = report type + serialized parameters |
| **Touches** | **No domain entity.** `activity_logs` only |

`entity_id` is `varchar(64)`, not `uuid` — 001's data-model chose that deliberately so *"a logical target
like `Report` (006) fits"*. `ReportGenerated` is one of the 18 values in the shared `AuditAction` enum
created by 001's T018.

---

## 3. Scope and parameters

```
visibleProjectIds : IQueryable<Guid>          ← never materialized
    = IProjectAccessPolicy.ApplyScope(db.Projects, caller).Select(p => p.Id)
```

**Parameters narrow within scope; they never widen it.**

| Caller | Scope | Named out-of-scope `projectId`/`userId` | `projectScope=all` |
|---|---|---|---|
| `Admin` | all | **403** | all |
| `ProjectManager` | owned projects | **403** | silently narrowed |
| `TeamMember` | member-of projects | **403** for `projectId`; for `userId` on Team Performance → **own row, no 403** (R-6) | silently narrowed |

**Why 006 has 403s where 005 has none:** a report can **name** a specific project or user. 005 names
nothing. Same principle, opposite outcome — *naming a resource invites a 403; not naming one cannot.*

---

## 4. Report shapes

All four share the envelope `{ reportType, generatedAt, scope, window{from,to}, timeZone:"UTC" }`.

### Uniform counting rules (shared with 003/005 — [research R-4](research.md))
- **closed** ⇔ `status = Done` (⇔ `closed_at` not null)
- **re-opened** (`closed_at` cleared) → **excluded** from `closedTasks` *and* Task Completion buckets
- **overdue** ⇔ `due_date < today (UTC)` **and** `status != Done`
- **throughput** ⇔ `closed_at` within the window
- **today** ⇔ **UTC, fixed**

### `ProjectProgressReport` — envelope + `rows[]` + `totals`
| Field | Notes |
|---|---|
| `projectId` · `projectName` · `status` | identity |
| `totalTasks` · `openTasks` · `closedTasks` | in-scope counts |
| `overdueTasks` | **must equal 005's overdue for this caller** |
| `completionPercent` | `closed ÷ total × 100`; **`0` when `total = 0`** |
| `projectedCompletion` | `today(UTC) + ceil(openTasks ÷ avgClosedPerDay)`; **`null`** when `avgClosedPerDay = 0` or `openTasks = 0` |

`totals`: `{ projects, avgCompletionPercent }`.

### `TaskCompletionReport` — envelope + `groupBy` + `buckets[]` + `totals`
`periodStart` (UTC) · `periodLabel` · `completedCount`. **Zero-filled continuous series** — every period in
the window is present, matching 005's zero-seeded enum maps: an absent bucket would force consumers to
guard, and would make a chart render a gap instead of a zero.

### `TeamPerformanceReport` — envelope + `rows[]`
`userId` · `fullName` · `isActive` · `throughput` · `workload` (assigned and not `Done`) · `overdueCount`.
**Exactly one row — the caller's own — for a TeamMember** (R-6). A member with no activity appears as a
**row of zeros** rather than being omitted, so absence is visible.

### `ActivityReport` — `PagedResult<ActivityReportRow>`
`id` · `timestamp` · `actorId`/`actorName` · `action` · `entityType`/`entityId` · `changeSummary`.
Sourced **only** through `IActivityLogService.QueryScopedAsync`.

---

## 5. Query composition order (fixed)

1. Base `IQueryable`
2. **Scope composed in** (`ApplyScope`) — always first
3. Window filter (`from`/`to`, UTC) and report-specific parameters
4. Named-resource check → **403** if outside scope (except the TeamMember `userId` clamp, R-6)
5. **Threshold estimate → 422** if the result set would exceed the limit — **before** materializing
6. `GroupBy` + aggregate, **executed in SQL**
7. Assemble the typed DTO; zero-fill buckets and seed enum keys
8. **`LogAsync` the `ReportGenerated` row**, then `SaveChangesAsync`

Step 5 before step 6 is the point: the guard must fire before a large set is materialized, not after.

---

## 6. Bounded vs paged

| Endpoint | Shape | Why |
|---|---|---|
| `/catalog` | plain array | four fixed descriptors |
| `/project-progress` | full `rows[]` | one row per visible project — bounded |
| `/task-completion` | full `buckets[]` | one per period in the window — bounded by `groupBy` |
| `/team-performance` | full `rows[]` | one per member — bounded |
| `/activity` | **`PagedResult<T>`** | the only genuinely unbounded set |

Same rule as 004 and 005: **paging follows the nature of the collection.** The three bounded reports
paginate **client-side** for readability, which costs nothing because the row set is already small — and is
what makes client-side export viable (R-3).

---

## 7. Deliberately absent

| Absent | Why |
|---|---|
| **New domain entity/table/migration** | Aggregation only (I.1) |
| **`ReportArtifact` / `ReportSchedule` tables** | Persisted artifacts (OQ-006-03) and scheduling (OQ-006-01) are out of scope for v1; the model does not preclude either |
| **`CanMutateAsync`, `xmin`, `ETag`** | No domain mutation |
| **Export endpoint / `?format`** | Export is client-side (R-3) |
| **Server-side CSV streaming** | Deferred behind the same threshold config (R-5) |
| **A shared metric-expression helper** | Deferred — the cross-feature parity test provides the guarantee without a new shared-kernel member (R-4) |
