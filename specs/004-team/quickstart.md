# Quickstart & Validation: 004 Team Management

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Contract**:
[docs/contracts/team.v1.yaml](../../docs/contracts/team.v1.yaml)

Each scenario maps to a Definition-of-Done item in spec 004 B.8. Setup is inherited from
[001](../001-auth-rbac/quickstart.md) — only deltas appear here.

---

## Prerequisites

Everything from 001 (.NET 10, Node 24, **Docker**, `oasdiff`). **004 adds no new tooling.**

**001 and 002 must be implemented** — 004 needs authenticated callers with role claims and real projects
with owners. **003 is needed only for V11** (the open-tasks block); if it is not yet built, insert `tasks`
rows directly — 004 only ever *reads* that table ([research R-5](research.md)).

```bash
dotnet ef database update -p src/ProjectManagementApp.Infrastructure -s src/ProjectManagementApp.Api
dotnet run --project src/ProjectManagementApp.Api
```

**Fixtures** (from builders, never the production seeder — [ADR-0007 §4](../../docs/adr/0007-implementation-conventions.md)):
tokens `$ADMIN`, `$PM`, `$PM2`, `$TM`, `$TM2`; project **A** owned by `PM`, project **B** owned by `PM2`;
one deactivated user `$INACTIVE_ID`.

---

## Validation scenarios

### V1 — Add a member · *(DoD 1, FR-003)*

```bash
curl -sS -X POST $API/projects/$A_ID/team -H "Authorization: Bearer $PM" -i \
  -H 'Content-Type: application/json' -d '{"userId":"'$TM_ID'"}'
```

**Expect** `201` · `Location: /api/projects/{projectId}/team/{userId}` · a `TeamMemberDto` whose `role`
shows the member's **global** role. Confirm `added_by` = the calling PM in the database.

Then include `"projectId":"<B's id>"` in the body → **ignored**; the membership still lands on A.

### V2 — 🎯 Any **active** user is eligible, regardless of role · *(DoD 11, FR-016)*

| Add to project A | Expect |
|---|---|
| `$TM` (TeamMember) | **201** |
| `$PM2` (a ProjectManager who does **not** own A) | **201** — membership is a link, not a role |
| `$ADMIN` | **201** |
| `$INACTIVE_ID` (deactivated) | **400** — the *only* add-time gate |

**The `PM2` row is the point**: adding a ProjectManager as a contributor does not change what they are.
Verify next that `PM2` can now **view** A's roster (V4) but still **cannot manage** it (V5).

### V3 — Membership grants no permissions — the schema proves it · *(DoD 3, FR-002)*

```sql
\d team_members
```

**Expect no role or permission column** — only `id`, `project_id`, `user_id`, `added_by`, `created_at`.
This is the single most important schema fact of the feature: there is exactly one source of a user's
permissions (their global role), so there is no second system to keep in sync.

### V4 — Roster visibility: owner **or** member · *(DoD 2, FR-005)*

With `$TM` and `$PM2` both members of A:

| Caller | `GET /projects/$A_ID/team` | Why |
|---|---|---|
| `$ADMIN` | **200** | any project |
| `$PM` | **200** | owns A |
| `$PM2` | **200** | **member** of A, though not the owner |
| `$TM` | **200**, the **full roster** | member — sees everyone, not just themselves |
| `$TM2` (not a member) | **403** | neither owner nor member |

**`$PM2` returning 200 here and 403 in V5 is the reason `CanViewTeamAsync` and `CanManageTeamAsync` are
two methods** (research R-1).

### V5 — Management is owner-only · *(DoD 2, FR-007)*

| Attempt on project A | Expect |
|---|---|
| `$PM` adds/removes | **200/201/204** — owner |
| `$ADMIN` adds/removes | **success** — any project |
| `$PM2` adds/removes (member, not owner) | **403** |
| `$TM` adds/removes | **403** — role gate, before any data is touched |

### V6 — Empty roster and unknown project · *(FR-006)*

`GET /projects/$B_ID/team` as `$PM2` (owner, no members yet) → **`200` with `[]`**, not 404.
`GET /projects/$UNKNOWN/team` → **404**.

### V7 — Duplicate add → 409, no duplicate row · *(DoD 4, FR-003)*

Re-run V1 verbatim. **Expect `409`** and exactly **one** `team_members` row for `(A, TM)`.

### V8 — 🎯 The concurrency guarantee is the **database constraint**, not the pre-check · *(DoD 6, research R-2/R-3)*

Fire two simultaneous adds of the same `(project, user)`:

```bash
curl -sS -X POST $API/projects/$A_ID/team -H "Authorization: Bearer $PM" \
  -H 'Content-Type: application/json' -d '{"userId":"'$TM2_ID'"}' -o /dev/null -w '%{http_code}\n' &
curl -sS -X POST $API/projects/$A_ID/team -H "Authorization: Bearer $PM" \
  -H 'Content-Type: application/json' -d '{"userId":"'$TM2_ID'"}' -o /dev/null -w '%{http_code}\n' &
wait
```

**Expect exactly one `201` and one `409` — never a `500`, and never two rows.**

This is the assertion that distinguishes a real guarantee from a TOCTOU race: the losing request must hit
the unique constraint and have its SQLSTATE `23505` **caught and mapped to 409**, not escape as an
unhandled `DbUpdateException`. Confirm the row count is 1.

> Only meaningful against real PostgreSQL. EF InMemory enforces no unique constraint, so this test would
> pass vacuously (ADR-0007 §2).

### V9 — No `xmin`, and none is needed · *(DoD 6, FR-011)*

```sql
SELECT column_name FROM information_schema.columns WHERE table_name='team_members';
```

**Expect no `updated_at` and no row-version column.** Then confirm neither endpoint requires `If-Match` —
sending one is simply ignored; omitting it is **not** a 400 (unlike 002/003). The absence is deliberate: a
membership has no mutable field, so there is no lost update to prevent.

### V10 — Removal is idempotent · *(FR-007)*

`DELETE /projects/$A_ID/team/$TM_ID` → **204**. Repeat → **404**. Remove a non-member → **404**.

### V11 — 🎯 Removal blocked while the member has open tasks · *(DoD 11, FR-017)*

Assign `$TM` a task in project A with status `InProgress`, then attempt removal:

**Expect `409`** with a dependency message naming the blocking count, **and verify nothing changed**:
- the `team_members` row still exists
- **no `activity_logs` row was written** — a blocked removal is a complete no-op (spec B.7)
- **the task was not modified** — 004 never mutates `tasks` (research R-5)

Now set the task to `Done` and retry → **204**. A member with only *completed* tasks is removable; their
historical work stays attributed via `assignee_id`.

Finally reassign a still-open task to `$TM2` and retry the original removal → **204**.

### V12 — Removal revokes visibility immediately · *(FR-007)*

After `$TM` is removed from A: their next `GET /api/projects/$A_ID` → **403**, and A disappears from their
`GET /api/projects` list. No grace period, no cached access.

### V13 — 004's records back 003's assignee validation · *(DoD 8)*

The cross-feature integration test: 003 accepts an assignee **iff** a matching `team_members` row exists.

1. Try assigning a task in A to `$TM2` **before** adding them → **400** (not in the pool)
2. Add `$TM2` to A → **201**
3. Retry the assignment → **200**

This proves the pool 004 maintains is exactly the pool 003 validates against — with neither feature calling
the other's handlers.

### V14 — Cascades and audit · *(DoD 7, FR-009/FR-010)*

```bash
psql -d pma -c "SELECT action, change_summary FROM activity_logs WHERE entity_type='TeamMember' ORDER BY timestamp DESC;"
```

**Expect** `TeamMemberAdded` and `TeamMemberRemoved` rows with actor and summary. Then:

| Action | Expect |
|---|---|
| Delete project A | its `team_members` rows cascade away; **audit rows remain** |
| Delete a member's user account | their memberships cascade away; audit remains |
| Delete the user who performed an add | `added_by` becomes **NULL**; the membership survives |

Roster reads write **no** audit rows.

### V15 — The contract gate catches drift · *(DoD 10, X.2)*

```bash
dotnet build -p:CheckApiContract=true      # → passes
```

Then **temporarily add a `role` property to `AddTeamMemberRequest`** and rebuild. **Expect the build to
FAIL.** That specific drift is worth rehearsing: a per-project role field would quietly introduce the
second permission system this feature exists to avoid. Revert.

### V16 — Frontend · *(DoD 9, FR-014)*

As `$TM` viewing A's roster: the table renders the full team, and **"Add member" is not rendered**. Force
`POST .../team` from the console → **403**. As `$PM`, trigger a 409 removal → the dialog surfaces the
blocking-tasks message and points to reassigning them. Confirm the route group is lazy (Network tab) and
that no component contains redirect logic.

---

## Test suite

```bash
dotnet test tests/ProjectManagementApp.Application.Tests      # handlers, validator, TeamAccessPolicy matrix
dotnet test tests/ProjectManagementApp.Infrastructure.Tests   # unique constraint race, cascades, SET NULL
dotnet test tests/ProjectManagementApp.Api.Tests              # three-role matrix, 409 paths
cd src/ProjectManagementApp.Web && npm test
```

**Never skip V8 or V11** — the constraint race and the open-tasks block are the two assertions that make
this feature's guarantees real, and both require real PostgreSQL.

---

## Definition-of-Done cross-reference

| DoD (spec B.8) | Proven by |
|---|---|
| 1 — three routes nested under the project | V1, V6, V10 |
| 2 — three-role matrix (Admin/PM/TM) | **V4**, **V5** |
| 3 — no role/permission column | **V3** |
| 4 — at most one membership per user per project | V7, **V8** |
| 5 — `project_id` from the route | V1 |
| 6 — no `xmin`; race resolved by the constraint; idempotent removal | **V8**, V9, V10 |
| 7 — audit in same transaction; remove audits first; cascades | V14 |
| 8 — backs 003's assignee validation | **V13** |
| 9 — lazy standalone `team` group, guards only | V16 |
| 10 — RFC 7807; contract authored before handlers | V15 |
| 11 — both clarified decisions (any active user; open-tasks 409) | **V2**, **V11** |
| 12 — unit + integration tests pass | Test suite |
