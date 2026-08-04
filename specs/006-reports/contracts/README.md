# Contracts for 006 Reports — pointer

**The contract is not stored here.** It lives at:

### → [`docs/contracts/reports.v1.yaml`](../../../docs/contracts/reports.v1.yaml)

One file covers the catalog **and** all four report types, per the planning directive.

## Contract files for the whole product — **complete**

| Feature | Contract | Ops | Status |
|---|---|---|---|
| 001 Auth & RBAC | `docs/contracts/auth.v1.yaml` | 6 | ✅ |
| 002 Projects | `docs/contracts/projects.v1.yaml` | 5 | ✅ |
| 003 Tasks | `docs/contracts/tasks.v1.yaml` | 8 | ✅ |
| 004 Team | `docs/contracts/team.v1.yaml` | 3 | ✅ |
| 005 Dashboard | `docs/contracts/dashboard.v1.yaml` | 2 | ✅ |
| **006 Reports** | **`docs/contracts/reports.v1.yaml`** | **5** | ✅ **authored (this plan)** |
| | | **29** | **All six authored before any handler exists** |

## Four things worth checking against the implementation

1. **No export endpoint, no `?format`.** The API returns JSON only; PDF and CSV are rendered client-side by
   the single `ReportExportService` (jsPDF + papaparse, Constitution III + VII.8). `formats` in the catalog
   descriptor lists **client render targets**, not API parameters. If a `?format` parameter ever appears in
   the generated spec, the export architecture has silently changed — the drift gate will catch it.
2. **`422` on the Activity Report is intentional**, and is absent from Constitution VI.2's list. It is
   admissible because **this spec declares it** (API catalog, B.5) and OQ-006-02 resolved it — whereas 002
   *rejected* 428 for the opposite reason: a plan may not invent a status code. See
   [research R-5](../research.md).
3. **A TeamMember naming a colleague on Team Performance gets their own row, not a 403.** This looks
   inconsistent with the named-out-of-scope-403 rule and is deliberately the safer behaviour: a 403 would
   confirm the colleague exists. [research R-6](../research.md).
4. **`overdueTasks` must equal 005's overdue count** for the same caller — a hard requirement (NFR-002)
   guarded by a cross-feature test, and the reason 005's configurable-timezone wording had to be corrected
   before this feature could be planned.

## The one write

Every data endpoint — **not** `/catalog` — writes exactly one `ReportGenerated` row to `activity_logs`.
That is the single deliberate exception to "read features don't write", and the contrast with **005**,
which writes nothing at all, is intentional rather than an inconsistency between sibling features.

## Drift gate

```bash
dotnet swagger tofile --output artifacts/openapi/generated.json \
    src/ProjectManagementApp.Api/bin/Release/net10.0/ProjectManagementApp.Api.dll v1
oasdiff breaking docs/contracts/reports.v1.yaml artifacts/openapi/generated.json --fail-on ERR
```

Tooling rationale: [ADR-0007 §1](../../../docs/adr/0007-implementation-conventions.md).
