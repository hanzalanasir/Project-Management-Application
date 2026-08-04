# Quickstart & Validation: 003 Task Management

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Contract**:
[docs/contracts/tasks.v1.yaml](../../docs/contracts/tasks.v1.yaml)

Each scenario maps to a Definition-of-Done item in spec 003 B.8. Setup is inherited from
[001](../001-auth-rbac/quickstart.md) and [002](../002-projects/quickstart.md) — only deltas appear here.

---

## Prerequisites

Everything from 001 (.NET 10, Node 24, Docker, `oasdiff`). **003 adds no new tooling.**

**001 and 002 must be implemented.** 003 needs authenticated callers with role claims (001) and real
projects with owners (002). It also needs `team_members` rows to validate assignees against — if 004 is not
yet built, **insert them directly**; 003 only ever *reads* that table ([research R-6](research.md)).

```bash
dotnet ef database update -p src/ProjectManagementApp.Infrastructure -s src/ProjectManagementApp.Api
dotnet run --project src/ProjectManagementApp.Api
```

**Fixture set** (extends 002's): tokens `$ADMIN`, `$PM`, `$PM2`, `$TM`, plus a second member `$TM2`.
Project **A** owned by `PM`; project **B** owned by `PM2`. `TM` and `TM2` are team members of **A**.
Task **T1** in A assigned to `TM`; task **T2** in A assigned to `TM2`; task **T3** in A unassigned.

> `TM2` is not optional. Without a second TeamMember you cannot prove that a member sees only *their own*
> assigned tasks rather than every task on a project they belong to — which is the exact distinction 003's
> scope predicate makes.

---

## Validation scenarios

### V1 — Create: `projectId` comes from the route · *(DoD 5, FR-003)*

```bash
curl -sS -X POST $API/projects/$A_ID/tasks -H "Authorization: Bearer $PM" -i \
  -H 'Content-Type: application/json' \
  -d '{"title":"Draft rollout checklist","priority":"High","dueDate":"2026-09-15","assigneeId":"'$TM_ID'"}'
```

**Expect** `201` · `Location: /api/tasks/{id}` · `ETag` · `status: "ToDo"` (default) · `projectId` = **A**.

Now include `"projectId":"<B's id>"` in the body. **Expect it to be ignored** — the task still lands in A.
Then `POST /api/projects/$B_ID/tasks` as `PM` (who does not own B) → **403**.

### V2 — 🎯 The graduated model: the *same* user, the *same* row, two outcomes · *(DoD 3)*

**This is the single most important test in the feature.** As `TM`, who **is** T1's assignee:

```bash
# Full edit → REFUSED
curl -sS -X PUT $API/tasks/$T1 -H "Authorization: Bearer $TM" -H "If-Match: $ETAG" \
     -H 'Content-Type: application/json' -d '{"title":"Renamed by assignee","priority":"Low"}' \
     -w '%{http_code}\n'

# Status change on the SAME row → PERMITTED
curl -sS -X PUT $API/tasks/$T1/status -H "Authorization: Bearer $TM" -H "If-Match: $ETAG" \
     -H 'Content-Type: application/json' -d '{"status":"InProgress"}' -w '%{http_code}\n'
```

**Expect `403` then `200`.** The 403 body must **name the narrower right**: *"You may update the status of
this task, but not its details."* Also confirm `TM` gets **403** on `PUT /tasks/$T1/assignee` and on
`DELETE /tasks/$T1` — the assignee can move status and nothing else.

### V3 — Privilege cannot be widened by payload · *(DoD 4, FR-008)*

As `TM`, send extra fields to the status endpoint:

```bash
curl -sS -X PUT $API/tasks/$T1/status -H "Authorization: Bearer $TM" -H "If-Match: $ETAG" \
  -H 'Content-Type: application/json' \
  -d '{"status":"InReview","title":"HACKED","assigneeId":"'$TM2_ID'","priority":"Critical"}'
```

**Expect `200`** with **only `status` changed** — title, assignee, and priority untouched. The extra fields
have no property to bind to; they are *structurally* inert, not merely rejected. Verify in the database.

### V4 — 🎯 The 15-cell matrix · *(DoD 3)*

Table-driven: 5 `TaskMutation` values × 3 roles.

| Mutation | Admin | ProjectManager (owns parent) | TeamMember (assignee) |
|---|---|---|---|
| Create · FullEdit · Reassign · Delete | 2xx | 2xx | **403** |
| StatusChange | 2xx | 2xx | **200** |

**Expect exactly this shape.** A TeamMember who is *not* the assignee must get **403** on status too — that
is the scope gate firing before the mutation gate.

### V5 — Scope: a member sees only *their own* tasks · *(DoD 2, FR-009)*

| Caller | `GET /api/tasks` returns | `totalCount` |
|---|---|---|
| `ADMIN` | T1, T2, T3 (+ B's tasks) | all |
| `PM` | T1, T2, T3 (all of A) | 3 |
| `TM` | **T1 only** | **1** |
| `TM2` | **T2 only** | **1** |

**Assert `totalCount`, not just items.** `TM` seeing `totalCount: 3` with one item would mean scope was
applied after counting — existence leaking through metadata. Note `TM` does **not** see T3 (unassigned) or
T2, despite being on project A's team: **scope is by assignment, not membership**.

### V6 — A filter narrows; it never widens · *(FR-009)*

```bash
curl -sS "$API/tasks?assigneeId=$TM2_ID" -H "Authorization: Bearer $TM" | jq '.items | length'
```

**Expect `0`** — an **empty page, not a 403**. A 403 would confirm that TM2's task exists. Then
`?projectId=$B_ID` as `PM` → also empty, not 403.

### V7 — Nested route 403/404 vs cross-project route silence · *(research R-4)*

```bash
curl -sS $API/projects/$B_ID/tasks       -H "Authorization: Bearer $PM"  -w '%{http_code}\n'  # → 403
curl -sS $API/projects/$UNKNOWN/tasks    -H "Authorization: Bearer $PM"  -w '%{http_code}\n'  # → 404
curl -sS $API/tasks                      -H "Authorization: Bearer $TM"  -w '%{http_code}\n'  # → 200
```

**Expect 403 / 404 / 200.** The asymmetry is intentional: **naming a resource invites a 403/404; not naming
one cannot**. Both routes must return identical *content* for the same caller and filter.

### V8 — Assignee must be an active team member · *(DoD 6, FR-004)*

| Attempt | Expect |
|---|---|
| Reassign T1 to a user **not** on project A's team | **400**, field error on `assigneeId` |
| Reassign T1 to a **deactivated** user | **400** |
| Reassign T1 to `null` | **200** — unassigned, now invisible to every TeamMember |
| Reassign T1 to `TM2` (a valid member) | **200**, audited **from → to** |

After the last one, `TM`'s next `GET /api/tasks/$T1` → **403**, immediately. No grace period, no
notification — scope is re-evaluated fresh on every read.

### V9 — Due date must fall inside the project window · *(DoD 7, FR-005)*

Create a task in A with `dueDate` outside A's `startDate`…`endDate`.
**Expect `400`** with a field error — a cross-field rule (ADR-0005) validated against the *parent project*,
which is why it needs the handler's database access, not just the validator.

### V10 — 🎯 `closed_at` is derived and untamperable · *(FR-002, research R-3)*

```bash
# → Done
curl -sS -X PUT $API/tasks/$T1/status -H "Authorization: Bearer $PM" -H "If-Match: $E1" -d '{"status":"Done"}'
psql -d pma -c "SELECT status, closed_at FROM tasks WHERE id='$T1';"     -- closed_at = now

# re-open
curl -sS -X PUT $API/tasks/$T1/status -H "Authorization: Bearer $PM" -H "If-Match: $E2" -d '{"status":"InProgress"}'
psql -d pma -c "SELECT status, closed_at FROM tasks WHERE id='$T1';"     -- closed_at = NULL
```

**Expect** `closed_at` set on entry to `Done` and **cleared** on re-open. Then attempt to set it directly:
`{"status":"Done","closedAt":"2020-01-01T00:00:00Z"}` → **the field is ignored**; `closed_at` is `now`, not
2020. Backdating would corrupt every 006 report.

Finally, `Done` → `Done` (no-op): **`closed_at` unchanged**, and still audited.

### V11 — Any status may move to any other, including out of `Done` · *(OQ-003-03)*

Move `Done` → `Blocked` → `ToDo` → `InReview` as the **assignee**.
**Expect all to succeed** — no workflow enforcement in v1, and `Done` is not terminal for anyone.

### V12 — Concurrency on all three PUTs; DELETE exempt · *(DoD 10, FR-016, research R-5)*

For each of `PUT /tasks/{id}`, `/status`, `/assignee`: fetch the `ETag`, apply one update (→ `200` + new
ETag), then replay the **stale** ETag → **`409`**. Omit `If-Match` → **`400`**.

Then confirm `DELETE /api/tasks/{id}` **succeeds without `If-Match`** → `204`, and a second delete → `404`.

Cross-endpoint check: change status, then attempt a full edit with the **pre-status** ETag → **`409`**. All
three PUTs contend on the same row version, which is correct — they are conflicting writes to one row.

### V13 — Cascade: deleting a project removes its tasks; audit survives · *(DoD 9)*

```bash
curl -sS -X DELETE $API/projects/$A_ID -H "Authorization: Bearer $PM" -w '%{http_code}\n'   # → 204
psql -d pma -c "SELECT count(*) FROM tasks WHERE project_id='$A_ID';"                       -- → 0
psql -d pma -c "SELECT action FROM activity_logs WHERE entity_type='Task' AND entity_id='$T1';"
```

**Expect** tasks gone and their `TaskDeleted`/`TaskCreated` audit rows **still present** — audit is never
cascaded away, which is what keeps deleted work reportable by 006.

Separately: attempt to delete a **user** who is a task assignee → **RESTRICT violation**, surfaced as a
handled error, never a silent unassign.

### V14 — Every write audited; reads are not · *(DoD 9, FR-014)*

```sql
SELECT action, change_summary FROM activity_logs WHERE entity_type='Task' ORDER BY timestamp DESC LIMIT 10;
```

**Expect** `TaskCreated`, `TaskUpdated`, `TaskStatusChanged` (**from → to**, reflecting the `closed_at`
effect), `TaskReassigned` (**from → to**), `TaskDeleted`. **No separate `closed_at` event** — spec B.7
forbids it. Then list/read repeatedly → **no new rows**.

### V15 — Paging, clamping, search · *(DoD 8, FR-011)*

`pageSize=500` → clamped to **100**; `page=-1` → **400**; `sort=DROP TABLE` → **400**;
`?search=check` matches "Draft rollout **check**list" via the GIN trigram index (interior substring).

### V16 — The contract gate catches drift · *(DoD 12, X.2)*

```bash
dotnet build -p:CheckApiContract=true      # → passes
```

Then **temporarily add a second property to `UpdateTaskStatusRequest`** and rebuild.
**Expect the build to FAIL.** This is the highest-value drift check in the product: widening that one
schema would silently dismantle the graduated model. Revert.

### V17 — Frontend: only the status control is enabled for an assignee · *(DoD 11, FR-018)*

As `TM` on T1's detail screen: **only the status control is interactive**; Edit / Reassign / Delete are not
rendered. Force `PUT /api/tasks/{id}` from the console → **403**. Confirm the task list applies **no
client-side role filtering**, and that a 409 surfaces a reload-and-reapply prompt.

---

## Test suite

```bash
dotnet test tests/ProjectManagementApp.Application.Tests      # handlers, validators, TaskAccessPolicy (15-cell matrix)
dotnet test tests/ProjectManagementApp.Infrastructure.Tests   # indexes, cascade/RESTRICT, xmin, closed_at transitions
dotnet test tests/ProjectManagementApp.Api.Tests              # scope matrix over HTTP, graduated 403/200 pair, ETag flow
cd src/ProjectManagementApp.Web && npm test
```

**Never skip or mock V2, V4, or V12** — the graduated pair and the concurrency path are only meaningful
against real PostgreSQL (001 R-7).

---

## Definition-of-Done cross-reference

| DoD (spec B.8) | Proven by |
|---|---|
| 1 — eight routes, noun sub-resources | V1, V2, V12, V13 |
| 2 — three-role scope matrix | **V5**, V7 |
| 3 — **the graduated model proven**; 15-cell matrix | **V2**, **V4** |
| 4 — extra fields cannot alter anything | **V3** |
| 5 — `project_id` from the route | V1 |
| 6 — assignee validation (pool, inactive, unassign) | V8 |
| 7 — due date within the project window | V9 |
| 8 — out-of-scope absent from items/`totalCount`; clamping | V5, V15 |
| 9 — audit in same transaction; delete audits first; project cascade | V13, V14 |
| 10 — concurrent writes → 409 | **V12** |
| 11 — lazy standalone `tasks` group, guards only | V17 |
| 12 — RFC 7807; contract authored before handlers | V16 |
| 13 — unit + integration tests pass | Test suite |
