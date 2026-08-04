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
| **002** | **`pg_trgm` extension** (`AddProjectIndexes`) | the title search index — 003 does **not** re-enable it |
| **002** | **`ETagExtensions`** (002 T017) | reused verbatim by all three PUTs |
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

- [ ] T001 Create the slice folder structure `src/ProjectManagementApp.Application/Features/Tasks/{CreateTask,ListTasks,GetTaskById,UpdateTask,UpdateTaskStatus,ReassignTask,DeleteTask}/` per plan.md §Project Structure
- [ ] T002 [P] Generate TypeScript DTO types from `docs/contracts/tasks.v1.yaml` into `src/ProjectManagementApp.Web/src/app/core/api/generated/` via `openapi-generator` (types only; the service stays hand-written)
- [ ] T003 [P] Add `tasks.v1.yaml` to the `CheckApiContract` MSBuild target in `src/ProjectManagementApp.Api/ProjectManagementApp.Api.csproj` (ADR-0007 §1)
- [ ] T004 [P] Add the `Tasks:*` configuration section to `src/ProjectManagementApp.Api/appsettings.json` — `Paging:{DefaultPageSize=20,MaxPageSize=100}`, `DefaultStatus=ToDo`, `DefaultPriority=Medium`, `EnforceStatusWorkflow=false`, `AllowUnassigned=true`, `AllowAssignToInactiveUser=false`, `AllowWritesToTerminalStatusProject=true`, `MaskOutOfScopeAs404=false`, `MaxTitleLength`, `MaxDescriptionLength` (spec B.4)
- [ ] T005 [P] Create `TasksOptions` binding class in `src/ProjectManagementApp.Api/Configuration/TasksOptions.cs` and register it in `Program.cs`
- [ ] T006 [P] Scaffold the lazy `tasks` route group in `src/ProjectManagementApp.Web/src/app/features/tasks/tasks.routes.ts` and register it with `loadChildren` in `app.routes.ts` (standalone, ADR-0001)
- [ ] T007 **Verify 001/002 prerequisites before proceeding**: `TaskItem` entity and `tasks` table exist **with `closed_at`**; `ITaskAccessPolicy` is declared in `Application/Common/Interfaces/`; **`TaskMutation` exists** in `Application/Common/Models/`; `AuditAction` contains the five Task values; `pg_trgm` is enabled by 002's migration; `ETagExtensions` exists in `Api/Common/`. **Stop and fix the owning feature if any is missing** — do not create them here (ADR-0006 addendum)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The graduated access policy, indexes, DTOs, and controller shell every story depends on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Persistence

- [ ] T008 Extend `src/ProjectManagementApp.Infrastructure/Persistence/Configurations/TaskItemConfiguration.cs` with the six indexes from data-model.md §4: `(project_id)`, `(assignee_id)`, `(status)`, `(project_id,status)`, `(assignee_id,status)`, and the **GIN trigram index on `title`**
- [ ] T009 Create the `AddTaskIndexes` migration in `src/ProjectManagementApp.Infrastructure/Persistence/Migrations/`. **Must NOT create the `tasks` table** — it exists from 001's `InitialCreate`, including `closed_at`. **Do not re-issue `CREATE EXTENSION pg_trgm`** — 002's migration already enabled it and migrations apply in order (research R-7)
- [ ] T010 [P] Write migration test in `tests/ProjectManagementApp.Infrastructure.Tests/Tasks/MigrationTests.cs` asserting all six indexes exist and the trigram index is usable for `ILIKE`
- [ ] T011 [P] Write cascade/RESTRICT tests in `tests/ProjectManagementApp.Infrastructure.Tests/Tasks/CascadeBehaviorTests.cs` — deleting a **project** cascades its tasks; deleting a **user who is an assignee** is **restricted**; `activity_logs` rows survive both

### The graduated access policy — the core of this feature

- [ ] T012 Implement `TaskAccessPolicy : ITaskAccessPolicy` in `src/ProjectManagementApp.Application/Common/Authorization/TaskAccessPolicy.cs`, injecting `IApplicationDbContext` — `ApplyScope` (Admin unscoped · PM `t.Project.OwnerId == caller` · TM `t.AssigneeId == caller`), `CanReadAsync`, and **`CanMutateAsync(task, TaskMutation, caller)` resolving the 5 × 3 matrix in one `switch`** (data-model.md §3). Lives in `.Application` (002 R-1 pattern)
- [ ] T013 [P] Write the 🎯 **15-cell table-driven matrix test** in `tests/ProjectManagementApp.Application.Tests/Authorization/TaskAccessPolicyMatrixTests.cs` — every `TaskMutation` × role cell, asserting **`StatusChange` is the only cell a TeamMember may pass** (DoD 3)
- [ ] T014 [P] Write the `ApplyScope` three-role test in `tests/ProjectManagementApp.Application.Tests/Authorization/TaskAccessPolicyScopeTests.cs` against Testcontainers PostgreSQL — assert the PM predicate resolves **through the `Project` navigation** as a SQL join, and that a TeamMember sees only **assigned** tasks (not every task on a project they belong to)
- [ ] T015 Register `TaskAccessPolicy` as the `ITaskAccessPolicy` implementation in `src/ProjectManagementApp.Application/DependencyInjection.cs`

### Shared slice plumbing

- [ ] T016 [P] Create `TaskSummaryDto`, `TaskDetailDto`, and `UserRefDto` plus manual mapping extensions in `src/ProjectManagementApp.Application/Features/Tasks/`, matching the contract schemas. **`closedAt` appears in responses only** — never in a request shape (research R-3)
- [ ] T017 [P] Implement the task **sort whitelist** in `src/ProjectManagementApp.Application/Features/Tasks/ListTasks/TaskSortMap.cs` — `dueDate`, `-dueDate`, `priority`, `-priority`, `status`, `-status`, `title`, `-title`, `createdAt`, `-createdAt`; anything else is a validation error, never interpolated
- [ ] T018 [P] Write sort-whitelist tests in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/TaskSortMapTests.cs`
- [ ] T019 [P] Implement the shared **assignee-eligibility check** in `src/ProjectManagementApp.Application/Features/Tasks/Common/AssigneeValidator.cs` — the candidate must have a `team_members` row for the task's project **and** be active; used by both create and reassign. **Reads the shared `team_members` entity; never calls a 004 handler and never writes it** (research R-6)
- [ ] T020 [P] Write assignee-eligibility tests in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/AssigneeValidatorTests.cs` — in-pool active ✓; not in pool ✗; in pool but deactivated ✗; null (unassign) ✓
- [ ] T021 [P] Implement the **due-date window rule** in `src/ProjectManagementApp.Application/Features/Tasks/Common/DueDateWindowValidator.cs` — when set, `due_date` must fall within the parent project's `start_date`…`end_date` (ADR-0005 cross-field, needs the parent project so it lives beside the handler, not in the FluentValidation validator)
- [ ] T022 Create the thin `TasksController` shell in `src/ProjectManagementApp.Api/Controllers/TasksController.cs` with all **eight** route stubs and their `[Authorize]` attributes — note `/status` permits **all three roles** while create/edit/reassign/delete are `Admin,ProjectManager`. **No logic; one `MediatR.Send` per endpoint**

### Test fixtures & frontend shell

- [ ] T023 [P] Add `TaskBuilder` and the 003 fixture set to `tests/ProjectManagementApp.Application.Tests/Builders/` — extends 002's set with **two TeamMembers** (`TM`, `TM2`, both on project A) and tasks T1 (assigned TM), T2 (assigned TM2), T3 (unassigned). **`TM2` is required** to prove scope is by *assignment*, not membership (ADR-0007 §4)
- [ ] T024 [P] Implement `TasksService` in `src/ProjectManagementApp.Web/src/app/core/services/tasks.service.ts` — all eight calls, generated DTO types, capturing the `ETag` from detail responses for the three PUTs
- [ ] T025 [P] Create the four standalone component shells in `src/ProjectManagementApp.Web/src/app/features/tasks/{list,detail,create,edit}/` with routes wired and a functional role guard in `tasks.routes.ts`

**Checkpoint**: The 15-cell matrix is green before a single endpoint exists. Stories can begin.

---

## Phase 3: User Story 1 — Create a task within a project (Priority: P0) 🎯 MVP

**Goal**: A ProjectManager or Admin creates a task inside a project they own; `project_id` comes from the
route and an assignee must be a valid, active team member.

**Independent Test**: `POST /api/projects/{projectId}/tasks` → 201 + `Location` + `ETag`, `status` defaults
to `ToDo`, `projectId` matches the **route** even if the body says otherwise.

### Tests for User Story 1

- [ ] T026 [P] [US1] Write integration test in `tests/ProjectManagementApp.Api.Tests/Tasks/CreateTaskEndpointTests.cs` — **201**, `Location`, `ETag`, `status: ToDo`, `priority: Medium` defaults
- [ ] T027 [P] [US1] Write route-authority test in `tests/ProjectManagementApp.Api.Tests/Tasks/CreateTaskRouteAuthorityTests.cs` — a body `projectId` pointing elsewhere is **ignored**; creating in a project the PM does not own → **403**; unknown `projectId` → **404**
- [ ] T028 [P] [US1] Write role-gate test in `tests/ProjectManagementApp.Api.Tests/Tasks/CreateTaskAuthorizationTests.cs` — TeamMember → **403** (`TaskMutation.Create` denied)
- [ ] T029 [P] [US1] Write assignee + due-date validation tests in `tests/ProjectManagementApp.Api.Tests/Tasks/CreateTaskValidationTests.cs` — assignee not in the project's pool → **400**; deactivated assignee → **400**; `dueDate` outside the project window → **400**; omitted assignee → **201** (unassigned is legal)
- [ ] T030 [P] [US1] Write handler branch tests in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/CreateTaskCommandHandlerTests.cs` covering every conditional branch and the `TaskCreated` audit write

### Implementation for User Story 1

- [ ] T031 [US1] Create `CreateTaskCommand` in `src/ProjectManagementApp.Application/Features/Tasks/CreateTask/CreateTaskCommand.cs` — carries `ProjectId` **from the route**; the contract's `CreateTaskRequest` deliberately has no `projectId` property
- [ ] T032 [US1] Implement `CreateTaskCommandValidator` in `src/ProjectManagementApp.Application/Features/Tasks/CreateTask/CreateTaskCommandValidator.cs` — required title, max lengths, valid enum values
- [ ] T033 [US1] Implement `CreateTaskCommandHandler` in `src/ProjectManagementApp.Application/Features/Tasks/CreateTask/CreateTaskCommandHandler.cs` — load the parent project (404), `CanMutateAsync(Create)` (403), run `AssigneeValidator` and `DueDateWindowValidator` (400), persist with defaults, write the `TaskCreated` audit row in the **same** `SaveChangesAsync`
- [ ] T034 [US1] Wire `POST /api/projects/{projectId}/tasks` in `src/ProjectManagementApp.Api/Controllers/TasksController.cs` — `[Authorize(Roles="Admin,ProjectManager")]`, 201 + `Location: /api/tasks/{id}` + `ETag`
- [ ] T035 [P] [US1] Build the create form in `src/ProjectManagementApp.Web/src/app/features/tasks/create/` — Material Reactive Form (`title`, `description`, `priority`, `dueDate`, `assignee`) with a **project-scoped assignee picker** sourced from 004's roster endpoint, due-date-window validator, errors via the shared error-display component
- [ ] T036 [US1] Implement `TasksService.create()` and route to the new task's detail view in `src/ProjectManagementApp.Web/src/app/core/services/tasks.service.ts`
- [ ] T037 [P] [US1] Write Jasmine tests for the create form validators in `src/ProjectManagementApp.Web/src/app/features/tasks/create/create-task.component.spec.ts`

**Checkpoint**: 🎯 **MVP** — tasks exist. Verify against quickstart V1.

---

## Phase 4: User Story 2 — List and search tasks (Priority: P0)

**Goal**: One handler serves both list routes; scope is by assignment for a TeamMember; filters narrow but
never widen.

**Independent Test**: The three-role matrix holds on **both** routes with identical content, and
`totalCount` is scoped.

### Tests for User Story 2

- [ ] T038 [P] [US2] Write the 🎯 **three-role scope matrix test** in `tests/ProjectManagementApp.Api.Tests/Tasks/ListTasksScopeTests.cs` — Admin all; PM all tasks in owned projects; **`TM` sees only T1, `TM2` only T2** (not T3, not each other's) despite both being on project A. **Assert `totalCount`** (FR-010)
- [ ] T039 [P] [US2] Write **route-parity** test in `tests/ProjectManagementApp.Api.Tests/Tasks/ListTasksRouteParityTests.cs` — `GET /api/projects/{id}/tasks` and `GET /api/tasks?projectId={id}` return **identical content** for the same caller (one handler, one predicate — research R-4)
- [ ] T040 [P] [US2] Write the **403/404 asymmetry** test in `tests/ProjectManagementApp.Api.Tests/Tasks/ListTasksAsymmetryTests.cs` — nested route: out-of-scope project → **403**, unknown → **404**; cross-project route: **neither**, just scoped content
- [ ] T041 [P] [US2] Write filter-cannot-widen test in `tests/ProjectManagementApp.Api.Tests/Tasks/ListTasksFilterTests.cs` — `TM` passing `?assigneeId=<TM2>` gets an **empty page, not 403** (a 403 would confirm the task exists)
- [ ] T042 [P] [US2] Write paging/search tests in `tests/ProjectManagementApp.Api.Tests/Tasks/ListTasksPagingTests.cs` — `pageSize=500` clamped to 100; `page=-1` → 400; interior-substring title search via the trigram index; unwhitelisted `sort` → 400
- [ ] T043 [P] [US2] Write handler composition-order test in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/ListTasksQueryHandlerTests.cs` — scope → filter → **count** → sort → page (data-model.md §5)

### Implementation for User Story 2

- [ ] T044 [US2] Create `ListTasksQuery` in `src/ProjectManagementApp.Application/Features/Tasks/ListTasks/ListTasksQuery.cs` with `Page`, `PageSize`, `ProjectId?`, `Status?`, `AssigneeId?`, `Search?`, `Sort?`
- [ ] T045 [US2] Implement `ListTasksQueryValidator` in `src/ProjectManagementApp.Application/Features/Tasks/ListTasks/ListTasksQueryValidator.cs` — paging bounds and sort whitelist
- [ ] T046 [US2] Implement `ListTasksQueryHandler` in `src/ProjectManagementApp.Application/Features/Tasks/ListTasks/ListTasksQueryHandler.cs` — **one handler serving both routes**; `ApplyScope` → filters → `CountAsync` → whitelisted sort (default due date ascending) → `Skip/Take` clamped → project to `TaskSummaryDto`. No N+1 on project or assignee
- [ ] T047 [US2] Wire **both** `GET /api/projects/{projectId}/tasks` and `GET /api/tasks` in `src/ProjectManagementApp.Api/Controllers/TasksController.cs` — the nested route pre-populates the `ProjectId` filter and adds the project existence/scope check (403/404); the flat route does neither
- [ ] T048 [P] [US2] Build the list view in `src/ProjectManagementApp.Web/src/app/features/tasks/list/` — table with search, status/priority/assignee filters, sort, paginator; explicit empty/loading/error states; **no client-side role filtering**; "New Task" hidden for TeamMember (UX only)
- [ ] T049 [US2] Implement `TasksService.list()` and `listByProject()` in `src/ProjectManagementApp.Web/src/app/core/services/tasks.service.ts`

**Checkpoint**: Scope is proven on the highest-volume entity. Verify against quickstart V5–V7, V15.

---

## Phase 5: User Story 3 — View task detail (Priority: P0)

**Goal**: A permitted user opens one task; the response carries the `ETag` the three PUTs require.

**Independent Test**: 200/403/404/400 matrix per role, `ETag` present, and a TeamMember's view indicates
only status is editable by them.

### Tests for User Story 3

- [ ] T050 [P] [US3] Write the status matrix test in `tests/ProjectManagementApp.Api.Tests/Tasks/GetTaskByIdTests.cs` — in-scope **200** with `ETag`; out-of-scope **403**; unknown **404**; malformed id **400**
- [ ] T051 [P] [US3] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/GetTaskByIdQueryHandlerTests.cs` — 404 before scope check, then `CanReadAsync` denial; a task whose assignee was deactivated is still returned, flagged

### Implementation for User Story 3

- [ ] T052 [US3] Create `GetTaskByIdQuery` in `src/ProjectManagementApp.Application/Features/Tasks/GetTaskById/GetTaskByIdQuery.cs`
- [ ] T053 [US3] Implement `GetTaskByIdQueryHandler` in `src/ProjectManagementApp.Application/Features/Tasks/GetTaskById/GetTaskByIdQueryHandler.cs` — load with parent project and assignee, 404, `CanReadAsync` → 403, project to `TaskDetailDto`
- [ ] T054 [US3] Wire `GET /api/tasks/{id}` in `src/ProjectManagementApp.Api/Controllers/TasksController.cs`, emitting the **`ETag`** header
- [ ] T055 [P] [US3] Build the detail view in `src/ProjectManagementApp.Web/src/app/features/tasks/detail/` — all fields plus project, assignee, timestamps; Edit/Reassign/Delete rendered only for permitted roles; **for a TeamMember only the status control is enabled** (UX only)
- [ ] T056 [US3] Implement `TasksService.getById()` and **store the `ETag`** for the three write flows in `src/ProjectManagementApp.Web/src/app/core/services/tasks.service.ts`

**Checkpoint**: The screen every write action launches from. Verify against quickstart V2 setup.

---

## Phase 6: User Story 4 — Edit a task / FullEdit (Priority: P0)

**Goal**: Admins and owning ProjectManagers edit task details. **A TeamMember is refused — even the
assignee.**

**Independent Test**: The assignee gets **403** on `PUT /api/tasks/{id}` — which, paired with US5's 200, is
the graduated model's acceptance test.

### Tests for User Story 4

- [ ] T057 [P] [US4] Write the 🎯 **assignee-refused test** in `tests/ProjectManagementApp.Api.Tests/Tasks/UpdateTaskGraduatedTests.cs` — the assignee receives **403** with a detail that **names the narrower right** ("You may update the status of this task, but not its details")
- [ ] T058 [P] [US4] Write cross-project denial test in `tests/ProjectManagementApp.Api.Tests/Tasks/UpdateTaskAuthorizationTests.cs` — PM editing a task in a project they do not own → **403**; Admin → succeeds
- [ ] T059 [P] [US4] Write concurrency test in `tests/ProjectManagementApp.Api.Tests/Tasks/UpdateTaskConcurrencyTests.cs` — stale `If-Match` → **409** **and the write did not land**; absent `If-Match` → **400**
- [ ] T060 [P] [US4] Write immutability test in `tests/ProjectManagementApp.Api.Tests/Tasks/UpdateTaskImmutabilityTests.cs` — `projectId` cannot be changed through this endpoint (it is not in `UpdateTaskRequest`); a no-op update still refreshes `updated_at` and audits
- [ ] T061 [P] [US4] Write handler tests in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/UpdateTaskCommandHandlerTests.cs` — `CanMutateAsync(FullEdit)` branches, due-date window revalidation, `TaskUpdated` changed-field summary

### Implementation for User Story 4

- [ ] T062 [US4] Create `UpdateTaskCommand` in `src/ProjectManagementApp.Application/Features/Tasks/UpdateTask/UpdateTaskCommand.cs` — `Title`, `Description`, `Priority`, `DueDate` **only**, plus the row version from `If-Match`. **Deliberately excludes `status`, `assigneeId`, `projectId`** — each has its own endpoint or is immutable
- [ ] T063 [US4] Implement `UpdateTaskCommandValidator` in `src/ProjectManagementApp.Application/Features/Tasks/UpdateTask/UpdateTaskCommandValidator.cs`
- [ ] T064 [US4] Implement `UpdateTaskCommandHandler` in `src/ProjectManagementApp.Application/Features/Tasks/UpdateTask/UpdateTaskCommandHandler.cs` — load with project (404), **`CanMutateAsync(FullEdit)`** at write time (403), revalidate the due-date window, apply the row version for the `xmin` check, persist, audit `TaskUpdated`; map `DbUpdateConcurrencyException` → `ErrorKind.Conflict`
- [ ] T065 [US4] Wire `PUT /api/tasks/{id}` in `src/ProjectManagementApp.Api/Controllers/TasksController.cs` — `[Authorize(Roles="Admin,ProjectManager")]`, **require `If-Match`**, emit the new `ETag`
- [ ] T066 [P] [US4] Build the edit form in `src/ProjectManagementApp.Web/src/app/features/tasks/edit/` — pre-populated, same validators as create, unsaved-changes guard, **not reachable for TeamMember** (guard) with the API refusing regardless
- [ ] T067 [US4] Implement `TasksService.update()` sending `If-Match` and surfacing **409** as a reload-and-reapply prompt in `src/ProjectManagementApp.Web/src/app/core/services/tasks.service.ts`

**Checkpoint**: Half of the graduated pair. Verify against quickstart V2 (the 403 half), V12.

---

## Phase 7: User Story 5 — Update task status (Priority: P0) 🎯 the graduated cell

**Goal**: The assignee can move their own task's status — and nothing else. `closed_at` is set and cleared
as a **derived** side effect.

**Independent Test**: The *same* TeamMember refused by US4 succeeds here with **200**, and a payload
carrying `title`/`assigneeId` changes neither.

### Tests for User Story 5

- [ ] T068 [P] [US5] Write the 🎯 **graduated-pair test** in `tests/ProjectManagementApp.Api.Tests/Tasks/UpdateTaskStatusGraduatedTests.cs` — the same assignee, same row: **403** on `PUT /tasks/{id}` and **200** on `PUT /tasks/{id}/status`. *This is the feature's headline acceptance test* (DoD 3)
- [ ] T069 [P] [US5] Write the 🎯 **payload-widening test** in `tests/ProjectManagementApp.Api.Tests/Tasks/UpdateTaskStatusPayloadTests.cs` — a status request also carrying `title`, `assigneeId`, `priority` returns **200 with only `status` changed**; verify in the database that the other columns are untouched (DoD 4)
- [ ] T070 [P] [US5] Write the 🎯 **`closed_at` transition test** in `tests/ProjectManagementApp.Infrastructure.Tests/Tasks/ClosedAtTransitionTests.cs` — → `Done` **sets** `closed_at`; away from `Done` **clears** it; `Done`→`Done` leaves it **unchanged**; a request supplying `closedAt` is **ignored** (never backdatable — 006 depends on this)
- [ ] T071 [P] [US5] Write scope test in `tests/ProjectManagementApp.Api.Tests/Tasks/UpdateTaskStatusScopeTests.cs` — a TeamMember who is **not** the assignee → **403** (scope gate fires before the mutation gate); a PM on their own project → 200
- [ ] T072 [P] [US5] Write workflow-freedom test in `tests/ProjectManagementApp.Api.Tests/Tasks/UpdateTaskStatusWorkflowTests.cs` — any status may move to any other **including out of `Done`**, for every role (OQ-003-03); an invalid enum value → **400**
- [ ] T073 [P] [US5] Write handler + audit tests in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/UpdateTaskStatusCommandHandlerTests.cs` — `TaskStatusChanged` records **from → to** and reflects the `closed_at` effect; **no separate audit action is emitted** (spec B.7)

### Implementation for User Story 5

- [ ] T074 [US5] Create `UpdateTaskStatusCommand` in `src/ProjectManagementApp.Application/Features/Tasks/UpdateTaskStatus/UpdateTaskStatusCommand.cs` — **exactly one bindable property, `Status`**, plus the row version from `If-Match`. **Adding a second property would dismantle the graduated model**; the contract's `UpdateTaskStatusRequest` has `additionalProperties: false` for the same reason
- [ ] T075 [US5] Implement `UpdateTaskStatusCommandHandler` in `src/ProjectManagementApp.Application/Features/Tasks/UpdateTaskStatus/UpdateTaskStatusCommandHandler.cs` — load (404), **`CanMutateAsync(StatusChange)`** (403), apply the status, **derive `closed_at`** (set on entry to `Done`, clear on exit, unchanged on a no-op), persist with the `xmin` check, audit `TaskStatusChanged` from → to in one transaction
- [ ] T076 [US5] Wire `PUT /api/tasks/{id}/status` in `src/ProjectManagementApp.Api/Controllers/TasksController.cs` — **`[Authorize]` permitting all three roles** (unlike the other writes), require `If-Match`, emit the new `ETag`
- [ ] T077 [P] [US5] Build the status control on the detail view and inline in the list at `src/ProjectManagementApp.Web/src/app/features/tasks/` — a dropdown or drag-between-columns board. **For a TeamMember this is the only enabled write control on the screen**
- [ ] T078 [US5] Implement `TasksService.updateStatus()` in `src/ProjectManagementApp.Web/src/app/core/services/tasks.service.ts`
- [ ] T079 [P] [US5] Write a Jasmine test asserting the status control is enabled and the edit/reassign/delete controls are absent for a TeamMember, in `src/ProjectManagementApp.Web/src/app/features/tasks/detail/task-detail.component.spec.ts`

**Checkpoint**: 🎯 **The graduated model is proven.** Verify against quickstart V2, V3, V4, V10, V11.

---

## Phase 8: User Story 6 — Delete a task (Priority: P1)

**Goal**: Admins and owning ProjectManagers delete a task; the assignee cannot; the audit survives.

**Independent Test**: 204 with the `TaskDeleted` audit row retained; second delete 404; **no `If-Match`
required**.

### Tests for User Story 6

- [ ] T080 [P] [US6] Write delete + audit-survival test in `tests/ProjectManagementApp.Api.Tests/Tasks/DeleteTaskTests.cs` — **204**; `TaskDeleted` written **before** removal and retained; second delete → **404**; **no `If-Match` needed** (ADR-0007 §3)
- [ ] T081 [P] [US6] Write authorization test in `tests/ProjectManagementApp.Api.Tests/Tasks/DeleteTaskAuthorizationTests.cs` — the **assignee** → **403**; cross-project PM → **403**; Admin → succeeds; deleting a `Done` task is permitted
- [ ] T082 [P] [US6] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/DeleteTaskCommandHandlerTests.cs` — `CanMutateAsync(Delete)`, audit-before-removal ordering

### Implementation for User Story 6

- [ ] T083 [US6] Create `DeleteTaskCommand` in `src/ProjectManagementApp.Application/Features/Tasks/DeleteTask/DeleteTaskCommand.cs`
- [ ] T084 [US6] Implement `DeleteTaskCommandHandler` in `src/ProjectManagementApp.Application/Features/Tasks/DeleteTask/DeleteTaskCommandHandler.cs` — load (404), `CanMutateAsync(Delete)` (403), write the `TaskDeleted` snapshot audit **first**, then remove, in one transaction
- [ ] T085 [US6] Wire `DELETE /api/tasks/{id}` in `src/ProjectManagementApp.Api/Controllers/TasksController.cs` returning **204**, **without** an `If-Match` requirement
- [ ] T086 [P] [US6] Add the delete action to the detail view and list row menu in `src/ProjectManagementApp.Web/src/app/features/tasks/`, behind a confirmation dialog naming the task; rendered only for permitted roles
- [ ] T087 [US6] Implement `TasksService.delete()` and refresh the list on success in `src/ProjectManagementApp.Web/src/app/core/services/tasks.service.ts`

**Checkpoint**: Verify against quickstart V13.

---

## Phase 9: User Story 7 — Reassign a task (Priority: P1)

**Goal**: Managers assign or reassign work to a valid, active team member; the previous assignee's access
ends immediately.

**Independent Test**: Reassignment succeeds for a pool member, is refused (400) for a non-member, and the
previous assignee's next read returns **403**.

### Tests for User Story 7

- [ ] T088 [P] [US7] Write reassignment test in `tests/ProjectManagementApp.Api.Tests/Tasks/ReassignTaskTests.cs` — valid pool member → **200**, audited **from → to**; `null` → **200** (unassigned, now invisible to every TeamMember)
- [ ] T089 [P] [US7] Write pool-validation test in `tests/ProjectManagementApp.Api.Tests/Tasks/ReassignTaskValidationTests.cs` — candidate not on the project's team → **400** with a field error; **deactivated** candidate → **400**
- [ ] T090 [P] [US7] Write the 🎯 **access-revocation test** in `tests/ProjectManagementApp.Api.Tests/Tasks/ReassignTaskRevocationTests.cs` — after reassigning away from `TM`, their **next** `GET /api/tasks/{id}` returns **403**. No grace period, no cached access (Clarifications 2026-07-22)
- [ ] T091 [P] [US7] Write authorization test in `tests/ProjectManagementApp.Api.Tests/Tasks/ReassignTaskAuthorizationTests.cs` — a TeamMember reassigning their **own** task, even to themselves → **403** (`TaskMutation.Reassign`)
- [ ] T092 [P] [US7] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Tasks/ReassignTaskCommandHandlerTests.cs` — no-op reassignment to the current assignee still audits; concurrency → 409

### Implementation for User Story 7

- [ ] T093 [US7] Create `ReassignTaskCommand` in `src/ProjectManagementApp.Application/Features/Tasks/ReassignTask/ReassignTaskCommand.cs` — **`AssigneeId` only** (nullable), plus the row version from `If-Match`
- [ ] T094 [US7] Implement `ReassignTaskCommandHandler` in `src/ProjectManagementApp.Application/Features/Tasks/ReassignTask/ReassignTaskCommandHandler.cs` — load (404), **`CanMutateAsync(Reassign)`** (403), run `AssigneeValidator` against the shared `team_members` entity (400) — **read-only, never mutating 004's data** — persist with the `xmin` check, audit `TaskReassigned` from → to
- [ ] T095 [US7] Wire `PUT /api/tasks/{id}/assignee` in `src/ProjectManagementApp.Api/Controllers/TasksController.cs` — `[Authorize(Roles="Admin,ProjectManager")]`, require `If-Match`, emit the new `ETag`
- [ ] T096 [P] [US7] Build the assignee picker on the detail view in `src/ProjectManagementApp.Web/src/app/features/tasks/detail/` — limited to the parent project's team members (sourced from 004's roster), with confirmation on change; **not rendered for TeamMember**
- [ ] T097 [US7] Implement `TasksService.reassign()` in `src/ProjectManagementApp.Web/src/app/core/services/tasks.service.ts`

**Checkpoint**: All seven stories complete. Verify against quickstart V8.

---

## Phase 10: Polish & Cross-Cutting Concerns

- [ ] T098 **Prove the contract gate fails**: temporarily add a second property to `UpdateTaskStatusRequest`, run `dotnet build -p:CheckApiContract=true`, confirm the build **fails**, then revert. **This is the highest-value drift check in the product** — widening that one schema would silently dismantle the graduated model (quickstart V16)
- [ ] T099 Execute the full quickstart validation **V1–V17** in `specs/003-tasks/quickstart.md` and record results
- [ ] T100 [P] Add the **demo tasks** half of the seed to `src/ProjectManagementApp.Infrastructure/Services/DataSeeder.cs` — tasks across several statuses on the demo projects 002 seeded, idempotent (Constitution IV.5; this completes IV.5's "demo projects **with tasks**")
- [ ] T101 [P] Verify **no N+1** on project or assignee in the list query by inspecting generated SQL, and confirm the scope predicate resolves as a join rather than a post-query filter (NFR-002)
- [ ] T102 [P] Add an architecture test in `tests/ProjectManagementApp.Application.Tests/Architecture/NoTaskMutationBypassTests.cs` asserting **every** write handler under `Features/Tasks/` calls `CanMutateAsync` — the belt-and-braces guarantee must not be droppable by a future slice (spec T.2)
- [ ] T103 [P] Audit Serilog output for the tasks endpoints — authorization denials logged with actor, task id, **attempted `TaskMutation`**, and reason (NFR-003)
- [ ] T104 [P] Add XML doc comments to `TasksController`, the seven handlers, and `TaskAccessPolicy` (Constitution VIII.3)
- [ ] T105 [P] Regenerate the ERD in `docs/erd.md` after `AddTaskIndexes` (Constitution X.4)
- [ ] T106 [P] Update the root `README.md` with the tasks module — the eight endpoints, configuration keys, and a note that `/status` and `/assignee` are **authorization boundaries, not convenience routes**
- [ ] T107 Remove commented-out code and any `console.log` across `src/ProjectManagementApp.Web/src/app/features/tasks/` and the 003 backend slices (Constitution VIII.4)
- [ ] T108 Run a security review against spec 003 §Security Rules — attribute-only role gates, narrow DTOs, `project_id` from the route, scope **and** mutation re-checked at write time, assignment validated without mutating 004's pool

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
