# Quickstart & Validation: 006 Reports

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Contract**:
[docs/contracts/reports.v1.yaml](../../docs/contracts/reports.v1.yaml)

Each scenario maps to a Definition-of-Done item in spec 006 B.8. Setup inherited from
[001](../001-auth-rbac/quickstart.md) — only deltas appear here.

---

## Prerequisites

Everything from 001. **006 adds no backend tooling and no migration.** Frontend adds **jsPDF** and
**papaparse** (Constitution III).

**001–005 must be implemented.** 006 depends specifically on:
- **003's `closed_at`** — set on entry to `Done`, cleared on re-open (all completion metrics rest on it)
- **`IActivityLogService.QueryScopedAsync`** — added to 001 during 005's planning pass
- **005's fixed-UTC overdue rule** — without it V6's parity assertion is unsatisfiable

```bash
dotnet run --project src/ProjectManagementApp.Api      # no `ef database update` — 006 adds no migration
```

**Fixtures**: `$ADMIN`, `$PM` (owns A), `$PM2` (owns B), `$TM`, `$TM2` (both members of A). Seed A with
tasks across statuses, several **closed at known dates spread over the window**, some overdue, and some
assigned to each member.

---

## Validation scenarios

### V1 — The catalog describes exactly four reports · *(DoD 1)*

```bash
curl -sS $API/reports/catalog -H "Authorization: Bearer $PM" | jq '.[].type'
```

**Expect** `ProjectProgress`, `TaskCompletion`, `TeamPerformance`, `Activity` — **four, no more**
(OQ-006-06). Each descriptor lists its ordered parameters with `required` flags, and `formats`
`["json","pdf","csv"]`.

Then as `$TM`: **expect Team Performance annotated "self only"**. Confirm the catalog returns **no**
project or task data, and (V12) that it writes **no** audit row.

### V2 — Three-role scope across every report · *(DoD 2)*

For each of the four reports:

| Caller | Rows/buckets cover |
|---|---|
| `$ADMIN` | all projects |
| `$PM` | **only A** |
| `$PM2` | **only B** |
| `$TM` | only A (member) — and Team Performance is **one row** |

**`$PM`'s output must contain nothing from B**, and vice versa. Empty scope → `200` with empty
`rows`/`buckets`, never 403.

### V3 — Named out-of-scope → 403; `all` → silently narrowed · *(DoD 4, FR-004)*

```bash
curl -sS "$API/reports/project-progress?from=…&to=…&projectScope=$B_ID" -H "Authorization: Bearer $PM" -w '%{http_code}\n'   # → 403
curl -sS "$API/reports/project-progress?from=…&to=…&projectScope=all"   -H "Authorization: Bearer $PM" -w '%{http_code}\n'   # → 200, A only
```

**Expect 403 then 200.** The distinction is the product-wide rule: **naming a resource invites a 403; not
naming one cannot.** (005 never 403s because it names nothing.)

### V4 — 🎯 TeamMember self-only returns their **own row**, not 403 · *(DoD 3, FR-005)*

```bash
curl -sS "$API/reports/team-performance?from=…&to=…&userId=$TM2_ID" -H "Authorization: Bearer $TM" | jq '.rows | length, .rows[0].userId'
```

**Expect `1` and `$TM`'s own id** — **not 403**, and not `$TM2`'s data.

This is the defining least-privilege test. A 403 here would *confirm that `$TM2` exists and is outside the
caller's scope* — exactly the inference a peer-comparison report must not permit. Then confirm
**Admin/PM naming an out-of-scope `userId` does get 403** — they are entitled to know their scope boundary.

### V5 — Completion metrics and the re-open rule · *(DoD 6, FR-017)*

With 3 of 12 tasks in A closed:

**Expect** `completionPercent: 25`, `closedTasks: 3`, `openTasks: 9`. On a project with **zero** tasks →
**`completionPercent: 0`**, never a divide-by-zero or `NaN`.

Now **re-open** one closed task (003's status endpoint clears `closed_at`) and re-run **both** reports:
- Project Progress `closedTasks` → **2**
- Task Completion: that task **drops out of its bucket**

**The same rule must apply in both** — divergent counting between two reports in one feature would be worse
than any cross-feature drift.

### V6 — 🎯 Dashboard/Reports value parity · *(DoD 5, NFR-002)*

```bash
D=$(curl -sS $API/dashboard/summary -H "Authorization: Bearer $PM" | jq '.overdueTaskCount')
R=$(curl -sS "$API/reports/project-progress?from=…&to=…&projectScope=all" -H "Authorization: Bearer $PM" | jq '[.rows[].overdueTasks] | add')
[ "$D" = "$R" ] && echo "PARITY OK" || echo "MISMATCH: dashboard=$D report=$R"
```

**Expect identical values.** Then repeat with the **server clock in a non-UTC zone** — both must be
*unchanged and still equal*. This is what 005's fixed-UTC correction bought; with a configurable timezone
this assertion would pass on one machine and fail on another.

### V7 — Task Completion bucketing is zero-filled and UTC · *(DoD 6)*

`groupBy=week` over a 4-week window with completions in only 2 weeks.

**Expect four buckets**, two with counts and **two with `completedCount: 0`** — a continuous series, so a
chart shows a zero rather than a gap. Invalid `groupBy` → **400**.

Then close a task at `23:30 UTC` on a bucket boundary and confirm it lands in the bucket determined **by
UTC**, not by local time.

### V8 — Projected completion, including its null cases · *(DoD 6, FR-017)*

| Situation | Expect |
|---|---|
| Steady throughput, open tasks remain | a plausible future date |
| **No** closures in the window (`avgClosedPerDay = 0`) | **`null`** |
| **No** open tasks (already complete) | **`null`** |

**Never a divide-by-zero, never a past date presented as a projection.**

### V9 — Activity Report reads through the service, scoped · *(DoD 7, FR-007)*

Assert in test that the handler calls **`IActivityLogService.QueryScopedAsync`** and that **no LINQ query
against `db.ActivityLogs`** exists under `Features/Reports/`. Then per role: `$PM` sees only A's activity,
`$PM2` only B's, `$ADMIN` both — with `totalCount` scoped.

Filters (`entityType`, `actorId`, `projectId`) **narrow within scope**; a named out-of-scope `projectId` →
**403**. Paging: `pageSize=500` → clamped to 100; `page=-1` → **400**.

### V10 — 🎯 The 422 guard fires **before** anything is materialized · *(DoD 7, FR-009)*

Seed activity beyond `Reports:LargeReportRowThreshold`, then request a window covering it:

**Expect `422`** with a narrow-your-range message — **not 400**, and not a slow 200. Verify via query
logging that **no large result set was materialized**: the guard is an estimate that runs first.

Then narrow the window → **200**. Confirm the frontend surfaces the prompt rather than attempting a render.

### V11 — 🎯 Export is client-side, from the same JSON · *(DoD 8, FR-010)*

In the browser, on any report:
1. **Export PDF** → downloads via **jsPDF**; **Network tab shows no additional request**
2. **Export CSV** → downloads via **papaparse**; again **no server round-trip**
3. Compare both to the on-screen preview — **same rows, same values**: format is a representation, not a
   different query
4. Confirm **no `?format` parameter** exists on the API and **no export endpoint** is defined
5. Both run through the single `ReportExportService` — **not duplicated per report component** (VII.8)
6. An empty report exports a valid, empty PDF/CSV (headers only)

### V12 — 🎯 Exactly one audit row per generation — and none for the catalog · *(DoD 9, FR-011)*

```sql
SELECT count(*) FROM activity_logs WHERE action='ReportGenerated';
```

| Action | Expected delta |
|---|---|
| One report data request | **+1** — with `entity_type='Report'`, actor, and serialized parameters in `change_summary` |
| `GET /reports/catalog` | **0** |
| Export the *same* previewed data to **both** PDF and CSV | **0** — format is a client action; the server audits the **generation** |
| A request that 400s or 403s | **0** — nothing was generated |

**And confirm no domain entity was written**: `projects`, `tasks`, `team_members`, `users` all unchanged,
no `xmin` bump anywhere. This is the one deliberate write in a read-only feature — it must be exactly one,
and exactly here.

### V13 — No migration, no new table · *(DoD 9, FR-001)*

```bash
ls src/ProjectManagementApp.Infrastructure/Persistence/Migrations/
```

**Expect no 006-authored migration**, and **no `ReportArtifact` or `ReportSchedule` table** — reports are
transient and re-generated on demand (OQ-006-03), scheduling is out of scope (OQ-006-01).

### V14 — The contract gate catches drift · *(DoD 11, X.2)*

```bash
dotnet build -p:CheckApiContract=true      # → passes
```

Then **temporarily add a `format` query parameter** to a report endpoint and rebuild. **Expect the build to
FAIL.** That is the highest-value drift check here: a server-side `?format` would silently move export
back onto the server, contradicting Constitution III's jsPDF lock and VII.8. Revert.

### V15 — Frontend · *(DoD 10, FR-014)*

Parameter forms are built **from the catalog**, not hard-coded — verify by adding a parameter to a
descriptor and seeing the form change with no component edit. Charts use **Chart.js**. As `$TM`, Team
Performance renders a **single-member card**, not a comparison chart. Route group is lazy; a functional
role guard is the only navigation block; the 422 surfaces a narrow-your-range prompt.

---

## Test suite

```bash
dotnet test tests/ProjectManagementApp.Application.Tests      # report handlers, metric definitions, self-only clamp
dotnet test tests/ProjectManagementApp.Infrastructure.Tests   # scoped audit read, threshold estimate
dotnet test tests/ProjectManagementApp.Api.Tests              # scope matrix, 403 vs self-row, 422, exactly-one-audit
cd src/ProjectManagementApp.Web && npm test                   # ReportsService, ReportExportService, catalog-driven forms
```

**Never skip V4, V6, or V12** — least-privilege, cross-feature parity, and the single-audit guarantee are
this feature's three real promises.

---

## Definition-of-Done cross-reference

| DoD (spec B.8) | Proven by |
|---|---|
| 1 — catalog + four report GETs; no write endpoint | V1, V11.4, V13 |
| 2 — three-role scope matrix, filter-at-source | **V2** |
| 3 — TeamMember Team Performance = one own row | **V4** |
| 4 — named out-of-scope 403; `all` auto-narrows | V3 |
| 5 — Dashboard value parity | **V6** |
| 6 — bucketing, zero-filled series, completion source | V5, V7, V8 |
| 7 — Activity via service, paginated, scoped, 422 guard | V9, **V10** |
| 8 — PDF via jsPDF, CSV via papaparse, one export service | **V11** |
| 9 — exactly one audit row; no domain write; no migration | **V12**, V13 |
| 10 — lazy standalone `reports` group, catalog-driven forms, Chart.js | V15 |
| 11 — RFC 7807; contract authored before handlers | V14 |
| 12 — all six OQ-006-01..06 resolved and covered | V1 (four reports), V5 (counting rule), V7 (UTC), V10 (threshold), V13 (transient, no scheduling) |
| 13 — unit + integration tests pass | Test suite |
