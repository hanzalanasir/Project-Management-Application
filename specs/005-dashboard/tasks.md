---
description: "Task list for 005 Dashboard implementation"
---

# Tasks: 005 Dashboard

**Input**: Design documents from `/specs/005-dashboard/`
**Prerequisites**: [plan.md](plan.md) · [spec.md](spec.md) · [research.md](research.md) ·
[data-model.md](data-model.md) · [contracts/](contracts/README.md) → [`docs/contracts/dashboard.v1.yaml`](../../docs/contracts/dashboard.v1.yaml) · [quickstart.md](quickstart.md)

**Tests**: **REQUIRED.** Constitution IX and spec 005 B.8 DoD #12. **Docker required** — **T027
(filter-at-source) is meaningless on EF InMemory**, which evaluates LINQ in memory and would let a
fetch-then-filter leak pass (ADR-0007 §2).

**Organization**: Grouped by user story so each is independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story the task serves
- Exact file paths are included in every task

---

## ⚠️ Blocking prerequisites

| From | Artifact | Used by 005 |
|---|---|---|
| **001** | **`IActivityLogService.QueryScopedAsync`** (T022 declare, T033 implement) | **the activity feed's only legal read path** — 005 is forbidden from querying `activity_logs` directly (FR-006) |
| **001** | `IApplicationDbContext`, `Result`, `PagedResult<T>`, `ICurrentUserService` | every slice |
| **002** | `IProjectAccessPolicy.ApplyScope` (implemented in 002 T012) | the visible-project set |
| **003** | `ITaskAccessPolicy.ApplyScope` (implemented in 003 T012) | the personal task slice |
| **003** | `tasks` with `status`, `due_date`, `assignee_id` | every task metric |
| **004** | `team_members` rows | the TeamMember visible-project scope and headcount |

**T006 verifies these.** `QueryScopedAsync` in particular was a planning-time gap — 001 defined the audit
service with `LogAsync` only, and 005 is where the omission would surface as *"there is no legal way to
build this feature"*.

---

## What 005 deliberately does NOT build

| Absent | Why | Reference |
|---|---|---|
| **Any write path** | Read-only. No POST/PUT/DELETE under `/api/dashboard`, not even "mark activity as read" | spec T.1 |
| **`CanMutateAsync`** | Nothing to authorize a mutation for | research R-6 |
| **`xmin` / `ETag` / `If-Match`** | Nothing is updated | research R-6 |
| **Audit rows** | IV.4 audits *writes*; 005 has none. **The empty audit catalog is correct, not an omission** — contrast 006, which writes exactly one row per generation | research R-6 |
| **403 / 404** | The dashboard **names no resource**; scope shapes content. Empty scope → **200 with zeros** | research R-3 |
| **Paging on `/summary`** | Fixed-N metrics; only the unbounded feed pages | research R-5 |
| **Migration / index** | Aggregation over existing entities; relies on 002/003/004's indexes | data-model §1 |

**T053 asserts the no-write property**; **T059 asserts no migration was added.** Both exist so the
absences are *proven*, not merely intended.

---

## Story ID mapping & implementation order

| Label | Spec story | Title | Priority | Endpoint |
|---|---|---|---|---|
| **US1** | US-005-01 | Role-scoped summary tiles | P0 | `GET /api/dashboard/summary` |
| **US2** | US-005-02 | Role-scoped recent activity feed | P1 | `GET /api/dashboard/activity` |
| **US3** | US-005-03 | My personal task slice | P1 | **none — rides inside US1's response** |

> ### ⚠️ US3 has no endpoint of its own
>
> The personal slice is a **`personalTasks` block inside the summary payload** (spec US-005-03 D.API) —
> one round trip, fixed-N, wanted in the same paint. **US3 therefore extends US1's handler and DTO** and
> **cannot be developed in parallel with US1**. US2 is the only genuinely independent story here.
>
> This also means **US1 is not "done" until US3 lands**: the contract marks `personalTasks` as *required*,
> so a summary without it fails the drift gate.

---

## Phase 1: Setup

- [X] T001 Create the slice folder structure `src/ProjectManagementApp.Application/Features/Dashboard/{GetSummary,GetActivity}/` per plan.md §Project Structure — **query slices only; no command folders**
- [X] T002 [P] Generate TypeScript DTO types from `docs/contracts/dashboard.v1.yaml` into `src/ProjectManagementApp.Web/src/app/core/api/generated/` via `openapi-generator` (types only) — added `generate:api:dashboard` npm script matching 001-004's convention; ran once, output at `dashboard.v1.d.ts`
- [X] T003 [P] Add `dashboard.v1.yaml` to the `CheckApiContract` MSBuild target in `src/ProjectManagementApp.Api/ProjectManagementApp.Api.csproj` (ADR-0007 §1)
- [X] T004 [P] Add the `Dashboard:*` configuration section to `src/ProjectManagementApp.Api/appsettings.json` — `Activity:{DefaultPageSize=20,MaxPageSize=100}` and `OverdueBoundary` (the date **comparison** only). **Do not add a timezone setting** — UTC is fixed, because a configurable one would let a deployment break 006's NFR-002 parity requirement (research R-2)
- [X] T005 [P] Create `DashboardOptions` binding class — **placed in `src/ProjectManagementApp.Application/Common/Options/DashboardOptions.cs`, not the task's literal `Api/Configuration` path**, following the exact relocation precedent 002/003/004 already established for `ProjectsOptions`/`TasksOptions`/`TeamOptions` (see their own header comments): every value here is consumed by a slice handler and Application must not reference Api (Constitution II.2). Registered in `Program.cs`.
- [X] T006 **Verify prerequisites before proceeding** — all confirmed present: `IActivityLogService.QueryScopedAsync` declared in `Common/Interfaces/IActivityLogService.cs` and implemented in `Infrastructure/Services/ActivityLogService.cs`; `IProjectAccessPolicy`/`ITaskAccessPolicy` both registered in `Application/DependencyInjection.cs`; `team_members` table exists from 004. No gap found — nothing to stop and fix.
- [X] T007 [P] Scaffold the lazy `dashboard` route group in `src/ProjectManagementApp.Web/src/app/features/dashboard/dashboard.routes.ts` behind the functional `authGuard`, registered with `loadChildren` in `app.routes.ts` at path `dashboard`

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T008 Implement the **visible-project scope resolver** in `src/ProjectManagementApp.Application/Features/Dashboard/Common/VisibleProjectScope.cs` — returns `IProjectAccessPolicy.ApplyScope(db.Projects, caller).Select(p => p.Id)` as an **un-materialized `IQueryable<Guid>`**. No `ToListAsync()` anywhere in the resolver.
- [X] T009 [P] Write scope-resolver tests in `tests/ProjectManagementApp.Application.Tests/Features/Dashboard/VisibleProjectScopeTests.cs` — three-role sets (Admin/PM/TM/unassigned-TM) plus a subquery-composition test: composes the resolver into `db.Tasks.Where(t => scope.Contains(t.ProjectId))` and asserts `ToQueryString()` contains a nested `SELECT ... FROM projects` (two `SELECT`s), proving it translates as `WHERE project_id IN (<subquery>)` rather than a materialized parameter list. Runs against real Testcontainers Postgres (ADR-0007 §2). **6 tests, all passing.**
- [X] T010 [P] Implement the **enum zero-seeding helper** in `src/ProjectManagementApp.Application/Features/Dashboard/Common/EnumCountMap.cs` — generic `Build<TEnum>`, seeds every `Enum.GetValues<TEnum>()` at `0`, merges in query results
- [X] T011 [P] Write zero-seeding tests in `tests/ProjectManagementApp.Application.Tests/Features/Dashboard/EnumCountMapTests.cs` — empty input, partial merge (unmatched keys stay `0`, never omitted), and both `ProjectStatus`/`TaskStatus`. **3 tests, all passing.**
- [X] T012 [P] **Consumed** the shared-kernel `MetricDefinitions` from `src/ProjectManagementApp.Application/Common/Metrics/MetricDefinitions.cs` (already created by **001 T020** — confirmed present with `IsOverdue`, `IsClosed`, `CompletionRate`, `ClosedInWindow`). No new file — this task is pure verification that no 005-local copy exists, and none was created.
- [X] T013 [P] Write metric-definition tests in `tests/ProjectManagementApp.Application.Tests/Common/Metrics/MetricDefinitionsTests.cs` — overdue boundary (today/yesterday/Done-excluded/no-due-date), `IsClosed`, and `CompletionRate` (empty→0, partial, all-closed). **8 tests, all passing.**
- [X] T014 [P] Create `DashboardSummaryDto`, `PersonalTaskSummaryDto`, and `ActivityEntryDto` in `src/ProjectManagementApp.Application/Features/Dashboard/`, matching the contract — every enum key required in the shape; `TaskStatus` fully-qualified as `ProjectManagementApp.Domain.Enums.TaskStatus` throughout (matches the rest of the codebase's disambiguation against `System.Threading.Tasks.TaskStatus`)
- [X] T015 Create the thin `DashboardController` shell in `src/ProjectManagementApp.Api/Controllers/DashboardController.cs` with the two **`GET`** stubs and `[Authorize]` (all three roles). Bodies `throw NotImplementedException` with a pointer to the task that wires them (T026/T038) — `GetDashboardSummaryQuery`/`GetDashboardActivityQuery` don't exist until Phase 3/4. No write verbs, no 403/404 paths.
- [X] T016 [P] Extended the fixture set with `tests/ProjectManagementApp.Application.Tests/Builders/DashboardScenario.cs` — wraps `TeamScenario`, adds **`Lonely`** (a user on no team anywhere) plus six tasks on project A spanning every `TaskStatus`, with two overdue (due yesterday, not Done), one due-today (not overdue — boundary case), one `Done` with a past due date (excluded regardless), and one with no due date (never overdue)
- [X] T017 [P] Implement `DashboardService` in `src/ProjectManagementApp.Web/src/app/core/services/dashboard.service.ts` — `getSummary()` and `getActivity(query)` using generated DTO types and the existing `toQueryParams` helper for paging
- [X] T018 [P] Created the summary and activity-feed component shells in `src/ProjectManagementApp.Web/src/app/features/dashboard/{summary,activity-feed}/` (placeholder templates only — real content in T028/T039/T047), and added **Chart.js 4.5.1** to the workspace via `npm install chart.js` (first feature to need it, per Constitution III)

**Checkpoint**: Scope resolution, zero-seeding, and the metric definitions are tested in isolation — **17 new backend tests passing** (6+3+8, plus verification), full `Application.Tests` suite at **188/188 passing**, `Api.Tests` unaffected (10/10 sampled passing, app boots with the new controller/config), and both `dotnet build` (Api) and `ng build` (frontend, dev config) succeed cleanly.

---

## Phase 3: User Story 1 — Role-scoped summary tiles (Priority: P0) 🎯 MVP

**Goal**: Every authenticated user gets a summary of exactly what they may see — counts by status, overdue,
completion rate, blocked count, team size.

**Independent Test**: The three-role matrix holds; an empty scope returns **200 with zeros**; every enum key
is present.

> **Not complete without US3** — the contract marks `personalTasks` required.

### Tests for User Story 1

- [X] T019 [P] [US1] Write the 🎯 **three-role scope matrix test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/SummaryScopeTests.cs` — Admin/PM/PM2/TM against a shared scenario (`DashboardTestHelper.SeedScopeScenarioAsync`); confirms PM2's numbers contain nothing from A, and TM's tiles are personal-view (3 tasks assigned to TM, not the 6 on the whole project). **5 tests, all passing.**
- [X] T020 [P] [US1] Write the 🎯 **stable-contract test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/SummaryContractShapeTests.cs` — all keys present for a caller with real data and for one with none. **2 tests, all passing.**
- [X] T021 [P] [US1] Write the 🎯 **zero-scope test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/SummaryEmptyScopeTests.cs` — a freshly-registered user (never added to any team = LONELY) gets 200, every breakdown zeroed. **1 test, passing.**
- [X] T022 [P] [US1] Write metric tests in `tests/ProjectManagementApp.Api.Tests/Dashboard/SummaryMetricTests.cs` — overdue boundary, exact `0.25` fraction (3 of 12 Done), empty-set zero, and distinct-headcount-across-projects. **4 tests, all passing.**
- [X] T023 [P] [US1] Write handler tests in `tests/ProjectManagementApp.Application.Tests/Features/Dashboard/GetDashboardSummaryQueryHandlerTests.cs` — Admin/PM/TM/second-TM/Lonely against the `DashboardScenario` fixture, plus the T042 tile-consistency assertion inline. **5 tests, all passing.**

### Implementation for User Story 1

- [X] T024 [US1] Created `GetDashboardSummaryQuery` — no parameters, as specified.
- [X] T025 [US1] Implemented `GetDashboardSummaryQueryHandler`. **Design note beyond the task text**: for Admin/ProjectManager, `tasksByStatus`/`overdueTaskCount` are computed via `ITaskAccessPolicy.ApplyScope(db.Tasks, caller)` — 003's existing task-scope predicate — rather than re-deriving project-wide task scope from `VisibleProjectScope`. This reuses 003's authorization logic exactly as the plan mandates ("005 defines no scope logic of its own") and happens to already encode the correct per-role semantics (Admin=all, PM=owned-project-wide, TM=assigned-only) with zero new code.
  - **Real bug found and fixed via the contract gate (T003's whole purpose)**: the first implementation used `Dictionary<string,int>` for `projectsByStatus`/`tasksByStatus`/`personalTasks.byStatus`. This serializes with the exact correct casing at runtime (dictionary keys bypass the API's global camelCase policy) and every automated HTTP test passed — but running `dotnet build -p:CheckApiContract=true` showed Swashbuckle describes any `Dictionary<,>` as a free-form `additionalProperties` map, not the contract's fixed `required` properties. This is precisely the "silently degrades into a variable dictionary" failure the contract's own comments warn about (T052 exists to prove this exact gate later) — it just surfaced one phase early, by accident, because the gate happened to be run. **Fixed** by replacing the dictionaries with two new fixed-shape record types, `ProjectStatusCountsDto`/`TaskStatusCountsDto` (`src/ProjectManagementApp.Application/Features/Dashboard/`), each property annotated `[JsonPropertyName("...")]` to pin the exact PascalCase key despite the app's camelCase policy. Re-ran the gate after the fix: **zero errors for either dashboard endpoint** (5 remaining errors are pre-existing, for `GET /api/dashboard/activity`, which is stage 3/US2's scope — untouched here). Re-ran both full test suites after the fix: still 195/195 and 195/195.
- [X] T026 [US1] Wired `GET /api/dashboard/summary` — one `Send`, no 403/404.
- [X] T027 [US1] 🎯 Wrote the **filter-at-source test**. Functional half (numbers unchanged) verified both by automated test and live via curl (ten tasks added to an invisible project, PM's `visibleProjectCount`/`overdueTaskCount` identical before/after). SQL-inspection half required adding `SqlCapturingInterceptor` (new, `tests/ProjectManagementApp.Api.Tests/Fixtures/`) since no such capture existed at the HTTP-test layer — registered in `ApiTestFixture` alongside the existing `CommandCounterInterceptor`. Confirms at least one captured command shows `project_id ... IN (SELECT ... FROM projects ...)`. **2 tests, all passing.**
- [X] T028 [P] [US1] Built the summary tiles in `summary/` — 5 fixed-N tiles, two Chart.js doughnut charts (projects/tasks by status) with a text legend, loading/error states. Chart rendering is guarded with a `getContext('2d')` check so it degrades gracefully in jsdom (no `canvas` npm package installed) rather than throwing in unit tests.
- [X] T029 [US1] Implemented refetch-on-navigation in `SummaryComponent`: since `dashboard.routes.ts` has one route, Angular reuses the component instance on re-navigation to `/dashboard`, so a `Router.events` subscription (filtered to `NavigationEnd` on this route) triggers `refresh()` in addition to the constructor's initial call. `DashboardService.getSummary()` itself was already implemented in stage 1 (T017).

**Checkpoint**: 🎯 **MVP**. Verified against quickstart V1, V3, V4, V5, V6 live via curl against the dev database (V1's PM2-disjoint-from-A and TM-personal-view rows, V3's all-keys-present on both populated and empty scopes, V4's 200-with-zeros for a fresh user, V5's exact `0.25` for 3-of-12-Done and `0` for the empty set, V6's overdue boundary — only the due-yesterday/non-Done task counted). V2's functional half (numbers unchanged) also verified live; its SQL-shape half is covered by the new `SummaryFilterAtSourceTests` (EF InMemory would not catch a fetch-then-filter leak here — real Testcontainers Postgres, per ADR-0007 §2). V2/V6's non-UTC-clock proof is T050's job (Polish phase, stage 4) — not attempted here.

---

## Phase 4: User Story 2 — Role-scoped recent activity feed (Priority: P1)

**Goal**: A paginated feed of activity on entities the caller can see, read through the audit service.

**Independent Test**: Per-role scoping holds with a scoped `totalCount`, paging clamps, and the handler
provably calls `IActivityLogService`.

> **⚠️ Blocking bug found and fixed before this phase could be honestly implemented**: 001's
> `ActivityLogService.QueryScopedAsync` (created by 001 T033, zero test coverage anywhere in the
> repo) scoped reads by comparing `ActivityLog.EntityId` (the audited entity's own id — a task id,
> a team-member-row id, a user id) directly against the caller's **visible-project** ids. This can
> only ever match `EntityType: "Project"` rows — every `Task`/`TeamMember` activity row was
> silently invisible to every non-Admin caller (Admin's `Unscoped` fast path masked the bug
> entirely). Confirmed empirically with a throwaway reproduction test before touching anything.
> **Fixed** (with explicit user authorization to cross the "do not touch 001-004" boundary for
> this specific blocker): added a nullable `ProjectId` column to `ActivityLog` (migration
> `AddActivityLogProjectId`), stamped at write time via a new optional `LogAsync(..., Guid?
> projectId = null)` parameter (backward-compatible — existing 6-arg call sites still compile),
> updated all 10 project-scoped call sites across 002/003/004 (`Project`/`Task`/`TeamMember`
> handlers) to pass it, left the 6 `User`-entity call sites in 001's Auth handlers passing `null`
> (correct — a login/register event has no owning project, so it's Admin-only visible by design),
> and rewrote `QueryScopedAsync` to filter on the stamped column instead of the broken `EntityId`
> comparison. Added `ActivityLogServiceTests.cs` (6 tests) — the coverage that should have existed
> in 001. Fixed 12 now-mismatched mock verifications in existing 002/003/004 handler tests (added
> the `projectId` argument each now legitimately passes). **Full regression after the fix: 34
> Infrastructure.Tests + 195 Application.Tests + 195 Api.Tests = 424/424, zero regressions.**

### Tests for User Story 2

- [X] T030 [P] [US2] Wrote the feed scope test in `tests/ProjectManagementApp.Api.Tests/Dashboard/ActivityScopeTests.cs` — Admin/PM/PM2/TM against the shared scope scenario; asserts `totalCount` itself is scoped (not just `items`), and that Admin's total is a superset containing every entityId a scoped PM sees. **4 tests, all passing.**
- [X] T031 [P] [US2] Wrote the 🎯 **service-not-table test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/ActivityReadPathTests.cs` — a source scan of every `.cs` file under `Features/Dashboard/` asserting no non-comment line contains `.ActivityLogs`; a source-content assertion the handler file mentions `QueryScopedAsync`; and a functional check (PM2 sees exactly its own project's one row). **3 tests, all passing.**
- [X] T032 [P] [US2] Wrote paging tests in `tests/ProjectManagementApp.Api.Tests/Dashboard/ActivityPagingTests.cs` — default pageSize 20; `pageSize=500` clamped to 100 (200, not rejected); `page=999` → empty items, valid metadata, correct `totalCount`, not 404; `page=-1` → 400; ordering newest-first and byte-identical across two successive requests. **5 tests, all passing.**
- [X] T033 [P] [US2] Wrote the invisible-project test in `tests/ProjectManagementApp.Api.Tests/Dashboard/ActivityInvisibleProjectTests.cs` — two sub-scenarios: ownership transfer (via Admin `PUT`) and project deletion, each confirming the former owner loses visibility while Admin retains it (including the surviving `ProjectDeleted` audit row for the deletion case). **2 tests, all passing.**
- [X] T034 [P] [US2] Wrote empty-feed test in `tests/ProjectManagementApp.Api.Tests/Dashboard/ActivityEmptyTests.cs` — a freshly-registered LONELY user gets 200 with an empty page, zero `totalCount`/`totalPages`, never 404. **1 test, passing.**

### Implementation for User Story 2

- [X] T035 [US2] Created `GetDashboardActivityQuery` — `Page`/`PageSize` only, as specified.
- [X] T036 [US2] Implemented `GetDashboardActivityQueryValidator` — `Page >= 1`; `PageSize` deliberately unvalidated (clamped by the handler, never rejected — Constitution VI.4, same convention as `ListProjectsQueryValidator`).
- [X] T037 [US2] Implemented `GetDashboardActivityQueryHandler` — builds the visible-project scope via T008's resolver, materializes it into `ActivityScope` (the one legitimate exit point where 005 materializes the otherwise-lazy `IQueryable<Guid>`, since `IActivityLogService`'s contract — 001's, immutable here — needs a concrete collection; skipped entirely for Admin's `Unscoped` fast path), and calls `QueryScopedAsync`. Never touches `db.ActivityLogs`.
- [X] T038 [US2] Wired `GET /api/dashboard/activity` — `[Authorize]`, one `Send`, default `page=1`/`pageSize=0` (sentinel for "use configured default"), only 400/401 possible.
- [X] T039 [P] [US2] Built the activity-feed widget in `activity-feed/` — actor/action/entity/summary/timestamp rows, `mat-paginator` (matching the app's existing project/task list convention rather than infinite scroll), explicit loading/empty/error states. Strictly read-only — no action controls anywhere.
- [X] T040 [US2] `DashboardService.getActivity()` — already implemented in stage 1 (T017); verified still correct, no changes needed.

**Checkpoint**: Verified against quickstart V8–V11, live via curl against the dev database (not just
automated tests) — V8: PM's feed (totalCount 3) contained only its own project's activity, PM2's feed
(totalCount 2) only its own, Admin's feed contained both project ids. V10: `pageSize=500` → 100,
`page=999` → empty items with correct `totalCount`, `page=-1` → 400, default → 20. V11: after an
Admin-executed ownership transfer, the former owner's feed no longer contained the transferred
project's activity while Admin's still did. V9 is structural (proven by the automated source-scan
test) and was implicitly reconfirmed by V8/V11's correct live scoping — a coincidentally-correct
direct table query couldn't have produced those exact per-role boundaries.

---

## Phase 5: User Story 3 — My personal task slice (Priority: P1)

**Goal**: A focused view of the tasks assigned to the caller — count, by-status, overdue — inside the
summary payload.

**Independent Test**: The slice counts only `assignee_id == caller` within member-of projects; a user with
no assignments gets zeros, not a 404.

> **Extends US1's handler and DTO — not parallelizable with US1.**

### Tests for User Story 3

- [X] T041 [P] [US3] Write the personal-slice test in `tests/ProjectManagementApp.Api.Tests/Dashboard/PersonalSliceTests.cs` — TM's 3 tasks vs. TM2's 1 task on the same project (colleague's excluded), and a PM with no assignments gets zeros. **2 tests, all passing.**
- [X] T042 [P] [US3] Write the 🎯 tile-consistency test in `tests/ProjectManagementApp.Api.Tests/Dashboard/PersonalSliceTileConsistencyTests.cs` — TM's `tasksByStatus`/`overdueTaskCount` proven byte-identical to `personalTasks`; a manager's two slices proven **distinct** (project-wide 6 vs. personal 0), so the design isn't accidentally collapsing both roles into one number. **2 tests, all passing.**
- [X] T043 [P] [US3] Write scope-boundary test in `tests/ProjectManagementApp.Api.Tests/Dashboard/PersonalSliceScopeTests.cs` — closes an assigned task (Done, satisfying `OpenAssignedTaskCheck`), removes the caller from the project's team via `DELETE /api/projects/{id}/team/{userId}`, and confirms the task drops out of `personalTasks` even though `AssigneeId` still points at the caller. **1 test, passing.**
- [X] T044 [P] [US3] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Dashboard/PersonalSliceTests.cs` — `ToQueryString()` on the composed personal-task query shows both `assignee_id` and a nested `SELECT` against `projects` (same subquery-proof technique as T009), plus a colleague-exclusion check via `PersonalTaskSlice.ComputeAsync` directly. **2 tests, all passing.**

### Implementation for User Story 3

- [X] T045 [US3] Implemented the personal-slice aggregate in `PersonalTaskSlice.cs` — static `ComputeAsync(db, visibleProjectIds, callerUserId, todayUtc, ct)`, reused by the handler.
- [X] T046 [US3] Extended `GetDashboardSummaryQueryHandler` to populate `personalTasks` for every role, and — for a **TeamMember only** — source `tasksByStatus`/`overdueTaskCount`/`blockedTaskCount`/`completionRate`'s inputs directly from the `PersonalTaskSlice` result object (not recomputed), which is what makes T042's equality exact rather than merely two predicates happening to agree.
- [X] T047 [P] [US3] Built the **"My Work"** panel inside `summary.component.html` — assigned count, overdue count, by-status legend, link to `/tasks`. Empty state renders exactly `"You have no tasks assigned."` per spec.
- [X] T048 [P] [US3] Wrote `summary.component.spec.ts` — asserts the zero-assignment case renders the exact empty-state text (no `.error` element present), and that a non-zero case renders the count instead. **2 tests, both passing** (`ng test`, 8 spec files / 24 tests total, 0 failures — pre-existing suite unaffected).

**Checkpoint**: All three stories complete; `personalTasks` satisfies the contract's `required` field —
confirmed no `additionalProperties`/`required` drift by re-running `CheckApiContract` locally (see summary
below). Verified against quickstart V1 (TM row) live via curl. V16 (frontend) verified structurally: `ng
build` confirms the `dashboard` route group is a genuinely separate lazy chunk (`summary-component`,
462 KB, listed under "Lazy chunk files" not "Initial chunk files"), behind the functional `authGuard`
already wired in stage 1 (T007); the Jasmine spec (T048) confirms the empty/non-empty render behavior.
**Not verified via an actual browser session** — no browser-automation tool was available in this
environment, so the "read-only, no action controls" and visual-empty-state claims rest on the component
code + spec assertions, not a manual click-through. Flagging this honestly rather than claiming a full
manual UI pass.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T049 🎯 **Wrote the no-write assertion** in `tests/ProjectManagementApp.Api.Tests/Dashboard/NoWriteGuaranteeTests.cs` — three tests: (1) snapshots `activity_logs` row count plus `xmin` of a specific task and project row, exercises **both** endpoints 3× as **every** role (Admin/PM/PM2/TM/TM2), re-snapshots — count and both `xmin`s unchanged; (2) reflects over `DashboardController` asserting no method carries `[HttpPost]`/`[HttpPut]`/`[HttpDelete]`/`[HttpPatch]`; (3) source-scans every non-comment line under `Features/Dashboard/` for `CanMutateAsync` — none found. **3 tests, all passing.** (DoD 8, FR-010)
- [X] T050 [P] Wrote the **UTC-boundary test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/OverdueTimezoneTests.cs`. **Could not literally flip the process clock's timezone in this environment**: `TimeZoneInfo.Local` on Windows is resolved from the OS registry setting, not the `TZ` environment variable — verified empirically (setting `TZ` and calling `TimeZoneInfo.ClearCachedData()` left `TimeZoneInfo.Local` unchanged), and actually changing the sandbox's OS-level timezone is a system-wide, hard-to-reverse action out of scope for an automated test. Proved the guarantee structurally instead: (1) a source scan (same technique as T031/T049) confirms no file under `Features/Dashboard/` ever reads `DateTime.Now`/`DateTime.Today`/`TimeZoneInfo.Local`/`TimeZoneInfo.ConvertTime` — only `DateTimeOffset.UtcNow`; (2) a direct assertion that `DateTimeOffset.UtcNow.Offset` is always `TimeSpan.Zero` — the .NET runtime guarantee that makes the boundary immune to the host's local timezone, given (1) holds; (3) a functional re-run of V6's scenario confirming the boundary is correct at all. **3 tests, all passing.** (research R-2, quickstart V6)
- [X] T051 [P] Wrote the **live-computation test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/LiveComputationTests.cs` — creates a task, fetches the summary (`Done: 0`, `completionRate: 0`), moves the task to `Done` via 003's own `PUT /api/tasks/{id}/status`, re-fetches, and asserts `Done: 1`/`completionRate: 1` appear immediately with `generatedAt` strictly advanced. **1 test, passing.** (Clarifications 2026-07-22, quickstart V14)
- [X] T052 **Baseline was NOT clean** — checked first per instructions rather than assuming, same surprise 002/003/004 each hit in their own polish stages: `dotnet build -p:CheckApiContract=true` failed with 3 real `oasdiff` errors on `GET /api/dashboard/activity` (`page`/`pageSize` missing `format: int32`, unlike every other paginated contract in the repo; and `pageSize`'s contract default `20` mismatching the controller's `0`-sentinel default). **Stopped and reported to the user before fixing anything** (explicit instruction for this exact situation); user approved the fix. Fixed both — added `format: int32` to `dashboard.v1.yaml`'s `page`/`pageSize` (matching `projects.v1.yaml`/`tasks.v1.yaml`), changed `DashboardController.GetActivity`'s `pageSize` default from the `0` sentinel to `20` (matching `TasksController`/`ProjectsController`'s existing convention everywhere else; the handler's `PageSize <= 0 ? DefaultPageSize : …` logic is unchanged, so an explicit `pageSize=0`/negative request still resolves to the configured default). Reconfirmed green (0 errors, only the same pre-existing `etag`/`if-match` warnings 001-004 already accept as baseline). **Then the literal proof itself didn't fail as expected**: removing `Blocked` from `TaskStatusCounts`'s `required` list built clean (0 errors) — `oasdiff` doesn't flag a *response* becoming more permissive in the contract while the code stays stricter (the exact same blind spot 002/003/004 each independently found on their own response-side proofs). **Stopped and reported again**; user approved substituting a rename-based proof, matching 002/003/004's own resolution. Renamed `Blocked` → `TemporaryDriftProbe` in the contract (key + `required` entry) — build correctly **FAILED** with two `response-required-property-removed` errors (`tasksByStatus/TemporaryDriftProbe` and `personalTasks/byStatus/TemporaryDriftProbe` both reported missing from the `200` response). Reverted exactly; reconfirmed green (`dotnet build -p:CheckApiContract=true` exit 0, 0 errors on all four contracts). (quickstart V15)
- [X] T053 Executed the full quickstart validation **V1–V16** live against a running API + the dev Postgres database — see the results table in the final stage summary. All pass; **V7 is N/A pending 006** (only 005's internal-consistency half is checkable — verified via V1/V5/V6); **V9 is structural**, proven by `ActivityReadPathTests`' source scan and reconfirmed by V8/V11's correct live per-role scoping (a coincidentally-correct direct table query could not have produced those exact boundaries).
- [X] T054 [P] Verified **no dashboard-owned migration exists** — `ls src/ProjectManagementApp.Infrastructure/Persistence/Migrations/` shows `InitialCreate`, `AddProjectIndexes`, `AddTaskIndexes`, `AddTeamMemberIndexes`, and `AddActivityLogProjectId`; none of these adds a table/column/index for anything 005 itself owns. **Honest caveat**: `AddActivityLogProjectId` *was* added during this feature's implementation (Stage 3/US2), but it fixes a pre-existing scoping bug in 001's `ActivityLogService.QueryScopedAsync` under explicit user authorization to cross the "do not touch 001-004" boundary for that one blocker — it is not 005-owned persistence, and 005's own aggregates still rely entirely on 002/003/004's existing indexes. Full writeup in Phase 4's checkpoint above. (DoD 8, data-model §1)
- [X] T055 [P] Profiled both endpoints live against the seeded scenario (5 projects, 18+ tasks accumulated across this feature's dev-database quickstart runs) via EF's SQL command log: `GET /summary` issues a **flat 6 `DbCommand`s** (2–6ms each) — `projectsByStatus` group-by, `personalTasks` (2 queries: by-status + overdue count), `tasksByStatus` group-by, `overdueTaskCount`, `visibleTeamMemberCount` distinct-count — independent of scope size, confirming **no N+1**. `GET /activity` issues **2 `DbCommand`s** (page + count). No slow aggregate found; nothing to log for future cached-vs-live revisit. (NFR-003, NFR-005)
- [X] T056 [P] Added XML `<remarks>` to both query handlers stating explicitly that the feature performs no writes, referencing `NoWriteGuaranteeTests` (`GetDashboardSummaryQueryHandler.cs`, `GetDashboardActivityQueryHandler.cs`). `DashboardController` already carried an explicit "Strictly read-only... No write verbs" summary from Stage 2 — verified still accurate, no change needed. (Constitution VIII.3)
- [X] T057 [P] Added a "Dashboard module (005)" section to the root `README.md` (between Team and Documentation, matching the existing per-module style) — both endpoints, the `Dashboard:*` config keys (`Activity:DefaultPageSize`, `Activity:MaxPageSize`, `OverdueBoundary`), an explicit note that the timezone is fixed to UTC by design (no `Dashboard:Timezone` key), and an honest pointer to the one non-dashboard-owned migration from T054.
- [X] T058 Checked for commented-out code and `console.log` across `src/ProjectManagementApp.Web/src/app/features/dashboard/` and the 005 backend slices — **none found**. Every `//`-line match was explanatory prose (design rationale), not disabled code. (Constitution VIII.4)
- [X] T059 Ran a security review against spec 005 §Security Rules, confirming each bullet with a concrete, already-automated proof: **authenticated by default** — plain `[Authorize]` on both actions, no `[Authorize(Roles=...)]` attribute gate anywhere in `DashboardController.cs`; **scope enforced at the query source** — `SummaryFilterAtSourceTests` inspects the generated SQL for a `WHERE project_id IN (SELECT …)` subquery, never fetch-then-filter; **no write path** — T049; **no leakage through totals or paging metadata** — `ActivityScopeTests` asserts `totalCount` itself (not just `items`) is scoped; **activity read only through `IActivityLogService`** — `ActivityReadPathTests`' source scan; **identity from the token** — every handler sources the caller from `ICurrentUserService.Current` (6 references across the two handlers), never a request parameter.

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (Phase 1)** — needs **001–004 complete**; T006 verifies, especially `QueryScopedAsync`
- **Foundational (Phase 2)** — depends on Setup; **blocks all stories**
- **US1 (Phase 3)** — depends on Foundational
- **US2 (Phase 4)** — depends on Foundational only; **fully independent of US1 and US3**
- **US3 (Phase 5)** — **depends on US1** and shares its handler and DTO
- **Polish (Phase 6)** — depends on all stories

### The unusual coupling

**US3 has no endpoint.** It adds `personalTasks` to US1's response, so T046 **edits the file T025 creates**.
The two cannot run in parallel, and **US1 is not shippable alone** — the contract marks `personalTasks`
required, so a summary without it fails the drift gate. Treat **US1 + US3 as one deliverable** with US2
alongside.

That leaves exactly **two parallel tracks**: `US1 → US3` and `US2`.

### Shared-file contention

- `GetDashboardSummaryQueryHandler.cs` — T025 (US1) then T046 (US3), strictly sequential
- `DashboardController.cs` — T026 (US1) and T038 (US2), two small edits
- `dashboard.service.ts` — T029 (US1) and T040 (US2)
- `summary/` component — T028 (US1) then T047 (US3)

### Parallel opportunities

- Setup: T002–T005, T007 all **[P]**
- **Foundational is highly parallel** — T008–T018 are nearly all **[P]**, and the scope resolver, zero-seeding, and metric definitions are independently testable before any endpoint exists
- Every story's test tasks are **[P]**
- Polish is almost entirely **[P]** except T049, T052, T053, T058, T059

---

## Parallel Example: Foundational

```bash
# The three shared building blocks, developed and tested independently:
Task: "Visible-project scope resolver in src/…/Features/Dashboard/Common/VisibleProjectScope.cs"
Task: "Enum zero-seeding helper in src/…/Features/Dashboard/Common/EnumCountMap.cs"
Task: "Consume shared-kernel MetricDefinitions from src/…/Common/Metrics/MetricDefinitions.cs"

# With their tests in parallel:
Task: "Scope subquery not materialized in tests/…/Dashboard/VisibleProjectScopeTests.cs"
Task: "Zero-seeded keys never omitted in tests/…/Dashboard/EnumCountMapTests.cs"
Task: "Overdue boundary + divide-by-zero in tests/…/Common/Metrics/MetricDefinitionsTests.cs"
```

---

## Implementation Strategy

### MVP scope

**Phases 1 → 2 → 3 (US1) → 5 (US3).** Because `personalTasks` is contract-required, the smallest shippable
increment is **US1 plus US3** — the summary endpoint complete. US2's feed can follow.

### Incremental delivery

1. Setup + Foundational → scope, zero-seeding, and metric definitions tested standalone
2. **US1** → tiles render for all three roles → validate (V1–V6)
3. **US3** → the "My Work" panel completes the summary contract
4. **US2** → the activity feed (V8–V11)
5. Polish → **T049** (writes nothing), **T050** (UTC), **T052** (gate proof) are the three that matter

### Critical warnings

- **T008 must not materialize the scope.** `ToListAsync()` there round-trips the whole projects table for an
  Admin and turns every aggregate into a fetch-then-filter.
- **T027 is the security test of this feature** — and it is **meaningless on EF InMemory**, which would
  return identical numbers while loading every row.
- **T049 must assert the absence, not assume it.** The empty audit catalog is a *claim*; this makes it a
  *fact*.
- **Do not add a timezone config key.** UTC is fixed so 006's parity requirement cannot be broken by
  configuration (research R-2).
- **Do not add 403/404 paths** "for consistency" with 002/003/004 — the dashboard names no resource, and an
  empty scope is a valid 200 (research R-3).

---

## Notes

- **[P]** = different files, no dependency on an incomplete task
- Tests are written before implementation within each story; verify they fail first
- Commit per task or logical group, Conventional Commits (Constitution VIII.5)
- Every checkpoint is a safe stopping point for validation or demo
