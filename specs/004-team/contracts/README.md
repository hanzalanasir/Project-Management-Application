# Contracts for 004 Team — pointer

**The contract is not stored here.** It lives at:

### → [`docs/contracts/team.v1.yaml`](../../../docs/contracts/team.v1.yaml)

## Why

Constitution **X.2** mandates the contract be authored before its handler and versioned under
`/docs/contracts/`. Same convention as [001](../../001-auth-rbac/contracts/README.md),
[002](../../002-projects/contracts/README.md), and [003](../../003-tasks/contracts/README.md).

## Contract files for the whole product

| Feature | Contract | Ops | Status |
|---|---|---|---|
| 001 Auth & RBAC | `docs/contracts/auth.v1.yaml` | 6 | ✅ authored |
| 002 Projects | `docs/contracts/projects.v1.yaml` | 5 | ✅ authored |
| 003 Tasks | `docs/contracts/tasks.v1.yaml` | 8 | ✅ authored |
| **004 Team** | **`docs/contracts/team.v1.yaml`** | **3** | ✅ **authored (this plan)** |
| 005 Dashboard | `docs/contracts/dashboard.v1.yaml` | 2 | ⏳ |
| 006 Reports | `docs/contracts/reports.v1.yaml` | 5 | ⏳ |

## Two absences a reviewer will notice — both deliberate

This contract omits things every other feature's contract has. Neither is an oversight:

1. **No `ETag` / `If-Match`.** A `team_members` row has no mutable field — it is added or removed, never
   edited — so there is no lost update for optimistic concurrency to prevent. The guarantee comes from the
   `UNIQUE (project_id, user_id)` constraint instead: a concurrent duplicate add resolves to one **201**
   and one **409**. See [research R-2](../research.md); the shared-contracts §5 taxonomy was extended to
   name this exclusion category explicitly.
2. **No pagination envelope.** The roster returns a plain array. Constitution VI.4 requires paging for
   collections that *"can exceed 50 items"*; a project team is bounded and human-scale, so the rule does
   not fire. This is **compliance, not an exception** — no waiver needed. See [research R-4](../research.md).

Also absent, and equally deliberate: **no role field on `TeamMemberDto` writes**. The `role` shown in the
roster is a read-only reflection of the member's *global* role. There is no per-project role in this
product, and adding one to this schema would be a significant design change — the drift gate will surface
it.

## Drift gate

```bash
dotnet swagger tofile --output artifacts/openapi/generated.json \
    src/ProjectManagementApp.Api/bin/Release/net10.0/ProjectManagementApp.Api.dll v1
oasdiff breaking docs/contracts/team.v1.yaml artifacts/openapi/generated.json --fail-on ERR
```

Tooling rationale: [ADR-0007 §1](../../../docs/adr/0007-implementation-conventions.md).
