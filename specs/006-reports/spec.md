# Feature Specification: Reports

**Feature Number**: 006
**Feature Name**: Reports (Parameterized Read-Only Analytics & Export)
**Phase**: Phase 1 — Foundation
**Priority**: Must Have (Critical)
**Complexity**: High
**Type**: Read-only analytics + export — **no new domain entity, no new domain table, no migration against domain tables; one audit-only write** (`ReportGenerated`)
**Depends On**: **001 Auth & RBAC** (Users, role model, JWT claims, `CurrentUser`, `IActivityLogService`) · **002 Projects** (Project entity, ownership scope, status enum) · **003 Tasks** (Task entity, status enum, due date, completion timestamp, assignee) · **004 Team** (TeamMember membership records) · **005 Dashboard** (no runtime dependency — but the two features MUST share role-scoping and metric definitions) — all five **referenced, not redefined**
**Enables**: — (final module of the initial delivery)
**Created**: 2026-07-22
**Status**: Draft — Ready for Planning
**Governed By**: Project Constitution v1.3.0 (Principles I Scope, II Architecture — vertical slice / Clean Architecture, III Stack, IV Data Access, V Security & Authorization, VI API Design, VII Frontend incl. VII.8 export, VIII Code Quality, IX Testing, X Documentation — API-first)
**Cross-cutting contracts**: [docs/shared-contracts.md](../../docs/shared-contracts.md) — `Result<T>`, `ErrorKind`, `CurrentUser`, `AccessDecision` (**`ApplyScope` + `CanReadAsync` only — `CanMutateAsync` is not used; Reports never mutates a domain entity**), `PagedResult<T>` (Activity Report preview only — see T.4), error→HTTP mapping · ADRs [0001](../../docs/adr/0001-angular-standalone-components.md) standalone Angular · [0002](../../docs/adr/0002-same-origin-hosting.md) same-origin hosting · [0003](../../docs/adr/0003-result-error-contract.md) error contract · [0004](../../docs/adr/0004-optimistic-concurrency.md) concurrency (**not applicable — no mutations to domain entities**) · [0005](../../docs/adr/0005-mapping-and-validation.md) mapping/validation · [0006](../../docs/adr/0006-vertical-slice-clean-architecture-api-first.md) vertical slice + Clean Architecture + API-first
**Generated Via**: `/speckit.specify` (merged requirements + solution design, per project convention)
**Revised**: 2026-07-29 — re-organized against Constitution v1.3.0 (vertical slice / Clean Architecture / API-first; II.2/IV.1/X.2/VII.3). Business rules, roles, endpoints, status codes, read model, and clarifications are unchanged — only the code-organization framing was updated. Satisfies the Governance §5 revision gate.

---

## Purpose

Produce the **report artifacts** the brief's Reports module requires — Project Progress, Task Completion, Team Performance, and Activity — from the data owned by 001–005, and let a user **export** them to PDF and CSV (Constitution VII.8, via a report-export service, not per-component). This feature introduces **no new domain entity, no new domain table, and no migration against domain tables**. It reads, aggregates, and hands back report data; the browser renders the download.

Reports is the second read-only feature (after 005 Dashboard) and shares its **role-scoped-at-the-source** discipline, but differs deliberately in three ways worth naming up front:
1. **Parameterized, not snapshot** — a report caller chooses a **date window, project scope, and grouping**; Dashboard tiles are unparameterized "right now" snapshots.
2. **Downloadable artifacts** — a report yields a **PDF/CSV download**; Dashboard is view-only.
3. **Historical windows** — a report may cover a **time-series window over the past** that Dashboard deliberately never shows.

There is **one deliberate exception** to the "read features don't write" rule: report generation is a **security-relevant action** ("who ran which report, with what parameters, when"), so each generation writes a single **audit-only** `activity_logs` entry (`ReportGenerated`) through `IActivityLogService`. This is the sole write this feature performs, it touches **no domain entity**, and it is the one place a read-side feature legitimately writes to the audit log (contrast 005, which writes nothing — see B.7).

This is a **merged specification**: requirements (Purpose → User Stories → Functional Requirements) and solution design (Technical Design → Implementation Blueprint) live in one file by project convention.

### Brief coverage confirmation (001–006)

With this feature the initial delivery is complete. Every requirement in the brief maps to exactly one spec:

| Brief requirement | Covered by |
|---|---|
| **Frontend modules** — Dashboard · Project Management · Task Management · Team Management · Reports · Authentication | 005 · 002 · 003 · 004 · **006** · 001 |
| **Backend** — JWT auth + RBAC · Project endpoints (`GET/GET{id}/POST/PUT/DELETE /projects`) · Task CRUD · Team management · Reporting endpoints | 001 · 002 · 003 · 004 · **006** |
| **Database tables** — Users · Projects · Tasks · TeamMembers · ActivityLogs | 001 · 002 · 003 · 004 · 001 (written by all) |
| **Steps** — env setup · frontend (lazy modules, services, Reactive Forms, NgRx) · backend (EF Core, controllers, JWT, logging/exception handling) · DB (schema, migrations, seed) · integration · **reporting + charts + export** · IIS deploy | Constitution X/XI + every feature's frontend/backend sections; charts (Chart.js) 005/006; **export 006**; seed 001; deploy ADR-0002 |
| **Deliverables** — source · README · API docs (Swagger) · technical/architecture docs · demo · deployment instructions | all specs + Constitution X.1/X.3/X.5, XI.3 (implementation-time deliverables) |
| **Bonus** — Gantt · notifications · Slack/email · advanced search · role-based dashboards · real-time · Docker · CI/CD · cloud | **Out of scope** (Constitution I.2); role-scoped dashboards partially satisfy "role-based dashboards" in base scope; architecture does not preclude real-time (II.4) |

**No brief requirement is left uncovered.** Bonus items remain out of scope per Constitution I.2.

## Business Value

Reporting is the deliverable a stakeholder actually takes away from the meeting: a PDF of project progress, a CSV of task throughput, a record of who did what. Dashboard answers "how are things right now"; Reports answers "how did things go over this period, in a form I can file, email, or present." By reusing Dashboard's exact scope and metric definitions, a manager never sees one overdue count on the dashboard and a different one in the report. By auditing every generation, the organization can answer "who pulled the team-performance numbers last quarter." And by keeping the whole feature read-only over domain data, it adds analytical value with zero risk to the integrity of Users, Projects, Tasks, or Teams.

## Actors

**Primary Actors**
- **Admin** — any report, any project, any date range; any user included in Team Performance.
- **ProjectManager** — reports scoped to the projects they **own** (002 ownership); Team Performance limited to members of those projects (004).
- **TeamMember** — a **read-only self-view**: Project Progress and Task Completion for projects they are a **member** of (004); Team Performance limited to **their own row**; Activity limited to entries on entities they can already see.

**Secondary Actors**
- **Source features (non-actor)** — 001 (audit-log service, Users), 002 (Projects + ownership scope), 003 (Tasks + completion data), 004 (membership) supply every datum; 005 (Dashboard) defines the metric semantics Reports must match. Reports owns none of them.

## Scope Summary

**In scope**: a **report catalog** endpoint that describes the available report types and their parameters (drives the report-selection UI); four parameterized, role-scoped report endpoints — **Project Progress**, **Task Completion**, **Team Performance**, **Activity** — each returning a typed JSON report for on-screen preview; **PDF and CSV export** of any report via a client-side **report-export service** (jsPDF + a lightweight CSV utility, Constitution III/VII.8); reuse of 002/003's `ApplyScope` predicates so every report resolves to the **same values Dashboard would show the same caller**; a single **audit-only** `ReportGenerated` entry per generation; the lazy-loaded Angular **`reports` route group** (standalone components per ADR-0001) with a dedicated `ReportsService`, the shared `ReportExportService`, Reactive-Forms parameter pickers, Chart.js visualizations, and functional role guards.

**Out of scope**: **any write to a domain entity** (the `ReportGenerated` audit entry is the sole write); user-authored custom reports or a report-query DSL (brief's "advanced search/filter" bonus); scheduled/recurring generation and email/Slack delivery (brief bonus; the model must not *preclude* a later `ReportSchedule` — OQ-006-01); **persisted report history / "recently generated" list** (reports are transient by default; a `ReportArtifact` concept would be a separate spec — OQ-006-03); real-time report push via SignalR (must remain possible — Constitution I.2 / II.4 — but generated on demand for v1); chart libraries other than Chart.js (III locks Chart.js; D3 only with justification); new domain entities, tables, or migrations against domain tables.

## Access Logic (applies to every story)

Enforced **server-side**, in this order:

1. **Authenticated by default** — every endpoint requires a valid JWT (001). No/invalid/expired token → **401**.
2. **Role gate (controller, attribute-only)** — `[Authorize]`; every report endpoint permits **all three roles** (each gets *their own scoped report*). Ad-hoc role checks in method bodies remain prohibited (Constitution V.2).
3. **Scope gate (slice handler, at the query source)** — reports' handlers reuse `ApplyScope` (002/003): Admin unscoped; ProjectManager scoped to owned projects; TeamMember scoped to member-of projects (and, for Team Performance, to **their own row**). Aggregates are computed **within scope, in SQL** — never fetched then filtered.
4. **Parameter scope-check** — because reports are **parameterized** (unlike Dashboard), a caller can *name* a specific `projectId`/`userId`. A **named resource outside the caller's scope → 403** (inheriting 002/003/004's convention, maskable to 404 by config). A `projectScope=all` request is auto-narrowed to the caller's visible set (no 403). *(This is why Reports surfaces 403 where Dashboard, which is parameter-free, does not.)*
5. **No mutation gate** — `CanMutateAsync` is deliberately absent; the feature has no domain write path.
6. **Identity from the token** — the acting user comes from `ICurrentUserService`, never the request body.

## Role & Permission Model

The three roles are defined in [001 Auth & RBAC](../001-auth-rbac/spec.md) — each user holds **exactly one**, carried as a single JWT `role` claim. This feature adds **no new role dimension**; it maps the three onto report scope, with a **least-privilege** rule for the TeamMember that is stated here explicitly so the reasoning is visible:

| Report | Admin | ProjectManager | TeamMember (least-privilege) |
|---|---|---|---|
| **Project Progress** | Any project(s) | Owned projects | Member-of projects only |
| **Task Completion** | Any project(s) / any assignee | Owned projects / their members | Member-of projects; may filter to self |
| **Team Performance** | Any user, any project | Members of owned projects | **Only their own row** — a personal productivity view, **never a peer comparison** |
| **Activity** | All activity | Activity on owned projects | **Only entries on entities they can already see** under 001–005 read scope |

**Why the TeamMember rule is drawn this way (least privilege):** a TeamMember is a contributor, not a supervisor. They may legitimately review *their own* progress and throughput and the activity on work they can already see — but exposing *colleagues'* throughput (Team Performance) or projects they aren't on (Project Progress) would grant oversight the role does not carry. So Team Performance for a TeamMember returns a **single row (themselves)**, and every report is bounded by the same visibility 002/003/004 already enforce. Admin and ProjectManager scopes mirror their existing project authority exactly.

---

## Clarifications

### Session 2026-07-22

- Q: Timezone for date-range parameters and bucketed/displayed timestamps — UTC or caller-local? → A: **UTC (server zone), fixed.** `from`/`to` and all bucket boundaries (Task Completion day/week/month) are evaluated in UTC, so a report is **reproducible across viewers** — the same parameters yield the same numbers and the same exported PDF/CSV regardless of who runs it or where they are. The applied timezone (UTC) is shown in the report header. Caller-local display is a possible later option, not a v1 configurable.
- Q: Large-Activity threshold — the row limit, and forced-narrowing vs server-side streaming fallback? → A: **~10,000 rows (configurable), forced narrowing via 422.** A report/export whose result set would exceed the threshold returns **422** with a "narrow the date range" prompt; the **entire export pipeline stays client-side** (jsPDF/papaparse), so no server-side CSV streaming path is built for v1. Server-side streaming can be added later behind the same threshold config without changing the JSON contract.
- Q: How is "task completed" determined for Task Completion / Project Progress, given 003's Task has no completion timestamp? → A: **Add a dedicated `closed_at` timestamp to 003's Task** (set when status → `Done`, cleared on re-open) via a **lightweight additive migration** — a small follow-up to 003 actioned at plan time, **not a redesign**. Reports reads `closed_at` for completion buckets and closed counts; a re-opened task (its `closed_at` cleared) drops from completed counts until re-completed. This spec records the decision; it does not edit 003.
- Q: (OQ-006-04) Exact fields of each report DTO, the "projected completion" formula, and how a re-opened task is counted? → A: **Field tables finalized** in the Consolidated Read Model (below). A task is **"closed" iff `status = Done`** (`closed_at` not null); a **re-opened** task (`closed_at` cleared) is **excluded** from both Project Progress's `closedTasks` count **and** Task Completion's buckets — the identical rule in both, consistent with 003's `closed_at`-cleared-on-reopen. **Projected completion** = a linear throughput extrapolation: `avgClosedPerDay = closedTasksInWindow ÷ windowDays`, `projectedCompletion = today(UTC) + ceil(openTasks ÷ avgClosedPerDay)`; **`null`** when there is no throughput or no open tasks.
- Q: (OQ-006-06) Does v1 ship more than the four baseline reports? → A: **Four only for v1** — Project Progress, Task Completion, Team Performance, Activity. The brief names three explicitly, and the `ActivityLogs` table's stated purpose ("for auditing and reporting") covers the fourth (Activity). More types can be added later via the catalog without a contract change.
- Q: (OQ-006-01) Scheduled/emailed reports in v1? → A: **No — out of scope for v1** (brief bonus, Slack/email). The model does not preclude a later `ReportSchedule` concept, but no schema is committed now.
- Q: (OQ-006-03) Are generated PDFs/CSVs persisted server-side? → A: **No — transient for v1**, re-generated on demand. A "recent reports" history would be a separate `ReportArtifact` spec; the model leaves room but commits no persistence now.

---

## User Stories

> Story IDs `US-006-01..06`. Each story: **A** Summary · **B** Quality Validation (INVEST · 3Cs · 7Cs · Given-When-Then · edge cases · audit/security · configurability) · **C** UI · **D** API · **E** DB · **F** Separation of concerns. Consolidated read model, API catalog, technical design, and implementation blueprint follow the stories.

---

### US-006-01 — Discover reports and their parameters (catalog)

**A. Summary**
- **Story ID**: US-006-01 · **Title**: List the available reports and the parameters each needs
- **Actor**: Admin · ProjectManager · TeamMember
- **User story**: *As any authenticated user, I want to see which reports I can run and what inputs each requires, so that the app can present me the right parameter form without hard-coding it.*
- **Business value**: Drives a **dynamic, self-describing** report-selection UI; the single source of truth for "what reports exist."
- **Priority**: **P1** · **Reason**: Enables the report UI, but the reports themselves are the headline.
- **Dependencies**: none beyond 001 auth. **Out of scope**: running a report (US-006-02..05).

**B. Quality validation**
- **INVEST** — Independent ✔ (metadata endpoint, standalone); Negotiable ✔ (the descriptor shape); Valuable ✔ (removes hard-coded forms); Estimable ✔; Small ✔ (a static-ish descriptor list); Testable ✔ (catalog lists exactly the four report types with their declared parameters).
- **3Cs** — Card ✔ (stands alone: "tell me what reports exist and their inputs"); Conversation ✔ (surfaced whether the catalog itself is role-filtered and whether it is paginated — see Edge cases); Confirmation ✔ (the Given/When/Then scenarios cover the descriptor contents and role-visibility — sufficient to call this story done).
- **7Cs** — Clear ✔ (states the catalog is metadata, not data); Concise ✔; Concrete ✔ (each descriptor lists parameter name/type/required and supported formats); Correct ✔ (matches the four report contracts in the API catalog); Coherent ✔ (the parameters it declares are exactly those the report endpoints accept); Complete ✔ (all four types, their params, and formats); Courteous n/a (a metadata list, no user-facing prose).
- **Given/When/Then**
  1. **Given** an authenticated caller, **When** they request the catalog, **Then** **200** with a descriptor per report type: `type`, display `title`, the ordered `parameters` (name, type, required), and the supported export `formats` (`json`, `pdf`, `csv`).
  2. **Given** the catalog, **When** it is inspected, **Then** every parameter a report endpoint accepts appears in that report's descriptor, and no report the caller's role can never run is presented as runnable.
  3. **Given** a role with a narrower surface (TeamMember), **When** they read the catalog, **Then** report descriptors reflect their constraints (e.g. Team Performance is annotated "self only").
- **Edge cases**: the catalog is a **small fixed set** (four types) — returned as a **plain array, not paginated** (see T.4); role-annotation of descriptors (what a role may run) vs a flat list for all — default annotate; adding a fifth report type later (OQ-006-06) extends the catalog without a contract change.
- **Audit/security**: reading the catalog is **not** audited (it exposes no data, only metadata); it returns nothing about projects/tasks the caller cannot see.
- **Configurability**: per-report default parameter values. (v1 exposes exactly the **four** baseline report types — Clarifications 2026-07-22, OQ-006-06 — extensible later via the catalog.)

**C. UI** — **F006-S01 Report Picker** (standalone component in the lazy-loaded `reports` route group): renders the parameter form **dynamically from the catalog** (Reactive Forms built from the descriptor), so a new report type needs no new form code. Loading/error states explicit.

**D. API** — `GET /api/reports/catalog` · `[Authorize]` (all three roles) · **200** with a plain array of report descriptors · **401**.

**E. DB** — **no reads of domain data** (descriptors are static/config-derived); **no writes**.

**F. Separation** — UI: catalog-driven dynamic form. Backend: `GetReportCatalogQueryHandler` returns descriptors (role-annotated); the controller only `Send`s the query. DB: none. QA: descriptor completeness, role annotation, plain-array shape.

---

### US-006-02 — Project Progress Report

**A. Summary**
- **Story ID**: US-006-02 · **Title**: Generate a project-progress report over a date window
- **Actor**: Admin (any) · ProjectManager (owned) · TeamMember (member-of)
- **User story**: *As a manager, I want a per-project progress report — completion %, open vs closed tasks, overdue count, projected completion — over a date window, so that I can report status to stakeholders.*
- **Business value**: The flagship report; the artifact a manager presents. Also the **primary three-role scope acceptance test** for the feature.
- **Priority**: **P0** · **Reason**: The most-requested report and the scope proof.
- **Dependencies**: 002 (projects + scope), 003 (tasks + completion). **Out of scope**: export mechanics (US-006-06).

**B. Quality validation**
- **INVEST** — Independent ✔; Negotiable ✔ (columns were the negotiated decision — now finalized, OQ-006-04, Clarifications 2026-07-22); Valuable ✔; Estimable ✔; Small ✔ (a scoped grouped aggregate per project); Testable ✔ (each metric assertable per role against seeded data).
- **3Cs** — Card ✔ (stands alone: "per-project progress over a window"); Conversation ✔ (surfaced the column set and the "projected completion" formula — both now finalized in the Consolidated Read Model, Clarifications 2026-07-22 — and the named-project-out-of-scope 403; see Edge cases); Confirmation ✔ (the Given/When/Then scenarios cover the three-role scope matrix, the named-out-of-scope 403, the date window, and value-consistency with Dashboard — sufficient, with columns now fixed).
- **7Cs** — Clear ✔ (states scope resolves to the same set 002 would show); Concise ✔; Concrete ✔ (named metrics, exact 200/400/403 outcomes); Correct ✔ (overdue uses 003's definition, matching 005); Coherent ✔ (the overdue/open/closed counts equal Dashboard's for the same caller — one definition, two surfaces); Complete ✔ (all three roles, window, and the out-of-scope case covered, with columns flagged); Courteous n/a (a data report, no prose copy).
- **Given/When/Then**
  1. **Given** projects across owners, **When** an **Admin** runs the report with `projectScope=all`, **Then** **200** with one row per project across the whole system, for the requested window.
  2. **Given** the same data, **When** a **ProjectManager** runs it, **Then** rows cover **only projects they own**; **When** a **TeamMember** runs it, **Then** rows cover **only projects they are a member of**.
  3. **Given** a caller who names a specific `projectId` **outside their scope**, **When** they run it, **Then** **403** (maskable to 404 by config) — the parameterized analogue of 002's out-of-scope read.
  4. **Given** a window `from`/`to`, **When** the report computes, **Then** completion %, open/closed counts, and overdue reflect that window and the caller's scope, and **overdue equals the value 005 Dashboard shows the same caller**.
  5. **Given** generation succeeds, **When** the response returns, **Then** a single `ReportGenerated` audit entry (actor, report type, parameters, timestamp) is written (US-006-06 / B.7).
- **Edge cases**: empty scope (no visible projects) → **200** with an empty `rows` array, not 403/404; a project with zero tasks → completion `0%`, not a divide-by-zero; the column set is **finalized** in the Consolidated Read Model (Clarifications 2026-07-22); "projected completion" uses the linear throughput extrapolation defined there (`null` when no throughput/no open tasks); `from` after `to` → **400**.
- **Audit/security**: generation writes the `ReportGenerated` audit entry (the one write); scope enforced in the query; a named out-of-scope project → 403; no out-of-scope project leaks through a total.
- **Configurability**: default window (e.g. last 30 days); whether "projected completion" is included. (The column set is fixed by the field tables — Clarifications 2026-07-22 — not configurable.)

**C. UI** — **F006-S02 Project Progress** (report view): parameter form (window, project scope) + a Chart.js progress visualization + a tabular body; Export to PDF/CSV via the shared export service (US-006-06). The table paginates **client-side** over the returned (bounded) row set — one row per visible project is a small set.

**D. API** — `GET /api/reports/project-progress?from=&to=&projectScope=` · `[Authorize]` (all three roles) · **200** typed report DTO · **400** (bad params) · **403** (named out-of-scope project) · **401**.

**E. DB** — **reads only** — `projects` (002) + `tasks` (003) aggregated within scope; **writes one `activity_logs`** row (`ReportGenerated`). No domain write, no migration.

**F. Separation** — UI: parameter form + chart + table. Backend: `GetProjectProgressQueryHandler` composes 002's `ApplyScope` into grouped aggregates, audits generation; the controller only `Send`s the query. DB: scoped aggregate read + one audit write (handler → `DbContext` + `IActivityLogService`). QA: three-role matrix (**primary acceptance**), named-out-of-scope 403, Dashboard-value parity, audit written.

---

### US-006-03 — Task Completion Report

**A. Summary**
- **Story ID**: US-006-03 · **Title**: Generate a task-completion trend over a date window
- **Actor**: Admin (any) · ProjectManager (owned) · TeamMember (member-of, may filter to self)
- **User story**: *As a manager, I want a task-completion trend grouped by day/week/month over a window, optionally filtered by project or assignee, so that I can see throughput over time.*
- **Business value**: The historical time-series Dashboard deliberately does not show — the clearest "are we speeding up or slowing down" view.
- **Priority**: **P1** · **Reason**: High value; the signature "historical window" report.
- **Dependencies**: 003 (task **`closed_at`** completion timestamp — see Assumptions/Dependencies), 002/004 (scope). **Out of scope**: export mechanics (US-006-06).

**B. Quality validation**
- **INVEST** — Independent ✔; Negotiable ✔ (grouping set; columns now finalized, OQ-006-04, Clarifications 2026-07-22); Valuable ✔; Estimable ✔; Small ✔ (a grouped-by-period aggregate); Testable ✔ (period buckets assertable against seeded completions).
- **3Cs** — Card ✔ (stands alone: "completion trend over a window, grouped"); Conversation ✔ (surfaced the **completion-timestamp dependency on 003** (Assumptions), timezone-of-bucketing (OQ-006-05), and grouping granularity; see Edge cases); Confirmation ✔ (the Given/When/Then scenarios cover grouping, scope, the optional assignee filter, and the empty window — sufficient now that the completion-timestamp (`closed_at`) and timezone (UTC) questions are settled, Clarifications 2026-07-22).
- **7Cs** — Clear ✔ (names the completion-timestamp assumption rather than hiding it); Concise ✔; Concrete ✔ (day/week/month buckets, exact 200/400 outcomes); Correct ✔ (a task counts in the bucket of its completion date within scope); Coherent ✔ (scope identical to the other reports and Dashboard); Complete ✔ (grouping, scope, filters, and empty window covered); Courteous n/a.
- **Given/When/Then**
  1. **Given** completed tasks over a window, **When** the report runs with `groupBy=week`, **Then** **200** with one bucket per week in the window, each carrying the count of tasks completed in that week **within the caller's scope**.
  2. **Given** `groupBy` = day / week / month, **When** chosen, **Then** the buckets change accordingly; an invalid `groupBy` → **400**.
  3. **Given** an optional `assigneeId` filter, **When** applied by an Admin/ProjectManager, **Then** the trend narrows to that assignee (within scope); **When** applied by a **TeamMember**, **Then** it is constrained to **themselves** (they cannot trend a colleague).
  4. **Given** a window with no completions, **When** the report runs, **Then** **200** with buckets present but zero-valued (a continuous series), not an empty body.
- **Edge cases**: a task completed exactly on a bucket boundary — assigned to one bucket deterministically **in UTC** (Clarifications 2026-07-22); a very wide window with `groupBy=day` (bounded by the number of days — moderate, returned in full); the **completion timestamp** source is 003's **`closed_at`** (Clarifications 2026-07-22); a task later re-opened after completion has its `closed_at` cleared, so it drops from completed buckets until re-completed.
- **Audit/security**: generation audited (`ReportGenerated`); scope in the query; a TeamMember cannot filter to another assignee.
- **Configurability**: grouping granularity (day/week/month); default window. (Re-opened tasks are handled by the `closed_at`-cleared rule — Clarifications 2026-07-22 — not a config toggle.)

**C. UI** — **F006-S03 Task Completion** (report view): window + grouping + optional project/assignee filters; a Chart.js line/bar trend + a per-bucket table; PDF/CSV export (US-006-06).

**D. API** — `GET /api/reports/task-completion?from=&to=&groupBy=day|week|month&projectScope=&assigneeId=` · `[Authorize]` (all three roles) · **200** typed report DTO · **400** · **403** (named out-of-scope project/assignee) · **401**.

**E. DB** — **reads only** — `tasks` (003) grouped by completion period within scope; **writes one `activity_logs`** (`ReportGenerated`). No migration.

**F. Separation** — UI: trend chart + bucket table. Backend: `GetTaskCompletionQueryHandler` (scoped grouped-by-period aggregate), audits generation; the controller only `Send`s the query. DB: scoped aggregate + one audit write. QA: bucket correctness, grouping, TeamMember-self constraint, zero-filled series.

---

### US-006-04 — Team Performance Report

**A. Summary**
- **Story ID**: US-006-04 · **Title**: Generate a per-member performance report (throughput, workload, overdue)
- **Actor**: Admin (any user) · ProjectManager (members of owned projects) · TeamMember (**self only**)
- **User story**: *As a manager, I want per-team-member throughput (tasks closed in window), current workload (tasks assigned), and overdue count, scoped to my projects, so that I can balance load and recognize delivery.*
- **Business value**: The people-facing report — and the sharpest least-privilege boundary in the feature.
- **Priority**: **P1** · **Reason**: High value; must be scoped carefully to avoid peer-surveillance by TeamMembers.
- **Dependencies**: 003 (assignee + completion), 004 (membership pool for who's on the team). **Out of scope**: export mechanics (US-006-06).

**B. Quality validation**
- **INVEST** — Independent ✔; Negotiable ✔ (columns now finalized, OQ-006-04, Clarifications 2026-07-22); Valuable ✔; Estimable ✔; Small ✔ (a per-user aggregate over scoped tasks); Testable ✔ (the **self-only** rule for a TeamMember is directly assertable).
- **3Cs** — Card ✔ (stands alone: "per-member throughput/workload/overdue in my scope"); Conversation ✔ (surfaced the **TeamMember self-only least-privilege** rule and how "throughput" is defined; see Role Model and Edge cases); Confirmation ✔ (the Given/When/Then scenarios include the defining least-privilege test — a TeamMember gets exactly one row, themselves — sufficient, with columns now finalized, Clarifications 2026-07-22).
- **7Cs** — Clear ✔ (states the self-only rule outright); Concise ✔; Concrete ✔ (throughput = tasks closed in window; workload = currently assigned; overdue = assigned & overdue); Correct ✔ (overdue reuses 003's rule, matching 005); Coherent ✔ (the member pool is exactly 004's, and scope matches the other reports); Complete ✔ (all three roles incl. the self-only boundary, and the three metrics covered); Courteous n/a.
- **Given/When/Then**
  1. **Given** members across projects, **When** an **Admin** runs it for a project, **Then** **200** with one row per member (throughput, workload, overdue) over the window.
  2. **Given** a **ProjectManager**, **When** they run it, **Then** rows cover **only members of projects they own** — never a member of a project they don't own.
  3. **Given** a **TeamMember**, **When** they run it, **Then** the report returns **exactly one row — themselves** (personal productivity), regardless of any `userId` parameter they supply; requesting another user's row is ignored/denied (**self-only least privilege**).
  4. **Given** an Admin/ProjectManager `userId` filter, **When** applied, **Then** the report narrows to that member if within scope, else **403** (named out-of-scope user).
  5. **Given** generation, **Then** a `ReportGenerated` audit entry is written.
- **Edge cases**: a member with **no** activity in the window → a row of zeros (they still appear, so absence is visible); a deactivated member who had throughput in the window → still shown (historical), flagged inactive; a TeamMember passing `userId` of a colleague → **their own row is returned regardless** (not a 403 that would confirm the colleague exists — the safest least-privilege response); "throughput" = tasks whose `closed_at` falls in the window (finalized field tables, Clarifications 2026-07-22).
- **Audit/security**: generation audited; the **self-only** rule is enforced in the slice handler, not the UI; a TeamMember can never widen to a peer via any parameter.
- **Configurability**: default window; whether inactive members appear. (Metric columns are fixed by the field tables — Clarifications 2026-07-22.)

**C. UI** — **F006-S04 Team Performance** (report view): window + project scope (Admin/PM) or a fixed self-view (TeamMember); a Chart.js bar comparison (Admin/PM) or a single-member card (TeamMember); PDF/CSV export.

**D. API** — `GET /api/reports/team-performance?from=&to=&projectScope=&userId=` · `[Authorize]` (all three roles) · **200** typed report DTO (a TeamMember always gets a single-row DTO) · **400** · **403** (named out-of-scope user, for Admin/PM) · **401**.

**E. DB** — **reads only** — `tasks` (003, by assignee) + `team_members` (004) within scope; **writes one `activity_logs`** (`ReportGenerated`). No migration.

**F. Separation** — UI: comparison chart vs single-member card by role. Backend: `GetTeamPerformanceQueryHandler` (per-user scoped aggregate; **forces self-row for TeamMember**), audits generation; the controller only `Send`s the query. DB: scoped aggregate + one audit write. QA: **TeamMember-single-self-row (defining test)**, PM-only-own-members, throughput/workload/overdue correctness.

---

### US-006-05 — Activity Report

**A. Summary**
- **Story ID**: US-006-05 · **Title**: Generate a filtered, paginated activity-log excerpt over a window
- **Actor**: Admin (all) · ProjectManager (owned-project activity) · TeamMember (entities they can see)
- **User story**: *As an auditor or manager, I want a chronological activity excerpt over a window, filtered by project, entity type, or actor, so that I can review what changed and by whom.*
- **Business value**: The audit-facing report; turns 001's `activity_logs` into a reviewable, exportable record.
- **Priority**: **P1** · **Reason**: High value for compliance; the one report that can be large.
- **Dependencies**: 001 (`IActivityLogService`), 002/004 (scope). **Out of scope**: marking entries read (no domain writes); export mechanics (US-006-06).

**B. Quality validation**
- **INVEST** — Independent ✔; Negotiable ✔ (default page size; the threshold was the negotiated decision — ~10,000 rows, OQ-006-02, Clarifications 2026-07-22); Valuable ✔; Estimable ✔; Small ✔ (a scoped, paginated read + filters); Testable ✔ (scope, filters, and paging directly assertable).
- **3Cs** — Card ✔ (stands alone: "filtered activity excerpt over a window"); Conversation ✔ (surfaced the **large-window threshold + fallback** (OQ-006-02), reading **through `IActivityLogService`**, and how a deleted-entity's surviving audit rows are scoped; see Edge cases); Confirmation ✔ (the Given/When/Then scenarios cover scope, the three filters, pagination, the too-wide-window guard, and service-sourcing — sufficient now that the threshold value is settled at ~10,000 rows, Clarifications 2026-07-22).
- **7Cs** — Clear ✔ (states the feed is read through the service, never the table); Concise ✔; Concrete ✔ (exact `PagedResult<T>`, exact 200/400/422 outcomes); Correct ✔ (matches 005's activity-scoping rule); Coherent ✔ (activity scope equals the caller's visible entities across the app); Complete ✔ (scope, filters, paging, and the large-window guard covered); Courteous n/a.
- **Given/When/Then**
  1. **Given** audit entries over a window, **When** an **Admin** runs the report, **Then** **200** with a `PagedResult<ActivityReportRow>` (newest first) across all entries in the window.
  2. **Given** a **ProjectManager** or **TeamMember**, **When** they run it, **Then** entries are limited to **entities in their visible scope** — never another team's activity.
  3. **Given** `entityType`, `actorId`, or `projectId` filters, **When** applied, **Then** results narrow **within** the caller's scope (a filter can only narrow, never widen); a named out-of-scope `projectId`/`actorId` → **403**.
  4. **Given** `?page`/`?pageSize`, **When** requested, **Then** a scoped `PagedResult<T>` is returned; `pageSize` above the max is **clamped**; bad paging → **400**.
  5. **Given** a window whose result set would exceed the configured **large-report threshold**, **When** requested for export, **Then** **422** with a "narrow the date range" message (forced narrowing — see T.4 / OQ-006-02).
  6. **Given** the implementation, **When** the excerpt is built, **Then** it is sourced through **`IActivityLogService`** (001), never a direct `activity_logs` query.
- **Edge cases**: activity for a project the caller can no longer see (deleted, or no longer owned) is **scoped out for non-Admins**, visible to Admin only (matching 005); an empty window → **200** empty page; the **threshold is ~10,000 rows and the fallback is forced narrowing (422)** — Clarifications 2026-07-22 (no server-side streaming in v1); window bounds are evaluated in UTC (Clarifications 2026-07-22).
- **Audit/security**: **generation is audited** (`ReportGenerated`) — note the mild reflexivity: running the Activity Report itself creates an audit entry, which will appear in future Activity Reports (acceptable and correct); the excerpt reads through the service; entries carry no secrets (001 guarantees this at write time).
- **Configurability**: default/max page size; the large-report threshold (default **~10,000 rows**, configurable) with a fixed **forced-narrowing** fallback (Clarifications 2026-07-22); default ordering (newest first).

**C. UI** — **F006-S05 Activity Report** (report view): window + entity-type/actor/project filters; a paginated table over the `PagedResult<T>`; a "narrow your range" prompt on **422**; PDF/CSV export of the (bounded) result.

**D. API** — `GET /api/reports/activity?from=&to=&projectId=&entityType=&actorId=&page=&pageSize=` · `[Authorize]` (all three roles) · **200** `PagedResult<ActivityReportRow>` · **400** (bad paging) · **403** (named out-of-scope) · **422** (window too wide for export) · **401**.

**E. DB** — **reads only**, through `IActivityLogService` (001), scoped to visible entities; **writes one `activity_logs`** (`ReportGenerated`). No migration.

**F. Separation** — UI: filter form + paginated table + narrow-range prompt. Backend: `GetActivityReportQueryHandler` calls the 001 audit-log **read** (`IActivityLogService`) with the caller's scope + filters + paging, audits generation; the controller only `Send`s the query. DB: scoped paginated audit read + one audit write. QA: per-role scope, filters-cannot-widen, clamp, service-not-direct-table, too-wide-window 422.

---

### US-006-06 — Export a report to PDF or CSV (and audit the generation)

**A. Summary**
- **Story ID**: US-006-06 · **Title**: Download any report as PDF or CSV, with generation audited
- **Actor**: Admin · ProjectManager · TeamMember (each over their scoped report)
- **User story**: *As any authenticated user, I want to download the report I'm viewing as a PDF or CSV, so that I can file, email, or present it — and the organization can see that I generated it.*
- **Business value**: The brief's core Reports deliverable (VII.8) and the audit trail of report access, in one story.
- **Priority**: **P0** · **Reason**: Export *is* the Reports module's defining capability.
- **Dependencies**: US-006-02..05 (the report data); 001 (`IActivityLogService` for the audit write); Constitution III (jsPDF, CSV utility) / VII.8 (report-export service).
- **Out of scope**: server-side PDF rendering; persisting the generated file (OQ-006-03).

**B. Quality validation**
- **INVEST** — Independent ✔ (export is a distinct capability over any report); Negotiable ✔ (server-vs-client CSV was the decision — resolved client-side, T.4); Valuable ✔; Estimable ✔; Small ✔ (a client transform + one audit write); Testable ✔ (a PDF/CSV is produced from the same JSON the preview used; an audit row is written).
- **3Cs** — Card ✔ (stands alone: "download this report as PDF/CSV"); Conversation ✔ (surfaced the **client-side jsPDF/papaparse** decision, the API-returns-JSON-only stance, and the **audit-on-generation exception**; see T.4 and Audit); Confirmation ✔ (the Given/When/Then scenarios cover PDF, CSV, the same-data guarantee, and the single audit entry — sufficient to call this story done).
- **7Cs** — Clear ✔ (states PDF/CSV are client-side representations of the same JSON, not separate endpoints); Concise ✔; Concrete ✔ (jsPDF for PDF, papaparse for CSV, one `ReportGenerated` entry per generation); Correct ✔ (matches Constitution III library lock and VII.8's report-export-service rule); Coherent ✔ (export logic lives in one `ReportExportService`, not per report component — VII.8); Complete ✔ (both formats, the same-data guarantee, and the audit write covered); Courteous ✔ (the download is named meaningfully, e.g. `project-progress_2026-07.pdf`, and a busy/disabled state prevents double-clicks).
- **Given/When/Then**
  1. **Given** a report previewed on-screen (JSON), **When** the user chooses **PDF**, **Then** the **client-side** `ReportExportService` renders the PDF via **jsPDF** and the browser downloads it — no server round-trip for the format.
  2. **Given** the same report, **When** the user chooses **CSV**, **Then** the `ReportExportService` serializes the same data to CSV via the **papaparse-based utility** (client-side) and downloads it.
  3. **Given** either export, **When** compared to the on-screen preview, **Then** the PDF/CSV represents **exactly the same underlying data** (same rows, same scope) — format is a representation, not a different query.
  4. **Given** a report is generated (its data endpoint returns successfully), **When** the response is produced, **Then** **exactly one** `ReportGenerated` `activity_logs` entry is written (actor, report type, parameters, timestamp) via `IActivityLogService` — the sole write this feature performs.
  5. **Given** the Activity Report over a too-wide window, **When** export is attempted, **Then** the **422** guard (US-006-05) blocks it with a narrow-range prompt before any large client-side render.
- **Edge cases**: a report with an empty body → a valid but empty PDF/CSV (headers only); a very large (but under-threshold) client render → a busy indicator, no UI freeze (bounded by the threshold); the chosen format (json/pdf/csv) is a **client** action — the server audits the **generation** (data request), not each format render, so re-exporting the same previewed data to both PDF and CSV does **not** double-audit; download filename includes report type + window.
- **Audit/security**: **the one exception** — report generation writes a single `ReportGenerated` audit entry through `IActivityLogService`; it targets a logical `Report` (no domain entity), carries the parameters, and is the only write. No domain data is modified. The audit records *that the data was generated*; the client-side format choice is not separately server-audited.
- **Configurability**: enabled formats per report (default json+pdf+csv); the download filename pattern; whether generation is audited on every data request or only on explicit export (default: on generation — see B.7 / OQ).

**C. UI** — export controls on every report view (F006-S02..S05), backed by the **shared `ReportExportService`** (jsPDF + papaparse) — **not** duplicated per component (Constitution VII.8). A busy state during render; a meaningful filename.

**D. API** — **no dedicated export endpoint** — PDF/CSV are produced client-side from the report JSON (see T.4). The audit write happens **inside each report data endpoint** (US-006-02..05) on successful generation. The Activity export is gated by that report's **422** threshold.

**E. DB** — **writes one `activity_logs`** row per report generation (`ReportGenerated`); no other write, no migration.

**F. Separation** — UI: export buttons → `ReportExportService`. Backend: each report query handler emits the `ReportGenerated` audit via `IActivityLogService` on success. DB: one audit write per generation. QA: PDF via jsPDF, CSV via papaparse, same-data guarantee, exactly-one-audit-per-generation, no-double-audit-on-reexport.

---

## Consolidated Read Model (no new domain table, no migration)

> This feature adds **no** domain entity, domain table, or migration against domain tables (Constitution I.1 — it implements the brief's Reports module by aggregation + export, not new persistence). It **reads** the entities below and projects them into **transient report DTOs** (never persisted). There is **no `xmin`** (no domain mutation). The **only** persisted output is one **audit-only** `activity_logs` row per generation (`ReportGenerated`) — written through 001's service, targeting a logical `Report`, not a domain entity.

| Source entity | Owner | Read for |
|---|---|---|
| `projects` | 002 | Project Progress rows; the visible-project scope (via `ApplyScope`) |
| `tasks` | 003 | Completion %, open/closed/overdue, throughput; **`closed_at` completion timestamp** (agreed 003 follow-up — see Assumptions/Dependencies) |
| `team_members` | 004 | The member pool for Team Performance; TeamMember visible-project scope |
| `users` | 001 | Actor/member/assignee display names |
| `activity_logs` | 001 | The Activity Report — **read only through `IActivityLogService`** — **and** the destination of the one `ReportGenerated` write |

**Transient report DTOs (not persisted):** `ReportDescriptor` (catalog), `ProjectProgressReport`, `TaskCompletionReport`, `TeamPerformanceReport`, `ActivityReport` (a `PagedResult<ActivityReportRow>`). Each is a **stable typed contract** (not a free-form dictionary).

**Report DTO field tables (finalized — Clarifications 2026-07-22, OQ-006-04).** Every report DTO is an **envelope** + a typed row/bucket collection. All four share the envelope header `{ reportType, generatedAt, scope, window{from,to}, timeZone:"UTC" }`.

> **Closed / completion counting — uniform across every report.** A task is **"closed" iff `status = Done`** (equivalently `closed_at` is not null — 003's invariant). A **re-opened** task (`closed_at` cleared, status ≠ `Done`) is **not** counted as closed and is **excluded** from Task Completion's buckets — the **identical** rule applied to Project Progress's `closedTasks` count. **Overdue** = `due_date < today (UTC)` and `status ≠ Done` (003's rule, equal to 005's value for the same caller). **Throughput** (Team Performance, projected completion) = tasks whose `closed_at` falls within the window.

**`ProjectProgressReport`** — envelope + `rows[]` (one per in-scope project) + `totals`:

| `rows[]` field | Type | Meaning |
|---|---|---|
| `projectId` · `projectName` | uuid · string | Project identity |
| `status` | `ProjectStatus` (002) | Current project status |
| `totalTasks` · `openTasks` · `closedTasks` | int | In-scope task counts; `closed = status Done`, `open = not Done` |
| `overdueTasks` | int | Open & `due_date < today (UTC)` — equals 005's overdue for this caller |
| `completionPercent` | decimal | `closedTasks ÷ totalTasks × 100`; **`0` when `totalTasks = 0`** (no divide-by-zero) |
| `projectedCompletion` | date? | Linear extrapolation (below); **`null`** when no throughput or no open tasks |

`totals`: `{ projects:int, avgCompletionPercent:decimal }`. **Projected-completion formula (directional):** `avgClosedPerDay = closedTasksInWindow ÷ windowDays`; `projectedCompletion = today(UTC) + ceil(openTasks ÷ avgClosedPerDay)`; **`null`** when `avgClosedPerDay = 0` (no recent throughput) or `openTasks = 0` (already complete).

**`TaskCompletionReport`** — envelope (+ `groupBy`) + `buckets[]` + `totals`:

| `buckets[]` field | Type | Meaning |
|---|---|---|
| `periodStart` | date | Bucket start (UTC), per `groupBy` = day/week/month |
| `periodLabel` | string | Human label (e.g. `2026-W30`) |
| `completedCount` | int | Tasks whose `closed_at` falls in the bucket, in scope (re-opened excluded) |

Series is **zero-filled continuous** (every period in the window present). `totals`: `{ completed:int }`.

**`TeamPerformanceReport`** — envelope + `rows[]` (one per member; **exactly one — self — for a TeamMember**):

| `rows[]` field | Type | Meaning |
|---|---|---|
| `userId` · `fullName` | uuid · string | Member identity |
| `isActive` | bool | 001 `is_active` (inactive members with in-window throughput still shown, flagged) |
| `throughput` | int | Tasks whose `closed_at` is in the window, assigned to this member, in scope |
| `workload` | int | Tasks currently assigned to them and **not** `Done` |
| `overdueCount` | int | Assigned to them, open, `due_date < today (UTC)` |

**`ActivityReport`** — a `PagedResult<ActivityReportRow>` (envelope + `page/pageSize/totalCount/totalPages`), each `ActivityReportRow`:

| Field | Type | Meaning |
|---|---|---|
| `id` | uuid | Audit entry id |
| `timestamp` | timestamptz | When (UTC) |
| `actorId` · `actorName` | uuid? · string | Who (null actor = system/seed) |
| `action` | string | e.g. `TaskStatusChanged`, `ProjectCreated`, `ReportGenerated` |
| `entityType` · `entityId` | string · string | Target entity |
| `changeSummary` | string | Human-readable before→after (001; no secrets) |

**Explicitly *not* created (out of scope; model must not preclude):** a `ReportArtifact` table (persisted generated files — OQ-006-03) or a `ReportSchedule` table (scheduled/emailed reports — OQ-006-01). The design leaves room for both without needing them now.

## Consolidated API Catalog

> Base path `/api`; JSON over HTTPS; errors as **RFC 7807 Problem Details** via the shared `ErrorKind` mapper. Per Constitution **X.2 (API-first)**, the OpenAPI contract for these routes is authored and reviewed under `/docs/contracts/` **before** the handlers are implemented, and the code is validated against it; **Swagger UI** is enabled in development for local exploration only. Authenticated by default (001). **All endpoints are `GET`, returning JSON** — PDF/CSV are client-side representations (T.4); there is **no write endpoint**. Resource-oriented under `/api/reports` (Constitution VI.6).

| Method · Route | Purpose | Role gate | Access gate (in handler) | Success | Failure |
|---|---|---|---|---|---|
| `GET /api/reports/catalog` | Report types + their parameters | `[Authorize]` (all 3) | role-annotate | **200** descriptor array | 401 |
| `GET /api/reports/project-progress` | Per-project progress over a window | `[Authorize]` (all 3) | `ApplyScope` | **200** typed report | 400, 401, 403 |
| `GET /api/reports/task-completion` | Completion trend, grouped | `[Authorize]` (all 3) | `ApplyScope` | **200** typed report | 400, 401, 403 |
| `GET /api/reports/team-performance` | Per-member throughput/workload/overdue | `[Authorize]` (all 3) | `ApplyScope` + TeamMember self-only | **200** typed report | 400, 401, 403 |
| `GET /api/reports/activity` | Filtered, paginated activity excerpt | `[Authorize]` (all 3) | `ApplyScope` | **200** `PagedResult<T>` | 400, 401, 403, **422** (window too wide) |

*Every data endpoint (not the catalog) writes one `ReportGenerated` audit entry on success.*

---

## Technical Design — Parameterized Role-Scoped Reporting & Client-Side Export

> The read-only-plus-one-audit-write posture, the endpoint-shape and export-architecture decisions (resolved here, not deferred), how scope folds into parameterized queries, the flows, failure handling, and security guarantees.

### T.1 Posture — read-only over domain data, one audit-only write
- **The .NET API is the authority**; the **slice handler** owns aggregation, scope, and the audit write. The **thin controller** does nothing but `MediatR.Send(...)` (Constitution II.2).
- **No domain writes.** No create/update/delete of Users/Projects/Tasks/TeamMembers. So `CanMutateAsync` is **not** implemented and `xmin` (ADR-0004) is **not applicable**.
- **One audit-only write — the deliberate exception.** Each report **generation** writes a single `ReportGenerated` `activity_logs` entry via `IActivityLogService`. Constitution IV.4 audits *domain writes*; report generation is not a domain write but **is** a security-relevant access event, so it is audited by exception. This is the one place a read-side feature legitimately writes to the audit log (005 Dashboard writes nothing; see B.7).
- **The Angular frontend is convenience** for viewing; it also performs the **export render** (jsPDF/papaparse) — but never a security decision. The API re-checks scope on every request.

### T.2 Role-scoped, parameterized aggregation (the heart)
Every report reuses the existing `ApplyScope` predicates (002/003/004) — Reports defines no new scope logic — and layers **parameters** on top:
- **Visible set** = `IProjectAccessPolicy.ApplyScope` (Admin all · PM owned · TeamMember member-of).
- **Parameters narrow, never widen.** `from`/`to`, `projectScope`, `groupBy`, `assigneeId`, `entityType`, `actorId` filter *within* the visible set.
- **Named out-of-scope → 403** (the key difference from Dashboard). Because a report can *name* a `projectId`/`userId`, requesting one outside scope returns **403** (maskable to 404 by config), inheriting 002/003/004's convention. Dashboard, being parameter-free, never 403s; Reports does. A `projectScope=all` request is silently narrowed to the visible set (no 403).
- **TeamMember self-only for Team Performance** — enforced in the slice handler: the per-user aggregate is forced to `assignee_id == caller`, ignoring/denying any other `userId`.
- **Value parity with Dashboard (hard requirement).** Where a metric appears in both surfaces (overdue count, open/closed counts), Reports reuses the **same scope predicate and the same metric definition** (003's overdue rule, `Done` terminal), so for a given caller and window the numbers **match** what 005 shows. (NFR-002.)
- **Filter at the source.** Aggregates translate the scope + parameters to SQL; nothing is fetched then filtered.

### T.3 The endpoints, with concrete examples

**(1) Catalog**
```
GET /api/reports/catalog     Authorization: Bearer eyJ…

→ 200 OK
[ { "type":"ProjectProgress", "title":"Project Progress",
    "parameters":[ {"name":"from","type":"date","required":true},
                   {"name":"to","type":"date","required":true},
                   {"name":"projectScope","type":"projectIds|all","required":false} ],
    "formats":["json","pdf","csv"] },
  { "type":"TaskCompletion", "title":"Task Completion",
    "parameters":[ …, {"name":"groupBy","type":"day|week|month","required":true},
                   {"name":"assigneeId","type":"userId","required":false} ], "formats":[…] },
  { "type":"TeamPerformance", "title":"Team Performance", "parameters":[…], "formats":[…] },
  { "type":"Activity", "title":"Activity", "parameters":[…,{"name":"entityType"},{"name":"actorId"}], "formats":[…] } ]
```

**(2) Project Progress (JSON preview)**
```
GET /api/reports/project-progress?from=2026-07-01&to=2026-07-31&projectScope=all
Authorization: Bearer eyJ…   (role=ProjectManager)

→ 200 OK
{ "reportType":"ProjectProgress", "generatedAt":"2026-07-22T11:30:00Z", "scope":"ProjectManager",
  "window":{ "from":"2026-07-01", "to":"2026-07-31" },
  "rows":[ { "projectId":"4d9b…c3", "projectName":"Apollo Rollout",
             "completionPercent":62.5, "openTasks":6, "closedTasks":10, "overdueTasks":2,
             "projectedCompletion":"2026-09-20" } ],
  "totals":{ "projects":3, "avgCompletionPercent":58.0 } }
```
`overdueTasks` here equals the overdue count 005 Dashboard shows this same caller. A named out-of-scope `projectScope=<id you don't own>` → **403**.

**(3) Activity (scoped + paginated)**
```
GET /api/reports/activity?from=2026-07-01&to=2026-07-31&entityType=Task&page=1&pageSize=50
Authorization: Bearer eyJ…   (role=TeamMember)

→ 200 OK   { "items":[ { "id":"…","actorName":"Priya Nair","action":"TaskStatusChanged",
                          "entityType":"Task","entityId":"9ac4…","timestamp":"…",
                          "changeSummary":"InProgress → InReview" } ],
             "page":1,"pageSize":50,"totalCount":1,"totalPages":1 }
→ 422 Unprocessable  { "title":"Report window too large",
                       "detail":"Narrow the date range; this window exceeds the export row limit." }
```

### T.4 Two decisions resolved here (not deferred)

**(a) Endpoint shape → resource-per-report-type** (not a single POST-with-discriminator).
Each report has a **distinct typed parameter contract** (Project Progress takes a project scope; Task Completion adds `groupBy`; Team Performance adds `userId`; Activity adds `entityType`/`actorId`/paging). A single `POST /api/reports {type, params}` would force those into one **polymorphic, weakly-typed body** — the opposite of the "stable typed contract, not a dictionary" principle 005 established, and worse for Swagger. So each report is its own **`GET /api/reports/{report-type}`** with a typed query, matching 002/003/004's resource orientation (VI.6). A **`GET /api/reports/catalog`** describes them for the dynamic UI. *(The user offered to defer this; it is decided here because the typed-contract argument is decisive.)*

**(b) Export architecture → client-side PDF (jsPDF) and client-side CSV (papaparse); the API returns JSON only.**
- **PDF is client-side by mandate.** Constitution III locks **jsPDF**, a browser library — so the default (and only) PDF path is: the API returns JSON, the Angular **`ReportExportService`** renders the PDF via jsPDF, the browser downloads it. This avoids a server-side PDF engine/dependency, keeps the API stateless, and honors III's library lock.
- **CSV is also client-side** (papaparse per III), **decided** for consistency: both PDF and CSV are then representations produced by the **one** `ReportExportService` (VII.8 — export logic in a service, not per component) from the **single JSON** the preview already fetched. The trade-off vs server-side CSV (bandwidth, server formatting control) is real but not worth a second code path at this app's scale; the JSON contract stays the single source. *(A future `?format=csv` server representation can be added without changing the JSON contract if ever needed — designed-for, not built.)*
- **Consequence:** `format` (json/pdf/csv) is a **client** concern; there is **no `?format` behavior on the API** and **no per-format endpoint**. "PDF and CSV are alternate representations of the same data, not separate endpoints" (brief) is honored by rendering both from the one JSON.
- **Large-report fallback (Activity).** A wide Activity window can be tens of thousands of rows — too many to render client-side. So each report data request is bounded by a **configurable large-report threshold (default ~10,000 rows — Clarifications 2026-07-22)**; a request/export that would exceed it returns **422** with a "narrow the date range" prompt (**forced narrowing** — the entire export pipeline stays client-side, no server-side CSV streaming in v1). Server-side streaming remains a possible later addition behind the same threshold config without changing the JSON contract.

### T.5 How a report is computed (step by step)
1. JWT validated; `ICurrentUserService` materializes `CurrentUser(UserId, Email, Role)` — never from the body.
2. The **thin controller** `Send`s the **typed query**; a **MediatR validation behaviour** runs its FluentValidation validator (window present, `from ≤ to`, valid `groupBy`, paging bounds) — else **400** (ADR-0005) — then the **slice handler** executes.
3. The handler resolves the **visible set** via `ApplyScope`; a **named** `projectId`/`userId` outside it → **403**; `projectScope=all` narrows to visible.
4. The scoped, parameterized aggregate runs **in SQL** (grouped counts / per-period / per-user), or, for Activity, a scoped paginated read **through `IActivityLogService`** — with the **threshold guard** (→ **422** if exceeded for export).
5. The typed report DTO is assembled (metric definitions identical to 005 for shared metrics).
6. `IActivityLogService.LogAsync` writes **one** `ReportGenerated` entry (actor, report type, serialized parameters, timestamp).
7. The JSON is returned; the client renders the chosen representation (jsPDF/papaparse) with no further server call.

### T.6 API behaviour rules
- **All `GET`, JSON only** — PDF/CSV are client-side; no write endpoint (the audit write is an internal side effect of a successful `GET`).
- **Status codes** (Constitution VI.2): 200 read; 400 bad params/paging; 401 unauthenticated; **403 named out-of-scope** resource (unlike Dashboard); 404 truly unknown id (maskable target of the 403 convention); **422 window-too-wide** for Activity export; 500 server. No 409/`xmin`. All errors are Problem Details (ADR-0003).
- **Pagination** (Constitution VI.4): the **Activity Report** returns `PagedResult<T>` (default 20, max 100, clamped). The **catalog** is a small fixed set → **plain array**. **Project Progress / Task Completion / Team Performance** return **bounded** DTOs (one row per visible project / per period / per member) → returned in full; the on-screen table paginates **client-side** for readability. *(Per-section reasoning, as requested.)*
- **Real-time-ready** — reports are generated on demand; a future SignalR push (II.4) is not precluded.
- **Versionable** — `/api/v1` prefix can be added later without breaking clients (VI.1); not adopted now.

### T.7 Failure handling (fail-safe)
- **Unknown/expired token → 401.**
- **Bad parameters (missing window, `from > to`, invalid `groupBy`, bad paging) → 400** with field errors.
- **Named out-of-scope project/user → 403** (maskable to 404); `projectScope=all` never 403s (auto-narrowed).
- **Empty visible scope → 200** with empty rows/buckets — never 403/404.
- **Activity window too wide → 422** with a narrow-range prompt (before any large render).
- **`Done`-timestamp gap** (if 003 lacks a completion timestamp) is handled per Assumptions, not a runtime failure.
- Uncaught errors → **500** Problem Details; the Angular `ErrorInterceptor` + global `ErrorHandler` surface them (Constitution VII.7).

### T.8 Security guarantees
- Every endpoint requires a valid JWT; role gate is **attribute-only** (V.1/V.2).
- **Scope enforced in the query source**; out-of-scope projects/tasks/members/activity are never loaded or counted; nothing leaks through a total.
- **Named out-of-scope resource → 403**, so a report cannot be used to read another team's data by naming its id.
- **TeamMember self-only** for Team Performance is enforced server-side; no parameter widens a TeamMember to a peer.
- **Value parity with Dashboard** prevents a "which number is right?" trust gap.
- The Activity excerpt is read **through `IActivityLogService`** (001's no-secrets guarantee).
- **Every generation is audited** (`ReportGenerated`) — the sole write, touching no domain entity.
- All data access via EF Core; reporting-grade aggregates are LINQ; any future raw SQL would follow Constitution IV.1's parameterized-and-reviewed exception (none needed for v1).

---

## Implementation Blueprint (build-ready detail)

> The (absent) domain schema, reused enums + the few report enums, service interfaces, configuration, error model, NFRs, the one-entry audit catalog, and the Definition of Done.

### B.1 Schema — none (domain); one audit write
> **No domain table and no migration against domain tables.** Reports reads `projects`/`tasks`/`team_members`/`users`/`activity_logs` (001–004) and returns transient DTOs. The **only** write is one `activity_logs` row per generation (`ReportGenerated`), via 001's service. **No `ReportArtifact`/`ReportSchedule` tables** (out of scope; see OQ-006-01/03). No `xmin`. New indexes are unnecessary — aggregates reuse those 002/003/004 declare (`projects(owner_id,status)`, `tasks(project_id,status)`, `tasks(assignee_id,status)`, `due_date` filter, `team_members(project_id,user_id)`), plus whatever `activity_logs` index 001 provides for the scoped audit read.

### B.2 Enumerations
- **Reused:** `ProjectStatus` (002), `TaskStatus` (003 — `Done` is the completion state for throughput/overdue).
- **New (report-local):** `ReportType` (`ProjectProgress, TaskCompletion, TeamPerformance, Activity`); `ReportFormat` (`Json, Pdf, Csv` — client render targets, not API params); `GroupingGranularity` (`Day, Week, Month`).
- **AuditAction (new):** `ReportGenerated` — the one audit action this feature emits.

### B.3 Vertical slices, handlers & shared abstractions (C#; nullable reference types on)

Per Constitution **II.2**, each report use-case is a self-contained **vertical slice** under
`Features/Reports/<UseCase>/`, holding its Query, its FluentValidation validator, and its handler.
Reports is read-only over domain data — no commands mutate a domain entity (T.1) — but each report
handler (except the catalog) writes the **one** `ReportGenerated` audit entry via `IActivityLogService`.
Controllers are **thin**: one endpoint maps one HTTP verb to a single `MediatR.Send(...)`. Per **IV.1**,
each query handler reads via the EF Core `DbContext` **directly** (grouped aggregates), reusing the shared
scope abstractions below — **no Repository is introduced**.

```text
Features/Reports/GetCatalog/          GET /api/reports/catalog          [Authorize] (all 3)
  GetReportCatalogQuery() : IRequest<Result<IReadOnlyList<ReportDescriptor>>>
  GetReportCatalogQueryHandler        // role-annotated descriptors; static/config-derived; NOT audited (metadata only)
  → Response: IReadOnlyList<ReportDescriptor>

Features/Reports/GetProjectProgress/  GET /api/reports/project-progress [Authorize] (all 3)
  GetProjectProgressQuery(From, To, ProjectScope?) : IRequest<Result<ProjectProgressReport>>
  GetProjectProgressQueryValidator    // window present, from <= to
  GetProjectProgressQueryHandler      // ApplyScope grouped aggregate; named-out-of-scope → 403; emits ReportGenerated
  → Response: ProjectProgressReport

Features/Reports/GetTaskCompletion/   GET /api/reports/task-completion  [Authorize] (all 3)
  GetTaskCompletionQuery(From, To, GroupBy, ProjectScope?, AssigneeId?) : IRequest<Result<TaskCompletionReport>>
  GetTaskCompletionQueryValidator     // valid groupBy, from <= to
  GetTaskCompletionQueryHandler       // scoped grouped-by-period aggregate; TeamMember constrained to self; emits ReportGenerated
  → Response: TaskCompletionReport

Features/Reports/GetTeamPerformance/  GET /api/reports/team-performance [Authorize] (all 3)
  GetTeamPerformanceQuery(From, To, ProjectScope?, UserId?) : IRequest<Result<TeamPerformanceReport>>
  GetTeamPerformanceQueryHandler      // per-user scoped aggregate; FORCES self-row for TeamMember; emits ReportGenerated
  → Response: TeamPerformanceReport

Features/Reports/GetActivity/         GET /api/reports/activity         [Authorize] (all 3)
  GetActivityReportQuery(From, To, ProjectId?, EntityType?, ActorId?, Page, PageSize) : IRequest<Result<PagedResult<ActivityReportRow>>>
  GetActivityReportQueryValidator     // paging bounds
  GetActivityReportQueryHandler       // scoped paginated read THROUGH IActivityLogService; 422 threshold guard; emits ReportGenerated
  → Response: PagedResult<ActivityReportRow>
```

**Shared cross-cutting abstractions the handlers depend on** (reused, **not** redefined; these are **shared-kernel** abstractions declared in [docs/shared-contracts.md §3](../../docs/shared-contracts.md) (`IProjectAccessPolicy`/`ITaskAccessPolicy`) and §6 (`IActivityLogService`), so the handler depends on shared-kernel contracts — it does **not** call 001's/002's/003's/004's slice handlers):
```text
//   IProjectAccessPolicy.ApplyScope (002), ITaskAccessPolicy.ApplyScope (003) — role scope
//   IActivityLogService (001): (a) SCOPED READ for the Activity Report; (b) LogAsync for the ONE
//     ReportGenerated audit write per generation. Both through the 001-owned service — never a direct table query.
```

**Frontend only (Constitution VII.8) — the single export service, NOT per-component** (unchanged by this revision; may now also be generated from the OpenAPI contract per VII.3):
```text
//   ReportExportService.toPdf(report)  -> jsPDF render + download
//   ReportExportService.toCsv(report)  -> papaparse serialize + download
//   The API returns JSON; PDF/CSV are produced here, client-side.
```
```text
// Query records carry { from, to, projectScope?, groupBy?, assigneeId?, userId?, entityType?, actorId?, page?, pageSize? }
//   as each report requires; scope is derived from the caller, never trusted from the body.
// Result<T>, ErrorKind, CurrentUser, AccessDecision, PagedResult<T> — docs/shared-contracts.md (ADR-0003), reused.
// Every report handler (except catalog) emits exactly one ReportGenerated audit entry on success.
// Report DTOs are stable typed contracts (one row per project/period/member; enum-keyed where applicable) — not dictionaries.
```

### B.4 Configuration (never hardcoded)
- `Reports:DefaultWindowDays` (e.g. 30) · `Reports:MaxWindowDays` (guard on absurd ranges)
- `Reports:Activity:{DefaultPageSize,MaxPageSize}` (20 / 100, clamped)
- `Reports:LargeReportRowThreshold` (default **10,000**, configurable) + `Reports:LargeReportFallback` (**`ForceNarrow`** for v1 — Clarifications 2026-07-22; `ServerStream` reserved for a later iteration, no v1 code path)
- `Reports:TimeZone` — **fixed to UTC** for v1 (Clarifications 2026-07-22): `from`/`to` and all bucket boundaries are evaluated in UTC for cross-viewer reproducibility; the header states the applied zone. (Caller-local display is a possible later addition, not a v1 knob.)
- `Reports:EnabledTypes` — v1 exposes the **four** baseline types (Clarifications 2026-07-22); the flag gates only *future* additional types, not the fixed v1 set
- `Reports:AuditOnGeneration` (default `true`; if ever changed to audit only explicit exports, see B.7)
- `Reports:MaskOutOfScopeAs404` (default `false` → 403, matching 002/003/004)
- `Reports:DownloadFilenamePattern`

### B.5 Error model (RFC 7807 Problem Details)
Via the shared `ErrorKind` mapper (ADR-0003): `400` validation (window, `from>to`, `groupBy`, paging) · `401` auth · `403` named out-of-scope project/user (maskable to 404) · `422` Activity window exceeds the large-report threshold · `500`. **No `409`/`xmin`** (no domain mutation). Never leak an out-of-scope figure or entity in an error body.

### B.6 Non-functional requirements
- **Security:** deny-by-default; scope in the query; named out-of-scope → 403; TeamMember self-only; every generation audited.
- **Performance:** each report is a scoped grouped aggregate pushed to the database (no N+1, no fetch-then-filter); bounded reports returned in full; the Activity read is paginated + threshold-guarded. Values reuse 005's query shapes where shared.
- **Consistency (hard):** for the same caller/window, a metric shared with Dashboard (overdue, open/closed) **must** equal Dashboard's value — same `ApplyScope`, same 003 definitions. Covered by a cross-feature test.
- **Observability:** Serilog structured logging; report-generation timings recorded; the `ReportGenerated` audit is the security record.
- **Testability (Constitution IX):** each report query handler is unit-tested; three-role scope matrix per report; the **TeamMember self-only** Team-Performance test; the **named-out-of-scope 403** test; the **Dashboard-value-parity** test; the Activity **422 threshold** test; export produces PDF (jsPDF) and CSV (papaparse) from the same JSON; **exactly one audit entry per generation**. Frontend `ReportsService`, `ReportExportService`, guards, and catalog-driven forms via Jasmine+Karma.

### B.7 Audit event catalog — one entry, the deliberate exception
> Reports is read-only over domain data, so — like 005 — it writes **no** audit entry *for domain reads*. **But** report generation is a security-relevant action, so each successful report data request emits **exactly one** `ReportGenerated` entry via `IActivityLogService`: `(actor_id, action='ReportGenerated', entity_type='Report', entity_id=<generated run id>, timestamp, change_summary=<report type + serialized parameters>)`. It targets a **logical `Report`**, not a domain entity, and modifies no domain state. This is **the one place a read-side feature legitimately writes to the audit log** — explicitly by design (contrast 005, whose catalog is intentionally empty). The catalog endpoint is **not** audited (metadata only). Default is audit-on-generation; auditing only explicit exports is a config option (`Reports:AuditOnGeneration`) but not the default, since the security-relevant fact is that the data was produced.

### B.8 Definition of Done
1. `GET /api/reports/catalog` + the four report `GET`s exist and behave per the API-catalog status table; **no write endpoint**; PDF/CSV are client-side.
2. The three-role scope matrix holds for every report: Admin unscoped; ProjectManager owned-projects; TeamMember member-of — proven by integration tests, with **filter-at-source** verified.
3. **TeamMember Team Performance returns exactly one row (themselves)** regardless of any `userId` supplied — the defining least-privilege test.
4. A **named out-of-scope** `projectId`/`userId` returns **403** (maskable to 404); `projectScope=all` auto-narrows without 403.
5. **Dashboard value parity:** for the same caller/window, overdue and open/closed counts equal 005's — proven by a cross-feature test.
6. Task Completion buckets by day/week/month over the window (zero-filled continuous series); the completion-timestamp source is per Assumptions.
7. The Activity Report reads **through `IActivityLogService`**, is paginated (`PagedResult<T>`, clamped), scoped, and returns **422** with a narrow-range prompt when the window exceeds the threshold.
8. **Export**: PDF via **jsPDF** and CSV via **papaparse**, both from the one `ReportExportService` (not per component), representing the **same data** as the preview.
9. **Exactly one `ReportGenerated` audit entry** per report generation (asserted); re-exporting the same previewed data to PDF and CSV does **not** double-audit; the catalog is not audited; **no domain entity is written** and **no migration is added**.
10. The Angular `reports` route group is lazy-loaded with standalone components; HTTP in `ReportsService`; export in `ReportExportService`; charts use Chart.js; parameter forms are catalog-driven Reactive Forms; a functional role guard is the only navigation block.
11. Errors are RFC 7807 via the shared mapper; the OpenAPI contract for all endpoints is authored/reviewed under `/docs/contracts/` **before** the handlers and the code is validated against it (API-first, X.2), with Swagger UI in development; backend warnings-as-errors + nullable; frontend strict mode.
12. All open questions **OQ-006-01..06 are resolved** (Clarifications 2026-07-22) and covered by tests: report field tables + the closed/re-open counting rule (FR-017); the four-report v1 set (FR-018); UTC bucketing; the ~10,000-row threshold + forced-narrowing; scheduling/email out of scope; transient artifacts.
13. Unit + integration tests pass (Constitution IX.3 — no merge on failing tests).

### B.9 Open questions for review
| # | Question | Recommendation (not yet a decision) | Status |
|---|---|---|---|
| OQ-006-01 | Scheduled / emailed reports? | **Resolved (Clarifications 2026-07-22): out of scope for v1** (brief bonus — Slack/email); model left open to a later `ReportSchedule` concept, no schema committed now | **Resolved** |
| OQ-006-02 | Exact large-Activity **threshold value** and fallback — forced-narrowing (v1) vs server-side streaming? | **Resolved (Clarifications 2026-07-22): ~10,000 rows (configurable), forced narrowing (422)** — export stays fully client-side; server-side streaming deferred as a possible later addition behind the same config. | **Resolved** |
| OQ-006-03 | Persist generated PDFs/CSVs server-side (a "recent reports" list) or always transient? | **Resolved (Clarifications 2026-07-22): transient for v1** (re-generate on demand); persistence would be a separate `ReportArtifact` spec | **Resolved** |
| OQ-006-04 | The exact **columns/fields** in each report body | **Resolved (Clarifications 2026-07-22): field tables finalized** in the Consolidated Read Model, incl. the projected-completion formula and the uniform closed/re-open counting rule (`closed = Done`; re-opened excluded from both Project Progress `closedTasks` and Task Completion buckets) | **Resolved** |
| OQ-006-05 | **Timezone** for date-range params and displayed timestamps — server UTC vs caller-local? | **Resolved (Clarifications 2026-07-22): UTC**, fixed for v1 — reproducible across viewers; applied zone shown in the report header; caller-local is a possible later addition. | **Resolved** |
| OQ-006-06 | Additional **report types** beyond the baseline four? | **Resolved (Clarifications 2026-07-22): four only for v1** (brief names three; ActivityLogs' "auditing and reporting" purpose covers the fourth); more can be added later via the catalog without a contract change | **Resolved** |

*Decided here (not open):* endpoint shape (resource-per-type) and export architecture (client-side jsPDF + papaparse, JSON-only API) — see T.4.

---

## Functional Requirements

- **FR-001**: The system MUST expose `GET /api/reports/catalog` and one `GET` endpoint per report type (`project-progress`, `task-completion`, `team-performance`, `activity`); it MUST NOT expose any create/update/delete endpoint and MUST introduce no new domain entity, domain table, or migration against domain tables.
- **FR-002**: Each report MUST accept at minimum a **date window** (`from`/`to`) and a **project scope** (a project id, a list, or "all visible"); Task Completion MUST accept a **grouping** (`day`/`week`/`month`); Team Performance MUST accept an optional user filter; Activity MUST accept optional `entityType`/`actorId` filters and paging.
- **FR-003**: Every report MUST be **role-scoped at the query source** via the reused `ApplyScope` predicates — Admin all, ProjectManager owned projects, TeamMember member-of projects — and MUST NOT fetch-then-filter.
- **FR-004**: A **named** `projectId`/`userId` outside the caller's scope MUST return **403** (maskable to 404 by config); `projectScope=all` MUST be auto-narrowed to the caller's visible set without a 403.
- **FR-005**: The **Team Performance** report MUST return, for a **TeamMember**, **only their own row**, regardless of any supplied `userId` (least-privilege, enforced server-side).
- **FR-006**: Shared metrics (overdue count, open/closed counts) MUST resolve to the **same value 005 Dashboard shows the same caller** for the same window — same scope predicate and same 003 definitions.
- **FR-007**: The **Activity Report** MUST be read **through 001's `IActivityLogService`** (never a direct `activity_logs` query), MUST be scoped to the caller's visible entities, and MUST be paginated via `PagedResult<T>` (default 20, max 100, clamped) per Constitution VI.4.
- **FR-008**: A caller with an empty visible scope MUST receive **200** with an empty body (rows/buckets), never **403** or **404**.
- **FR-009**: The system MUST enforce a **configurable large-report threshold** (default **~10,000 rows**); a report/export exceeding it MUST return **422** with a narrow-range message (**forced narrowing**, export stays client-side — no server-side streaming in v1) rather than attempting an unbounded render.
- **FR-010**: PDF export MUST be produced **client-side via jsPDF** and CSV export **client-side via a papaparse-based utility**, both from the single JSON the preview uses, in **one shared report-export service** (Constitution III, VII.8) — not per component and not per-format endpoints.
- **FR-011**: Each successful report **generation** MUST write **exactly one** `ReportGenerated` `activity_logs` entry (actor, report type, parameters, timestamp) via `IActivityLogService`; this is the **sole write** the feature performs and MUST NOT modify any domain entity; the catalog endpoint MUST NOT be audited.
- **FR-012**: Reports MUST NOT modify any domain entity (Users/Projects/Tasks/TeamMembers); `CanMutateAsync` and `xmin` MUST NOT be used.
- **FR-013**: Errors MUST be RFC 7807 Problem Details via the shared mapper; the OpenAPI contract for all endpoints MUST be authored and reviewed under `/docs/contracts/` **before** the handlers are implemented (API-first, Constitution X.2), with the code validated against the contract and Swagger UI enabled in development; error statuses are limited to 400/401/403/404/422 (no 409).
- **FR-014**: The Angular `reports` feature area MUST be lazy-loaded via route-level code splitting with standalone components (ADR-0001); HTTP MUST live in `ReportsService`; export in `ReportExportService`; parameter forms MUST be **catalog-driven** Reactive Forms; charts MUST use Chart.js (Constitution III); a functional role guard MUST be the only navigation block.
- **FR-015**: All data access MUST go through EF Core (Constitution IV.1); aggregates are LINQ grouped queries with scope + parameters translated to SQL.
- **FR-016**: The architecture MUST NOT preclude adding scheduled/emailed reports, persisted report artifacts, or real-time push later (Constitution I.2 / II.4) — but none is built in v1. Scheduled/emailed reports (OQ-006-01) and persisted report artifacts (OQ-006-03) are **out of scope for v1** (Clarifications 2026-07-22).
- **FR-017**: Each report DTO's fields MUST match the **field tables in the Consolidated Read Model** (Clarifications 2026-07-22). A task MUST be counted **closed iff `status = Done`** (`closed_at` not null); a **re-opened** task (`closed_at` cleared) MUST be excluded from **both** Project Progress's `closedTasks` count **and** Task Completion's buckets — the same rule in both. "Projected completion" MUST be the linear throughput extrapolation defined there (`null` when throughput is 0 or there are no open tasks).
- **FR-018**: v1 MUST ship **exactly the four baseline reports** — Project Progress, Task Completion, Team Performance, Activity (Clarifications 2026-07-22); additional report types are added later via the catalog without a contract change.

## Non-Functional Requirements
- **NFR-001**: Server-side enforcement is authoritative; the frontend never gates for security (it only renders exports).
- **NFR-002**: Shared metrics are **value-identical to Dashboard** for the same caller/window (a cross-feature test guards this).
- **NFR-003**: Aggregates are single scoped grouped queries (no N+1, no fetch-then-filter); the Activity read is paginated + threshold-guarded.
- **NFR-004**: Nullable reference types + warnings-as-errors (backend); TypeScript strict mode (frontend).
- **NFR-005**: Structured logging (Serilog); report-generation timings recorded; `ReportGenerated` is the security audit record.

## Security Rules
- Authenticated by default; role gate via attributes only; scope enforced in the query source.
- Named out-of-scope project/user → 403; `projectScope=all` auto-narrowed.
- TeamMember Team Performance is self-only, enforced server-side.
- Activity read only through `IActivityLogService`; every generation audited; no domain write.

## Audit / Compliance Expectations
Reports performs **no domain-data audit** (it makes no domain write) — but it writes **exactly one `ReportGenerated` audit entry per generation** (actor, report type, parameters, timestamp) via `IActivityLogService`, targeting a logical `Report`. This is the deliberate exception where a read-side feature writes to the audit log, giving the organization a record of *who ran which report with what parameters, when*. The Activity the reports display was already audited by the features that made those changes. Append-only; no secrets; the report-generation entries themselves appear in future Activity Reports (correct and expected).

## Assumptions
- 001–005 are implemented: `users`, the role model, `ICurrentUserService`, `IActivityLogService` (with a **scoped read** method, as 005 also assumes, and `LogAsync` for the audit write), `IProjectAccessPolicy.ApplyScope` (002), `ITaskAccessPolicy.ApplyScope` (003), and `team_members` (004) all exist and are consumed here.
- **Task completion timestamp — resolved (Clarifications 2026-07-22):** accurate Task Completion trends and "closed" counts require knowing **when** a task completed. 003's Task will expose a dedicated **`closed_at`** timestamp (set when status → `Done`, cleared on re-open), added via a **lightweight additive migration to 003** — a small plan-time follow-up, **not a redesign**. Reports reads `closed_at` for its completion buckets and closed counts; a re-opened task (`closed_at` cleared) is excluded from completed counts until re-completed. This spec records the decision and does **not** modify 003 — the column is added when 003 is planned/built.
- **Timezone:** `from`/`to`, bucket boundaries, and displayed timestamps are evaluated in **UTC** (fixed for v1 — Clarifications 2026-07-22) so a report is reproducible across viewers; the applied zone is shown in the report header.
- **Transient artifacts:** generated PDFs/CSVs are **not persisted** server-side; they are re-generated on demand (OQ-006-03).
- Metric definitions (overdue = due-before-today & not `Done`; completion; throughput) are **shared with 005** so the two surfaces never disagree.
- A project's visible set and a team's membership are bounded/human-scale; only the Activity Report is genuinely large (hence its pagination + threshold).

## Dependencies
- **Depends on**: [001](../001-auth-rbac/spec.md) (Users, role model, JWT, `ICurrentUserService`, `IActivityLogService` — read + the audit write) · [002](../002-projects/spec.md) (Project entity, ownership scope, status enum) · [003](../003-tasks/spec.md) (Task entity, status enum, due date, assignee, and a **`closed_at` completion timestamp** — see the follow-up below) · [004](../004-team/spec.md) (membership — a TeamMember's visible projects) · [005](../005-dashboard/spec.md) (**no runtime dependency**, but Reports MUST share 005's role-scoping and metric definitions so shared numbers match). All five **referenced, not redefined**.
- **Cross-spec follow-up (agreed, Clarifications 2026-07-22):** 003 adds a nullable **`closed_at`** timestamp to `tasks` (set on → `Done`, cleared on re-open) via a lightweight additive migration; Reports consumes it for completion buckets and closed counts. This is a plan-time follow-up to 003, actioned when 003 is built — **not** a change made by this spec.
- **Infrastructure**: PostgreSQL 18 via EF Core 10 + Npgsql; MediatR (query dispatch for vertical slices); Serilog; OpenAPI contract under `/docs/contracts/` + Swagger UI (dev); Chart.js; jsPDF; a papaparse-based CSV utility (Constitution III).

## Out of Scope
Any write to a domain entity (the `ReportGenerated` audit entry is the sole write); user-authored custom reports / a report-query DSL (brief bonus); scheduled/recurring generation and email/Slack delivery (brief bonus — model not precluded); persisted report history / "recently generated" list (transient by default — separate `ReportArtifact` spec if ever needed); real-time report push via SignalR (not precluded — II.4 — but on-demand for v1); chart libraries other than Chart.js (D3 only with justification); new domain entities, tables, or migrations against domain tables.

---

## Sequence Note

This is the **sixth and final** module of the initial delivery (001 Auth & RBAC, 002 Projects, 003 Tasks, 004 Team, 005 Dashboard complete). It follows the structural template set by [001](../001-auth-rbac/spec.md)/[005](../005-dashboard/spec.md) and the merged-file convention. With Reports specified, **every requirement in the brief's Frontend, Backend, Database, Steps, and Deliverables lists is covered by exactly one of 001–006** (see *Brief coverage confirmation* in Scope Summary); bonus features remain out of scope per Constitution I.2. The specification phase of the project is complete.
