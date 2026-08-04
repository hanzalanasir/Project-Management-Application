# Quickstart & Validation: 002 Project Management

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Contract**:
[docs/contracts/projects.v1.yaml](../../docs/contracts/projects.v1.yaml)

How to exercise the feature and **prove it works**. Each scenario maps to a Definition-of-Done item in
spec 002 B.8. Setup is inherited from
[001's quickstart](../001-auth-rbac/quickstart.md) — only the deltas appear here.

---

## Prerequisites

Everything from [001 §Prerequisites](../001-auth-rbac/quickstart.md) (.NET 10, Node 24, Docker for
Testcontainers, `oasdiff`). **002 adds no new tooling.**

**001 must be implemented and running** — 002 needs authenticated callers with real role claims. The seeded
Admin / ProjectManager / TeamMember accounts from US-001-06 are the test fixtures for every scenario below.

```bash
# Apply 002's index migration (the projects TABLE already exists — see data-model.md §1)
dotnet ef database update -p src/ProjectManagementApp.Infrastructure -s src/ProjectManagementApp.Api

dotnet run --project src/ProjectManagementApp.Api
cd src/ProjectManagementApp.Web && npm start
```

Grab tokens once and reuse them:

```bash
API=https://localhost:7001/api
tok() { curl -sS -X POST $API/auth/login -H 'Content-Type: application/json' \
        -d "{\"email\":\"$1\",\"password\":\"$2\"}" | jq -r .accessToken; }
ADMIN=$(tok admin@example.com   "$SEED_PW")
PM=$(tok    pm@example.com      "$SEED_PW")
PM2=$(tok   pm2@example.com     "$SEED_PW")   # a SECOND manager — needed for cross-owner denial
TM=$(tok    member@example.com  "$SEED_PW")
```

> Seed a **second ProjectManager** if one does not exist. Without it, the single most important test in
> this feature — a manager being refused another manager's project — cannot be written.

---

## Validation scenarios

### V1 — Create: the owner comes from the token, never the body · *(DoD 3, FR-003)*

```bash
curl -sS -X POST $API/projects -H "Authorization: Bearer $PM" -i \
  -H 'Content-Type: application/json' \
  -d '{"name":"Apollo Rollout","description":"Regional launch",
       "startDate":"2026-08-01","endDate":"2026-11-30","status":"Planning"}'
```

**Expect** `201` · `Location: /api/projects/{id}` · an `ETag` header · `owner.id` = **the PM's own id**.

Now attempt forgery — repeat with `"ownerId":"<some other user id>"` in the body.
**Expect the field to be silently ignored**; the owner is still the calling PM. *(T.8: ownership can never be forged.)*

### V2 — TeamMember is refused every write · *(DoD 2, FR-005)*

```bash
curl -sS -X POST   $API/projects       -H "Authorization: Bearer $TM" -d '{...}' -o /dev/null -w '%{http_code}\n'
curl -sS -X PUT    $API/projects/$ID   -H "Authorization: Bearer $TM" -d '{...}' -o /dev/null -w '%{http_code}\n'
curl -sS -X DELETE $API/projects/$ID   -H "Authorization: Bearer $TM"            -o /dev/null -w '%{http_code}\n'
```

**Expect `403` from all three**, refused at the **role gate** before any data is touched.

### V3 — An ineligible owner is rejected · *(FR-003, Clarifications 2026-07-22)*

As **Admin**, create a project with `"ownerId"` set to the **TeamMember's** id.

**Expect `400`** with a field error on `ownerId` — a TeamMember may not own a project they could never
manage. Repeat on **update** (ownership transfer): **also 400**.

### V4 — 🎯 The three-role scope matrix (the primary acceptance test) · *(DoD 2)*

Seed: project **A** owned by `PM`, project **B** owned by `PM2`, and assign `TM` to **A** only.

| Caller | `GET /api/projects` returns | `totalCount` |
|---|---|---|
| `ADMIN` | A **and** B | 2 |
| `PM` | **A only** | **1** |
| `PM2` | **B only** | **1** |
| `TM` | **A only** (assigned) | **1** |

**The `totalCount` column is the real assertion.** A PM seeing `totalCount: 2` while receiving one item
would mean the scope filter ran *after* counting — out-of-scope existence leaking through metadata
(FR-007). Assert counts, not just items.

### V5 — Out-of-scope is 403; unknown is 404 · *(DoD 2, FR-011)*

```bash
curl -sS $API/projects/$B_ID              -H "Authorization: Bearer $PM"  -o /dev/null -w '%{http_code}\n'  # → 403
curl -sS $API/projects/00000000-0000-0000-0000-000000000000 -H "Authorization: Bearer $PM" -w '%{http_code}\n' # → 404
curl -sS $API/projects/not-a-guid         -H "Authorization: Bearer $PM"  -o /dev/null -w '%{http_code}\n'  # → 400
curl -sS $API/projects/$B_ID              -H "Authorization: Bearer $ADMIN" -o /dev/null -w '%{http_code}\n' # → 200
```

**Expect 403 / 404 / 400 / 200** respectively. Then flip `Projects:MaskOutOfScopeAs404=true` and confirm
the first call becomes **404** — the configurable hardening path (OQ-002-03).

### V6 — Empty scope is an empty list, not an error · *(FR-006)*

A TeamMember with **no** assignments lists projects.

**Expect `200`** with `items: []` and `totalCount: 0` — **not** 403, **not** 404. An empty workspace is a
valid state, not a failure.

### V7 — Paging, clamping, and bad input · *(DoD 5, FR-008)*

```bash
curl -sS "$API/projects?page=1&pageSize=500" -H "Authorization: Bearer $ADMIN" | jq .pageSize   # → 100 (clamped)
curl -sS "$API/projects?page=999"            -H "Authorization: Bearer $ADMIN" | jq '.items|length'  # → 0, metadata valid
curl -sS "$API/projects?page=-1"             -H "Authorization: Bearer $ADMIN" -w '%{http_code}\n'   # → 400
curl -sS "$API/projects?sort=DROP+TABLE"     -H "Authorization: Bearer $ADMIN" -w '%{http_code}\n'   # → 400
```

**Expect** oversized `pageSize` **clamped to 100** (not rejected), an out-of-range page to return empty
items with valid metadata, and negative paging **and an unrecognized `sort`** to return 400 (R-3's
whitelist).

### V8 — Search finds substrings, within scope · *(FR-008)*

```bash
curl -sS "$API/projects?search=pollo" -H "Authorization: Bearer $ADMIN" | jq '.items[].name'
```

**Expect** "Apollo Rollout" to match on the **interior** substring `pollo` — this is what the `pg_trgm`
GIN index exists for (R-3); a B-tree would force a sequential scan here. Then run the same search as `PM2`
and **expect zero results** — a filter narrows a scope, it can never widen one.

### V9 — 🎯 Optimistic concurrency: stale write is refused · *(DoD 10, FR-017)*

```bash
ETAG=$(curl -sS -D - -o /dev/null $API/projects/$ID -H "Authorization: Bearer $PM" | grep -i '^etag:' | tr -d '\r' | cut -d' ' -f2)

# First update with the current ETag → succeeds, returns a NEW ETag
curl -sS -X PUT $API/projects/$ID -H "Authorization: Bearer $PM" -H "If-Match: $ETAG" \
     -H 'Content-Type: application/json' -d '{"name":"Apollo Rollout v2","startDate":"2026-08-01","status":"Active"}' -w '%{http_code}\n'

# Replay the SAME (now stale) ETag → must be refused
curl -sS -X PUT $API/projects/$ID -H "Authorization: Bearer $PM" -H "If-Match: $ETAG" \
     -H 'Content-Type: application/json' -d '{"name":"Clobbered","startDate":"2026-08-01","status":"Active"}' -w '%{http_code}\n'
```

**Expect `200` then `409`.** Then confirm the name is still `Apollo Rollout v2` — **the second write must
not have landed**. A silent overwrite here is the failure ADR-0004 exists to prevent.

Finally, omit `If-Match` entirely → **expect `400`**, not a successful last-write-wins update (R-2).

### V10 — Cross-owner mutation is refused at write time · *(DoD 2, FR-010)*

`PM` attempts `PUT` and `DELETE` on project **B** (owned by `PM2`).
**Expect `403` for both**, and verify B is unchanged. The check is re-evaluated **at write time** — a
stale earlier read must never authorize a later mutation.

### V11 — Delete cascades; the audit survives · *(DoD 6, 7; FR-012, FR-013)*

With project A carrying tasks and team members (insert directly if 003/004 aren't built yet):

```bash
curl -sS -X DELETE $API/projects/$A_ID -H "Authorization: Bearer $PM" -w '%{http_code}\n'   # → 204
psql -d pma -c "SELECT count(*) FROM tasks        WHERE project_id='$A_ID';"   -- → 0
psql -d pma -c "SELECT count(*) FROM team_members WHERE project_id='$A_ID';"   -- → 0
psql -d pma -c "SELECT action, change_summary FROM activity_logs WHERE entity_id='$A_ID';"
```

**Expect** dependents gone, and the `ProjectDeleted` audit row **still present** with a pre-removal
snapshot. Audit rows are never cascaded away — that is what keeps a deleted project reportable by 006.

Then delete twice → **second call 404**.

### V12 — A user who owns projects cannot be deleted · *(DoD 7)*

Attempt to delete `PM` while they own a project.
**Expect a RESTRICT violation** surfaced as a handled error, not a cascade that silently destroys projects.

### V13 — Every write is audited, in the same transaction · *(DoD 6, FR-012)*

```sql
SELECT action, entity_type, actor_id, change_summary
FROM activity_logs WHERE entity_type='Project' ORDER BY timestamp DESC LIMIT 10;
```

**Expect** `ProjectCreated`, `ProjectUpdated` (with a changed-field summary), `ProjectDeleted`, and
`ProjectOwnerChanged` rows. Then force a mid-write failure and confirm **neither** the project change
**nor** its audit row persists — they commit together or not at all.

### V14 — Reads are not audited · *(spec Audit Expectations)*

List and fetch projects repeatedly, then re-check `activity_logs`.
**Expect no new rows.** Constitution IV.4 audits *writes*; auditing reads would drown the trail.

### V15 — The contract gate catches drift · *(DoD 9, X.2)*

```bash
dotnet build -p:CheckApiContract=true      # → passes
```

Now **temporarily** rename a response property (e.g. `totalCount` → `total`) and rebuild.
**Expect the build to FAIL** with an `oasdiff` breaking report. Revert. Same discipline as 001 V13 — a gate
never seen to fail is not a gate.

### V16 — Frontend: scoping is server-side, guards are the only block · *(DoD 8, FR-015)*

1. Log in as `TM` → the project list shows only assigned projects, and **"New Project" is not rendered**.
   Then force `POST /api/projects` from the console → **403**. *UI hiding is convenience; the API is the boundary.*
2. Confirm the list view applies **no client-side role filtering** — it renders exactly what the API returns.
3. Navigate to an edit route as `TM` → the **route guard** blocks it and the lazy chunk is never fetched
   (Network tab). No component-level redirect logic exists.
4. Trigger a 409 in the UI (edit the same project in two tabs) → the second save surfaces a
   reload-and-reapply prompt, not a silent overwrite.

---

## Test suite

```bash
dotnet test tests/ProjectManagementApp.Application.Tests      # handlers, validators, ProjectAccessPolicy
dotnet test tests/ProjectManagementApp.Infrastructure.Tests   # indexes, cascade/RESTRICT, xmin concurrency
dotnet test tests/ProjectManagementApp.Api.Tests              # scope matrix, 403/404, paging, ETag flow
cd src/ProjectManagementApp.Web && npm test                   # ProjectsService, guard, form validators
```

**The scope matrix (V4) and the concurrency test (V9) are the two that must never be skipped or mocked** —
both are only meaningful against real PostgreSQL (001 R-7).

---

## Definition-of-Done cross-reference

| DoD (spec B.8) | Proven by |
|---|---|
| 1 — five routes exactly as named, per the status table | V1, V4, V5, V9, V11 |
| 2 — three-role scope matrix; TeamMember 403 on writes | **V4**, V2, V5, V10 |
| 3 — ownership cannot be forged; transfer is Admin-only | V1, V3 |
| 4 — out-of-scope rows absent from items, `totalCount`, paging | **V4**, V8 |
| 5 — paging defaults, clamping, 400 on invalid | V7 |
| 6 — every write audited in the same transaction; delete audits first | V11, V13, V14 |
| 7 — cascade to dependents; user delete restricted | V11, V12 |
| 8 — lazy standalone `projects` group, HTTP in `ProjectsService`, guard-only navigation | V16 |
| 9 — RFC 7807; contract authored before handlers and validated against | V15 |
| 10 — concurrent updates rejected with 409, never a silent overwrite | **V9** |
| 11 — unit + integration tests pass | Test suite |
