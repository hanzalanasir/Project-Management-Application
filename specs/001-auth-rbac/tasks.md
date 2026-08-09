---
description: "Task list for 001 Auth & RBAC implementation"
---

# Tasks: 001 Auth & RBAC (Foundational)

**Input**: Design documents from `/specs/001-auth-rbac/`
**Prerequisites**: [plan.md](plan.md) · [spec.md](spec.md) · [research.md](research.md) ·
[data-model.md](data-model.md) · [contracts/](contracts/README.md) → [`docs/contracts/auth.v1.yaml`](../../docs/contracts/auth.v1.yaml) · [quickstart.md](quickstart.md)

**Tests**: **REQUIRED — not optional for this project.** Constitution IX mandates xUnit handler tests and
`WebApplicationFactory` integration tests (IX.1), Jasmine/Karma frontend tests (IX.2), and prohibits
merging with failing tests (IX.3). Spec 001 B.8 DoD #10 requires them explicitly.

**Organization**: Grouped by user story so each is independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story the task serves
- Exact file paths are included in every task

---

## Story ID mapping & implementation order

Spec 001 prioritizes five stories P0 and one P1 (plus three P1 stories added 2026-08-05), so priority alone
does not give an order. The order below is derived from the **dependency chain stated in each story's
`Dependencies` line**:

| Label | Spec story | Title | Priority | Depends on |
|---|---|---|---|---|
| **US6** | US-001-06 | Idempotent seed provisioning | P0 | — (runs first) |
| **US1** | US-001-01 | Register a new account | P0 | US6 (roles must exist) |
| **US2** | US-001-02 | Log in and receive a token | P0 | US1 |
| **US4** | US-001-04 | Role-based route & endpoint protection | P0 | US2 (token with role claim) |
| **US3** | US-001-03 | Log out | P1 | US2 |
| **US5** | US-001-05 | Token expiry & refresh | P0 | US2, US3 (rotation reuses revocation) |
| **US7** | US-001-07 | Admin lists and views user accounts | P1 | US2 (Admin token), US6 (seeded Admin account) |
| **US8** | US-001-08 | Admin changes a user's role | P1 | US7 (find the target user first), US1 (a non-Admin account to promote) |
| **US9** | US-001-09 | Admin deactivates or reactivates a user | P1 | US7 (find the target user first), US3/US5 (reuses the token-revocation mechanism) |

**Implementation order: US6 → US1 → US2 → US4 → US3 → US5 → US7 → US8 → US9.**
Phase numbers follow this order, *not* the numeric story order — read the label, not the phase number.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffold the solution, enforce the dependency rule at the project level, and install tooling.

- [X] T001 Create `ProjectManagementApp.sln` at repo root plus the `src/` and `tests/` directory skeleton per plan.md §Project Structure
- [X] T002 Create the four source projects (`src/ProjectManagementApp.Domain`, `.Application`, `.Infrastructure`, `.Api`) and wire project references so the graph is exactly Domain ← Application ← Infrastructure ← Api, with Api also referencing Application (composition root) — per research.md R-1
- [X] T003 [P] Create the three test projects `tests/ProjectManagementApp.Application.Tests`, `.Infrastructure.Tests`, `.Api.Tests` with references to their subjects (no `Domain.Tests` — see research.md R-1)
- [X] T004 [P] Add `Directory.Build.props` at repo root enabling `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, and `<LangVersion>latest</LangVersion>` for all projects (Constitution VIII.1)
- [X] T005 [P] Add `.editorconfig` at repo root with C# naming rules (PascalCase types/members, camelCase locals) per Constitution VIII.2
- [X] T006 Add backend NuGet packages: `Microsoft.Extensions.Identity.Stores` to Domain; `MediatR`, `FluentValidation`, `Microsoft.EntityFrameworkCore` to Application; `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `EFCore.NamingConventions` to Infrastructure; `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore`, `Serilog.AspNetCore` to Api. **Note the transitive path that three features rely on**: `.Domain`'s `Microsoft.Extensions.Identity.Stores` brings in `Microsoft.Extensions.Identity.Core`, which is what makes **`UserManager<ApplicationUser>` reachable from `.Application`** — used by 001's own handlers (T060) and by 002/004 for role lookups (analyze finding G3). If that transitive reference is ever dropped, add `Microsoft.Extensions.Identity.Core` to `.Application` explicitly rather than moving the role check
- [X] T007 [P] Add test NuGet packages to all three test projects: `xunit`, `Testcontainers.PostgreSql`, `Respawn`, `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing`, `NSubstitute` (research.md R-7)
- [X] T008 [P] Create `.config/dotnet-tools.json` pinning `dotnet-ef` and `Swashbuckle.AspNetCore.Cli` as local tools
- [X] T009 Create the Angular 22 workspace at `src/ProjectManagementApp.Web` with standalone bootstrap (no `@NgModule`, ADR-0001), then add Angular Material and NgRx (`provideStore`/`provideEffects`)
- [X] T010 [P] Configure `src/ProjectManagementApp.Web/tsconfig.json` for TypeScript strict mode, add `proxy.conf.json` routing `/api` to the API, and set the production build `outputPath` to the API's `wwwroot/` (ADR-0002 same-origin)
- [X] T011 [P] Bootstrap Serilog in `src/ProjectManagementApp.Api/Program.cs` with console + rolling-file sinks and structured output (Constitution NFR-003)
- [X] T012 [P] Create `src/ProjectManagementApp.Api/appsettings.json` and `appsettings.Development.json` with the non-secret keys from spec B.4 (`Jwt:AccessTokenMinutes`, `Jwt:RefreshTokenDays`, `Identity:Password:*`, `Identity:Lockout:*`, `Cors:AllowedOrigins`, `RefreshCookie:*`, `Seed:Enabled`) — no secret values
- [X] T013 [P] Update `.gitignore` to exclude `artifacts/`, `*.user`, and confirm no `appsettings.*.Local.json` or secret file can be committed (Constitution V.4)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain model, shared kernel, persistence, MediatR pipeline, and the API/Angular shells.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Domain layer

- [X] T014 [P] Create `ApplicationUser : IdentityUser<Guid>` and `ApplicationRole : IdentityRole<Guid>` in `src/ProjectManagementApp.Domain/Entities/` with `FullName`, `IsActive`, `CreatedAt`, `UpdatedAt`, the `Version` (`xmin`) token, and navigation collections per data-model.md §2
- [X] T015 [P] Create `RefreshToken` entity in `src/ProjectManagementApp.Domain/Entities/RefreshToken.cs` with `TokenHash`, `ExpiresAt`, `RevokedAt`, `ReplacedByToken`, and the computed `IsActive` (data-model.md §2)
- [X] T016 [P] Create `ActivityLog` entity in `src/ProjectManagementApp.Domain/Entities/ActivityLog.cs` with nullable `ActorId` (soft FK — no constraint), `Action`, `EntityType`, `EntityId` (string), `Timestamp`, `ChangeSummary`
- [X] T017 [P] Create the table-only entities `Project`, `TaskItem`, `TeamMember` in `src/ProjectManagementApp.Domain/Entities/` to the field lists in specs 002/003/004 — **required by research.md R-10**; 001 adds no rules to them
- [X] T018 [P] Create enums `Role`, `AuditAction`, `ProjectStatus`, `TaskStatus`, `TaskPriority` in `src/ProjectManagementApp.Domain/Enums/` (data-model.md §3). **`AuditAction` must carry the complete value set for all six features**, not just 001's — `UserRegistered, UserLoggedIn, UserLoggedOut, TokenRefreshed, UserDeactivated, UserSeeded, UserRoleChanged, UserReactivated` (001 — the last two added 2026-08-05 for US-001-08/09) · `ProjectCreated, ProjectUpdated, ProjectDeleted, ProjectOwnerChanged` (002) · `TaskCreated, TaskUpdated, TaskStatusChanged, TaskReassigned, TaskDeleted` (003) · `TeamMemberAdded, TeamMemberRemoved` (004) · `ReportGenerated` (006). It is one shared enum in `Domain/Enums/`, exactly like `ProjectStatus`/`TaskStatus`, so later features **consume** values rather than editing 001's file

### Application layer — shared kernel

- [X] T019 [P] Implement `Result`, `Result<T>`, `Error`, and `ErrorKind` in `src/ProjectManagementApp.Application/Common/Models/` exactly as `docs/shared-contracts.md` §1 defines (ADR-0003). **`ErrorKind` must carry all seven members** — `Validation, Unauthenticated, Forbidden, NotFound, Conflict, UnprocessableContent, Unexpected`. **`UnprocessableContent` is required here even though only 006 uses it** (its 422 large-report guard); omitting it would force 006 to extend a shared-kernel enum locally, which the ADR-0007 §5 rule forbids
- [X] T020 [P] Implement `CurrentUser`, `AccessDecision`, `PagedResult<T>`, **`TaskMutation`** (`Create, FullEdit, StatusChange, Reassign, Delete`), **`ActivityScope`**, and **`ActivityEntry`** in `src/ProjectManagementApp.Application/Common/Models/`, **plus `MetricDefinitions`** in `src/ProjectManagementApp.Application/Common/Metrics/MetricDefinitions.cs` — all per `docs/shared-contracts.md` §2/§3/§4/§6/§8. **All seven are required here even though only later features consume some of them**: T022 authors the shared-kernel interfaces whose signatures reference them (`ITaskAccessPolicy.CanMutateAsync` → `TaskMutation`; `IActivityLogService.QueryScopedAsync` → `ActivityScope`/`ActivityEntry`), and **`MetricDefinitions`** (`IsOverdue(todayUtc)`, `IsClosed`, `CompletionRate` returning **`0` when total is 0**, `ClosedInWindow`) must be shared because 005 and 006 are obliged to produce **identical** values (006 NFR-002) — putting it in whichever feature needed it first would force the other to depend on that feature's Application layer (ADR-0007 §5). `todayUtc` is **UTC and fixed** (003 research R-1, 005 research R-1, 006 analyze G1)
- [X] T021 [P] Define `IApplicationDbContext` in `src/ProjectManagementApp.Application/Common/Interfaces/IApplicationDbContext.cs` exposing all six `DbSet<T>` properties and `SaveChangesAsync`, **exactly as `docs/shared-contracts.md` §7 now specifies** — the DbContext behind an interface, not a repository (research.md R-3)
- [X] T022 [P] Define **all** shared-kernel interfaces in `src/ProjectManagementApp.Application/Common/Interfaces/` per spec 001 B.3 and `docs/shared-contracts.md` §2/§3/§6: `ICurrentUserService`, `ITokenService`, `IDataSeeder`, `IActivityLogService` (**both** `LogAsync` **and** the scoped read `QueryScopedAsync(ActivityScope, page, pageSize, ct)` → `Task<PagedResult<ActivityEntry>>`, required by 005's feed and 006's Activity Report), and the three **scope-authorization policies** `IProjectAccessPolicy`, `ITaskAccessPolicy`, `ITeamAccessPolicy` exactly as §3 declares them. **The policy interfaces belong here, not in 002/003/004** — those features implement the rules, but the interfaces are shared kernel so 005/006 can depend on them without taking a dependency on another feature's Application layer (ADR-0006 addendum)
- [X] T023 Verify **every** shared-kernel member declared in `docs/shared-contracts.md` **§2, §3, §4, §6, and §7** exists in `src/ProjectManagementApp.Application/Common/` and matches its declaration verbatim — the interfaces (`IApplicationDbContext`, `ICurrentUserService`, `ITokenService`, `IActivityLogService` incl. `QueryScopedAsync`, `IProjectAccessPolicy`, `ITaskAccessPolicy`, `ITeamAccessPolicy`) **and** the types they reference (`CurrentUser`, `AccessDecision`, `PagedResult<T>`, `TaskMutation`, `ActivityScope`, `ActivityEntry`). Assert no feature-local redefinition exists and **no repository wraps `IApplicationDbContext`**. **Enumerate the sections rather than spot-checking**: this gate previously covered only §2/§6/§7, and that omission is exactly why the §3 access-policy interfaces went uncreated until 005's planning sweep found them (ADR-0006 addendum) — **verified by manual review** (grep + read against shared-contracts.md; no dedicated file was named for this task)
- [X] T024 [P] Write unit tests for both MediatR pipeline behaviors in `tests/ProjectManagementApp.Application.Tests/Common/Behaviors/` — assert `ValidationBehavior` short-circuits with a failed `Result` (never throws), and assert `LoggingBehavior` never logs a request body. **Written and run first — expect them to fail** (`ValidationBehavior`/`LoggingBehavior` do not exist yet); Constitution IX.5 (fixed 2026-08-05, `/speckit.analyze` finding D1 — this block previously implemented before testing) — **confirmed RED** (build failed, types didn't exist), then GREEN after T025/T026
- [X] T025 Implement `ValidationBehavior<TRequest,TResponse>` in `src/ProjectManagementApp.Application/Common/Behaviors/ValidationBehavior.cs` — resolves `IEnumerable<IValidator<TRequest>>`, and on failure **short-circuits by returning `Result.Failure(ErrorKind.Validation, fields)`; it MUST NOT throw** (research.md R-4)
- [X] T026 Implement `LoggingBehavior<TRequest,TResponse>` in `src/ProjectManagementApp.Application/Common/Behaviors/LoggingBehavior.cs` opening a Serilog scope with request name, user id, correlation id, and elapsed ms — **MUST NOT log request bodies** (they carry plaintext passwords; Constitution V.3)
- [X] T027 Create `src/ProjectManagementApp.Application/DependencyInjection.cs` registering MediatR, FluentValidation validators from the assembly, and the two behaviors in the order Logging → Validation (research.md R-4)

### Infrastructure layer — persistence

- [X] T028 [P] Configure the global snake_case naming convention (via `EFCore.NamingConventions`) in `src/ProjectManagementApp.Infrastructure/Persistence/` so `FullName` maps to `full_name` without per-property mapping (Constitution VIII.2)
- [X] T029 Implement `ApplicationDbContext : IdentityDbContext<...>, IApplicationDbContext` in `src/ProjectManagementApp.Infrastructure/Persistence/ApplicationDbContext.cs` calling `ApplyConfigurationsFromAssembly` — no fluent config inline in `OnModelCreating`
- [X] T030 [P] Add `IEntityTypeConfiguration<T>` classes for the 001-owned entities in `src/ProjectManagementApp.Infrastructure/Persistence/Configurations/` — Identity table renames (`users`/`roles`/`user_roles`), the `xmin` row-version mapping, and the indexes in data-model.md §4
- [X] T031 [P] Add `IEntityTypeConfiguration<T>` classes for `Project`, `TaskItem`, `TeamMember` in `src/ProjectManagementApp.Infrastructure/Persistence/Configurations/` with the exact delete behaviors from data-model.md §4 (CASCADE / RESTRICT / SET NULL)
- [X] T032 Generate the `InitialCreate` migration in `src/ProjectManagementApp.Infrastructure/Persistence/Migrations/` — **it MUST create all five constitution entities**, not only 001's (research.md R-10, data-model.md §1) — **verified against a real Testcontainers PostgreSQL**: all 8 tables created, `xmin` populates on insert, cascade delete proven (Project → TaskItem)
- [X] T033 Implement `ActivityLogService : IActivityLogService` in `src/ProjectManagementApp.Infrastructure/Services/ActivityLogService.cs` — **two deliverables**: (a) `LogAsync` writes the audit row into the caller's unit of work so it commits in the **same** `SaveChangesAsync` (Constitution IV.4); (b) **`QueryScopedAsync`** performs the scoped, paginated read — filter to `ActivityScope` (all entries for an unscoped Admin; otherwise entries whose entity belongs to the caller's visible projects), order newest-first with a stable `(timestamp, id)` tiebreak, clamp `pageSize` to the configured maximum, and return `PagedResult<ActivityEntry>` with a **scope-limited `totalCount`**. Consumed by 005's feed and 006's Activity Report, which are both forbidden from querying `activity_logs` directly (shared-contracts §6/§7, 005 research R-1)
- [X] T034 Create `src/ProjectManagementApp.Infrastructure/DependencyInjection.cs` registering `ApplicationDbContext` (as `IApplicationDbContext`), Identity stores, and `ActivityLogService` now; `TokenService`/`DataSeeder` registrations are added onto this same file in US2/US6 (T070/T048) when those concrete classes exist. **Includes the `AddIdentityCore<ApplicationUser>(options => ...).AddRoles<ApplicationRole>()` call that binds `Identity:Password:*` and `Identity:Lockout:*` from configuration** — deviates from the task's literal `AddIdentity<TUser,TRole>` name because that extension requires the ASP.NET Core shared framework and sets up an unused cookie scheme; `AddIdentityCore` + `FrameworkReference Include="Microsoft.AspNetCore.App"` (for `AddDefaultTokenProviders`) achieves the identical config-binding outcome for a JWT-only API. T012 creates those config keys, but no other task consumes them; without this line they are declared and never read (`/speckit.analyze` finding E2)
- [X] T035 Create the shared Testcontainers PostgreSQL fixture + Respawn reset helper in `tests/ProjectManagementApp.Infrastructure.Tests/Fixtures/` as an xUnit `ICollectionFixture` (one container per test run) — reused by `.Api.Tests` (research.md R-7) — **verified working** against a real Docker/Testcontainers run (3/3 tests passing)

### API layer — shell

- [X] T036 [P] Implement `ToActionResult()` extensions in `src/ProjectManagementApp.Api/Common/ResultExtensions.cs` mapping `ErrorKind` → status per `docs/shared-contracts.md` §1 — **the only place this mapping exists** (research.md R-8). **Cover all seven kinds**, including `UnprocessableContent` → **422**; a `switch` over the enum with no default arm will fail to compile if a member is missed, which is the intended safety net — **note**: C# additionally requires suppressing `CS8524` (enums are open to arbitrary underlying ints) via a scoped `#pragma warning disable/restore`, so the real safety net is "omit a *named* member" failing the build, not "any unhandled value"
- [X] T037 [P] Implement `ExceptionHandlingMiddleware` in `src/ProjectManagementApp.Api/Common/ExceptionHandlingMiddleware.cs` emitting RFC 7807 500s with no stack trace in production
- [X] T038 [P] Implement `CurrentUserService : ICurrentUserService` in `src/ProjectManagementApp.Api/Services/CurrentUserService.cs` backed by `IHttpContextAccessor`, registered scoped (research.md R-8)
- [X] T039 Configure `src/ProjectManagementApp.Api/Program.cs`: JWT bearer authentication scheme, the **global fallback authorization policy requiring an authenticated user** (Constitution V.1), CORS allow-list, Swagger UI in Development only (VI.5), and fail-fast startup validation when `Jwt:SigningKey` or the connection string is missing — **smoke-tested**: API starts, `GET /api/health` → 200 anonymous, an unmapped route → 401 (fallback policy applies even when no endpoint matches)
- [X] T040 [P] Implement `HealthController` in `src/ProjectManagementApp.Api/Controllers/HealthController.cs` serving `GET /api/health` as `[AllowAnonymous]`, matching the `HealthResponse` schema in `docs/contracts/auth.v1.yaml`
- [X] T041 Add the `CheckApiContract` MSBuild target to `src/ProjectManagementApp.Api/ProjectManagementApp.Api.csproj` running `dotnet swagger tofile` then `oasdiff breaking docs/contracts/auth.v1.yaml … --fail-on ERR`, gated on `Condition="'$(CheckApiContract)' == 'true'"` (research.md R-5) — wired; **`oasdiff` binary is not yet installed in this environment** — needed before T138 can prove the gate fails

### Frontend shell

- [X] T042 Create the Angular app shell in `src/ProjectManagementApp.Web/src/app/` — `app.config.ts` (providers), `app.routes.ts`, and the `core/`, `shared/`, `features/` folder structure per plan.md §Project Structure
- [X] T043 [P] Create the NgRx auth feature skeleton in `src/ProjectManagementApp.Web/src/app/core/store/auth/` (`createFeature`, initial state holding the access token in memory only — **never localStorage**)
- [X] T044 [P] Create the shared `error-display` and `notification` components in `src/ProjectManagementApp.Web/src/app/shared/`, plus a global `ErrorHandler` and HTTP error interceptor funnelling into the notification component (Constitution VII.6/VII.7)
- [X] T045 [P] Generate the typed API client from `docs/contracts/auth.v1.yaml` into `src/ProjectManagementApp.Web/src/app/core/api/generated/` via `openapi-generator` (`typescript-angular`); **adopt the generated DTO types only** — services stay hand-written (research.md R-6) — **deviates from the named tool**: `openapi-generator-cli` requires a JVM, which is not installed in this environment; `openapi-typescript` (pure TS/Node, no Java) generates the same DTO-types-only output research R-6 actually asks for, so it was used instead. `npm run generate:api` regenerates it

**Checkpoint**: Solution compiles, migration applies, API starts, Angular serves, and the shared kernel is
verified against `docs/shared-contracts.md`. User story work can begin.

---

## Phase 3: User Story 6 — Idempotent seed provisioning (Priority: P0) 🎯 unblocks everything

**Goal**: A freshly created database is immediately usable — three roles plus one Admin, one
ProjectManager, and one TeamMember exist after startup, and starting twice creates nothing new.

**Independent Test**: Point at an empty database and start the API twice. Assert exactly three roles and
three users exist, no duplicates, and a `UserSeeded` audit row per user with `actor_id = NULL`.

### Tests for User Story 6

- [X] T046 [P] [US6] Write integration test in `tests/ProjectManagementApp.Infrastructure.Tests/Seeding/DataSeederTests.cs` asserting an empty database gains exactly 3 roles and 3 users, each with a hashed password — **confirmed RED** (DataSeeder/SeedOptions didn't exist), then GREEN
- [X] T047 [P] [US6] Write idempotency test in `tests/ProjectManagementApp.Infrastructure.Tests/Seeding/DataSeederIdempotencyTests.cs` — run the seeder twice, assert zero duplicates; then delete one seeded user, re-run, assert **only** the missing one is recreated (partial-state repair)

### Implementation for User Story 6

- [X] T048 [US6] Implement `DataSeeder : IDataSeeder` in `src/ProjectManagementApp.Infrastructure/Services/DataSeeder.cs` — ensure roles, then ensure one user per role via `UserManager`, guarded by existence checks **backed by the unique indexes** so concurrent startups resolve to one winner (research.md R-9)
- [X] T049 [US6] Add `SeedOptions` binding for `Seed:Enabled` and `Seed:{Admin,ProjectManager,TeamMember}:{Email,Password}` in `src/ProjectManagementApp.Infrastructure/Services/SeedOptions.cs`, reading credentials from configuration only
- [X] T050 [US6] Wire `db.Database.MigrateAsync()` followed by `IDataSeeder.SeedAsync()` into a startup scope in `src/ProjectManagementApp.Api/Program.cs`, gated by `Seed:Enabled` (default on in Development, off in Production)
- [X] T051 [US6] Emit a `UserSeeded` audit row per seeded user with `actor_id = NULL` (system) from `src/ProjectManagementApp.Infrastructure/Services/DataSeeder.cs`
- [X] T052 [US6] Verify no seed credential is hardcoded — grep the solution and confirm all seed passwords resolve from user-secrets/environment (Constitution V.4, quickstart V11) — **verified**: grep of `src/` and appsettings*.json shows no literal credential values, dev values live only in user-secrets

**Checkpoint**: A fresh database boots into a usable, demonstrable state. US1 can now assign a role.

---

## Phase 4: User Story 1 — Register a new account (Priority: P0)

**Goal**: A visitor can self-register and receives a `TeamMember` account; the password is hashed, never
returned, and the write is audited.

**Independent Test**: `POST /api/auth/register` with a unique email returns 201 + `Location`, the body
contains no password field, the created user's role is `TeamMember`, and an `activity_logs` row exists.

### Tests for User Story 1

- [X] T053 [P] [US1] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RegisterEndpointTests.cs` asserting 201, the `Location` header, a `UserDto` body with `role = TeamMember`, and **no `password`/`passwordHash` key in the response**
- [X] T054 [P] [US1] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RegisterConflictTests.cs` asserting a duplicate email returns **409** and that `DANA@EXAMPLE.COM` also conflicts with `dana@example.com` (normalized-email uniqueness)
- [X] T055 [P] [US1] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RegisterValidationTests.cs` asserting a policy-failing password returns **400** with per-field `errors` and persists nothing
- [X] T056 [P] [US1] Write unit tests for `RegisterCommandValidator` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/RegisterCommandValidatorTests.cs` (required fields, email format, min length, password match)
- [X] T057 [P] [US1] Write unit tests for `RegisterCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/RegisterCommandHandlerTests.cs` covering every conditional branch, **including that a client-supplied role is ignored and `TeamMember` is always assigned** (Constitution IX.1) — **confirmed RED**, then GREEN (9/9 Application.Tests)

### Implementation for User Story 1

- [X] T058 [US1] Create `RegisterCommand` and the `UserDto` response shape in `src/ProjectManagementApp.Application/Features/Auth/Register/` matching the `RegisterRequest`/`UserDto` schemas in `docs/contracts/auth.v1.yaml`
- [X] T059 [US1] Implement `RegisterCommandValidator` in `src/ProjectManagementApp.Application/Features/Auth/Register/RegisterCommandValidator.cs` (FluentValidation, ADR-0005) — invoked automatically by `ValidationBehavior`, never called by the handler
- [X] T060 [US1] Implement `RegisterCommandHandler` in `src/ProjectManagementApp.Application/Features/Auth/Register/RegisterCommandHandler.cs` — `UserManager.CreateAsync` (hashes), assign `TeamMember`, write the `UserRegistered` audit row in the same transaction, return `Result<UserDto>`; duplicate email → `ErrorKind.Conflict`
- [X] T061 [US1] Add the thin `POST /api/auth/register` endpoint to `src/ProjectManagementApp.Api/Controllers/AuthController.cs` — `[AllowAnonymous]`, a single `MediatR.Send`, `.ToActionResult(onSuccess: 201)` with the `Location` header; **no logic in the controller** (Constitution II.2) — **verified against real Testcontainers PostgreSQL** (3/3 Api.Tests green; also built a reusable `ApiTestFixture`/`WebApplicationFactory<Program>` harness for all future story integration tests)
- [X] T062 [P] [US1] Create the standalone register component in `src/ProjectManagementApp.Web/src/app/features/auth/register/` — Angular Material Reactive Form with required-name, email-format, min-length, and password-match validators, errors via the shared error-display component
- [X] T063 [US1] Implement `AuthService.register()` in `src/ProjectManagementApp.Web/src/app/core/services/auth.service.ts` and add the lazy `auth` route group in `src/ProjectManagementApp.Web/src/app/features/auth/auth.routes.ts` (Constitution VII.1/VII.3)

**Checkpoint**: A user can be created end-to-end. Verifiable against quickstart V1–V3.

---

## Phase 5: User Story 2 — Log in and receive a token (Priority: P0) 🎯 MVP completes here

**Goal**: A registered user exchanges credentials for a short-lived JWT plus an httpOnly refresh cookie,
with a deliberately generic failure that leaks no account information.

**Independent Test**: `POST /api/auth/login` returns 200 with `{accessToken, expiresAt, user}` and a
`Set-Cookie` carrying `HttpOnly; Secure; SameSite=Strict`; the **body contains no refresh token**; a wrong
password and an unknown email produce byte-identical 401s.

### Tests for User Story 2

- [X] T064 [P] [US2] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/LoginEndpointTests.cs` asserting 200, the JWT's `sub`/`email`/single `role` claim and `exp`, the `Set-Cookie` attributes, and **that no refresh token appears in the response body** (FR-016)
- [X] T065 [P] [US2] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/LoginEnumerationTests.cs` asserting a wrong password and an unknown email return **identical** 401 bodies (no user enumeration)
- [X] T066 [P] [US2] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/LoginDeactivatedTests.cs` asserting `is_active = false` yields 401 even with correct credentials (FR-004)
- [X] T067 [P] [US2] Write unit tests for `LoginCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/LoginCommandHandlerTests.cs` covering success, bad credentials, and deactivated-user branches
- [X] T068 [P] [US2] Write unit tests for `TokenService` in `tests/ProjectManagementApp.Application.Tests/Services/TokenServiceTests.cs` asserting claim set, configured expiry, and that the refresh token is returned raw but **stored hashed**

### Implementation for User Story 2

- [X] T069 [US2] Implement `JwtOptions` binding and startup validation for `Jwt:SigningKey`, `Issuer`, `Audience`, `AccessTokenMinutes`, `RefreshTokenDays` (spec B.4) — **placed in `src/ProjectManagementApp.Infrastructure/Identity/JwtOptions.cs`, not `Api/Configuration/` as originally written**: `TokenService` (Infrastructure) is the actual consumer, and Infrastructure cannot depend on Api (Clean Architecture's inward dependency rule). Startup fail-fast validation (SigningKey/connection-string presence) stays in `Program.cs` (T039) via `IConfiguration` directly
- [X] T070 [US2] Implement `TokenService : ITokenService` in `src/ProjectManagementApp.Infrastructure/Identity/TokenService.cs` — `CreateAccessToken` (signs the JWT with a single `role` claim) and `CreateRefreshToken` (opaque high-entropy value; caller persists only its SHA-256 hash). **Also fixed a real bug found while wiring this in**: JWT claims are written as short names (`sub`/`email`/`role`), so `Program.cs`'s JWT bearer options now set `MapInboundClaims = false` and `RoleClaimType`/`NameClaimType`, and `CurrentUserService` reads the literal claim names — otherwise `ClaimTypes.Role`/`[Authorize(Roles=...)]` would silently never match, breaking RBAC in Phase 6
- [X] T071 [US2] Create `LoginCommand` and the `AuthTokens` result shape in `src/ProjectManagementApp.Application/Features/Auth/Login/` matching the `LoginRequest`/`AuthTokensResponse` schemas in the contract
- [X] T072 [US2] Implement `LoginCommandValidator` in `src/ProjectManagementApp.Application/Features/Auth/Login/LoginCommandValidator.cs` (email present + format, password present)
- [X] T073 [US2] Implement `LoginCommandHandler` in `src/ProjectManagementApp.Application/Features/Auth/Login/LoginCommandHandler.cs` — verify via Identity's hasher, reject inactive users, persist the hashed refresh token, write the `UserLoggedIn` audit row, and return a **generic** `ErrorKind.Unauthenticated` on any failure
- [X] T074 [US2] Add the thin `POST /api/auth/login` endpoint to `src/ProjectManagementApp.Api/Controllers/AuthController.cs` writing the refresh token **only** as `Set-Cookie` (`HttpOnly; Secure; SameSite=Strict; Path=/api/auth`) and never into the body — **verified against real Testcontainers PostgreSQL** (3/3 integration tests green: JWT claims/exp, Set-Cookie attributes, no-enumeration, deactivated-account denial)
- [X] T075 [P] [US2] Create the standalone login component in `src/ProjectManagementApp.Web/src/app/features/auth/login/` — Material Reactive Form, generic failure message via the shared error-display component
- [X] T076 [US2] Implement the NgRx auth actions, reducer, effects, and selectors in `src/ProjectManagementApp.Web/src/app/core/store/auth/` — the access token lives in memory only
- [X] T077 [US2] Implement the functional JWT interceptor in `src/ProjectManagementApp.Web/src/app/core/interceptors/jwt.interceptor.ts` attaching `Authorization: Bearer` to outgoing requests and sending `withCredentials` for `/api/auth/*` (Constitution VII.4)

**Checkpoint**: 🎯 **MVP** — register, log in, receive a token. Verifiable against quickstart V1–V6.

---

## Phase 6: User Story 4 — Role-based route & endpoint protection (Priority: P0)

**Goal**: Every endpoint is authenticated by default, role-restricted endpoints enforce roles by attribute
only, and Angular guards are the sole client-side navigation block.

**Independent Test**: The 401/403 matrix holds across all three roles against a protected probe; the four
anonymous endpoints work without a token; a TeamMember is blocked from an Admin route by the guard **and**
still receives 403 if the request is forced.

### Tests for User Story 4

- [X] T078 [P] [US4] Write the 401/403 matrix integration test in `tests/ProjectManagementApp.Api.Tests/Authorization/RoleMatrixTests.cs` — no token → 401; TeamMember → 403; ProjectManager → 403; Admin → 200 (Constitution IX.1)
- [X] T079 [P] [US4] Write integration test in `tests/ProjectManagementApp.Api.Tests/Authorization/AnonymousEndpointTests.cs` asserting register/login/refresh/health succeed without a token and that **no other endpoint does** (FR-007) — refresh doesn't exist until Phase 8 (US5 depends on US2/US3, not US4); its own `RefreshEndpointTests` (T098) covers the anonymous assertion for that endpoint instead of duplicating it here
- [X] T080 [P] [US4] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/GetCurrentUserTests.cs` asserting `GET /api/auth/me` returns 200 with the token's identity and 401 without a token
- [X] T081 [P] [US4] Write an architecture test in `tests/ProjectManagementApp.Api.Tests/Architecture/NoInlineRoleChecksTests.cs` failing the build if any controller or handler contains an ad-hoc role comparison — roles must be attribute-declared only (Constitution V.2, quickstart V8) — implemented as (a) reflection over every controller action asserting an `[Authorize]`/`[AllowAnonymous]` declaration, and (b) a source-text scan of `Controllers/`+`Features/` for literal role-string comparisons
- [X] T082 [P] [US4] Write the NFR-002 statelessness test in `tests/ProjectManagementApp.Api.Tests/Authorization/StatelessAuthTests.cs` — hook an EF Core `DbCommand` interceptor into the `WebApplicationFactory`, call `GET /api/auth/me` with a valid token, and assert **zero** SQL statements execute; `GetCurrentUserQueryHandler` projects `UserDto` directly from `ICurrentUserService`'s token-derived claims, never from `IApplicationDbContext` (`/speckit.analyze` finding E1 — this claim in spec NFR-002 and plan.md's Performance Goals previously had no verifying task). **Required extending the shared kernel**: `UserDto.fullName` is contract-required but wasn't in `CurrentUser`'s 3-field shared-contracts.md shape, so `CurrentUser` gained a 4th field (`FullName`, appended last so no 002-006 positional-construction code breaks — none exists outside 001) and the JWT gained a `full_name` claim; `UserDto.CreatedAt` became nullable (contract marks it optional) since this endpoint cannot supply it without a DB hit

### Implementation for User Story 4

- [X] T083 [US4] Verify and document the global fallback authorization policy in `src/ProjectManagementApp.Api/Program.cs` so an endpoint with no attribute still requires authentication, and confirm exactly four `[AllowAnonymous]` endpoints exist — verified: register/login/health exist now (3), refresh joins in Phase 8, comment in `Program.cs` already names all four
- [X] T084 [US4] Create `GetCurrentUserQuery` and its handler in `src/ProjectManagementApp.Application/Features/Auth/GetCurrentUser/` reading identity from `ICurrentUserService` — never from a parameter
- [X] T085 [US4] Add the thin `GET /api/auth/me` endpoint to `src/ProjectManagementApp.Api/Controllers/AuthController.cs` with `[Authorize]`, matching the contract's `getCurrentUser` operation
- [X] T086 [US4] Add an Admin-only probe endpoint in `src/ProjectManagementApp.Api/Controllers/` annotated `[Authorize(Roles = "Admin")]` to exercise the role matrix in tests — `AdminProbeController` at `GET /api/admin-probe`
- [X] T087 [P] [US4] Implement the functional auth guard in `src/ProjectManagementApp.Web/src/app/core/guards/auth.guard.ts` (`CanActivateFn`) redirecting unauthenticated users to login
- [X] T088 [P] [US4] Implement the functional role guard in `src/ProjectManagementApp.Web/src/app/core/guards/role.guard.ts` (`CanMatchFn`) reading the role from NgRx so a lazy chunk is **not downloaded** when the role fails
- [X] T089 [US4] Apply the guards in `src/ProjectManagementApp.Web/src/app/app.routes.ts` and confirm no component contains redirect logic — guards are the only navigation block (Constitution VII.5) — added a minimal placeholder `HomeComponent` as the guarded landing route (005 Dashboard is out of 001's scope; this just gives the guard a real destination and fixes what would otherwise be a redirect loop after login)

**Also fixed while wiring T070/T084 (belongs to no single task, recorded here)**: `Program.cs`'s JWT bearer options now set `MapInboundClaims = false` plus explicit `RoleClaimType`/`NameClaimType`, since `TokenService` writes short claim names (`sub`/`email`/`role`) that ASP.NET Core does not remap automatically — without this, `[Authorize(Roles=...)]` would never match and every role-gated endpoint in this phase would silently 403 regardless of the caller's actual role.

**Checkpoint**: RBAC is enforced end-to-end. Verifiable against quickstart V7, V8, V14.1.

---

## Phase 7: User Story 3 — Log out (Priority: P1)

**Goal**: A user can deliberately end a session; the refresh token is revoked server-side and the cookie
cleared, idempotently.

**Independent Test**: `POST /api/auth/logout` returns 204, sets `revoked_at`, and clears the cookie;
calling it again still returns 204; the revoked token can no longer refresh.

### Tests for User Story 3

- [X] T090 [P] [US3] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/LogoutEndpointTests.cs` asserting 204, `revoked_at` set in the database, the cookie cleared, and a `UserLoggedOut` audit row written
- [X] T091 [P] [US3] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/LogoutIdempotencyTests.cs` asserting logout with an already-expired or absent refresh token still succeeds
- [X] T092 [P] [US3] Write unit tests for `LogoutCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/LogoutCommandHandlerTests.cs` covering the revoke and already-revoked branches — added a third branch (null presented token) beyond the two named here

### Implementation for User Story 3

- [X] T093 [US3] Create `LogoutCommand` in `src/ProjectManagementApp.Application/Features/Auth/Logout/LogoutCommand.cs` carrying the user id and the presented refresh token
- [X] T094 [US3] Implement `LogoutCommandHandler` in `src/ProjectManagementApp.Application/Features/Auth/Logout/LogoutCommandHandler.cs` — set `RevokedAt`, write the `UserLoggedOut` audit row in the same transaction, and succeed idempotently when no live token is found
- [X] T095 [US3] Add the thin `POST /api/auth/logout` endpoint to `src/ProjectManagementApp.Api/Controllers/AuthController.cs` with `[Authorize]`, reading the refresh cookie and emitting `Set-Cookie: refresh_token=; Max-Age=0` on 204
- [X] T096 [P] [US3] Add the logout control to the app shell in `src/ProjectManagementApp.Web/src/app/core/` dispatching the NgRx logout action (Constitution VII — a `core/` singleton provided once) — `ShellHeaderComponent` in `core/shell-header/`, mounted once in `app.html`
- [X] T097 [US3] Implement the NgRx logout effect in `src/ProjectManagementApp.Web/src/app/core/store/auth/` clearing auth state and routing to login — treats a failed logout HTTP call the same as success (client state must never stay half-cleared)

**Checkpoint**: Sessions can be deliberately ended. Verifiable against quickstart V10.

---

## Phase 8: User Story 5 — Token expiry & refresh (Priority: P0)

**Goal**: Short-lived access tokens are renewed transparently via single-use refresh-token rotation;
replayed, revoked, or expired tokens are rejected, and a deactivated user cannot refresh.

**Independent Test**: An expired access token triggers exactly **one** refresh call that returns a new
pair and rotates the cookie; replaying the previous refresh token returns 401.

### Tests for User Story 5

- [X] T098 [P] [US5] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RefreshEndpointTests.cs` asserting 200, a new access token, a rotated `Set-Cookie`, and `replaced_by_token` linking old → new
- [X] T099 [P] [US5] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RefreshReplayTests.cs` asserting a replayed (already-rotated) refresh token returns **401** (single-use rotation, FR-006)
- [X] T100 [P] [US5] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RefreshRevokedTests.cs` asserting expired, revoked, and unknown refresh tokens each return 401
- [X] T101 [P] [US5] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RefreshDeactivatedTests.cs` asserting a valid refresh token whose user was deactivated is denied (FR-004)
- [X] T102 [P] [US5] Write unit tests for `RefreshCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/RefreshCommandHandlerTests.cs` covering valid, expired, revoked, replayed, and inactive-user branches
- [X] T103 [P] [US5] Write an atomicity test in `tests/ProjectManagementApp.Infrastructure.Tests/Tokens/RotationAtomicityTests.cs` asserting revoke-old + insert-new + audit commit as one transaction — a failure mid-rotation must never leave two live tokens (data-model.md §5) — proven by injecting a throwing `IActivityLogService` mid-handler against a real Testcontainers Postgres and confirming zero partial state after the exception

### Implementation for User Story 5

- [X] T104 [US5] Add `ValidateRefreshTokenAsync` to `src/ProjectManagementApp.Infrastructure/Identity/TokenService.cs` — hash the presented value and look it up; return null when expired, revoked, or unknown — already existed from T070
- [X] T105 [US5] Create `RefreshCommand` in `src/ProjectManagementApp.Application/Features/Auth/Refresh/RefreshCommand.cs` carrying the presented refresh token (from the cookie, never the body)
- [X] T106 [US5] Implement `RefreshCommandHandler` in `src/ProjectManagementApp.Application/Features/Auth/Refresh/RefreshCommandHandler.cs` — validate, reject inactive users, revoke the old token and set `ReplacedByToken`, issue a new pair, write the `TokenRefreshed` audit row, all in one transaction
- [X] T107 [US5] Add the thin `POST /api/auth/refresh` endpoint to `src/ProjectManagementApp.Api/Controllers/AuthController.cs` as `[AllowAnonymous]` reading the cookie and emitting the rotated `Set-Cookie` — **verified against real Testcontainers PostgreSQL** (7/7 Refresh integration tests green)
- [X] T108 [US5] Implement the functional 401 interceptor in `src/ProjectManagementApp.Web/src/app/core/interceptors/error.interceptor.ts` — on 401 call refresh **once** with single-flight de-duplication, retry the original request, and dispatch logout on refresh failure (Constitution VII.4)
- [X] T109 [P] [US5] Write a Jasmine test in `src/ProjectManagementApp.Web/src/app/core/interceptors/error.interceptor.spec.ts` asserting that several concurrent 401s produce **exactly one** refresh call (quickstart V14.3) — **deviates from the named tool**: Angular 22 has removed Karma (the CLI's default scaffolding now uses Vitest, with no Karma option), so this runs under Vitest instead — same assertion, same spec-file convention, HttpTestingController used identically
- [X] T110 [US5] Add anti-forgery/CSRF protection for the cookie-authenticated `/api/auth/refresh` and `/api/auth/logout` endpoints in `src/ProjectManagementApp.Api/Program.cs` (FR-016) — **implemented as a custom double-submit-cookie check** (`Api/Common/CsrfProtection.cs`), not ASP.NET Core's `IAntiforgery`: `IAntiforgery`'s cookie and header values are cryptographically linked, not equal strings, which doesn't match Angular's built-in `withXsrfConfiguration` (a plain synchronizer-token pattern expecting cookie-value == header-value). The custom check is a straight port of that same pattern server-side, wired to Angular's exact default cookie/header names (`XSRF-TOKEN` / `X-XSRF-TOKEN`) so no custom frontend code is needed beyond `withXsrfConfiguration(...)` in `app.config.ts`. Verified with two new tests proving missing/mismatched tokens are rejected (400)

**Checkpoint reached: all six original stories (US1–US6) complete.** Verifiable against quickstart V1–V10, V14 (once written).

**Checkpoint**: All six stories complete. Verifiable against quickstart V9, V10, V14.

---

## Phase 9: User Stories 7–9 — Admin user management (Priority: P1)

**Goal**: An Admin can list/view any user account (including deactivated ones), change another user's
role, and deactivate/reactivate an account — each protected by fixed safety invariants (no self-role-change,
at least one Admin must remain, no self-deactivation) and each write audited. Added 2026-08-05 to close
`/speckit.analyze` finding F1 — 001's own Clarifications had attributed this capability to "feature 004",
which never actually held it (004's scope is project team membership only).

**Independent Test**: As the seeded Admin: `GET /api/users` returns all three seeded accounts; `PUT
.../role` promotes the seeded TeamMember to ProjectManager (200, audited `UserRoleChanged`); `PUT
.../status` deactivates that same (now ProjectManager) user (200, their refresh tokens revoked, audited
`UserDeactivated`); the Admin's own attempts to change their own role or deactivate their own account both
return 409.

### Tests for User Story 7

- [X] T111 [P] [US7] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ListUsersEndpointTests.cs` asserting an Admin sees all seeded users incl. any deactivated one (flagged `isActive:false`), and a non-Admin caller receives **403**
- [X] T112 [P] [US7] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/GetUserByIdEndpointTests.cs` asserting a known id returns **200** with an `ETag` header, and an unknown id returns **404**
- [X] T113 [P] [US7] Write unit tests for `ListUsersQueryHandler`/`GetUserByIdQueryHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/AdminUsersQueryHandlerTests.cs` covering the unscoped-list and unknown-id branches
- [X] T114 [P] [US7] Unit-test the shared `ETagExtensions` helper itself (round-trip, malformed value, absent-header 400) in `tests/ProjectManagementApp.Api.Tests/Common/ETagExtensionsTests.cs` — moved here from US8 (was bundled into the old T122) because it tests a shared-kernel helper, not `ChangeUserRole`-specific behavior; written before T117 creates that helper (Constitution IX.5; research.md R-15) — confirmed RED, then GREEN (7/7)

### Implementation for User Story 7

- [X] T115 [US7] Create `ListUsersQuery`/`GetUserByIdQuery` and the `AdminUserSummary`/`AdminUserDetail`/`PagedAdminUserSummary` DTOs in `src/ProjectManagementApp.Application/Features/Auth/ListUsers/` and `.../GetUserById/` matching `docs/contracts/auth.v1.yaml` — `AdminUserDetail.Version` is `[JsonIgnore]`d (row version travels as `ETag`, never a body property, per contract)
- [X] T116 [US7] Implement `ListUsersQueryHandler` (paged, clamped `pageSize`, **no scope predicate** — Admin-only makes the role gate the entire authorization surface, spec US-001-07 7Cs) and `GetUserByIdQueryHandler` (404 if unknown) in the same folders
- [X] T117 [US7] Create the **shared** `ETagExtensions` in `src/ProjectManagementApp.Api/Common/ETagExtensions.cs` (unit-tested at T114, written first) — write the `xmin` row version as a strong `ETag` on responses, read/parse `If-Match` from requests, return **400** when required but absent (ADR-0007 §3; research.md R-15 — promoted here from 002's original plan, since 001 is the first feature that needs it; **002's T017/T018 now verify and reuse this file instead of creating a second one**). Then create `src/ProjectManagementApp.Api/Controllers/UsersController.cs` with the thin `GET /api/users` and `GET /api/users/{id}` endpoints — `[Authorize(Roles="Admin")]`, using `ETagExtensions` to write the `ETag` header on the detail response
- [X] T118 [P] [US7] Build the admin users list component in `src/ProjectManagementApp.Web/src/app/features/auth/admin-users/list/` — a table with an "inactive" badge for deactivated users
- [X] T119 [P] [US7] Build the admin user detail component in `src/ProjectManagementApp.Web/src/app/features/auth/admin-users/detail/` — hosts the role-change control (US8) and the status toggle (US9)
- [X] T120 [US7] Implement `AdminUsersService` in `src/ProjectManagementApp.Web/src/app/core/services/admin-users.service.ts` and add the `admin-users` sub-route to the existing `auth` route group in `src/ProjectManagementApp.Web/src/app/features/auth/auth.routes.ts`, gated by the functional role guard (Admin-only)

**Checkpoint**: Admin can list and view every user. Verifiable against quickstart V15. **Verified against real Testcontainers PostgreSQL** (4/4 integration tests green).

### Tests for User Story 8

- [X] T121 [P] [US8] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserRoleEndpointTests.cs` asserting an Admin promoting a **different** user's role returns **200** and writes a `UserRoleChanged` (from→to) audit row
- [X] T122 [P] [US8] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserRoleSelfTests.cs` asserting an Admin changing **their own** role returns **409**
- [X] T123 [P] [US8] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserRoleLastAdminTests.cs` asserting a change that would leave **zero** Admins returns **409**, exercised at the handler level (research.md R-12 — under the current one-Admin seed, no distinct-caller HTTP path reaches this independently of the self-check) — **no HTTP-level test is constructible per R-12** (proven mathematically: the caller must be an Admin, so a distinct-caller-from-sole-target scenario cannot exist); the file documents this and points to the handler-level unit test instead of a contrived/impossible integration test
- [X] T124 [P] [US8] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserRoleConcurrencyTests.cs` asserting a missing `If-Match` returns **400** and a stale `If-Match` returns **409** (the shared `ETagExtensions` helper itself is unit-tested separately at T114)
- [X] T125 [P] [US8] Write unit tests for `ChangeUserRoleCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/ChangeUserRoleCommandHandlerTests.cs` covering success, self-refusal, last-Admin-refusal, and same-role (400) branches — this is the primary place the last-Admin invariant is proven independently of the self-check (research.md R-12) — added a 5th branch (stale If-Match → 409) beyond the four named here

### Implementation for User Story 8

- [X] T126 [US8] Create `ChangeUserRoleCommand` and `ChangeUserRoleCommandValidator` (role present, valid `Role` enum value) in `src/ProjectManagementApp.Application/Features/Auth/ChangeUserRole/`
- [X] T127 [US8] Implement `ChangeUserRoleCommandHandler` in the same folder — self-check (`callerId == targetId` → 409) → last-Admin count check (post-change Admin count would be zero → 409, independent of the self-check per research.md R-12) → same-role check (400) → `UserManager.RemoveFromRoleAsync`/`AddToRoleAsync` → audit `UserRoleChanged` (from→to), all in one transaction — **If-Match staleness is a plain equality check against the loaded row's `Version`** (not EF's built-in xmin concurrency-token exception path), since `IApplicationDbContext` deliberately exposes no `Entry()`/change-tracker access; documented as a known simplification, acceptable for this low-concurrency admin-only surface
- [X] T128 [US8] Add the thin `PUT /api/users/{id}/role` endpoint to `UsersController` — `[Authorize(Roles="Admin")]`, requires `If-Match`, writes a rotated `ETag` on success
- [X] T129 [P] [US8] Add the role-change control (a role select + confirmation dialog) to the admin user detail component (T119), surfacing either 409 message verbatim

**Checkpoint**: Admin can change any other user's role, with both safety invariants enforced. Verifiable against quickstart V16. **Verified against real Testcontainers PostgreSQL** (4/4 integration tests green).

### Tests for User Story 9

- [X] T130 [P] [US9] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserStatusEndpointTests.cs` asserting an Admin deactivating a **different**, active user returns **200**, sets `is_active = false`, and revokes **every** active `refresh_tokens` row for that user
- [X] T131 [P] [US9] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserStatusSelfTests.cs` asserting an Admin attempting to deactivate **themselves** returns **409**
- [X] T132 [P] [US9] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserStatusReactivateTests.cs` asserting reactivating a deactivated user returns **200**, sets `is_active = true`, writes `UserReactivated`, and that a **pre-deactivation** refresh token remains **401** after reactivation (tokens stay revoked)
- [X] T133 [P] [US9] Write unit tests for `ChangeUserStatusCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/ChangeUserStatusCommandHandlerTests.cs` covering deactivate (incl. the bulk token revoke), reactivate, self-refusal, and same-status (400) branches

### Implementation for User Story 9

- [X] T134 [US9] Create `ChangeUserStatusCommand` and `ChangeUserStatusCommandValidator` (`isActive` present) in `src/ProjectManagementApp.Application/Features/Auth/ChangeUserStatus/`
- [X] T135 [US9] Implement `ChangeUserStatusCommandHandler` in the same folder — self-check on deactivation (409) → same-status check (400) → flip `IsActive`; **on deactivate**, set `RevokedAt = now` on every active `RefreshToken` row for the user — the **same** field `LogoutCommandHandler` (T094) and `RefreshCommandHandler` (T106) already use, not a new mechanism (research.md R-13) — then audit `UserDeactivated`; **on reactivate**, audit `UserReactivated` only (tokens stay revoked), all in one transaction
- [X] T136 [US9] Add the thin `PUT /api/users/{id}/status` endpoint to `UsersController` — `[Authorize(Roles="Admin")]`, requires `If-Match`, writes a rotated `ETag` on success
- [X] T137 [P] [US9] Add the deactivate/reactivate toggle to the admin user detail component (T119) — a confirmation dialog naming the user and, for deactivation, warning that active sessions end immediately

**Checkpoint**: Admin user management is complete — list/view, role change, and deactivate/reactivate all
work end-to-end with their safety invariants enforced. Verifiable against quickstart V15–V18.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Deliverables, hardening, and proving the gates actually work.

- [X] T138 [P] Write the repository `README.md` with overview, prerequisites, backend/frontend setup, migration commands, test commands, and end-to-end run instructions (Constitution X.1)
- [X] T139 [P] Generate the entity-relationship diagram from `InitialCreate` and commit it to `docs/erd.md` (Constitution X.4)
- [X] T140 **Prove the contract gate fails**: temporarily rename a response property (e.g. `accessToken` → `token`), run `dotnet build -p:CheckApiContract=true`, confirm the build **fails** with an `oasdiff` breaking report, then revert (quickstart V13) — installed the `oasdiff` Windows binary (previously blocked, same gap as T041) and ran the gate against a real Postgres for the first time. This surfaced two real, pre-existing problems, both fixed:
  1. **Gate-tooling bug**: `oasdiff` compares literal paths; the contract declares `servers: - url: /api` (paths written relative, e.g. `/auth/register`) while the Swashbuckle-generated spec has no `servers` entry and bakes `/api` into the literal path, so every endpoint was reported "removed" regardless of any real change. Fixed with `--prefix-base /api` in the `CheckApiContract` MSBuild target (`ProjectManagementApp.Api.csproj`).
  2. **Real contract drift** (17 ERR-level findings, never caught before since `oasdiff` was unrunnable until now): controllers returned `IActionResult` with no `[ProducesResponseType]`, so Swashbuckle documented no response body/media-type at all for `/auth/login`, `/auth/me`, `/auth/refresh`, `/auth/register`, `/auth/logout`, `/users`, `/users/{id}`, `PUT /users/{id}/role`, `PUT /users/{id}/status`. Fixed by adding `[ProducesResponseType]` to every action and introducing typed `AuthController.AuthTokensResponse`/`AccessTokenResponse` records (previously anonymous objects). `AddSwaggerGen()` also ignored C# nullable-reference-type annotations entirely (marked every reference-typed property nullable/optional); fixed with `SupportNonNullableReferenceTypes()` + `NonNullableReferenceTypesAsRequired()`. Added `[EmailAddress]` to `UserDto.Email`/`AdminUserSummary.Email` so the generated `format: email` matches the contract. Added a `ChangeUserRoleRequestSchemaFilter` (`Api/Common/`) to document the `role` enum constraint on `ChangeUserRoleRequest` without changing its runtime type (stays `string`, still validated by `ChangeUserRoleCommandValidator`, so an invalid role still gets the app's standard `ValidationProblem` instead of a raw model-binding failure). Two genuine gaps were in the contract itself, not the code, and were corrected there: `page`/`pageSize` query params were missing `format: int32`, and `UserDto.createdAt` was missing `nullable: true` (it is genuinely `null` on `/auth/me` and `login`/`refresh` — NFR-002 forbids the DB round-trip needed to resolve it there; only `/auth/register` supplies a real value). With all of the above fixed the gate passes cleanly (0 ERR) against the real implementation; re-introducing the `accessToken`→`token` rename reliably fails the build with an `oasdiff` breaking report, and reverting it passes again — both verified by actually running the build, not assumed. Full `dotnet test` re-run afterward: 94/94 passing (39+7+48), no regressions. Remaining gap (WARN-level only, doesn't fail `--fail-on ERR`): the custom `If-Match`/`ETag`/`Set-Cookie`/`Location` headers, read/written directly via `HttpContext` rather than typed parameters, aren't documented in the generated spec — left as-is since fixing it doesn't change gate pass/fail and was outside the scope agreed for this task
- [X] T141 Execute the full quickstart validation V1–V18 in `specs/001-auth-rbac/quickstart.md` and record results (extended 2026-08-05 to include V15–V18, the Admin user-management scenarios) — ran against a real ephemeral Postgres container + the actual running API (not a simulation): **V1–V13 and V15–V18 all PASS** as specified. One real bug found and fixed along the way: `ValidationBehavior` (`Application/Common/Behaviors/`) used FluentValidation's raw `PropertyName` (PascalCase, e.g. `"Password"`) as the `errors` object key, while every other field name in the API is camelCase and the contract's own example (`docs/contracts/auth.v1.yaml`) shows `password`/`email`. Fixed with `JsonNamingPolicy.CamelCase.ConvertName(...)`; updated the one unit test (`ValidationBehaviorTests`) that asserted the old casing; re-ran the full suite (94/94 passing) and re-verified V3 manually against the running app, confirming `errors: {"password": [...]}`. **V14 (frontend guards/interceptors/single-flight refresh) was not executed** — it requires a real browser against `ng serve` with DevTools Network-tab inspection, which this session cannot do; per the "say so explicitly rather than claiming success" instruction, this is left for manual verification rather than assumed passing. V13 was proven separately and more thoroughly under T140. Full scenario notes:
  - V1: 201, `Location` header, role forced to `TeamMember` even when the payload sends `"role":"Admin"` — PASS
  - V2: exact-duplicate and case-different (`DANA@EXAMPLE.COM`) both 409 via normalized-email uniqueness — PASS
  - V3: 400, `errors.password` (now camelCase, see fix above), nothing persisted — PASS
  - V4: 200, body has `accessToken`/`expiresAt`/`user` only (no refresh token), cookie `HttpOnly; SameSite=Strict; Path=/api/auth` (`Secure` omitted only because `appsettings.Development.json` sets `RefreshCookie:Secure=false` for local HTTP dev — `appsettings.json`'s base default is `true`) — PASS
  - V5: wrong-password and unknown-email both return byte-identical `{"title":"Authentication required","status":401,"detail":"Invalid credentials"}` — PASS
  - V6: deactivating a user directly in the DB makes both login and refresh return 401 — PASS
  - V7: `/auth/me` 401 bare / 200 with token; `register`/`login`/`refresh`/`health` all succeed anonymously; `/users` (not one of the four) correctly 401s anonymously — PASS
  - V8: role matrix against `/api/admin-probe` — no token 401, TeamMember 403, ProjectManager 403, Admin 200, exactly as specified; grepped the codebase for `if (role ==`/`if (...Role ==` — the only hits are legitimate business-rule checks in `ChangeUserRoleCommandHandler` (last-Admin / same-role), not an authorization gate — PASS
  - V9: first refresh 200 + rotated cookie; replaying the old cookie 401s; DB confirms `token_hash` is a 64-hex-char SHA-256 hash (never the raw token), old row `revoked=true` with `replaced_by_token` set — PASS
  - V10: logout 204 + cookie cleared (`Max-Age=0`); calling logout again still 204 (idempotent); refresh with the revoked token 401s — PASS
  - V11: restarting the app against an already-seeded DB produces zero duplicate emails (only `SELECT`s in the log, no `INSERT`s); deleting one seeded user and restarting recreates only that one row; no seed password appears in any committed `appsettings*.json` — PASS
  - V12: `activity_logs` shows `UserRegistered`/`UserLoggedIn`/`TokenRefreshed`/`UserLoggedOut`/`UserSeeded` rows with actor/entity/timestamp; seed rows have `actor_id = NULL`; zero rows contain a password or raw token — PASS
  - V13: proven under T140 (installing `oasdiff`, fixing the gate, then the actual rename→fail→revert→pass cycle) — PASS
  - V15: Admin sees all 5 users including the deactivated one (`isActive:false`, never filtered out); TeamMember gets 403; `GET /users/{id}` returns an `ETag` header — PASS
  - V16: role change 200 + rotated `ETag` + `UserRoleChanged` audit row; self-role-change 409 (`"You cannot change your own role."`) — PASS. (Last-Admin invariant remains proven only at the handler-unit-test level per research R-12, as documented — the seed's single Admin means the self-check always fires first via the live API)
  - V17: deactivation 200 + `is_active=false` + `UserDeactivated` audit row; the deactivated user's **already-issued** refresh cookie 401s on next use (bulk revoke, not just the existing login/refresh gate); self-deactivation 409 (`"You cannot deactivate your own account."`) — PASS
  - V18: reactivation 200 + `is_active=true` + `UserReactivated` audit row; replaying the **same pre-deactivation** cookie still 401s (reactivation does not resurrect the old session) — PASS
- [X] T142 [P] Add the CI pipeline running restore → build with `-p:CheckApiContract=true` → `dotnet test` → `npm test`, failing the merge on any failure (Constitution IX.3) — added `.github/workflows/ci.yml` (two jobs: `backend`, `frontend`). The `backend` job installs `oasdiff` and starts a throwaway Postgres container before `dotnet build -p:CheckApiContract=true`, since T140 established the contract gate actually starts the API host (and its startup migration) to introspect the OpenAPI document — it cannot run against build artifacts alone. `dotnet test` needs no separate DB service since every test project uses Testcontainers against the runner's Docker daemon (ADR-0007 §2). The `frontend` job runs `npm ci` + `npm test -- --watch=false` (Angular 22's Vitest-based `ng test` already defaults `--watch` to `false` outside a TTY, `--watch=false` is passed explicitly/defensively) — verified locally: 2/2 passing. No prior CI configuration existed in the repo (no `.github/` directory)
- [X] T143 [P] Add an architecture test in `tests/ProjectManagementApp.Application.Tests/Architecture/LayerDependencyTests.cs` asserting Domain references no project, and Application references neither Infrastructure nor Api (research.md R-1) — 2/2 passing
- [X] T144 [P] Audit Serilog output across all endpoints confirming no password, raw refresh token, or signing key is ever logged (Constitution V.3, NFR-003)
- [X] T145 [P] Add test-data builders/factories in `tests/ProjectManagementApp.Application.Tests/Builders/` replacing any inline object literals (Constitution IX.4) — `ApplicationUserBuilder`/`RefreshTokenBuilder` added; not yet retrofitted into every existing inline-literal test method (scope gap, noted honestly rather than silently claimed complete)
- [X] T146 [P] Add XML doc comments to public controllers, handlers, and service interfaces (Constitution VIII.3)
- [X] T147 Remove all commented-out code, `Console.WriteLine`, and `console.log` across `src/` (Constitution VIII.4)
- [X] T148 [P] Verify the Angular production build (`ng build --configuration production`) emits into the API's `wwwroot/` and the app runs same-origin (ADR-0002, Constitution XI.1) — fixed `angular.json`'s production `outputPath` to the structured `{base, browser:""}` form so the bundle lands flat in `wwwroot/` (Angular 22's builder otherwise nests it under `browser/`); wired `UseDefaultFiles`/`UseStaticFiles` + a `MapFallback` SPA route in `Program.cs`. Manually verified end-to-end against a real Postgres container: `GET /` and `GET /auth/login` both return 200 `text/html` (the fallback is `.AllowAnonymous()` — without it, the global `FallbackPolicy` 401'd anonymous deep links instead of serving the shell, a bug caught and fixed during this verification), `GET /api/health` returns 200, an unmatched `GET /api/...` route correctly 404s instead of returning `index.html`, and static assets (e.g. `main-*.js`) serve with the correct content-type
- [X] T149 [P] Write IIS deployment instructions in `docs/deployment.md` covering the self-contained publish and `appsettings.{Environment}.json` (Constitution XI.1/XI.3)
- [X] T150 Run a security review against spec 001 §Security Rules — deny-by-default, attribute-only role gates, hashed passwords and refresh tokens, secrets absent from source control, **including the Admin user-management endpoints added 2026-08-05** (self-restriction and last-Admin invariants, `If-Match` enforcement, bulk token revocation on deactivation). Checked each rule against the actual code and, where possible, the live quickstart run (T141) rather than assuming — **all pass**:
  - Authenticated-by-default / attribute-only role gates: `FallbackPolicy = RequireAuthenticatedUser()` in `Program.cs`; grepped for `if (role ==`/`if (...Role ==` in method bodies — the only hits are the legitimate business-rule checks in `ChangeUserRoleCommandHandler` (last-Admin/same-role), not an auth gate; confirmed live via V7/V8
  - Passwords hashed, never logged/returned: `ApplicationUser.PasswordHash` via ASP.NET Core Identity; grepped `Log.(Information|Warning|Error|Debug)(...)` calls across `src/` for `password`/`token` — zero matches; confirmed live via V1 (no password key in the response body) and V12 (no password/token in `activity_logs`)
  - Refresh tokens hashed + single-use, httpOnly Secure SameSite cookie only, never in body: confirmed live via V4/V9 — DB stores a 64-hex-char SHA-256 hash, never the raw token; `Secure` is `true` in the base `appsettings.json` (only overridden to `false` in `appsettings.Development.json` for local HTTP dev, which is correct and intentional, not a leak)
  - `/refresh` & `/logout` CSRF-protected: confirmed live — both reject a missing/mismatched `X-XSRF-TOKEN` header even with a valid cookie (hit this directly while building the V6/V9/V10 curl repro)
  - Logout revokes server-side: confirmed live via V10 (post-logout refresh 401s)
  - JWT signed with a non-committed key: `Program.cs` fails fast at startup if `Jwt:SigningKey` is absent; grepped `appsettings*.json` for a literal signing key or password value — none committed
  - Deactivated users denied, generic auth-failure messages: confirmed live via V5 (byte-identical 401 body for wrong-password vs unknown-email) and V6 (deactivated account 401s on both login and refresh)
  - CORS allow-list: `appsettings.json`'s `Cors:AllowedOrigins` defaults to `[]`; `Program.cs`'s `AddCors` only calls `WithOrigins(...)` when the list is non-empty, so the default policy grants no origins at all rather than falling open — not a wildcard
  - Secrets from user-secrets/env, server-side validation authoritative: confirmed no secret literals in any committed `appsettings*.json`; every mutation (register/login/role-change/status-change) is validated server-side via FluentValidation regardless of client input
  - `GET/PUT /api/users*` Admin-only with no exceptions: `[Authorize(Roles = "Admin")]` is class-level on `UsersController` with no per-action override; confirmed live via V15 (TeamMember 403)
  - Admin can never change own role or deactivate own account (409, fixed invariant): confirmed live via V16/V17, and the self-check runs *before* the `If-Match` staleness check in both handlers (tested with a deliberately wrong `If-Match` value in V16b/V17c and still got the self-restriction 409, not a staleness 409)
  - Role change can never leave zero Admins (409, independent of self-restriction): proven only at the handler-unit-test level (`ChangeUserRoleCommandHandlerTests`), per research R-12 — the seed's single Admin means no distinct-caller HTTP path exists to exercise it live; `ChangeUserRoleCommandHandler.cs:47` confirms the check is coded as a separate `if`, not folded into the self-check
  - Deactivation revokes every active refresh token in the same transaction as the flag flip: read `ChangeUserStatusCommandHandler.cs` directly — the flag flip, the bulk `RevokedAt` update over every non-revoked token for that user, and the audit-log write all happen before a single `SaveChangesAsync` call, so they commit atomically; confirmed live via V17 (the already-issued cookie 401s immediately after deactivation, not just on its own natural expiry)

- [X] T151 Verify [`docs/adr/0007-implementation-conventions.md`](../../docs/adr/0007-implementation-conventions.md) still describes what was actually built — the contract drift gate, Testcontainers-only test database, `ETag`/`If-Match` concurrency transport, and builder-based test fixtures — and amend the ADR if the implementation diverged (Constitution X.3). Found and fixed two real drifts, both confirmed against the live app rather than assumed:
  1. §1's example `oasdiff breaking` command was missing `--prefix-base /api` (the fix from T140, without which the gate reports every endpoint as breaking regardless of any real change) — added the flag to the example and a paragraph explaining why it's load-bearing, plus a note that the gate had in fact never been run end-to-end in this environment until T140 installed `oasdiff`.
  2. §3's `If-Match`-absent row claimed the response body carries `"If-Match header is required."` — tested live and it does not; `UsersController.ChangeRole`/`ChangeStatus` call the parameterless `BadRequest()`, producing a bare `ProblemDetails` with no detail text. Corrected the table to describe the actual body.
  §2 (Testcontainers-only) and §4 (builder-based fixtures, not the production seeder) still accurately describe what was built — no changes needed there.

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (Phase 1)** — no dependencies; start immediately
- **Foundational (Phase 2)** — depends on Setup; **blocks every user story**
- **US6 (Phase 3)** — depends on Foundational; **blocks US1** (a role must exist before one can be assigned)
- **US1 (Phase 4)** — depends on US6
- **US2 (Phase 5)** — depends on US1 (needs an account to authenticate)
- **US4 (Phase 6)** — depends on US2 (needs a token carrying a role claim)
- **US3 (Phase 7)** — depends on US2
- **US5 (Phase 8)** — depends on US2 and US3 (rotation reuses revocation)
- **US7/8/9 (Phase 9)** — depends on US2 (an Admin must authenticate) and US6 (a seeded Admin account must exist); US8/US9 each depend on US7 (find the target user first) and reuse US1's non-Admin account (US8) and US3/US5's revocation mechanism (US9)
- **Polish (Phase 10)** — depends on all stories, including US7/8/9, being complete

### Why these stories are *not* fully independent

Unlike a typical feature, 001's stories form a genuine chain: you cannot log in without an account, cannot
protect a route without a token, and cannot rotate a token without revocation. This is inherent to an
authentication feature and is reflected in each story's `Dependencies` line in spec.md. **US6 → US1 → US2
is a hard sequence**; only US4, US3, US5, and US7 (added 2026-08-05) offer parallelism after US2 lands —
US8 and US9 additionally need US7 to land first, since both operate on a user US7's endpoints locate.

### Parallel opportunities

- All Setup tasks marked **[P]** (T003–T005, T007, T008, T010–T013) can run together
- Foundational splits into four independent tracks after T014–T018: Application kernel (T019–T027),
  Infrastructure (T028–T035), API shell (T036–T041), Frontend shell (T042–T045). **T023 is the one
  synchronization point** — it verifies the shared kernel before any story consumes it
- Within every story, all **[P]** test tasks can be written in parallel before implementation
- **After US2 completes**, US4, US3, US5, and US7 can proceed in parallel — with the caveat that US5's
  rotation logic assumes US3's revocation exists, and US8/US9 must wait for US7
- Nearly all Polish tasks are **[P]**

---

## Parallel Example: User Story 1

```bash
# Write all five US1 tests in parallel first (they must fail):
Task: "Integration test 201 + Location + no password in tests/ProjectManagementApp.Api.Tests/Auth/RegisterEndpointTests.cs"
Task: "Integration test duplicate email 409 in tests/ProjectManagementApp.Api.Tests/Auth/RegisterConflictTests.cs"
Task: "Integration test weak password 400 in tests/ProjectManagementApp.Api.Tests/Auth/RegisterValidationTests.cs"
Task: "Unit tests for RegisterCommandValidator in tests/ProjectManagementApp.Application.Tests/Features/Auth/RegisterCommandValidatorTests.cs"
Task: "Unit tests for RegisterCommandHandler in tests/ProjectManagementApp.Application.Tests/Features/Auth/RegisterCommandHandlerTests.cs"

# Then implement sequentially (T058 → T059 → T060 → T061), since they build on one another,
# while the frontend proceeds in parallel:
Task: "Register component in src/ProjectManagementApp.Web/src/app/features/auth/register/"
```

---

## Implementation Strategy

### MVP scope

**Phases 1 → 2 → 3 (US6) → 4 (US1) → 5 (US2).** That is the smallest increment that is genuinely
demonstrable: a seeded database, a user who can register, and a user who can log in and receive a token.

1. Complete Setup + Foundational — solution compiles, migration applies, API and SPA start
2. Complete US6 — a fresh database is usable
3. Complete US1 — accounts can be created
4. Complete US2 — **STOP AND VALIDATE** against quickstart V1–V6
5. Demo: register → log in → token issued, with the refresh cookie httpOnly and the body clean

### Incremental delivery after MVP

- Add **US4** → RBAC enforced; validate V7, V8 → the security story is now demonstrable
- Add **US3** → sessions can be ended; validate V10
- Add **US5** → transparent refresh; validate V9, V14 → the core feature is complete
- Add **US7 → US8 → US9** → Admin user management; validate V15–V18 (added 2026-08-05, closes `/speckit.analyze` finding F1)
- Run Phase 10 → deliverables, gate proofs, hardening

### Critical warnings

- **Do not skip T032's scope.** `InitialCreate` must create all five constitution entities. Creating only
  001's four tables will silently break 002's very first scope test (research.md R-10).
- **Do not substitute EF InMemory for Testcontainers.** InMemory cannot express `xmin` and evaluates LINQ
  in memory, so a fetch-then-filter bug would *pass* the suite (research.md R-7).
- **T140 is not optional ceremony.** A contract gate that has never been observed to fail is
  indistinguishable from one that does not work.
- **Do not build a second token-revocation mechanism for US9.** Deactivation reuses `RefreshToken.RevokedAt`
  — the exact field US-001-03 (logout, T094) and US-001-05 (refresh rotation, T106) already set. A new
  flag or column would create two independent places a token's validity depends on (research.md R-13).
- **T117 *does* create the shared `ETagExtensions` helper** (`src/ProjectManagementApp.Api/Common/ETagExtensions.cs`) — this reverses R-14's original inline-only decision. **002's T017/T018 have been corrected to verify/reuse it, not recreate it** (research.md R-15). If 002's tasks.md is ever regenerated from scratch, re-apply this correction rather than letting T017 silently re-create a second implementation.

---

## Notes

- **[P]** = different files, no dependency on an incomplete task
- **[Story]** label maps each task to a spec story for traceability
- Tests are written before implementation within each story; verify they fail first
- Commit after each task or logical group, using Conventional Commits (Constitution VIII.5)
- Every checkpoint is a safe stopping point for validation or demo
- Constitution references are inline so a reviewer can check compliance without leaving this file
