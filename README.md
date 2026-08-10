# ProjectManagementApp

A full-stack project management application: Angular 22 (standalone, NgRx) + .NET 10 Web API
(vertical-slice + Clean Architecture, MediatR) + PostgreSQL 18, with JWT/RBAC authentication.

This repository currently implements **001 — Auth & RBAC** (registration, login, logout, token
refresh, role-based access control, Admin user management), **002 — Projects** (create, list/
search, view, edit, delete — role-scoped and audited), and **003 — Tasks** (create, list/search,
view, edit, status update, delete, reassign — the **graduated authorization model**: a TeamMember
may change only the status of a task assigned to them, nothing else). Features 004–006 (Team,
Dashboard, Reports) are specified under `specs/` but not yet implemented.

## Architecture

- **Backend**: `src/ProjectManagementApp.{Domain,Application,Infrastructure,Api}` — Clean
  Architecture, dependency direction enforced by project references (Domain ← Application ←
  Infrastructure ← Api). Vertical slices live under `Application/Features/<Area>/<UseCase>/`.
- **Frontend**: `src/ProjectManagementApp.Web` — Angular 22 standalone components, NgRx for auth
  session state, functional guards/interceptors.
- See `docs/adr/` for architectural decision records and `docs/shared-contracts.md` for the
  cross-feature shared kernel.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Node.js 24](https://nodejs.org/) + npm 11
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (tests use Testcontainers —
  a real PostgreSQL 18 container, never EF InMemory)
- PostgreSQL 18 (for running the app itself outside of tests — a local instance or a container).
  The `pg_trgm` extension is required for 002's project name search and 003's task title search
  (both `?search=`) — the `AddProjectIndexes` migration enables it automatically
  (`CREATE EXTENSION IF NOT EXISTS pg_trgm`), so no manual setup is needed as long as the
  connecting role can create extensions (true for the default `postgres` superuser role; a
  restricted role may need this granted separately). 003 does **not** re-enable the extension.

## Backend setup

```powershell
cd src/ProjectManagementApp.Api

# Configure secrets (development) — never commit these values
dotnet user-secrets init
dotnet user-secrets set "Jwt:SigningKey" "<a-random-32+-character-string>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=projectmanagementapp;Username=postgres;Password=<your-password>"
dotnet user-secrets set "Seed:Admin:Email" "admin@example.com"
dotnet user-secrets set "Seed:Admin:Password" "<a-policy-valid-password>"
dotnet user-secrets set "Seed:ProjectManager:Email" "pm@example.com"
dotnet user-secrets set "Seed:ProjectManager:Password" "<a-policy-valid-password>"
dotnet user-secrets set "Seed:TeamMember:Email" "member@example.com"
dotnet user-secrets set "Seed:TeamMember:Password" "<a-policy-valid-password>"

dotnet run
```

The API applies pending EF Core migrations and (in Development, via `Seed:Enabled`) seeds one
Admin, one ProjectManager, and one TeamMember account on startup.

## Frontend setup

```powershell
cd src/ProjectManagementApp.Web
npm install
npm start   # ng serve — proxies /api to the backend (proxy.conf.json), same-origin dev setup
```

## Migrations

```powershell
cd src/ProjectManagementApp.Infrastructure
dotnet ef migrations add <DescriptiveName> --project . --startup-project .
dotnet ef database update --project . --startup-project .
```

## Tests

Backend integration tests require Docker (Testcontainers spins up a real, ephemeral PostgreSQL
container per test run — see ADR-0007 §2).

```powershell
# Backend — from the repo root
dotnet test

# Frontend — from src/ProjectManagementApp.Web
npm test
```

To verify the API-first contract drift gate (normally CI-only):

```powershell
dotnet build src/ProjectManagementApp.Api -p:CheckApiContract=true
```

## End-to-end run

1. Start PostgreSQL (or leave Docker running for Testcontainers-backed tests).
2. `dotnet run` from `src/ProjectManagementApp.Api` (applies migrations + seeds on first run).
3. `npm start` from `src/ProjectManagementApp.Web`.
4. Open the dev server URL, register or log in with a seeded account, and explore.

For a production-style same-origin build (the Angular bundle served by the API itself), see
`docs/deployment.md`.

## Projects module (002)

Five endpoints under `/api/projects`, all authenticated by default (Constitution V.1) with a
two-layer authorization model: a coarse role gate declared by `[Authorize(Roles=...)]`, plus a
finer ownership/assignment scope gate applied inside the slice handler (`IProjectAccessPolicy`).

| Method | Route | Role gate | Notes |
|---|---|---|---|
| `GET` | `/api/projects` | any authenticated role | role-scoped, paginated, searchable, sortable |
| `POST` | `/api/projects` | Admin, ProjectManager | owner taken from the token for a PM; Admin may set it |
| `GET` | `/api/projects/{id}` | any authenticated role | 403 out-of-scope vs 404 unknown (maskable to 404) |
| `PUT` | `/api/projects/{id}` | Admin, ProjectManager (owner only) | requires `If-Match`; 409 on stale |
| `DELETE` | `/api/projects/{id}` | Admin, ProjectManager (owner only) | cascades to tasks/team members; audit row survives |

Configuration keys (`Projects:*` in `appsettings.json` or user-secrets):

| Key | Default | Meaning |
|---|---|---|
| `Projects:DefaultPageSize` | `20` | List page size when `pageSize` is omitted |
| `Projects:MaxPageSize` | `100` | List page size is clamped to this, never rejected |
| `Projects:DefaultStatus` | `Planning` | Status when creating a project without one |
| `Projects:MaskOutOfScopeAs404` | `false` | Hardening: hide the 403/404 distinction on `GET /api/projects/{id}` |
| `Projects:AllowOwnershipTransfer` | `true` | Master switch for Admin-only ownership transfer on `PUT` |
| `Projects:MaxNameLength` | `200` | Validation bound |
| `Projects:MaxDescriptionLength` | `2000` | Validation bound |

See `specs/002-projects/quickstart.md` for the full manual validation scenarios (V1–V16).

## Tasks module (003)

Eight endpoints across two route shapes, all authenticated by default, with a **graduated**
authorization model on top of the same role-gate + scope-gate pattern 002 uses: passing the role
and scope gates does not grant a whole write — a third gate, per **mutation kind**
(`ITaskAccessPolicy.CanMutateAsync`), decides which fields a caller may actually change. A
TeamMember passes exactly one mutation kind, `StatusChange`, and only on a task assigned to them —
every other write (`FullEdit`, `Reassign`, `Delete`, `Create`) is refused even for their own task.

**`/status` and `/assignee` are authorization boundaries, not convenience routes.** They exist
because the request bodies they bind are structurally narrower than `PUT /api/tasks/{id}` — a
TeamMember's status update has no `title`/`assigneeId` property to smuggle a wider edit through,
so the graduated model is enforced both structurally (narrow DTOs) and behaviourally
(`CanMutateAsync`), never by one alone.

| Method | Route | Role gate | Notes |
|---|---|---|---|
| `GET` | `/api/projects/{projectId}/tasks` | any authenticated role | nested route: 403/404 on the project itself |
| `POST` | `/api/projects/{projectId}/tasks` | Admin, ProjectManager | `projectId` comes from the route, never the body |
| `GET` | `/api/tasks` | any authenticated role | flat route: scope shapes content, never 403/404 |
| `GET` | `/api/tasks/{id}` | any authenticated role | 403 out-of-scope vs 404 unknown (maskable to 404) |
| `PUT` | `/api/tasks/{id}` | any authenticated role (`FullEdit` gate lives in the policy) | requires `If-Match`; TeamMember always refused, message names their narrower right |
| `DELETE` | `/api/tasks/{id}` | any authenticated role (`Delete` gate lives in the policy) | no `If-Match` required; audit row written before removal |
| `PUT` | `/api/tasks/{id}/status` | any authenticated role — **the one write a TeamMember can do** | requires `If-Match`; `closedAt` derived, never accepted |
| `PUT` | `/api/tasks/{id}/assignee` | any authenticated role (`Reassign` gate lives in the policy) | requires `If-Match`; candidate must be an active team member of the project |

Four of the five write endpoints show `any authenticated role` as their attribute — this is
deliberate, not a gap: the actual role/scope/mutation decision is made once, inside
`CanMutateAsync`, so every denial (however it happens) is logged with full context (actor, task id,
reason) through the same MediatR pipeline. An attribute-level `[Authorize(Roles=...)]` would 403 a
TeamMember before ever reaching that policy — silently, with no reason logged.

Configuration keys (`Tasks:*` in `appsettings.json` or user-secrets):

| Key | Default | Meaning |
|---|---|---|
| `Tasks:DefaultPageSize` | `20` | List page size when `pageSize` is omitted |
| `Tasks:MaxPageSize` | `100` | List page size is clamped to this, never rejected |
| `Tasks:DefaultStatus` | `ToDo` | Status when creating a task without one |
| `Tasks:DefaultPriority` | `Medium` | Priority when creating a task without one |
| `Tasks:MaskOutOfScopeAs404` | `false` | Hardening: hide the 403/404 distinction on `GET /api/tasks/{id}` |
| `Tasks:MaxTitleLength` | `200` | Validation bound |
| `Tasks:MaxDescriptionLength` | `2000` | Validation bound |

See `specs/003-tasks/quickstart.md` for the full manual validation scenarios (V1–V17).

## Documentation

- `docs/shared-contracts.md` — the cross-feature shared kernel (`Result<T>`, `CurrentUser`,
  access policies, pagination, concurrency, audit).
- `docs/adr/` — architectural decision records.
- `docs/erd.md` — entity-relationship diagram.
- `docs/deployment.md` — IIS deployment instructions.
- `specs/001-auth-rbac/quickstart.md` — manual validation scenarios (V1–V18).
- `specs/002-projects/quickstart.md` — manual validation scenarios (V1–V16).
- `specs/003-tasks/quickstart.md` — manual validation scenarios (V1–V17), incl. the graduated-model proof.
