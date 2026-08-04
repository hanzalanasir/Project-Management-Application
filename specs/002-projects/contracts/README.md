# Contracts for 002 Projects — pointer

**The contract is not stored here.** It lives at:

### → [`docs/contracts/projects.v1.yaml`](../../../docs/contracts/projects.v1.yaml)

## Why

Constitution **X.2** mandates that the OpenAPI contract is authored and reviewed *before* its handler is
implemented and **versioned under `/docs/contracts/`**. The Spec-Kit plan template's default is this
spec-local `contracts/` directory; where the two disagree, the constitution wins — one repo-wide contract
directory is what makes the cross-feature drift check and the Angular `openapi-generator` step practical.
Duplicating the YAML here would create exactly the drift X.2 exists to prevent.

Same convention as [001](../../001-auth-rbac/contracts/README.md).

## Contract files for the whole product

| Feature | Contract | Status |
|---|---|---|
| 001 Auth & RBAC | `docs/contracts/auth.v1.yaml` | ✅ authored |
| **002 Projects** | **`docs/contracts/projects.v1.yaml`** | ✅ **authored (this plan)** |
| 003 Tasks | `docs/contracts/tasks.v1.yaml` | ⏳ authored when 003 is planned |
| 004 Team | `docs/contracts/team.v1.yaml` | ⏳ |
| 005 Dashboard | `docs/contracts/dashboard.v1.yaml` | ⏳ |
| 006 Reports | `docs/contracts/reports.v1.yaml` | ⏳ |

## Drift gate

```bash
dotnet swagger tofile --output artifacts/openapi/generated.json \
    src/ProjectManagementApp.Api/bin/Release/net10.0/ProjectManagementApp.Api.dll v1
oasdiff breaking docs/contracts/projects.v1.yaml artifacts/openapi/generated.json --fail-on ERR
```

Tooling rationale and alternatives: [001 research R-5](../../001-auth-rbac/research.md) — reused here
unchanged, per the planning directive.

## Note on the concurrency mechanism

002 is the first feature to actually contend on `xmin`. The row version travels as an **`ETag` header on
GET** and a **required `If-Match` header on PUT** — not as a body field. Rationale, and why a missing
`If-Match` returns 400 rather than degrading to last-write-wins: [research.md R-2](../research.md).
