# ProjectManagementApp

A full-stack project management application: Angular 22 (standalone, NgRx) + .NET 10 Web API
(vertical-slice + Clean Architecture, MediatR) + PostgreSQL 18, with JWT/RBAC authentication.

This repository currently implements **001 — Auth & RBAC** (registration, login, logout, token
refresh, role-based access control, Admin user management) and **002 — Projects** (create, list/
search, view, edit, delete — role-scoped and audited). Features 003–006 (Tasks, Team, Dashboard,
Reports) are specified under `specs/` but not yet implemented.

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
  The `pg_trgm` extension is required for 002's project name search (`?search=`) — the
  `AddProjectIndexes` migration enables it automatically (`CREATE EXTENSION IF NOT EXISTS pg_trgm`),
  so no manual setup is needed as long as the connecting role can create extensions (true for the
  default `postgres` superuser role; a restricted role may need this granted separately).

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

## Documentation

- `docs/shared-contracts.md` — the cross-feature shared kernel (`Result<T>`, `CurrentUser`,
  access policies, pagination, concurrency, audit).
- `docs/adr/` — architectural decision records.
- `docs/erd.md` — entity-relationship diagram.
- `docs/deployment.md` — IIS deployment instructions.
- `specs/001-auth-rbac/quickstart.md` — manual validation scenarios (V1–V18).
