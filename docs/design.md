# UI/UX Design Specification

**Governed by**: Project Constitution v1.4.0 · **Downstream of**: `constitution.md`, and every `specs/00X-*/{spec,plan,tasks}.md` (001 Auth & RBAC, 002 Projects, 003 Tasks, 004 Team, 005 Dashboard, 006 Reports), all of which are final and authoritative
**Status**: This file describes the **visual and UX layer** over decisions already locked in by the specs/plans/tasks. It introduces no component, route, field, endpoint, or role rule that those files do not already name. Where a concrete detail is required (a component's name, a folder, a field), it is cited to its source rather than invented.

---

## 1. Visual Theme & Color Palette

Constitution III commits the UI component library, and this palette is constrained to **blue, grey, and white only** — a professional, low-noise palette appropriate to an internal project-management tool, with semantic accents reserved for status communication.

### Palette

| Token | Hex | Usage |
|---|---|---|
| **Primary — Blue 700** | `#1565C0` | App bar, primary buttons, active nav item, links, focus rings |
| **Primary — Blue 500** | `#1E88E5` | Hover states, secondary emphasis, chart primary series |
| **Primary — Blue 100** | `#BBDEFB` | Selected-row highlight, chip backgrounds, light accents |
| **Secondary — Slate Grey 700** | `#37474F` | Secondary buttons, headings on light surfaces, sidebar background (dark variant) |
| **Secondary — Grey 500** | `#78909C` | Secondary text, icons, disabled-adjacent labels |
| **Neutral — Grey 200** | `#ECEFF1` | Card/table zebra striping, dividers, input backgrounds |
| **Neutral — Grey 50** | `#FAFAFA` | Page background |
| **White** | `#FFFFFF` | Surfaces: cards, dialogs, the app bar's contrasting text, table rows |
| **Text — Grey 900** | `#212121` | Primary body text |
| **Text — Grey 600** | `#616161` | Secondary/meta text (timestamps, helper text) |

### Semantic colors (status communication only — never used as a primary/secondary substitute)

| Token | Hex | Usage |
|---|---|---|
| **Success** | `#2E7D32` (green 800) | `Completed`/`Done` status chips, 2xx confirmation toasts, success snackbar |
| **Warning** | `#F9A825` (amber 800) | `OnHold`/`Blocked` status chips, overdue badges, approaching-limit banners |
| **Error** | `#C62828` (red 800) | Validation errors, 4xx/5xx toasts, destructive-action confirmation, the shared error-display component (Constitution VII.6) |
| **Info** | `#0277BD` (light-blue 800) | Informational banners (e.g. role-scope explainer), neutral system notices |

**Rule**: semantic colors appear only on status chips, badges, form validation, and the notification/toast component (Constitution VII.7) — never as a page's dominant color. The interface otherwise reads as blue-on-grey-on-white at every screen.

---

## 2. UI Component Library

**Confirmed, not chosen here.** Constitution III states Angular Material is the default and Bootstrap an alternative that must not be mixed with it; **[001's plan.md](../specs/001-auth-rbac/plan.md) §Constitution Check, row III**, states Material is committed by that plan: *"Angular Material (III's default, committed by this plan)"* — and Follow-up 4 of the same plan records that once 002–006 build on Material, introducing Bootstrap would require a constitution amendment. Every subsequent plan (002 §Primary Dependencies, 003, 004, 005, 006) lists Angular Material among inherited, unchanged dependencies. This file therefore treats Angular Material as decided, not optional.

### Theming approach

Material's theming system is customized via **one custom Material theme file**, not per-component style overrides (per Constitution VIII's naming/consistency intent and to avoid the drift a scattered-override approach invites):

- A single `styles/_theme.scss` (or `theme.scss`, wired through Angular's `@use '@angular/material' as mat;` API) defines:
  - A custom **primary palette** built from Blue 700/500/100 (§1) via `mat.define-palette` (M2) or an equivalent M3 custom palette JSON.
  - A custom **secondary/tertiary palette** from Slate Grey 700/500.
  - A **neutral surface palette** using Grey 50/200/White for backgrounds, cards, and dividers.
  - Semantic colors (success/warning/error/info, §1) are declared as **SCSS custom properties layered on top of the Material theme**, since Material's own palette system has no native "warning"/"info" slot — status chips and the notification component consume these tokens directly rather than reusing Material's `warn` palette (which is reserved for destructive-action UI only, per Material convention).
- `mat.core()` is included once; `mat.all-component-themes($project-theme)` applies the palette globally, so no component in `features/*` sets its own Material color inputs.
- Dark-mode is out of scope for v1 (not requested by any spec); the theme file is structured so a dark palette variant could be added later without touching component templates — consistent with Constitution I.2's "do not build in a way that would require rewriting later" posture, applied here by convention rather than by a cited requirement.

---

## 3. Component & Route Inventory

This is the traceability bridge: every entry below is named in a spec's UI subsection (`F0XX-SXX`) and/or scaffolded at the cited task/plan path. Every layout in §4 references only entries from this table.

**Route-path note** (see §9 Gaps): specs and plans fix the **lazy-loaded feature-area prefix** (`auth`, `projects`, `tasks`, `team`, `dashboard`, `reports`) registered via `loadChildren` in `app.routes.ts`, and fix each component's **folder name** within that area. No spec/plan/tasks file fixes the literal sub-path segment (e.g. whether the create screen resolves at `/projects/create` or `/projects/new`). The table below states the **feature-area route group** and the **component folder** as sourced; it does not assert a specific sub-path string.

### 001 — Auth (route group: `auth`, lazy-loaded)

| Component | Spec ID | Folder (source) | Purpose |
|---|---|---|---|
| Register | F001-S01 Register | `features/auth/register/` (001 tasks.md T062) | Self-registration form → always `TeamMember` |
| Login | F001-S02 Login | `features/auth/login/` (001 tasks.md T075) | Credential entry → token pair |

**Cross-cutting, not routed**: logout control in the app shell (`core/`, 001 spec US-001-03 C); JWT + 401 interceptors (`core/interceptors/`); functional route/role guards (`core/guards/`); shared `error-display` and `notification` components (`shared/`, Constitution VII.6/VII.7).

### 002 — Projects (route group: `projects`)

| Component | Spec ID | Folder (source) | Purpose |
|---|---|---|---|
| Project List | F002-S01 Project List | `features/projects/list/` (002 tasks.md T024, T046) | Role-scoped, searchable, paginated |
| Create Project | F002-S02 Create Project | `features/projects/create/` (002 tasks.md T034) | Reactive form |
| Project Detail | F002-S03 Project Detail | `features/projects/detail/` (002 tasks.md T055) | Read-only view + Edit/Delete actions |
| Edit Project | F002-S04 Edit Project | `features/projects/edit/` (002 tasks.md T066) | Pre-populated form |

### 003 — Tasks (route group: `tasks`)

| Component | Spec ID | Folder (source) | Purpose |
|---|---|---|---|
| Task List | F003-S01 Task List | `features/tasks/list/` (003 tasks.md T025, T048) | Role-scoped, filterable, paginated |
| Create Task | F003-S02 Create Task | `features/tasks/create/` (003 tasks.md T035) | Reactive form, project-scoped assignee picker |
| Task Detail | F003-S03 Task Detail | `features/tasks/detail/` (003 tasks.md T055, T096) | Full detail + status control + assignee picker |
| Edit Task | F003-S04 Edit Task | `features/tasks/edit/` (003 tasks.md T066) | Full-edit form (not reachable by TeamMember) |
| Status control | (part of F003-S03/list) | inline in `detail/` and `list/` (003 tasks.md T077) | The one write control available to a TeamMember |

### 004 — Team (route group: `team`)

| Component | Spec ID | Folder (source) | Purpose |
|---|---|---|---|
| Project Team (roster) | F004-S01 Project Team | `features/team/roster/` (004 tasks.md T020, T040) | Roster table, remove action |
| Add Team Member | F004-S02 Add Team Member | `features/team/add-member-dialog/` (004 tasks.md T020, T031) | Dialog, searchable user picker |

**No independent top-level entry point — resolved.** Unlike the other five route groups, `team` has no standalone nav link; it is reached via a **Team tab/section on Project Detail (F002-S03)**, per 002 tasks.md T055 (now extended: "a Team tab/section linking to `features/team/roster/` for the current project"). The roster's reciprocal navigation back to its parent project is covered by 004 tasks.md T040 (now extended: "a 'back to project' link to the parent Project Detail").

### 005 — Dashboard (route group: `dashboard`)

| Component | Spec ID | Folder (source) | Purpose |
|---|---|---|---|
| Dashboard Summary | F005-S01 Dashboard Summary | `features/dashboard/summary/` (005 tasks.md T018, T028) | Fixed-N tiles + charts + "My Work" panel (T047) |
| Activity Feed | F005-S02 Activity Feed | `features/dashboard/activity-feed/` (005 tasks.md T018, T039) | Paginated feed widget |

### 006 — Reports (route group: `reports`)

| Component | Spec ID | Folder (source) | Purpose |
|---|---|---|---|
| Report Picker | F006-S01 Report Picker | `features/reports/picker/` (006 tasks.md T022, T030) | Catalog-driven dynamic parameter form |
| Project Progress | F006-S02 Project Progress | `features/reports/project-progress/` (006 tasks.md T041) | Chart + client-paginated table + export |
| Task Completion | F006-S03 Task Completion | `features/reports/task-completion/` (006 tasks.md T052) | Trend chart + bucket table + export |
| Team Performance | F006-S04 Team Performance | `features/reports/team-performance/` (006 tasks.md T062) | Bar comparison (Admin/PM) or single-member card (TeamMember) + export |
| Activity Report | F006-S05 Activity Report | `features/reports/activity/` (006 tasks.md T072) | Filter form + paginated table + 422 prompt + export |

**Cross-cutting, not routed**: `ReportExportService` (`core/services/report-export.service.ts`, 006 plan.md §Project Structure) — the single jsPDF/papaparse service backing every export control, per Constitution VII.8.

**Total inventoried screens/surfaces**: 2 (Auth) + 4 (Projects) + 4 (Tasks) + 2 (Team) + 2 (Dashboard) + 5 (Reports) = **19**, matching the per-feature "UI screens" counts stated in each plan.md's Technical Context (001: 2; 002: 4; 003: 4; 004: 2 surfaces; 005: 2 surfaces; 006: 5), plus the app-shell cross-cutting elements listed above.

---

## 4. Module Layouts

Every field or metric named below is cited to its source section. Layouts use only the components from §3.

### Dashboard (005) — `features/dashboard/`

**Dashboard Summary** (F005-S01) renders as a fixed grid of tiles, not a scrollable list (005 spec T.6: "summary tiles are fixed-N scalar/enum metrics and are deliberately not paginated"):

- **Headline tile row**: `visibleProjectCount`, `overdueTaskCount`, `completionRate`, `blockedTaskCount` — all four are top-level fields of `DashboardSummaryDto` (005 spec §Consolidated Read Model, and B.3's DTO shape).
- **Projects-by-status tile**: one segment per `ProjectStatus` value, zero-seeded (005 spec §Consolidated Read Model — `projectsByStatus` is "a stable typed contract... zeros included"), rendered as a small Chart.js donut (§6 below).
- **Tasks-by-status tile**: one segment per `TaskStatus` value, same zero-seeding rule (`tasksByStatus`), Chart.js donut.
- **Team size tile**: `visibleTeamMemberCount` (005 spec §Consolidated Read Model).
- **"My Work" panel** (US-005-03; scaffolded inside `summary/` per 005 tasks.md T047, not a separate route): `personalTasks.assignedTotal`, `personalTasks.byStatus`, `personalTasks.overdueCount` — shown for every role but described in the spec as "TeamMember-prominent" (005 spec C, US-005-03); for Admin/ProjectManager it is typically empty (005 spec Edge cases, US-005-03).
- Every tile is **read-only** — 005 spec T.1 states this feature has no write path; no tile carries an action button beyond a link into 003's task list (US-005-03 C).

**Activity Feed** (F005-S02) is a separate scrollable widget below the tile grid, backed by `PagedResult<ActivityEntryDto>` (005 spec §Consolidated API Catalog): each row shows `actorName`, `action`, `entityType`/`entityId`, a relative `timestamp`, and `changeSummary` (005 spec §Consolidated Read Model, `ActivityEntryDto`).

### Projects (002) — `features/projects/`

- **Project List** (F002-S01): a table with a search box, a status filter, sort, and a paginator, over `PagedResult<ProjectSummaryDto>` (002 spec §Consolidated API Catalog, US-002-02 D). Rendered columns are drawn from the summary DTO fields shown in the technical design's list example (002 spec T.3(2)): `name`, `status`, `startDate`, `endDate`, `owner.fullName`. The "New Project" action is hidden for TeamMember (UX only —002 spec C, US-002-02).
- **Create Project** (F002-S02): a Reactive Form with `name`, `description`, `startDate`, `endDate`, `status` (002 spec C, US-002-01) and a date-order cross-field validator; the owner field is shown only to Admin (002 spec C, US-002-01).
- **Project Detail** (F002-S03): every field from the Project entity — `name`, `description`, `start_date`/`end_date`, `status`, `owner`, `created_at`/`updated_at` (002 spec §Consolidated Data Model; C, US-002-03) — with Edit/Delete actions rendered only for permitted roles (UX only).
- **Edit Project** (F002-S04): the same field set as Create, pre-populated, with the owner field editable only for Admin (002 spec C, US-002-04) and an unsaved-changes guard; on a **409** response the user is shown a reload-and-reapply prompt (002 spec T.7).

### Tasks (003) — `features/tasks/`

- **Task List** (F003-S01): search, status/priority/assignee filters, sort, paginator, over `PagedResult<TaskSummaryDto>` (003 spec §Consolidated API Catalog; C, US-003-02). "New Task" is hidden for TeamMember (UX only).
- **Create Task** (F003-S02): `title`, `description`, `priority`, `dueDate`, `assignee` — the assignee picker is limited to the parent project's team members (003 spec C, US-003-01), with a due-date-within-project-window validator.
- **Task Detail** (F003-S03): `title`, `description`, `status`, `priority`, `dueDate`, parent project, `assignee`, timestamps (003 spec C, US-003-03). Edit/Reassign/Delete render only for permitted roles; **for a TeamMember, only the status control is enabled** — the visual expression of the graduated `TaskMutation` model (003 spec T.2, C).
- **Edit Task** (F003-S04): same field set as Create minus assignee, pre-populated, with an unsaved-changes guard; not reachable for TeamMember via navigation (guard), and refused by the API regardless (003 spec C, US-003-04).
- **Status control**: a dropdown or a drag-between-columns board (003 spec C, US-003-05), present on both the detail view and inline in the list row. This is the single write control a TeamMember sees anywhere in the product (003 spec C, US-003-05: "the only enabled write control on the screen").
- **Assignee picker** (on Task Detail): limited to the parent project's team pool, sourced from 004's roster endpoint (003 spec C, US-003-07; 003 tasks.md T096); not rendered for TeamMember.

### Team (004) — `features/team/`

- **Project Team / roster** (F004-S01): a table with columns member name, email, **global role** (a read-only reflection, not a per-project role — 004 spec T.2), added-at, and a remove action rendered only for Admin/owner (004 spec C, US-004-02). Search/filter is **client-side** over the bounded list — 004's roster is deliberately unpaged (004 spec T.6: "the rule targets collections that can exceed 50 items… a project team is bounded"). On a **409** remove attempt, the confirmation dialog surfaces the blocking-tasks message and points the manager to reassign or close them first (004 spec C, US-004-03). **Reached via a Team tab/section on Project Detail** (F002-S03, 002 tasks.md T055) rather than a standalone top-level nav item — consistent with every 004 endpoint being nested under `/projects/{projectId}/team` (004 spec §Consolidated API Catalog); a "back to project" link (004 tasks.md T040) provides the reciprocal navigation.
- **Add Team Member** (F004-S02, a dialog per 004 tasks.md T020/T031, not a full page): a single searchable user picker over **any active user regardless of global role** (004 spec C, US-004-01, Clarifications 2026-07-22).

### Reports (006) — `features/reports/`

- **Report Picker** (F006-S01): the parameter form is **built dynamically from `GET /api/reports/catalog`** (006 spec C, US-006-01) — the picker's fields are not hard-coded per report type but derived from each descriptor's `parameters` array (006 spec T.3(1)).
- **Project Progress** (F006-S02): a parameter form (window, `projectScope`) + a Chart.js progress visualization + a table over `ProjectProgressReport.rows[]`, whose columns are `projectId`/`projectName`, `status`, `totalTasks`/`openTasks`/`closedTasks`, `overdueTasks`, `completionPercent`, `projectedCompletion` (006 spec §Consolidated Read Model field table). The table paginates **client-side**, since the row set is bounded to the caller's visible projects (006 spec T.6).
- **Task Completion** (F006-S03): window + `groupBy` (day/week/month) + optional project/assignee filters, a Chart.js line/bar trend, and a per-bucket table over `buckets[]` (`periodStart`, `periodLabel`, `completedCount` — 006 spec field table). The series is zero-filled and continuous (006 spec §Consolidated Read Model).
- **Team Performance** (F006-S04): window + project scope (Admin/PM) or a fixed self-view (TeamMember); a Chart.js bar comparison for Admin/PM or a **single-member card** for TeamMember, over `rows[]` (`userId`/`fullName`, `isActive`, `throughput`, `workload`, `overdueCount` — 006 spec field table). A TeamMember's table/card always renders **exactly one row** (006 spec Given/When/Then, US-006-04 scenario 3).
- **Activity Report** (F006-S05): window + `entityType`/`actorId`/`projectId` filters, a paginated table over `PagedResult<ActivityReportRow>` (`timestamp`, `actorName`, `action`, `entityType`/`entityId`, `changeSummary` — 006 spec field table), and a "narrow your range" prompt shown on **422** (006 spec C, US-006-05) — displayed before any export render is attempted (006 tasks.md T077).
- **Export controls**: present on all four report views (not the picker), calling the shared `ReportExportService` — never a per-component implementation (006 spec C, US-006-06; Constitution VII.8). A busy state prevents double-clicks during a large render (006 spec Edge cases, US-006-06).

---

## 5. Role-Based Dashboards

Data-level scoping is defined by 001's role model and 002/003's `ApplyScope`/`CanReadAsync`/`CanMutateAsync` policies, and by 004's binary `ITeamAccessPolicy` and 005/006's read-only reuse of the same predicates. This section covers **only** the visual/UX expression of that scoping — no new access rule is introduced.

### Navigation

- The main nav (app shell, `core/`) renders the same **five** top-level route groups (Dashboard, Projects, Tasks, Reports, plus the Auth-owned logout control) for every role — **navigation itself is not role-filtered**, because 002 spec's Access Logic states scope shapes *content*, and 002/003's out-of-scope convention is a **403 at the resource**, not a hidden nav item; hiding entry points that the API would legitimately serve empty/scoped results for would contradict 002 US-002-02's "empty list, not an error" behavior for a TeamMember with no assignments. **Team is deliberately not among them** — every 004 endpoint is nested under a specific project (`/projects/{projectId}/team`), so a standalone top-level "Team" link would have nothing to point at without a project already selected. Instead, Team is reached via a **Team tab/section on Project Detail** (F002-S03, 002 tasks.md T055), with a reciprocal "back to project" link on the roster itself (004 tasks.md T040) — no project-picker screen is introduced.
- **Write-only actions are hidden per role**, consistently with each spec's own "(UX only — the API still enforces …)" annotations:
  - "New Project" hidden for TeamMember (002 spec C, US-002-02).
  - "New Task" hidden for TeamMember (003 spec C, US-003-02).
  - Task Edit/Reassign/Delete hidden for TeamMember; only the status control renders (003 spec C, US-003-03/US-003-05).
  - "Add member" and the roster's remove action hidden for TeamMember; remove hidden for a non-owning ProjectManager (004 spec C, US-004-01/US-004-02/US-004-03).
  - Project Edit/Delete hidden for non-owning ProjectManager and TeamMember (002 spec C, US-002-03/US-002-05).

### Badges and banners indicating scope

- **Dashboard Summary** carries a `scope` label from `DashboardSummaryDto.scope` (005 spec §Consolidated Read Model — literally the caller's role string, e.g. `"ProjectManager"`) and each report's envelope carries the same `scope` field (006 spec §Consolidated Read Model, shared envelope header). Rendering this value as a small header badge ("Showing: your owned projects" / "Showing: your assigned work" / "Showing: all projects") is the natural expression of a field both 005 and 006 already return for exactly this purpose — no new field is introduced.
- **Team Performance for TeamMember** (F006-S04) is the sharpest visual case: the layout switches from a comparison bar chart to a **single-member card**, which is itself the least-privilege boundary made visible (006 spec T.2, "Role & Permission Model": "never a peer comparison"). No banner is needed here — the single-row layout *is* the signal.
- **Roster's own-role column** (F004-S01) displays each member's **global** role read-only, explicitly labeled as such in the spec (004 spec T.2: "a read-only reflection... for display") — this is the one place a role badge appears next to another user, and it is descriptive, not a scope indicator.

### What is deliberately not built

Per-role dashboard *layouts* beyond the differences above (e.g., a wholly different widget set per role, or a role-specific theme) are not specified anywhere in 001–006 and are not introduced here — 005 spec's Role & Permission Model table (§Role & Permission Model) states role determines *which slice of data is aggregated*, "never what actions are available," so the same Dashboard Summary template renders for all three roles with role-scoped data filled in.

---

## 6. Data Visualization

Chart.js is Constitution III's default charting library and is confirmed as the choice in use by 005's plan.md (§Primary Dependencies: *"Chart.js (Constitution III) is used for the status charts — first feature to need it"*) and 006's plan.md (§Primary Dependencies: *"Frontend adds jsPDF and papaparse... plus Chart.js already introduced by 005"*).

| Chart | Module | Data | Type | Source |
|---|---|---|---|---|
| Projects-by-status | 005 Dashboard Summary | `DashboardSummaryDto.projectsByStatus` | Donut/segmented (small, per §4) | 005 spec C, US-005-01; §Consolidated Read Model |
| Tasks-by-status | 005 Dashboard Summary | `DashboardSummaryDto.tasksByStatus` | Donut/segmented | 005 spec C, US-005-01 |
| Project Progress visualization | 006 Project Progress | `ProjectProgressReport.rows[]` (`completionPercent` per project) | Progress/bar visualization | 006 spec C, US-006-02 ("a Chart.js progress visualization") |
| Task Completion trend | 006 Task Completion | `TaskCompletionReport.buckets[]` | Line/bar trend | 006 spec C, US-006-03 ("a Chart.js line/bar trend") |
| Team Performance comparison | 006 Team Performance (Admin/PM only) | `TeamPerformanceReport.rows[]` | Bar comparison | 006 spec C, US-006-04 ("a Chart.js bar comparison") |

**Styling**: chart series use the Blue 500/700 primary tokens (§1) for the dominant series, with semantic colors (§1) reserved for status-coded segments (e.g. a `Blocked` segment in amber, a `Done`/`Completed` segment in green) — consistent with the constraint that blue/grey/white remain the base palette and semantic colors mark status, not decoration.

**D3 escape hatch**: Constitution III permits D3.js only "if a chart requires capabilities Chart.js cannot provide." No report or dashboard spec across 001–006 invokes this escape hatch — every chart named above (§4, this table) is explicitly specified as a Chart.js chart, and no D3-specific requirement (e.g., custom force-directed layouts, non-standard chart geometries) appears anywhere in 005 or 006. **006 uses no D3.**

---

## 7. Empty, Loading, and Error States

These are not separately documented elsewhere in 001–006, but every module's spec/tasks explicitly calls for them, so each maps onto the components inventoried in §3 rather than introducing new ones.

| Module / component | Empty state | Loading state | Error state | Source |
|---|---|---|---|---|
| 002 Project List | "No projects match your filters" / no projects at all | skeleton/spinner while fetching | inline via the shared error-display component | 002 spec C, US-002-02: "Empty, loading, and error states are explicit" |
| 002 Project Detail | n/a (always resolves to a project or a denial) | spinner | not-found (404) / forbidden (403) states, explicit | 002 spec C, US-002-03: "Loading/error/not-found/forbidden states are explicit" |
| 003 Task List | empty result set, not an error, for a TeamMember with no assignments | spinner | inline error | 003 spec C, US-003-02; Given/When/Then scenario 3 ("empty list, not an error") |
| 003 Task Detail | n/a | spinner | not-found/forbidden, explicit | 003 spec C, US-003-03 |
| 004 Roster | empty array rendered plainly (a project with no members), not a 404 | spinner | forbidden state, explicit | 004 spec C/D, US-004-02: "empty, loading, error, and forbidden states are explicit"; Given/When/Then scenario 4 |
| 005 Dashboard Summary | "nothing assigned to you yet" for a zero-scope caller — **always 200, never an error state for empty scope** | spinner | inline error (network/5xx only — the API itself never 403/404s here) | 005 spec C, US-005-01; T.7: "no 403 for the dashboard itself... a caller with an empty visible set gets a valid 200"; 005 tasks.md T028 |
| 005 Activity Feed | empty page, not a 404 | spinner/skeleton rows | inline error | 005 spec C, US-005-02: "Empty, loading, and error states explicit" |
| 006 Report Picker | n/a (catalog is a fixed set) | spinner while the catalog loads | inline error | 006 spec C, US-006-01: "Loading/error states explicit" |
| 006 Activity Report | empty page for a zero-activity window | spinner | **the 422 "narrow your range" prompt is a distinct third state**, shown before any export render | 006 spec C, US-006-05; T.7 |
| Cross-cutting | — | — | uncaught API/5xx errors funnel through the global `ErrorInterceptor` + Angular `ErrorHandler` to the shared notification (snackbar/toast) component | Constitution VII.7; cited identically in every plan's Constitution Check (VII.7 row) |

**Convention**: "forbidden" (403) is rendered as its own explicit state distinct from a generic error, wherever a spec calls it out (002 Detail, 004 roster) — it is not folded into the generic error-display path, since a 403 in this app is frequently a deliberate, expected outcome (e.g., a ProjectManager opening a colleague's project) rather than a failure.

---

## 8. Responsive Behavior

No spec, plan, or tasks file across 001–006 states a device or breakpoint target — this is a genuine gap (flagged again in §9) that a UI specification must still resolve to be complete. The following is this file's own design decision, not sourced from any spec, and does not alter any component, route, or field named above.

**Breakpoints** (Angular Material's CDK Layout / BreakpointObserver conventions, chosen for consistency with the already-committed Material library, §2):

| Breakpoint | Width | Layout behavior |
|---|---|---|
| **Handset** | < 599px | Single-column stacked layout; the app-shell nav collapses to a Material `mat-sidenav` opened via a hamburger toggle; data tables (Project/Task List, rosters, report tables) switch to a **card-per-row** layout instead of a horizontal table; Dashboard tiles stack to one column; charts scale to full container width and reduce to essential series labels. |
| **Tablet** | 600–959px | Two-column layout where applicable (e.g., Dashboard tiles in a 2-column grid); nav remains a collapsible sidenav; tables remain tabular but drop lower-priority columns (e.g., `updated_at` timestamps) behind an expandable row. |
| **Desktop** | ≥ 960px | Full layout as described in §4: persistent left/top nav, multi-column tile grids, full-width data tables with all columns, side-by-side parameter form + chart on report views. |

**Rules applied uniformly**:
- Every table inventoried in §3/§4 (Project/Task List, Roster, all four Report tables, the Activity Feed) is wrapped in a horizontally scrollable container at Tablet and below, per Material's standard `mat-table` responsive pattern, so no layout requires horizontal page scroll.
- Dialogs (Add Team Member, delete/remove confirmations) render as full-screen sheets on Handset and centered modals from Tablet up — a standard Material `MatDialog` responsive configuration, not a custom component.
- Charts (§6) are sized to their container (`responsive: true` in Chart.js configuration) rather than fixed pixel dimensions, so the same chart component serves all three breakpoints without a mobile-specific variant.

---

## 9. Traceability & Gaps

### Traceability confirmation

Every module layout in §4 (Dashboard, Projects, Tasks, Team, Reports) cites at least one `specs/00X/{spec,plan,tasks}.md` source per field, metric, or component named. §3's inventory maps 1:1 onto the `F0XX-SXX` identifiers and task-scaffolding paths across all six features, with no component invented beyond what 001–006 already scaffold. §6's chart list matches every chart explicitly named in 005 and 006's spec.md UI subsections. No Gantt chart, advanced-search/filter UI, notification center, or role-based-dashboard-as-a-distinct-product-surface was described, consistent with Constitution I.2 and each spec's own Out of Scope section — because no such component exists in any plan.md or tasks.md to describe.

### Step 4 check — cross-file consistency (performed before writing this file)

Component naming was cross-checked between each spec.md's `F0XX-SXX` UI subsections and the corresponding tasks.md scaffolding tasks (T-numbers cited in §3). **No inconsistency was found**: every `F0XX-SXX` component maps cleanly onto exactly one tasks.md folder path (e.g., F004-S01 "Project Team" ↔ `features/team/roster/`; F004-S02 "Add Team Member" ↔ `features/team/add-member-dialog/`). Nothing required flagging or stopping under this file's step 4 instruction.

### Step 5 gaps — described without inventing backing content

1. **Literal route path segments are undefined.** Every spec/plan/tasks file fixes a feature-area's lazy-load prefix (`auth`, `projects`, `tasks`, `team`, `dashboard`, `reports`) and each component's **folder name** within it, but no file states the literal path string for sub-routes (e.g., whether Create Project resolves at `/projects/create`, `/projects/new`, or is a dialog rather than a route at all — note 004's "Add Team Member" is explicitly a dialog per its tasks.md folder name `add-member-dialog/`, while 002/003's Create/Edit are folders without an equivalent "dialog" qualifier, implying full routes, but this is inferred from naming convention, not stated). **This needs human resolution** — specifically, a decision on the Angular route-path scheme (e.g., `/projects/:id/edit` vs. `/projects/edit/:id`) before routing code is implemented.
2. **Responsive breakpoints and device targets (§8) are wholly this file's own decision.** No spec, plan, or tasks file across 001–006 mentions a breakpoint, a target device class, or a mobile-vs-desktop layout rule. §8's breakpoints are a reasonable default given the committed Material library, but **they were not derived from any spec and should be confirmed** (or overridden) by whoever owns the demo/delivery target, since the brief's device-support expectations are not stated in this document's source material either.
3. **Dashboard/report chart-series color mapping to status values (§6's "amber for Blocked, green for Done") is this file's own styling decision**, not sourced from any spec — the specs fix which metrics are charted and via which library, not per-segment color. This is a low-risk default (it reuses §1's already-cited semantic tokens) but is called out for completeness, since the task's instruction is to flag, not silently invent, backing content beyond what a spec defines.

No other gaps were found: every visual claim in §1–§7 traces to a specific constitution principle, spec section, plan section, or tasks.md task ID cited inline.
