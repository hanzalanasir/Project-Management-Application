---
description: "Task list for 006 Reports implementation"
---

# Tasks: 006 Reports

**Input**: Design documents from `/specs/006-reports/`
**Prerequisites**: [plan.md](plan.md) · [spec.md](spec.md) · [research.md](research.md) ·
[data-model.md](data-model.md) · [contracts/](contracts/README.md) → [`docs/contracts/reports.v1.yaml`](../../docs/contracts/reports.v1.yaml) · [quickstart.md](quickstart.md)

**Tests**: **REQUIRED.** Constitution IX and spec 006 B.8 DoD #13. **Docker required** — Testcontainers
PostgreSQL (ADR-0007 §2).

**Organization**: Grouped by user story so each is independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story the task serves
- Exact file paths are included in every task

---

## ⚠️ Blocking prerequisites — the last feature depends on all five

| From | Artifact | Used by 006 |
|---|---|---|
| **001** | **`IActivityLogService.QueryScopedAsync`** | the Activity Report's **only legal** read path (FR-007) |
| **001** | `IActivityLogService.LogAsync` + `AuditAction.ReportGenerated` (T018) | the one write per generation |
| **002** | `IProjectAccessPolicy.ApplyScope` | every report's visible-project set |
| **003** | `ITaskAccessPolicy.ApplyScope` | task-scoped aggregates |
| **003** | **`tasks.closed_at`** — set on → `Done`, cleared on re-open | **every completion metric**; `updated_at` is not a substitute |
| **004** | `team_members` rows | the Team Performance member pool |
| **001** | **`MetricDefinitions`** (shared kernel, §8 — overdue, closed, completionRate, throughput; **UTC fixed**) | **imported from `Common/Metrics/`, not re-implemented** — this is what makes T083's parity test pass by construction. **Not** from `Features/Dashboard/`: importing 005's feature folder would violate ADR-0006's addendum (analyze finding G1) |

**T007 verifies these.** `closed_at` and 005's shared metric definitions are the two that would fail
silently rather than loudly if absent.

---

## What 006 deliberately does NOT build

| Absent | Why | Reference |
|---|---|---|
| **Export endpoint / `?format` param** | The API returns **JSON only**; jsPDF and papaparse render both formats in the browser. Constitution III locks jsPDF — a *browser* library — so client-side is the only compliant path | research R-3 |
| **Server-side PDF/CSV, streaming writers, temp files** | No backend export infrastructure at all | research R-3 |
| **`ReportArtifact` / `ReportSchedule` tables** | Reports are transient (OQ-006-03); scheduling is out of scope (OQ-006-01) | data-model §7 |
| **Any domain write** | Read-only over domain data. The **one** write is a `ReportGenerated` audit row | research R-1 |
| **Migration** | Aggregation only | data-model §1 |

**T082 asserts the export architecture** and **T081 asserts exactly-one-audit.** Both exist so the design
is *proven*, not merely intended.

---

## Story ID mapping & implementation order

| Label | Spec story | Title | Priority | Endpoint |
|---|---|---|---|---|
| **US1** | US-006-01 | Report catalog | P1 | `GET /api/reports/catalog` |
| **US2** | US-006-02 | Project Progress | **P0** | `GET /api/reports/project-progress` |
| **US3** | US-006-03 | Task Completion | P1 | `GET /api/reports/task-completion` |
| **US4** | US-006-04 | Team Performance | P1 | `GET /api/reports/team-performance` |
| **US5** | US-006-05 | Activity Report | P1 | `GET /api/reports/activity` |
| **US6** | US-006-06 | Export to PDF/CSV + audit | **P0** | **none — see below** |

> ### ⚠️ US6 has no endpoint, and splits in two
>
> **US6 is a capstone, not a sibling.** It has two halves:
> - **The audit write** lives *inside* US2–US5's handlers. To avoid four stories editing one file, the
>   shared `ReportGenerationAudit` helper is built in **Foundational (T017)** and each report handler calls
>   it. US6 then only *asserts* the behaviour.
> - **The export** is **pure frontend** — one `ReportExportService` consumed by all four report views.
>
> So **US6 depends on US2–US5** and cannot precede them, despite being P0. Dependency beats priority here.

---

## Phase 1: Setup

- [ ] T001 Create the slice folder structure `src/ProjectManagementApp.Application/Features/Reports/{GetCatalog,GetProjectProgress,GetTaskCompletion,GetTeamPerformance,GetActivityReport}/` — **query slices only, no commands**
- [ ] T002 [P] Generate TypeScript DTO types from `docs/contracts/reports.v1.yaml` into `src/ProjectManagementApp.Web/src/app/core/api/generated/` via `openapi-generator` (types only)
- [ ] T003 [P] Add `reports.v1.yaml` to the `CheckApiContract` MSBuild target in `src/ProjectManagementApp.Api/ProjectManagementApp.Api.csproj` (ADR-0007 §1)
- [ ] T004 [P] Add the `Reports:*` configuration section to `src/ProjectManagementApp.Api/appsettings.json` — `DefaultWindowDays=30`, `MaxWindowDays`, `Activity:{DefaultPageSize=20,MaxPageSize=100}`, `LargeReportRowThreshold=10000`, `LargeReportFallback=ForceNarrow`, `EnabledTypes`, `AuditOnGeneration=true`, `MaskOutOfScopeAs404=false`, `DownloadFilenamePattern`. **`TimeZone` is fixed to UTC and is deliberately not a knob** (research R-4)
- [ ] T005 [P] Create `ReportsOptions` binding class in `src/ProjectManagementApp.Api/Configuration/ReportsOptions.cs` and register it in `Program.cs`
- [ ] T006 [P] Add **jsPDF** and **papaparse** to `src/ProjectManagementApp.Web/package.json` (Constitution III) — no backend export packages
- [ ] T007 **Verify prerequisites before proceeding**: `QueryScopedAsync` implemented; `AuditAction.ReportGenerated` exists; **`tasks.closed_at` is populated by 003's status handler**; 005's `MetricDefinitions` are importable; `IProjectAccessPolicy`/`ITaskAccessPolicy` registered. **Stop and fix the owning feature if any is missing** — in particular, do **not** substitute `updated_at` for `closed_at` (it moves on later edits and would corrupt every completion metric)
- [ ] T008 [P] Scaffold the lazy `reports` route group in `src/ProjectManagementApp.Web/src/app/features/reports/reports.routes.ts` behind a functional role guard, registered with `loadChildren` in `app.routes.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T009 [P] Implement the shared `ReportEnvelope` in `src/ProjectManagementApp.Application/Features/Reports/Common/ReportEnvelope.cs` — `reportType`, `generatedAt`, `scope`, `window{from,to}`, and **`timeZone` always `"UTC"`**, matching the contract
- [ ] T010 [P] Implement `ReportWindow` parsing/validation in `src/ProjectManagementApp.Application/Features/Reports/Common/ReportWindow.cs` — `from`/`to` required, **evaluated in UTC**, `from > to` → validation error, `MaxWindowDays` guard
- [ ] T011 [P] Write window tests in `tests/ProjectManagementApp.Application.Tests/Features/Reports/ReportWindowTests.cs` — `from > to` rejected; boundaries inclusive; **the same window yields the same buckets regardless of process timezone**
- [ ] T012 Implement the **report scope resolver** in `src/ProjectManagementApp.Application/Features/Reports/Common/ReportScope.cs` — reuse `IProjectAccessPolicy.ApplyScope` as an **un-materialized `IQueryable<Guid>`**. Parse `projectScope` as either the literal **`all`** (narrow to the visible set silently, **no 403**) **or a comma-separated list of project ids** (research R-7, FR-002): validate each id's format (malformed or empty list → `ErrorKind.Validation` → **400**), then require **every** named id to be within scope — **if any one is not, fail the whole request** with `ErrorKind.Forbidden` → **403**. **Never partially fulfil** by silently dropping out-of-scope ids: that would return a report that appears to cover everything requested but does not (FR-004)
- [ ] T013 [P] Write scope-resolver tests in `tests/ProjectManagementApp.Application.Tests/Features/Reports/ReportScopeTests.cs` — `all` auto-narrows with **no 403**; a single named out-of-scope id **does** 403; **a comma-separated list of all-in-scope ids succeeds**; **a list where even one id is out of scope returns 403 for the whole request** (not a partial result); a malformed id or empty list → **400**; the resolver composes as a subquery rather than materializing
- [ ] T014 [P] Implement the **report counting rules** in `src/ProjectManagementApp.Application/Features/Reports/Common/ReportCountingRules.cs` by **importing the shared-kernel `MetricDefinitions`** from `src/ProjectManagementApp.Application/Common/Metrics/` (created by **001 T020**, declared in `docs/shared-contracts.md` **§8**) — `IsClosed` ⇔ `status = Done` (⇔ `closed_at` not null); a **re-opened** task (`closed_at` cleared) is **excluded** from closed counts *and* completion buckets; `IsOverdue` ⇔ `due_date < today (UTC)` and not `Done`; `ClosedInWindow` ⇔ throughput. **Do not re-implement these, and do not import from `Features/Dashboard/`** — the first causes drift that only T083 would catch after the fact; the second makes 006 depend on 005's Application layer, which ADR-0006's addendum forbids (research R-4, analyze G1)
- [ ] T015 [P] Write counting-rule tests in `tests/ProjectManagementApp.Application.Tests/Features/Reports/ReportCountingRulesTests.cs` — the re-open exclusion applies **identically** to Project Progress's `closedTasks` and Task Completion's buckets; divide-by-zero yields `0`
- [ ] T016 [P] Implement the **large-report threshold guard** in `src/ProjectManagementApp.Application/Features/Reports/Common/RowThresholdGuard.cs` — run an **indexed `CountAsync()` over the scoped *and* filtered query** (the *same* predicate the real query uses), **before any paging or projection**; return `ErrorKind.UnprocessableContent` → **422** when the count exceeds `LargeReportRowThreshold`. **Do not estimate heuristically, do not use `EXPLAIN` statistics, and do not fetch `threshold + 1` rows** — the first two make the threshold advisory, the third materializes most of what the guard exists to prevent (research R-5)
- [ ] T017 Implement `ReportGenerationAudit` in `src/ProjectManagementApp.Application/Features/Reports/Common/ReportGenerationAudit.cs` — write **exactly one** `ReportGenerated` row via `IActivityLogService.LogAsync` with `entity_type='Report'`, a generated run id as `entity_id`, and report type + serialized parameters in `change_summary`. Gated by `Reports:AuditOnGeneration`. **Shared by all four report handlers so no story edits another's file** (research R-1)
- [ ] T018 [P] Write audit-helper tests in `tests/ProjectManagementApp.Application.Tests/Features/Reports/ReportGenerationAuditTests.cs` — one row per invocation, correct `entity_type`, parameters captured, **and no domain entity touched**
- [ ] T019 **Verify** that the existing `ErrorKind.UnprocessableContent` (declared in `docs/shared-contracts.md` §1, created by **001 T019**, mapped to **422** by **001 T036**) is wired end-to-end, and that 006's handlers return it for the large-report guard. **006 consumes this member; it does not introduce it** — extending a shared-kernel enum from a downstream feature is forbidden by ADR-0007 §5. If it is missing, **patch 001 first**. 422 is absent from Constitution VI.2's list but is **declared by this spec** and resolved via OQ-006-02 — a spec may declare a status code; a plan may not invent one (research R-5, plan Complexity Tracking)
- [ ] T020 Create the thin `ReportsController` shell in `src/ProjectManagementApp.Api/Controllers/ReportsController.cs` with the five **`GET`** stubs and `[Authorize]` (all three roles). **No write verbs, no `?format` parameter, no export route** (research R-3)
- [ ] T021 [P] Implement `ReportsService` in `src/ProjectManagementApp.Web/src/app/core/services/reports.service.ts` — the five calls using generated DTO types
- [ ] T022 [P] Create the report-view component shells in `src/ProjectManagementApp.Web/src/app/features/reports/{picker,project-progress,task-completion,team-performance,activity}/` with routes wired in `reports.routes.ts`
- [ ] T023 [P] Extend the fixture set in `tests/ProjectManagementApp.Application.Tests/Builders/` — tasks **closed at known dates spread across a window**, some re-opened, some overdue, plus members with differing throughput. The re-opened task is what T015 and T041 depend on

**Checkpoint**: Envelope, scope, counting rules, threshold guard, and the audit helper are tested in
isolation, before any endpoint exists.

---

## Phase 3: User Story 1 — Report catalog (Priority: P1)

**Goal**: A self-describing catalog so the frontend builds parameter forms dynamically.

**Independent Test**: Returns exactly four descriptors with ordered parameters and formats; role-annotated;
**writes no audit row**.

### Tests for User Story 1

- [ ] T024 [P] [US1] Write the catalog test in `tests/ProjectManagementApp.Api.Tests/Reports/CatalogTests.cs` — **exactly four** descriptors (`ProjectProgress`, `TaskCompletion`, `TeamPerformance`, `Activity`); each lists its ordered `parameters` with `required` flags and `formats` `["json","pdf","csv"]`; the body is a **plain array**, not paged
- [ ] T025 [P] [US1] Write the role-annotation test in `tests/ProjectManagementApp.Api.Tests/Reports/CatalogAnnotationTests.cs` — a TeamMember sees Team Performance annotated **"self only"**; the catalog exposes **no** project or task data
- [ ] T026 [P] [US1] Write the 🎯 **catalog-not-audited test** in `tests/ProjectManagementApp.Api.Tests/Reports/CatalogNotAuditedTests.cs` — calling `/catalog` repeatedly adds **zero** `ReportGenerated` rows (FR-011)

### Implementation for User Story 1

- [ ] T027 [US1] Create `ReportDescriptor` and the static descriptor set in `src/ProjectManagementApp.Application/Features/Reports/GetCatalog/ReportDescriptor.cs`, matching the contract schema
- [ ] T028 [US1] Create `GetReportCatalogQuery` and its handler in `src/ProjectManagementApp.Application/Features/Reports/GetCatalog/` — return role-annotated descriptors. **Do not call `ReportGenerationAudit`** — the catalog is metadata only
- [ ] T029 [US1] Wire `GET /api/reports/catalog` in `src/ProjectManagementApp.Api/Controllers/ReportsController.cs` — `[Authorize]`, one `Send`
- [ ] T030 [P] [US1] Build the 🎯 **catalog-driven report picker** in `src/ProjectManagementApp.Web/src/app/features/reports/picker/` — Reactive Forms constructed **from the descriptors at runtime**, so a new report type needs no new form code. Explicit loading/error states
- [ ] T031 [US1] Implement `ReportsService.getCatalog()` in `src/ProjectManagementApp.Web/src/app/core/services/reports.service.ts`

**Checkpoint**: The picker renders forms it did not hard-code. Verify against quickstart V1.

---

## Phase 4: User Story 2 — Project Progress (Priority: P0) 🎯 flagship

**Goal**: Per-project completion %, open/closed, overdue, and projected completion over a window — with
overdue **equal to the Dashboard's** value.

**Independent Test**: The three-role scope matrix holds; a named out-of-scope project 403s; `all` narrows
silently; one audit row is written.

### Tests for User Story 2

- [ ] T032 [P] [US2] Write the scope matrix test in `tests/ProjectManagementApp.Api.Tests/Reports/ProjectProgressScopeTests.cs` — Admin all; PM owned only; TM member-of only; **empty scope → 200 with empty `rows`**, never 403
- [ ] T033 [P] [US2] Write the named-out-of-scope test in `tests/ProjectManagementApp.Api.Tests/Reports/ProjectProgressForbiddenTests.cs` — a **named** `projectScope=<other PM's project>` → **403**; `projectScope=all` → **200**, silently narrowed; **a comma-separated list of in-scope ids → 200 covering exactly those**; **a list mixing in-scope and out-of-scope ids → 403 for the whole request**, with nothing partially returned (FR-002, FR-004, research R-7)
- [ ] T034 [P] [US2] Write metric tests in `tests/ProjectManagementApp.Api.Tests/Reports/ProjectProgressMetricTests.cs` — `completionPercent` = 25 with 3 of 12 closed; **`0` when `totalTasks = 0`** (no divide-by-zero); `openTasks` + `closedTasks` = `totalTasks`
- [ ] T035 [P] [US2] Write the 🎯 **projected-completion test** in `tests/ProjectManagementApp.Api.Tests/Reports/ProjectedCompletionTests.cs` — a plausible date with steady throughput; **`null`** when `avgClosedPerDay = 0`; **`null`** when `openTasks = 0`. Never a divide-by-zero, never a past date presented as a projection
- [ ] T036 [P] [US2] Write the audit test in `tests/ProjectManagementApp.Api.Tests/Reports/ProjectProgressAuditTests.cs` — **exactly one** `ReportGenerated` row per successful request; **zero** when the request 400s or 403s
- [ ] T037 [P] [US2] Write handler tests in `tests/ProjectManagementApp.Application.Tests/Features/Reports/GetProjectProgressQueryHandlerTests.cs` — scope composed before aggregation; counting rules imported, not re-implemented

### Implementation for User Story 2

- [ ] T038 [US2] Create `GetProjectProgressQuery` + `GetProjectProgressQueryValidator` in `src/ProjectManagementApp.Application/Features/Reports/GetProjectProgress/` — window required, `from ≤ to`
- [ ] T039 [US2] Implement `GetProjectProgressQueryHandler` in `src/ProjectManagementApp.Application/Features/Reports/GetProjectProgress/GetProjectProgressQueryHandler.cs` — resolve scope (T012), aggregate per project using the **shared counting rules** (T014), compute `projectedCompletion` as `today(UTC) + ceil(openTasks ÷ avgClosedPerDay)` with the two `null` cases, assemble envelope + `rows` + `totals`, then call **`ReportGenerationAudit`** (T017)
- [ ] T040 [US2] Wire `GET /api/reports/project-progress` in `src/ProjectManagementApp.Api/Controllers/ReportsController.cs`
- [ ] T041 [P] [US2] Build the Project Progress view in `src/ProjectManagementApp.Web/src/app/features/reports/project-progress/` — parameter form (from the catalog), a **Chart.js** progress visualization, a tabular body paginated **client-side** over the bounded row set, and Export controls
- [ ] T042 [US2] Implement `ReportsService.getProjectProgress()` in `src/ProjectManagementApp.Web/src/app/core/services/reports.service.ts`

**Checkpoint**: 🎯 The flagship report works end to end. Verify against quickstart V2, V3, V5, V8.

---

## Phase 5: User Story 3 — Task Completion (Priority: P1)

**Goal**: A completion trend bucketed by day/week/month, zero-filled and continuous.

**Independent Test**: Buckets change with `groupBy`; empty periods appear as `0`; a re-opened task drops out.

### Tests for User Story 3

- [ ] T043 [P] [US3] Write the bucketing test in `tests/ProjectManagementApp.Api.Tests/Reports/TaskCompletionBucketTests.cs` — `groupBy=week` over a 4-week window with completions in 2 weeks yields **four buckets, two with `0`** (zero-filled continuous series); invalid `groupBy` → **400**
- [ ] T044 [P] [US3] Write the 🎯 **UTC boundary test** in `tests/ProjectManagementApp.Api.Tests/Reports/TaskCompletionTimezoneTests.cs` — a task closed at `23:30 UTC` on a boundary lands in the bucket determined **by UTC**; re-run with a non-UTC process clock and assert the bucket is **unchanged**
- [ ] T045 [P] [US3] Write the 🎯 **re-open exclusion test** in `tests/ProjectManagementApp.Api.Tests/Reports/TaskCompletionReopenTests.cs` — re-opening a completed task (003 clears `closed_at`) **removes it from its bucket**, and the **same** task disappears from Project Progress's `closedTasks`. One rule, both reports (FR-017)
- [ ] T046 [P] [US3] Write the assignee-filter test in `tests/ProjectManagementApp.Api.Tests/Reports/TaskCompletionAssigneeFilterTests.cs` — honoured for Admin/PM within scope; **constrained to self for a TeamMember** (they cannot trend a colleague)
- [ ] T047 [P] [US3] Write handler tests in `tests/ProjectManagementApp.Application.Tests/Features/Reports/GetTaskCompletionQueryHandlerTests.cs` — grouped aggregate executed in SQL; zero-fill applied after

### Implementation for User Story 3

- [ ] T048 [US3] Create `GetTaskCompletionQuery` + validator in `src/ProjectManagementApp.Application/Features/Reports/GetTaskCompletion/` — window required, `groupBy` in `day|week|month`
- [ ] T049 [US3] Implement the **bucket generator** in `src/ProjectManagementApp.Application/Features/Reports/GetTaskCompletion/BucketGenerator.cs` — enumerate every period in the window (UTC) so the series is **continuous and zero-filled**, then merge counts in
- [ ] T050 [US3] Implement `GetTaskCompletionQueryHandler` in `src/ProjectManagementApp.Application/Features/Reports/GetTaskCompletion/GetTaskCompletionQueryHandler.cs` — scope, group by `closed_at` period using the shared counting rules, zero-fill, clamp a TeamMember's `assigneeId` to self, then audit
- [ ] T051 [US3] Wire `GET /api/reports/task-completion` in `src/ProjectManagementApp.Api/Controllers/ReportsController.cs`
- [ ] T052 [P] [US3] Build the Task Completion view in `src/ProjectManagementApp.Web/src/app/features/reports/task-completion/` — window + grouping + optional filters, a **Chart.js** line/bar trend, a per-bucket table, Export controls
- [ ] T053 [US3] Implement `ReportsService.getTaskCompletion()` in `src/ProjectManagementApp.Web/src/app/core/services/reports.service.ts`

**Checkpoint**: Verify against quickstart V5, V7.

---

## Phase 6: User Story 4 — Team Performance (Priority: P1)

**Goal**: Per-member throughput, workload, and overdue — with the sharpest least-privilege boundary in the
product.

**Independent Test**: 🎯 A TeamMember always receives **exactly one row — their own** — regardless of any
`userId`, and **never a 403**.

### Tests for User Story 4

- [ ] T054 [P] [US4] Write the 🎯 **self-only clamp test** in `tests/ProjectManagementApp.Api.Tests/Reports/TeamPerformanceSelfOnlyTests.cs` — a TeamMember passing a **colleague's** `userId` receives **one row: their own**, status **200**, **not 403**. A 403 would *confirm the colleague exists and is out of scope* — exactly the inference a peer-comparison report must not permit (research R-6, DoD 3)
- [ ] T055 [P] [US4] Write the contrasting test in `tests/ProjectManagementApp.Api.Tests/Reports/TeamPerformanceForbiddenTests.cs` — **Admin and ProjectManager** naming an out-of-scope `userId` **do** get **403**; they are entitled to know their own scope boundary
- [ ] T056 [P] [US4] Write the metric test in `tests/ProjectManagementApp.Api.Tests/Reports/TeamPerformanceMetricTests.cs` — `throughput` counts tasks whose `closed_at` is in the window; `workload` counts currently-assigned non-`Done`; `overdueCount` per the shared rule
- [ ] T057 [P] [US4] Write the visibility test in `tests/ProjectManagementApp.Api.Tests/Reports/TeamPerformanceVisibilityTests.cs` — a member with **no activity** appears as a **row of zeros** (absence is visible, not omitted); a **deactivated** member with in-window throughput is still shown, flagged
- [ ] T058 [P] [US4] Write handler tests in `tests/ProjectManagementApp.Application.Tests/Features/Reports/GetTeamPerformanceQueryHandlerTests.cs` — the self-clamp is applied **in the handler**, before aggregation, and cannot be bypassed by any parameter

### Implementation for User Story 4

- [ ] T059 [US4] Create `GetTeamPerformanceQuery` + validator in `src/ProjectManagementApp.Application/Features/Reports/GetTeamPerformance/`
- [ ] T060 [US4] Implement `GetTeamPerformanceQueryHandler` in `src/ProjectManagementApp.Application/Features/Reports/GetTeamPerformance/GetTeamPerformanceQueryHandler.cs` — **if the caller is a TeamMember, force `userId` to their own id and ignore any supplied value** (no 403); otherwise honour the filter within scope or 403. Aggregate per member over 004's pool using the shared counting rules, include zero-rows, then audit
- [ ] T061 [US4] Wire `GET /api/reports/team-performance` in `src/ProjectManagementApp.Api/Controllers/ReportsController.cs`
- [ ] T062 [P] [US4] Build the Team Performance view in `src/ProjectManagementApp.Web/src/app/features/reports/team-performance/` — a **Chart.js bar comparison** for Admin/PM, and a **single-member card** for a TeamMember; Export controls
- [ ] T063 [US4] Implement `ReportsService.getTeamPerformance()` in `src/ProjectManagementApp.Web/src/app/core/services/reports.service.ts`

**Checkpoint**: The least-privilege boundary holds. Verify against quickstart V4.

---

## Phase 7: User Story 5 — Activity Report (Priority: P1)

**Goal**: A filtered, paginated activity excerpt — the one report that can be large, and therefore the one
guarded by the row threshold.

**Independent Test**: Read through the audit service, scoped and paginated; an over-threshold window
returns **422 before anything is materialized**.

### Tests for User Story 5

- [ ] T064 [P] [US5] Write the 🎯 **422 guard test** in `tests/ProjectManagementApp.Api.Tests/Reports/ActivityThresholdTests.cs` — seed beyond `LargeReportRowThreshold`, request the wide window, assert **422** (not 400, not a slow 200), and verify via query logging that **no large result set was materialized**; narrowing the window then returns **200** (research R-5, DoD 7)
- [ ] T065 [P] [US5] Write the 🎯 **service-not-table test** in `tests/ProjectManagementApp.Api.Tests/Reports/ActivityReadPathTests.cs` — the handler calls **`IActivityLogService.QueryScopedAsync`**, and **no LINQ query against `db.ActivityLogs` exists under `Features/Reports/`** (FR-007)
- [ ] T066 [P] [US5] Write the scope + filter test in `tests/ProjectManagementApp.Api.Tests/Reports/ActivityScopeTests.cs` — per-role scoping with a **scoped `totalCount`**; `entityType`/`actorId`/`projectId` filters **narrow within scope**; a named out-of-scope `projectId` → **403**
- [ ] T067 [P] [US5] Write paging tests in `tests/ProjectManagementApp.Api.Tests/Reports/ActivityPagingTests.cs` — default 20; `pageSize=500` **clamped to 100**; `page=-1` → **400**; newest-first stable ordering
- [ ] T068 [P] [US5] Write handler tests in `tests/ProjectManagementApp.Application.Tests/Features/Reports/GetActivityReportQueryHandlerTests.cs` — **threshold guard runs before materialization**, then the scoped read, then the audit

### Implementation for User Story 5

- [ ] T069 [US5] Create `GetActivityReportQuery` + validator in `src/ProjectManagementApp.Application/Features/Reports/GetActivityReport/` — window, optional `projectId`/`entityType`/`actorId`, paging bounds
- [ ] T070 [US5] Implement `GetActivityReportQueryHandler` in `src/ProjectManagementApp.Application/Features/Reports/GetActivityReport/GetActivityReportQueryHandler.cs` — resolve scope, **run `RowThresholdGuard` first (→ 422)**, then call `IActivityLogService.QueryScopedAsync` with filters and paging, project to `ActivityReportRow`, then audit. **Never query `activity_logs` directly**
- [ ] T071 [US5] Wire `GET /api/reports/activity` in `src/ProjectManagementApp.Api/Controllers/ReportsController.cs` including the **422** response path
- [ ] T072 [P] [US5] Build the Activity Report view in `src/ProjectManagementApp.Web/src/app/features/reports/activity/` — filter form, paginated table over `PagedResult<T>`, and a **"narrow your range" prompt on 422** shown *before* any export is attempted
- [ ] T073 [US5] Implement `ReportsService.getActivity()` in `src/ProjectManagementApp.Web/src/app/core/services/reports.service.ts`

**Checkpoint**: All four reports work. Verify against quickstart V9, V10.

---

## Phase 8: User Story 6 — Export + audit verification (Priority: P0) 🎯 capstone

**Goal**: Download any report as PDF or CSV, rendered entirely in the browser, with generation auditable.

**Independent Test**: Both exports produce the **same data as the preview** with **no additional network
request**, and re-exporting does **not** double-audit.

> Depends on US2–US5. The audit half was built in Foundational (T017); this story wires the export service
> and **proves** the audit contract.

### Tests for User Story 6

- [ ] T074 [P] [US6] Write export tests in `src/ProjectManagementApp.Web/src/app/core/services/report-export.service.spec.ts` — `toPdf()` produces a PDF via **jsPDF**; `toCsv()` produces CSV via **papaparse**; both from the **same JSON object** the preview holds; an **empty report** yields a valid empty artifact (headers only)
- [ ] T075 [P] [US6] Write the 🎯 **no-round-trip test** in `src/ProjectManagementApp.Web/src/app/features/reports/export-network.spec.ts` — triggering either export issues **zero HTTP requests** (spy on `HttpClient`). Export is a client-side representation, not a second query
- [ ] T076 [P] [US6] Write the 🎯 **no-double-audit test** in `tests/ProjectManagementApp.Api.Tests/Reports/ExportAuditTests.cs` — fetch a report once, then export the **same previewed data** to **both** PDF and CSV: the `ReportGenerated` count increases by **exactly one** (from the fetch), not three (FR-011, DoD 9)
- [ ] T077 [P] [US6] Write the 422-blocks-export test in `src/ProjectManagementApp.Web/src/app/features/reports/activity/activity-report.component.spec.ts` — an over-threshold Activity window surfaces the narrow-range prompt and **never attempts a client render**

### Implementation for User Story 6

- [ ] T078 [US6] Implement `ReportExportService` in `src/ProjectManagementApp.Web/src/app/core/services/report-export.service.ts` — `toPdf(report)` via jsPDF and `toCsv(report)` via papaparse, plus a meaningful download filename from `Reports:DownloadFilenamePattern` (e.g. `project-progress_2026-07.pdf`). **One service consumed by all four report views — never duplicated per component** (Constitution VII.8)
- [ ] T079 [US6] Add Export to PDF / Export to CSV controls to all four report views under `src/ProjectManagementApp.Web/src/app/features/reports/`, wired to the shared service, with a **busy state that prevents double-clicks** during a large render
- [ ] T080 [US6] Verify **no `?format` parameter and no export endpoint** exist in `src/ProjectManagementApp.Api/Controllers/ReportsController.cs` — format is a client concern only (research R-3)

**Checkpoint**: All six stories complete. Verify against quickstart V11, V12.

---

## Phase 9: Polish & Cross-Cutting Concerns

- [ ] T081 🎯 **Write the exactly-one-audit suite** in `tests/ProjectManagementApp.Api.Tests/Reports/AuditContractTests.cs` — one data request → **+1**; `/catalog` → **0**; a 400 or 403 request → **0**; re-export of previewed data → **0**. **And assert no domain entity was written** and no `xmin` bumped anywhere (DoD 9, FR-011/FR-012)
- [ ] T082 🎯 **Write the export-architecture assertion** in `tests/ProjectManagementApp.Api.Tests/Reports/ExportArchitectureTests.cs` — the API declares **no** `?format` parameter, **no** export route, and **no** non-`GET` verb under `/api/reports`; no server-side PDF/CSV package is referenced by the backend projects (research R-3)
- [ ] T083 🎯 **Write the cross-feature parity test** in `tests/ProjectManagementApp.Api.Tests/CrossFeature/DashboardReportParityTests.cs` — for the same caller and window, 005's `overdueTaskCount` **equals** the sum of 006's `rows[].overdueTasks`. **Run it with the process clock in a non-UTC zone**, where a configurable timezone would have broken it (006 NFR-002; closes 005 plan Follow-up 4)
- [ ] T084 **Prove the contract gate fails**: temporarily add a `format` query parameter to a report endpoint, run `dotnet build -p:CheckApiContract=true`, confirm the build **fails**, then revert. That drift would silently move export back onto the server, contradicting Constitution III's jsPDF lock and VII.8 (quickstart V14)
- [ ] T085 Execute the full quickstart validation **V1–V15** in `specs/006-reports/quickstart.md` and record results
- [ ] T086 [P] Verify **no migration and no `ReportArtifact`/`ReportSchedule` table** were added — `ls src/ProjectManagementApp.Infrastructure/Persistence/Migrations/` shows nothing new (DoD 9, quickstart V13)
- [ ] T087 [P] Profile each report against a seeded dataset and record timings in the PR — confirm scoped grouped aggregates with **no N+1**, and that the threshold guard adds negligible cost (NFR-003)
- [ ] T088 [P] Add XML doc comments to `ReportsController` and the five handlers, noting the **one deliberate audit write** and that export is client-side (Constitution VIII.3)
- [ ] T089 [P] Update the root `README.md` with the reports module — the five endpoints, the `Reports:*` keys, the **fixed-UTC** note, and that **`Reports:AuditOnGeneration` should stay `true`** (006 plan Follow-up 3)
- [ ] T090 [P] Confirm the brief's Reports deliverable is met end to end — charts (Chart.js), export (jsPDF + papaparse), and four report types — and record it in `docs/` alongside the ERD
- [ ] T091 Remove commented-out code and any `console.log` across `src/ProjectManagementApp.Web/src/app/features/reports/` and the 006 backend slices (Constitution VIII.4)
- [ ] T092 Run a security review against spec 006 §Security Rules — scope in the query source, named out-of-scope 403, **TeamMember self-only enforced server-side**, activity read only through the service, every generation audited, no domain write

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (Phase 1)** — needs **001–005 complete**; T007 verifies, especially `closed_at` and 005's metric definitions
- **Foundational (Phase 2)** — depends on Setup; **blocks all stories**
- **US1 (Phase 3)** — depends on Foundational only; **fully independent** of the report stories
- **US2, US3, US4, US5 (Phases 4–7)** — each depends only on Foundational; **independent of one another**
- **US6 (Phase 8)** — **depends on US2–US5** (needs report data to export and audit)
- **Polish (Phase 9)** — depends on all; **T083 additionally requires 005 to be running**

### Story independence

**Five of six stories are independent.** Foundational front-loads everything shared — scope, counting
rules, threshold guard, audit helper — so US1–US5 touch no common logic. US6 is the capstone.

That gives up to **five parallel tracks** after Foundational, then US6.

### Shared-file contention

- `ReportsController.cs` — T029/T040/T051/T061/T071, five small edits
- `reports.service.ts` — T031/T042/T053/T063/T073
- The four report views each gain Export controls in **T079** (US6) after they exist

### Parallel opportunities

- Setup: T002–T006, T008 all **[P]**
- **Foundational is highly parallel** — T009–T023 are mostly **[P]** and independently testable before any endpoint exists
- Every story's test tasks are **[P]**
- Polish is largely **[P]** except T084, T085, T091, T092

---

## Parallel Example: after Foundational

```bash
# Five stories, five developers, no shared logic:
Task: "Catalog descriptors in src/…/Features/Reports/GetCatalog/"
Task: "Project Progress handler in src/…/Features/Reports/GetProjectProgress/"
Task: "Task Completion handler in src/…/Features/Reports/GetTaskCompletion/"
Task: "Team Performance handler in src/…/Features/Reports/GetTeamPerformance/"
Task: "Activity Report handler in src/…/Features/Reports/GetActivityReport/"

# Sequence only the controller and service edits (T029/T040/T051/T061/T071).
```

---

## Implementation Strategy

### MVP scope

**Phases 1 → 2 → 3 (US1) → 4 (US2).** The catalog plus the flagship Project Progress report is the
smallest genuinely demonstrable increment: a picker that builds its own form, and a real report behind it.

US2 alone is technically shippable, but without the catalog the form is hard-coded — which is precisely
what US1 exists to avoid.

### Incremental delivery

1. Setup + Foundational → scope, counting rules, threshold, audit helper tested standalone
2. **US1** → the picker drives itself from the catalog (V1)
3. **US2** → the flagship report, and the scope/403 pattern all others follow (V2–V5, V8)
4. **US3** → the historical trend, and the re-open rule proven across two reports (V5, V7)
5. **US4** → the self-only least-privilege boundary (V4)
6. **US5** → the audit-facing report and the 422 guard (V9, V10)
7. **US6** → export, and the audit contract proven (V11, V12)
8. Polish → **T081**, **T082**, **T083** are the three that matter

### Critical warnings

- **T014 must import the shared-kernel `MetricDefinitions` (`Common/Metrics/`, §8) — not re-implement them,
  and not import from `Features/Dashboard/`.** Re-implementing is how the two surfaces drift, and T083's
  parity test is the only thing that would catch it, after the fact. Importing from 005's feature folder
  compiles but makes 006 depend on another feature's Application layer (ADR-0006 addendum, ADR-0007 §5).
- **T007 must not accept `updated_at` as a substitute for `closed_at`.** `updated_at` moves on any later
  edit, so every completion metric would silently drift as finished tasks are touched.
- **T054's assertion is the absence of a 403.** A TeamMember naming a colleague must get **their own row**;
  returning 403 confirms the colleague exists and is out of scope.
- **T064 must verify the guard runs _before_ materialization.** A 422 returned after loading 10,000 rows
  protects the browser but not the server.
- **T076 must show re-export does NOT re-audit.** The server audits *generation*; format is a client
  choice, and auditing it would triple-count every report.

---

## Notes

- **[P]** = different files, no dependency on an incomplete task
- Tests are written before implementation within each story; verify they fail first
- Commit per task or logical group, Conventional Commits (Constitution VIII.5)
- Every checkpoint is a safe stopping point for validation or demo
