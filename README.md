# ProjectManagementApp

A full-stack project management application: Angular 22 (standalone, NgRx) + .NET 10 Web API
(vertical-slice + Clean Architecture, MediatR) + PostgreSQL 18, with JWT/RBAC authentication.

This repository currently implements **001 — Auth & RBAC**: registration, login, logout, token
refresh, role-based access control, and Admin user management. Features 002–006 (Projects, Tasks,
Team, Dashboard, Reports) are specified under `specs/` but not yet implemented.

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
- PostgreSQL 18 (for running the app itself outside of tests — a local instance or a container)

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

## Documentation

- `docs/shared-contracts.md` — the cross-feature shared kernel (`Result<T>`, `CurrentUser`,
  access policies, pagination, concurrency, audit).
- `docs/adr/` — architectural decision records.
- `docs/erd.md` — entity-relationship diagram.
- `docs/deployment.md` — IIS deployment instructions.
- `specs/001-auth-rbac/quickstart.md` — manual validation scenarios (V1–V18).
