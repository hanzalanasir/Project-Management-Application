# Implementation Plan: 001 Auth & RBAC

**Branch**: `001-auth-rbac` | **Date**: 2026-07-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-auth-rbac/spec.md`

**Governed by**: Project Constitution **v1.3.0** · **Governance §5 gate**: spec 001 was revised against
v1.3.0 on 2026-07-29 (see its `**Revised**` header line) — **the gate is satisfied and planning may
proceed.**

> **This is the foundational plan.** The solution layout, MediatR pipeline, contract tooling, test
> strategy, and shared-kernel wiring decided here are the **project-wide defaults**. Features 002–006
> reuse them by citation and re-decide nothing — their plans cover only what is genuinely new to them.

---

## Summary

Build the identity and authorization layer every other feature sits behind: registration, login, logout,
short-lived JWT access tokens with single-use refresh-token rotation, the three-role model
(Admin / ProjectManager / TeamMember) enforced by `[Authorize(Roles=…)]` attributes, an audited write path,
and idempotent seeding so a fresh database is immediately usable.

**Technical approach.** A four-assembly Clean Architecture solution where the dependency rule is enforced
by the compiler rather than by review. Each use case is a vertical slice under
`Application/Features/Auth/<UseCase>/` holding its Command/Query, FluentValidation validator, handler, and
response shape. Controllers are thin — one HTTP verb, one `MediatR.Send(...)`, no logic. Two open-generic
pipeline behaviors (`LoggingBehavior` → `ValidationBehavior`) mean handlers never invoke a validator and
every request is logged with its duration. The OpenAPI contract is hand-authored **before** any handler
exists and a CI build step fails on breaking drift between it and the running implementation. Persistence
goes through `IApplicationDbContext` — the DbContext behind an Application-owned interface, **not** a
repository — preserving `IQueryable` composition, which later features' scope predicates depend on for
correctness.

---

## Technical Context

**Language/Version**: C# 13 on **.NET 10**, nullable reference types enabled, warnings-as-errors ·
TypeScript 5.x strict on **Angular 22** (Node 24, npm 11)

**Primary Dependencies**: ASP.NET Core Web API · **MediatR** (command/query dispatch, III) ·
**FluentValidation** (ADR-0005) · **EF Core 10 + Npgsql** · **ASP.NET Core Identity** ·
JWT bearer authentication · **Serilog** (console + rolling file) · **Swashbuckle** (dev Swagger UI only) ·
**Angular Material** (III's default, committed by this plan) · **NgRx** (auth session)

**Storage**: **PostgreSQL 18**, accessed exclusively through EF Core; Code-First migrations; snake_case
identifiers; `xmin` mapped as the optimistic-concurrency token (ADR-0004)

**Testing**: **xUnit** + **Testcontainers PostgreSQL** + **Respawn** (backend — real PostgreSQL wherever
EF is involved, [research.md R-7](research.md)) · **`WebApplicationFactory`** integration tests (IX.1) ·
**Jasmine + Karma** (frontend, IX.2)

**Target Platform**: IIS on Windows Server (XI.1); API self-contained deployment serving the Angular
production bundle **same-origin** (ADR-0002), which is what allows `SameSite=Strict` on the refresh cookie
in every environment

**Project Type**: Web application — .NET Web API backend + Angular SPA frontend, one repository

**Performance Goals**: Auth endpoints p95 < 200 ms on dev hardware · login/refresh are single-round-trip
database operations · **authorization on protected reads costs zero database round-trips** (stateless JWT
validation) — NFR-002

**Constraints**: Server-side enforcement is authoritative, the frontend is never trusted (NFR-001) ·
secrets never in source control (V.4) · passwords never stored plaintext, logged, or returned (V.3) ·
refresh tokens stored hashed and never placed in a response body (FR-016)

**Scale/Scope**: Hundreds of users; thousands of projects/tasks once 002–003 land — pagination mandatory
from day one (VI.4). This feature: **6 endpoints, 6 user stories, 2 UI screens** (Register, Login) plus
app-shell cross-cutting UI (guards, interceptors, logout control).

---

## Constitution Check

*GATE: must pass before Phase 0. Re-checked after Phase 1 design — see §Post-Design Re-check.*

| Principle | Gate | Status |
|---|---|---|
| **I.1** Scope fidelity | Implements the brief's Authentication module; no deviation | ✅ |
| **I.2** No bonus features | No SSO/MFA/social login; architecture does not preclude them | ✅ |
| **II.1** Three-tier separation | Angular presentation-only; API owns all authorization; PostgreSQL sole state | ✅ |
| **II.2** Vertical slice + Clean Architecture + thin controllers | `Features/Auth/<UseCase>/` in `.Application`; controllers are one `Send()`; dependency arrow enforced by assembly references (R-1) | ✅ |
| **II.3** JSON/HTTPS, resource-oriented URLs | `/api/auth/*`, nouns, HTTP verbs | ✅ |
| **II.4** Real-time not precluded | No design choice blocks adding SignalR later | ✅ |
| **III** Stack locked | .NET 10, EF Core 10 + Npgsql, PG 18, Angular 22, NgRx, Serilog, Identity, JWT, **MediatR**; **Angular Material** chosen as III's stated default (Bootstrap MUST NOT also be added) | ✅ |
| **IV.1** DbContext, no raw SQL | `IApplicationDbContext`, direct LINQ, **no repository** (R-3); zero raw SQL | ✅ |
| **IV.2** Migrations, descriptive names | `InitialCreate`; no manual DDL | ✅ |
| **IV.3** Five entities, FKs, navigations, explicit cascade | All five created in `InitialCreate` (R-10, data-model §1); cascade table in data-model §4 | ✅ |
| **IV.4** Audit every write | Audit row committed in the **same** `SaveChangesAsync` as the change (data-model §5) | ✅ |
| **IV.5** Idempotent seed | `IDataSeeder` at startup; existence checks **+ unique indexes** for the multi-instance race (R-9) | ✅ |
| **V.1** Authenticated by default | Global fallback policy; exactly four `[AllowAnonymous]` endpoints — register, login, refresh, health | ✅ |
| **V.2** Roles via attributes only | `[Authorize(Roles=…)]`; **no in-body role checks** (grep-asserted, quickstart V8) | ✅ |
| **V.3** Password handling | Identity PBKDF2; never logged — `LoggingBehavior` deliberately does not log request bodies (R-4) | ✅ |
| **V.4** Secrets | user-secrets (dev) / environment (prod); **fail fast at startup if absent** | ✅ |
| **V.5** Validation at the boundary | FluentValidation via `ValidationBehavior`; frontend validates for UX only | ✅ |
| **V.6** CORS allow-list | Explicit list, no wildcard; inert in practice under same-origin (ADR-0002) | ✅ |
| **VI.1** `/api` base, versionable | Contract `servers: /api`; a future `/api/v1` prefix does not break clients | ✅ |
| **VI.2** Status codes | 201+`Location`, 200, 204, 400, 401, 403, 409 — mapped in one place (R-8) | ✅ |
| **VI.3** RFC 7807 | Single `ToActionResult()` mapper; contract declares `application/problem+json` | ✅ |
| **VI.4** Pagination | **N/A** — 001 exposes no collection endpoint. `PagedResult<T>` lands with 002 | ➖ |
| **VI.5** Swagger UI in development | Swashbuckle enabled in dev **for exploration only**; it is not the contract (X.2) | ✅ |
| **VI.6** Exact brief endpoints | 001 owns the auth endpoints; the five named project routes arrive in 002 | ✅ |
| **VII.1–VII.2** Lazy standalone `auth` route group | `loadChildren` → `auth.routes.ts`; **no `@NgModule`** (ADR-0001) | ✅ |
| **VII.3** HTTP in services; may be contract-generated | `AuthService` wraps a generated typed client; **DTO types generated** from `auth.v1.yaml` (R-6) | ✅ |
| **VII.4** JWT + 401 interceptors | Functional `HttpInterceptorFn`s; single-flight refresh | ✅ |
| **VII.5** Guards are the only navigation block | Functional `CanActivateFn`/`CanMatchFn`; no component-level redirects | ✅ |
| **VII.6** Reactive Forms + shared error display | Material form-field + one error-display component | ✅ |
| **VII.7** Global error handling | `ErrorInterceptor` + global `ErrorHandler` → notification component | ✅ |
| **VII.8** Export service | **N/A** — no reports in 001 | ➖ |
| **VIII.1–VIII.5** Code quality | warnings-as-errors, nullable, TS strict, naming, doc comments, Conventional Commits | ✅ |
| **IX.1** xUnit on **handlers** + `WebApplicationFactory` | Per-branch handler tests; controller happy-path + error-path integration tests (R-7) | ✅ |
| **IX.2** Jasmine + Karma | Guards, interceptors, validators, components with logic | ✅ |
| **IX.3** No merge on failing tests | CI gate | ✅ |
| **IX.4** Builders/factories for test data | No inline literals | ✅ |
| **X.1** README | Task in this feature's breakdown | ✅ |
| **X.2** **API-first** | `docs/contracts/auth.v1.yaml` hand-authored **before** handlers; CI diff gate; **proven to fail** (quickstart V13) | ✅ |
| **X.3** ADRs for significant decisions | ADR-0001..0006 apply; **see Follow-ups — ADR-0007 candidate** | ⚠️ |
| **X.4** ERD | Generated from `InitialCreate`; task in breakdown | ✅ |
| **X.5** Demo script | Deferred to delivery; quickstart V1–V14 is the seed material | ✅ |
| **XI.1–XI.3** IIS deployment, env config, docs | Self-contained publish; `appsettings.{Env}.json` + `environment.{name}.ts`; no hardcoded env values | ✅ |
| **Governance §5** Pre-v1.2.0 spec revision gate | **Satisfied** — 001 revised 2026-07-29 against v1.3.0 | ✅ |

**Gate result: PASS.** Two entries carry deviations that are justified in Complexity Tracking below; one
(X.3) is a recommended follow-up, not a blocker.

---

## Project Structure

### Documentation (this feature)

```text
specs/001-auth-rbac/
├── plan.md              # This file
├── research.md          # Phase 0 — 10 decisions, all NEEDS CLARIFICATION resolved
├── data-model.md        # Phase 1 — entities, EF config, migration scope
├── quickstart.md        # Phase 1 — run + 14 validation scenarios mapped to DoD
├── contracts/README.md  # Pointer → docs/contracts/auth.v1.yaml (X.2 location wins)
├── checklists/
│   └── requirements.md  # Pre-existing spec-quality checklist
└── tasks.md             # Phase 2 — created by /speckit.tasks, NOT by this command

docs/contracts/auth.v1.yaml   # THE CONTRACT — authored before any handler
```

### Source Code (repository root)

```text
ProjectManagementApp.sln
│
├── src/
│   ├── ProjectManagementApp.Domain/              # → no project references (R-2)
│   │   ├── Entities/         ApplicationUser, ApplicationRole, RefreshToken, ActivityLog,
│   │   │                     Project, TaskItem, TeamMember      # last three: table-only in 001
│   │   └── Enums/            Role, AuditAction, ProjectStatus, TaskStatus, TaskPriority
│   │
│   ├── ProjectManagementApp.Application/         # → Domain
│   │   ├── Common/
│   │   │   ├── Behaviors/    LoggingBehavior, ValidationBehavior          # MediatR pipeline (R-4)
│   │   │   ├── Interfaces/   IApplicationDbContext, ICurrentUserService,
│   │   │   │                 ITokenService, IActivityLogService, IDataSeeder
│   │   │   └── Models/       Result, Result<T>, Error, ErrorKind, CurrentUser,
│   │   │                     AccessDecision, PagedResult<T>               # shared-contracts §1–§4
│   │   ├── Features/
│   │   │   └── Auth/                                                      # vertical slices, spec B.3
│   │   │       ├── Register/         RegisterCommand · Validator · Handler · UserDto
│   │   │       ├── Login/            LoginCommand · Validator · Handler · AuthTokens
│   │   │       ├── Refresh/          RefreshCommand · Handler
│   │   │       ├── Logout/           LogoutCommand · Handler
│   │   │       └── GetCurrentUser/   GetCurrentUserQuery · Handler
│   │   └── DependencyInjection.cs
│   │
│   ├── ProjectManagementApp.Infrastructure/      # → Application, Domain
│   │   ├── Persistence/      ApplicationDbContext (: IApplicationDbContext),
│   │   │                     Configurations/, Migrations/InitialCreate
│   │   ├── Identity/         TokenService                                  # JWT sign + refresh validate
│   │   ├── Services/         ActivityLogService, DataSeeder
│   │   └── DependencyInjection.cs
│   │
│   ├── ProjectManagementApp.Api/                 # → Application, Infrastructure (composition root)
│   │   ├── Controllers/      AuthController, HealthController              # thin: one Send() each
│   │   ├── Common/           ResultExtensions (Result → ActionResult, R-8),
│   │   │                     ExceptionHandlingMiddleware
│   │   ├── Services/         CurrentUserService                            # IHttpContextAccessor (R-8)
│   │   ├── Program.cs        # auth, DI, Serilog, CORS, Swagger(dev), migrate+seed
│   │   └── appsettings.json / appsettings.Development.json
│   │
│   └── ProjectManagementApp.Web/                 # Angular 22 workspace
│       └── src/app/
│           ├── core/         interceptors/ (jwt, error/401) · guards/ (auth, role) ·
│           │                 services/ · store/auth/ (NgRx) · api/generated/ (from contract)
│           ├── shared/       error-display/ · notification/
│           └── features/auth/  register/ · login/ · auth.routes.ts        # lazy standalone
│
├── tests/
│   ├── ProjectManagementApp.Application.Tests/   # handlers, validators, behaviors, token service
│   ├── ProjectManagementApp.Infrastructure.Tests/# migrations, xmin, cascades, seeder idempotency
│   └── ProjectManagementApp.Api.Tests/           # WebApplicationFactory: HTTP, 401/403 matrix
│
└── docs/
    ├── contracts/auth.v1.yaml
    ├── adr/0001..0006
    └── shared-contracts.md
```

**Structure Decision.** Four source assemblies, one per Clean Architecture layer, because Constitution
II.2's inward dependency rule becomes a **compile error** rather than a review convention when layers are
separate projects ([research.md R-1](research.md)). Vertical slices live inside `.Application` exactly as
spec 001 B.3 specifies — slices are a *folder* organization within the Application layer, not a
replacement for it. Three test projects, one per layer that has behavior worth testing; **no
`Domain.Tests`**, since 001's Domain is entities and enums only — it gets created when Domain first gains
a behavioral rule (likely 003's `closed_at` invariant). The Angular workspace sits under `src/` and builds
into the API's `wwwroot/` to satisfy ADR-0002's same-origin requirement.

---

## Complexity Tracking

> Deviations from a strict reading of a principle, each justified.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| **`.Domain` takes a package reference on `Microsoft.Extensions.Identity.Stores`** — so it is not literally "depends on nothing" (II.2) | III locks ASP.NET Core Identity, and **IV.3 requires navigation properties** (`Project.Owner`, `TaskItem.Assignee`, `TeamMember.User`). Those navigations force the user entity to be visible from Domain. The referenced package is a storage- and framework-agnostic POCO/abstraction package — no EF Core, no ASP.NET, no database. | *Pure Domain `User` + separate Infrastructure `ApplicationUser` with mapping* — creates two user identities to keep in sync, **breaks IV.3 navigations** (Domain would hold bare `Guid` FKs), and taxes all six features with mapping for a purity win no requirement asks for. |
| **`.Application` references EF Core** (for `DbSet<T>`/LINQ via `IApplicationDbContext`) | IV.1 makes direct `DbContext` access the default persistence path, and `IProjectAccessPolicy.ApplyScope` must fold a predicate into a **live `IQueryable`**. That composition is impossible if Application cannot see EF types. | *Repository returning materialized collections* — already rejected by ADR-0006, and it would push scope filtering into memory, so out-of-scope rows would be loaded — **breaking 002 FR-007/NFR-002, a security property, not a style rule.** |
| **CI-only contract diff by default** (X.2's gate does not run on every local build) | A build → emit → diff cycle on every `dotnet build` adds seconds to the inner loop and requires `oasdiff` on every machine. The build that gates merges is the authoritative one. | *Always-on local gate* — measurably worse inner loop for a check that cannot reach `main` unnoticed anyway. Opt-in locally via `-p:CheckApiContract=true`. |
| **Seven projects** (4 source + 3 test) + an Angular workspace for one feature | Not a violation of any principle — II.2 **mandates** the Domain/Application/Infrastructure separation, and separate assemblies are its enforcement mechanism. Recorded here only because the count looks high at first glance. | *Fewer projects* would make the mandated dependency rule unenforceable (see R-1). |

---

## Post-Design Re-check (after Phase 1)

Re-evaluated after `data-model.md`, `auth.v1.yaml`, and `quickstart.md` were produced:

- **II.2 still holds** — the contract-first design introduced no controller logic; every endpoint in
  `auth.v1.yaml` maps to exactly one slice in the structure above.
- **IV.3 strengthened, not weakened** — designing `InitialCreate` surfaced the cross-spec obligation
  (R-10) that all five entities must exist from the first migration. Missing it would have silently broken
  002's first scope test. Captured in data-model §1.
- **V.3 verified against the pipeline** — `LoggingBehavior` sits outside `ValidationBehavior` and would
  naturally log the request object; the design explicitly forbids logging request bodies because
  `RegisterCommand`/`LoginCommand` carry plaintext passwords. This was found *because* the behavior order
  was written down.
- **X.2 now has a proof step** — quickstart V13 requires deliberately breaking a DTO and observing the
  build fail. An ungated gate would otherwise be indistinguishable from a working one.
- **No new violations.** Complexity Tracking is unchanged from the pre-Phase-0 evaluation.

**Gate result after design: PASS.**

---

## Follow-ups (not blockers for `/speckit.tasks`)

1. ~~**ADR-0007 candidate (X.3).**~~ **✅ RESOLVED 2026-07-31.** 002 and 003 confirmed both conventions
   generalize, so R-5 (contract drift gate) and R-7 (Testcontainers over InMemory) are now recorded in
   **[`docs/adr/0007-implementation-conventions.md`](../../docs/adr/0007-implementation-conventions.md)**,
   together with 002's `ETag`/`If-Match` concurrency transport and the test-fixture rule. One ADR covers
   all four, since they share the theme of how the repo verifies itself.
2. ~~**`IApplicationDbContext` belongs in the shared kernel.**~~ **✅ RESOLVED 2026-07-31.** Approved and
   actioned: `IApplicationDbContext` is now documented as **`docs/shared-contracts.md` §7 — Persistence
   access**, carrying the same shared-kernel status as `ICurrentUserService` (§2), the access policies
   (§3), and `IActivityLogService` (§6). The existing *Frontend conventions* section moved §7 → §8 (and
   later → §9 when *Shared metric definitions* was added as §8); this
   was safe because §1–§6 are cross-referenced by five specs, ADR-0006, and this plan, while **§7 had no
   external references**. The corresponding verification task also moved from Polish into **Foundational
   (T023)**, so the shared kernel is confirmed before any story consumes it — 002–006 can now depend on
   the interface directly instead of each re-deriving how a handler crosses the
   Application/Infrastructure boundary.
3. **`ITokenService` and `IActivityLogService` placement.** Both are already named in shared-contracts
   (§6) and spec 001 B.3 as shared abstractions; this plan puts their interfaces in
   `Application/Common/Interfaces/` and implementations in Infrastructure. Consistent with the shared-kernel
   rule — noted for confirmation, no action expected.
4. **Angular Material commitment is now binding.** III permits Bootstrap as an alternative but forbids
   mixing. Once 002–006 build on Material components, introducing Bootstrap would be a constitution
   violation requiring an amendment.

---

## Phase status

| Phase | Output | Status |
|---|---|---|
| Phase 0 — Outline & Research | `research.md` | ✅ Complete — 10 decisions, 0 unresolved |
| Phase 1 — Design & Contracts | `data-model.md`, `docs/contracts/auth.v1.yaml`, `quickstart.md` | ✅ Complete |
| Phase 2 — Task breakdown | `tasks.md` | ⏳ Run `/speckit.tasks` |
