# Contracts for 001 Auth & RBAC — pointer

**The contract is not stored here.** It lives at:

### → [`docs/contracts/auth.v1.yaml`](../../../docs/contracts/auth.v1.yaml)

## Why

Constitution **X.2** mandates that the OpenAPI contract is *"authored and reviewed as part of a feature's
spec/plan, before its handler is implemented, **versioned under `/docs/contracts/`**"*. The Spec-Kit plan
template's default location for Phase 1 contract output is this spec-local `contracts/` directory.

Where the two disagree, **the constitution wins** — a single repo-wide contract directory is what makes
the cross-feature build-time drift check practical (one place to diff, one place for the Angular
`openapi-generator` step to read). Duplicating the YAML here would create exactly the drift X.2 exists to
prevent.

This file exists so that anyone following the Spec-Kit convention finds the contract rather than
concluding none was written.

## Contract files for the whole product

| Feature | Contract | Status |
|---|---|---|
| 001 Auth & RBAC | `docs/contracts/auth.v1.yaml` | ✅ authored (this plan) |
| 002 Projects | `docs/contracts/projects.v1.yaml` | ⏳ authored when 002 is planned |
| 003 Tasks | `docs/contracts/tasks.v1.yaml` | ⏳ |
| 004 Team | `docs/contracts/team.v1.yaml` | ⏳ |
| 005 Dashboard | `docs/contracts/dashboard.v1.yaml` | ⏳ |
| 006 Reports | `docs/contracts/reports.v1.yaml` | ⏳ |

## Drift gate

CI fails the build if the implementation diverges breakingly from the authored contract:

```bash
dotnet swagger tofile --output artifacts/openapi/generated.json \
    src/ProjectManagementApp.Api/bin/Release/net10.0/ProjectManagementApp.Api.dll v1
oasdiff breaking docs/contracts/auth.v1.yaml artifacts/openapi/generated.json --fail-on ERR
```

Rationale and tool alternatives: [research.md R-5](../research.md).
