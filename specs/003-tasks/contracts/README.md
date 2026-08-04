# Contracts for 003 Tasks — pointer

**The contract is not stored here.** It lives at:

### → [`docs/contracts/tasks.v1.yaml`](../../../docs/contracts/tasks.v1.yaml)

## Why

Constitution **X.2** mandates the OpenAPI contract be authored before its handler and **versioned under
`/docs/contracts/`**. Where that conflicts with the Spec-Kit template's spec-local `contracts/` default,
the constitution wins — one repo-wide directory is what makes the drift check and the Angular
`openapi-generator` step practical. Same convention as
[001](../../001-auth-rbac/contracts/README.md) and [002](../../002-projects/contracts/README.md).

## Contract files for the whole product

| Feature | Contract | Ops | Status |
|---|---|---|---|
| 001 Auth & RBAC | `docs/contracts/auth.v1.yaml` | 6 | ✅ authored |
| 002 Projects | `docs/contracts/projects.v1.yaml` | 5 | ✅ authored |
| **003 Tasks** | **`docs/contracts/tasks.v1.yaml`** | **8** | ✅ **authored (this plan)** |
| 004 Team | `docs/contracts/team.v1.yaml` | 3 | ⏳ |
| 005 Dashboard | `docs/contracts/dashboard.v1.yaml` | 2 | ⏳ |
| 006 Reports | `docs/contracts/reports.v1.yaml` | 5 | ⏳ |

## What is worth reading in this contract

**The endpoint shapes *are* the authorization design.** `/tasks/{id}/status` and `/tasks/{id}/assignee`
are not convenience routes — they exist so that a narrow request body makes privilege escalation
*structurally* impossible, backing up the `CanMutateAsync(TaskMutation)` check
([research R-2](../research.md)).

Three details a reviewer should verify against the code:

1. **`UpdateTaskStatusRequest` has exactly one property.** Adding a second would silently dismantle the
   graduated model — and the drift gate will surface it.
2. **`closedAt` appears only in responses**, never in a request schema (research R-3). 006 Reports depends
   on it being untamperable.
3. **`If-Match` is required on all three PUTs and deliberately absent on DELETE** (research R-5).

## Drift gate

```bash
dotnet swagger tofile --output artifacts/openapi/generated.json \
    src/ProjectManagementApp.Api/bin/Release/net10.0/ProjectManagementApp.Api.dll v1
oasdiff breaking docs/contracts/tasks.v1.yaml artifacts/openapi/generated.json --fail-on ERR
```

Tooling rationale: [001 research R-5](../../001-auth-rbac/research.md) — reused unchanged.
