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

- [ ] T001 Create `ProjectManagementApp.sln` at repo root plus the `src/` and `tests/` directory skeleton per plan.md §Project Structure
- [ ] T002 Create the four source projects (`src/ProjectManagementApp.Domain`, `.Application`, `.Infrastructure`, `.Api`) and wire project references so the graph is exactly Domain ← Application ← Infrastructure ← Api, with Api also referencing Application (composition root) — per research.md R-1
- [ ] T003 [P] Create the three test projects `tests/ProjectManagementApp.Application.Tests`, `.Infrastructure.Tests`, `.Api.Tests` with references to their subjects (no `Domain.Tests` — see research.md R-1)
- [ ] T004 [P] Add `Directory.Build.props` at repo root enabling `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, and `<LangVersion>latest</LangVersion>` for all projects (Constitution VIII.1)
- [ ] T005 [P] Add `.editorconfig` at repo root with C# naming rules (PascalCase types/members, camelCase locals) per Constitution VIII.2
- [ ] T006 Add backend NuGet packages: `Microsoft.Extensions.Identity.Stores` to Domain; `MediatR`, `FluentValidation`, `Microsoft.EntityFrameworkCore` to Application; `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `EFCore.NamingConventions` to Infrastructure; `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore`, `Serilog.AspNetCore` to Api. **Note the transitive path that three features rely on**: `.Domain`'s `Microsoft.Extensions.Identity.Stores` brings in `Microsoft.Extensions.Identity.Core`, which is what makes **`UserManager<ApplicationUser>` reachable from `.Application`** — used by 001's own handlers (T060) and by 002/004 for role lookups (analyze finding G3). If that transitive reference is ever dropped, add `Microsoft.Extensions.Identity.Core` to `.Application` explicitly rather than moving the role check
- [ ] T007 [P] Add test NuGet packages to all three test projects: `xunit`, `Testcontainers.PostgreSql`, `Respawn`, `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing`, `NSubstitute` (research.md R-7)
- [ ] T008 [P] Create `.config/dotnet-tools.json` pinning `dotnet-ef` and `Swashbuckle.AspNetCore.Cli` as local tools
- [ ] T009 Create the Angular 22 workspace at `src/ProjectManagementApp.Web` with standalone bootstrap (no `@NgModule`, ADR-0001), then add Angular Material and NgRx (`provideStore`/`provideEffects`)
- [ ] T010 [P] Configure `src/ProjectManagementApp.Web/tsconfig.json` for TypeScript strict mode, add `proxy.conf.json` routing `/api` to the API, and set the production build `outputPath` to the API's `wwwroot/` (ADR-0002 same-origin)
- [ ] T011 [P] Bootstrap Serilog in `src/ProjectManagementApp.Api/Program.cs` with console + rolling-file sinks and structured output (Constitution NFR-003)
- [ ] T012 [P] Create `src/ProjectManagementApp.Api/appsettings.json` and `appsettings.Development.json` with the non-secret keys from spec B.4 (`Jwt:AccessTokenMinutes`, `Jwt:RefreshTokenDays`, `Identity:Password:*`, `Identity:Lockout:*`, `Cors:AllowedOrigins`, `RefreshCookie:*`, `Seed:Enabled`) — no secret values
- [ ] T013 [P] Update `.gitignore` to exclude `artifacts/`, `*.user`, and confirm no `appsettings.*.Local.json` or secret file can be committed (Constitution V.4)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain model, shared kernel, persistence, MediatR pipeline, and the API/Angular shells.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Domain layer

- [ ] T014 [P] Create `ApplicationUser : IdentityUser<Guid>` and `ApplicationRole : IdentityRole<Guid>` in `src/ProjectManagementApp.Domain/Entities/` with `FullName`, `IsActive`, `CreatedAt`, `UpdatedAt`, the `Version` (`xmin`) token, and navigation collections per data-model.md §2
- [ ] T015 [P] Create `RefreshToken` entity in `src/ProjectManagementApp.Domain/Entities/RefreshToken.cs` with `TokenHash`, `ExpiresAt`, `RevokedAt`, `ReplacedByToken`, and the computed `IsActive` (data-model.md §2)
- [ ] T016 [P] Create `ActivityLog` entity in `src/ProjectManagementApp.Domain/Entities/ActivityLog.cs` with nullable `ActorId` (soft FK — no constraint), `Action`, `EntityType`, `EntityId` (string), `Timestamp`, `ChangeSummary`
- [ ] T017 [P] Create the table-only entities `Project`, `TaskItem`, `TeamMember` in `src/ProjectManagementApp.Domain/Entities/` to the field lists in specs 002/003/004 — **required by research.md R-10**; 001 adds no rules to them
- [ ] T018 [P] Create enums `Role`, `AuditAction`, `ProjectStatus`, `TaskStatus`, `TaskPriority` in `src/ProjectManagementApp.Domain/Enums/` (data-model.md §3). **`AuditAction` must carry the complete value set for all six features**, not just 001's — `UserRegistered, UserLoggedIn, UserLoggedOut, TokenRefreshed, UserDeactivated, UserSeeded, UserRoleChanged, UserReactivated` (001 — the last two added 2026-08-05 for US-001-08/09) · `ProjectCreated, ProjectUpdated, ProjectDeleted, ProjectOwnerChanged` (002) · `TaskCreated, TaskUpdated, TaskStatusChanged, TaskReassigned, TaskDeleted` (003) · `TeamMemberAdded, TeamMemberRemoved` (004) · `ReportGenerated` (006). It is one shared enum in `Domain/Enums/`, exactly like `ProjectStatus`/`TaskStatus`, so later features **consume** values rather than editing 001's file

### Application layer — shared kernel

- [ ] T019 [P] Implement `Result`, `Result<T>`, `Error`, and `ErrorKind` in `src/ProjectManagementApp.Application/Common/Models/` exactly as `docs/shared-contracts.md` §1 defines (ADR-0003). **`ErrorKind` must carry all seven members** — `Validation, Unauthenticated, Forbidden, NotFound, Conflict, UnprocessableContent, Unexpected`. **`UnprocessableContent` is required here even though only 006 uses it** (its 422 large-report guard); omitting it would force 006 to extend a shared-kernel enum locally, which the ADR-0007 §5 rule forbids
- [ ] T020 [P] Implement `CurrentUser`, `AccessDecision`, `PagedResult<T>`, **`TaskMutation`** (`Create, FullEdit, StatusChange, Reassign, Delete`), **`ActivityScope`**, and **`ActivityEntry`** in `src/ProjectManagementApp.Application/Common/Models/`, **plus `MetricDefinitions`** in `src/ProjectManagementApp.Application/Common/Metrics/MetricDefinitions.cs` — all per `docs/shared-contracts.md` §2/§3/§4/§6/§8. **All seven are required here even though only later features consume some of them**: T022 authors the shared-kernel interfaces whose signatures reference them (`ITaskAccessPolicy.CanMutateAsync` → `TaskMutation`; `IActivityLogService.QueryScopedAsync` → `ActivityScope`/`ActivityEntry`), and **`MetricDefinitions`** (`IsOverdue(todayUtc)`, `IsClosed`, `CompletionRate` returning **`0` when total is 0**, `ClosedInWindow`) must be shared because 005 and 006 are obliged to produce **identical** values (006 NFR-002) — putting it in whichever feature needed it first would force the other to depend on that feature's Application layer (ADR-0007 §5). `todayUtc` is **UTC and fixed** (003 research R-1, 005 research R-1, 006 analyze G1)
- [ ] T021 [P] Define `IApplicationDbContext` in `src/ProjectManagementApp.Application/Common/Interfaces/IApplicationDbContext.cs` exposing all six `DbSet<T>` properties and `SaveChangesAsync`, **exactly as `docs/shared-contracts.md` §7 now specifies** — the DbContext behind an interface, not a repository (research.md R-3)
- [ ] T022 [P] Define **all** shared-kernel interfaces in `src/ProjectManagementApp.Application/Common/Interfaces/` per spec 001 B.3 and `docs/shared-contracts.md` §2/§3/§6: `ICurrentUserService`, `ITokenService`, `IDataSeeder`, `IActivityLogService` (**both** `LogAsync` **and** the scoped read `QueryScopedAsync(ActivityScope, page, pageSize, ct)` → `Task<PagedResult<ActivityEntry>>`, required by 005's feed and 006's Activity Report), and the three **scope-authorization policies** `IProjectAccessPolicy`, `ITaskAccessPolicy`, `ITeamAccessPolicy` exactly as §3 declares them. **The policy interfaces belong here, not in 002/003/004** — those features implement the rules, but the interfaces are shared kernel so 005/006 can depend on them without taking a dependency on another feature's Application layer (ADR-0006 addendum)
- [ ] T023 Verify **every** shared-kernel member declared in `docs/shared-contracts.md` **§2, §3, §4, §6, and §7** exists in `src/ProjectManagementApp.Application/Common/` and matches its declaration verbatim — the interfaces (`IApplicationDbContext`, `ICurrentUserService`, `ITokenService`, `IActivityLogService` incl. `QueryScopedAsync`, `IProjectAccessPolicy`, `ITaskAccessPolicy`, `ITeamAccessPolicy`) **and** the types they reference (`CurrentUser`, `AccessDecision`, `PagedResult<T>`, `TaskMutation`, `ActivityScope`, `ActivityEntry`). Assert no feature-local redefinition exists and **no repository wraps `IApplicationDbContext`**. **Enumerate the sections rather than spot-checking**: this gate previously covered only §2/§6/§7, and that omission is exactly why the §3 access-policy interfaces went uncreated until 005's planning sweep found them (ADR-0006 addendum)
- [ ] T024 [P] Write unit tests for both MediatR pipeline behaviors in `tests/ProjectManagementApp.Application.Tests/Common/Behaviors/` — assert `ValidationBehavior` short-circuits with a failed `Result` (never throws), and assert `LoggingBehavior` never logs a request body. **Written and run first — expect them to fail** (`ValidationBehavior`/`LoggingBehavior` do not exist yet); Constitution IX.5 (fixed 2026-08-05, `/speckit.analyze` finding D1 — this block previously implemented before testing)
- [ ] T025 Implement `ValidationBehavior<TRequest,TResponse>` in `src/ProjectManagementApp.Application/Common/Behaviors/ValidationBehavior.cs` — resolves `IEnumerable<IValidator<TRequest>>`, and on failure **short-circuits by returning `Result.Failure(ErrorKind.Validation, fields)`; it MUST NOT throw** (research.md R-4)
- [ ] T026 Implement `LoggingBehavior<TRequest,TResponse>` in `src/ProjectManagementApp.Application/Common/Behaviors/LoggingBehavior.cs` opening a Serilog scope with request name, user id, correlation id, and elapsed ms — **MUST NOT log request bodies** (they carry plaintext passwords; Constitution V.3)
- [ ] T027 Create `src/ProjectManagementApp.Application/DependencyInjection.cs` registering MediatR, FluentValidation validators from the assembly, and the two behaviors in the order Logging → Validation (research.md R-4)

### Infrastructure layer — persistence

- [ ] T028 [P] Configure the global snake_case naming convention (via `EFCore.NamingConventions`) in `src/ProjectManagementApp.Infrastructure/Persistence/` so `FullName` maps to `full_name` without per-property mapping (Constitution VIII.2)
- [ ] T029 Implement `ApplicationDbContext : IdentityDbContext<...>, IApplicationDbContext` in `src/ProjectManagementApp.Infrastructure/Persistence/ApplicationDbContext.cs` calling `ApplyConfigurationsFromAssembly` — no fluent config inline in `OnModelCreating`
- [ ] T030 [P] Add `IEntityTypeConfiguration<T>` classes for the 001-owned entities in `src/ProjectManagementApp.Infrastructure/Persistence/Configurations/` — Identity table renames (`users`/`roles`/`user_roles`), the `xmin` row-version mapping, and the indexes in data-model.md §4
- [ ] T031 [P] Add `IEntityTypeConfiguration<T>` classes for `Project`, `TaskItem`, `TeamMember` in `src/ProjectManagementApp.Infrastructure/Persistence/Configurations/` with the exact delete behaviors from data-model.md §4 (CASCADE / RESTRICT / SET NULL)
- [ ] T032 Generate the `InitialCreate` migration in `src/ProjectManagementApp.Infrastructure/Persistence/Migrations/` — **it MUST create all five constitution entities**, not only 001's (research.md R-10, data-model.md §1)
- [ ] T033 Implement `ActivityLogService : IActivityLogService` in `src/ProjectManagementApp.Infrastructure/Services/ActivityLogService.cs` — **two deliverables**: (a) `LogAsync` writes the audit row into the caller's unit of work so it commits in the **same** `SaveChangesAsync` (Constitution IV.4); (b) **`QueryScopedAsync`** performs the scoped, paginated read — filter to `ActivityScope` (all entries for an unscoped Admin; otherwise entries whose entity belongs to the caller's visible projects), order newest-first with a stable `(timestamp, id)` tiebreak, clamp `pageSize` to the configured maximum, and return `PagedResult<ActivityEntry>` with a **scope-limited `totalCount`**. Consumed by 005's feed and 006's Activity Report, which are both forbidden from querying `activity_logs` directly (shared-contracts §6/§7, 005 research R-1)
- [ ] T034 Create `src/ProjectManagementApp.Infrastructure/DependencyInjection.cs` registering `ApplicationDbContext` (as `IApplicationDbContext`), Identity stores, `ActivityLogService`, `TokenService`, and `DataSeeder`. **Includes the `AddIdentity<ApplicationUser, ApplicationRole>(options => ...)` call that binds `Identity:Password:*` and `Identity:Lockout:*` from configuration** — T012 creates those config keys, but no other task consumes them; without this line they are declared and never read (`/speckit.analyze` finding E2)
- [ ] T035 Create the shared Testcontainers PostgreSQL fixture + Respawn reset helper in `tests/ProjectManagementApp.Infrastructure.Tests/Fixtures/` as an xUnit `ICollectionFixture` (one container per test run) — reused by `.Api.Tests` (research.md R-7)

### API layer — shell

- [ ] T036 [P] Implement `ToActionResult()` extensions in `src/ProjectManagementApp.Api/Common/ResultExtensions.cs` mapping `ErrorKind` → status per `docs/shared-contracts.md` §1 — **the only place this mapping exists** (research.md R-8). **Cover all seven kinds**, including `UnprocessableContent` → **422**; a `switch` over the enum with no default arm will fail to compile if a member is missed, which is the intended safety net
- [ ] T037 [P] Implement `ExceptionHandlingMiddleware` in `src/ProjectManagementApp.Api/Common/ExceptionHandlingMiddleware.cs` emitting RFC 7807 500s with no stack trace in production
- [ ] T038 [P] Implement `CurrentUserService : ICurrentUserService` in `src/ProjectManagementApp.Api/Services/CurrentUserService.cs` backed by `IHttpContextAccessor`, registered scoped (research.md R-8)
- [ ] T039 Configure `src/ProjectManagementApp.Api/Program.cs`: JWT bearer authentication scheme, the **global fallback authorization policy requiring an authenticated user** (Constitution V.1), CORS allow-list, Swagger UI in Development only (VI.5), and fail-fast startup validation when `Jwt:SigningKey` or the connection string is missing
- [ ] T040 [P] Implement `HealthController` in `src/ProjectManagementApp.Api/Controllers/HealthController.cs` serving `GET /api/health` as `[AllowAnonymous]`, matching the `HealthResponse` schema in `docs/contracts/auth.v1.yaml`
- [ ] T041 Add the `CheckApiContract` MSBuild target to `src/ProjectManagementApp.Api/ProjectManagementApp.Api.csproj` running `dotnet swagger tofile` then `oasdiff breaking docs/contracts/auth.v1.yaml … --fail-on ERR`, gated on `Condition="'$(CheckApiContract)' == 'true'"` (research.md R-5)

### Frontend shell

- [ ] T042 Create the Angular app shell in `src/ProjectManagementApp.Web/src/app/` — `app.config.ts` (providers), `app.routes.ts`, and the `core/`, `shared/`, `features/` folder structure per plan.md §Project Structure
- [ ] T043 [P] Create the NgRx auth feature skeleton in `src/ProjectManagementApp.Web/src/app/core/store/auth/` (`createFeature`, initial state holding the access token in memory only — **never localStorage**)
- [ ] T044 [P] Create the shared `error-display` and `notification` components in `src/ProjectManagementApp.Web/src/app/shared/`, plus a global `ErrorHandler` and HTTP error interceptor funnelling into the notification component (Constitution VII.6/VII.7)
- [ ] T045 [P] Generate the typed API client from `docs/contracts/auth.v1.yaml` into `src/ProjectManagementApp.Web/src/app/core/api/generated/` via `openapi-generator` (`typescript-angular`); **adopt the generated DTO types only** — services stay hand-written (research.md R-6)

**Checkpoint**: Solution compiles, migration applies, API starts, Angular serves, and the shared kernel is
verified against `docs/shared-contracts.md`. User story work can begin.

---

## Phase 3: User Story 6 — Idempotent seed provisioning (Priority: P0) 🎯 unblocks everything

**Goal**: A freshly created database is immediately usable — three roles plus one Admin, one
ProjectManager, and one TeamMember exist after startup, and starting twice creates nothing new.

**Independent Test**: Point at an empty database and start the API twice. Assert exactly three roles and
three users exist, no duplicates, and a `UserSeeded` audit row per user with `actor_id = NULL`.

### Tests for User Story 6

- [ ] T046 [P] [US6] Write integration test in `tests/ProjectManagementApp.Infrastructure.Tests/Seeding/DataSeederTests.cs` asserting an empty database gains exactly 3 roles and 3 users, each with a hashed password
- [ ] T047 [P] [US6] Write idempotency test in `tests/ProjectManagementApp.Infrastructure.Tests/Seeding/DataSeederIdempotencyTests.cs` — run the seeder twice, assert zero duplicates; then delete one seeded user, re-run, assert **only** the missing one is recreated (partial-state repair)

### Implementation for User Story 6

- [ ] T048 [US6] Implement `DataSeeder : IDataSeeder` in `src/ProjectManagementApp.Infrastructure/Services/DataSeeder.cs` — ensure roles, then ensure one user per role via `UserManager`, guarded by existence checks **backed by the unique indexes** so concurrent startups resolve to one winner (research.md R-9)
- [ ] T049 [US6] Add `SeedOptions` binding for `Seed:Enabled` and `Seed:{Admin,ProjectManager,TeamMember}:{Email,Password}` in `src/ProjectManagementApp.Infrastructure/Services/SeedOptions.cs`, reading credentials from configuration only
- [ ] T050 [US6] Wire `db.Database.MigrateAsync()` followed by `IDataSeeder.SeedAsync()` into a startup scope in `src/ProjectManagementApp.Api/Program.cs`, gated by `Seed:Enabled` (default on in Development, off in Production)
- [ ] T051 [US6] Emit a `UserSeeded` audit row per seeded user with `actor_id = NULL` (system) from `src/ProjectManagementApp.Infrastructure/Services/DataSeeder.cs`
- [ ] T052 [US6] Verify no seed credential is hardcoded — grep the solution and confirm all seed passwords resolve from user-secrets/environment (Constitution V.4, quickstart V11)

**Checkpoint**: A fresh database boots into a usable, demonstrable state. US1 can now assign a role.

---

## Phase 4: User Story 1 — Register a new account (Priority: P0)

**Goal**: A visitor can self-register and receives a `TeamMember` account; the password is hashed, never
returned, and the write is audited.

**Independent Test**: `POST /api/auth/register` with a unique email returns 201 + `Location`, the body
contains no password field, the created user's role is `TeamMember`, and an `activity_logs` row exists.

### Tests for User Story 1

- [ ] T053 [P] [US1] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RegisterEndpointTests.cs` asserting 201, the `Location` header, a `UserDto` body with `role = TeamMember`, and **no `password`/`passwordHash` key in the response**
- [ ] T054 [P] [US1] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RegisterConflictTests.cs` asserting a duplicate email returns **409** and that `DANA@EXAMPLE.COM` also conflicts with `dana@example.com` (normalized-email uniqueness)
- [ ] T055 [P] [US1] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RegisterValidationTests.cs` asserting a policy-failing password returns **400** with per-field `errors` and persists nothing
- [ ] T056 [P] [US1] Write unit tests for `RegisterCommandValidator` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/RegisterCommandValidatorTests.cs` (required fields, email format, min length, password match)
- [ ] T057 [P] [US1] Write unit tests for `RegisterCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/RegisterCommandHandlerTests.cs` covering every conditional branch, **including that a client-supplied role is ignored and `TeamMember` is always assigned** (Constitution IX.1)

### Implementation for User Story 1

- [ ] T058 [US1] Create `RegisterCommand` and the `UserDto` response shape in `src/ProjectManagementApp.Application/Features/Auth/Register/` matching the `RegisterRequest`/`UserDto` schemas in `docs/contracts/auth.v1.yaml`
- [ ] T059 [US1] Implement `RegisterCommandValidator` in `src/ProjectManagementApp.Application/Features/Auth/Register/RegisterCommandValidator.cs` (FluentValidation, ADR-0005) — invoked automatically by `ValidationBehavior`, never called by the handler
- [ ] T060 [US1] Implement `RegisterCommandHandler` in `src/ProjectManagementApp.Application/Features/Auth/Register/RegisterCommandHandler.cs` — `UserManager.CreateAsync` (hashes), assign `TeamMember`, write the `UserRegistered` audit row in the same transaction, return `Result<UserDto>`; duplicate email → `ErrorKind.Conflict`
- [ ] T061 [US1] Add the thin `POST /api/auth/register` endpoint to `src/ProjectManagementApp.Api/Controllers/AuthController.cs` — `[AllowAnonymous]`, a single `MediatR.Send`, `.ToActionResult(onSuccess: 201)` with the `Location` header; **no logic in the controller** (Constitution II.2)
- [ ] T062 [P] [US1] Create the standalone register component in `src/ProjectManagementApp.Web/src/app/features/auth/register/` — Angular Material Reactive Form with required-name, email-format, min-length, and password-match validators, errors via the shared error-display component
- [ ] T063 [US1] Implement `AuthService.register()` in `src/ProjectManagementApp.Web/src/app/core/services/auth.service.ts` and add the lazy `auth` route group in `src/ProjectManagementApp.Web/src/app/features/auth/auth.routes.ts` (Constitution VII.1/VII.3)

**Checkpoint**: A user can be created end-to-end. Verifiable against quickstart V1–V3.

---

## Phase 5: User Story 2 — Log in and receive a token (Priority: P0) 🎯 MVP completes here

**Goal**: A registered user exchanges credentials for a short-lived JWT plus an httpOnly refresh cookie,
with a deliberately generic failure that leaks no account information.

**Independent Test**: `POST /api/auth/login` returns 200 with `{accessToken, expiresAt, user}` and a
`Set-Cookie` carrying `HttpOnly; Secure; SameSite=Strict`; the **body contains no refresh token**; a wrong
password and an unknown email produce byte-identical 401s.

### Tests for User Story 2

- [ ] T064 [P] [US2] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/LoginEndpointTests.cs` asserting 200, the JWT's `sub`/`email`/single `role` claim and `exp`, the `Set-Cookie` attributes, and **that no refresh token appears in the response body** (FR-016)
- [ ] T065 [P] [US2] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/LoginEnumerationTests.cs` asserting a wrong password and an unknown email return **identical** 401 bodies (no user enumeration)
- [ ] T066 [P] [US2] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/LoginDeactivatedTests.cs` asserting `is_active = false` yields 401 even with correct credentials (FR-004)
- [ ] T067 [P] [US2] Write unit tests for `LoginCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/LoginCommandHandlerTests.cs` covering success, bad credentials, and deactivated-user branches
- [ ] T068 [P] [US2] Write unit tests for `TokenService` in `tests/ProjectManagementApp.Application.Tests/Services/TokenServiceTests.cs` asserting claim set, configured expiry, and that the refresh token is returned raw but **stored hashed**

### Implementation for User Story 2

- [ ] T069 [US2] Implement `JwtOptions` binding and startup validation in `src/ProjectManagementApp.Api/Configuration/JwtOptions.cs` for `Jwt:SigningKey`, `Issuer`, `Audience`, `AccessTokenMinutes`, `RefreshTokenDays` (spec B.4)
- [ ] T070 [US2] Implement `TokenService : ITokenService` in `src/ProjectManagementApp.Infrastructure/Identity/TokenService.cs` — `CreateAccessToken` (signs the JWT with a single `role` claim) and `CreateRefreshToken` (opaque high-entropy value; caller persists only its SHA-256 hash)
- [ ] T071 [US2] Create `LoginCommand` and the `AuthTokens` result shape in `src/ProjectManagementApp.Application/Features/Auth/Login/` matching the `LoginRequest`/`AuthTokensResponse` schemas in the contract
- [ ] T072 [US2] Implement `LoginCommandValidator` in `src/ProjectManagementApp.Application/Features/Auth/Login/LoginCommandValidator.cs` (email present + format, password present)
- [ ] T073 [US2] Implement `LoginCommandHandler` in `src/ProjectManagementApp.Application/Features/Auth/Login/LoginCommandHandler.cs` — verify via Identity's hasher, reject inactive users, persist the hashed refresh token, write the `UserLoggedIn` audit row, and return a **generic** `ErrorKind.Unauthenticated` on any failure
- [ ] T074 [US2] Add the thin `POST /api/auth/login` endpoint to `src/ProjectManagementApp.Api/Controllers/AuthController.cs` writing the refresh token **only** as `Set-Cookie` (`HttpOnly; Secure; SameSite=Strict; Path=/api/auth`) and never into the body
- [ ] T075 [P] [US2] Create the standalone login component in `src/ProjectManagementApp.Web/src/app/features/auth/login/` — Material Reactive Form, generic failure message via the shared error-display component
- [ ] T076 [US2] Implement the NgRx auth actions, reducer, effects, and selectors in `src/ProjectManagementApp.Web/src/app/core/store/auth/` — the access token lives in memory only
- [ ] T077 [US2] Implement the functional JWT interceptor in `src/ProjectManagementApp.Web/src/app/core/interceptors/jwt.interceptor.ts` attaching `Authorization: Bearer` to outgoing requests and sending `withCredentials` for `/api/auth/*` (Constitution VII.4)

**Checkpoint**: 🎯 **MVP** — register, log in, receive a token. Verifiable against quickstart V1–V6.

---

## Phase 6: User Story 4 — Role-based route & endpoint protection (Priority: P0)

**Goal**: Every endpoint is authenticated by default, role-restricted endpoints enforce roles by attribute
only, and Angular guards are the sole client-side navigation block.

**Independent Test**: The 401/403 matrix holds across all three roles against a protected probe; the four
anonymous endpoints work without a token; a TeamMember is blocked from an Admin route by the guard **and**
still receives 403 if the request is forced.

### Tests for User Story 4

- [ ] T078 [P] [US4] Write the 401/403 matrix integration test in `tests/ProjectManagementApp.Api.Tests/Authorization/RoleMatrixTests.cs` — no token → 401; TeamMember → 403; ProjectManager → 403; Admin → 200 (Constitution IX.1)
- [ ] T079 [P] [US4] Write integration test in `tests/ProjectManagementApp.Api.Tests/Authorization/AnonymousEndpointTests.cs` asserting register/login/refresh/health succeed without a token and that **no other endpoint does** (FR-007)
- [ ] T080 [P] [US4] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/GetCurrentUserTests.cs` asserting `GET /api/auth/me` returns 200 with the token's identity and 401 without a token
- [ ] T081 [P] [US4] Write an architecture test in `tests/ProjectManagementApp.Api.Tests/Architecture/NoInlineRoleChecksTests.cs` failing the build if any controller or handler contains an ad-hoc role comparison — roles must be attribute-declared only (Constitution V.2, quickstart V8). **Also write the NFR-002 statelessness test** in `tests/ProjectManagementApp.Api.Tests/Authorization/StatelessAuthTests.cs` — hook an EF Core `DbCommand` interceptor into the `WebApplicationFactory`, call `GET /api/auth/me` with a valid token, and assert **zero** SQL statements execute; `GetCurrentUserQueryHandler` projects `UserDto` directly from `ICurrentUserService`'s token-derived claims, never from `IApplicationDbContext` (`/speckit.analyze` finding E1 — this claim in spec NFR-002 and plan.md's Performance Goals previously had no verifying task)

### Implementation for User Story 4

- [ ] T082 [US4] Verify and document the global fallback authorization policy in `src/ProjectManagementApp.Api/Program.cs` so an endpoint with no attribute still requires authentication, and confirm exactly four `[AllowAnonymous]` endpoints exist
- [ ] T083 [US4] Create `GetCurrentUserQuery` and its handler in `src/ProjectManagementApp.Application/Features/Auth/GetCurrentUser/` reading identity from `ICurrentUserService` — never from a parameter
- [ ] T084 [US4] Add the thin `GET /api/auth/me` endpoint to `src/ProjectManagementApp.Api/Controllers/AuthController.cs` with `[Authorize]`, matching the contract's `getCurrentUser` operation
- [ ] T085 [US4] Add an Admin-only probe endpoint in `src/ProjectManagementApp.Api/Controllers/` annotated `[Authorize(Roles = "Admin")]` to exercise the role matrix in tests
- [ ] T086 [P] [US4] Implement the functional auth guard in `src/ProjectManagementApp.Web/src/app/core/guards/auth.guard.ts` (`CanActivateFn`) redirecting unauthenticated users to login
- [ ] T087 [P] [US4] Implement the functional role guard in `src/ProjectManagementApp.Web/src/app/core/guards/role.guard.ts` (`CanMatchFn`) reading the role from NgRx so a lazy chunk is **not downloaded** when the role fails
- [ ] T088 [US4] Apply the guards in `src/ProjectManagementApp.Web/src/app/app.routes.ts` and confirm no component contains redirect logic — guards are the only navigation block (Constitution VII.5)

**Checkpoint**: RBAC is enforced end-to-end. Verifiable against quickstart V7, V8, V14.1.

---

## Phase 7: User Story 3 — Log out (Priority: P1)

**Goal**: A user can deliberately end a session; the refresh token is revoked server-side and the cookie
cleared, idempotently.

**Independent Test**: `POST /api/auth/logout` returns 204, sets `revoked_at`, and clears the cookie;
calling it again still returns 204; the revoked token can no longer refresh.

### Tests for User Story 3

- [ ] T089 [P] [US3] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/LogoutEndpointTests.cs` asserting 204, `revoked_at` set in the database, the cookie cleared, and a `UserLoggedOut` audit row written
- [ ] T090 [P] [US3] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/LogoutIdempotencyTests.cs` asserting logout with an already-expired or absent refresh token still succeeds
- [ ] T091 [P] [US3] Write unit tests for `LogoutCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/LogoutCommandHandlerTests.cs` covering the revoke and already-revoked branches

### Implementation for User Story 3

- [ ] T092 [US3] Create `LogoutCommand` in `src/ProjectManagementApp.Application/Features/Auth/Logout/LogoutCommand.cs` carrying the user id and the presented refresh token
- [ ] T093 [US3] Implement `LogoutCommandHandler` in `src/ProjectManagementApp.Application/Features/Auth/Logout/LogoutCommandHandler.cs` — set `RevokedAt`, write the `UserLoggedOut` audit row in the same transaction, and succeed idempotently when no live token is found
- [ ] T094 [US3] Add the thin `POST /api/auth/logout` endpoint to `src/ProjectManagementApp.Api/Controllers/AuthController.cs` with `[Authorize]`, reading the refresh cookie and emitting `Set-Cookie: refresh_token=; Max-Age=0` on 204
- [ ] T095 [P] [US3] Add the logout control to the app shell in `src/ProjectManagementApp.Web/src/app/core/` dispatching the NgRx logout action (Constitution VII — a `core/` singleton provided once)
- [ ] T096 [US3] Implement the NgRx logout effect in `src/ProjectManagementApp.Web/src/app/core/store/auth/` clearing auth state and routing to login

**Checkpoint**: Sessions can be deliberately ended. Verifiable against quickstart V10.

---

## Phase 8: User Story 5 — Token expiry & refresh (Priority: P0)

**Goal**: Short-lived access tokens are renewed transparently via single-use refresh-token rotation;
replayed, revoked, or expired tokens are rejected, and a deactivated user cannot refresh.

**Independent Test**: An expired access token triggers exactly **one** refresh call that returns a new
pair and rotates the cookie; replaying the previous refresh token returns 401.

### Tests for User Story 5

- [ ] T097 [P] [US5] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RefreshEndpointTests.cs` asserting 200, a new access token, a rotated `Set-Cookie`, and `replaced_by_token` linking old → new
- [ ] T098 [P] [US5] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RefreshReplayTests.cs` asserting a replayed (already-rotated) refresh token returns **401** (single-use rotation, FR-006)
- [ ] T099 [P] [US5] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RefreshRevokedTests.cs` asserting expired, revoked, and unknown refresh tokens each return 401
- [ ] T100 [P] [US5] Write integration test in `tests/ProjectManagementApp.Api.Tests/Auth/RefreshDeactivatedTests.cs` asserting a valid refresh token whose user was deactivated is denied (FR-004)
- [ ] T101 [P] [US5] Write unit tests for `RefreshCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/RefreshCommandHandlerTests.cs` covering valid, expired, revoked, replayed, and inactive-user branches
- [ ] T102 [P] [US5] Write an atomicity test in `tests/ProjectManagementApp.Infrastructure.Tests/Tokens/RotationAtomicityTests.cs` asserting revoke-old + insert-new + audit commit as one transaction — a failure mid-rotation must never leave two live tokens (data-model.md §5)

### Implementation for User Story 5

- [ ] T103 [US5] Add `ValidateRefreshTokenAsync` to `src/ProjectManagementApp.Infrastructure/Identity/TokenService.cs` — hash the presented value and look it up; return null when expired, revoked, or unknown
- [ ] T104 [US5] Create `RefreshCommand` in `src/ProjectManagementApp.Application/Features/Auth/Refresh/RefreshCommand.cs` carrying the presented refresh token (from the cookie, never the body)
- [ ] T105 [US5] Implement `RefreshCommandHandler` in `src/ProjectManagementApp.Application/Features/Auth/Refresh/RefreshCommandHandler.cs` — validate, reject inactive users, revoke the old token and set `ReplacedByToken`, issue a new pair, write the `TokenRefreshed` audit row, all in one transaction
- [ ] T106 [US5] Add the thin `POST /api/auth/refresh` endpoint to `src/ProjectManagementApp.Api/Controllers/AuthController.cs` as `[AllowAnonymous]` reading the cookie and emitting the rotated `Set-Cookie`
- [ ] T107 [US5] Implement the functional 401 interceptor in `src/ProjectManagementApp.Web/src/app/core/interceptors/error.interceptor.ts` — on 401 call refresh **once** with single-flight de-duplication, retry the original request, and dispatch logout on refresh failure (Constitution VII.4)
- [ ] T108 [P] [US5] Write a Jasmine test in `src/ProjectManagementApp.Web/src/app/core/interceptors/error.interceptor.spec.ts` asserting that several concurrent 401s produce **exactly one** refresh call (quickstart V14.3)
- [ ] T109 [US5] Add anti-forgery/CSRF protection for the cookie-authenticated `/api/auth/refresh` and `/api/auth/logout` endpoints in `src/ProjectManagementApp.Api/Program.cs` (FR-016)

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

- [ ] T110 [P] [US7] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ListUsersEndpointTests.cs` asserting an Admin sees all seeded users incl. any deactivated one (flagged `isActive:false`), and a non-Admin caller receives **403**
- [ ] T111 [P] [US7] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/GetUserByIdEndpointTests.cs` asserting a known id returns **200** with an `ETag` header, and an unknown id returns **404**
- [ ] T112 [P] [US7] Write unit tests for `ListUsersQueryHandler`/`GetUserByIdQueryHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/AdminUsersQueryHandlerTests.cs` covering the unscoped-list and unknown-id branches

### Implementation for User Story 7

- [ ] T113 [US7] Create `ListUsersQuery`/`GetUserByIdQuery` and the `AdminUserSummary`/`AdminUserDetail`/`PagedAdminUserSummary` DTOs in `src/ProjectManagementApp.Application/Features/Auth/ListUsers/` and `.../GetUserById/` matching `docs/contracts/auth.v1.yaml`
- [ ] T114 [US7] Implement `ListUsersQueryHandler` (paged, clamped `pageSize`, **no scope predicate** — Admin-only makes the role gate the entire authorization surface, spec US-001-07 7Cs) and `GetUserByIdQueryHandler` (404 if unknown) in the same folders
- [ ] T115 [US7] Create the **shared** `ETagExtensions` in `src/ProjectManagementApp.Api/Common/ETagExtensions.cs` — write the `xmin` row version as a strong `ETag` on responses, read/parse `If-Match` from requests, return **400** when required but absent (ADR-0007 §3; research.md R-15 — promoted here from 002's original plan, since 001 is the first feature that needs it; **002's T017/T018 now verify and reuse this file instead of creating a second one**). Then create `src/ProjectManagementApp.Api/Controllers/UsersController.cs` with the thin `GET /api/users` and `GET /api/users/{id}` endpoints — `[Authorize(Roles="Admin")]`, using `ETagExtensions` to write the `ETag` header on the detail response
- [ ] T116 [P] [US7] Build the admin users list component in `src/ProjectManagementApp.Web/src/app/features/auth/admin-users/list/` — a table with an "inactive" badge for deactivated users
- [ ] T117 [P] [US7] Build the admin user detail component in `src/ProjectManagementApp.Web/src/app/features/auth/admin-users/detail/` — hosts the role-change control (US8) and the status toggle (US9)
- [ ] T118 [US7] Implement `AdminUsersService` in `src/ProjectManagementApp.Web/src/app/core/services/admin-users.service.ts` and add the `admin-users` sub-route to the existing `auth` route group in `src/ProjectManagementApp.Web/src/app/features/auth/auth.routes.ts`, gated by the functional role guard (Admin-only)

**Checkpoint**: Admin can list and view every user. Verifiable against quickstart V15.

### Tests for User Story 8

- [ ] T119 [P] [US8] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserRoleEndpointTests.cs` asserting an Admin promoting a **different** user's role returns **200** and writes a `UserRoleChanged` (from→to) audit row
- [ ] T120 [P] [US8] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserRoleSelfTests.cs` asserting an Admin changing **their own** role returns **409**
- [ ] T121 [P] [US8] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserRoleLastAdminTests.cs` asserting a change that would leave **zero** Admins returns **409**, exercised at the handler level (research.md R-12 — under the current one-Admin seed, no distinct-caller HTTP path reaches this independently of the self-check)
- [ ] T122 [P] [US8] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserRoleConcurrencyTests.cs` asserting a missing `If-Match` returns **400** and a stale `If-Match` returns **409**. **Also unit-test the shared `ETagExtensions` itself** (round-trip, malformed value, absent-header 400) in `tests/ProjectManagementApp.Api.Tests/Common/ETagExtensionsTests.cs` — this is the one place that helper is unit-tested at all, now that it is created here rather than in 002 (research.md R-15)
- [ ] T123 [P] [US8] Write unit tests for `ChangeUserRoleCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/ChangeUserRoleCommandHandlerTests.cs` covering success, self-refusal, last-Admin-refusal, and same-role (400) branches — this is the primary place the last-Admin invariant is proven independently of the self-check (research.md R-12)

### Implementation for User Story 8

- [ ] T124 [US8] Create `ChangeUserRoleCommand` and `ChangeUserRoleCommandValidator` (role present, valid `Role` enum value) in `src/ProjectManagementApp.Application/Features/Auth/ChangeUserRole/`
- [ ] T125 [US8] Implement `ChangeUserRoleCommandHandler` in the same folder — self-check (`callerId == targetId` → 409) → last-Admin count check (post-change Admin count would be zero → 409, independent of the self-check per research.md R-12) → same-role check (400) → `UserManager.RemoveFromRoleAsync`/`AddToRoleAsync` → audit `UserRoleChanged` (from→to), all in one transaction
- [ ] T126 [US8] Add the thin `PUT /api/users/{id}/role` endpoint to `UsersController` — `[Authorize(Roles="Admin")]`, requires `If-Match`, writes a rotated `ETag` on success
- [ ] T127 [P] [US8] Add the role-change control (a role select + confirmation dialog) to the admin user detail component (T117), surfacing either 409 message verbatim

**Checkpoint**: Admin can change any other user's role, with both safety invariants enforced. Verifiable against quickstart V16.

### Tests for User Story 9

- [ ] T128 [P] [US9] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserStatusEndpointTests.cs` asserting an Admin deactivating a **different**, active user returns **200**, sets `is_active = false`, and revokes **every** active `refresh_tokens` row for that user
- [ ] T129 [P] [US9] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserStatusSelfTests.cs` asserting an Admin attempting to deactivate **themselves** returns **409**
- [ ] T130 [P] [US9] Write integration test in `tests/ProjectManagementApp.Api.Tests/Users/ChangeUserStatusReactivateTests.cs` asserting reactivating a deactivated user returns **200**, sets `is_active = true`, writes `UserReactivated`, and that a **pre-deactivation** refresh token remains **401** after reactivation (tokens stay revoked)
- [ ] T131 [P] [US9] Write unit tests for `ChangeUserStatusCommandHandler` in `tests/ProjectManagementApp.Application.Tests/Features/Auth/ChangeUserStatusCommandHandlerTests.cs` covering deactivate (incl. the bulk token revoke), reactivate, self-refusal, and same-status (400) branches

### Implementation for User Story 9

- [ ] T132 [US9] Create `ChangeUserStatusCommand` and `ChangeUserStatusCommandValidator` (`isActive` present) in `src/ProjectManagementApp.Application/Features/Auth/ChangeUserStatus/`
- [ ] T133 [US9] Implement `ChangeUserStatusCommandHandler` in the same folder — self-check on deactivation (409) → same-status check (400) → flip `IsActive`; **on deactivate**, set `RevokedAt = now` on every active `RefreshToken` row for the user — the **same** field `LogoutCommandHandler` (T093) and `RefreshCommandHandler` (T105) already use, not a new mechanism (research.md R-13) — then audit `UserDeactivated`; **on reactivate**, audit `UserReactivated` only (tokens stay revoked), all in one transaction
- [ ] T134 [US9] Add the thin `PUT /api/users/{id}/status` endpoint to `UsersController` — `[Authorize(Roles="Admin")]`, requires `If-Match`, writes a rotated `ETag` on success
- [ ] T135 [P] [US9] Add the deactivate/reactivate toggle to the admin user detail component (T117) — a confirmation dialog naming the user and, for deactivation, warning that active sessions end immediately

**Checkpoint**: Admin user management is complete — list/view, role change, and deactivate/reactivate all
work end-to-end with their safety invariants enforced. Verifiable against quickstart V15–V18.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Deliverables, hardening, and proving the gates actually work.

- [ ] T136 [P] Write the repository `README.md` with overview, prerequisites, backend/frontend setup, migration commands, test commands, and end-to-end run instructions (Constitution X.1)
- [ ] T137 [P] Generate the entity-relationship diagram from `InitialCreate` and commit it to `docs/erd.md` (Constitution X.4)
- [ ] T138 **Prove the contract gate fails**: temporarily rename a response property (e.g. `accessToken` → `token`), run `dotnet build -p:CheckApiContract=true`, confirm the build **fails** with an `oasdiff` breaking report, then revert (quickstart V13)
- [ ] T139 Execute the full quickstart validation V1–V18 in `specs/001-auth-rbac/quickstart.md` and record results (extended 2026-08-05 to include V15–V18, the Admin user-management scenarios)
- [ ] T140 [P] Add the CI pipeline running restore → build with `-p:CheckApiContract=true` → `dotnet test` → `npm test`, failing the merge on any failure (Constitution IX.3)
- [ ] T141 [P] Add an architecture test in `tests/ProjectManagementApp.Application.Tests/Architecture/LayerDependencyTests.cs` asserting Domain references no project, and Application references neither Infrastructure nor Api (research.md R-1)
- [ ] T142 [P] Audit Serilog output across all endpoints confirming no password, raw refresh token, or signing key is ever logged (Constitution V.3, NFR-003)
- [ ] T143 [P] Add test-data builders/factories in `tests/ProjectManagementApp.Application.Tests/Builders/` replacing any inline object literals (Constitution IX.4)
- [ ] T144 [P] Add XML doc comments to public controllers, handlers, and service interfaces (Constitution VIII.3)
- [ ] T145 Remove all commented-out code, `Console.WriteLine`, and `console.log` across `src/` (Constitution VIII.4)
- [ ] T146 [P] Verify the Angular production build (`ng build --configuration production`) emits into the API's `wwwroot/` and the app runs same-origin (ADR-0002, Constitution XI.1)
- [ ] T147 [P] Write IIS deployment instructions in `docs/deployment.md` covering the self-contained publish and `appsettings.{Environment}.json` (Constitution XI.1/XI.3)
- [ ] T148 Run a security review against spec 001 §Security Rules — deny-by-default, attribute-only role gates, hashed passwords and refresh tokens, secrets absent from source control, **including the Admin user-management endpoints added 2026-08-05** (self-restriction and last-Admin invariants, `If-Match` enforcement, bulk token revocation on deactivation)

- [ ] T149 Verify [`docs/adr/0007-implementation-conventions.md`](../../docs/adr/0007-implementation-conventions.md) still describes what was actually built — the contract drift gate, Testcontainers-only test database, `ETag`/`If-Match` concurrency transport, and builder-based test fixtures — and amend the ADR if the implementation diverged (Constitution X.3)

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
- **T138 is not optional ceremony.** A contract gate that has never been observed to fail is
  indistinguishable from one that does not work.
- **Do not build a second token-revocation mechanism for US9.** Deactivation reuses `RefreshToken.RevokedAt`
  — the exact field US-001-03 (logout, T093) and US-001-05 (refresh rotation, T105) already set. A new
  flag or column would create two independent places a token's validity depends on (research.md R-13).
- **T115 *does* create the shared `ETagExtensions` helper** (`src/ProjectManagementApp.Api/Common/ETagExtensions.cs`) — this reverses R-14's original inline-only decision. **002's T017/T018 have been corrected to verify/reuse it, not recreate it** (research.md R-15). If 002's tasks.md is ever regenerated from scratch, re-apply this correction rather than letting T017 silently re-create a second implementation.

---

## Notes

- **[P]** = different files, no dependency on an incomplete task
- **[Story]** label maps each task to a spec story for traceability
- Tests are written before implementation within each story; verify they fail first
- Commit after each task or logical group, using Conventional Commits (Constitution VIII.5)
- Every checkpoint is a safe stopping point for validation or demo
- Constitution references are inline so a reviewer can check compliance without leaving this file
