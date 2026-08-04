# Contracts for 005 Dashboard — pointer

**The contract is not stored here.** It lives at:

### → [`docs/contracts/dashboard.v1.yaml`](../../../docs/contracts/dashboard.v1.yaml)

## Why

Constitution **X.2** mandates the contract be authored before its handler and versioned under
`/docs/contracts/`. Same convention as 001–004.

## Contract files for the whole product

| Feature | Contract | Ops | Status |
|---|---|---|---|
| 001 Auth & RBAC | `docs/contracts/auth.v1.yaml` | 6 | ✅ authored |
| 002 Projects | `docs/contracts/projects.v1.yaml` | 5 | ✅ authored |
| 003 Tasks | `docs/contracts/tasks.v1.yaml` | 8 | ✅ authored |
| 004 Team | `docs/contracts/team.v1.yaml` | 3 | ✅ authored |
| **005 Dashboard** | **`docs/contracts/dashboard.v1.yaml`** | **2** | ✅ **authored (this plan)** |
| 006 Reports | `docs/contracts/reports.v1.yaml` | 5 | ⏳ |

## Three absences a reviewer will notice — all deliberate

1. **No 403 and no 404, on either endpoint.** Every other feature's contract has them. The dashboard
   **names no resource** — it asks "summarize what I can see", which always has a valid answer, including
   "nothing". A caller with an empty scope gets `200` with zeros. Returning 403 would mean *"you are
   forbidden from your own empty dashboard"*. This is the stated exception to 002's convention, and it
   follows the same principle 003 and 006 apply: **naming a resource invites a 403; not naming one
   cannot.** See [research R-3](../research.md).
2. **No write endpoint** — not even "mark activity as read". Consequently there is no `CanMutateAsync`, no
   `xmin`, no `ETag`/`If-Match`, and **no audit rows at all**. The empty audit catalog is intentional:
   IV.4 audits *writes*, and this feature has none. Contrast **006**, which is also read-only over domain
   data but writes exactly one `ReportGenerated` row per generation.
3. **No paging on `/summary`.** It is a fixed-N set of scalar and enum-keyed metrics — there is nothing to
   page. Only the unbounded feed pages. Same "nature of the collection" rule as 004's roster.

## Two things worth checking against the implementation

- **Every enum key is `required` in `ProjectStatusCounts` / `TaskStatusCounts`**, with
  `additionalProperties: false`. A status with no rows must serialize as `0`, not vanish. If it vanishes,
  the payload has silently become the "free-form stat dictionary" the spec forbids — and the drift gate
  will catch it.
- **`overdueTaskCount` and `completionRate` must equal 006's values** for the same caller (006 NFR-002,
  guarded by a cross-feature test). That requires `today` to be evaluated in **UTC, fixed** — see
  [research R-2](../research.md), which flags 005's spec wording as needing correction.

## Drift gate

```bash
dotnet swagger tofile --output artifacts/openapi/generated.json \
    src/ProjectManagementApp.Api/bin/Release/net10.0/ProjectManagementApp.Api.dll v1
oasdiff breaking docs/contracts/dashboard.v1.yaml artifacts/openapi/generated.json --fail-on ERR
```

Tooling rationale: [ADR-0007 §1](../../../docs/adr/0007-implementation-conventions.md).
