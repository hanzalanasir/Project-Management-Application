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

- [ ] T001 Create the slice folder structure `src/ProjectManagementApp.Application/Features/Dashboard/{GetSummary,GetActivity}/` per plan.md §Project Structure — **query slices only; no command folders**
- [ ] T002 [P] Generate TypeScript DTO types from `docs/contracts/dashboard.v1.yaml` into `src/ProjectManagementApp.Web/src/app/core/api/generated/` via `openapi-generator` (types only)
- [ ] T003 [P] Add `dashboard.v1.yaml` to the `CheckApiContract` MSBuild target in `src/ProjectManagementApp.Api/ProjectManagementApp.Api.csproj` (ADR-0007 §1)
- [ ] T004 [P] Add the `Dashboard:*` configuration section to `src/ProjectManagementApp.Api/appsettings.json` — `Activity:{DefaultPageSize=20,MaxPageSize=100}` and `OverdueBoundary` (the date **comparison** only). **Do not add a timezone setting** — UTC is fixed, because a configurable one would let a deployment break 006's NFR-002 parity requirement (research R-2)
- [ ] T005 [P] Create `DashboardOptions` binding class in `src/ProjectManagementApp.Api/Configuration/DashboardOptions.cs` and register it in `Program.cs`
- [ ] T006 **Verify prerequisites before proceeding**: **`IActivityLogService.QueryScopedAsync` exists and is implemented**; `IProjectAccessPolicy` and `ITaskAccessPolicy` have implementations registered in DI; `team_members` rows can be created. **Stop and fix the owning feature if any is missing** — do not query `activity_logs` directly as a workaround (FR-006, ADR-0006 addendum)
- [ ] T007 [P] Scaffold the lazy `dashboard` route group in `src/ProjectManagementApp.Web/src/app/features/dashboard/dashboard.routes.ts` behind a functional auth guard, registered with `loadChildren` in `app.routes.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T008 Implement the **visible-project scope resolver** in `src/ProjectManagementApp.Application/Features/Dashboard/Common/VisibleProjectScope.cs` — return `IProjectAccessPolicy.ApplyScope(db.Projects, caller).Select(p => p.Id)` as an **un-materialized `IQueryable<Guid>`**. **Never call `ToListAsync()`** — for an Admin that is the entire projects table round-tripped to the app and back (research R-4)
- [ ] T009 [P] Write scope-resolver tests in `tests/ProjectManagementApp.Application.Tests/Features/Dashboard/VisibleProjectScopeTests.cs` — the three-role sets, and an assertion (via query logging) that the resolver composes as a **subquery** rather than materializing
- [ ] T010 [P] Implement the **enum zero-seeding helper** in `src/ProjectManagementApp.Application/Features/Dashboard/Common/EnumCountMap.cs` — build a map pre-populated with **every** enum value at `0`, then merge query results in. This is what makes the payload a stable typed contract rather than a variable dictionary
- [ ] T011 [P] Write zero-seeding tests in `tests/ProjectManagementApp.Application.Tests/Features/Dashboard/EnumCountMapTests.cs` — a status with no rows appears as `0` and is **never omitted**
- [ ] T012 [P] **Consume** the shared-kernel `MetricDefinitions` from `src/ProjectManagementApp.Application/Common/Metrics/MetricDefinitions.cs` (created by **001 T020**, declared in `docs/shared-contracts.md` **§8**) — `IsOverdue(todayUtc)`, `IsClosed`, `CompletionRate`, `ClosedInWindow`. **Do not create a 005-local copy under `Features/Dashboard/`**: 006 must produce identical values (006 NFR-002), and a feature-local definition would force 006 to depend on 005's Application layer, which ADR-0006's addendum forbids (ADR-0007 §5). **UTC is fixed, not configurable** (research R-2, R-4)
- [ ] T013 [P] Write metric-definition tests in `tests/ProjectManagementApp.Application.Tests/Common/Metrics/MetricDefinitionsTests.cs` — the overdue boundary (due **today** is *not* overdue; due yesterday **is**; `Done` excluded regardless; **no due date is never overdue**), and `completionRate` returning `0` rather than `NaN` on an empty set. **Tests live beside the shared-kernel type, not under `Features/Dashboard/`** — 006 depends on the same behaviour
- [ ] T014 [P] Create `DashboardSummaryDto`, `PersonalTaskSummaryDto`, and `ActivityEntryDto` in `src/ProjectManagementApp.Application/Features/Dashboard/`, matching the contract — **every enum key `required`**, matching `additionalProperties: false`
- [ ] T015 Create the thin `DashboardController` shell in `src/ProjectManagementApp.Api/Controllers/DashboardController.cs` with the two **`GET`** stubs and `[Authorize]` (all three roles). **No write verbs, and no 403/404 paths** — scope shapes content (research R-3)
- [ ] T016 [P] Extend the fixture set in `tests/ProjectManagementApp.Application.Tests/Builders/` with **`LONELY`** — a TeamMember on no teams, required by T032's zero-scope assertion — plus tasks spanning every status, some overdue, some `Done`, some with no due date
- [ ] T017 [P] Implement `DashboardService` in `src/ProjectManagementApp.Web/src/app/core/services/dashboard.service.ts` — the two calls using generated DTO types
- [ ] T018 [P] Create the summary and activity-feed component shells in `src/ProjectManagementApp.Web/src/app/features/dashboard/{summary,activity-feed}/`, and add **Chart.js** to the workspace (Constitution III — first feature to need it)

**Checkpoint**: Scope resolution, zero-seeding, and the metric definitions are tested in isolation.

---

## Phase 3: User Story 1 — Role-scoped summary tiles (Priority: P0) 🎯 MVP

**Goal**: Every authenticated user gets a summary of exactly what they may see — counts by status, overdue,
completion rate, blocked count, team size.

**Independent Test**: The three-role matrix holds; an empty scope returns **200 with zeros**; every enum key
is present.

> **Not complete without US3** — the contract marks `personalTasks` required.

### Tests for User Story 1

- [ ] T019 [P] [US1] Write the 🎯 **three-role scope matrix test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/SummaryScopeTests.cs` — Admin across all; PM across owned only; TM across member-of only, **with task tiles counting only tasks assigned to them** (personal-view, Clarifications 2026-07-22)
- [ ] T020 [P] [US1] Write the 🎯 **stable-contract test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/SummaryContractShapeTests.cs` — **all five `ProjectStatus` and all five `TaskStatus` keys present with `0` for empty ones**, never omitted (DoD 4)
- [ ] T021 [P] [US1] Write the 🎯 **zero-scope test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/SummaryEmptyScopeTests.cs` — `LONELY` receives **200** with all counts zero and all breakdowns present. **Not 403, not 404, not an empty body** (DoD 5, FR-007)
- [ ] T022 [P] [US1] Write metric tests in `tests/ProjectManagementApp.Api.Tests/Dashboard/SummaryMetricTests.cs` — `overdueTaskCount` per the boundary rule; `completionRate` = `0` with no tasks and `0.25` with 3 of 12 `Done`; `blockedTaskCount` derived from `tasksByStatus.Blocked`; `visibleTeamMemberCount` counts a user on several visible projects **once**
- [ ] T023 [P] [US1] Write handler tests in `tests/ProjectManagementApp.Application.Tests/Features/Dashboard/GetDashboardSummaryQueryHandlerTests.cs` — each aggregate composed against the scope subquery, and no aggregate executed before scope is applied

### Implementation for User Story 1

- [ ] T024 [US1] Create `GetDashboardSummaryQuery` in `src/ProjectManagementApp.Application/Features/Dashboard/GetSummary/GetDashboardSummaryQuery.cs` — **no parameters**; scope is derived from the caller, never passed in
- [ ] T025 [US1] Implement `GetDashboardSummaryQueryHandler` in `src/ProjectManagementApp.Application/Features/Dashboard/GetSummary/GetDashboardSummaryQueryHandler.cs` — build the visible-project subquery (T008), then run each metric as a **single `GROUP BY` pushed to SQL**: projects-by-status, tasks-by-status, overdue, team headcount. Merge into zero-seeded maps (T010), derive `completionRate` and `blockedTaskCount` **with no extra query**, and assemble the typed DTO. **Live per request — no caching** (Clarifications 2026-07-22)
- [ ] T026 [US1] Wire `GET /api/dashboard/summary` in `src/ProjectManagementApp.Api/Controllers/DashboardController.cs` — `[Authorize]`, one `Send`. **No 403 or 404 path exists on this endpoint**
- [ ] T027 [US1] 🎯 Write the **filter-at-source test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/SummaryFilterAtSourceTests.cs` — capture a PM's summary, add ten tasks to a project they cannot see, re-fetch and assert **every number is unchanged**; then inspect the generated SQL and confirm the scope appears as `WHERE project_id IN (<subquery>)`, **not** a post-query filter (DoD 3, NFR-002)
- [ ] T028 [P] [US1] Build the summary tiles in `src/ProjectManagementApp.Web/src/app/features/dashboard/summary/` — fixed-N read-only tiles plus small **Chart.js** status charts; explicit loading, empty ("nothing assigned to you yet"), and error states; **no pagination**
- [ ] T029 [US1] Implement `DashboardService.getSummary()` and refetch on navigation in `src/ProjectManagementApp.Web/src/app/core/services/dashboard.service.ts`

**Checkpoint**: 🎯 **MVP** — the headline payload. Verify against quickstart V1–V6.

---

## Phase 4: User Story 2 — Role-scoped recent activity feed (Priority: P1)

**Goal**: A paginated feed of activity on entities the caller can see, read through the audit service.

**Independent Test**: Per-role scoping holds with a scoped `totalCount`, paging clamps, and the handler
provably calls `IActivityLogService`.

### Tests for User Story 2

- [ ] T030 [P] [US2] Write the feed scope test in `tests/ProjectManagementApp.Api.Tests/Dashboard/ActivityScopeTests.cs` — Admin sees all; PM only owned-project activity; TM only member-of. **Assert `totalCount` is scoped** — a TeamMember must never learn the system-wide activity volume
- [ ] T031 [P] [US2] Write the 🎯 **service-not-table test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/ActivityReadPathTests.cs` — assert the handler calls **`IActivityLogService.QueryScopedAsync`**, and that **no LINQ query against `db.ActivityLogs` exists anywhere under `Features/Dashboard/`** (FR-006, DoD 6)
- [ ] T032 [P] [US2] Write paging tests in `tests/ProjectManagementApp.Api.Tests/Dashboard/ActivityPagingTests.cs` — default `pageSize` **20**; `pageSize=500` **clamped to 100**; `page=999` returns empty items with valid metadata; `page=-1` → **400**; ordering newest-first and stable across requests
- [ ] T033 [P] [US2] Write the invisible-project test in `tests/ProjectManagementApp.Api.Tests/Dashboard/ActivityInvisibleProjectTests.cs` — activity for a project the caller can no longer see (ownership transferred, or deleted — audit rows survive) is **scoped out for non-Admins** and remains visible to **Admin**
- [ ] T034 [P] [US2] Write empty-feed test in `tests/ProjectManagementApp.Api.Tests/Dashboard/ActivityEmptyTests.cs` — an empty scope or a fresh system returns **200 with an empty page**, never 404

### Implementation for User Story 2

- [ ] T035 [US2] Create `GetDashboardActivityQuery` in `src/ProjectManagementApp.Application/Features/Dashboard/GetActivity/GetDashboardActivityQuery.cs` — `Page` and `PageSize` only; **scope is derived from the caller, never accepted from the request**
- [ ] T036 [US2] Implement `GetDashboardActivityQueryValidator` in `src/ProjectManagementApp.Application/Features/Dashboard/GetActivity/GetDashboardActivityQueryValidator.cs` — paging bounds (negative or non-numeric → 400)
- [ ] T037 [US2] Implement `GetDashboardActivityQueryHandler` in `src/ProjectManagementApp.Application/Features/Dashboard/GetActivity/GetDashboardActivityQueryHandler.cs` — build the visible-project scope, call **`IActivityLogService.QueryScopedAsync(scope, page, pageSize)`**, project to `ActivityEntryDto`. **Never touch `db.ActivityLogs` directly**
- [ ] T038 [US2] Wire `GET /api/dashboard/activity` in `src/ProjectManagementApp.Api/Controllers/DashboardController.cs` — `[Authorize]`, one `Send`; the only error statuses are **400** and **401**
- [ ] T039 [P] [US2] Build the activity-feed widget in `src/ProjectManagementApp.Web/src/app/features/dashboard/activity-feed/` — rows showing actor, action, entity type/id, relative timestamp, change summary; infinite-scroll or pager over `PagedResult<T>`; explicit empty/loading/error states. **Strictly read-only — no "mark read", no row actions**
- [ ] T040 [US2] Implement `DashboardService.getActivity()` with paging parameters in `src/ProjectManagementApp.Web/src/app/core/services/dashboard.service.ts`

**Checkpoint**: Verify against quickstart V8–V11.

---

## Phase 5: User Story 3 — My personal task slice (Priority: P1)

**Goal**: A focused view of the tasks assigned to the caller — count, by-status, overdue — inside the
summary payload.

**Independent Test**: The slice counts only `assignee_id == caller` within member-of projects; a user with
no assignments gets zeros, not a 404.

> **Extends US1's handler and DTO — not parallelizable with US1.**

### Tests for User Story 3

- [ ] T041 [P] [US3] Write the personal-slice test in `tests/ProjectManagementApp.Api.Tests/Dashboard/PersonalSliceTests.cs` — counts only tasks where `assignee_id == caller`; a **colleague's tasks on the same project are excluded**; a user with no assignments gets zeros, not 404
- [ ] T042 [P] [US3] Write the tile-consistency test in `tests/ProjectManagementApp.Api.Tests/Dashboard/PersonalSliceTileConsistencyTests.cs` — 🎯 **for a TeamMember, `personalTasks.byStatus` and the summary's `tasksByStatus` are the same numbers** (personal-view; there is no divergent project-wide count in v1)
- [ ] T043 [P] [US3] Write scope-boundary test in `tests/ProjectManagementApp.Api.Tests/Dashboard/PersonalSliceScopeTests.cs` — a task assigned to the caller in a project they are **not** a member of is excluded; note 004's removal-block invariant means this cannot occur for an **open** task anyway
- [ ] T044 [P] [US3] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Dashboard/PersonalSliceTests.cs` — the assignee predicate and member-of scope are **both applied in the query**, not in memory

### Implementation for User Story 3

- [ ] T045 [US3] Implement the personal-slice aggregate in `src/ProjectManagementApp.Application/Features/Dashboard/GetSummary/PersonalTaskSlice.cs` — the task aggregates re-run with the extra `assignee_id == caller` predicate, zero-seeded like the others
- [ ] T046 [US3] **Extend** `GetDashboardSummaryQueryHandler` in `src/ProjectManagementApp.Application/Features/Dashboard/GetSummary/GetDashboardSummaryQueryHandler.cs` to populate `personalTasks`, and — for a **TeamMember** — source the summary's `tasksByStatus`/`overdueTaskCount` tiles from the same slice (personal-view). **Shared file with T025; sequence after it**
- [ ] T047 [P] [US3] Build the **"My Work"** panel inside the summary view at `src/ProjectManagementApp.Web/src/app/features/dashboard/summary/` — my-task count, by-status mini-breakdown, my-overdue count, and a link into 003's task list filtered to the caller. Empty state: "You have no tasks assigned."
- [ ] T048 [P] [US3] Write a Jasmine test asserting the "My Work" panel renders zeros rather than an error for a user with no assignments, in `src/ProjectManagementApp.Web/src/app/features/dashboard/summary/summary.component.spec.ts`

**Checkpoint**: All three stories complete; `personalTasks` satisfies the contract's `required`. Verify
against quickstart V1 (TM row), V16.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T049 🎯 **Write the no-write assertion** in `tests/ProjectManagementApp.Api.Tests/Dashboard/NoWriteGuaranteeTests.cs` — snapshot `activity_logs` count, exercise **both** endpoints repeatedly as **every** role, and assert the count is **unchanged**; assert no domain row was modified and no `xmin` bumped; assert **no POST/PUT/DELETE route exists under `/api/dashboard`** and that **`CanMutateAsync` is not referenced anywhere under `Features/Dashboard/`** (DoD 8, FR-010)
- [ ] T050 [P] Write the **UTC-boundary test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/OverdueTimezoneTests.cs` — run the overdue assertion with the process clock in a **non-UTC** zone and confirm the count is **unchanged**. This is what makes 006's NFR-002 parity requirement satisfiable (research R-2, quickstart V6)
- [ ] T051 [P] Write the **live-computation test** in `tests/ProjectManagementApp.Api.Tests/Dashboard/LiveComputationTests.cs` — fetch the summary, mutate a task status via 003, re-fetch, and assert the new value appears **immediately** with `generatedAt` advanced (Clarifications 2026-07-22, quickstart V14)
- [ ] T052 **Prove the contract gate fails**: temporarily remove one enum key from `TaskStatusCounts`'s `required` list, run `dotnet build -p:CheckApiContract=true`, confirm the build **fails**, then revert. That is exactly how the stable typed contract would silently degrade into a variable dictionary (quickstart V15)
- [ ] T053 Execute the full quickstart validation **V1–V16** in `specs/005-dashboard/quickstart.md` and record results
- [ ] T054 [P] Verify **no migration was added by 005** — `ls src/ProjectManagementApp.Infrastructure/Persistence/Migrations/` shows nothing new, and no index was created here. Aggregates rely on 002/003/004's indexes (DoD 8, data-model §1)
- [ ] T055 [P] Profile each summary aggregate against a seeded dataset and record timings in the PR — confirm **no N+1**, and log slow aggregates so the live-vs-cached decision can be revisited under real load (NFR-003, NFR-005)
- [ ] T056 [P] Add XML doc comments to `DashboardController` and both query handlers, noting explicitly that the feature performs no writes (Constitution VIII.3)
- [ ] T057 [P] Update the root `README.md` with the dashboard module — the two endpoints, the `Dashboard:*` keys, and a note that **the timezone is fixed to UTC by design**
- [ ] T058 Remove commented-out code and any `console.log` across `src/ProjectManagementApp.Web/src/app/features/dashboard/` and the 005 backend slices (Constitution VIII.4)
- [ ] T059 Run a security review against spec 005 §Security Rules — scope enforced in the query source, no write path, no leakage through totals or paging metadata, identity from the token

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
