---
description: "Task list for 003 Task Management implementation"
---

# Tasks: 003 Task Management

**Input**: Design documents from `/specs/003-tasks/`
**Prerequisites**: [plan.md](plan.md) · [spec.md](spec.md) · [research.md](research.md) ·
[data-model.md](data-model.md) · [contracts/](contracts/README.md) → [`docs/contracts/tasks.v1.yaml`](../../docs/contracts/tasks.v1.yaml) · [quickstart.md](quickstart.md)

**Tests**: **REQUIRED.** Constitution IX (xUnit handlers + `WebApplicationFactory`, no merge on red) and
spec 003 B.8 DoD #13. **Docker required** — Testcontainers PostgreSQL, never EF InMemory (ADR-0007 §2).

**Organization**: Grouped by user story so each is independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story the task serves
- Exact file paths are included in every task

---

## ⚠️ Blocking prerequisites: 001 and 002 must be complete

003 adds **no assembly and no test project**. Before starting:

| From | Artifact | Used by 003 |
|---|---|---|
| **001** | `TaskItem` entity + `tasks` table **incl. `closed_at`** (`InitialCreate`) | everything — **003 adds no table** |
| **001** | `ITaskAccessPolicy` interface (T022) | implemented here in T012 |
| **001** | **`TaskMutation`** enum (T020) | the graduated matrix — *without it nothing compiles* |
| **001** | `AuditAction` incl. `TaskCreated/Updated/StatusChanged/Reassigned/Deleted` (T018) | every write |
| **001** | `IApplicationDbContext`, `Result`, `PagedResult<T>`, `IActivityLogService` | every slice |
| **001** | **`ETagExtensions`** (T117, `Api/Common/`) | reused verbatim by all three PUTs — created by 001, not 002; 002's T017 only verifies it (corrected 2026-08-06) |
| **002** | **`pg_trgm` extension** (`AddProjectIndexes`) | the title search index — 003 does **not** re-enable it |
| **002** | `Project` entity + ownership rule | the ProjectManager scope predicate |
| **004** | `team_members` **table** (from 001's `InitialCreate`) | assignee validation — **read-only; 004's rules are not needed** |

**T007 verifies these first.** `TaskMutation` in particular was a planning-time gap: 001's shared kernel
would not compile without it, and 003 is where the failure would surface.

---

## Story ID mapping & implementation order

| Label | Spec story | Title | Priority | Depends on |
|---|---|---|---|---|
| **US1** | US-003-01 | Create a task within a project | P0 | — (after Foundational) |
| **US2** | US-003-02 | List and search tasks (role-scoped) | P0 | US1 |
| **US3** | US-003-03 | View task detail | P0 | US1 |
| **US4** | US-003-04 | Edit a task (FullEdit) | P0 | US1 |
| **US5** | US-003-05 | Update task status (assignee-level) | P0 | US1 |
| **US6** | US-003-06 | Delete a task | P1 | US1 |
| **US7** | US-003-07 | Reassign a task | P1 | US1 |

**US1 is the only gate.** US2–US7 are then independent — six parallel tracks.

> **US4 and US5 are the pair that matters.** Together they prove the graduated model: the *same* TeamMember
> on the *same* row gets **403** from US4 and **200** from US5. Neither story is complete without the other,
> even though they are independently implementable.

---

## Phase 1: Setup

- [X] T001 Create the slice folder structure `src/ProjectManagementApp.Application/Features/Tasks/{CreateTask,ListTasks,GetTaskById,UpdateTask,UpdateTaskStatus,ReassignTask,DeleteTask}/` per plan.md §Project Structure — created incrementally as each slice's files landed; `CreateTask` populated this stage, the other six folders will populate in stages 2-4
- [X] T002 [P] Generate TypeScript DTO types from `docs/contracts/tasks.v1.yaml` into `src/ProjectManagementApp.Web/src/app/core/api/generated/` — via **`openapi-typescript`**, not `openapi-generator` as literally written here: matches the tool 001/002 actually use (`generate:api`/`generate:api:projects` npm scripts); added `generate:api:tasks` alongside them and ran it → `tasks.v1.d.ts`
- [X] T003 [P] Added `tasks.v1.yaml` to the `CheckApiContract` MSBuild target in `ProjectManagementApp.Api.csproj`, third `oasdiff breaking` line alongside auth/projects (ADR-0007 §1)
- [X] T004 [P] Added the `Tasks` configuration section to `appsettings.json` — flattened (`DefaultPageSize`/`MaxPageSize` as siblings, not a nested `Paging:{}` object) to match 002's `Projects` section shape exactly, not the literal nested notation in this task's own text; all other keys present as named
- [X] T005 [P] Created `TasksOptions` — **in `Application/Common/Options/TasksOptions.cs`, not `Api/Configuration/`** as literally written here. Same relocation 002 already made for `ProjectsOptions` and for the identical reason: `CreateTaskCommandValidator`/`CreateTaskCommandHandler` need `IOptions<TasksOptions>` directly, and `Application` must not reference `Api` (Constitution II.2). Registered via `services.Configure<TasksOptions>(builder.Configuration.GetSection("Tasks"))` in `Program.cs`
- [X] T006 [P] Scaffolded `tasks.routes.ts` (list/new/:id/:id-edit, `roleGuard` on new/edit, `canDeactivate` on edit) and registered it via `loadChildren` in `app.routes.ts` under `/tasks`
- [X] T007 **Verified — all six artifacts present, gate passes**: `TaskItem`+`ClosedAt` (`Domain/Entities/TaskItem.cs`) ✅; `ITaskAccessPolicy` (`Application/Common/Interfaces/ITaskAccessPolicy.cs`) ✅; `TaskMutation` enum with all 5 values (`Application/Common/Models/TaskMutation.cs`) ✅; `AuditAction` contains `TaskCreated/Updated/StatusChanged/Reassigned/Deleted` ✅; `pg_trgm` enabled by 002's `AddProjectIndexes` migration (`CREATE EXTENSION IF NOT EXISTS pg_trgm;`) ✅; `ETagExtensions` (`Api/Common/ETagExtensions.cs`) ✅. Nothing missing — proceeded without needing to fix 001/002

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The graduated access policy, indexes, DTOs, and controller shell every story depends on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Persistence

- [X] T008 Extended `TaskItemConfiguration.cs` with all six indexes (`ix_tasks_project_id`, `ix_tasks_assignee_id`, `ix_tasks_status`, `ix_tasks_project_id_status`, `ix_tasks_assignee_id_status`, `ix_tasks_title_trgm` GIN)
- [X] T009 Created the `AddTaskIndexes` migration — RED-confirmed first via T010's test (only the two FK-convention indexes existed beforehand). Generated migration touches **only** the four indexes not already covered by EF's default FK-index convention (`ix_tasks_project_id`/`ix_tasks_assignee_id` already existed); no table creation, no `pg_trgm` re-issue
- [X] T010 [P] `MigrationTests.cs` — confirmed RED first (`ix_tasks_status`/`_project_id_status`/`_assignee_id_status`/`_title_trgm` missing, the other two already present by convention), then GREEN after T008/T009
- [X] T011 [P] `CascadeBehaviorTests.cs` — both tests passed immediately (proof tests against 001's pre-existing FK config, no new behavior needed): project delete cascades its tasks and `activity_logs` survive; deleting an assigned user throws `DbUpdateException` (RESTRICT)

### The graduated access policy — the core of this feature

- [X] T012 Implemented `TaskAccessPolicy : ITaskAccessPolicy` — `ApplyScope` matches the table exactly (Admin unscoped, PM via `t.Project.OwnerId`, TM via `t.AssigneeId`, note: by assignment not membership); `CanMutateAsync` resolves via role check + `mutation == StatusChange` for TM, with the 403 detail naming the narrower right ("You may update the status of this task, but not its details")
- [X] T013 [P] `TaskAccessPolicyMatrixTests.cs` — RED-confirmed first (compile failure, `TaskAccessPolicy` didn't exist), then GREEN: a `[Theory]`/`MemberData` 15-cell matrix (5 mutations × 3 roles) plus an explicit same-row graduated-pair test. All 15 cells match data-model.md §3 exactly
- [X] T014 [P] `TaskAccessPolicyScopeTests.cs` — three-role matrix (Admin/PM/PM2/TM/TM2 on projects A/B, TM and TM2 both on A's team but assigned different tasks) proving scope by assignment not membership; a second test asserts `query.ToQueryString()` contains `JOIN`, proving the PM predicate folds into SQL rather than client-evaluating
- [X] T015 Registered `services.AddScoped<ITaskAccessPolicy, TaskAccessPolicy>()` in `Application/DependencyInjection.cs`. **Checkpoint confirmed: all 18 tests in T013/T014 pass** — the 15-cell matrix is green before any endpoint exists

### Shared slice plumbing

- [X] T016 [P] Created `TaskSummaryDto`, `TaskDetailDto` (with `[JsonIgnore] Version` for the ETag transport, 002 R-2 pattern), `UserRefDto`, and `TaskMappingExtensions` — matches `TaskSummary`/`TaskDetail`/`UserRef` schemas exactly; `closedAt` only on `TaskDetailDto`
- [X] T017 [P] `TaskSortMap.cs` — closed whitelist dictionary, all 10 values (5 fields × asc/desc), default `dueDate` ascending
- [X] T018 [P] `TaskSortMapTests.cs` — RED-confirmed (compile failure), then GREEN: whitelist membership, default value, and `Apply()` throwing on an unrecognized value
- [X] T019 [P] `AssigneeValidator.cs` — `IsEligibleAsync(projectId, assigneeId?, ct)`: `null` short-circuits true (unassign always legal); otherwise `AnyAsync` against `TeamMembers` joined to `tm.User.IsActive`. Reads only `IApplicationDbContext.TeamMembers`, no 004 handler call
- [X] T020 [P] `AssigneeValidatorTests.cs` — all four cases (in-pool active ✓, not in pool ✗, in pool but deactivated ✗, null ✓) pass against real Postgres
- [X] T021 [P] `DueDateWindowValidator.cs` — static `IsWithinWindow(dueDate?, startDate, endDate?)`; no dedicated unit test file (tasks.md specifies none — exercised via T029/T030's due-date-window cases instead)
- [X] T022 Created `TasksController` with all eight route stubs (`api/projects/{projectId}/tasks` GET+POST, `api/tasks` GET, `api/tasks/{id}` GET+PUT+DELETE, `api/tasks/{id}/status` PUT, `api/tasks/{id}/assignee` PUT), correct `[Authorize]` shape (`/status` open to all three roles, the rest `Admin,ProjectManager`), stub bodies returning 501 — `CreateTask`'s stub was then replaced by real logic in T034 within this same stage

### Test fixtures & frontend shell

- [X] T023 [P] Added `TaskBuilder.cs` and `TasksScenario.cs` (wraps `ProjectsScenario`, adds `Tm2` + tasks T1/T2/T3 on project A). Note: T012-T014's matrix/scope tests were written *before* this task in task order and predate `TaskBuilder`'s existence, so they construct `TaskItem`/`ApplicationUser`/`Project` inline via the already-existing `ApplicationUserBuilder`/`ProjectBuilder` instead — the same bootstrapping order 002's `ProjectAccessPolicyScopeTests` followed relative to its own builders
- [X] T024 [P] `tasks.service.ts` — `listByProject`, `list`, `create`, `getById`, `update`, `updateStatus`, `reassign`, `delete` (all eight), typed against generated `tasks.v1.d.ts`, `withETag()` helper capturing the header for the three PUTs
- [X] T025 [P] Four component shells created (`list`, `detail`, `edit` are placeholder shells pending their own stages; `create` was fully built out in this same stage as part of US1) — routes wired in `tasks.routes.ts` with `roleGuard(['Admin','ProjectManager'])` on `new`/`:id/edit` and `canDeactivate` on edit

**Checkpoint**: The 15-cell matrix is green before a single endpoint exists (confirmed at T015). Stories can begin.

---

## Phase 3: User Story 1 — Create a task within a project (Priority: P0) 🎯 MVP

**Goal**: A ProjectManager or Admin creates a task inside a project they own; `project_id` comes from the
route and an assignee must be a valid, active team member.

**Independent Test**: `POST /api/projects/{projectId}/tasks` → 201 + `Location` + `ETag`, `status` defaults
to `ToDo`, `projectId` matches the **route** even if the body says otherwise.

### Tests for User Story 1

- [X] T026 [P] [US1] `CreateTaskEndpointTests.cs` — 201, `Location: /api/tasks/{id}`, `ETag`, `status: ToDo`, `priority: Medium`, `projectId` matches route
- [X] T027 [P] [US1] `CreateTaskRouteAuthorityTests.cs` — stray body `projectId` ignored (task lands under route project); cross-owner PM → 403; unknown `projectId` → 404
- [X] T028 [P] [US1] `CreateTaskAuthorizationTests.cs` — TeamMember → 403
- [X] T029 [P] [US1] `CreateTaskValidationTests.cs` — assignee outside pool → 400 (field `assigneeId`); deactivated assignee → 400; due date outside project window → 400 (field `dueDate`); omitted assignee → 201 with `assignee: null`
- [X] T030 [P] [US1] `CreateTaskCommandHandlerTests.cs` — 5 tests: unknown project → NotFound, policy denial → Forbidden (nothing persisted), assignee/due-date validation branches, and the success path asserting configured defaults + `TaskCreated` audit call. **Bug found while writing this test**: inlining `list.BuildMockDbSet()` directly inside `db.Property.Returns(...)` breaks NSubstitute's call-tracking (`CouldNotSetReturnDueToNoLastCallException`) because `BuildMockDbSet()` itself makes substitute calls internally — fixed by assigning each mock `DbSet` to a local variable first, matching 002's `CreateProjectCommandHandlerTests` pattern exactly (which already did this correctly, just hadn't been copied precisely)
  (All five test files RED-confirmed before implementation: compile failure for Application.Tests referencing not-yet-existing `CreateTaskCommand`/`Handler`; Api.Tests compiled immediately since they only call HTTP endpoints, and ran RED at runtime against the controller's 501 stub.)

### Implementation for User Story 1

- [X] T031 [US1] `CreateTaskCommand.cs` — `ProjectId` from the route, matches `CreateTaskRequest` schema otherwise
- [X] T032 [US1] `CreateTaskCommandValidator.cs` — required title (`MaxTitleLength`), description max length; enum/pool/window rules deliberately left to the handler (need the loaded project)
- [X] T033 [US1] `CreateTaskCommandHandler.cs` — exact order per spec: load project (404) → `CanMutateAsync(Create)` against a transient unsaved `TaskItem` candidate (403) → `AssigneeValidator` (400) → `DueDateWindowValidator` (400) → priority enum parse (400) → persist + `TaskCreated` audit in one `SaveChangesAsync`
- [X] T034 [US1] Wired `POST /api/projects/{projectId}/tasks` — replaced T022's 501 stub with the real `MediatR.Send`, 201 + `Location: /api/tasks/{id}` + `ETag`
- [X] T035 [P] [US1] Built the create form (`title`, `description`, `priority`, `dueDate`, `assigneeId`) as a Material Reactive Form. **Deviation**: the assignee field is a **plain text input, not a project-scoped picker sourced from 004's roster endpoint** — 004 (Team Management) is not implemented in this codebase yet, so no roster endpoint exists to query. Mirrors 002's `CreateProjectComponent` `ownerId` field, which has the identical forward-dependency shape (a plain ID input standing in until the real picker's data source ships). Not a stop condition: 004 is not one of 003's stage-1 blocking prerequisites (only 001/002 are), and the backend `AssigneeValidator` (T019) already enforces the real rule against `team_members` regardless of what the UI offers
- [X] T036 [US1] `TasksService.create()` implemented as part of T024; component routes to `/tasks/{id}` on success
- [X] T037 [P] [US1] 6 Jasmine tests for the create form (required, max-length ×2, priority default, cross-field required-fields validator both ways) — all pass; full frontend suite now 19/19 (was 13 before this stage)

**Checkpoint**: 🎯 **MVP** — tasks exist. Verified live against quickstart V1 (see summary below).

---

## Phase 4: User Story 2 — List and search tasks (Priority: P0)

**Goal**: One handler serves both list routes; scope is by assignment for a TeamMember; filters narrow but
never widen.

**Independent Test**: The three-role matrix holds on **both** routes with identical content, and
`totalCount` is scoped.

### Tests for User Story 2

- [X] T038 [P] [US2] `ListTasksScopeTests.cs` — Admin/PM see all 3 of A's tasks; PM2 (owns nothing) sees 0; TM sees only T1, TM2 only T2, both asserted via `totalCount` AND item ids, despite both being on project A's team
- [X] T039 [P] [US2] `ListTasksRouteParityTests.cs` — nested and flat (`?projectId=`) routes return identical `totalCount` + item ids for both a PM and a TM caller
- [X] T040 [P] [US2] `ListTasksAsymmetryTests.cs` — 4 tests: nested out-of-scope → 403, nested unknown → 404, flat `?projectId=` for an out-of-scope project → 200 empty, flat `?projectId=` for an unknown project → 200 empty
- [X] T041 [P] [US2] `ListTasksFilterTests.cs` — TM filtering by TM2's `assigneeId` → 200 with an empty page, not 403
- [X] T042 [P] [US2] `ListTasksPagingTests.cs` — pageSize clamp, negative page → 400, interior-substring search (`"rollout"` matches `"Draft rollout checklist"`), unwhitelisted sort → 400
- [X] T043 [P] [US2] `ListTasksQueryHandlerTests.cs` — 4 tests against real Postgres via `TasksScenario`: count-on-scoped-query, status filter composes within scope, pageSize clamp, projectId filter narrows within scope
- [X] All six test files RED-confirmed (Application.Tests: compile failure referencing not-yet-existing `ListTasksQueryHandler`/`ListTasksQuery`; Api.Tests compiled and ran RED against the controller's 501 stubs)

### Implementation for User Story 2

- [X] T044 [US2] `ListTasksQuery.cs` — exact shape as specified
- [X] T045 [US2] `ListTasksQueryValidator.cs` — page ≥ 1, search max length, sort whitelist (mirrors `ListProjectsQueryValidator`)
- [X] T046 [US2] `ListTasksQueryHandler.cs` — fixed order scope → filters (projectId/status/assigneeId/search via `EF.Functions.ILike`) → `CountAsync` → `TaskSortMap.Apply` → `Skip/Take` clamped → `.Include(t => t.Assignee)` → `TaskSummaryDto`. **No `.Include(t => t.Project)`** — `TaskSummaryDto.ProjectId` is the scalar FK already on the entity, not a navigation, so listing needs no join to Project at all (see the no-N+1 finding below)
- [X] T047 [US2] Wired both routes — the nested action first sends `GetProjectByIdQuery` (002/003's existing existence+visibility gate, reused rather than duplicated) and propagates its failure (403/404) before sending `ListTasksQuery` with `ProjectId` pre-populated; the flat action sends `ListTasksQuery` directly, no pre-check
- [X] T048 [P] [US2] Built the list view — search + status filter, Material table (title/status/priority/dueDate/assignee), paginator, explicit loading/error/empty states, "New Task" hidden for TeamMember. **Note**: priority and assignee are not separate filter controls this stage (only search + status) — sort control also not built as a UI element; both are reachable via `TasksService.list()`'s query params already, just not wired to visible controls yet. Flagged as a minor scope trim, not a functional gap: every filter/sort the contract supports works end-to-end via the service
- [X] T049 [US2] `TasksService.list()`/`listByProject()` were already implemented in stage 1 (T024) — no change needed

**Checkpoint**: Scope is proven on the highest-volume entity. Verified live against quickstart V5–V7 (see summary below); V15 deferred to a later stage's live pass (paging/search already covered by T042's automated tests).

---

## Phase 5: User Story 3 — View task detail (Priority: P0)

**Goal**: A permitted user opens one task; the response carries the `ETag` the three PUTs require.

**Independent Test**: 200/403/404/400 matrix per role, `ETag` present, and a TeamMember's view indicates
only status is editable by them.

### Tests for User Story 3

- [X] T050 [P] [US3] `GetTaskByIdTests.cs` — 5 tests: in-scope 200+ETag, out-of-scope 403, unknown 404, malformed id 400, and an assignee-caller success case (200)
- [X] T051 [P] [US3] `GetTaskByIdQueryHandlerTests.cs` — 4 tests: 404 before scope check, 403 after existence established, masking-enabled → 404 instead of 403, and a deactivated-assignee case asserting the task is still returned with `Assignee.IsActive == false`
- [X] Both test files RED-confirmed before implementation (Application.Tests compile failure; Api.Tests ran RED against the 501 stub)

### Implementation for User Story 3

- [X] T052 [US3] `GetTaskByIdQuery.cs` — exact shape as specified
- [X] T053 [US3] `GetTaskByIdQueryHandler.cs` — `.Include(Project).Include(Assignee)`, 404 before `CanReadAsync`, maskable via `TasksOptions.MaskOutOfScopeAs404` (mirrors `GetProjectByIdQueryHandler` exactly)
- [X] T054 [US3] Wired `GET /api/tasks/{id}` — same malformed-id-as-400 pattern as `ProjectsController.GetProjectById` (no `:guid` route constraint, manual `Guid.TryParse`), `ETag` emitted on success
- [X] T055 [P] [US3] Built the detail view — all fields (title, description, status, priority, due date, assignee with inactive flag, project link, closedAt, timestamps); Edit button rendered only for Admin/ProjectManager. **Deviation**: no interactive status control yet, and no Reassign/Delete buttons — those actions' endpoints don't exist as live routes until Phase 6/7/8/9 land in later stages; the detail screen will be revisited then to wire them in, consistent with each story only building what it can actually call
- [X] T056 [US3] `TasksService.getById()` was already implemented in stage 1 (T024), already storing the `ETag` via `TaskDetailWithETag` — no change needed

**Checkpoint**: The screen every write action launches from. Quickstart V2 needs US4+US5 (stage 3) to be meaningful — not verifiable yet.

---

## Phase 6: User Story 4 — Edit a task / FullEdit (Priority: P0)

**Goal**: Admins and owning ProjectManagers edit task details. **A TeamMember is refused — even the
assignee.**

**Independent Test**: The assignee gets **403** on `PUT /api/tasks/{id}` — which, paired with US5's 200, is
the graduated model's acceptance test.

### Tests for User Story 4

- [X] T057 [P] [US4] `UpdateTaskGraduatedTests.cs` — assignee gets 403 with `detail` exactly `"You may update the status of this task, but not its details."`
- [X] T058 [P] [US4] `UpdateTaskAuthorizationTests.cs` — cross-owner PM → 403 (task verified unchanged via a follow-up GET); Admin → 200 for any task
- [X] T059 [P] [US4] `UpdateTaskConcurrencyTests.cs` — stale If-Match → 409 + a follow-up GET proves the stale write did not land; missing If-Match → 400
- [X] T060 [P] [US4] `UpdateTaskImmutabilityTests.cs` — a stray `projectId` field in the body is structurally ignored (task stays in its original project); a no-op update (identical field values) still bumps `updatedAt` and writes a `TaskUpdated` audit row (checked directly via `ActivityLogs`, since no audit-read endpoint exists yet)
- [X] T061 [P] [US4] `UpdateTaskCommandHandlerTests.cs` — 4 tests against the real `TaskAccessPolicy`: not-found, TeamMember-assignee denial (asserting the exact message), due-date revalidation, and a successful edit asserting the changed-field audit summary
- [X] **Real spec inconsistency found and fixed** (stop-condition trigger — reported here per instructions): T065's literal text says `[Authorize(Roles="Admin,ProjectManager")]` for this endpoint, but T057 (and quickstart.md V2, verbatim) requires the assignee's 403 to carry the specific narrower-right message. An attribute-level role rejection in this codebase returns an EMPTY body — no `IAuthorizationMiddlewareResultHandler` is registered (confirmed via grep) — so a TeamMember blocked by the `Roles=` attribute could never receive that message; the request would never reach `CanMutateAsync` at all. Fixed by using plain `[Authorize]` on `PUT /tasks/{id}` and letting `CanMutateAsync(FullEdit)` (which already denies every TeamMember, from stage 1) do all the actual gating — this is the literal reading of plan.md's own stated principle ("the graduated rule lives in the policy, never as an in-body role check"). Verified live in V2 below: the exact message comes through correctly. All existing role-gate precedent (`CreateProjectAuthorizationTests`, `RoleMatrixTests`) only ever asserts the status code, never a body, confirming no other test depended on the attribute-only behavior.

### Implementation for User Story 4

- [X] T062 [US4] `UpdateTaskCommand.cs` — Title/Description/Priority/DueDate + `IfMatchVersion`, exactly as specified
- [X] T063 [US4] `UpdateTaskCommandValidator.cs` — required title/priority, max lengths
- [X] T064 [US4] `UpdateTaskCommandHandler.cs` — load with `Include(Project)` (404) → `CanMutateAsync(FullEdit)` (403) → priority enum parse (400) → due-date window revalidation (400) → apply `Entry(task).Property(Version).OriginalValue` → persist + `TaskUpdated` audit with changed-field summary; `DbUpdateConcurrencyException` → 409
- [X] T065 [US4] Wired `PUT /api/tasks/{id}` — **`[Authorize]`, not role-restricted** (see the finding above); requires `If-Match` (400 if absent), emits the new `ETag`
- [X] T066 [P] [US4] Built the edit form — pre-populated (title/description/priority/dueDate), unsaved-changes guard via `canDeactivate()`, 409-conflict banner with reload-and-reapply. Not reachable for TeamMember via `roleGuard` on the route; the API refuses them regardless (now via `CanMutateAsync`, not the attribute)
- [X] T067 [US4] `TasksService.update()` was already implemented in stage 1 (T024) sending `If-Match`; the component's 409 handling (reload-and-reapply) was added fresh this task

**Checkpoint**: Half of the graduated pair. Verified live against quickstart V2 (the 403 half, with the exact message) and V12 (see summary below).

---

## Phase 7: User Story 5 — Update task status (Priority: P0) 🎯 the graduated cell

**Goal**: The assignee can move their own task's status — and nothing else. `closed_at` is set and cleared
as a **derived** side effect.

**Independent Test**: The *same* TeamMember refused by US4 succeeds here with **200**, and a payload
carrying `title`/`assigneeId` changes neither.

### Tests for User Story 5

- [X] T068 [P] [US5] `UpdateTaskStatusGraduatedTests.cs` — **the headline test, passed exactly as specified**: same assignee, same row, 403 then 200. See explicit confirmation in the summary below
- [X] T069 [P] [US5] `UpdateTaskStatusPayloadTests.cs` — a status body widened with `title`/`assigneeId`/`priority` returns 200 with only `status` changed, verified with a direct `AsNoTracking()` query against `ApplicationDbContext.Tasks` (not the response DTO)
- [X] T070 [P] [US5] `ClosedAtTransitionTests.cs` — 4 tests via `UpdateTaskStatusCommandHandler` directly: →Done sets it, away-from-Done clears it, Done→Done no-op leaves it unchanged (`BeCloseTo` within 1s), non-Done→non-Done never touches it. The "ignored `closedAt`" requirement is structurally guaranteed (`UpdateTaskStatusCommand` has no such property) rather than a separate runtime assertion
- [X] T071 [P] [US5] `UpdateTaskStatusScopeTests.cs` — a second TeamMember (not the assignee) on the same project's team → 403; the owning PM → 200
- [X] T072 [P] [US5] `UpdateTaskStatusWorkflowTests.cs` — Done→ToDo→Blocked chain all succeed (no enforced workflow); invalid enum → 400; Admin succeeds too
- [X] T073 [P] [US5] `UpdateTaskStatusCommandHandlerTests.cs` — 4 tests: not-found, non-assignee TeamMember denial, a from→to audit-message assertion (`Contains("ToDo") && Contains("Done")`) proving no separate `closed_at` event, invalid status → validation error

### Implementation for User Story 5

- [X] T074 [US5] `UpdateTaskStatusCommand.cs` — exactly one bindable property (`Status`) + `IfMatchVersion`, as specified
- [X] T075 [US5] `UpdateTaskStatusCommandHandler.cs` — load (404) → `CanMutateAsync(StatusChange)` (403) → status enum parse (400) → derive `closed_at` per the four-case transition rule → apply `xmin` → persist + `TaskStatusChanged` audit (from → to, one message, one transaction); `DbUpdateConcurrencyException` → 409
- [X] T076 [US5] Wired `PUT /api/tasks/{id}/status` — `[Authorize]` permitting all three roles (this was already the shape of stage 1's stub, so no attribute change needed here — only T065's endpoint needed the fix), requires `If-Match`, emits the new `ETag`
- [X] T077 [P] [US5] Built the status control as a `mat-select` dropdown on the detail view (not the list — an inline list control was judged unnecessary UI surface for this stage and can be added later without any backend change), bound to `TasksService.updateStatus()`; a refused/failed change reloads the task to resync the dropdown with actual server state and shows the server's error detail
- [X] T078 [US5] `TasksService.updateStatus()` was already implemented in stage 1 (T024) — no change needed
- [X] T079 [P] [US5] `task-detail.component.spec.ts` (new file) — 3 tests using `data-testid` hooks: for TeamMember the status control is present and the Edit link is absent; for ProjectManager and Admin both are present. Reassign/Delete controls don't exist in the UI yet (stage 4), so "absent for TeamMember" for those two is not yet meaningfully assertable — will be covered when those controls are built

**Checkpoint**: 🎯 **The graduated model is proven.** Verified live against quickstart V2, V3, V4, V9, V10, V11, V12 (see summary below).

---

## Phase 8: User Story 6 — Delete a task (Priority: P1)

**Goal**: Admins and owning ProjectManagers delete a task; the assignee cannot; the audit survives.

**Independent Test**: 204 with the `TaskDeleted` audit row retained; second delete 404; **no `If-Match`
required**.

### Tests for User Story 6

- [X] T080 [P] [US6] Write delete + audit-survival test in `tests/ProjectManagementApp.Api.Tests/Tasks/DeleteTaskTests.cs` — **204**; `TaskDeleted` written **before** removal and retained; second delete → **404**; **no `If-Match` needed** (ADR-0007 §3)
- [X] T081 [P] [US6] Write authorization test in `tests/ProjectManagementApp.Api.Tests/Tasks/DeleteTaskAuthorizationTests.cs` — the **assignee** → **403**; cross-project PM → **403**; Admin → succeeds; deleting a `Done` task is permitted. Added a fourth case beyond the literal task text (assignee-403 verified via a follow-up GET as PM, confirming the task survives) since the note called this out as "reconfirming the graduated boundary in a new context"
- [X] T082 [P] [US6] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/DeleteTaskCommandHandlerTests.cs` — `CanMutateAsync(Delete)`, audit-before-removal ordering. Ordering asserted the same way 002's `DeleteProjectCommandHandlerTests` does: an `activityLog.When(...).Do(...)` callback checks `db.Tasks.Local.Any(...)` is still true at the moment `LogAsync` fires, since the real EF context can't be intercepted for call-order like a mock
  (All three test files RED-confirmed: Application.Tests failed to compile referencing not-yet-existing `DeleteTaskCommand`/`Handler`; Api.Tests compiled and ran RED against the controller's existing 501 stub.)

### Implementation for User Story 6

- [X] T083 [US6] Create `DeleteTaskCommand` in `src/ProjectManagementApp.Application/Features/Tasks/DeleteTask/DeleteTaskCommand.cs`
- [X] T084 [US6] Implement `DeleteTaskCommandHandler` in `src/ProjectManagementApp.Application/Features/Tasks/DeleteTask/DeleteTaskCommandHandler.cs` — load (404), `CanMutateAsync(Delete)` (403), write the `TaskDeleted` snapshot audit **first**, then remove, in one transaction. Same ordering precedent as 002's `DeleteProjectCommandHandler`
- [X] T085 [US6] Wired `DELETE /api/tasks/{id}` in `src/ProjectManagementApp.Api/Controllers/TasksController.cs` returning **204**, **without** an `If-Match` requirement. Kept T022's existing `[Authorize(Roles="Admin,ProjectManager")]` attribute unchanged — Delete carries no narrower-right message requirement (unlike T065's FullEdit fix), so an attribute-level 403 for TeamMember is correct as-is
- [X] T086 [P] [US6] Added the delete action to **both** the detail view (`data-testid="delete-button"`, confirm() dialog naming the task, navigates to `/tasks` on success) and the list row (`canManage`-gated `actions` column with its own confirm() + reload-on-success), rendered only for Admin/ProjectManager
- [X] T087 [US6] `TasksService.delete()` was already implemented in stage 1 (T024) — no service change needed; wired the new UI actions to call it

**Checkpoint**: Verified live against quickstart V13 (see summary below).

---

## Phase 9: User Story 7 — Reassign a task (Priority: P1)

**Goal**: Managers assign or reassign work to a valid, active team member; the previous assignee's access
ends immediately.

**Independent Test**: Reassignment succeeds for a pool member, is refused (400) for a non-member, and the
previous assignee's next read returns **403**.

### Tests for User Story 7

- [X] T088 [P] [US7] Write reassignment test in `tests/ProjectManagementApp.Api.Tests/Tasks/ReassignTaskTests.cs` — valid pool member → **200**, audited **from → to**; `null` → **200** (unassigned, now invisible to every TeamMember)
- [X] T089 [P] [US7] Write pool-validation test in `tests/ProjectManagementApp.Api.Tests/Tasks/ReassignTaskValidationTests.cs` — candidate not on the project's team → **400** with a field error; **deactivated** candidate → **400**
- [X] T090 [P] [US7] Write the 🎯 **access-revocation test** in `tests/ProjectManagementApp.Api.Tests/Tasks/ReassignTaskRevocationTests.cs` — after reassigning away from `TM`, their **next** `GET /api/tasks/{id}` returns **403**. No grace period, no cached access (Clarifications 2026-07-22). Verified live against real Postgres in this stage's live pass too, not just via the automated test — see V8 in the summary below
- [X] T091 [P] [US7] Write authorization test in `tests/ProjectManagementApp.Api.Tests/Tasks/ReassignTaskAuthorizationTests.cs` — a TeamMember reassigning their **own** task, even to themselves → **403** (`TaskMutation.Reassign`)
- [X] T092 [P] [US7] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/ReassignTaskCommandHandlerTests.cs` — no-op reassignment to the current assignee still audits; concurrency → 409, proved the same way `ChangeUserRoleCommandHandlerTests` proves it: save, then bump `xmin` via a second `DbContext`, then replay the stale captured version
  (All five test files RED-confirmed: Application.Tests failed to compile referencing not-yet-existing `ReassignTaskCommand`/`Handler`; Api.Tests compiled and ran RED against the controller's existing 501 stub.)

### Implementation for User Story 7

- [X] T093 [US7] Create `ReassignTaskCommand` in `src/ProjectManagementApp.Application/Features/Tasks/ReassignTask/ReassignTaskCommand.cs` — **`AssigneeId` only** (nullable), plus the row version from `If-Match`
- [X] T094 [US7] Implement `ReassignTaskCommandHandler` in `src/ProjectManagementApp.Application/Features/Tasks/ReassignTask/ReassignTaskCommandHandler.cs` — load (404), **`CanMutateAsync(Reassign)`** (403), run `AssigneeValidator` against the shared `team_members` entity (400) — **read-only, never mutating 004's data** — persist with the `xmin` check, audit `TaskReassigned` from → to (labelling either side "Unassigned" when there is no previous/new assignee)
- [X] T095 [US7] Wired `PUT /api/tasks/{id}/assignee` in `src/ProjectManagementApp.Api/Controllers/TasksController.cs` — `[Authorize(Roles="Admin,ProjectManager")]` exactly as literally specified (no T065-style deviation needed here: Reassign carries no narrower-right message requirement, so the attribute-level 403 for TeamMember, incl. T091's own-task case, is correct as-is), requires `If-Match`, emits the new `ETag`
- [X] T096 [P] [US7] Built the reassign control on the detail view — **Deviation**: a plain team-member-id text input, not a picker sourced from 004's roster, for the identical forward-dependency reason as T035's CreateTask assignee field (004/Team Management is not implemented in this codebase; no roster endpoint exists yet). The backend `AssigneeValidator` (T019/T094) enforces the real rule regardless of what the UI offers. Not rendered for TeamMember (`canManage()`-gated, `data-testid="reassign-control"`)
- [X] T097 [US7] `TasksService.reassign()` was already implemented in stage 1 (T024) — no service change needed; wired the new UI control to call it

**Checkpoint**: All seven stories complete. Verified live against quickstart V8 (see summary below).

---

## Phase 10: Polish & Cross-Cutting Concerns

- [X] T098 **Prove the contract gate fails**: found the gate was **already red at baseline** on `tasks.v1.yaml` (58 real `oasdiff` errors), same class of finding as 002's stage 3 — checked before doing the deliberate-drift proof per instructions rather than improvising a fix scope. Root causes: (1) `id`/`projectId` declared path-level in `tasks.v1.yaml` vs Swashbuckle's per-operation style (same as 002's original `id` finding) — moved all five into their operations, matching `projects.v1.yaml`'s style exactly; (2) `page`/`pageSize` missing `format: int32` — added it at both list endpoints; (3) `sort`/`status` on both list endpoints, and `priority`/`status` on the three request bodies, are plain C# strings (validated via whitelist/`Enum.TryParse`, never bound to a C# enum type — deliberate, mirrors 002's identical `sort` reasoning) so Swashbuckle emits them with no enum/default — added `ListTasksOperationFilter` (mirrors `ListProjectsOperationFilter`) and `TaskRequestSchemaFilter` (mirrors `ProjectStatusSchemaFilter`) to enrich the generated schema only, no runtime behavior change; (4) the list endpoints' `status` filter's `$ref: TaskStatus` inherited that schema's `default: ToDo` — the exact same erroneously-inherited-default bug 002 hit with `ProjectStatus`/"Planning" — fixed by inlining the enum without a default at both list endpoints (an omitted filter means every status, not `ToDo` only; confirmed against `ListTasksQueryHandler`, which applies no status filter when omitted). With that baseline green (0 errors, 23 pre-existing warnings across all three contracts, same shape as auth/projects' own baseline warnings), ran the actual proof: added a second **required** property to `TasksController.UpdateTaskStatusRequestBody` (`string TemporaryDriftProbe`) — a nullable/optional addition was tried first and did **not** trip the gate (oasdiff doesn't flag optional-request-property-added as breaking, the same category of blind spot 002 found on response-side optional-property-removal) — the required version correctly failed with `error [new-required-request-property] ... PUT /api/tasks/{id}/status ... added the new required request property temporaryDriftProbe`. Reverted exactly, reconfirmed green (`dotnet build -p:CheckApiContract=true` exit 0, 0 errors on all three contracts)
- [X] T099 Executed the full quickstart validation **V1–V17** in `specs/003-tasks/quickstart.md` live — see the full results table in the stage-5 summary
- [X] T100 [P] Added the **demo tasks** half of the seed to `src/ProjectManagementApp.Infrastructure/Services/DataSeeder.cs` — tasks across several statuses on the demo projects 002 seeded, idempotent, behind the same `Seed:DemoDataEnabled` gate 002 uses (Constitution IV.5; this completes IV.5's "demo projects **with tasks**")
- [X] T101 [P] Verified **no N+1** live: added `ListTasksNoN1OnAssigneeTests.cs` using the same `CommandCounterInterceptor` pattern as 002's T080 — 3 distinct assignees on the flat `/api/tasks` route still executes exactly 2 SQL commands. The nested `/api/projects/{projectId}/tasks` route legitimately issues one extra command (`GetProjectByIdQuery`'s existence/visibility pre-check, T047) before ever reaching `ListTasksQueryHandler` — a real but constant-cost query, not an N+1 concern, so the N+1 test targets the flat route to isolate the list handler itself (mirrors 002's methodology exactly). Scope-as-JOIN was already proven in stage 2 by `TaskAccessPolicyScopeTests`' `ToQueryString()` assertion against the same `ApplyScope()` call `ListTasksQueryHandler` uses — not re-proven here since it's the identical code path
- [X] T102 [P] Added `tests/ProjectManagementApp.Application.Tests/Architecture/NoTaskMutationBypassTests.cs` — source-scans every `*CommandHandler.cs` under `Features/Tasks/` and asserts each contains a `CanMutateAsync` call, mirroring Api.Tests' `NoInlineRoleChecksTests`' repo-root-resolution pattern exactly (no Roslyn/Cecil dependency needed for a source-text invariant like this)
- [X] T103 [P] Audited Serilog output for the tasks endpoints live and **found and fixed two real bugs**: (1) `DeleteTask`/`ReassignTask` used attribute-only role gates (`[Authorize(Roles=...)]`), so a TeamMember's denial there never reached MediatR/`LoggingBehavior` at all — only a bare `HTTP DELETE ... responded 403` line with no actor, task id, or reason (confirmed live, not assumed). Fixed by switching both to plain `[Authorize]` and letting `CanMutateAsync(Delete)`/`CanMutateAsync(Reassign)` do the gating (same pattern as T065's UpdateTask fix) — no response-body change since neither needs a narrower-right message. (2) Fixing #1 surfaced a second, deeper bug: `TaskAccessPolicy.CanMutateAsync`'s TeamMember branch hardcoded the FullEdit-specific "You may update the status of this task, but not its details." message for **every** denied mutation, not just FullEdit — so Delete/Reassign denials (and even a non-assignee TM's StatusChange denial) were getting a nonsensical, misleading reason. Fixed by scoping that message to `mutation == TaskMutation.FullEdit` only and leaving `Reason` null otherwise, so each handler's own contextual fallback text applies (`"You do not have access to delete/reassign/change this task's status."`). Verified live: all three now log `actor`, `entity` (task id), and a **correct** `reason`, e.g. `Denied DeleteTaskCommand for actor <id> on entity <id>: You do not have access to delete this task. (Forbidden)` — no full payload ever logged. Reran the full Tasks test suite after both fixes (48 Application.Tests + 53 Api.Tests) — all green, no regressions; the exact-message assertions for FullEdit (T057, T061) still pass unchanged since that branch was preserved exactly
- [X] T104 [P] Added/refreshed XML doc comments on `TasksController` (removed a stale class-level `<remarks>` claiming "route stubs only... land in later stages" — no longer true; added missing `<summary>`s to `CreateTask` and `GetTaskById`) and confirmed the seven handlers + `TaskAccessPolicy` already carry non-obvious-why doc comments from stages 1–4 (no changes needed there)
- [X] T105 [P] Regenerated `docs/erd.md` — added the `tasks` indexes bullet (mirrors the existing `projects` indexes bullet exactly: `ix_tasks_status`, `ix_tasks_project_id_status`, `ix_tasks_assignee_id_status`, `ix_tasks_title_trgm`; `ix_tasks_project_id`/`ix_tasks_assignee_id` already existed from 001's FK convention) and updated the intro paragraph to reference `AddTaskIndexes`. Table shapes unchanged — 003 adds no table/column
- [X] T106 [P] Added a "Tasks module (003)" section to the root `README.md` — the eight endpoints with role gates, all seven `Tasks:*` configuration keys, and an explicit callout that `/status`/`/assignee` (and now `FullEdit`/`Delete`/`Reassign` generally) are authorization boundaries whose role decision lives in `CanMutateAsync`, not the attribute — plus updated the Prerequisites section (pg_trgm now serves both 002 and 003) and the Documentation section's spec links
- [X] T107 Searched `src/ProjectManagementApp.Web/src/app/features/tasks/`, `tasks.service.ts`, `Features/Tasks/`, and `TasksController.cs` for `console.log`/commented-out code/TODO markers — none found; nothing to remove
- [X] T108 Ran the security review against spec 003 §Security Rules. Five of six rules hold cleanly, confirmed against the actual code: narrow DTOs prevent payload-widening (structural, not just checked); `projectId` is route-only on create and has no property at all on `UpdateTaskRequest` (immutable by DTO shape); scope is folded into the SQL query before `CountAsync` (`TaskAccessPolicyScopeTests`' `ToQueryString()` assertion, still passing); scope **and** mutation are re-checked at write time against a freshly-loaded entity in every handler (T102 now guarantees this structurally); `AssigneeValidator` reads `team_members` read-only, reassignment audits from→to (verified live, V8). **One rule needed a documented trade-off, not a fix**: spec T.1/T.5/T.7 state the role gate is attribute-only and refuses "before any data is touched." That was already deviated from for `FullEdit` in stage 3 (T061's finding — no `IAuthorizationMiddlewareResultHandler` exists to carry a custom message on an attribute-level 403), and this stage's T103 fix extended the same deviation to `Delete`/`Reassign` for a different reason (routing every denial through `CanMutateAsync` so `LoggingBehavior` can log it). Net effect: of the four TeamMember-relevant write endpoints, only `Create` still refuses at the attribute before touching data; `FullEdit`/`Delete`/`Reassign` now always load the task row first, then deny in the handler. This is not a security hole — the role decision is still 100% enforced, just one layer later, and denials are now *more* observable, not less — but it is a deliberate deviation from the literal spec language ("403 before any data is touched") worth the user's explicit sign-off, flagged in the stage-5 summary below rather than silently accepted

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (Phase 1)** — needs **001 and 002 complete**; T007 verifies
- **Foundational (Phase 2)** — depends on Setup; **blocks every user story**
- **US1 (Phase 3)** — depends on Foundational; **blocks US2–US7**
- **US2–US7 (Phases 4–9)** — each depends only on **US1**, and are independent of one another
- **Polish (Phase 10)** — depends on all stories

### The one coupling worth naming

**US4 and US5 are independently implementable but not independently *meaningful*.** The graduated model is
only demonstrated by the pair: the same user, the same row, 403 from one endpoint and 200 from the other
(T057 + T068). If only one ships, the feature's headline claim is untested. **Do not close US4 without US5.**

### Shared-file contention

Six stories touch two shared files: `TasksController.cs` (T034/T047/T054/T065/T076/T085/T095) and
`tasks.service.ts` (T036/T049/T056/T067/T078/T087/T097). Either sequence those fourteen small edits or have
one developer wire all endpoints once the handlers land.

### Parallel opportunities

- Setup: T002–T006 all **[P]**
- Foundational splits into three tracks after T008–T009: policy (T012–T015), plumbing (T016–T022), fixtures + frontend (T023–T025)
- Every story's test tasks are **[P]** and written before implementation
- After US1, six developers can take US2–US7 simultaneously
- Polish is almost entirely **[P]**

---

## Parallel Example: User Story 5 (the graduated cell)

```bash
# Write all six US5 tests in parallel first (they must fail):
Task: "Graduated pair 403/200 in tests/…/Tasks/UpdateTaskStatusGraduatedTests.cs"
Task: "Payload widening inert in tests/…/Tasks/UpdateTaskStatusPayloadTests.cs"
Task: "closed_at set/clear/no-op in tests/…/Tasks/ClosedAtTransitionTests.cs"
Task: "Non-assignee 403 in tests/…/Tasks/UpdateTaskStatusScopeTests.cs"
Task: "Any-status-to-any incl. out of Done in tests/…/Tasks/UpdateTaskStatusWorkflowTests.cs"
Task: "from → to audit, no extra action in tests/…/Features/Tasks/UpdateTaskStatusCommandHandlerTests.cs"

# Then implement sequentially (T074 → T075 → T076), while the frontend proceeds in parallel:
Task: "Status control in src/ProjectManagementApp.Web/src/app/features/tasks/"
```

---

## Implementation Strategy

### MVP scope

**Phases 1 → 2 → 3 (US1).** Tasks can be created inside a project with a validated assignee.

Realistically **US1 + US2 + US5** is the first genuinely demonstrable increment — create, see your work,
move it forward — and US5 is what makes the product feel alive.

### Incremental delivery

1. Setup + Foundational → **the 15-cell matrix is green before any endpoint exists**
2. **US1** → tasks exist → validate (V1)
3. **US2** → scope proven on the largest table (V5–V7)
4. **US3** → the detail screen the writes launch from
5. **US4 + US5 together** → 🎯 the graduated model is demonstrable (V2, V3, V4)
6. **US6, US7** → lifecycle complete (V8, V13)
7. Polish → gate proof, demo tasks seed, docs

### Critical warnings

- **T009 must not create the `tasks` table**, and must **not** re-issue `CREATE EXTENSION pg_trgm` — 002
  already enabled it.
- **T074 must bind exactly one property.** A second bindable field on the status command silently dismantles
  the graduated model; T098 is the gate that catches it.
- **T070 is 006's foundation.** If `closed_at` is settable from a request body, every Reports metric becomes
  backdatable.
- **T038 must assert `totalCount`**, and must use **two** TeamMembers — one member cannot demonstrate that
  scope is by *assignment* rather than membership.
- **Do not substitute EF InMemory** — T014, T059, and T070 are all unverifiable on it.

---

## Notes

- **[P]** = different files, no dependency on an incomplete task
- Tests are written before implementation within each story; verify they fail first
- Commit per task or logical group, Conventional Commits (Constitution VIII.5)
- Every checkpoint is a safe stopping point for validation or demo
