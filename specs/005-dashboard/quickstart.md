# Quickstart & Validation: 005 Dashboard

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Contract**:
[docs/contracts/dashboard.v1.yaml](../../docs/contracts/dashboard.v1.yaml)

Each scenario maps to a Definition-of-Done item in spec 005 B.8. Setup inherited from
[001](../001-auth-rbac/quickstart.md) — only deltas appear here.

---

## Prerequisites

Everything from 001. **005 adds no new tooling and no migration.**

**001–004 must be implemented** — 005 aggregates over all of them and computes nothing of its own.
It also requires **`IActivityLogService.QueryScopedAsync`**, which does not exist in 001 as currently
planned ([research R-1](research.md)); the feed cannot be built until it is added.

```bash
dotnet run --project src/ProjectManagementApp.Api      # no `ef database update` — 005 adds no migration
```

**Fixtures** (builders, not the production seeder — ADR-0007 §4): `$ADMIN`, `$PM`, `$PM2`, `$TM`, `$TM2`,
plus `$LONELY` — a TeamMember on **no** teams, which V4 needs. Project **A** (owner `PM`, members `TM`,
`TM2`), project **B** (owner `PM2`). Seed A with tasks across several statuses, some overdue, some
assigned to `TM`.

---

## Validation scenarios

### V1 — The three-role scope matrix · *(DoD 2)*

`GET /api/dashboard/summary` as each role:

| Caller | `visibleProjectCount` | `tasksByStatus` counts |
|---|---|---|
| `$ADMIN` | all projects | all tasks system-wide |
| `$PM` | only A | all tasks in A |
| `$PM2` | only B | all tasks in B |
| `$TM` | only A (member) | **only tasks assigned to `$TM`** |

**`$PM2`'s numbers must contain nothing from A**, and vice versa. The TeamMember row is the subtle one —
their tiles are **personal-view**, not project-wide (Clarifications 2026-07-22).

### V2 — 🎯 Filter-at-source: out-of-scope data contributes to **no** aggregate · *(DoD 3, NFR-002)*

Capture `$PM`'s summary. Now add ten tasks to project **B** (which `$PM` cannot see) and re-fetch.

**Expect every number unchanged.** Then inspect the generated SQL (EF logging) and confirm the scope
appears as a **`WHERE project_id IN (<subquery>)`**, not as a post-query filter.

> This is the assertion InMemory cannot make. Under InMemory a fetch-then-filter implementation returns
> identical numbers while loading every row — the test passes and the leak ships (ADR-0007 §2).

### V3 — 🎯 The stable typed contract · *(DoD 4)*

On a scope where some statuses have no rows:

```bash
curl -sS $API/dashboard/summary -H "Authorization: Bearer $PM" | jq '.projectsByStatus, .tasksByStatus'
```

**Expect all five `ProjectStatus` keys and all five `TaskStatus` keys present, with `0` for the empty
ones** — never omitted. A vanished key means the payload has degraded into a free-form dictionary, and
every frontend consumer now needs a null-guard.

### V4 — 🎯 Empty scope returns 200 with zeros — **never 403** · *(DoD 5, FR-007)*

```bash
curl -sS $API/dashboard/summary -H "Authorization: Bearer $LONELY" -w '\n%{http_code}\n'
```

**Expect `200`**, `visibleProjectCount: 0`, every breakdown present with zeros, `completionRate: 0`,
`personalTasks.assignedTotal: 0`. **Not 403. Not 404. Not an empty body.**

This is the deliberate exception to 002's out-of-scope-403 convention: the dashboard names no resource, so
"nothing to show" is a valid answer, not a denial. A new user's landing page must not be an error screen.

### V5 — `completionRate` does not divide by zero · *(DoD 11)*

As `$LONELY` (no tasks) and on a project with tasks but none `Done`:
**Expect `completionRate: 0`** in both cases — never `NaN`, never a 500.

Then with 3 of 12 tasks `Done` → **`0.25`**.

### V6 — Overdue boundary and the UTC rule · *(DoD 7, FR-005)*

Seed three tasks: due **yesterday**, due **today**, due **tomorrow** — all `InProgress` — plus one due
yesterday but `Done`, and one with **no** due date.

**Expect `overdueTaskCount` = 1** — only the yesterday/`InProgress` one:
- due **today** → not yet overdue (strictly *before* today)
- `Done` → excluded regardless of date
- no due date → never overdue

Then confirm the boundary is computed in **UTC**. Run the same assertion with the server clock in a
non-UTC zone — **the count must not change**. ([research R-2](research.md) — this is what makes 006's
parity requirement satisfiable.)

### V7 — 🎯 Dashboard/Reports value parity · *(006 NFR-002)*

Once 006 exists, for the same caller and window:

```bash
curl -sS $API/dashboard/summary            -H "Authorization: Bearer $PM" | jq '.overdueTaskCount'
curl -sS "$API/reports/project-progress?from=…&to=…" -H "Authorization: Bearer $PM" | jq '[.rows[].overdueTasks] | add'
```

**Expect identical numbers.** A manager seeing one overdue count on the dashboard and a different one in
the report destroys trust in both surfaces. This is 006's hard requirement, and 005 is half of it.

### V8 — Activity feed is scoped · *(DoD 6, FR-006)*

Generate writes across A and B, then fetch `GET /api/dashboard/activity` per role.

**Expect** `$PM` to see **only A's** activity, `$PM2` **only B's**, `$ADMIN` **both**. `totalCount` must be
scoped too — a TeamMember must never learn the system-wide activity volume.

### V9 — The feed is read through the service, not the table · *(DoD 6)*

Assert in test that the handler calls **`IActivityLogService.QueryScopedAsync`** and that **no LINQ query
against `db.ActivityLogs`** exists in `Features/Dashboard/`.

> ⚠️ This method **does not exist in 001 as currently planned** — see [research R-1](research.md). Until it
> is added, the only way to build the feed is the forbidden one.

### V10 — Feed paging · *(DoD 6, FR-006)*

`pageSize=500` → **clamped to 100**; `page=999` → empty items with valid metadata; `page=-1` → **400**;
default `pageSize` → **20**. Ordering newest-first and stable across requests.

### V11 — Activity for a now-invisible project · *(spec edge case)*

Have `$PM` lose access to a project (transfer ownership, or delete it — audit rows survive).

**Expect** those entries **scoped out for `$PM`** but still visible to **`$ADMIN`**.

### V12 — 🎯 The feature writes **nothing** · *(DoD 8, FR-010)*

```sql
SELECT count(*) FROM activity_logs;    -- before
```

Exercise **both** endpoints repeatedly as every role, then re-count.

**Expect the count unchanged.** No `activity_logs` row, no domain write, no `xmin` bump anywhere. Confirm
there is no POST/PUT/DELETE under `/api/dashboard` at all, and that `CanMutateAsync` is not referenced
anywhere in `Features/Dashboard/`.

The empty audit catalog is **correct, not an omission** — IV.4 audits writes, and there are none.

### V13 — No migration was added · *(DoD 8)*

```bash
ls src/ProjectManagementApp.Infrastructure/Persistence/Migrations/
```

**Expect no 005-authored migration**, and no new index. 005 relies entirely on indexes 002/003/004 declare.

### V14 — Live per request · *(Clarifications 2026-07-22)*

Fetch the summary, mutate a task's status via 003, fetch again.

**Expect the new value immediately** — no staleness window, no cache invalidation to wait for. Confirm
`generatedAt` advances between calls.

### V15 — The contract gate catches drift · *(DoD 10, X.2)*

```bash
dotnet build -p:CheckApiContract=true      # → passes
```

Then **temporarily remove one enum key from `TaskStatusCounts`** (e.g. drop `Blocked` from `required`) and
rebuild. **Expect the build to FAIL.** That is the drift most worth catching here — it is exactly how the
stable typed contract silently becomes a variable dictionary. Revert.

### V16 — Frontend · *(DoD 9, FR-012)*

Tiles render read-only with Chart.js; **no "mark read", no action controls** on the feed. Empty state for
`$LONELY` reads "nothing assigned to you yet" rather than an error. The route group is lazy (Network tab),
behind a functional auth guard, and refetches on navigation.

---

## Test suite

```bash
dotnet test tests/ProjectManagementApp.Application.Tests      # query handlers, metric definitions
dotnet test tests/ProjectManagementApp.Infrastructure.Tests   # (little here — 005 owns no persistence)
dotnet test tests/ProjectManagementApp.Api.Tests              # scope matrix, zero-scope 200, no-write assertion
cd src/ProjectManagementApp.Web && npm test
```

**Never skip V2 or V12** — filter-at-source and the no-write guarantee are this feature's two real
promises, and V2 is meaningless on anything but real PostgreSQL.

---

## Definition-of-Done cross-reference

| DoD (spec B.8) | Proven by |
|---|---|
| 1 — two GETs, no write endpoints | V12 |
| 2 — three-role scope matrix | **V1** |
| 3 — filter-at-source proven | **V2** |
| 4 — stable typed contract, zeros included | **V3** |
| 5 — empty scope → 200, never 403/404 | **V4** |
| 6 — feed via `IActivityLogService`, paged, scoped | V8, V9, V10, V11 |
| 7 — overdue rule + documented boundary/timezone | V6 |
| 8 — no migration, no `xmin`, **no audit row** | **V12**, V13 |
| 9 — lazy standalone `dashboard` group, Chart.js, guard-only | V16 |
| 10 — RFC 7807; contract authored before handlers | V15 |
| 11 — the four clarified decisions (personal-view, live, completion rate + blocked, feed 20/100 all-visible) | V1, V5, V10, V14 |
| 12 — unit + integration tests pass | Test suite |
