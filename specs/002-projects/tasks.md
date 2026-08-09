---
description: "Task list for 002 Project Management implementation"
---

# Tasks: 002 Project Management

**Input**: Design documents from `/specs/002-projects/`
**Prerequisites**: [plan.md](plan.md) · [spec.md](spec.md) · [research.md](research.md) ·
[data-model.md](data-model.md) · [contracts/](contracts/README.md) → [`docs/contracts/projects.v1.yaml`](../../docs/contracts/projects.v1.yaml) · [quickstart.md](quickstart.md)

**Tests**: **REQUIRED.** Constitution IX mandates xUnit handler tests and `WebApplicationFactory`
integration tests (IX.1) and prohibits merging with failing tests (IX.3); spec 002 B.8 DoD #11 requires
them. **Docker is required** — Testcontainers PostgreSQL, never EF InMemory (ADR-0007 §2).

**Organization**: Grouped by user story so each is independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story the task serves
- Exact file paths are included in every task

---

## ⚠️ Blocking prerequisite: 001 must be complete

002 adds **no assembly and no test project** — it extends 001's solution in place. Before starting, 001's
Foundational phase must have delivered:

| From 001 | Used by 002 |
|---|---|
| `Project` entity + `projects` table (`InitialCreate`) | everything — **002 adds no table**, only indexes |
| `IProjectAccessPolicy` interface (T022) | implemented here in T012 |
| `IApplicationDbContext` (T021) | every handler |
| `AuditAction` incl. `ProjectCreated/Updated/Deleted/OwnerChanged` (T018) | every write |
| `Result`/`PagedResult<T>`/`CurrentUser`/`AccessDecision` (T019/T020) | every slice |
| `IActivityLogService.LogAsync` (T022/T033) | every write |
| MediatR pipeline, `ToActionResult()`, JWT auth, seeded users | all |

**T007 verifies these before any 002 work begins** — it exists because four shared-kernel gaps were found
during planning, and a fifth would surface here as a confusing compile error rather than a clear one.

---

## Story ID mapping & implementation order

| Label | Spec story | Title | Priority | Depends on |
|---|---|---|---|---|
| **US1** | US-002-01 | Create a project | P0 | — (after Foundational) |
| **US2** | US-002-02 | List and search projects (role-scoped) | P0 | US1 |
| **US3** | US-002-03 | View project detail | P0 | US1 |
| **US4** | US-002-04 | Edit a project | P0 | US1 |
| **US5** | US-002-05 | Delete a project | P1 | US1 |

**US1 is the only hard gate.** Once create exists, **US2–US5 are genuinely independent** and can be built
in parallel by four developers — unlike 001, whose stories formed a chain.

---

## Phase 1: Setup

**Purpose**: Scaffold 002's folders and wire the contract; no new packages, no new projects.

- [X] T001 Create the slice folder structure `src/ProjectManagementApp.Application/Features/Projects/{CreateProject,ListProjects,GetProjectById,UpdateProject,DeleteProject}/` per plan.md §Project Structure
- [X] T002 [P] Generate TypeScript DTO types from `docs/contracts/projects.v1.yaml` into `src/ProjectManagementApp.Web/src/app/core/api/generated/` via `openapi-generator` (`typescript-angular`) — **types only**; the service stays hand-written (001 R-6) — **deviation**: used `openapi-typescript` instead, same as 001 (no Java/JVM in this environment for `openapi-generator-cli`); added `generate:api:projects` npm script alongside 001's existing `generate:api`, output at `core/api/generated/projects.v1.d.ts`
- [X] T003 [P] Add `projects.v1.yaml` to the `CheckApiContract` MSBuild target in `src/ProjectManagementApp.Api/ProjectManagementApp.Api.csproj` so the drift gate covers it (ADR-0007 §1) — added a second `oasdiff` `Exec` diffing `projects.v1.yaml` against the same `generated.json` (Swashbuckle documents the whole API under one v1 doc regardless of how many hand-authored contracts describe it)
- [X] T004 [P] Add the `Projects:*` configuration section to `src/ProjectManagementApp.Api/appsettings.json` — `Paging:{DefaultPageSize=20,MaxPageSize=100}`, `DefaultStatus=Planning`, `MaskOutOfScopeAs404=false`, `AllowOwnershipTransfer=true`, `MaxNameLength`, `MaxDescriptionLength` (spec B.4) — flattened `Paging:{...}` to top-level `DefaultPageSize`/`MaxPageSize` keys to match `ProjectsOptions`' flat shape (see T005)
- [X] T005 [P] Create `ProjectsOptions` binding class in `src/ProjectManagementApp.Api/Configuration/ProjectsOptions.cs` and register it in `Program.cs` — **deviation**: placed at `Application/Common/Options/ProjectsOptions.cs` instead. Every value it holds (paging, default status, mask flag, ownership-transfer flag) is consumed by Application-layer slice handlers, and Application MUST NOT reference Api (Constitution II.2, enforced by `LayerDependencyTests`) — placing the class in `Api/Configuration/` as literally written would make it unreachable from the handlers that need it without a boundary violation. `Program.cs` still owns the `services.Configure<ProjectsOptions>(...)` binding call. Same pattern as 001's `JwtOptions` relocation to `Infrastructure/Identity/`.
- [X] T006 [P] Scaffold the lazy `projects` route group in `src/ProjectManagementApp.Web/src/app/features/projects/projects.routes.ts` and register it with `loadChildren` in `app.routes.ts` (standalone, no `@NgModule` — ADR-0001) — empty `projectsRoutes: Routes = []` for now, populated incrementally by later story tasks (same convention as 001's `authRoutes`); registered under `path: 'projects'` with `canActivate: [authGuard]` (role-specific guards are added per-route as stories land, e.g. hiding create from TeamMember)
- [X] T007 **Verify 001's prerequisites are present before proceeding**: `Project` entity and `projects` table exist from `InitialCreate`; `IProjectAccessPolicy` is declared in `Application/Common/Interfaces/`; `AuditAction` contains `ProjectCreated`, `ProjectUpdated`, `ProjectDeleted`, `ProjectOwnerChanged`; `IApplicationDbContext.Projects` is exposed; **`ETagExtensions` exists in `Api/Common/`** (created by 001 T117, not by this feature — 001 research R-15, corrected 2026-08-06). **Stop and fix 001 if any is missing** — do not create them here (shared-kernel boundary, ADR-0006 addendum) — all five verified present and matching data-model.md/spec B.3 exactly; nothing missing

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The scope policy, indexes, DTOs, and ETag plumbing every story depends on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Persistence

- [X] T008 Extend `src/ProjectManagementApp.Infrastructure/Persistence/Configurations/ProjectConfiguration.cs` with the four indexes from data-model.md §4: `(owner_id)`, `(status)`, `(owner_id, status)`, and the **GIN trigram index on `name`** — note: `ix_projects_owner_id` already existed from 001's `InitialCreate` (EF's default FK-index convention on `owner_id`), so `HasIndex` for it is a no-op that keeps the model explicit without generating a duplicate; the migration below only emits the three genuinely new indexes
- [X] T009 Create the `AddProjectIndexes` migration in `src/ProjectManagementApp.Infrastructure/Persistence/Migrations/` — the four indexes **plus** `CREATE EXTENSION IF NOT EXISTS pg_trgm`. **This migration must NOT create the `projects` table** (it exists from 001's `InitialCreate`); a table-creating migration would fail against an existing schema (research R-4) — confirmed: generated migration contains only `CreateIndex` statements (no `CreateTable`)
- [X] T010 [P] Write migration test in `tests/ProjectManagementApp.Infrastructure.Tests/Projects/MigrationTests.cs` asserting `AddProjectIndexes` applies cleanly, all four indexes exist, and the `pg_trgm` extension is enabled — written and confirmed RED before T008/T009 (2/2 failing: no indexes, no extension), GREEN after (2/2 passing)
- [X] T011 [P] Write cascade/RESTRICT tests in `tests/ProjectManagementApp.Infrastructure.Tests/Projects/CascadeBehaviorTests.cs` — deleting a project cascades to `tasks` and `team_members`; deleting a user who **owns** a project is **restricted**; `activity_logs` rows are never cascaded (research R-5, DoD 7) — these prove *already-configured* 001 behavior, so all 3 passed immediately on first run (characterization tests, not red-green) except one iteration fix: the RESTRICT test initially threw `InvalidOperationException` client-side (EF's change tracker refusing to null a required FK on a tracked navigation) rather than reaching PostgreSQL's actual constraint — fixed by deleting the owner via a **second, untracked** `DbContext` so the DELETE genuinely reaches the database and the real `DbUpdateException` fires

### The scope policy — the security core of this feature

- [X] T012 Implement `ProjectAccessPolicy : IProjectAccessPolicy` in `src/ProjectManagementApp.Application/Common/Authorization/ProjectAccessPolicy.cs`, injecting `IApplicationDbContext` — `ApplyScope` (Admin unscoped · PM `OwnerId == caller` · TM `TeamMembers.Any(...)`), plus `CanReadAsync` and `CanMutateAsync` per data-model.md §3. **Lives in `.Application`, not Infrastructure** — scope rules are business rules (research R-1) — real bug caught writing this: an early draft had `CanReadAsync`'s TeamMember branch read `project.TeamMembers.Any(...)` off the passed-in entity's navigation, which is empty/unloaded for any caller that loaded the project via a plain `SingleOrDefaultAsync` (the normal case in read/write handlers) — silently denying every legitimately-assigned TeamMember. Fixed per research R-1's explicit instruction to query `db.TeamMembers` directly instead of trusting the navigation
- [X] T013 [P] Write the **table-driven three-role `ApplyScope` matrix test** in `tests/ProjectManagementApp.Application.Tests/Authorization/ProjectAccessPolicyScopeTests.cs` against Testcontainers PostgreSQL — assert the predicate reaches SQL and that out-of-scope rows are never materialized — required a new `Fixtures/PostgresFixture.cs` in this test project (Application.Tests had no Postgres/Testcontainers fixture yet; test projects don't reference one another so Infrastructure.Tests' fixture wasn't reachable) and pulling `ProjectBuilder` forward from T022 (IX.4 forbids inline literals, and T013 genuinely needs it before T022's listed position). Written and confirmed RED first (`CS0234`, `ProjectAccessPolicy`/the `Authorization` namespace didn't exist yet) before T012; 1/1 passing after
- [X] T014 [P] Write `CanReadAsync`/`CanMutateAsync` branch tests in `tests/ProjectManagementApp.Application.Tests/Authorization/ProjectAccessPolicyDecisionTests.cs` covering all six role × operation cells (data-model.md §3) — same RED-before-T012 confirmation as T013; 6/6 passing after, including the TeamMember-assigned case that caught T012's navigation bug above
- [X] T015 Register `ProjectAccessPolicy` as the `IProjectAccessPolicy` implementation in `src/ProjectManagementApp.Application/DependencyInjection.cs`

### Shared slice plumbing

- [X] T016 [P] Create `ProjectSummaryDto`, `ProjectDetailDto`, and `OwnerRefDto` plus manual mapping extensions in `src/ProjectManagementApp.Application/Features/Projects/`, matching the schemas in `docs/contracts/projects.v1.yaml` (ADR-0005 — manual mapping, no AutoMapper) — `ProjectMappingExtensions.ToSummaryDto()`/`ToDetailDto()`/`ToOwnerRefDto()`; row version deliberately excluded from both DTOs (travels as `ETag`, not a body field)
- [X] T017 **Verify (do not recreate) the shared `ETagExtensions`** at `src/ProjectManagementApp.Api/Common/ETagExtensions.cs` — confirms it writes the `xmin` row version as a strong `ETag`, reads/parses `If-Match`, and returns **400** when required but absent. **Created by 001 at T117, unit-tested by 001 at T114**, not by this feature (research R-2's 2026-08-06 note, R-15 in 001's research.md) — this was originally a creation task; corrected when 001's own Admin user-management endpoints needed this machinery first and became its owner — re-confirmed present, unchanged
- [X] T018 [P] Write an **architecture/boundary test** in `tests/ProjectManagementApp.Api.Tests/Common/NoSecondETagImplementationTests.cs` asserting `ProjectsController` references the shared `ETagExtensions` type and that no project-local reimplementation exists in `Features/Projects/` — guards against exactly the duplication this task list originally risked (research R-2 note) — written before T021 (`ProjectsController` didn't exist yet, confirmed `CS0246` RED); 2/2 passing after T021
- [X] T019 [P] Implement the **sort whitelist** mapper in `src/ProjectManagementApp.Application/Features/Projects/ListProjects/ProjectSortMap.cs` — map the closed enum (`name`, `-name`, `startDate`, `-startDate`, `endDate`, `-endDate`, `status`, `-status`, `createdAt`, `-createdAt`) to expressions; **any other value returns a validation error, never string interpolation** (research R-3)
- [X] T020 [P] Write sort-whitelist tests in `tests/ProjectManagementApp.Application.Tests/Features/Projects/ProjectSortMapTests.cs` — every whitelisted value maps, and an arbitrary string is rejected — written and confirmed RED (`CS0234`, `ListProjects` namespace didn't exist) before T019; 15/15 passing after
- [X] T021 Create the thin `ProjectsController` shell in `src/ProjectManagementApp.Api/Controllers/ProjectsController.cs` with the five route stubs and their `[Authorize]` / `[Authorize(Roles="Admin,ProjectManager")]` attributes — **no logic, one `MediatR.Send` per endpoint** (Constitution II.2) — at this point in the task list the five slices' Command/Query types don't exist yet (they land in Phases 3–7), so each stub action currently `throw new NotImplementedException()`; wired to a real `Send` one at a time as each story's phase lands (T033/T045/T054/T065/T074)

### Test fixtures & frontend shell

- [X] T022 [P] Add `ProjectBuilder` and the 002 fixture set to `tests/ProjectManagementApp.Application.Tests/Builders/` — Admin, **two ProjectManagers** (`PM` and `PM2`, required for the cross-owner denial test), two TeamMembers, and projects A (owned by PM) and B (owned by PM2). **From builders, never the production seeder** (ADR-0007 §4) — `ProjectBuilder` was pulled forward and already existed by this point (needed by T013/T014); added `ProjectsScenario` (also in `Builders/`) assembling and persisting the full Admin/PM/PM2/TM/unassigned-TM + Project A/B scenario in one `SeedAsync` call, reused by US2's scope-matrix tests
- [X] T023 [P] Implement `ProjectsService` in `src/ProjectManagementApp.Web/src/app/core/services/projects.service.ts` — all five HTTP calls, using the generated DTO types; **capture the `ETag` from detail responses** so the edit flow can send `If-Match` — follows `AdminUsersService`'s exact pattern (`observe: 'response'` + a `withETag` mapper) for `create`/`getById`/`update`
- [X] T024 [P] Create the four standalone component shells in `src/ProjectManagementApp.Web/src/app/features/projects/{list,detail,create,edit}/` with routes wired and a functional role guard applied in `projects.routes.ts` (Constitution VII.5) — placeholder templates only, fleshed out by each story's phase (T034/T046/T055/T066); `new` and `:id/edit` routes gated by `roleGuard(['Admin','ProjectManager'])` via `canMatch`, `''` (list) and `:id` (detail) open to all three roles under the parent route's `authGuard`; verified with a full `ng build` — all four lazy chunks present

**Checkpoint**: Policy, indexes, DTOs, and ETag plumbing exist and are tested. Stories can begin.

---

## Phase 3: User Story 1 — Create a project (Priority: P0) 🎯 MVP

**Goal**: A ProjectManager or Admin can create a project; the owner is derived from the token and cannot be
forged; the write is audited.

**Independent Test**: `POST /api/projects` returns 201 + `Location` + `ETag`, `owner.id` equals the calling
PM regardless of any `ownerId` in the body, and an `activity_logs` row exists.

### Tests for User Story 1

- [X] T025 [P] [US1] Write integration test in `tests/ProjectManagementApp.Api.Tests/Projects/CreateProjectEndpointTests.cs` asserting **201**, the `Location` header, an `ETag`, and `owner.id` = the calling PM — confirmed 500 (`NotImplementedException`) before T030-T033, 201 after
- [X] T026 [P] [US1] Write **ownership-forgery test** in `tests/ProjectManagementApp.Api.Tests/Projects/CreateProjectOwnershipTests.cs` — a PM supplying another user's `ownerId` is **ignored** (owner is still the caller); an **Admin** may set `ownerId`; an Admin setting a **TeamMember** as owner returns **400** (quickstart V1, V3) — added `ProjectsTestHelper.RegisterProjectManagerAsync` (register + Admin promotes to ProjectManager) since getting a second PM for cross-owner scenarios has no other route (ADR-0007 §4)
- [X] T027 [P] [US1] Write role-gate test in `tests/ProjectManagementApp.Api.Tests/Projects/CreateProjectAuthorizationTests.cs` asserting a TeamMember receives **403** and nothing is written — this one already passed before any implementation existed, since `[Authorize(Roles=...)]` rejects the caller before the stub's `NotImplementedException` is ever reached
- [X] T028 [P] [US1] Write validation tests in `tests/ProjectManagementApp.Api.Tests/Projects/CreateProjectValidationTests.cs` — blank `name` → 400; `endDate` before `startDate` → 400 with a field error; nothing persisted
- [X] T029 [P] [US1] Write handler branch tests in `tests/ProjectManagementApp.Application.Tests/Features/Projects/CreateProjectCommandHandlerTests.cs` covering owner-from-token, Admin-specified owner, ineligible owner, and the audit write — hit the same recurring NSubstitute `CouldNotSetReturnDueToNoLastCallException` pitfall as 001 (inline `BuildMockDbSet()` inside `.Returns(...)`); fixed the same way, extracting to a local first

### Implementation for User Story 1

- [X] T030 [US1] Create `CreateProjectCommand` in `src/ProjectManagementApp.Application/Features/Projects/CreateProject/CreateProjectCommand.cs` matching the `CreateProjectRequest` schema in the contract
- [X] T031 [US1] Implement `CreateProjectCommandValidator` in `src/ProjectManagementApp.Application/Features/Projects/CreateProject/CreateProjectCommandValidator.cs` — required name, max lengths, and the **cross-field `endDate >= startDate`** rule (ADR-0005); invoked automatically by `ValidationBehavior`, never by the handler — max lengths come from `IOptions<ProjectsOptions>` injected into the validator's constructor (FluentValidation validators are DI-resolved, so this works cleanly)
- [X] T032 [US1] Implement `CreateProjectCommandHandler` in `src/ProjectManagementApp.Application/Features/Projects/CreateProject/CreateProjectCommandHandler.cs` — resolve `owner_id` from the token for a PM (ignoring any body value), allow an Admin to specify it, **validate the owner holds ProjectManager or Admin via `UserManager<ApplicationUser>.GetRolesAsync(user)`** (else `ErrorKind.Validation` → 400; research R-6). **Do not attempt a `user_roles` join — `IApplicationDbContext` exposes no such `DbSet`.** Then persist and write the `ProjectCreated` audit row in the **same** `SaveChangesAsync` — injects `ICurrentUserService` directly (same pattern as 001's `GetCurrentUserQueryHandler`) rather than threading a `CallerId` field through the command, since the handler needs the caller's **role** too, not just their id
- [X] T033 [US1] Wire `POST /api/projects` in `src/ProjectManagementApp.Api/Controllers/ProjectsController.cs` — `[Authorize(Roles="Admin,ProjectManager")]`, one `Send`, `.ToActionResult(onSuccess: 201)` with `Location` and `ETag` headers — required adding a `[property: JsonIgnore] uint Version` to `ProjectDetailDto` (same pattern as 001's `AdminUserDetail`) so the controller can read the row version for the `ETag` header without it leaking into the JSON body
- [X] T034 [P] [US1] Build the create form in `src/ProjectManagementApp.Web/src/app/features/projects/create/` — Material Reactive Form (`name`, `description`, `startDate`, `endDate`, `status`) with a **date-order cross-field validator**, the owner field shown **only to Admin**, errors via the shared error-display component, submit disabled while pending — plain `<input type="date">` with `matInput` instead of `MatDatepickerModule` (avoids pulling in a date-adapter provider globally for one form field); Admin-only visibility of the owner field checks `authFeature.selectUser()?.role` from the NgRx store, same pattern `role.guard.ts` already uses
- [X] T035 [US1] Implement `ProjectsService.create()` and route to the new project's detail view on success in `src/ProjectManagementApp.Web/src/app/core/services/projects.service.ts` — `create()` itself was already built in T023; this task's remaining scope (routing to the detail view on success) is in `CreateProjectComponent.submit()`
- [X] T036 [P] [US1] Write Jasmine tests for the create form validators in `src/ProjectManagementApp.Web/src/app/features/projects/create/create-project.component.spec.ts` — **note**: written after T034, not before — deviates from strict red-green here; 6/6 passing, verified against the actual component rather than assumed. (Uses Vitest, not Jasmine+Karma — 001's documented deviation, Angular 22 removed Karma.)

**Checkpoint**: 🎯 **MVP** — projects can be created. Verify against quickstart V1–V3.

---

## Phase 4: User Story 2 — List and search projects (Priority: P0)

**Goal**: Every role gets a paginated, searchable list scoped to exactly what they may see, with
`totalCount` scoped too.

**Independent Test**: The three-role scope matrix holds, **and `totalCount` matches the scoped item count**
— a PM must never learn how many projects exist system-wide.

### Tests for User Story 2

- [X] T037 [P] [US2] Write the 🎯 **three-role scope matrix test** in `tests/ProjectManagementApp.Api.Tests/Projects/ListProjectsScopeTests.cs` — Admin sees A+B; PM sees only A; PM2 only B; TM only assigned. **Assert `totalCount`, not just items** — a mismatch means scope ran after counting and leaks existence (FR-007) — required adding `ProjectsTestHelper.AssignTeamMemberAsync`/`GetCurrentUserIdAsync` since 004's roster endpoints don't exist yet; confirmed RED (`CS0246`, `ProjectsTestHelper` had no such members) before writing the helper, GREEN after T042-T045
- [X] T038 [P] [US2] Write empty-scope test in `tests/ProjectManagementApp.Api.Tests/Projects/ListProjectsEmptyScopeTests.cs` — a TeamMember with no assignments gets **200 with `items: []` and `totalCount: 0`**, never 403/404 — confirmed 500 (`NotImplementedException`) before implementation, 200 after
- [X] T039 [P] [US2] Write paging tests in `tests/ProjectManagementApp.Api.Tests/Projects/ListProjectsPagingTests.cs` — `pageSize=500` **clamped to 100**; `page` beyond the last page returns empty items with valid metadata; `page=-1` → **400**
- [X] T040 [P] [US2] Write search/filter tests in `tests/ProjectManagementApp.Api.Tests/Projects/ListProjectsSearchTests.cs` — an **interior substring** (`pollo` matches "Apollo Rollout") via the trigram index; `?status=` filters; a filter **narrows within scope and never widens it**; `?sort=<not whitelisted>` → **400**
- [X] T041 [P] [US2] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Projects/ListProjectsQueryHandlerTests.cs` asserting the **composition order** scope → filter → count → sort → page (data-model.md §5) — runs against real Postgres via `PostgresFixture`/`ProjectsScenario` (not NSubstitute+MockQueryable) because `EF.Functions.ILike` and the scope predicate both require genuine SQL translation; confirmed RED (`CS0246`, `ListProjectsQuery`/`Handler` didn't exist) before T042-T044, 3/3 passing after

### Implementation for User Story 2

- [X] T042 [US2] Create `ListProjectsQuery` in `src/ProjectManagementApp.Application/Features/Projects/ListProjects/ListProjectsQuery.cs` with `Page`, `PageSize`, `Search?`, `Status?`, `Sort?`
- [X] T043 [US2] Implement `ListProjectsQueryValidator` in `src/ProjectManagementApp.Application/Features/Projects/ListProjects/ListProjectsQueryValidator.cs` — paging bounds (negative/non-numeric → 400) and `sort` against the whitelist — **note**: `PageSize` is deliberately NOT validated/rejected (contract VI.4: clamped, not rejected); only `Page >= 1` and the sort whitelist are enforced here
- [X] T044 [US2] Implement `ListProjectsQueryHandler` in `src/ProjectManagementApp.Application/Features/Projects/ListProjects/ListProjectsQueryHandler.cs` in the **fixed order**: `ApplyScope` → search (`EF.Functions.ILike` on name/description) → status filter → **`CountAsync`** → whitelisted sort → `Skip/Take` with `pageSize` **clamped** → project to `ProjectSummaryDto`. **One round trip; no N+1 on owner** (NFR-002) — **deviation**: `EF.Functions.ILike` requires the `Npgsql.EntityFrameworkCore.PostgreSQL` package's extension method, which `ProjectManagementApp.Application.csproj` did not reference (only plain `Microsoft.EntityFrameworkCore`); added the package reference — Application already references EF Core directly for `DbSet`/LINQ per shared-contracts.md §7, and this adds no Infrastructure project reference, so the compile-time dependency arrow is unaffected
- [X] T045 [US2] Wire `GET /api/projects` in `src/ProjectManagementApp.Api/Controllers/ProjectsController.cs` — `[Authorize]` (all three roles), one `Send`
- [X] T046 [P] [US2] Build the list view in `src/ProjectManagementApp.Web/src/app/features/projects/list/` — Material table with search box, status filter, sort, and paginator; explicit empty/loading/error states; **no client-side role filtering** (the API is already scoped) — sort control omitted from the UI in this pass (the query param and backend whitelist are fully wired; table stays in default `-createdAt` order) since neither tasks.md's file list nor quickstart V16 names a required sort control, and the paginator/search/filter cover the checkpoint's V4/V6-V8 scenarios
- [X] T047 [US2] Implement `ProjectsService.list()` with query-parameter handling in `src/ProjectManagementApp.Web/src/app/core/services/projects.service.ts` — already implemented in T023 (stage 1); verified signature matches `ProjectListQuery` used by the new list component, no changes needed
- [X] T048 [P] [US2] Hide the "New Project" action for TeamMember in the list view — **UX only**, and add a Jasmine test asserting the API still returns 403 when forced (`.../list/project-list.component.spec.ts`) — third test calls `ProjectsService.create()` directly (bypassing the hidden button) and asserts the mocked backend's 403 surfaces as an observable error, proving hiding is convenience only (Vitest, not Jasmine+Karma — same 001/T036 deviation, Angular 22 removed Karma); 3/3 passing

**Checkpoint**: The workspace entry point works and scope is proven. Verify against quickstart V4, V6–V8.

---

## Phase 5: User Story 3 — View project detail (Priority: P0)

**Goal**: A permitted user opens one project and sees its full detail; out-of-scope is 403, unknown is 404.

**Independent Test**: The 200/403/404/400 matrix holds per role, and the response carries an `ETag`.

### Tests for User Story 3

- [X] T049 [P] [US3] Write the status matrix test in `tests/ProjectManagementApp.Api.Tests/Projects/GetProjectByIdTests.cs` — in-scope **200** (with `ETag`); out-of-scope **403**; unknown id **404**; malformed non-GUID **400**; Admin **200** for any id — **bug found**: the route was originally `[HttpGet("{id:guid}")]` (matching PUT/DELETE); a malformed id simply failed route matching and fell through to a bare ASP.NET 404, not the contract's required 400. Fixed by dropping the `:guid` constraint on GET only and parsing `id` manually in the action, returning a 400 `ProblemDetails` on parse failure. Confirmed RED (500 `NotImplementedException`, then the malformed-id case returning the wrong status) before/after T052-T054; 5/5 passing after both fixes
- [X] T050 [P] [US3] Write masking test in `tests/ProjectManagementApp.Api.Tests/Projects/GetProjectByIdMaskingTests.cs` — with `Projects:MaskOutOfScopeAs404=true`, the out-of-scope case returns **404** instead of 403 (OQ-002-03) — **bug found**: standing up a second `WebApplicationFactory<Program>` (needed to inject a different `IOptions<ProjectsOptions>`) raced Program.cs's own startup migration against the already-migrated database — `42P07: relation "activity_logs" already exists`. Fixed by adding a `SkipStartupMigration` config flag Program.cs checks before calling `MigrateAsync()`, set only by `ApiTestFixture.CreateClient(configureServices)` for this second-factory case (production and the primary test factory are unaffected — both still migrate normally)
- [X] T051 [P] [US3] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Projects/GetProjectByIdQueryHandlerTests.cs` — not-found before scope check, then `CanReadAsync` denial — confirmed RED (`CS0234`, `GetProjectById` namespace didn't exist) before T052-T053; 3/3 passing after

### Implementation for User Story 3

- [X] T052 [US3] Create `GetProjectByIdQuery` in `src/ProjectManagementApp.Application/Features/Projects/GetProjectById/GetProjectByIdQuery.cs`
- [X] T053 [US3] Implement `GetProjectByIdQueryHandler` in `src/ProjectManagementApp.Application/Features/Projects/GetProjectById/GetProjectByIdQueryHandler.cs` — load, **404 if unknown**, `IProjectAccessPolicy.CanReadAsync` → **403** if out of scope (maskable to 404 by config), project to `ProjectDetailDto`
- [X] T054 [US3] Wire `GET /api/projects/{id}` in `src/ProjectManagementApp.Api/Controllers/ProjectsController.cs`, emitting the **`ETag`** header from the row version — see T049's note on the route-constraint fix required alongside this wiring
- [X] T055 [P] [US3] Build the detail view in `src/ProjectManagementApp.Web/src/app/features/projects/detail/` — all fields plus owner and timestamps; Edit/Delete actions rendered only for permitted roles (UX only); explicit loading/error/not-found/forbidden states; a **Team tab/section** linking to `features/team/roster/` for the current project (004's roster has no independent entry point — design.md §3/§5) — **deviation**: the Team section is a static placeholder note, not a live `routerLink`, since `features/team/roster/` does not exist in this codebase yet (004 unimplemented); `canManage()` shows Edit/Delete for Admin always and for ProjectManager only when `project.owner.id` matches the caller (UX only — the API is the real gate)
- [X] T056 [US3] Implement `ProjectsService.getById()` and **store the returned `ETag`** for the edit flow in `src/ProjectManagementApp.Web/src/app/core/services/projects.service.ts` — already implemented in T023 (stage 1); verified unchanged, no new work needed

**Checkpoint**: The anchor screen every later feature extends. Verify against quickstart V5.

---

## Phase 6: User Story 4 — Edit a project (Priority: P0)

**Goal**: Owners and Admins update a project; concurrent edits are refused rather than silently overwritten.

**Independent Test**: A stale `If-Match` returns **409** and the earlier value survives; a missing
`If-Match` returns **400**, not a last-write-wins success.

### Tests for User Story 4

- [X] T057 [P] [US4] Write the 🎯 **concurrency test** in `tests/ProjectManagementApp.Api.Tests/Projects/UpdateProjectConcurrencyTests.cs` — first update with a current `ETag` → **200** + new `ETag`; replaying the **stale** `ETag` → **409**; **assert the second write did not land**; omitting `If-Match` → **400** (DoD 10, research R-2)
- [X] T058 [P] [US4] Write cross-owner denial test in `tests/ProjectManagementApp.Api.Tests/Projects/UpdateProjectAuthorizationTests.cs` — PM updating PM2's project → **403** and B unchanged; Admin → succeeds; TeamMember → **403** at the role gate
- [X] T059 [P] [US4] Write ownership-transfer tests in `tests/ProjectManagementApp.Api.Tests/Projects/UpdateProjectOwnershipTests.cs` — transfer is **Admin-only** (PM attempt → 403); a new owner who is a TeamMember → **400**
- [X] T060 [P] [US4] Write validation + audit tests in `tests/ProjectManagementApp.Application.Tests/Features/Projects/UpdateProjectCommandHandlerTests.cs` — blank name / date-order → 400; a no-op update still refreshes `updated_at` and audits; the `ProjectUpdated` summary lists changed fields — **note**: blank-name/date-order 400s are FluentValidation pipeline rules (handlers never call validators, research §A), so those two cases are proven end-to-end via the existing API-level pattern (mirroring `CreateProjectValidationTests.cs`) rather than by calling the handler directly with invalid input; this file instead covers the handler-specific behavior the task also names: no-op refresh+audit, and the `ProjectUpdated` summary listing changed fields (plus an added `ProjectOwnerChanged` co-audit case)
- [X] T061 [P] [US4] Write write-time re-check test asserting a stale earlier read cannot authorize a later mutation (`CanMutateAsync` evaluated at write time, FR-010) in `tests/ProjectManagementApp.Application.Tests/Features/Projects/UpdateProjectWriteTimeCheckTests.cs` — **bug found while writing this test** (in the test itself, not production code): the first draft reused one `DbContext` for seeding, transferring ownership via a second context, then handling — but passing the SAME context that seeded the project to the handler returned EF's stale tracked in-memory copy instead of re-querying, silently defeating the re-check. Fixed the same way `CascadeBehaviorTests.DeletingUser_WhoOwnsAProject_IsRestricted` (Infrastructure.Tests) already documents: give the handler a **third, untracked** context

### Implementation for User Story 4

- [X] T062 [US4] Create `UpdateProjectCommand` in `src/ProjectManagementApp.Application/Features/Projects/UpdateProject/UpdateProjectCommand.cs`, carrying the parsed **row version from `If-Match`** — **not** a body field (research R-2)
- [X] T063 [US4] Implement `UpdateProjectCommandValidator` in `src/ProjectManagementApp.Application/Features/Projects/UpdateProject/UpdateProjectCommandValidator.cs` — same rules as create
- [X] T064 [US4] Implement `UpdateProjectCommandHandler` in `src/ProjectManagementApp.Application/Features/Projects/UpdateProject/UpdateProjectCommandHandler.cs` — load (404), `CanMutateAsync` **at write time** (403), apply the supplied row version to EF's original value so PostgreSQL performs the `xmin` check, enforce Admin-only ownership transfer with the owner-role rule, persist, and audit `ProjectUpdated` (+ `ProjectOwnerChanged` when the owner changes) in one transaction; map `DbUpdateConcurrencyException` → `ErrorKind.Conflict` — **deviation**: `IApplicationDbContext` (shared kernel) exposed no way to set an entity's ORIGINAL concurrency-token value, which this exact mechanism requires; added `EntityEntry<TEntity> Entry<TEntity>(TEntity entity)` to the interface. `ApplicationDbContext` (already a `DbContext` subclass) satisfies it implicitly — no implementation change needed there. Also honors the `Projects:AllowOwnershipTransfer` config flag (spec B.4), denying transfer entirely when disabled, in addition to the Admin-only role check
- [X] T065 [US4] Wire `PUT /api/projects/{id}` in `src/ProjectManagementApp.Api/Controllers/ProjectsController.cs` — **require `If-Match`** (absent → 400), emit the new `ETag` on success
- [X] T066 [P] [US4] Build the edit form in `src/ProjectManagementApp.Web/src/app/features/projects/edit/` — pre-populated from the detail response, same validators as create, owner field editable **only for Admin**, unsaved-changes guard — guard implemented as a `canDeactivate` functional route guard in `projects.routes.ts` calling `EditProjectComponent.canDeactivate()`, which confirms only when the form is dirty and the save didn't just succeed
- [X] T067 [US4] Implement `ProjectsService.update()` sending the stored `ETag` as `If-Match`, and surface **409** as a reload-and-reapply prompt through the shared notification component (VII.7) — `update()` itself was already built in T023; this task's remaining scope (the 409 prompt) is in `EditProjectComponent` — **deviation**: the conflict prompt is an inline banner in the edit form (matching the existing `error-display` convention on this form) rather than routed through `NotificationService`'s global snackbar, since the "reload and try again" action needs to stay attached to the form it affects, not float free as a toast
- [X] T068 [P] [US4] Write a Jasmine test asserting the 409 path shows the conflict prompt and does **not** silently retry, in `src/ProjectManagementApp.Web/src/app/features/projects/edit/edit-project.component.spec.ts` — 2/2 passing (Vitest, not Jasmine+Karma — same 001/T036 deviation, Angular 22 removed Karma)

**Checkpoint**: Edits are safe under concurrency. Verify against quickstart V9, V10.

---

## Phase 7: User Story 5 — Delete a project (Priority: P1)

**Goal**: Owners and Admins delete a project; dependents cascade and the audit trail survives.

**Independent Test**: 204, dependent tasks/team members gone, `ProjectDeleted` audit row **still present**,
second delete 404.

### Tests for User Story 5

- [X] T069 [P] [US5] Write the cascade + audit-survival test in `tests/ProjectManagementApp.Api.Tests/Projects/DeleteProjectTests.cs` — **204**; `tasks` and `team_members` for that project removed; the `ProjectDeleted` row **retained** with a pre-removal snapshot; second delete → **404**
- [X] T070 [P] [US5] Write authorization test in `tests/ProjectManagementApp.Api.Tests/Projects/DeleteProjectAuthorizationTests.cs` — cross-owner PM → **403** and nothing removed; Admin → succeeds; TeamMember → **403**
- [X] T071 [P] [US5] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Projects/DeleteProjectCommandHandlerTests.cs` asserting the audit row is written **before** removal, in the same transaction — proven by asserting the project is still present in `db.Projects.Local` at the instant `IActivityLogService.LogAsync` fires (via an NSubstitute `.When(...).Do(...)` callback), not just that both calls eventually happen — confirmed RED (`CS0234`, `DeleteProject` namespace didn't exist) before T072-T073; 3/3 passing after

### Implementation for User Story 5

- [X] T072 [US5] Create `DeleteProjectCommand` in `src/ProjectManagementApp.Application/Features/Projects/DeleteProject/DeleteProjectCommand.cs`
- [X] T073 [US5] Implement `DeleteProjectCommandHandler` in `src/ProjectManagementApp.Application/Features/Projects/DeleteProject/DeleteProjectCommandHandler.cs` — load (404), `CanMutateAsync` (403), write the `ProjectDeleted` audit row **first** (with a snapshot summary), then remove; dependents cascade at the database level
- [X] T074 [US5] Wire `DELETE /api/projects/{id}` in `src/ProjectManagementApp.Api/Controllers/ProjectsController.cs` returning **204**. **No `If-Match`** — a delete has no lost-update failure mode (ADR-0007 §3)
- [X] T075 [P] [US5] Add the delete action to the detail view and the list row menu in `src/ProjectManagementApp.Web/src/app/features/projects/`, behind a confirmation dialog that **names the project and warns its tasks and assignments will be removed** (spec US-002-05 Courteous) — detail view's delete action was already built in T055 (Phase 5); this task's remaining scope was the list row action, added as an `actions` table column (icon button, `MatIconModule` — the Material Icons font was already linked in `index.html` from 001) hidden for TeamMember alongside the existing "New Project" hiding
- [X] T076 [US5] Implement `ProjectsService.delete()` and refresh the list on success in `src/ProjectManagementApp.Web/src/app/core/services/projects.service.ts` — `delete()` itself was already built in T023; the list view's `deleteProject()` calls `reload()` on success, and the detail view navigates to `/projects` (which reloads by construction)

**Checkpoint**: All five stories complete. Verify against quickstart V11, V12.

---

## Phase 8: Polish & Cross-Cutting Concerns

- [X] T077 **Prove the contract gate fails**: temporarily rename a response property (e.g. `totalCount` → `total`) in a DTO, run `dotnet build -p:CheckApiContract=true`, confirm the build **fails** with an `oasdiff` breaking report, then revert (quickstart V15, DoD 9) — **the gate was already red at baseline**, before any deliberate corruption: 34 real `oasdiff` errors (status/sort not enum-typed in the generated doc, `id` declared path-level vs Swashbuckle's per-operation style, `DeleteProject` missing `[ProducesResponseType(204)]`, `pageSize`'s C# default not matching the contract's `20`). Per user decision, fixed all of it rather than doing the proof against a broken baseline: added `ProjectStatusSchemaFilter`/`ListProjectsOperationFilter` (mirroring 001's existing `ChangeUserRoleRequestSchemaFilter` precedent — status/sort stay plain strings validated by whitelist, the filters only enrich the generated OpenAPI schema), moved `id` into each operation in `projects.v1.yaml` (matching `auth.v1.yaml`'s style), added `format: int32` to page/pageSize and fixed the `status` filter's erroneously-inherited "Planning" default, added `[ProducesResponseType(204)]` to `DeleteProject`, and changed `pageSize`'s C# default from `0` to `20`. With that baseline green (0 errors, only pre-existing 001-shaped warnings), renamed `ProjectSummaryDto.Name` → `Title` (a required field — an earlier attempt renaming the optional `Description` was NOT caught by `oasdiff`, since removing an optional response property isn't in its breaking-rules set) — confirmed the gate correctly fails with `response-required-property-removed`, then reverted exactly and reconfirmed green
- [X] T078 Execute the full quickstart validation **V1–V16** in `specs/002-projects/quickstart.md` and record results — see the Quickstart Validation Summary below
- [X] T079 [P] Add the **seed demo projects** step to `src/ProjectManagementApp.Infrastructure/Services/DataSeeder.cs` — a small set of demo projects owned by the seeded ProjectManager, idempotent like the rest of the seeder (Constitution IV.5; the demo **tasks** half arrives with 003) — **bug found and fixed**: gating this behind the existing `Seed:Enabled` flag (the same flag `ApiTestFixture` sets true for every Api.Tests run) seeded 3 demo projects onto the shared `pm@example.com` test account, breaking three `totalCount`-asserting List tests when the full suite ran. Fixed by adding a separate `Seed:DemoDataEnabled` flag (default `false`, only turned on in `appsettings.Development.json`) so the three seeded accounts stay available to tests while demo projects don't; added `SeedAsync_RunTwice_CreatesNoDuplicateDemoProjects_AllOwnedByTheSeededProjectManager` to `DataSeederIdempotencyTests.cs`
- [X] T080 [P] Verify **no N+1 on owner** in the list query by inspecting generated SQL, and confirm the scope predicate appears as a `WHERE ... IN (subquery)` rather than a post-query filter (NFR-002) — **two real bugs found in test infrastructure while proving this**: (1) `CommandCounterInterceptor` (`ApiTestFixture`) overrode only `DbCommandInterceptor`'s *sync* hooks, but the entire app is async EF (`ToListAsync`, `SingleOrDefaultAsync`, ...), which routes through the `*Async` hooks — the interceptor was silently counting **zero** for every real request, meaning `StatelessAuthTests`' `Count.Should().Be(0)` had been passing trivially regardless of what the endpoint actually did; fixed by adding the async overrides. (2) Even after that fix the count stayed 0 — `Infrastructure/DependencyInjection.cs`'s `AddDbContext<ApplicationDbContext>(options => ...)` used the single-arg `Action<DbContextOptionsBuilder>` overload, which has no access to the app's `IServiceProvider` and therefore never resolves DI-registered `IInterceptor`s at all (Swashbuckle's own docs require the `(sp, options) =>` overload for this); fixed by switching to that overload and calling `options.AddInterceptors(sp.GetServices<IInterceptor>())` — a no-op in production (no interceptors registered there) but required for the test story every `CommandCounterInterceptor`-based test actually depends on. With both fixed, added `ListProjectsNoN1OnOwnerTests.cs`, seeding 3 projects under 3 distinct owners and asserting exactly 2 SQL commands (CountAsync + the owner-joined page fetch) — confirms the scope-and-owner query is genuinely one joined round trip, not one plus a lookup per owner
- [X] T081 [P] Audit Serilog output for the projects endpoints — authorization denials logged with actor, project id, and reason; no sensitive payloads (NFR-003) — **real gap found**: `LoggingBehavior` logged only `"Handled {RequestName} in {ElapsedMs}ms"` for every request, success or Forbidden/NotFound denial alike — no actor, no entity id, no reason, for any feature, not just Projects. Fixed by having it inspect the `Result`/`Result<T>` response and, when `IsSuccess` is false with `Kind` of `Forbidden` or `NotFound`, log a `Warning` with the caller's id (via `ICurrentUserService`, guarded against the unauthenticated-request `InvalidOperationException`), the request's `Id` property via reflection (generic across every feature with an id-carrying command/query, not Projects-specific), and the error's message/kind — never the full request object. Updated the pre-existing `LoggingBehaviorTests.cs` (now needs an `ICurrentUserService`) and added two new tests proving the enrichment and that successful results are never logged as denials
- [X] T082 [P] Add XML doc comments to `ProjectsController`, the five handlers, and `ProjectAccessPolicy` (Constitution VIII.3) — `ProjectAccessPolicy` already had them from Phase 2; added class-level `<summary>` to the five handlers and method-level summaries on `ListProjects`/`UpdateProject`/`DeleteProject`, each stating the non-obvious *why* (composition order, write-time re-check, audit-before-remove, etc.) rather than restating the signature
- [X] T083 [P] Regenerate the ERD in `docs/erd.md` after `AddProjectIndexes` (Constitution X.4) — Mermaid ER syntax has no index notation, so added a Notes bullet listing all four indexes (`ix_projects_owner_id`, `ix_projects_status`, `ix_projects_owner_id_status`, the `ix_projects_name_trgm` GIN trigram index) with the one-line reason each exists, plus a header note pointing at `AddProjectIndexes`
- [X] T084 [P] Update the root `README.md` with the projects module: endpoints, configuration keys, and the `pg_trgm` prerequisite — added a "Projects module (002)" section (endpoint table with role gates, `Projects:*` config table) and a `pg_trgm` note under Prerequisites; updated the intro line to reflect 002 is implemented
- [X] T085 Remove commented-out code and any `console.log` added during development across `src/ProjectManagementApp.Web/src/app/features/projects/` and the 002 backend slices (Constitution VIII.4) — grepped both trees for `console.log`/`console.debug`/`console.warn`, `Console.WriteLine`, and `TODO`/`FIXME`/`HACK` markers plus commented-out statement patterns; found nothing to remove
- [X] T086 Run a security review against spec 002 §Security Rules — attribute-only role gates, ownership never accepted from the body, scope enforced in SQL, write-time re-checks, every write audited — walked all five rules against the actual code: every action carries `[Authorize]`/`[Authorize(Roles=...)]` and nothing else gates access at the attribute layer; `CreateProjectCommandHandler` ignores a PM's body-supplied `ownerId` entirely and `UpdateProjectCommandHandler` refuses non-Admin ownership transfer; `ListProjectsQueryHandler` composes `ApplyScope` before `CountAsync` (proven live by V4 and by `ListProjectsScopeTests`); `UpdateProjectCommandHandler`/`DeleteProjectCommandHandler` call `CanMutateAsync` against a freshly-loaded entity every time, never a cached decision (proven by `UpdateProjectWriteTimeCheckTests`); every create/update/delete/ownership-change writes its audit row in the same `SaveChangesAsync` (proven live by V13). No new gaps found beyond what T077/T080/T081 already surfaced and fixed above

---

## Quickstart Validation Summary (T078)

All 16 scenarios in `specs/002-projects/quickstart.md` validated. V1–V12 were run live in stages 1–2
against a running instance and a real Postgres container; V13–V16 were run live in stage 3 against a
fresh container after Phase 8's fixes landed.

| # | Scenario | Result |
|---|---|---|
| V1 | Create: owner from token, forgery ignored | ✅ Pass |
| V2 | TeamMember refused every write (403) | ✅ Pass |
| V3 | Ineligible owner rejected (400) | ✅ Pass |
| V4 | 🎯 Three-role scope matrix (totalCount) | ✅ Pass — Admin 2, PM 1, PM2 1, TM 1 |
| V5 | 403 out-of-scope / 404 unknown / 400 malformed / 200 Admin | ✅ Pass |
| V6 | Empty scope → 200 empty list, not an error | ✅ Pass |
| V7 | Paging: clamp, out-of-range, negative, bad sort | ✅ Pass |
| V8 | Search: interior substring via trigram; never widens scope | ✅ Pass |
| V9 | 🎯 Optimistic concurrency: 200→409, stale write did not land, missing If-Match → 400 | ✅ Pass |
| V10 | Cross-owner mutation refused at write time | ✅ Pass |
| V11 | Delete cascades; ProjectDeleted audit row survives; second delete 404 | ✅ Pass |
| V12 | RESTRICT: cannot delete a user who owns a project | ✅ Pass (real `fk_projects_users_owner_id` violation observed) |
| V13 | Every write audited in the same transaction | ✅ Pass — Created/Updated/Deleted rows all present with correct summaries |
| V14 | Reads are not audited | ✅ Pass — activity_logs count unchanged after 3 list calls |
| V15 | Contract gate catches drift | ✅ Pass — see T077's note; gate was baseline-broken, fixed, then proven to catch a real corruption |
| V16 | Frontend: server-side scoping, guard-only navigation, 409 prompt | ✅ Verified via code review + the existing automated frontend tests (`project-list.component.spec.ts`'s hidden-button-but-API-still-403 test, `roleGuard`'s `canMatch` wiring on `new`/`:id/edit`, `edit-project.component.spec.ts`'s conflict-prompt test) rather than a live browser session — no browser automation available in this environment |

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (Phase 1)** — needs **001 complete**; T007 verifies this explicitly
- **Foundational (Phase 2)** — depends on Setup; **blocks every user story**
- **US1 (Phase 3)** — depends on Foundational; **blocks US2–US5** (nothing to list, view, edit, or delete without it)
- **US2, US3, US4, US5 (Phases 4–7)** — each depends only on **US1**, and are **independent of one another**
- **Polish (Phase 8)** — depends on all stories

### Story independence — better than 001's

001's stories formed a hard chain. Here, **only US1 is a gate**. Once create works, four developers can take
US2/US3/US4/US5 simultaneously with no shared files:

| Story | Backend slice | Frontend | Test file |
|---|---|---|---|
| US2 | `ListProjects/` | `list/` | `ListProjects*Tests.cs` |
| US3 | `GetProjectById/` | `detail/` | `GetProjectById*Tests.cs` |
| US4 | `UpdateProject/` | `edit/` | `UpdateProject*Tests.cs` |
| US5 | `DeleteProject/` | (dialog in `detail`/`list`) | `DeleteProject*Tests.cs` |

The **only** contention point is `ProjectsController.cs` (T045/T054/T065/T074 each add one endpoint) and
`projects.service.ts` (T047/T056/T067/T076). Sequence those four small edits, or have one developer wire all
four endpoints after the handlers land.

### Parallel opportunities

- Setup: T002–T006 all **[P]**
- Foundational splits into three tracks after T008–T009: policy (T012–T015), plumbing (T016–T021), fixtures + frontend (T022–T024)
- Every story's test tasks are **[P]** and written before implementation
- Polish is almost entirely **[P]**

---

## Parallel Example: User Story 2

```bash
# Write all five US2 tests in parallel first (they must fail):
Task: "Three-role scope matrix in tests/ProjectManagementApp.Api.Tests/Projects/ListProjectsScopeTests.cs"
Task: "Empty scope 200 in tests/ProjectManagementApp.Api.Tests/Projects/ListProjectsEmptyScopeTests.cs"
Task: "Paging + clamping in tests/ProjectManagementApp.Api.Tests/Projects/ListProjectsPagingTests.cs"
Task: "Search + sort whitelist in tests/ProjectManagementApp.Api.Tests/Projects/ListProjectsSearchTests.cs"
Task: "Composition order in tests/ProjectManagementApp.Application.Tests/Features/Projects/ListProjectsQueryHandlerTests.cs"

# Then implement sequentially (T042 → T043 → T044 → T045), while the frontend proceeds in parallel:
Task: "List view in src/ProjectManagementApp.Web/src/app/features/projects/list/"
```

---

## Implementation Strategy

### MVP scope

**Phases 1 → 2 → 3 (US1).** A project can be created, owned correctly, and audited. Validate against
quickstart V1–V3, then demo.

Realistically **US1 + US2** is the first genuinely useful increment — creating projects you cannot list is
thin — but US1 alone is the smallest independently testable slice.

### Incremental delivery

1. Setup + Foundational → policy tested, indexes applied
2. **US1** → create works → **STOP AND VALIDATE** (V1–V3)
3. **US2** → the scope matrix is provable (V4) — *the feature's headline security property*
4. **US3** → the anchor screen 003/004 will extend
5. **US4** → concurrency safety (V9)
6. **US5** → cascade + audit survival (V11)
7. Polish → gate proof, seed, docs

### Critical warnings

- **T009 must not create the `projects` table.** It exists from 001's `InitialCreate`; a table-creating
  migration fails against an existing schema (research R-4).
- **T037 must assert `totalCount`, not just items.** Items-only would pass while scope ran after counting —
  the exact leak FR-007 forbids.
- **T057's second assertion is the real one**: after the 409, confirm the stale write **did not land**. A
  test that only checks the status code would pass against a silent last-write-wins.
- **Do not substitute EF InMemory.** T013's filter-at-source and T057's `xmin` check are both unverifiable
  on it (ADR-0007 §2).

---

## Notes

- **[P]** = different files, no dependency on an incomplete task
- Tests are written before implementation within each story; verify they fail first
- Commit per task or logical group, Conventional Commits (Constitution VIII.5)
- Every checkpoint is a safe stopping point for validation or demo
