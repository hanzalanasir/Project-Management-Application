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

- [X] T001 Created the slice folder structure `src/ProjectManagementApp.Application/Features/Reports/{GetCatalog,GetProjectProgress,GetTaskCompletion,GetTeamPerformance,GetActivityReport,Common}/` — query slices only, no commands.
- [X] T002 [P] Generated TypeScript DTO types from `docs/contracts/reports.v1.yaml` — added `generate:api:reports` npm script matching 001-005's convention; ran once, output at `reports.v1.d.ts`. **Found and fixed a real contract bug first**: six unquoted flow-mapping `description:` values contained unescaped commas (`{ ..., description: Tasks whose \`closed_at\` falls in this bucket, in scope. ... }`), which is invalid YAML inside a flow mapping (comma is the entry separator) — `openapi-typescript` failed to parse the file at all. Fixed by single-quoting each affected description (lines for `pageSize`, `TaskCompletionBucket.periodStart`/`completedCount`, `TeamPerformanceRow.isActive`/`throughput`/`overdueCount`). No semantic change — pure YAML-authoring fix, contract meaning unchanged.
- [X] T003 [P] Added `reports.v1.yaml` to the `CheckApiContract` MSBuild target (sixth and final `oasdiff breaking` call against the same `generated.json`, same pattern as 002-005). Not exercised yet — the drift-gate proof is Polish stage's job; build succeeds with the new contract wired in.
- [X] T004 [P] Added the `Reports:*` section to `appsettings.json`: `DefaultWindowDays=30`, `MaxWindowDays=366`, `Activity:{DefaultPageSize=20,MaxPageSize=100}`, `LargeReportRowThreshold=10000`, `LargeReportFallback="ForceNarrow"`, `EnabledTypes=[ProjectProgress,TaskCompletion,TeamPerformance,Activity]`, `AuditOnGeneration=true`, `MaskOutOfScopeAs404=false`, `DownloadFilenamePattern="{reportType}_{from:yyyy-MM-dd}_{to:yyyy-MM-dd}"`. No `TimeZone` key — deliberately absent (research R-4). `MaxWindowDays` value (366) is my own reasonable default; the spec did not pin an exact number.
- [X] T005 [P] Created `ReportsOptions` — **placed in `src/ProjectManagementApp.Application/Common/Options/ReportsOptions.cs`, not the task's literal `Api/Configuration` path**, following the exact relocation precedent 002-005 already established (`ProjectsOptions`/`TasksOptions`/`TeamOptions`/`DashboardOptions`): every value is consumed by a slice handler and Application must not reference Api (Constitution II.2). Registered in `Program.cs`.
- [X] T006 [P] Added **jsPDF 4.2.1** and **papaparse 5.5.4** (+ `@types/papaparse` dev dependency) via `npm install` — no backend export packages. `npm audit` reported 5 pre-existing transitive vulnerabilities (Angular CLI's own MCP SDK dependency, openapi-typescript's js-yaml) — unrelated to jsPDF/papaparse and not introduced by this change; left as-is.
- [X] T007 **Verified all five prerequisites — no gap found**: `IActivityLogService.QueryScopedAsync` implemented; `AuditAction.ReportGenerated` present in the 18-value enum; `UpdateTaskStatusCommandHandler` sets `task.ClosedAt` precisely on entry to `Done` and clears it precisely on exit from `Done` (verified the exact condition in source — never conflated with `UpdatedAt`); 005's `MetricDefinitions` (`IsOverdue`, `IsClosed`, `CompletionRate`, `ClosedInWindow`) exist and are importable from `Common/Metrics/`; `IProjectAccessPolicy`/`ITaskAccessPolicy` both registered in `Application/DependencyInjection.cs`. Nothing to stop and fix.
- [X] T008 [P] Scaffolded the lazy `reports` route group in `reports.routes.ts` (five child routes: picker at `''`, plus `project-progress`/`task-completion`/`team-performance`/`activity`), registered via `loadChildren` in `app.routes.ts`. **No role is fully excluded from Reports** (spec Security Rules — every authenticated caller may reach the picker; TeamMember's least-privilege is server-side per-report, not a route gate), so the guard is the same `authGuard` 005's Dashboard registers at the same level — there is no dedicated Reports role guard to write.

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T009 [P] Implemented `ReportEnvelope` (+ `ReportWindowDto`) in `Features/Reports/Common/ReportEnvelope.cs` — `reportType`, `generatedAt`, `scope`, `window{from,to}`, `timeZone` defaulted to the literal `"UTC"`. `Scope`/`ReportType` are plain `string`, not C# enums — matches every other role/status field in this codebase (the app registers no `JsonStringEnumConverter`; validated server-side via whitelist instead).
- [X] T010 [P] Implemented `ReportWindow` in `Features/Reports/Common/ReportWindow.cs` — two pure predicates, `IsOrdered(from, to)` and `WithinMaxSpan(from, to, maxWindowDays)`, composed into each report query's own FluentValidation validator in later stages (matches this codebase's existing validation pipeline — `ValidationBehavior` converts FluentValidation failures into the same `Result<T>`/RFC7807 path as everything else, so `from`/`to` are bound as nullable in the controller and "required" is owned by the validator, not ASP.NET's default model-binding 400).
- [X] T011 [P] Wrote `ReportWindowTests.cs` — `from > to` rejected, `from == to` accepted (both boundaries inclusive), max-span exactly-at-limit accepted / one-day-over rejected. **6 tests, all passing.** `DateOnly` carries no offset of its own, so these tests need no clock at all — that absence is itself the proof the window can't drift with the process's local timezone.
- [X] T012 Implemented `ReportScope` in `Features/Reports/Common/ReportScope.cs` — reuses `IProjectAccessPolicy.ApplyScope` as an un-materialized `IQueryable<Guid>`; three branches (`null`/`"all"` silently narrows; a comma-separated all-in-scope list succeeds; any one out-of-scope id 403s the whole request) exactly as specified. **Real bug found and fixed during T013**: the initial named-ids success path returned `namedIds.AsQueryable()` — a LINQ-to-Objects queryable, not `IAsyncEnumerable`-backed — which threw `InvalidOperationException` the moment a caller (or test) called EF's `ToListAsync()` on it. Fixed by returning `visibleProjectIds.Where(id => namedIds.Contains(id))` instead: still a single EF-translatable subquery, and the prior `CountAsync()` check already proves it covers exactly the named ids.
- [X] T013 [P] Wrote `ReportScopeTests.cs` against real Testcontainers Postgres (ADR-0007 §2) — null/`"all"` narrows with no Forbidden; a named single out-of-scope id 403s (`ErrorKind.Forbidden`); a comma-separated all-in-scope list succeeds covering exactly those ids; a list with one out-of-scope id among in-scope ones 403s the whole request (not partial); a malformed id and a trailing-empty-entry list both return `ErrorKind.Validation`; the `"all"` branch composes as a genuine subquery (`ToQueryString()` shows two `SELECT`s). **8 tests, all passing** (after the T012 fix above).
- [X] T014 [P] Implemented `ReportCountingRules` in `Features/Reports/Common/ReportCountingRules.cs` — thin re-exports of `MetricDefinitions.IsOverdue`/`IsClosed`/`ClosedInWindow`/`CompletionRate` (imported from `Common/Metrics/`, **not** `Features/Dashboard/**`), plus one genuinely-new predicate `IsOpenAssignment` for Team Performance's `workload` (no Dashboard counterpart to reuse). `IsClosed` alone **is** the re-open exclusion rule for both Project Progress's `closedTasks` and Task Completion's buckets — nothing extra was written for it, by construction.
- [X] T015 [P] Wrote `ReportCountingRulesTests.cs` — `IsClosed` true for `Done`+`closed_at` set, false for a re-opened task (`InProgress`+`closed_at` cleared) with an explicit comment that this single evaluation is the proof both consuming surfaces share the rule; `IsOverdue` boundary case; `IsOpenAssignment` true/false; `CompletionRate` zero-tasks-is-zero and exact-quarter. **7 tests, all passing.**
- [X] T016 [P] Implemented `RowThresholdGuard` in `Features/Reports/Common/RowThresholdGuard.cs` — generic `CheckAsync<T>(IQueryable<T> scopedAndFilteredQuery, threshold, ct)`, one indexed `CountAsync()`, `ErrorKind.UnprocessableContent` when the count exceeds the threshold. No standalone unit test task exists for this in tasks.md (T064 in US5/stage 4 is where it's proven against a real over-threshold dataset) — deliberately left untested in isolation here since a meaningful test needs a real scoped-and-filtered query to run the guard against.
  - **⚠️ Flagging a real design tension for stage 4, not fixed here**: research.md R-5's own illustrative code counts directly against a raw `activity_logs`-shaped `IQueryable`, but FR-007/T065 requires the Activity Report to read **only** through `IActivityLogService.QueryScopedAsync`, which has no filter parameters (`entityType`/`actorId`/`projectId`) and no plain-count method — only `(ActivityScope scope, int page, int pageSize, ct) -> PagedResult<ActivityEntry>`. T070 in stage 4 will need to resolve this (e.g. a page-1-pageSize-1 probe read of `TotalCount`, or extending `IActivityLogService` under the same kind of explicit authorization 005 obtained for its own `ActivityLogService` bug) — flagging now so it's not a surprise later, per this stage's own "stop and report if you find a real bug" instruction.
- [X] T017 Implemented `ReportGenerationAudit` in `Features/Reports/Common/ReportGenerationAudit.cs` — one `IActivityLogService.LogAsync` call, `entityType="Report"`, a fresh `Guid` run id as `entityId`, `changeSummary` = report type + JSON-serialized parameters, gated by `ReportsOptions.AuditOnGeneration`. `projectId` deliberately left `null` (a report can span several or zero projects, so it has no single owning project — matches 001's convention for project-less events, Admin-only visible). Shared by all four report handlers, built once here so no later story edits this file.
- [X] T018 [P] Wrote `ReportGenerationAuditTests.cs` (pure NSubstitute unit tests, no database) — exactly one `LogAsync` call with `action="ReportGenerated"`/`entityType="Report"`; `changeSummary` contains both the report type and the parameters; two invocations use distinct run ids; `AuditOnGeneration=false` suppresses the call entirely. **4 tests, all passing.** "No domain entity touched" is proven structurally by the helper's own signature (it takes no `IApplicationDbContext`, so it has no way to touch one) — the functional half against a real database is T081/T036 in later stages.
- [X] T019 **Verified** `ErrorKind.UnprocessableContent` — present in the enum, mapped to 422 in `ResultExtensions.ToProblemResult`. Already fully wired from 001; nothing to patch.
- [X] T020 Created the thin `ReportsController` shell — five `[HttpGet]` actions (`catalog`, `project-progress`, `task-completion`, `team-performance`, `activity`) under `[Route("api/reports")]`, plain `[Authorize]` on each (no role attribute — matches spec's "role gate via attributes only... scope enforced in the query source"), full parameter signatures matching the contract, bodies `throw new NotImplementedException(...)` pointing at the exact task that wires each one (T029/T040/T051/T061/T071). No write verbs, no `?format` parameter, no export route.
- [X] T021 [P] Implemented `ReportsService` in `reports.service.ts` — five methods (`getCatalog`, `getProjectProgress`, `getTaskCompletion`, `getTeamPerformance`, `getActivity`) using the generated DTO types and the existing `toQueryParams` helper, typed query-parameter interfaces per report.
- [X] T022 [P] Created the five report-view component shells (`picker/`, `project-progress/`, `task-completion/`, `team-performance/`, `activity/`) — placeholder templates only, each pointing at the exact task (T030/T041/T052/T062/T072) that builds its real content, matching 005 T018's identical precedent.
- [X] T023 [P] Extended the fixture set with `tests/ProjectManagementApp.Application.Tests/Builders/ReportsScenario.cs` (extends `TeamScenario`) — a fixed window (2026-07-01 to 2026-07-31) with: two tasks closed in-window assigned to `Tm` and one to `Tm2` (deliberate throughput asymmetry, 2 vs 1, for Team Performance); one re-opened task (`InProgress` + `closed_at` cleared) for the FR-017 exclusion rule T015/T041/T045 depend on; one overdue-and-open task; one open no-due-date task assigned to `Tm2` (workload only, no throughput/overdue effect). Verified it seeds cleanly against real Postgres via a throwaway probe test (written, run, deleted — same pattern used elsewhere in this codebase).

**Checkpoint**: Envelope, scope, counting rules, threshold guard, and the audit helper are tested in
isolation — **25 new backend tests passing** (6+8+7+4, all Application.Tests), full `Application.Tests`
suite at **220/220 passing** (195 inherited from 001-005 + 25 new), `Infrastructure.Tests` unaffected
(**34/34 passing**), `Api.Tests` unaffected (**217/217 passing** — no HTTP-level 006 tests exist yet;
`ReportsController`'s five actions all throw `NotImplementedException` until their user stories land), and
both `dotnet build` and `ng build`/`ng test` (**24/24 passing**, 5 new lazy chunks for the report views)
succeed cleanly.

**Checkpoint**: Envelope, scope, counting rules, threshold guard, and the audit helper are tested in
isolation, before any endpoint exists.

---

## Phase 3: User Story 1 — Report catalog (Priority: P1)

**Goal**: A self-describing catalog so the frontend builds parameter forms dynamically.

**Independent Test**: Returns exactly four descriptors with ordered parameters and formats; role-annotated;
**writes no audit row**.

### Tests for User Story 1

- [X] T024 [P] [US1] Wrote `CatalogTests.cs` — exactly four descriptors returned as a plain array (not paged); every descriptor's `parameters` are ordered with `from`/`to` first and always `required: true`; `formats` is `["json","pdf","csv"]` on all four; 401 without a token. **3 tests, all passing.**
- [X] T025 [P] [US1] Wrote `CatalogAnnotationTests.cs` — a TeamMember sees Team Performance's `note` as `"self only"`; Admin/ProjectManager see `null`; every descriptor's property set is exactly `{type,title,note,parameters,formats}` — no project or task data leaks in. **3 tests, all passing.**
- [X] T026 [P] [US1] Wrote the 🎯 **catalog-not-audited test** in `CatalogNotAuditedTests.cs` — calls `/catalog` five times across Admin/PM/TM and asserts the `activity_logs` row count is unchanged (raw `SqlQuery<int>` before/after, same pattern as 005's `NoWriteGuaranteeTests`). **1 test, passing.**

### Implementation for User Story 1

- [X] T027 [US1] Created `ReportDescriptor`/`ReportParameterDescriptor` + the static `ReportCatalog.Describe(callerRole)` descriptor set in `Features/Reports/GetCatalog/ReportDescriptor.cs`, matching the contract schema exactly (including the `all|projectIds` and pipe-delimited enum `type` strings used verbatim by the frontend's dynamic form).
- [X] T028 [US1] Created `GetReportCatalogQuery` + `GetReportCatalogQueryHandler` — pure function of `ICurrentUserService.Current.Role`, no database access, **does not call `ReportGenerationAudit`** (verified by T026).
- [X] T029 [US1] Wired `GET /api/reports/catalog` in `ReportsController` — added `IMediator` constructor injection (the controller had none before this stage), one `Send` + `.ToActionResult()`.
- [X] T030 [P] [US1] Built the 🎯 **catalog-driven report picker** (`picker/report-picker.component.ts/html/scss`) — cards list every descriptor; selecting one calls `fb.group(...)` over that descriptor's `parameters` array at runtime (a loop, not a switch on report type), so a fifth report needs no new form code (proven by `report-picker.component.spec.ts`'s test that two different descriptors produce two different control sets from the same code path). Pipe-delimited parameter types (`day|week|month`, entity-type enum) render as a `mat-select`; `all|projectIds` and `uuid`/`date`/`integer` render as typed inputs. Submitting navigates to `/reports/<type-route>` with the form values as query params.
- [X] T031 [US1] `ReportsService.getCatalog()` — **already implemented in stage 1 (T021)**; verified it matches the wired `/api/reports/catalog` route and response shape, no change needed.

**Checkpoint**: The picker renders forms it did not hard-code — proven by a unit test that swaps the
selected descriptor and asserts the control set changes. Verified live against quickstart V1 (running
API, real JWTs): exactly four types returned; TeamMember's Team Performance carries `"note":"self only"`,
Admin/PM see `"note":null`.

---

## Phase 4: User Story 2 — Project Progress (Priority: P0) 🎯 flagship

**Goal**: Per-project completion %, open/closed, overdue, and projected completion over a window — with
overdue **equal to the Dashboard's** value.

**Independent Test**: The three-role scope matrix holds; a named out-of-scope project 403s; `all` narrows
silently; one audit row is written.

### Tests for User Story 2

- [X] T032 [P] [US2] Wrote `ProjectProgressScopeTests.cs` — Admin sees every project; PM sees only their owned project; a second PM sees only theirs (nothing from the first); TeamMember sees only their member-of project; a brand-new PM with zero projects gets **200 with an empty `rows` array**, never 403; 401 without a token. **6 tests, all passing.**
- [X] T033 [P] [US2] Wrote `ProjectProgressForbiddenTests.cs` — a PM naming the other PM's project → **403**; `projectScope=all` → **200**, silently narrowed to just the caller's own project; a named list where every id is in scope (Admin naming both A and B) → **200** covering exactly those; a list mixing an owned id with an out-of-scope id → **403 for the whole request**; a malformed id → **400**, never 403. **5 tests, all passing.**
- [X] T034 [P] [US2] Wrote `ProjectProgressMetricTests.cs` — a fresh 12-task project with exactly 3 `Done` yields `completionPercent: 25`, `closedTasks: 3`, `openTasks: 9`, and `open + closed == total`; a brand-new zero-task project yields `completionPercent: 0` (never NaN/divide-by-zero). **2 tests, all passing.**
- [X] T035 [P] [US2] Wrote the 🎯 **projected-completion test** in `ProjectedCompletionTests.cs` — steady throughput (2 closed "now", 3 open) yields a non-null future date; zero closures in the window (open tasks remain) yields `null`; zero open tasks (fully closed) yields `null`. **3 tests, all passing.**
- [X] T036 [P] [US2] Wrote `ProjectProgressAuditTests.cs` — a successful request writes **exactly +1** `ReportGenerated` row (raw SQL count, filtered on `action = 'ReportGenerated'`); a 403 (named out-of-scope) writes **zero**; a 400 (`from > to`) writes **zero**. **3 tests, all passing** — but only after a real bug fix, see T039's note below.
- [X] T037 [P] [US2] Wrote `GetProjectProgressQueryHandlerTests.cs` in `Application.Tests` (real Postgres) — a PM's rows never include a project they don't own (scope composed before any task is aggregated); a re-opened task drops out of `closedTasks` via the same `ReportCountingRules.IsClosed` T014 rule, proven with the shared `ReportsScenario` fixture; a scope failure (named out-of-scope) returns a `Forbidden` `Result` and never calls `IActivityLogService.LogAsync` at all (`DidNotReceiveWithAnyArgs`). **3 tests, all passing.**

### Implementation for User Story 2

- [X] T038 [US2] Created `GetProjectProgressQuery` (`From`/`To` as nullable `DateOnly?`, `ProjectScope` as `string?`) + `GetProjectProgressQueryValidator` — `from`/`to` required (FluentValidation owns "required", not ASP.NET's default binding 400, per stage-1's established convention), `from ≤ to`, and window span ≤ `ReportsOptions.MaxWindowDays`.
- [X] T039 [US2] Implemented `GetProjectProgressQueryHandler` — resolves scope via `ReportScope` (T012, project-membership based — **deliberately not `ITaskAccessPolicy.ApplyScope`**, which scopes a TeamMember to *assigned* tasks only; spec's Role & Permission table requires a TeamMember to see the **full** stats of every project they're a *member* of, so per-project task aggregation queries `db.Tasks.Where(t => t.ProjectId == project.Id)` directly), aggregates per project with `ReportCountingRules` (T014), computes `projectedCompletion` with both null guards, then calls `ReportGenerationAudit` (T017).
  **⚠️ Real bug found and fixed (caught by T036's audit test, not by inspection)**: `IActivityLogService.LogAsync`'s own doc comment states it only *stages* the row on the caller's unit of work — "the caller is responsible for calling SaveChangesAsync." Every command handler in this codebase calls `SaveChangesAsync` explicitly; this is the **first query handler that writes anything**, and it had no `SaveChangesAsync` call anywhere in its path, so the audit row was silently never persisted (T036 initially failed: `Expected after to be 1, but found 0`). Fixed by adding `await _db.SaveChangesAsync(ct);` immediately after the `ReportGenerationAudit.RecordAsync` call. **This is a real gap every other US2-US5 handler (stage 2-4) must also account for** — flagging explicitly since T017's own doc comment didn't call it out, and it's easy to copy the same handler shape into US3-US5 and reproduce the same silent no-op.
- [X] T040 [US2] Wired `GET /api/reports/project-progress` — added `IMediator.Send(new GetProjectProgressQuery(from, to, projectScope), ct)` + `.ToActionResult()`.
- [X] T041 [P] [US2] Built the Project Progress view (`project-progress/project-progress.component.ts/html/scss`) — reads `from`/`to`/`projectScope` from the route's query params (set by the picker), renders a Chart.js bar chart of `completionPercent` per project (same jsdom-canvas-guard pattern as 005's `SummaryComponent`), and a table with every `ProjectProgressRow` column plus a totals footer. Renders every value exactly as the API returns it — no client-side recomputation of completion %, overdue, or projected completion. **Export controls are explicitly out of scope here** — they land in US6 (T079) against all four views at once.
- [X] T042 [US2] `ReportsService.getProjectProgress()` — **already implemented in stage 1 (T021)**; verified the query-parameter shape (`from`/`to`/`projectScope`) matches the now-wired controller exactly, no change needed.

**Checkpoint**: 🎯 The flagship report works end to end — 26 new backend tests (6+5+2+3+3+3+... see above,
26 total across Api.Tests + Application.Tests) plus 5 new frontend tests, all passing. Verified live
against a running API with real JWTs and real Postgres:
- **V2** — Admin/PM/second-PM/TM each see a strictly different row set sized to their scope; a
  brand-new PM's empty scope returns `200` with zero rows.
- **V3** — a PM naming the other PM's project → `403`; `projectScope=all` → `200` silently narrowed
  (the other PM's project confirmed absent from the response body).
- **V5** — a 12-task/3-closed project returns exactly `completionPercent: 25`; re-opening one closed
  task drops `closedTasks` from 3 to 2 live, in the same request shape Task Completion will reuse in
  stage 3.
- **V8** — a project with open tasks but zero in-window closures returns `projectedCompletion: null`;
  a fully-closed project (`openTasks: 0`) also returns `null`; a project with real throughput returns
  a real future date.

---

## Phase 5: User Story 3 — Task Completion (Priority: P1)

**Goal**: A completion trend bucketed by day/week/month, zero-filled and continuous.

**Independent Test**: Buckets change with `groupBy`; empty periods appear as `0`; a re-opened task drops out.

### Tests for User Story 3

- [X] T043 [P] [US3] Wrote `TaskCompletionBucketTests.cs` — `groupBy=week` over a fixed 4-week window (2026-06-01..06-28, chosen because it's an exact 4-ISO-week span) with closures backdated into weeks 1 and 3 yields **exactly four buckets, `[1,0,1,0]`**; missing/invalid `groupBy` → **400**. **3 tests, all passing.**
- [X] T044 [P] [US3] Wrote the 🎯 **UTC boundary test** in `TaskCompletionTimezoneTests.cs` — same two-part structure as 005's `OverdueTimezoneTests` (T050): (1) a source scan proving no file under `Features/Reports/` contains `DateTime.Now`/`DateTime.Today`/`TimeZoneInfo.Local`/`TimeZoneInfo.ConvertTime`; (2) `DateTimeOffset.UtcNow.Offset == TimeSpan.Zero` as the runtime guarantee that makes that scan sufficient; (3) a task closed at `23:30 UTC` on 2026-06-15 (backdated via the new `SetTaskClosedAtAsync` DB escape hatch — `closed_at` can't be backdated through the real status-transition endpoint, which always stamps "now") lands in the `2026-06-15` bucket. **Honestly could not literally flip the OS timezone** (same empirical finding 005's T050 already documented for this Windows sandbox — `TimeZoneInfo.Local` reads the OS registry, not `TZ`); proven structurally instead, per this stage's own instruction. **3 tests, all passing.**
- [X] T045 [P] [US3] Wrote the 🎯 **re-open exclusion test** in `TaskCompletionReopenTests.cs` — closes a task (backdated `closed_at`), confirms it counts in both Task Completion's bucket AND Project Progress's `closedTasks`, re-opens it, confirms it drops from BOTH — the same `ReportCountingRules.ClosedInWindow`/`IsClosed` call underlies both handlers, so this is one shared rule, not two independently-correct ones. **1 test, passing** (after fixing a stale-ETag bug in the test itself — see note below).
- [X] T046 [P] [US3] Wrote `TaskCompletionAssigneeFilterTests.cs` — a PM's `assigneeId` filter narrows to just that member; a TeamMember naming a colleague's id gets back their OWN completion count instead (never the colleague's, never zero-by-rejection). **2 tests, all passing.**
- [X] T047 [P] [US3] Wrote `GetTaskCompletionQueryHandlerTests.cs` (real Postgres, `ReportsScenario`) — month-grouping sums all 3 in-window closures into one bucket with the re-opened task excluded (proving T037's Project Progress result and this bucket agree, per T045's note); day-grouping zero-fills 6 of 7 days. **2 tests, all passing** — but only after fixing a real EF/Npgsql translation bug, see T050's note.

### Implementation for User Story 3

- [X] T048 [US3] Created `GetTaskCompletionQuery` + `GetTaskCompletionQueryValidator` — window required (`from`/`to` nullable `DateOnly?`, FluentValidation owns "required"), `groupBy` required and restricted to `day|week|month`.
- [X] T049 [US3] Implemented `BucketGenerator` — three pure static methods (day/week/month), each enumerates every period in `[from,to]` FIRST (independent of any data) then sums matching day-counts in — zero-fill is structural, not a post-hoc patch. Week buckets are ISO weeks (Monday-start, `System.Globalization.ISOWeek` for the `YYYY-Www` label); month buckets are calendar months.
- [X] T050 [US3] Implemented `GetTaskCompletionQueryHandler` — resolves scope (T012), clamps a TeamMember's `assigneeId` to their own id (never a 403 — same "silent substitution" shape as T060's Team Performance clamp, applied to a filter here), then aggregates.
  **⚠️ Real bug found and fixed (caught by T047's own handler tests, not by inspection)**: the original implementation grouped in SQL via `.GroupBy(t => t.ClosedAt!.Value.UtcDateTime.Date)` — Npgsql cannot translate reading that computed group key back, and threw `InvalidOperationException: No coercion operator is defined between types 'System.DateTimeOffset' and 'System.Nullable<DateTime>'` on every call. Fixed by projecting the raw `ClosedAt` column (`.Select(t => t.ClosedAt!.Value)`, still filtered by scope+window entirely in SQL) and grouping into day-buckets client-side over that already-bounded set — Task Completion has no threshold guard because its result size is capped by the report window itself, unlike Activity's arbitrary row count, so this is not a materialization-safety regression.
- [X] T051 [US3] Wired `GET /api/reports/task-completion`.
- [X] T052 [P] [US3] Built the Task Completion view (`task-completion/*.ts/html/scss`) — reads `from`/`to`/`groupBy`/`projectScope`/`assigneeId` from query params, a Chart.js line chart of the bucket series, a per-bucket table with a totals row. Renders the API's zero-filled series directly — no client-side re-bucketing. Export controls deferred to US6.
- [X] T053 [US3] `ReportsService.getTaskCompletion()` — **already implemented in stage 1 (T021)**; verified against the now-wired endpoint, no change needed.

**Checkpoint**: Verified live against quickstart V5, V7 (see stage-3 summary) — zero-fill, UTC boundary,
and the shared re-open rule all hold against a running API.

---

## Phase 6: User Story 4 — Team Performance (Priority: P1)

**Goal**: Per-member throughput, workload, and overdue — with the sharpest least-privilege boundary in the
product.

**Independent Test**: 🎯 A TeamMember always receives **exactly one row — their own** — regardless of any
`userId`, and **never a 403**.

### Tests for User Story 4

- [X] T054 [P] [US4] Wrote the 🎯 **self-only clamp test** in `TeamPerformanceSelfOnlyTests.cs` — a TeamMember naming a colleague's `userId` gets **status 200**, **exactly one row**, and that row is their own — never the colleague's, never 403. Also covers the no-`userId`-supplied case (still exactly one, own row). **2 tests, all passing.**
- [X] T055 [P] [US4] Wrote the contrasting test in `TeamPerformanceForbiddenTests.cs` — a PM naming a user who's a member of a DIFFERENT PM's project → **403**; Admin naming a real user with no `team_members` row anywhere → **403** (a real account, but outside every scope's member pool). **2 tests, all passing.**
- [X] T056 [P] [US4] Wrote `TeamPerformanceMetricTests.cs` — 2 tasks closed "now" → `throughput: 2`; 3 open + 1 overdue-open (all currently assigned, not Done) → `workload: 4`; the 1 overdue task → `overdueCount: 1`. **1 test, passing.**
- [X] T057 [P] [US4] Wrote `TeamPerformanceVisibilityTests.cs` — a team member with zero tasks still appears as a full row of zeros (never omitted); a member deactivated (`IsActive=false` via direct DB write — no deactivation endpoint exists yet) after closing a task still appears, `isActive: false`, throughput unchanged. **2 tests, all passing.**
- [X] T058 [P] [US4] Wrote `GetTeamPerformanceQueryHandlerTests.cs` — a TeamMember naming a colleague's `userId` (with and without an additional in-scope `projectScope` also set) always gets exactly their own row — the clamp is applied unconditionally in the handler, before any pool lookup runs at all, so no parameter combination reaches the lookup; a PM naming a real but out-of-scope user (Admin, who has no `team_members` row on the PM's project) gets `Forbidden` and the audit service is never called. **3 tests, all passing.**

### Implementation for User Story 4

- [X] T059 [US4] Created `GetTeamPerformanceQuery` + `GetTeamPerformanceQueryValidator` — same window-required/`from≤to`/max-span shape as the other three report validators.
- [X] T060 [US4] Implemented `GetTeamPerformanceQueryHandler` — resolves project scope via `ReportScope` (T012, same as Project Progress); if the caller is a TeamMember, `memberIds = [caller.UserId]` directly, with **no pool lookup at all** (not even an unchecked one) — the clamp can't leak anything because the code path that would check membership never runs. For Admin/PM, the member pool is `db.TeamMembers.Where(tm => visibleProjectIds.Contains(tm.ProjectId)).Select(tm => tm.UserId).Distinct()` (unfiltered by `IsActive`, so deactivated members stay visible per T057); a named `userId` is checked against that pool and 403s if absent. Per-member `throughput`/`workload`/`overdueCount` are computed from tasks scoped to `visibleProjectIds` AND that member's assignment — not system-wide — so a member's stats never leak activity from a project outside the caller's own scope.
- [X] T061 [US4] Wired `GET /api/reports/team-performance`.
- [X] T062 [P] [US4] Built the Team Performance view (`team-performance/*.ts/html/scss`) — reads the caller's role from the NgRx auth store (`authFeature.selectUser`, same pattern `CreateProjectComponent` already uses) to pick between a Chart.js grouped-bar comparison (Admin/PM) and a single-member stat card (TeamMember) — the branch is driven by the actual response shape (which the server already guarantees is one row for a TeamMember), not a client-side re-derivation of the least-privilege rule. Export controls deferred to US6.
- [X] T063 [US4] `ReportsService.getTeamPerformance()` — **already implemented in stage 1 (T021)**; verified against the now-wired endpoint, no change needed.

**Checkpoint**: The least-privilege boundary holds — verified live against quickstart V4 (see stage-3
summary): a TeamMember naming a colleague gets exactly their own row at 200, Admin/PM naming an
out-of-scope user get 403.

---

## Phase 7: User Story 5 — Activity Report (Priority: P1)

**Goal**: A filtered, paginated activity excerpt — the one report that can be large, and therefore the one
guarded by the row threshold.

**Independent Test**: Read through the audit service, scoped and paginated; an over-threshold window
returns **422 before anything is materialized**.

### Tests for User Story 5

- [X] T064 [P] [US5] Wrote the 🎯 **422 guard test** in `ActivityThresholdTests.cs` — a client with `Reports:LargeReportRowThreshold` overridden to 3 (via `ApiTestFixture.CreateClient(services => ...)`, so the test seeds a handful of real rows rather than 10,000), 6 real activity rows (1 `ProjectCreated` + 5 `TaskCreated`), request → **422**; `SqlCapture.CommandTexts` asserted to contain no `"LIMIT 20"` (the real page's clamped size) — only the guard's own `LIMIT 1` probe ever executed; narrowing via `entityType=Project` (1 row, under threshold) → **200**. **1 test, passing.**
- [X] T065 [P] [US5] Wrote the 🎯 **service-not-table test** in `ActivityReadPathTests.cs` — source scan (005's `OverdueTimezoneTests`/T031 technique) proving no non-comment `.ActivityLogs` literal exists under `Features/Reports/`; a second check confirms the handler file textually references `IActivityLogService`/`QueryScopedAsync`. **2 tests, all passing.**
- [X] T066 [P] [US5] Wrote `ActivityScopeTests.cs` — PM sees only their project's activity, a second PM only theirs, Admin sees both; `entityType=Project` narrows within scope; a named out-of-scope `projectId` → **403**; `totalCount` for a scoped PM is strictly less than Admin's system-wide count. **4 tests, all passing.**
- [X] T067 [P] [US5] Wrote `ActivityPagingTests.cs` — no `pageSize` → defaults to 20; `pageSize=500` → **200**, clamped to 100 (never rejected); `page=-1` → **400**; three task-creation rows come back newest-first. **4 tests, all passing.**
- [X] T068 [P] [US5] Wrote `GetActivityReportQueryHandlerTests.cs` — a pure NSubstitute unit test (an Admin caller needs no `IApplicationDbContext` scope resolution at all, so no Postgres required): over-threshold, only the `pageSize=1` probe is ever called, the real `pageSize=20` read and `LogAsync` are both never called; under-threshold, `Received.InOrder` proves probe → real read → audit, in that exact sequence. **2 tests, all passing.**

### Implementation for User Story 5

- [X] T069 [US5] Created `GetActivityReportQuery` + `GetActivityReportQueryValidator` — window required, `page ≥ 1` enforced (400, never silently clamped — `pageSize` IS clamped, matching 005's own convention), `entityType` restricted to the contract's five-value enum.
- [X] T070 [US5] Implemented `GetActivityReportQueryHandler` — scope (Admin: `Unscoped`, skips `db.Projects` entirely; PM/TM: materialized `visibleProjectIds`, named out-of-scope `projectId` → 403) → `RowThresholdGuard.CheckAsync` via a `page=1,pageSize=1` probe through `IActivityLogService.QueryScopedAsync` → the real paged read → `ReportGenerationAudit` → `SaveChangesAsync`.
  **⚠️ Resolved the design tension flagged in stage 1**: `IActivityLogService.QueryScopedAsync` had no `entityType`/`actorId`/`projectId`/window filters and no plain-count method — 006 cannot query `activity_logs` directly (FR-007), so there was no legal way to implement T066's filters at all without extending the shared interface. Extended `QueryScopedAsync` with five new **optional, backward-compatible** parameters (`from`, `to`, `projectId`, `entityType`, `actorId`) in both `IActivityLogService` (`Application/Common/Interfaces`) and its `Infrastructure` implementation — the same kind of explicit, minimal extension already made once before when this method was first added to 001 during 005's planning (per its own doc comment). Every existing call site (005's `GetDashboardActivityQueryHandler`, all of `ActivityLogServiceTests.cs`) uses the method positionally and needed no change; one test double (`RotationAtomicityTests.ThrowingActivityLogService`) had to add the new parameters to keep implementing the interface — a pure signature update, not a behavior change. Also extended `RowThresholdGuard` with a `Func<CancellationToken, Task<int>>` overload alongside the existing `IQueryable<T>` one, so Activity's service-mediated count and Project Progress's direct-queryable count share the same threshold comparison and error message.
- [X] T071 [US5] Wired `GET /api/reports/activity` including the 422 path (via `Result`/`ToActionResult`'s existing `ErrorKind.UnprocessableContent` → 422 mapping from stage 1, T019).
- [X] T072 [P] [US5] Built the Activity Report view (`activity/*.ts/html/scss`) — reads filters from query params, shows a table over the `PagedResult`-shaped response, and on a 422 response shows a "narrow your range" prompt **instead of** any table — `report()` stays `null` in that state, so no export controls (T079) ever mount, verified directly by T077.
- [X] T073 [US5] `ReportsService.getActivity()` — **already implemented in stage 1 (T021)**; verified against the now-wired endpoint, no change needed.

**Checkpoint**: All four reports work — verified live against quickstart V9, V10 (see stage-4 summary).

---

## Phase 8: User Story 6 — Export + audit verification (Priority: P0) 🎯 capstone

**Goal**: Download any report as PDF or CSV, rendered entirely in the browser, with generation auditable.

**Independent Test**: Both exports produce the **same data as the preview** with **no additional network
request**, and re-exporting does **not** double-audit.

> Depends on US2–US5. The audit half was built in Foundational (T017); this story wires the export service
> and **proves** the audit contract.

### Tests for User Story 6

- [X] T074 [P] [US6] Wrote `report-export.service.spec.ts` — `toPdf()`/`toCsv()` both render from the same `ExportableReport` object; an empty report (`rows: []`, real `columns`) produces a valid artifact for both formats without throwing (headers-only, never an error or a blank file). jsPDF's own `save()` drives its internal browser-download flow directly (no stable `save` prototype method to spy on, and jsdom has no real download surface) — the meaningful assertion from this side is that the full render (header row, every data row, jsPDF's save call) completes without error; CSV's own `downloadTextFile` helper IS directly spy-able (`document.createElement`, `URL.createObjectURL`, anchor `.click()`), so those assertions are exact: filename, MIME type, one click, one revoke. **4 tests, all passing.**
- [X] T075 [P] [US6] Wrote the 🎯 **no-round-trip test** in `export-network.spec.ts` — loads `ProjectProgressComponent` (the flagship report) through `HttpTestingController`, flushes one report, then calls `exportPdf()`/`exportCsv()` and asserts `httpMock.verify()` throws on nothing outstanding — zero additional requests either way. The other three views share the identical export-from-`report()`-signal shape (verified by code inspection: each `toExportable()` reads only the local signal, never calls `ReportsService` again). **2 tests, all passing.**
- [X] T076 [P] [US6] Wrote the 🎯 **no-double-audit test** in `ExportAuditTests.cs` — one `project-progress` fetch, `ReportGenerated` count **+1** exactly. Export itself makes no server call at all (proven directly by T075), so there is no second/third increment to even attempt — the test's docstring makes that structural argument explicit rather than simulating a fake "export request" that couldn't exist. **1 test, passing.**
- [X] T077 [P] [US6] Wrote the 422-blocks-export test in `activity-report.component.spec.ts` — a 422 response shows the narrow-range prompt, no `<table>`, and critically **no `.export-controls`** (the `report()` signal stays `null`, so the buttons — which only render inside `report(); as data` — never mount); a normal 200 response shows `.export-controls` alongside the table, confirming the negative test isn't just "buttons always absent". **2 tests, all passing.**

### Implementation for User Story 6

- [X] T078 [US6] Implemented `ReportExportService` (`toPdf`/`toCsv`) over a single `ExportableReport` shape (`reportType`, `windowFrom`/`windowTo`, `columns`, `rows: (string|number)[][]`) — deliberately report-DTO-agnostic, so the service has no knowledge of four different report schemas; each view's own `toExportable()` does that flattening. `toPdf` lays out a title + header row + every data row via jsPDF's `text()`/`addPage()` (no `jspdf-autotable` plugin dependency — not in `package.json`); `toCsv` uses `Papa.unparse({fields, data})` and a plain `Blob`+anchor download. Filename is `{kebab-report-type}_{from}_{to}.{ext}` — conceptually matching `Reports:DownloadFilenamePattern`, generated client-side (that backend option is never sent over the wire; there is no config-exposing endpoint, so the frontend derives an equivalent pattern independently rather than literally reading the server value).
- [X] T079 [US6] Added Export PDF / Export CSV buttons to all four views, each disabled via a per-component `exporting` signal that's `true` for the synchronous duration of the export call (a same-tick double-click guard, not a network debounce — both jsPDF and papaparse run synchronously). Team Performance shows the buttons for both the TeamMember single-row case and the Admin/PM comparison case (anywhere `data.rows.length > 0`). Activity's buttons are inside the `report(); as data` branch only, per T077.
- [X] T080 [US6] Verified by direct inspection of `ReportsController.cs`: exactly five `[HttpGet]` actions (`catalog`, `project-progress`, `task-completion`, `team-performance`, `activity`), no `format`/`?format` parameter on any of them, no export route, no `[HttpPost]`/`[HttpPut]`/`[HttpDelete]`/`[HttpPatch]` anywhere in the file. Stage 5's T082/T084 will turn this into a permanent automated assertion; this stage's check was the manual confirmation the task calls for.

**Checkpoint**: All six stories complete — verified live against quickstart V9, V10, V11, V12 (see
stage-4 summary).

---

## Phase 9: Polish & Cross-Cutting Concerns

- [X] T081 🎯 **Wrote the exactly-one-audit suite** — `tests/ProjectManagementApp.Api.Tests/Reports/AuditContractTests.cs`, 9 tests: `[Theory]` over all four data reports proving +1 each; catalog → 0 (3 repeated calls); a 400 → 0; a 403 (named out-of-scope `projectScope`) → 0; re-export of already-previewed data → 0 (mirrors T076's structural argument — export makes no HTTP call at all, so there is nothing left to audit); and a combined proof that generating all four reports touches **no** domain row count (`projects`/`tasks`) and bumps **no** row's `xmin` — same technique as 005's `NoWriteGuaranteeTests` (T049), adapted from "zero writes" to "exactly one write, and it's the audit row, never a domain row." All 9 pass.
- [X] T082 🎯 **Wrote the export-architecture assertion** — `tests/ProjectManagementApp.Api.Tests/Reports/ExportArchitectureTests.cs`, 8 tests: no `format` parameter on any `ReportsController` action; no `export` route segment; no `[HttpPost]`/`[HttpPut]`/`[HttpDelete]`/`[HttpPatch]` anywhere on the controller; exactly the five expected `[HttpGet]` actions; and — the one manual inspection can't reliably catch — a reflection scan of all four backend `.csproj` files (Api/Application/Infrastructure/Domain) confirming none references a server-side PDF/CSV package (iTextSharp, QuestPDF, Syncfusion, Aspose, CsvHelper, wkhtmltopdf, PuppeteerSharp). All 8 pass; confirms none are referenced.
- [X] T083 🎯 **Wrote the cross-feature parity test** — `tests/ProjectManagementApp.Api.Tests/CrossFeature/DashboardReportParityTests.cs`. Core assertion (parameterized over Admin and PM callers): 005's `GET /api/dashboard/summary` → `overdueTaskCount` equals the sum of 006's `GET /api/reports/project-progress` → `rows[].overdueTasks`, for the same caller — passes by construction because both handlers call the same `MetricDefinitions.IsOverdue(today)` predicate from the shared kernel (`GetProjectProgressQueryHandler` via `ReportCountingRules`, `GetDashboardSummaryQueryHandler` directly), never a re-implementation. **Non-UTC process clock**: not actually flipped — same honest precedent as 005's own `OverdueTimezoneTests` (T050), which already established that `TimeZoneInfo.Local` on this Windows sandbox is resolved from the OS registry, not the `TZ` env var, so flipping it is a system-wide action out of scope for an automated test. Proven structurally instead: a source scan confirms no file under `Features/Reports/` reads `DateTime.Now`/`DateTime.Today`/`TimeZoneInfo.Local`/`TimeZoneInfo.ConvertTime` (only `Features/Dashboard/` was scanned by T050; this task adds the equivalent scan for `Features/Reports/`), plus the runtime guarantee `DateTimeOffset.UtcNow.Offset == TimeSpan.Zero`. Also live-verified (quickstart V6): `Dashboard overdueTaskCount=1` matched `Reports overdueTasks sum=1` for the same PM caller against the real dev database.
- [X] T084 **Proved the contract gate fails, then reverted** — with two real findings along the way, both handled and documented rather than hidden:
  1. **The baseline gate was NOT clean before this task started** (the "check the baseline first" instruction this stage carried caught something real). `ReportsController` had zero `[ProducesResponseType]` attributes anywhere (every action showed a bare `200 OK` with no schema to Swashbuckle) and `entityType`/`groupBy` were plain `string?` parameters with no enum type, so Swashbuckle couldn't emit the enum constraint the contract declares. Root-caused and fixed **before** T081 began (not part of T081-092's own scope, but required to unblock T084): added `[ProducesResponseType]` to all five actions, and added `ReportsOperationFilter` (an `IOperationFilter`, mirroring the existing `ListProjectsOperationFilter`/`ListTasksOperationFilter` precedent exactly) to annotate `entityType`/`groupBy`'s enum schema **without** changing them to real C# enum types — a real C# enum on a query parameter would have silently swapped FluentValidation's clean "'entityType' must be one of: ..." `ValidationProblem` for ASP.NET's raw model-binding 400, a behavior regression the codebase's own established pattern (`ProjectStatusSchemaFilter`'s doc comment) explicitly designed around. Also added `format: int32` to `page`/`pageSize` in `docs/contracts/reports.v1.yaml` (a doc-only precision fix, the same "usually missing format: int32" class of drift flagged as typical for 002-005's own polish stages). Gate confirmed clean after the fix, before proceeding.
  2. **The literal instruction's proof ("add a `format` query parameter, confirm the build fails") does not actually fail**, tried directly: `oasdiff breaking` treats an added, unadvertised *optional* query parameter as non-breaking by design (existing clients are unaffected by a parameter they never send) — confirmed both via the MSBuild target and a direct `oasdiff breaking`/`diff` invocation. A second attempt — renaming a required response DTO property (`OverdueTasks` → `OverdueTasksRenamed`, verified via the generated OpenAPI JSON to actually change the schema and its `required` array) — **also** did not fail the gate; `oasdiff breaking`'s default ruleset here does not check response-body schema property removal at all, only request parameters, response media-type existence, and response headers. **Stopped and reported both findings to the user** (per this stage's explicit instruction) rather than silently substituting a different drift; user chose to use a genuinely-caught category instead. **Final, successful proof**: temporarily removed `"Report"` from `ReportsOperationFilter`'s `entityType` enum list (an enum-value removal — the same category T084's own pre-work had just fixed at baseline) → `dotnet build -p:CheckApiContract=true` failed with `MSB3073`/exit 1 and `error [request-parameter-enum-value-removed]` → reverted → confirmed clean build again. quickstart.md's V14 should be read with this same caveat; both describe the identical `?format`-add proof that doesn't hold under this repo's current `oasdiff` config.
- [X] T085 **Executed the full quickstart V1–V15 live**, results below.
- [X] T086 [P] **Verified** — `ls src/ProjectManagementApp.Infrastructure/Persistence/Migrations/` shows only `InitialCreate`, `AddProjectIndexes`, `AddTaskIndexes`, `AddTeamMemberIndexes`, `AddActivityLogProjectId` (005's fix) — nothing 006-authored, no `ReportArtifact`/`ReportSchedule` table.
- [X] T087 [P] **Profiled live** against the dev database (accumulated real data from all prior spec stages — dozens of projects, hundreds of tasks, not a synthetic empty seed). Representative handler timings from the live run's Serilog output: `GetProjectProgressQuery` 14-36ms, `GetTaskCompletionQuery` 8-49ms, `GetTeamPerformanceQuery` 58ms, `GetActivityReportQuery` 8-105ms (the 105ms case was the first cold-cache call), `GetReportCatalogQuery` 0ms. All scoped, grouped aggregates run as a single SQL statement per report (no N+1 — confirmed by inspection of each handler in stages 1-4, `GroupBy` composed in SQL except Task Completion's documented Npgsql-translation workaround which groups client-side over an already date-bounded set, not per-row). The threshold guard's probe (`page=1, pageSize=1`) added a single lightweight `SELECT count(*)` + `LIMIT 1` pair ahead of the real read — negligible relative to the real page's own query cost, confirmed both by automated `SqlCapture` assertions (T064) and by inspecting the live server log's query pairs for a 422 case (exactly one count+limit-1 pair, no `LIMIT 20`) versus a 200 case (two pairs: probe then real page, plus the audit `INSERT`).
- [X] T088 [P] **Added.** `ReportsController`'s class-level `<remarks>` rewritten (was stale from the stage-1 stub) to state the one-deliberate-audit-write rule and the client-side-export rule explicitly, citing `ExportArchitectureTests`. `GetProjectProgressQueryHandler`, `GetTaskCompletionQueryHandler`, and `GetTeamPerformanceQueryHandler` each gained a doc-comment line noting the one `ReportGenerated` write; `GetActivityReportQueryHandler` already documented it in its `<remarks>` from stage 4, and `GetReportCatalogQueryHandler` already documented its deliberate **non**-audit from stage 1.
- [X] T089 [P] **Updated** root `README.md` — new "Reports module (006)" section (mirrors the existing Dashboard-module section's structure exactly): the five endpoints table, the `Reports:*` configuration keys table (`MaxWindowDays`, `LargeReportRowThreshold`, `AuditOnGeneration` — with the "should stay `true`" note), the no-migration statement, the Dashboard/Reports parity note, and a link to `specs/006-reports/quickstart.md`. Also added the (previously missing) 005 and 006 quickstart links to the root Documentation list.
- [X] T090 [P] **Confirmed and recorded** in `docs/reports-deliverable.md`: all four report types map to a backend slice + frontend component; `chart.js@^4.5.1` is the only charting library referenced anywhere under `features/reports/` (grep-verified across the three visualized reports — Activity is intentionally table-only); `jspdf@^4.2.1` + `papaparse@^5.5.4` are consumed exclusively through one `ReportExportService`, referenced by all four report components including Activity (grep-verified); cross-cutting guarantees (no migration, exactly-one-audit, Dashboard parity, contract-gate proof) cross-referenced to their respective tests. Explicitly disclosed what was **not** done: no headless-browser click-through — Chart.js rendering and PDF/CSV download prompts are confirmed via the automated frontend spec suite and static import/dependency checks only, same disclosed limitation as 005's dashboard charts.
- [X] T091 **Scanned, nothing to remove.** No `console.log`/`console.debug`/`console.warn` anywhere under `features/reports/` or the reports-related `core/services/`; no `Console.Write*` in the backend Reports slices; no commented-out statement lines (`// var`, `// return`, `// if(`, `// await`, `// const`, `// let`) in either the backend `Features/Reports/` tree or the frontend `features/reports/` tree. Also removed one incidental leftover: a stray `project-progress_2026-07-01_2026-07-31.pdf` file in the Web project root from earlier manual export testing (untracked artifact, not part of any commit).
- [X] T092 **Security review performed against spec 006 §Security Rules, all four confirmed**:
  - *"Authenticated by default; role gate via attributes only; scope enforced in the query source"* — all five `ReportsController` actions carry plain `[Authorize]` (verified by direct inspection), no `[Authorize(Roles=...)]` anywhere; scope resolution happens inside each handler via `ReportScope.ResolveAsync`/`IProjectAccessPolicy`.
  - *"Named out-of-scope project/user → 403; `projectScope=all` auto-narrowed"* — covered by `ProjectProgressForbiddenTests`, `TeamPerformanceForbiddenTests`, `ActivityScopeTests`, plus this task's own `AuditContractTests.A403Request_WritesNoAuditRow`; live-verified (quickstart V3).
  - *"TeamMember Team Performance is self-only, enforced server-side"* — covered by `TeamPerformanceSelfOnlyTests`; live-verified (quickstart V4): TM naming TM2's `userId` returns 1 row, TM's own id, not 403.
  - *"Activity read only through `IActivityLogService`; every generation audited; no domain write"* — covered by `ActivityReadPathTests` (source scan proving no `.ActivityLogs` literal under `Features/Reports/`) and this task's `AuditContractTests` (exactly-one-audit, no domain write, no `xmin` bump).
  No new findings beyond T084's contract-gate baseline fix (already remediated above).

**Checkpoint**: Feature complete. All Phase 9 tasks done; full regression green (292 Api.Tests,
230 Application.Tests, 34 Infrastructure.Tests, 37 frontend); contract gate clean. Full live
quickstart V1-V15 results:

| # | Scenario | Result |
|---|---|---|
| V1 | Catalog describes exactly 4 reports; TM sees `"note":"self only"` on Team Performance | PASS |
| V2 | Three-role scope across every report (Admin all, PM only A, PM2 only B, TM only A) | PASS |
| V3 | Named out-of-scope `projectScope` → 403; `all` → 200, silently narrowed | PASS |
| V4 | TM naming TM2's `userId` → 1 row, TM's own id, not 403; Admin naming a stranger → 403 | PASS |
| V5 | 3/12 closed → `completionPercent:25`; re-open → `closedTasks` 3→2 | PASS |
| V6 | Dashboard `overdueTaskCount` (1) == Reports summed `overdueTasks` (1), same PM caller | PASS |
| V7 | 5 weekly buckets, zero-filled, one non-zero (2 closures today); invalid `groupBy` → 400 | PASS |
| V8 | Steady case → plausible future date (2027-03-15); zero-task project → `null` | PASS |
| V9 | Scoped totalCount per role (PM=20, PM2=1); named out-of-scope `projectId` → 403; `pageSize=500`→100 clamp; `page=-1`→400 | PASS |
| V10 | Threshold override (5) via a second API instance: wide window → 422 with only a probe query executed (no `LIMIT 20` before the 422); narrowed (via `entityType` filter) → 200 with probe+real-page+audit-INSERT all logged | PASS |
| V11.4 | `?format=pdf` on a report → ignored, still 200; `/reports/export` → 404 (no such route) | PASS (1-4/5/6 covered by automated `export-network.spec.ts`/T075, `ExportArchitectureTests`/T082, `report-export.service.spec.ts`/T078 — no live browser session) |
| V12 | Direct `psql` against dev DB: +1 per data request, +0 catalog/400/403/re-export; `projects.xmin` unchanged | PASS (live DB query, improves on stage 4's disclosed gap) |
| V13 | No 006-authored migration; no `ReportArtifact`/`ReportSchedule` table | PASS |
| V14 | Gate passes at baseline; literal `?format`-add proof does not fail (see T084); enum-removal proof does fail (`MSB3073`), revert confirmed clean | PASS (with the T084 caveat) |
| V15 | Catalog-driven forms, Chart.js, TM single-member card, lazy route, 422 prompt | Covered by automated frontend suite (37 passing) — no live browser session |

Two scenarios (V11 parts 1-3/5, V15) rely on the automated frontend suite rather than a live browser
click-through, disclosed honestly rather than claimed as directly observed. Everything else was
exercised against a real running API instance and, for V12, a real `psql` query against the actual
dev database.

**006 Reports is complete and ready for review.**

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
