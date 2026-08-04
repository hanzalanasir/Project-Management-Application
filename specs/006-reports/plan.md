# Implementation Plan: 006 Reports

**Branch**: `006-reports` | **Date**: 2026-07-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/006-reports/spec.md`

**Governed by**: Project Constitution **v1.3.0** · **Governance §5 gate**: spec 006 was revised against
v1.3.0 on 2026-07-29 — **the gate is satisfied and planning may proceed.**

> **Final module.** This plan inherits everything structural from 001–005 and
> **[ADR-0007](../../docs/adr/0007-implementation-conventions.md)**; see [research §A](research.md).
> With 006 planned, **every requirement in the brief is covered by exactly one of 001–006.**

---

## Summary

Produce the brief's four report artifacts — Project Progress, Task Completion, Team Performance, Activity —
over parameterized date windows, role-scoped, with **client-side PDF/CSV export**.

**Technical approach.** Five **query-only** slices under `Features/Reports/<UseCase>/`: a catalog that
describes the reports (so the frontend builds parameter forms dynamically rather than hard-coding four),
plus one resource-per-report-type endpoint, each with its own typed parameter contract. Scope reuses 002's
and 003's `ApplyScope`; the Activity Report reads through `IActivityLogService.QueryScopedAsync`. Metric
definitions are **shared with 005, not merely similar** — a cross-feature test asserts identical values,
which is only possible because 005's timezone was fixed to UTC during its planning pass. The API returns
**JSON only**; jsPDF and papaparse render both formats in the browser through one `ReportExportService`.
The single departure from read-only is **one `ReportGenerated` audit row per generation** — a
security-relevant access record that touches no domain entity.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10 · TypeScript strict / Angular 22 — inherited, unchanged

**Primary Dependencies**: no new backend packages. Frontend adds **jsPDF** and **papaparse**
(Constitution III), plus Chart.js already introduced by 005.

**Storage**: PostgreSQL 18, **read-only over domain data**. **No table, column, index, or migration.** The
only write is one `activity_logs` row per generation.

**Testing**: xUnit + Testcontainers PostgreSQL (ADR-0007 §2). Three assertions carry the feature: the
**TeamMember self-only clamp**, **Dashboard value parity**, and **exactly-one-audit-per-generation**.

**Target Platform / Project Type**: unchanged — existing solution, extended in place

**Performance Goals**: each report is a scoped grouped aggregate pushed to the database; bounded reports
returned in full; the Activity Report is paginated **and** threshold-guarded so no unbounded set is ever
materialized or shipped to a browser renderer.

**Constraints**: scope in the query · parameters narrow, never widen · **UTC fixed** · metric definitions
value-identical to 005 · no domain write · export entirely client-side

**Scale/Scope**: **5 endpoints, 6 user stories, 5 UI screens** (report picker + four report views).

---

## Constitution Check

*GATE: must pass before Phase 0. Re-checked after Phase 1.* Rows marked **↩** are inherited.

| Principle | Gate | Status |
|---|---|---|
| **I.1** Scope fidelity | Implements the brief's Reports module **by aggregation + export**, no new persistence | ✅ |
| **I.2** No bonus features | No custom-report DSL, no scheduling/email, no persisted artifacts, no SignalR | ✅ |
| **II.1** Three-tier separation | Angular presentation + export render; API owns all data and scope | ✅ ↩ |
| **II.2** Vertical slice, thin controllers | 5 **query-only** slices; controller is one `Send()` | ✅ |
| **II.3** Resource-oriented URLs | `/api/reports/<report-type>`, nouns | ✅ |
| **II.4** Real-time not precluded | On-demand generation; nothing blocks a later push | ✅ |
| **III** Stack locked | **jsPDF + papaparse + Chart.js exactly as III specifies** — the client-side export decision is *required* by the library lock, not merely compatible with it | ✅ |
| **IV.1** DbContext, no raw SQL | LINQ grouped aggregates via `IApplicationDbContext`; no raw SQL | ✅ |
| **IV.2** Migrations | **N/A — 006 adds none** (asserted, quickstart V13) | ➖ |
| **IV.3** Entities/relationships | Reads only; defines nothing | ➖ |
| **IV.4** Audit every write | **One `ReportGenerated` row per generation** — the deliberate exception, stated in spec Purpose/T.1/B.7 and research R-1. Catalog unaudited; no domain entity written | ✅ |
| **IV.5** Seed | 006 adds none | ➖ |
| **V.1 / V.4** Auth by default, secrets | Inherited | ✅ ↩ |
| **V.2** Roles via attributes only | `[Authorize]` (all three roles); scope and the self-only clamp live in the handler | ✅ |
| **V.5** Validation | Window present, `from ≤ to`, valid `groupBy`, paging bounds — via the inherited behaviour | ✅ |
| **V.6** CORS | Inherited | ✅ ↩ |
| **VI.1 / VI.3 / VI.5** Base path, RFC 7807, Swagger dev | Inherited | ✅ ↩ |
| **VI.2** Status codes | 200/400/401/403/404 + **422**. 422 is absent from VI.2's list — **spec-declared and OQ-resolved**, see Complexity Tracking | ⚠️ justified |
| **VI.4** Pagination | Activity Report pages; catalog and the three bounded reports correctly do not | ✅ |
| **VI.6** Endpoint naming | Reporting endpoints under `/api/reports`, exactly as VI.6 directs | ✅ |
| **VII.1–VII.2** Lazy standalone `reports` group | `loadChildren` → `reports.routes.ts` | ✅ |
| **VII.3** HTTP in services | `ReportsService`; DTO types generated from the contract | ✅ |
| **VII.4 / VII.7** Interceptors, global errors | Inherited; 422 surfaces a narrow-range prompt | ✅ ↩ |
| **VII.5** Guards only | Functional role guard | ✅ |
| **VII.6** Reactive Forms | **Catalog-driven** Reactive Forms — built from descriptors, not hard-coded | ✅ |
| **VII.8** Export service | **The feature VII.8 was written for.** Export lives in one `ReportExportService`, never per component | ✅ |
| **VIII.1–VIII.5** Code quality | Inherited | ✅ ↩ |
| **IX.1** xUnit + `WebApplicationFactory` | Scope matrix per report; self-only clamp; 422; exactly-one-audit | ✅ |
| **IX.2 / IX.3 / IX.4** Frontend tests, no merge on red, builders | Inherited | ✅ ↩ |
| **X.2** API-first | `docs/contracts/reports.v1.yaml` authored **before** any handler; drift proof in quickstart V14 | ✅ |
| **X.3** ADRs | ADR-0001..0007 apply; **no new ADR** — 006 introduces no repo-wide convention | ✅ |
| **XI** Deployment | Unchanged | ✅ ↩ |
| **Governance §5** | **Satisfied** — 006 revised 2026-07-29 | ✅ |

**Gate result: PASS**, with one justified deviation recorded below.

---

## Project Structure

### Documentation (this feature)

```text
specs/006-reports/
├── plan.md · research.md · data-model.md · quickstart.md
├── contracts/README.md      # Pointer → docs/contracts/reports.v1.yaml
├── checklists/requirements.md
└── tasks.md                 # Phase 2 — /speckit.tasks

docs/contracts/reports.v1.yaml   # THE CONTRACT — catalog + 4 reports, 5 operations
```

### Source Code — **delta only**

```text
src/
├── ProjectManagementApp.Domain/                     # UNCHANGED — 006 defines no entity
│
├── ProjectManagementApp.Application/
│   └── Features/Reports/                            # NEW — five QUERY slices, no commands
│       ├── GetCatalog/           Query · Handler    → IReadOnlyList<ReportDescriptor>  (unaudited)
│       ├── GetProjectProgress/   Query · Validator · Handler
│       ├── GetTaskCompletion/    Query · Validator · Handler
│       ├── GetTeamPerformance/   Query · Handler    (forces self-row for TeamMember)
│       └── GetActivityReport/    Query · Validator · Handler  (422 threshold guard)
│
├── ProjectManagementApp.Api/Controllers/ReportsController.cs   # NEW — five thin GET endpoints
│
└── ProjectManagementApp.Web/src/app/
    ├── core/services/
    │   ├── reports.service.ts                       # NEW — HTTP
    │   └── report-export.service.ts                 # NEW — jsPDF + papaparse (VII.8), ONE service
    └── features/reports/                            # NEW — lazy standalone route group
        ├── picker/ (catalog-driven form) · project-progress/ · task-completion/
        ├── team-performance/ · activity/
        └── reports.routes.ts

tests/                                               # extended in place
├── …Application.Tests/Features/Reports/             # metric definitions, self-only clamp, projections
├── …Api.Tests/Reports/                              # scope matrix, 403-vs-self-row, 422, one-audit
└── …Api.Tests/CrossFeature/DashboardReportParityTests.cs   # NEW — 005 ↔ 006 value parity (NFR-002)
```

**Structure Decision.** Query-only slices, no new assembly, no new test project, **no new shared-kernel
member**. The one cross-cutting addition is the **cross-feature parity test**, which belongs to 006 because
it is 006's requirement — 005 recorded it as a follow-up precisely so it would land here.

---

## Complexity Tracking

> 001's four, 002's two, 003's one, and 004's one justified deviation carry forward unchanged. 005 added
> none.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| **`422 Unprocessable Content`** on the Activity Report — a status code **Constitution VI.2 does not list** | VI.2's list is *representative, not exhaustive* — the reasoning 001's Clarifications already used to admit **409**. 422 is the semantically correct answer here: the request is well-formed and every parameter individually valid; it is the **result size** that cannot be processed. Crucially, this code is **declared by the spec** (API catalog, B.5) and **resolved through OQ-006-02**, not invented at plan time. **002 rejected 428 for exactly the opposite reason** — a plan may not introduce a status code; a spec may declare one. That distinction is the rule worth preserving. | *400* — semantically wrong; conflates an oversized result with a malformed window, so a client cannot distinguish "fix your syntax" from "narrow your range". *Silent truncation at N rows* — rejected outright: a report that quietly omits data is worse than one that refuses, especially an audit-facing one. *Server-side streaming* — deferred (OQ-006-02); would sit behind the same threshold config. |
| **One `activity_logs` write in a read-only feature** | Report generation is not a domain write, so IV.4 does not compel it — but *"who ran the team-performance numbers, with what parameters, when"* **is** a security-relevant access event, and the audit log is where that record belongs. Stated three times in the spec so it reads as intentional. | *Write nothing (match 005)* — loses the access record the organization needs. *Log to Serilog only* — the answer to "who accessed what" should live with every other audit record and be queryable by the Activity Report itself. |

---

## Post-Design Re-check (after Phase 1)

- **A fourth shared-kernel gap was found in pre-flight and fixed before planning** — `AuditAction` is one
  shared enum created by 001's T018, but its value list held only 001's six *User* actions. 002/003/004/006
  all contribute values, and no plan said who adds them, so 002's first audited write would not have
  compiled. T018 and 001's data-model now specify the **complete 18-value set**. The §2/§3/§6/§7 sweep
  missed it because `AuditAction` is a Domain enum, not a shared-contracts type — a limit of that sweep
  worth remembering.
- **Both of 005's closures paid off immediately.** `QueryScopedAsync` gave the Activity Report a legal read
  path, and fixed-UTC made NFR-002's parity requirement satisfiable rather than breakable by configuration.
  Had either survived, 006 would have inherited a blocker.
- **The 422/428 tension was resolved into a rule** rather than an exception: *a spec may declare a status
  code; a plan may not invent one.* That keeps 002's rejection of 428 and 006's use of 422 consistent.
- **VII.8 verified as satisfied, not merely claimed** — export lives in one service, and the contract has
  no `?format` parameter or export endpoint for it to leak into.
- **The self-only-vs-403 asymmetry was re-examined** and holds: it is the *safer* behaviour, and it mirrors
  003's empty-page-not-403 filter rule for the same least-privileged role.
- **No new violations** beyond the two recorded.

**Gate result after design: PASS.**

---

## Follow-ups

1. **Cross-feature parity test now has a home** (closing 005's Follow-up 4). `DashboardReportParityTests`
   asserts 005's `overdueTaskCount` equals the sum of 006's `overdueTasks` for the same caller and window,
   and must be run with the server clock in a non-UTC zone to be meaningful.
2. **A shared metric-expression helper is deferred, not rejected** (research R-4). 005 and 006 currently
   guarantee parity by *shared definition plus a test*. If a third consumer ever needs "overdue", extract
   the expressions into the shared kernel rather than copying them a third time.
3. **`Reports:AuditOnGeneration` defaults to `true`** and should stay that way. The config exists for
   deployments that would rather audit only explicit exports; flipping it loses the record of *data
   production*, which is the security-relevant event. Documented so the default is a decision, not an
   accident.
4. **All six OQ-006-01..06 are resolved** in the spec's Clarifications and require no plan-time action.
5. ~~**D1 — `ErrorKind` had no `UnprocessableContent` member.**~~ **✅ RESOLVED 2026-08-04.** `/speckit.analyze`
   found that 006 T019 proposed *adding* a member to a shared-kernel enum. Fixed at the source:
   `shared-contracts` §1 now declares seven kinds with a **422** mapping row; **001 T019** creates all seven
   and **001 T036** maps all seven; **006 T019** now *verifies and consumes* rather than introduces.
6. ~~**F1 — `projectScope` list vs single id.**~~ **✅ RESOLVED 2026-08-04.** Decided in favour of the
   **list**: FR-002 and the T.3 catalog descriptor (`projectIds|all`) both call for it, so the bare-string
   contract was the outlier. Encoded as **comma-separated ids**, with **all-or-nothing 403** if any named id
   is out of scope (research **R-7**); contract, T012, T013, and T033 updated.
7. ~~**C1 — the row-count estimator was undefined.**~~ **✅ RESOLVED 2026-08-04.** Specified as an **indexed
   `CountAsync()` over the scoped and filtered query, before paging or projection** (research R-5), with
   heuristics, `EXPLAIN` statistics, and fetch-threshold+1 explicitly rejected. T016 updated.
8. **The recurring shared-kernel gap is now a written rule.** D1 was the **fifth** instance. Recorded as
   **[ADR-0007 §5](../../docs/adr/0007-implementation-conventions.md)** with the five-case table, the
   mechanical grep check, and the three traps that produced false passes (verification tasks, prose
   mentions, and sweeps scoped by section rather than by artifact).

---

## Brief coverage — complete

With 006 planned, every brief requirement maps to exactly one feature:

| Brief area | Covered by |
|---|---|
| Frontend modules — Dashboard · Projects · Tasks · Team · Reports · Auth | 005 · 002 · 003 · 004 · **006** · 001 |
| Backend — JWT+RBAC · project endpoints · task CRUD · team management · reporting | 001 · 002 · 003 · 004 · **006** |
| Database — Users · Projects · Tasks · TeamMembers · ActivityLogs | 001 (all five created in `InitialCreate`) |
| Reporting + charts + export | **006** (Chart.js, jsPDF, papaparse) |
| Deliverables — README · API docs · architecture docs · demo · deployment | Constitution X/XI + 001's Polish phase |
| Bonus — Gantt · notifications · Slack/email · real-time · Docker/CI/cloud | **Out of scope** (Constitution I.2); architecture precludes none |

**29 endpoints across six contracts, all authored before any handler exists.**

---

## Phase status

| Phase | Output | Status |
|---|---|---|
| Phase 0 — Outline & Research | `research.md` | ✅ 12 inherited by citation, 6 newly derived, 0 unresolved |
| Phase 1 — Design & Contracts | `data-model.md`, `docs/contracts/reports.v1.yaml`, `quickstart.md` | ✅ Complete |
| Phase 2 — Task breakdown | `tasks.md` | ⏳ Run `/speckit.tasks` |
