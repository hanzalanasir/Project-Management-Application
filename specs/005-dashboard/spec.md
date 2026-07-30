# Feature Specification: Dashboard

**Feature Number**: 005
**Feature Name**: Dashboard (Role-Scoped Read-Only Aggregation)
**Phase**: Phase 1 — Foundation
**Priority**: Must Have (Critical)
**Complexity**: Medium
**Type**: Read-only aggregation / metrics — **no new domain entities, no tables, no migrations**
**Depends On**: **001 Auth & RBAC** (Users, role model, JWT claims, `CurrentUser`, `IActivityLogService`) · **002 Projects** (Project entity, ownership scope, project status enum) · **003 Tasks** (Task entity, status enum, due date, assignee) · **004 Team** (TeamMember membership records) — all four **referenced, not redefined**
**Enables**: 006 Reports (shares this feature's read-only, role-scoped aggregation posture; adds export and historical windows)
**Created**: 2026-07-22
**Status**: Draft — Ready for Planning
**Governed By**: Project Constitution v1.3.0 (Principles I Scope, II Architecture — vertical slice / Clean Architecture, III Stack, V Security & Authorization, VI API Design, VII Frontend, VIII Code Quality, IX Testing, X Documentation — API-first)
**Cross-cutting contracts**: [docs/shared-contracts.md](../../docs/shared-contracts.md) — `Result<T>`, `ErrorKind`, `CurrentUser`, `AccessDecision` (**`ApplyScope` + `CanReadAsync` only — `CanMutateAsync` is not used; this feature performs no writes**), `PagedResult<T>` (activity feed only), error→HTTP mapping · ADRs [0001](../../docs/adr/0001-angular-standalone-components.md) standalone Angular · [0002](../../docs/adr/0002-same-origin-hosting.md) same-origin hosting · [0003](../../docs/adr/0003-result-error-contract.md) error contract · [0004](../../docs/adr/0004-optimistic-concurrency.md) concurrency (**not applicable — no mutations**) · [0005](../../docs/adr/0005-mapping-and-validation.md) mapping/validation · [0006](../../docs/adr/0006-vertical-slice-clean-architecture-api-first.md) vertical slice + Clean Architecture + API-first
**Generated Via**: `/speckit.specify` (merged requirements + solution design, per project convention)
**Revised**: 2026-07-29 — re-organized against Constitution v1.3.0 (vertical slice / Clean Architecture / API-first; II.2/IV.1/X.2/VII.3). Business rules, roles, endpoints, status codes, read model, and clarifications are unchanged — only the code-organization framing was updated. Satisfies the Governance §5 revision gate.

---

## Purpose

Surface the project statistics and key metrics the brief's Dashboard module requires — project counts by status, task counts by status, overdue work, team size, and a recent-activity feed — as a **strictly read-only aggregation** over the entities that already exist in 001–004. This feature introduces **no new domain entity, no new table, and no EF Core migration**. It reads and summarizes; it never writes.

Its one hard rule is that **every number is role-scoped at the source of the query**: an Admin sees the whole system, a ProjectManager sees only the projects they own, and a TeamMember sees only the projects they are a member of (plus their own assigned work). Scope is folded into the database query — never applied by fetching everything and filtering in memory — so out-of-scope data is never loaded, counted, or leaked through an aggregate.

This is a **merged specification**: requirements (Purpose → User Stories → Functional Requirements) and solution design (Technical Design → Implementation Blueprint) live in one file by project convention, so the team reviews *what* and *how* together.

## Business Value

A project-management tool is only as useful as the at-a-glance picture it gives each person of the work that is theirs to care about. The Dashboard turns four modules' worth of raw records into a single, immediately legible summary — how many projects are active, how much work is overdue, what has changed recently — without any user having to run a query or open every project. Because the view is role-scoped by construction, each person sees a focused workspace and never another team's numbers, and because it is strictly read-only it adds no attack surface, no audit burden, and no data-integrity risk: it cannot change anything. It is also the proving ground for the aggregation posture that 006 Reports inherits.

## Actors

**Primary Actors**
- **Admin** — a system-wide dashboard: all projects, all users, all tasks, all activity.
- **ProjectManager** — a dashboard scoped to the projects they **own**: task, team, and activity aggregates across those projects only.
- **TeamMember** — a dashboard scoped to the projects they are a **member** of (membership resolved via 004's records), plus a **personal task view** of the tasks assigned to them across those projects.

**Secondary Actors**
- **Source features (non-actor)** — 001 (audit-log service), 002 (projects + ownership scope), 003 (tasks), 004 (membership) supply every datum; this feature owns none of them.

## Scope Summary

**In scope**: role-scoped read-only aggregation endpoints returning (a) a **typed summary contract** — project counts by 002's status enum, task counts by 003's status enum, overdue-task count, visible-project count, and a visible-team-member count — and (b) a **paginated recent-activity feed** read through 001's `IActivityLogService` (never by querying the audit table directly); the reuse of 002's / 003's `ApplyScope` predicates so all aggregates filter at the query source; a TeamMember personal task slice; the lazy-loaded Angular **`dashboard` route group** (standalone components per ADR-0001) with a dedicated `DashboardService`, read-only widgets, and a functional route guard.

**Out of scope**: **any write operation whatsoever** — including a "mark activity as read" affordance (a read-tracking need, if it arises, is a separate spec); user-customizable dashboards (per-user widget layout, pinning, saving, rearranging); real-time push via SignalR (the architecture must not preclude it — Constitution I.2 / II.4 — but the initial release refetches on navigation/poll); export to PDF or CSV (owned by 006 Reports per Constitution VII.8); historical time-series charts and long-window trend analytics (006); notifications and threshold alerts (bonus, brief); any new entity, table, or migration.

## Access Logic (applies to every story)

Enforced **server-side**, in this order:

1. **Authenticated by default** — every endpoint requires a valid JWT (inherited from 001). No/invalid/expired token → **401**.
2. **Role gate (controller, attribute-only)** — `[Authorize]`; the dashboard endpoints permit **all three roles** (every authenticated user gets *their own scoped view*). There is no role that is refused outright — role shapes **content**, not access. Ad-hoc role checks in method bodies remain prohibited (Constitution V.2).
3. **Scope gate (slice handler, at the query source)** — the caller's **visible-project set** is computed by the handler folding the existing shared `ApplyScope` predicate into the query: Admin → all projects; ProjectManager → `owner_id == caller`; TeamMember → projects where the caller has a `team_members` row. Every task, team, and activity aggregate is then computed **within that set, in SQL** — never fetched then filtered. A caller with an empty visible set gets **zeroes and empty lists (200)**, not a 403.
4. **No mutation gate** — `CanMutateAsync` is deliberately absent because this feature has no write path. There is nothing to authorize a mutation for.
5. **Identity from the token** — the acting user comes from `ICurrentUserService` reading the validated JWT, never from the request body or query string.

## Role & Permission Model

The three roles are defined in [001 Auth & RBAC](../001-auth-rbac/spec.md) — each user holds **exactly one**, carried as a single JWT `role` claim. This feature introduces **no new role dimension** and honours 004's rule that membership carries no role: a TeamMember's dashboard scope comes from their membership *records*, while their *permissions* remain those of their global role. The role determines only **which slice of data is aggregated**, never what actions are available (there are none but reads).

| Dashboard content | Admin | ProjectManager | TeamMember |
|---|---|---|---|
| Visible projects (the scope everything else is computed within) | All projects | Projects they own | Projects they are a member of |
| Project counts by status | Across all | Across owned | Across member-of |
| Task counts by status / overdue | Across all tasks | Tasks in owned projects | **Only tasks assigned to them** (personal-view; Clarifications 2026-07-22) across member-of projects |
| Visible team-member count | All users on any project | Members across owned projects | Members across member-of projects |
| Recent activity feed | All activity | Activity in owned projects | Activity in member-of projects |
| Personal task view (my assigned tasks) | (n/a — Admins act system-wide) | (optional) | **Their assigned tasks across member-of projects** |

---

## Clarifications

### Session 2026-07-22

- Q: For a TeamMember, does the summary's "tasks by status" mean every task in their member-of projects (project-view) or only tasks assigned to them (personal-view)? → A: **Personal-view.** A TeamMember's task-by-status tile counts **only tasks assigned to them** (identical to US-005-03's personal slice), so there is one number to compute and test, it matches the "my work" mental model, and it never surfaces colleagues' work a TeamMember cannot act on. A project-wide breakdown may be added later as a secondary view but is not part of v1.
- Q: Are dashboard values computed live per request or served from a cached/materialized view? → A: **Live per request for v1.** Each metric is a single indexed grouped query over a small dataset, so live keeps values always-fresh with no staleness window and no invalidation logic to design or test. The read model stays inside the dashboard query handlers, so introducing a cache (fixed-cadence or event-driven) later is an internal change that does not alter the API contract — revisited only if real load demands it.
- Q: Which metrics beyond the baseline ship in v1? → A: **Baseline + completion rate + blocked-task count.** In addition to the baseline (projects-by-status, tasks-by-status, overdue, team size, activity feed, personal slice), v1 adds a **completion rate** (Done ÷ total tasks in the caller's scope, 0 when there are no tasks) and a **blocked-task count** (a headline tile derived from `tasksByStatus[Blocked]`, no extra query). Both reuse data already aggregated and need no field 002/003 did not commit to. Time-to-close and most-active-member need a completion timestamp / heavier aggregation and are deferred to **006 Reports**.
- Q: Activity-feed default page size, and all visible entries vs a filtered subset? → A: **Default `pageSize` 20, max 100, and show all visible entries** (no mutation-only filter). The paging bounds match 002–004's convention for one consistent rule across the app; showing all visible entries is the simplest correct behaviour since 001–004 audit only meaningful writes (there is no read/noise to exclude). The feed is scoped to activity on the caller's **visible projects and their tasks/team changes**; a subset filter can be added later if the feed ever proves busy.

---

## User Stories

> Story IDs `US-005-01..03`. Each story: **A** Summary · **B** Quality Validation (INVEST · 3Cs · 7Cs · Given-When-Then · edge cases · audit/security · configurability) · **C** UI · **D** API · **E** DB · **F** Separation of concerns. Consolidated read model, API catalog, technical design, and implementation blueprint follow the stories.

---

### US-005-01 — View the role-scoped project & task summary

**A. Summary**
- **Story ID**: US-005-01 · **Title**: See my summary tiles — projects, tasks, overdue, team size
- **Actor**: Admin · ProjectManager · TeamMember (each with a different scope)
- **User story**: *As any authenticated user, I want a summary of the projects and tasks I can see — counts by status, how much is overdue, and how many people are on my projects — so that I understand the state of my work at a glance.*
- **Business value**: The headline value of the whole module; also the proof that role-scoping holds across aggregated numbers.
- **Priority**: **P0** · **Reason**: The dashboard's primary payload.
- **Dependencies**: 002 (projects + status + scope), 003 (tasks + status + due date), 004 (membership for TeamMember scope). **Out of scope**: the activity feed (US-005-02) and export (006).

**B. Quality validation**
- **INVEST** — Independent ✔ (the summary stands alone from the feed); Negotiable ✔ (the exact metric set and numeric bounds are under clarification, OQ-005-01); Valuable ✔; Estimable ✔; Small ✔ (a handful of scoped aggregate queries into one typed contract); Testable ✔ (each count is exactly assertable per role against seeded data).
- **3Cs** — Card ✔ (stands alone: "show me my scoped summary tiles"); Conversation ✔ (surfaced which extra metrics belong in v1 (OQ-005-01), live-vs-cached computation (OQ-005-02), and the TeamMember project-view-vs-personal meaning of task counts (OQ-005-04); see Edge cases and Open Questions); Confirmation ✔ (the Given/When/Then scenarios cover the three-role scope matrix, the zero-scope case, at-source filtering, and the stable-contract shape — sufficient to call this story done once the metric set (OQ-005-01) is fixed).
- **7Cs** — Clear ✔ (states outright that scope is applied at the query source, not in memory); Concise ✔; Concrete ✔ (named metrics, exact enum-keyed breakdowns, exact 200 behaviour on empty scope); Correct ✔ (reuses 002/003 status enums verbatim, matches FR-002/FR-005); Coherent ✔ (the visible-project set is exactly 002's project-read scope, so the dashboard can never show a project the projects screen would hide); Complete ✔ (all three roles, the empty case, and the stable-contract requirement are covered, with the open metric set flagged rather than guessed); Courteous n/a (read-only tiles; no user-facing prose copy specified here).
- **Given/When/Then**
  1. **Given** projects and tasks across several owners, **When** an **Admin** requests the summary, **Then** **200** with counts computed across **all** projects/tasks.
  2. **Given** the same data, **When** a **ProjectManager** requests the summary, **Then** counts are computed across **only the projects they own** — another owner's projects contribute to no tile.
  3. **Given** the same data, **When** a **TeamMember** requests the summary, **Then** project counts are computed across **only the projects they are a member of**, and their **task-by-status and overdue tiles count only tasks assigned to them** (personal-view; Clarifications 2026-07-22) — identical to the US-005-03 slice.
  4. **Given** a caller whose visible-project set is **empty** (a TeamMember on no teams, or a ProjectManager owning nothing), **When** they request the summary, **Then** **200** with all counts **zero** and all breakdowns present-but-empty — **not** a 403 or 404.
  5. **Given** the response, **When** it is inspected, **Then** it is a **stable typed contract**: every `ProjectStatus` (002) and every `TaskStatus` (003) appears as a key with a count (zeros included), not a variable free-form dictionary.
  6. **Given** an **overdue** definition of "due date before today AND status is not the terminal `Done` state" (003), **When** overdue is counted, **Then** it reflects only tasks within the caller's visible-project set.
  7. **Given** the v1 metric set (Clarifications 2026-07-22), **When** the summary is returned, **Then** it also includes a **completion rate** (Done ÷ total tasks in scope, `0` when there are no tasks) and a **blocked-task count** — both scoped like every other tile (personal-view for a TeamMember).
- **Edge cases**: a caller with zero visible projects → all-zero summary (**200**, never 403); a project/task the caller cannot see → **excluded from every aggregate** because the scope predicate is in the query (never fetched then filtered); a status enum value with zero rows → present as `0` (stable contract); overdue **boundary** — a task due *today* is not yet overdue, one due *before* today is (state the exact bound; final numeric/date bounds are OQ-005-01); a task with **no** due date → never overdue; large data volume → aggregates computed in SQL (`GROUP BY status`), **live per request** for v1 (Clarifications 2026-07-22), with a cache deferrable behind the service seam if load ever demands it.
- **Audit/security**: **this operation writes nothing and therefore emits no `activity_logs` entry** — Constitution IV.4 audits *writes*, of which the dashboard has none (this absence is intentional; see B.7). Reads are not audited (consistent with 001–004). Scope is enforced **in the query** so no out-of-scope figure can leak through a total; identity comes from the token.
- **Configurability**: which metrics beyond the baseline appear (OQ-005-01); the overdue date boundary (default: strictly before today, timezone assumption in Assumptions). (Computation is **live per request** in v1, not configurable — Clarifications 2026-07-22.)

**C. UI** — **F005-S01 Dashboard Summary** (standalone components in the lazy-loaded `dashboard` route group): fixed-N summary **tiles** (project count, tasks-by-status mini-breakdown, overdue count, team size) plus small status charts (Chart.js per Constitution III). Tiles are read-only; **no pagination** (a fixed set of scalar metrics). Loading, empty ("nothing assigned to you yet"), and error states are explicit. The whole route is behind a functional auth guard.

**D. API** — `GET /api/dashboard/summary` · `[Authorize]` (all three roles) · **200** with the typed `DashboardSummaryDto` · **401**. (No 403: scope shapes content, not access.)

**E. DB** — **reads only** — `projects` (002), `tasks` (003), `team_members` (004), `users` (001), aggregated with `GROUP BY` inside the scope predicate. **No writes, no migration.** Relies on indexes 002/003/004 already declare (`owner_id`, `project_id`, `assignee_id`, `status`, and a `due_date` filter).

**F. Separation** — UI: read-only tiles + charts + states. Backend: `Features/Dashboard/GetSummary/` slice — `GetDashboardSummaryQueryHandler` composes 002's/003's `ApplyScope` into grouped-count queries and assembles the typed DTO; the controller only `Send`s the query. DB: scoped aggregate reads (handler → `DbContext` directly). QA: three-role scope matrix (**primary acceptance test**), zero-scope → zeroes, filter-at-source (no in-memory filtering), stable-contract keys.

---

### US-005-02 — View the role-scoped recent activity feed

**A. Summary**
- **Story ID**: US-005-02 · **Title**: See recent activity across the work I can see
- **Actor**: Admin · ProjectManager · TeamMember (each scoped)
- **User story**: *As any authenticated user, I want a paginated feed of recent activity on the projects I can see, so that I know what changed, when, and by whom without opening each record.*
- **Business value**: Turns 001's audit trail into a human-readable "what's new" — the second core dashboard widget.
- **Priority**: **P1** · **Reason**: High value, but the summary tiles are the headline.
- **Dependencies**: 001 (`IActivityLogService`), 002/004 (scope). **Out of scope**: marking entries read (no writes), and export (006).

**B. Quality validation**
- **INVEST** — Independent ✔ (the feed stands alone from the tiles); Negotiable ✔ (default page size and whether the feed is filtered to a subset are under clarification, OQ-005-03); Valuable ✔; Estimable ✔; Small ✔ (one scoped, paginated read); Testable ✔ (feed contents and paging are directly assertable per role).
- **3Cs** — Card ✔ (stands alone: "show me recent activity I'm allowed to see"); Conversation ✔ (surfaced the default page size, whether to show all visible entries or a filtered subset (OQ-005-03), and how activity for a now-invisible project is handled; see Edge cases and Open Questions); Confirmation ✔ (the Given/When/Then scenarios cover per-role scoping, pagination, the empty feed, and reading-through-the-service — sufficient to call this story done once OQ-005-03 is fixed).
- **7Cs** — Clear ✔ (states the feed is read **through `IActivityLogService`**, never by a direct audit-table query); Concise ✔; Concrete ✔ (exact `PagedResult<T>` envelope, exact 200/400 outcomes); Correct ✔ (matches FR-006/FR-007 and Constitution VI.4); Coherent ✔ (feed scope equals the summary's visible-project scope — one definition of "what I can see" across the module); Complete ✔ (scoping, pagination, empty feed, and the service-read constraint are all covered); Courteous n/a (a data feed with only standard empty/loading states).
- **Given/When/Then**
  1. **Given** audit entries across many projects, **When** an **Admin** requests the feed, **Then** **200** with a `PagedResult<ActivityEntryDto>` covering **all** entries, newest first.
  2. **Given** the same data, **When** a **ProjectManager** or **TeamMember** requests the feed, **Then** it contains **only** entries for entities in their visible-project set — never another team's activity.
  3. **Given** `?page`/`?pageSize`, **When** requested, **Then** a `PagedResult<T>` is returned whose `totalCount` is **scoped to the caller**; `pageSize` above the maximum is **clamped**, not rejected; non-numeric/negative paging → **400**.
  4. **Given** no visible activity (a caller with an empty scope, or a fresh system), **When** requested, **Then** **200** with an **empty page**, not a 404.
  5. **Given** the implementation, **When** the feed is built, **Then** it is sourced through **`IActivityLogService`** (001), not by querying the `activity_logs` table directly.
- **Edge cases**: activity for a project the caller **can no longer see** (e.g. a project a PM no longer owns, or that was deleted — audit rows survive per 001–004) is **scoped out for non-Admins** and visible to **Admin** only; `page` beyond the last page → empty items with valid metadata; a burst of activity between two page requests (feed is a snapshot per request; stable ordering by timestamp + id); **default `pageSize` 20, max 100, all visible entries shown (no subset filter)** — Clarifications 2026-07-22.
- **Audit/security**: **reading the feed writes nothing and emits no `activity_logs` entry** (see B.7). The read is scoped through the service to the caller's visible entities so no out-of-scope change summary leaks; entries never contain secrets (001 guarantees that at write time).
- **Configurability**: default page size **20**, maximum **100** (Clarifications 2026-07-22); the feed shows **all visible entries** (no subset filter in v1); default ordering newest first.

**C. UI** — **F005-S02 Activity Feed** (a paginated list widget on the dashboard route). Each row: actor, action, entity type/id, relative timestamp, change summary. Infinite-scroll or pager over the `PagedResult<T>`. Empty, loading, and error states explicit. Strictly read-only — **no "mark read", no actions**.

**D. API** — `GET /api/dashboard/activity?page=&pageSize=` · `[Authorize]` (all three roles) · **200** with `PagedResult<ActivityEntryDto>` · **400** (bad paging) · **401**.

**E. DB** — **reads only**, through `IActivityLogService` (001), scoped to the caller's visible entities. **No writes, no migration.**

**F. Separation** — UI: paginated feed widget + states. Backend: `GetDashboardActivityQueryHandler` calls the 001 audit-log **read** method (`IActivityLogService`) with the caller's visible-scope filter and paging; the controller only `Send`s the query. DB: scoped, paginated audit read via the service. QA: per-role scope, clamped `pageSize`, empty feed, service-not-direct-table, invisible-project exclusion for non-Admins.

---

### US-005-03 — View my personal task slice (TeamMember)

**A. Summary**
- **Story ID**: US-005-03 · **Title**: See the tasks assigned to me across my projects
- **Actor**: TeamMember (primary); available to any role as a personal slice
- **User story**: *As a TeamMember, I want a focused view of the tasks assigned to me across the projects I'm on — how many, by status, how many overdue — so that I know exactly what I personally need to do.*
- **Business value**: The TeamMember's most-used number: their own workload, distinct from a project-wide view.
- **Priority**: **P1** · **Reason**: Central to the TeamMember experience; a thin slice over the same data.
- **Dependencies**: 003 (assignee + status + due date), 004 (member-of scope). **Out of scope**: acting on the tasks (that is 003's endpoints); this is a read-only summary.

**B. Quality validation**
- **INVEST** — Independent ✔ (a self-contained personal slice); Negotiable ✔ (whether this slice also drives the summary's TeamMember task tile is OQ-005-04); Valuable ✔; Estimable ✔; Small ✔ (one scoped aggregate over assignee); Testable ✔ (personal counts assertable against seeded assignments).
- **3Cs** — Card ✔ (stands alone: "show me my own tasks, summarized"); Conversation ✔ (surfaced whether the summary's TeamMember task counts should mean *personal* or *whole-visible-project* (OQ-005-04), and the coherence with 004's removal-block invariant; see Edge cases); Confirmation ✔ (the Given/When/Then scenarios cover the assignee filter, the member-of scope, the empty case, and the overdue-personal count — sufficient to call this story done).
- **7Cs** — Clear ✔ (distinguishes "assigned to me" from "on a project I can see"); Concise ✔; Concrete ✔ (exact assignee predicate, exact status/overdue breakdown); Correct ✔ (matches FR-004 and 003's assignee semantics); Coherent ✔ (because 004 blocks removing a member with open assigned tasks, a personal open task can never sit in a project the caller is no longer a member of — the slice and the scope stay consistent); Complete ✔ (assignee filter, member-of scope, empty case, and overdue are covered); Courteous n/a (a read-only personal summary).
- **Given/When/Then**
  1. **Given** tasks assigned to the caller across projects they are a member of, **When** they request their personal slice, **Then** **200** with the count of their assigned tasks, a by-status breakdown, and their overdue count — computed only over tasks where `assignee_id == caller`.
  2. **Given** tasks assigned to **other** users on the same projects, **When** the caller requests their slice, **Then** those tasks are **excluded** — the slice is strictly the caller's own.
  3. **Given** a task assigned to the caller in a project they are **not** a member of, **When** the slice is computed, **Then** it is **excluded** (the slice is scoped to member-of projects) — and by 004's removal-block invariant this cannot occur for an **open** task anyway.
  4. **Given** a TeamMember with **no** assigned tasks, **When** they request the slice, **Then** **200** with zeroes, not a 404.
- **Edge cases**: a task reassigned away between two requests (the slice reflects current assignment); an overdue personal task (same boundary rule as US-005-01); for a TeamMember this personal slice **is** the number shown in the summary's task-by-status tile (personal-view; Clarifications 2026-07-22) — there is no divergent project-wide count in v1; a ProjectManager/Admin requesting a personal slice (permitted — returns their own assignments, typically empty for pure managers).
- **Audit/security**: **read-only — no `activity_logs` entry** (see B.7); the assignee filter and member-of scope are both applied **in the query**; no other user's workload is exposed.
- **Configurability**: whether the personal slice is surfaced for non-TeamMember roles (default: available to all, prominent for TeamMember); its relationship to the summary tile (OQ-005-04).

**C. UI** — a **"My Work"** panel on **F005-S01 Dashboard Summary** (TeamMember-prominent): my-task count, by-status mini-breakdown, my-overdue count, with a link into 003's task list filtered to the caller. Read-only tiles; no pagination. Empty state: "You have no tasks assigned."

**D. API** — surfaced within `GET /api/dashboard/summary` as a `personalTasks` block (present for the caller; see the typed contract in T.3), rather than a separate endpoint — one round trip for the whole summary. `[Authorize]` (all three roles) · **200**.

**E. DB** — **reads only** — `tasks` (003) filtered by `assignee_id == caller` within the member-of project set (004). **No writes, no migration.**

**F. Separation** — UI: "My Work" panel. Backend: `GetDashboardSummaryQueryHandler` includes the personal slice, computed by an assignee-filtered grouped count within the visible scope; the controller only `Send`s the query. DB: scoped assignee aggregate. QA: assignee-only inclusion, member-of scoping, empty slice, overdue-personal count.

---

## Consolidated Read Model (no new tables, no migration)

> This feature adds **no** entity, table, or EF Core migration (Constitution I.1 — it implements the brief's Dashboard module by aggregation, not by new persistence). It **reads** the entities below, all owned elsewhere, and projects them into **transient read-model DTOs** that are never persisted. There is no `xmin` (no mutation) and no audit write (no write path).

| Source entity | Owner | Read for |
|---|---|---|
| `projects` | 002 | Project counts by status; the visible-project scope (via 002's `ApplyScope`) |
| `tasks` | 003 | Task counts by status, overdue count, the TeamMember personal slice (assignee) |
| `team_members` | 004 | TeamMember visible-project scope; visible-team-member count |
| `users` | 001 | Actor/member display; distinct headcount |
| `activity_logs` | 001 | The recent-activity feed — **read only through `IActivityLogService`**, never by direct table query |

**Read-model DTOs (transient, not persisted):**
- `DashboardSummaryDto` — a **stable typed contract** (not a free-form stat dictionary): `generatedAt`, `scope` (the applied role scope, for labelling), `visibleProjectCount`, `projectsByStatus` (one entry per `ProjectStatus`, zeros included), `tasksByStatus` (one entry per `TaskStatus`, zeros included), `overdueTaskCount`, `completionRate` (Done ÷ total tasks in scope, `0` when no tasks; Clarifications 2026-07-22), `blockedTaskCount` (headline tile derived from `tasksByStatus[Blocked]`), `visibleTeamMemberCount`, and `personalTasks` (`{ assignedTotal, byStatus, overdueCount }`).
- `ActivityEntryDto` — `id`, `actorName`, `action`, `entityType`, `entityId`, `timestamp`, `changeSummary` (projected from the audit entry the service returns).

## Consolidated API Catalog

> Base path `/api`; JSON over HTTPS; errors as **RFC 7807 Problem Details** via the shared `ErrorKind` mapper ([shared-contracts §1](../../docs/shared-contracts.md)). Per Constitution **X.2 (API-first)**, the OpenAPI contract for these routes is authored and reviewed under `/docs/contracts/` **before** the handlers are implemented, and the code is validated against it; **Swagger UI** is enabled in development for local exploration only. Authenticated by default (001). **All endpoints are `GET` — there are no write endpoints** (see T.6). Resource-oriented under `/api/dashboard` (Constitution VI.6).

| Method · Route | Purpose | Role gate | Access gate (in handler) | Success | Failure |
|---|---|---|---|---|---|
| `GET /api/dashboard/summary` | Typed summary contract (tiles + personal slice) | `[Authorize]` (all 3) | `ApplyScope` (read-only) | **200** `DashboardSummaryDto` | 401 |
| `GET /api/dashboard/activity` | Paginated, scoped recent-activity feed | `[Authorize]` (all 3) | `ApplyScope` (read-only) | **200** `PagedResult<ActivityEntryDto>` | 400 (bad paging), 401 |

---

## Technical Design — Role-Scoped Read-Only Aggregation

> The detailed solution: the read-only posture, the endpoint-shape decision and why, how scope is folded into aggregate queries, the step-by-step flows, failure handling, and the security guarantees. Written so a developer can implement it directly.

### T.1 The read-only posture (what this feature deliberately does *not* have)
- **The .NET API is the authority**; the **slice handler** owns the aggregation and scope. The **thin controller** does nothing but `MediatR.Send(...)` (Constitution II.2).
- **No writes, ever.** There is no create/update/delete path, so: `CanMutateAsync` is **not** implemented (nothing to authorize a mutation for); `xmin`/optimistic concurrency (ADR-0004) is **not applicable** (nothing is updated); and the feature emits **no `activity_logs` entries** — Constitution IV.4's audit requirement is about *writes*, and there are none. These absences are intentional and are called out again in B.7 so a reviewer does not read them as omissions.
- **The Angular frontend is convenience.** Read-only widgets; a functional guard gates the route; the API re-checks scope on every request.

### T.2 Role-scoped aggregation is the whole feature (the heart)
Every number is computed **within the caller's visible-project set**, and that set is produced by **reusing the existing `ApplyScope` predicates** — this feature defines no new scope logic:
- **Visible projects** = `IProjectAccessPolicy.ApplyScope(projects, caller)` (002): Admin → all; ProjectManager → `owner_id == caller`; TeamMember → `team_members.Any(user_id == caller)`.
- **Task aggregates** = counts over `tasks` whose `project_id ∈ visibleProjectIds`, reusing 003's task scope where a personal slice is needed (`assignee_id == caller`).
- **Team count** = distinct `user_id` over `team_members` whose `project_id ∈ visibleProjectIds`.
- **Activity feed** = `IActivityLogService`'s scoped read over entities in the visible set.

**Filter at the source, never in memory.** Each aggregate is expressed so EF Core translates the scope predicate **into the SQL** (`WHERE project_id IN (scoped subquery) GROUP BY status`). The handler never loads all rows and filters afterward — that would both leak effort and risk a scoping mistake. This is the single most important implementation rule of the feature (NFR-002, DoD).

### T.3 The endpoints, with concrete examples

**(1) Summary** — one typed contract, one round trip:
```
GET /api/dashboard/summary     Authorization: Bearer eyJ…   (role=ProjectManager)

→ 200 OK
{ "generatedAt": "2026-07-22T11:00:00Z", "scope": "ProjectManager",
  "visibleProjectCount": 3,
  "projectsByStatus": { "Planning": 1, "Active": 1, "OnHold": 0, "Completed": 1, "Cancelled": 0 },
  "tasksByStatus":    { "ToDo": 8, "InProgress": 5, "InReview": 2, "Done": 12, "Blocked": 1 },
  "overdueTaskCount": 4,
  "completionRate": 0.43, "blockedTaskCount": 1,
  "visibleTeamMemberCount": 7,
  "personalTasks": { "assignedTotal": 0, "byStatus": { … }, "overdueCount": 0 } }
```
Every `ProjectStatus`/`TaskStatus` key is present with a count (zeros included) — a **stable typed contract**, not a variable dictionary. A caller with an empty scope gets the same shape with all zeros.

**(2) Activity feed** — scoped + paginated:
```
GET /api/dashboard/activity?page=1&pageSize=20     Authorization: Bearer eyJ…   (role=TeamMember)

→ 200 OK
{ "items": [ { "id": "…", "actorName": "Priya Nair", "action": "TaskStatusChanged",
               "entityType": "Task", "entityId": "9ac4…", "timestamp": "2026-07-22T10:40:00Z",
               "changeSummary": "InProgress → InReview" } ],
  "page": 1, "pageSize": 20, "totalCount": 1, "totalPages": 1 }
→ 400   (non-numeric or negative page/pageSize)
```
`totalCount` is scoped to the caller; a TeamMember never learns the system-wide activity volume.

### T.4 Endpoint-shape decision — summary payload **+** separate paginated feed (and why)
The brief allows either one aggregated endpoint or split widgets. The chosen shape is a **hybrid, and it is essentially forced by pagination**:
- The **summary** is a small, bounded set of scalar/enum-keyed metrics that belong together and are wanted in one paint — so it is **one typed `GET /api/dashboard/summary`** returning a stable contract (not a dictionary of untyped stats, per the brief). The TeamMember personal slice rides inside it (one round trip).
- The **activity feed** grows without bound and needs `?page`/`?pageSize`; paginating a *sub-list embedded inside a summary payload* is awkward and couples two very different cache/lifetime profiles. So the feed is its **own `GET /api/dashboard/activity`** returning `PagedResult<T>`.
- A single mega-`GET /api/dashboard` returning both a typed summary and a paged feed was rejected: it forces the feed's paging parameters onto the summary and defeats independent caching.

> **OQ-005-05 (deferred, low priority):** whether the *summary* should later be split into per-widget endpoints is a refinement tied to caching. Since OQ-005-02 chose **live per request** (no caching; Clarifications 2026-07-22), the motivation to split is gone — the committed single-summary shape stands for v1, and the split is revisited only if a future cache makes per-widget invalidation worthwhile.

### T.5 How a role-scoped summary is computed (step by step)
1. The JWT is validated; `ICurrentUserService` materializes `CurrentUser(UserId, Email, Role)` — never from the body.
2. The handler builds the **visible-project subquery** via `IProjectAccessPolicy.ApplyScope(projects, caller)` (002) — an `IQueryable<Guid>` of project ids, not a materialized list.
3. **Projects-by-status**: `GROUP BY status` over the scoped project query → the `projectsByStatus` map (all enum keys seeded to 0 first).
4. **Tasks-by-status / overdue**: `GROUP BY status` over `tasks WHERE project_id IN (visible subquery)`; overdue = `due_date < today AND status <> 'Done'` within the same scope.
5. **Team count**: `COUNT(DISTINCT user_id)` over `team_members WHERE project_id IN (visible subquery)`.
6. **Personal slice**: the task aggregate re-run with the extra predicate `assignee_id == caller`.
7. Assemble the typed `DashboardSummaryDto` and return it. All steps push scope into SQL; nothing is fetched then filtered. These run **live per request** in v1 (Clarifications 2026-07-22); a cache can be introduced **inside the query handler** later without a contract change.

### T.6 API behaviour rules
- **All `GET`, no writes** — there is no POST/PUT/DELETE in this feature; a "mark read" or any write is explicitly out of scope.
- **Status codes** (Constitution VI.2): 200 on read; 400 on bad paging (activity); 401 unauthenticated; **no 403** (every authenticated user gets their own scoped view — an empty scope is 200-with-zeros, not a denial); no 404 (the dashboard always exists for an authenticated caller); no 409/`xmin`. All errors are Problem Details from the shared mapper (ADR-0003).
- **Pagination** (Constitution VI.4): the **activity feed** returns `PagedResult<T>` with `?page`/`?pageSize`, default and max configurable (OQ-005-03), clamped not rejected. The **summary tiles are fixed-N scalar/enum metrics and are deliberately *not* paginated**; the personal slice inside the summary is likewise fixed-N.
- **Real-time-ready** — the read model is computed behind a service, so a future SignalR push (Constitution II.4) can broadcast summary deltas without changing the contract; the initial release refetches on navigation/poll.
- **Versionable** — routes are designed so a future `/api/v1` prefix can be added without breaking clients (Constitution VI.1); not adopted now.

### T.7 Failure handling (fail-safe)
- **Unknown/expired token → 401** (001).
- **No 403 for the dashboard itself** — role narrows content, not access; a caller with an empty visible set gets a valid **200** of zeroes/empties.
- **Bad paging on the feed → 400** with field errors.
- **Missing metric dependency** (e.g. a value that needs a field 002/003 did not commit to) is handled per the Assumptions section, not by a runtime failure.
- Uncaught errors → **500** as Problem Details; the Angular `ErrorInterceptor` + global `ErrorHandler` surface them via the shared notification component (Constitution VII.7).

### T.8 Security guarantees
- Every endpoint requires a valid JWT; the role gate is **attribute-declared only** (Constitution V.1, V.2).
- **Scope is enforced in the query source** — out-of-scope projects/tasks/members/activity are never loaded, so nothing leaks through a count, a total, or a paging figure.
- **No write path** — the feature cannot alter any record, which is both a security property (no injection into domain state) and the reason it produces no audit entries.
- The activity feed is read **through `IActivityLogService`**, inheriting 001's guarantee that audit entries carry no secrets.
- Identity comes from the token; a caller cannot request another user's scope by manipulating the request.
- All data access goes through EF Core; no raw SQL (Constitution IV.1) — except that reporting-grade aggregate queries remain LINQ here (any future raw SQL would follow Constitution IV.1's parameterized-and-reviewed exception, but none is needed for this feature).

---

## Implementation Blueprint (build-ready detail)

> Everything the team needs to build this feature: the (absent) schema, the reused enums, service interfaces, configuration, error model, NFRs, the (empty-by-design) audit catalog, and the Definition of Done.

### B.1 Schema — none
> **This feature creates no table and no EF Core migration.** It reads `projects`/`tasks`/`team_members`/`users`/`activity_logs` (owned by 001–004) and returns transient DTOs. There is **no `xmin`** (nothing is updated) and **no new index required** — the aggregates rely on indexes 002/003/004 already declare: `projects(owner_id, status)`, `tasks(project_id, status)`, `tasks(assignee_id, status)`, a `tasks(due_date)`-supporting filter, and `team_members(project_id, user_id)`. If profiling later shows a hot aggregate, an index is added by the owning feature's migration, not here.

### B.2 Enumerations — reused, none new
- **ProjectStatus** — reused from 002 (`Planning, Active, OnHold, Completed, Cancelled`); drives `projectsByStatus`.
- **TaskStatus** — reused from 003 (`ToDo, InProgress, InReview, Done, Blocked`); drives `tasksByStatus`; `Done` is the terminal state for the overdue rule.
- *(No new enum. `scope` in the DTO reuses the global `Role` values from 001.)*

### B.3 Vertical slices, handlers & shared abstractions (C#; nullable reference types on)

Per Constitution **II.2**, each read use-case is a self-contained **vertical slice** under
`Features/Dashboard/<UseCase>/`, holding its Query and handler. There are **no commands, no validators
beyond paging, and no write path** (T.1). Controllers are **thin**: one endpoint maps one HTTP verb to a
single `MediatR.Send(...)`. Per **IV.1**, each query handler reads via the EF Core `DbContext` **directly**
(grouped aggregates), reusing the shared scope abstractions below — **no Repository is introduced**.

```text
Features/Dashboard/GetSummary/    GET /api/dashboard/summary       [Authorize] (all 3)
  GetDashboardSummaryQuery() : IRequest<Result<DashboardSummaryDto>>   // scope derived from the caller, not the body
  GetDashboardSummaryQueryHandler // reuses IProjectAccessPolicy/ITaskAccessPolicy.ApplyScope; grouped counts; personal slice; live per request
  → Response: DashboardSummaryDto

Features/Dashboard/GetActivity/   GET /api/dashboard/activity      [Authorize] (all 3)
  GetDashboardActivityQuery(Page, PageSize) : IRequest<Result<PagedResult<ActivityEntryDto>>>
  GetDashboardActivityQueryHandler // reads through IActivityLogService's scoped read — never the audit table directly
  → Response: PagedResult<ActivityEntryDto>
```

**Shared cross-cutting abstractions the handlers depend on** (reused, **not** redefined; these are **shared-kernel** abstractions declared in [docs/shared-contracts.md §3](../../docs/shared-contracts.md) (`IProjectAccessPolicy`/`ITaskAccessPolicy`) and §6 (`IActivityLogService`), so the handler depends on shared-kernel contracts — it does **not** call 002's/003's/001's slice handlers):
```csharp
//  IProjectAccessPolicy.ApplyScope(IQueryable<Project>, CurrentUser)  — 002 (visible-project scope)
//  ITaskAccessPolicy.ApplyScope(IQueryable<TaskItem>, CurrentUser)    — 003 (task scope, for the personal slice)
//  IActivityLogService  — 001 (audit-log owner): this feature consumes a SCOPED READ method on it
//    (e.g. Task<PagedResult<ActivityEntry>> QueryScopedAsync(ActivityScope scope, int page, int pageSize, CancellationToken ct)).
//    Reading the audit log through its owning service — never a direct activity_logs query — is a
//    deliberate constraint; exposing a read on the 001-owned service is within 001's ownership of the
//    audit log and is NOT a change to 002/003 (see Assumptions).
```
```text
// GetDashboardActivityQuery carries { Page, PageSize }; scope is derived from the caller, not passed in the body.
// DashboardSummaryDto { DateTimeOffset GeneratedAt; string Scope; int VisibleProjectCount;
//   IReadOnlyDictionary<ProjectStatus,int> ProjectsByStatus;   // one entry per enum value, zeros included
//   IReadOnlyDictionary<TaskStatus,int> TasksByStatus; int OverdueTaskCount;
//   double CompletionRate; int BlockedTaskCount;   // v1 extras (Clarifications 2026-07-22); derived, no new query
//   int VisibleTeamMemberCount; PersonalTaskSummary PersonalTasks; }
// PersonalTaskSummary { int AssignedTotal; IReadOnlyDictionary<TaskStatus,int> ByStatus; int OverdueCount; }
// ActivityEntryDto { Guid Id; string ActorName; string Action; string EntityType; string EntityId;
//   DateTimeOffset Timestamp; string ChangeSummary; }
// Result<T>, ErrorKind, CurrentUser, AccessDecision, PagedResult<T> — docs/shared-contracts.md (ADR-0003), reused.
// The enum-keyed maps are a fixed, typed shape (one key per enum value), not a free-form string dictionary.
```
`IActivityLogService` is **reused from 001**; scope predicates come from **002/003**; nothing is redefined here. A future cache can be introduced **inside** the query handlers without changing the API contract.

### B.4 Configuration (never hardcoded)
- `Dashboard:Activity:{DefaultPageSize,MaxPageSize}` — **20 / 100** (Clarifications 2026-07-22), clamped not rejected
- *(No `Activity:Filter`: the feed shows **all visible** entries in v1 — a subset filter would be a later addition, not a v1 config; Clarifications 2026-07-22.)*
- *(No `Computation` toggle: values are computed **live per request** in v1 — a fixed decision, not configurable; Clarifications 2026-07-22. A cache would be introduced inside the query handlers in a later iteration, not via config.)*
- `Dashboard:Metrics` — the v1 set is **baseline + completion rate + blocked-task count** (Clarifications 2026-07-22); this flag gates only any *future* optional metrics, not the fixed v1 set
- `Dashboard:OverdueBoundary` / timezone assumption for "before today" (see Assumptions)
- *(No `TeamMemberTaskTile` toggle: the TeamMember task tile is **personal-view** — assigned-to-them only — a fixed v1 decision, not configurable; Clarifications 2026-07-22.)*

### B.5 Error model (RFC 7807 Problem Details)
Produced by the shared `ErrorKind` → status mapper ([shared-contracts §1](../../docs/shared-contracts.md), ADR-0003): `400` validation (bad paging on the activity feed) · `401` `Authentication required` · `500` `Unexpected error`. **No `403`** (scope shapes content, not access), **no `404`** (the dashboard always exists for an authenticated caller), **no `409`** (no writes, no concurrency). Never leak an out-of-scope figure in an error body.

### B.6 Non-functional requirements
- **Security:** deny-by-default authentication; scope enforced in the query source; no write path.
- **Performance:** each metric is a single grouped aggregate pushed to the database within the scope predicate; **no N+1**, **no fetch-then-filter**; the summary is a small fixed number of aggregate queries. Values are computed **live per request** in v1 (Clarifications 2026-07-22); the read model inside the query handlers makes swapping to a cached/materialized source a handler-internal change (not a contract change) if real load ever demands it.
- **Observability:** structured logging via **Serilog**; slow-aggregate timings logged for the caching decision (OQ-005-02).
- **Testability (Constitution IX):** each query handler is unit-tested; the three-role scope matrix is table-driven (each metric × each role against seeded data); filter-at-source is asserted (a project outside scope contributes to no count); the activity read is asserted to go **through `IActivityLogService`**; paging bounds tested; **a test asserts no write/audit occurs**. Frontend `DashboardService`, guard, and widget rendering via Jasmine+Karma.

### B.7 Audit event catalog — **intentionally empty**
> **This feature performs no write operations and therefore emits no `activity_logs` entries.** Constitution IV.4 requires an audit entry for every *write* to a domain entity; the Dashboard writes nothing, so it has nothing to audit, and its own reads are not audited (consistent with 001–004, which audit writes only). **This empty catalog is deliberate and is stated here so a reviewer does not flag a missing audit trail as an omission.** The activity the dashboard *displays* was audited by the features that performed those writes (001–004); the dashboard only reads it back.

### B.8 Definition of Done
1. Both `GET` endpoints exist and behave per the API-catalog status-code table; there are **no write endpoints** in the feature (verified).
2. The three-role scope matrix is proven by integration tests: an Admin's counts span all data; a ProjectManager's span only owned projects; a TeamMember's span only member-of projects — for every tile and the feed.
3. **Filter-at-source is proven**: a project/task/member/activity outside the caller's scope contributes to **no** aggregate, verified by inspecting the generated query or by a seeded negative test (never loaded, never counted).
4. The summary is a **stable typed contract**: every `ProjectStatus` and `TaskStatus` key is present with a count (zeros included); the response is not a free-form dictionary.
5. A caller with an **empty visible scope** receives **200** with zeroes/empties — never 403 or 404.
6. The activity feed is read **through `IActivityLogService`** (asserted), is paginated (`PagedResult<T>`, clamped `pageSize`, `400` on bad paging), and is scoped so non-Admins never see another team's entries.
7. The overdue count uses "due before today AND status ≠ `Done`" over the visible scope, with the date boundary and timezone assumption documented.
8. **No migration is added** by this feature; **no `xmin`**; and **no `activity_logs` entry is produced** by any dashboard call (asserted) — and this absence is documented (B.7) so it is not read as a defect.
9. The Angular `dashboard` route group is lazy-loaded with standalone components (no `@NgModule`); all HTTP lives in `DashboardService`; charts use Chart.js; a functional route guard is the only navigation block; the module refetches on navigation (SignalR-ready but not implemented).
10. Errors are RFC 7807 via the shared mapper; the OpenAPI contract for both endpoints is authored/reviewed under `/docs/contracts/` **before** the handlers and the code is validated against it (API-first, X.2), with Swagger UI enabled in development; backend compiles warnings-as-errors with nullable enabled; frontend compiles in strict mode.
11. The four clarified decisions (Clarifications 2026-07-22) are covered by tests: a TeamMember's task/overdue tiles are **personal-view** (assigned-to-them only); values are **live-computed** per request; the summary includes **completion rate** and **blocked-task count**; the activity feed defaults to **20** (max **100**) and shows **all visible entries**. OQ-005-05 (endpoint granularity) remains a low-priority refinement with no v1 action required.
12. Unit + integration tests pass (Constitution IX.3 — no merge on failing tests).

### B.9 Open questions for review
| # | Question | Recommendation (not yet a decision) | Status |
|---|---|---|---|
| OQ-005-01 | **Which additional metrics** beyond the baseline (completion rate, average time-to-close, most-active team member, blocked-task count, …) belong in the initial release, and their exact numeric bounds? | **Resolved (Clarifications 2026-07-22): baseline + completion rate + blocked-task count.** Both are cheap (derived from data already aggregated) and need no field 002/003 did not commit to; time-to-close and most-active-member are deferred to 006 Reports. | **Resolved** |
| OQ-005-02 | **Live-computed vs cached/materialized** values (freshness vs query cost as data grows); if cached, the refresh cadence and invalidation strategy? | **Resolved (Clarifications 2026-07-22): live per request for v1** — small dataset, always-fresh, no invalidation to design. A cache (fixed-cadence or event-driven) can be introduced later inside the query handlers without a contract change; revisit under real load. | **Resolved** |
| OQ-005-03 | **Activity feed** default page size, and whether it shows all visible audit entries or a filtered subset (e.g. mutation events only)? | **Resolved (Clarifications 2026-07-22): default 20, max 100, show all visible entries** (no subset filter; matches 002–004 paging). A filter can be added later if the feed proves noisy. | **Resolved** |
| OQ-005-04 | **TeamMember "tasks by status"** in the summary — the **whole visible project** (project-view) or **only their own assigned tasks** (personal-view)? | **Resolved (Clarifications 2026-07-22): personal-view** — the TeamMember task-by-status/overdue tiles count only tasks assigned to them (identical to US-005-03's slice); project-view may be a later secondary breakdown. | **Resolved** |
| OQ-005-05 | **Endpoint granularity** — keep summary as one typed payload, or later split into per-widget endpoints for independent lazy-load/caching? | Keep the committed shape (single typed summary + separate paginated activity). Because OQ-005-02 chose **live per request** (no caching), the main motivation to split the summary is gone — **no split planned for v1**; revisit only if a future cache makes per-widget invalidation worthwhile. | **Deferred (low priority — no v1 action)** |

---

## Functional Requirements

- **FR-001**: The system MUST expose read-only `GET /api/dashboard/summary` and `GET /api/dashboard/activity`; it MUST NOT expose any create/update/delete endpoint, and MUST introduce no new entity, table, or migration.
- **FR-002**: The summary MUST return a **stable typed contract** including project counts by 002's `ProjectStatus`, task counts by 003's `TaskStatus` (each enum value present with a count, zeros included), an overdue-task count, a **completion rate** (Done ÷ total tasks in scope, `0` when no tasks), a **blocked-task count**, a visible-project count, a visible-team-member count, and a TeamMember personal task slice (Clarifications 2026-07-22).
- **FR-003**: Every metric MUST be **role-scoped at the query source** via the reused `ApplyScope` predicates — Admin across all, ProjectManager across owned projects, TeamMember across member-of projects — and MUST NOT be produced by fetching all rows and filtering in memory.
- **FR-004**: The personal task slice MUST count only tasks whose `assignee_id` equals the caller, within the caller's member-of project set.
- **FR-005**: The overdue count MUST include tasks whose due date is before today and whose status is not the terminal `Done` state, within the caller's visible scope.
- **FR-006**: The activity feed MUST be read **through 001's `IActivityLogService`** (never by a direct `activity_logs` query), MUST be scoped to the caller's visible entities, MUST show all visible entries (no subset filter in v1), and MUST be paginated via `PagedResult<T>` with `?page`/`?pageSize` (default **20**, max **100**, clamped not rejected) per Constitution VI.4.
- **FR-007**: A caller with an empty visible scope MUST receive **200** with zero counts and empty lists — never **403** or **404**.
- **FR-008**: The summary tiles and the personal slice MUST NOT be paginated (fixed-N scalar/enum metrics); only the activity feed is paginated.
- **FR-009**: Role checks MUST be declared with `[Authorize]` attributes only; scope MUST be enforced in the slice handler via the reused `ApplyScope`, never in the controller; `CanMutateAsync` MUST NOT be used (there is no write path).
- **FR-010**: The feature MUST produce **no `activity_logs` entries** (it performs no writes); this absence MUST be documented so it is not mistaken for a missing audit trail (Constitution IV.4 applies to writes only).
- **FR-011**: Errors MUST be RFC 7807 Problem Details via the shared `ErrorKind` mapper; the OpenAPI contract for both endpoints MUST be authored and reviewed under `/docs/contracts/` **before** the handlers are implemented (API-first, Constitution X.2), with the code validated against the contract and Swagger UI enabled in development. The only error statuses are 400 (bad paging) and 401 (unauthenticated); there is no 403/404/409.
- **FR-012**: The Angular `dashboard` feature area MUST be lazy-loaded via route-level code splitting with standalone components (ADR-0001); all HTTP MUST live in a dedicated `DashboardService` (never in components); a functional route guard MUST be the only mechanism blocking navigation; charts MUST use Chart.js (Constitution III).
- **FR-013**: The architecture MUST NOT preclude adding real-time push (SignalR) later (Constitution II.4); the initial release refetches on navigation/poll.
- **FR-014**: All data access MUST go through EF Core (Constitution IV.1); aggregates are LINQ grouped queries with the scope predicate translated to SQL.

## Non-Functional Requirements
- **NFR-001**: Server-side enforcement is authoritative; the frontend never filters or gates for security.
- **NFR-002**: Every aggregate is computed in a single grouped query with the scope predicate pushed into SQL — **no N+1, no fetch-then-filter**; out-of-scope rows are never materialized.
- **NFR-003**: Structured logging (Serilog); slow-aggregate timings recorded to inform the live-vs-cached decision (OQ-005-02).
- **NFR-004**: Nullable reference types + warnings-as-errors (backend); TypeScript strict mode (frontend).
- **NFR-005**: The read model sits inside the dashboard query handlers, so switching from live computation to a cached/materialized source (OQ-005-02) is an internal change that does not alter the API contract.

## Security Rules
- Authenticated by default; role gate via attributes only; scope enforced in the query source.
- No write path — the feature cannot modify any record, and therefore adds no audit or integrity surface.
- Out-of-scope data is never loaded or counted; no leakage through totals or paging metadata.
- The activity feed is read only through `IActivityLogService` (001); identity from the token.

## Audit / Compliance Expectations
**The Dashboard produces no audit entries.** Constitution IV.4 requires auditing every *write* to a domain entity; this feature performs none, so it emits nothing to `activity_logs`, and — like 001–004 — it does not audit reads. The activity the dashboard displays was already audited by the feature that made each change. This is stated explicitly so the absence of an audit catalog is understood as correct, not missing.

## Assumptions
- 001–004 are implemented: `users`, the role model, the single JWT `role` claim, `ICurrentUserService`, `IProjectAccessPolicy.ApplyScope` (002), `ITaskAccessPolicy.ApplyScope` (003), and `team_members` (004) all exist and are consumed here.
- **The audit log is readable through its owning service.** 001's `IActivityLogService` exposes (or is extended with) a **scoped read** method returning audit entries filtered to a set of entity scopes and paginated. Because 001 owns `activity_logs` and its service, exposing a read is within 001's contract and is **not** a retroactive change to 002/003 (per the feature request's retroactive-note guidance, recorded here as an assumption rather than a cross-spec edit).
- **Overdue** uses 003's `due_date` and treats `Done` as the only terminal/completion status; "before today" is evaluated against a documented timezone assumption (server/UTC unless configured). Any additional field a richer metric might need (e.g. a task completion timestamp for time-to-close) that 002/003 did not commit to is an **assumption**, and such a metric is deferred to OQ-005-01 / 006 rather than forcing a schema change.
- **Team size** is reported as a **distinct visible-team-member headcount** across the caller's visible projects (a user on several visible projects counts once) rather than per-project rosters — the dashboard is a summary, and per-project rosters live on 004's team screen. (Stated as the chosen interpretation of the brief's "team size".)
- A project team and the metric set are small; the summary is computed **live per request** for v1 (resolved, Clarifications 2026-07-22), which keeps it always-fresh and simplest; a cache is deferred inside the query handlers.

## Dependencies
- **Depends on**: [001 Auth & RBAC](../001-auth-rbac/spec.md) (Users, role model, JWT, `ICurrentUserService`, `IActivityLogService`) · [002 Projects](../002-projects/spec.md) (Project entity, ownership scope via `ApplyScope`, project status enum) · [003 Tasks](../003-tasks/spec.md) (Task entity, status enum, due date, assignee) · [004 Team](../004-team/spec.md) (membership records — a TeamMember's visible projects are exactly those 004's rows place them on). All four are **referenced, not redefined**.
- **Consumed by**: 006 Reports inherits this read-only, role-scoped aggregation posture and adds export + historical windows.
- **No retroactive changes**: 001–004 are not modified to support the dashboard. The one cross-feature need — a scoped **read** on the 001-owned audit service — is recorded as an Assumption within 001's ownership, not as an edit to 002/003.
- **Infrastructure**: PostgreSQL 18 via EF Core 10 + Npgsql; MediatR (query dispatch for vertical slices); Serilog; OpenAPI contract under `/docs/contracts/` + Swagger UI (dev); Chart.js.

## Out of Scope
Any write operation, including a "mark activity as read" affordance (a separate spec if ever needed); user-customizable dashboards (widget layout, pinning, saving, rearranging); real-time push via SignalR (must remain possible — Constitution II.4 — but not built now); export to PDF/CSV (owned by 006 Reports, Constitution VII.8); historical time-series and long-window trend analytics (006); notifications and threshold alerts (bonus, brief); any new entity, table, or migration.

---

## Sequence Note

This is the **fifth** module in the sequence (001 Auth & RBAC, 002 Projects, 003 Tasks, 004 Team complete). It follows the structural template set by [001](../001-auth-rbac/spec.md)/[004](../004-team/spec.md) and the merged-file convention, and it is the first **strictly read-only** feature — establishing the role-scoped aggregation posture that **006 Reports** inherits and extends with export (jsPDF for PDF, a lightweight CSV utility, per Constitution III) and heavier historical-window query complexity.
