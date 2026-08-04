# Implementation Plan: 005 Dashboard

**Branch**: `005-dashboard` | **Date**: 2026-07-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/005-dashboard/spec.md`

**Governed by**: Project Constitution **v1.3.0** · **Governance §5 gate**: spec 005 was revised against
v1.3.0 on 2026-07-29 — **the gate is satisfied and planning may proceed.**

> **This plan inherits, it does not re-derive.** Solution layout, MediatR pipeline, and contract tooling
> come from [001](../001-auth-rbac/plan.md); access-policy consumption from [002](../002-projects/plan.md)
> and [003](../003-tasks/plan.md); the pagination-by-nature-of-collection rule from
> [004](../004-team/plan.md). The four repo-wide conventions live in
> **[ADR-0007](../../docs/adr/0007-implementation-conventions.md)**.

---

## Summary

Surface the brief's Dashboard module — project and task counts by status, overdue work, team size,
completion rate, blocked count, and a recent-activity feed — as a **strictly read-only aggregation** over
001–004. **No new entity, table, migration, or write path.**

**Technical approach.** Two **query-only** slices under `Features/Dashboard/<UseCase>/` — no commands, no
validators beyond paging. Every number is scoped by reusing 002's and 003's `ApplyScope` predicates; 005
defines no scope logic of its own. The visible-project set stays an **un-materialized `IQueryable<Guid>`**
so PostgreSQL evaluates it as a subquery and out-of-scope rows are never loaded. Enum-keyed maps are
**zero-seeded** so the payload is a stable typed contract rather than a variable dictionary. The feed reads
**through `IActivityLogService`**, never the audit table. `CanMutateAsync`, `xmin`, `ETag`, and the audit
catalog are all **deliberately absent** — and recorded as such, so their absence is not mistaken for an
omission.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10 · TypeScript strict / Angular 22 — inherited, unchanged

**Primary Dependencies**: no new backend packages. **Chart.js** (Constitution III) is used for the status
charts — first feature to need it.

**Storage**: PostgreSQL 18, **read-only**. **No table, no column, no index, no migration.** Aggregates rely
on indexes 002/003/004 already declare.

**Testing**: xUnit + Testcontainers PostgreSQL (ADR-0007 §2). Two assertions carry the feature:
**filter-at-source** (a claim about generated SQL, unverifiable on InMemory) and **"writes nothing"**.

**Target Platform / Project Type**: unchanged — existing solution, extended in place

**Performance Goals**: each metric is **one grouped aggregate** with scope pushed into SQL; no N+1, no
fetch-then-filter; the summary is a small fixed number of queries. **Live per request** in v1 — no cache,
no staleness window.

**Constraints**: scope in the query, never in memory · identity from the token · **no write of any kind** ·
metric definitions must be **value-identical to 006** (006 NFR-002)

**Scale/Scope**: **2 endpoints, 3 user stories, 2 UI surfaces** (summary tiles incl. the "My Work" panel,
activity feed).

---

## Constitution Check

*GATE: must pass before Phase 0. Re-checked after Phase 1.* Rows marked **↩** are inherited.

| Principle | Gate | Status |
|---|---|---|
| **I.1** Scope fidelity | Implements the brief's Dashboard module **by aggregation, not new persistence** | ✅ |
| **I.2** No bonus features | No customizable widgets, no notifications, no real-time push | ✅ |
| **II.1** Three-tier separation | Angular presentation-only | ✅ ↩ |
| **II.2** Vertical slice, thin controllers | 2 **query-only** slices; controller is one `Send()` | ✅ |
| **II.3** Resource-oriented URLs | `/api/dashboard/summary`, `/api/dashboard/activity` | ✅ |
| **II.4** Real-time not precluded | Read model sits behind query handlers, so a future SignalR push needs no contract change | ✅ |
| **III** Stack locked | Chart.js as specified; no new library | ✅ |
| **IV.1** DbContext, no raw SQL | LINQ grouped aggregates via `IApplicationDbContext`; no raw SQL | ✅ |
| **IV.2** Migrations | **N/A — 005 adds none** (asserted by test, quickstart V13) | ➖ |
| **IV.3** Entities/relationships | Reads only; defines nothing | ➖ |
| **IV.4** Audit every write | **N/A — there are no writes.** Empty audit catalog is intentional and documented (research R-6) | ➖ |
| **IV.5** Seed | 005 adds none | ➖ |
| **V.1 / V.4** Auth by default, secrets | Inherited | ✅ ↩ |
| **V.2** Roles via attributes only | `[Authorize]` (all three roles); **role shapes content, not access** | ✅ |
| **V.5** Validation | Paging bounds only | ✅ |
| **V.6** CORS | Inherited | ✅ ↩ |
| **VI.1 / VI.3 / VI.5** Base path, RFC 7807, Swagger dev | Inherited | ✅ ↩ |
| **VI.2** Status codes | **200, 400, 401 only** — no 403/404/409, deliberately (research R-3) | ✅ |
| **VI.4** Pagination | Feed pages (`PagedResult<T>`, 20/100, clamped); **tiles are fixed-N and correctly unpaged** | ✅ |
| **VI.6** Endpoint naming | Resource-oriented under `/api/dashboard` | ✅ |
| **VII.1–VII.2** Lazy standalone `dashboard` group | `loadChildren` → `dashboard.routes.ts` | ✅ |
| **VII.3** HTTP in services | `DashboardService`; DTO types generated from the contract | ✅ |
| **VII.4 / VII.7** Interceptors, global errors | Inherited | ✅ ↩ |
| **VII.5** Guards only | Functional auth guard on the route | ✅ |
| **VII.6** Reactive Forms | **N/A** — no forms in this feature | ➖ |
| **VII.8** Export service | **N/A** — export is 006's (VII.8 assigns it there) | ➖ |
| **VIII.1–VIII.5** Code quality | Inherited | ✅ ↩ |
| **IX.1** xUnit + `WebApplicationFactory` | Metric × role matrix table-driven; filter-at-source and no-write assertions | ✅ |
| **IX.2 / IX.3 / IX.4** Frontend tests, no merge on red, builders | Inherited | ✅ ↩ |
| **X.2** API-first | `docs/contracts/dashboard.v1.yaml` authored **before** any handler; drift proof in quickstart V15 | ✅ |
| **X.3** ADRs | ADR-0001..0007 apply; **no new ADR** — 005 introduces no repo-wide convention | ✅ |
| **XI** Deployment | Unchanged | ✅ ↩ |
| **Governance §5** | **Satisfied** — 005 revised 2026-07-29 | ✅ |

**Gate result: PASS.** No new Complexity Tracking entries — see below.

---

## Project Structure

### Documentation (this feature)

```text
specs/005-dashboard/
├── plan.md · research.md · data-model.md · quickstart.md
├── contracts/README.md      # Pointer → docs/contracts/dashboard.v1.yaml
├── checklists/requirements.md
└── tasks.md                 # Phase 2 — /speckit.tasks

docs/contracts/dashboard.v1.yaml   # THE CONTRACT — 2 operations, authored before any handler
```

### Source Code — **delta only**

```text
src/
├── ProjectManagementApp.Domain/                     # UNCHANGED — 005 defines no entity
│
├── ProjectManagementApp.Application/
│   ├── Common/Interfaces/IActivityLogService.cs     # ⚠️ EXTENDED IN 001 — needs QueryScopedAsync (R-1)
│   └── Features/Dashboard/                          # NEW — two QUERY slices, no commands
│       ├── GetSummary/   GetDashboardSummaryQuery · Handler   → DashboardSummaryDto
│       └── GetActivity/  GetDashboardActivityQuery · Validator (paging) · Handler
│                                                     → PagedResult<ActivityEntryDto>
│
├── ProjectManagementApp.Infrastructure/
│   └── Services/ActivityLogService.cs               # ⚠️ EXTENDED IN 001 — implement the scoped read
│
├── ProjectManagementApp.Api/Controllers/DashboardController.cs   # NEW — two thin GET endpoints
│
└── ProjectManagementApp.Web/src/app/
    ├── core/services/dashboard.service.ts           # NEW
    └── features/dashboard/                          # NEW — lazy standalone route group
        ├── summary/ (tiles + Chart.js + "My Work" panel) · activity-feed/
        └── dashboard.routes.ts

tests/                                               # extended in place
├── …Application.Tests/Features/Dashboard/           # metric definitions, zero-seeding, personal slice
└── …Api.Tests/Dashboard/                            # scope matrix, zero-scope 200, filter-at-source, no-write
```

**Structure Decision.** Query-only slices — 005 is the first feature with **no command and no validator
beyond paging**. No new assembly, no new test project, no new shared abstraction. Note the two files marked
⚠️: they belong to **001** and must be extended there, not here (research R-1).

---

## Complexity Tracking

> 001's four, 002's two, 003's one, and 004's one justified deviation carry forward unchanged.

**005 adds no new entries.** Its notable characteristics are **absences**, and none is a violation:

| Apparent deviation | Why it is *not* a violation |
|---|---|
| No 403/404 anywhere, unlike 002/003/004/006 | The dashboard **names no resource**; scope shapes content. Spec 002's Clarifications already record 005 as the stated exception, and 003 R-4 / 006 apply the same "naming invites a 403" principle at other granularities |
| Summary is not paginated | VI.4 targets collections that "can exceed 50 items". Fixed-N metrics do not — **compliance, not exception** (same reasoning as 004 R-4) |
| No audit rows at all | IV.4 audits **writes**; 005 performs none. Recorded in research R-6 and the spec's B.7 so it reads as intentional |
| No `CanMutateAsync` / `xmin` / `ETag` | No write path exists to authorize, version, or guard |

---

## Post-Design Re-check (after Phase 1)

- **Two upstream gaps found, neither workaroundable.** `IActivityLogService` has **only `LogAsync`** (001
  spec B.3), yet 005 and 006 both require a scoped read and are both **explicitly forbidden** from querying
  `activity_logs` directly. And 005's timezone wording (*"server/UTC unless configured"*) contradicts 006's
  fixed-UTC decision plus its hard value-parity requirement. Both are recorded as Follow-ups 1 and 2;
  neither is something 005 can decide unilaterally.
- **The 403 exception was re-examined rather than assumed.** It holds, and it is now expressible as one
  principle shared with 003 and 006 — *naming a resource invites a 403; not naming one cannot* — which is
  more defensible than "005 is special".
- **VI.4 verified as compliance for the summary**, using the rule's own ">50 items" wording, so no waiver
  was needed. Same result as 004's roster, reached the same way.
- **IV.4's absence made explicit** — the empty audit catalog is stated in three places (spec B.7,
  research R-6, contract README) precisely so a reviewer does not flag it.
- **No new violations.**

**Gate result after design: PASS.**

---

## Follow-ups

1. ~~⚠️ **`IActivityLogService` needs a scoped read.**~~ **✅ RESOLVED 2026-07-31.** `QueryScopedAsync` is
   now declared in **001 spec B.3**, created by **001 T022**, implemented by **001 T033** (with scope
   filter, stable newest-first ordering, clamped `pageSize`, scope-limited `totalCount`), with
   `ActivityScope`/`ActivityEntry` added to **T020** beside `AccessDecision`/`TaskMutation`.
   **shared-contracts §6** now documents both members and restates the never-query-the-table-directly rule.
2. ~~⚠️ **005's overdue timezone must be fixed to UTC.**~~ **✅ RESOLVED 2026-07-31.** Both locations in
   005's spec corrected: Assumptions now reads **"evaluated in UTC — fixed, not configurable"** with the
   parity reasoning stated, and B.4's `Dashboard:OverdueBoundary` is explicitly scoped to the date
   *comparison* only. 006's NFR-002 is now satisfiable by construction rather than by convention.
3. ~~**Two upstream gaps in three features is a pattern — run a sweep.**~~ **✅ RESOLVED 2026-07-31 — and
   it caught a third, exactly as hoped.** The sweep over shared-contracts §2/§3/§6/§7 against 001's
   tasks.md found that **all three scope-authorization policies (`IProjectAccessPolicy`,
   `ITaskAccessPolicy`, `ITeamAccessPolicy`) were declared shared-kernel in §3 but created by no task**.
   T022 authored only the §2/§6 interfaces. See Follow-up 5.
4. ~~**Cross-feature parity test belongs to 006.**~~ **✅ RESOLVED 2026-07-31.** 006 is planned and owns it:
   `tests/…Api.Tests/CrossFeature/DashboardReportParityTests.cs` asserts 005's `overdueTaskCount` equals the
   sum of 006's `overdueTasks` for the same caller and window (006 plan §Project Structure, 006 Follow-up 1).
   005's quickstart **V7** and 006's **V6** are the two halves of the same check. It is only meaningful
   because 005's timezone was fixed to UTC in Follow-up 2 — run it with the server clock in a non-UTC zone.
5. ~~⚠️ **The §3 access-policy interfaces were created by no task** (found by Follow-up 3's sweep).~~
   **✅ RESOLVED 2026-07-31.** **001 T022** now creates all three policy interfaces alongside the §2/§6
   ones, with a note that they belong in the shared kernel rather than in 002/003/004 — otherwise 005 and
   006 would depend on another feature's Application layer, violating the ADR-0006 addendum.
   **Root cause fixed too:** **T023**, the conformance gate, checked only **§2/§6/§7** — omitting §3 is
   precisely why this survived three planning passes. T023 now enumerates **§2/§3/§4/§6/§7** and names
   every member, so a fourth gap of this shape cannot hide behind a spot-check.

---

## Phase status

| Phase | Output | Status |
|---|---|---|
| Phase 0 — Outline & Research | `research.md` | ✅ 11 inherited by citation, 6 newly derived, 0 unresolved |
| Phase 1 — Design & Contracts | `data-model.md`, `docs/contracts/dashboard.v1.yaml`, `quickstart.md` | ✅ Complete |
| Phase 2 — Task breakdown | `tasks.md` | ⏳ Run `/speckit.tasks` |
