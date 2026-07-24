# Feature Specification: Task Management

**Feature Number**: 003
**Feature Name**: Task Management (Tasks CRUD, Assignment & Status Workflow)
**Phase**: Phase 1 — Foundation
**Priority**: Must Have (Critical)
**Complexity**: High
**Type**: Core domain / CRUD + graduated resource-level authorization
**Depends On**: **001 Auth & RBAC** (Users, role model, JWT claims, `CurrentUser`, `IActivityLogService`) · **002 Projects** (the `Project` entity and its ownership rule) — both **referenced, not redefined**
**Enables**: 005 Dashboard · 006 Reports (both aggregate Tasks)
**Created**: 2026-07-22
**Status**: Draft — Ready for Planning
**Governed By**: Project Constitution v1.1.1 (Principles II Architecture, III Stack, IV Data Access, V Security & Authorization, VI API Design, VII Frontend, VIII Code Quality, IX Testing)
**Cross-cutting contracts**: [docs/shared-contracts.md](../../docs/shared-contracts.md) — `Result<T>`, `ErrorKind`, `CurrentUser`, `AccessDecision`, `PagedResult<T>`, error→HTTP mapping · ADRs [0001](../../docs/adr/0001-angular-standalone-components.md) standalone Angular · [0002](../../docs/adr/0002-same-origin-hosting.md) same-origin hosting · [0003](../../docs/adr/0003-result-error-contract.md) error contract · [0004](../../docs/adr/0004-optimistic-concurrency.md) concurrency · [0005](../../docs/adr/0005-mapping-and-validation.md) mapping/validation
**Generated Via**: `/speckit.specify` (merged requirements + solution design, per project convention)

---

## Purpose

Own the **Task** — the unit of actual work in ProjectManagementApp. This feature provides task lifecycle management within a project (create, list/search, view, edit, update status, reassign, delete) and introduces the pattern the rest of the domain needs: **graduated authorization**, where the answer is not merely *may this user touch this row* but *how much of this row may they change*.

A TeamMember may move their own task's status but may not retitle it, reassign it, or delete it. That distinction cannot be expressed by a role attribute or by a simple yes/no ownership check — it is resolved inside `CanMutateAsync` by the kind of mutation being attempted.

This is a **merged specification**: requirements (Purpose → User Stories → Functional Requirements) and solution design (Technical Design → Implementation Blueprint) live in one file by project convention, so the team reviews *what* and *how* together.

## Business Value

Tasks are where the product delivers its value — projects are containers, tasks are the work. This feature makes a ProjectManager's plan executable (break a project down, assign it, track it) and gives a TeamMember a focused, safe surface: they see exactly the work assigned to them and can report progress on it without any risk of altering scope, ownership, or someone else's assignment. Every write is audited, so who changed what and when is answerable. The Dashboard's statistics and the Reports module's exports both read from this entity, so its status and assignment data is the foundation of every downstream metric.

## Actors

**Primary Actors**
- **Admin** — full CRUD on **any** task in **any** project, regardless of project ownership or assignment.
- **ProjectManager** — full CRUD on tasks within **projects they own**: create, edit, assign, reassign, change status, delete. Ownership is resolved through feature 002's `Project.owner_id`.
- **TeamMember** — **read-only** on tasks assigned to them, **plus** the ability to update the **status** of their own assigned tasks. Cannot create, delete, reassign, or edit any other field.

**Secondary Actors**
- **Consuming features (non-actor)** — 005 Dashboard (counts/aggregates by status, assignee, project) and 006 Reports (task exports) read this entity and inherit its scoping rules.

## Scope Summary

**In scope**: the `Task` entity (title, description, status, priority, due date, project FK, assignee FK) and its Code-First migration; the task endpoints under `/api/projects/{projectId}/tasks` and `/api/tasks/{id}`; role-scoped listing (Admin → all, ProjectManager → tasks in owned projects, TeamMember → tasks assigned to them) with `PagedResult<T>` paging plus project/status/assignee filtering and search; the **graduated mutation model** (`TaskMutation` — Create / FullEdit / StatusChange / Reassign / Delete) resolved in `CanMutateAsync`; assignee validation against the project's team-member pool; task status and priority enumerations; optimistic concurrency on the task row; an `activity_logs` entry on every write; the lazy-loaded Angular **`tasks` route group** (standalone components per ADR-0001) with a dedicated `TasksService`, Reactive Forms for create/edit, and functional role guards.

**Out of scope**: managing the team-member/assignment **pool** itself — creating or removing `team_members` rows (feature 004; this spec *validates against* that pool but never mutates it); the `Project` entity and its ownership rule (002); authentication, the role model, and the audit table definition (001); dashboard aggregation (005) and report export (006); Gantt/timeline views, task dependencies, sub-tasks, comments, attachments, time tracking, and recurring tasks (bonus scope, Constitution I.2).

## Access Logic (applies to every story)

Enforced **server-side**, in this order:

1. **Authenticated by default** — every endpoint requires a valid JWT (inherited from 001). No/invalid/expired token → **401**.
2. **Role gate (controller, attribute-only)** — `[Authorize(Roles = "...")]`. Task **creation, deletion, and reassignment** permit `Admin,ProjectManager`; reads and status updates permit all three roles. A role that is not permitted → **403**. Ad-hoc role checks in method bodies remain prohibited (Constitution V.2).
3. **Scope gate (service)** — *may this user touch this task at all?* Admin unscoped; ProjectManager scoped to tasks whose parent project they own; TeamMember scoped to tasks assigned to them. Out of scope → **403**.
4. **Mutation gate (service)** — *how much of it may they change?* The same TeamMember who passes the scope gate for a read is refused a `FullEdit`, `Reassign`, or `Delete` and permitted only a `StatusChange`. This is the graduated layer this feature adds, and it lives inside `CanMutateAsync` — never in the attribute, never in the controller (Constitution II.2).
5. **Identity from the token** — the acting user comes from `ICurrentUserService` reading the validated JWT, never from the request body.
6. **Deny by default** — if scope or mutation permission cannot be established, the request is denied. Frontend guards and conditionally rendered controls are convenience only.

## Role & Permission Model

The three roles are defined in [001 Auth & RBAC](../001-auth-rbac/spec.md) — each user holds **exactly one**, carried as a single JWT `role` claim. This feature adds no roles; it maps the existing three onto Task operations, and is the first feature where a role's permission differs **by field**, not just by row:

| Operation | Admin | ProjectManager | TeamMember |
|---|---|---|---|
| List / view tasks | All tasks | Tasks in owned projects | Only tasks assigned to them |
| Create task | ✔ any project | ✔ own projects | ✘ **403** |
| Edit task (title, description, priority, due date) | ✔ any | ✔ own projects | ✘ **403** |
| **Update task status** | ✔ any | ✔ own projects | **✔ own assigned tasks only** |
| Reassign task | ✔ any | ✔ own projects | ✘ **403** |
| Delete task | ✔ any | ✔ own projects | ✘ **403** |

**Assignment** is the TeamMember's basis of access, exactly as **ownership** is the ProjectManager's. A task's assignee must be a team member on that task's project (validated against feature 004's pool).

---

## Clarifications

### Session 2026-07-22

- Q: Should the cross-project `GET /api/tasks` endpoint be kept alongside the nested `GET /api/projects/{projectId}/tasks`, given only the nested route was named explicitly? → A: **Keep both.** The nested route serves a single project's task list; the cross-project route (`GET /api/tasks?projectId=&status=&assigneeId=`) is required for a TeamMember's "my work across all my projects" view and will be reused as-is by 005 Dashboard. Both use the identical `ApplyScope` predicate and `PagedResult<T>` envelope — no divergent scoping logic.
- Q: When a task is reassigned, what happens to the previous assignee's access on their next read attempt? → A: **No special handling — scope is re-evaluated fresh on every read.** The moment reassignment commits, the previous assignee's next `GET /api/tasks/{id}` returns **403**, exactly like any other out-of-scope task. No grace period, no cached access, no notification (a notification mechanism is bonus scope, Constitution I.2). This is simply `CanReadAsync` behaving as already specified — no new rule was added, only made explicit.
- Q: May the assignee's `StatusChange` right move a task's status *out of* `Done` (or is `Done` terminal for that right)? → A: **No restriction — `Done` is not terminal for the assignee.** `CanMutateAsync(StatusChange)` applies uniformly regardless of the task's current status, consistent with the "any status → any status" decision (OQ-003-03, no workflow enforcement in v1). Correcting a mis-set `Done` status is a legitimate assignee action and does not require escalation to `FullEdit`.

---

## User Stories

> Story IDs `US-003-01..07`. Each story: **A** Summary · **B** Quality Validation (INVEST · Given-When-Then · edge cases · audit/security · configurability) · **C** UI · **D** API · **E** DB · **F** Separation of concerns. Consolidated schema, API catalog, technical design, and implementation blueprint follow the stories.

---

### US-003-01 — Create a task within a project

**A. Summary**
- **Story ID**: US-003-01 · **Title**: Create a task inside a project
- **Actor**: ProjectManager (own projects) · Admin (any project)
- **User story**: *As a ProjectManager, I want to create a task inside a project I own, with a title, priority, due date, and optionally an assignee, so that the work is broken down and trackable.*
- **Business value**: Turns a project container into executable work; the origin of every downstream metric.
- **Priority**: **P0** · **Reason**: Nothing else in this feature exists without it.
- **Dependencies**: 002 (parent project + ownership). **Out of scope**: creating the team-member pool (004).

**B. Quality validation**
- **INVEST** — Independent ✔; Negotiable ✔ (field set, default status/priority); Valuable ✔; Estimable ✔; Small ✔ (one row + audit); Testable ✔ (row created under the right project, TeamMember blocked, audit written).
- **3Cs** — Card ✔ (stands alone: "create a task inside a project I own"); Conversation ✔ (surfaced an outside-pool assignee, a due date outside the project's window, and writes to a terminal-status project — see Edge cases); Confirmation ✔ (the six Given/When/Then scenarios cover PM-owns, cross-project denial, TeamMember denial, invalid-assignee, date-window, and the 201 shape — sufficient to call this story done).
- **7Cs** — Clear ✔; Concise ✔; Concrete ✔ (exact 201/403/400/404 codes, exact route-not-body rule for `project_id`); Correct ✔ (matches FR-003/FR-004/FR-005); Coherent ✔ (consistently defers ownership resolution to 002 rather than redefining it); Complete ✔ (creation, ownership, assignee validation, and date validation are all covered); Courteous n/a (no user-facing copy in this story).
- **Given/When/Then**
  1. **Given** a ProjectManager and a project **they own**, **When** they create a task with a valid payload, **Then** a `tasks` row is created with `project_id` from the route, `status` defaulting to `ToDo`, and an `activity_logs` entry (`TaskCreated`) is written **in the same transaction**.
  2. **Given** a ProjectManager and a project **owned by someone else**, **When** they create a task in it, **Then** **403** and nothing is written.
  3. **Given** a **TeamMember**, **When** they attempt to create a task, **Then** **403** at the role gate.
  4. **Given** an `assigneeId` that is **not a team member on that project**, **When** creating, **Then** **400** (`ErrorKind.Validation`) with a field error; nothing is stored.
  5. **Given** a `dueDate` outside the parent project's start/end window, **When** creating, **Then** **400** with a field error (ADR-0005 cross-field rule).
  6. **Given** a successful creation, **When** the response is returned, **Then** **201 Created** with `Location: /api/tasks/{id}` and the created task.
- **Edge cases**: unknown `projectId` in the route → **404**; omitted `assigneeId` (**allowed** — a task may be unassigned and simply invisible to TeamMembers until assigned); omitted `status`/`priority` → defaults `ToDo`/`Medium`; `dueDate` omitted (allowed); parent project already in a terminal status (`Completed`/`Cancelled`) — permitted by default, configurable; oversized title/description.
- **Audit/security**: creation audited; `project_id` comes from the **route**, never the body, so a task cannot be smuggled into another project; assignee validated against the project's pool.
- **Configurability**: default status and priority; whether tasks may be added to a terminal-status project; max lengths.

**C. UI** — **F003-S02 Create Task** (standalone component in the lazy-loaded `tasks` route group). Reactive form: `title`, `description`, `priority`, `dueDate`, `assignee` (a picker limited to the project's team members); explicit validators (required title, due-date-within-project-window, max lengths); errors via the shared error-display component; reachable only from a project the user may write to.

**D. API** — `POST /api/projects/{projectId}/tasks` · `[Authorize(Roles = "Admin,ProjectManager")]` · **201 Created** + `Location: /api/tasks/{id}`.

**E. DB** — writes **`tasks`** (FK `project_id` → `projects`, FK `assignee_id` → `users`), **`activity_logs`**.

**F. Separation** — UI: create form + project-scoped assignee picker. Backend: `ITaskService.CreateAsync` → `CanMutateAsync(…, TaskMutation.Create, …)` → assignee-pool + date-window validation → persist → audit. DB: task row + audit row in one transaction. QA: cross-project 403, TeamMember 403, invalid-assignee 400, date-window 400, 201 + Location.

---

### US-003-02 — List and search tasks (role-scoped)

**A. Summary**
- **Story ID**: US-003-02 · **Title**: List, filter, and page through the tasks I may see
- **Actor**: Admin · ProjectManager · TeamMember (each with a different scope)
- **User story**: *As any authenticated user, I want a filterable, paginated list of the tasks I'm permitted to see, so that I can focus on the right work without seeing anyone else's.*
- **Business value**: The working surface of the product — and the proof that scoping holds for the entity that carries the most rows.
- **Priority**: **P0** · **Reason**: Every task workflow starts from a list.
- **Dependencies**: US-003-01; 002 (project ownership); 004's `team_members` for nothing here — TeamMember scope is by **assignee**, not membership. **Out of scope**: saved views, bulk edit.

**B. Quality validation**
- **INVEST** — Independent ✔; Negotiable ✔ (filter set); Valuable ✔; Estimable ✔; Small ✔ (read path); Testable ✔ (scope per role directly assertable).
- **3Cs** — Card ✔ (stands alone: "role-scoped, filterable, paginated task list"); Conversation ✔ (surfaced the filter-cannot-widen-scope rule and an `assigneeId`-spoofing attempt — see Edge cases; and resolved via Clarifications whether the cross-project endpoint should even exist); Confirmation ✔ (the five Given/When/Then scenarios form the three-role scope matrix plus filtering and paging — the primary acceptance test, sufficient to call this story done).
- **7Cs** — Clear ✔ ("a filter can only narrow, never widen" is stated explicitly, twice); Concise ✔; Concrete ✔ (exact query parameters, exact `PagedResult<T>` semantics); Correct ✔ (matches FR-009/FR-010/FR-011); Coherent ✔ (both endpoints reuse the identical `ApplyScope` and envelope, per the Clarifications decision); Complete ✔ (three roles, four filter types, and paging metadata are all covered); Courteous n/a (a list view with no story-specific user-facing copy).
- **Given/When/Then**
  1. **Given** tasks across several projects, **When** an **Admin** lists, **Then** all tasks are returned (subject to paging).
  2. **Given** the same data, **When** a **ProjectManager** lists, **Then** only tasks whose **parent project they own** are returned — including tasks assigned to nobody.
  3. **Given** the same data, **When** a **TeamMember** lists, **Then** **only tasks assigned to them** are returned; unassigned tasks and colleagues' tasks never appear, and with no assignments the result is an **empty list, not an error**.
  4. **Given** `?status=`, `?assigneeId=`, `?projectId=`, or `?search=`, **When** listing, **Then** results match **within** the caller's scope — a filter can only ever narrow a scope, never widen it.
  5. **Given** `?page`/`?pageSize`, **When** listing, **Then** a `PagedResult<T>` envelope is returned whose `totalCount` is **scoped to the caller**; `pageSize` above the maximum is **clamped**, not rejected.
- **Edge cases**: a TeamMember passing `?assigneeId=` of another user (returns **empty**, never another user's tasks — the scope predicate wins); `page` beyond the last page (empty items, valid metadata); non-numeric/negative paging (**400**); unknown `projectId` filter (empty, not 404); overdue-task filtering by `dueDate`.
- **Audit/security**: the scope predicate is composed **into the query** so out-of-scope tasks are never loaded, counted, or paged; reads are not audited (Constitution IV.4).
- **Configurability**: default/maximum `pageSize`, default sort (due date ascending), searchable fields.

**C. UI** — **F003-S01 Task List**. Table with search, status/priority/assignee filters, sort, and paginator, built on the `PagedResult<T>` envelope. The view renders exactly what the API returns and applies **no client-side role filtering**. Empty/loading/error states explicit. "New Task" is hidden for TeamMember (UX only — the API still returns 403).

**D. API** — `GET /api/projects/{projectId}/tasks?page=&pageSize=&status=&assigneeId=&search=&sort=` (tasks within one project) and `GET /api/tasks?projectId=&status=&assigneeId=&…` (cross-project, e.g. a TeamMember's "my work" view) · `[Authorize]` (all three roles) · **200** with `PagedResult<TaskSummaryDto>`.

**E. DB** — reads **`tasks`**, joining **`projects`** for ProjectManager scope resolution. Indexes lead with `project_id` and `assignee_id`.

**F. Separation** — UI: list + filters + paginator + states. Backend: `ITaskAccessPolicy.ApplyScope` composes the scope predicate; `ITaskService.ListAsync` layers filter/search/sort/paging on top. DB: indexed scope + paging query. QA: three-role scope matrix (**primary acceptance test**), filter-cannot-widen-scope, clamped `pageSize`, empty state.

---

### US-003-03 — View task detail

**A. Summary**
- **Story ID**: US-003-03 · **Title**: View a single task's details
- **Actor**: Admin (any) · ProjectManager (owned projects) · TeamMember (assigned)
- **User story**: *As a permitted user, I want to open a task and see its full detail, so that I understand what is being asked, by when, and at what priority.*
- **Business value**: The screen where work is actually understood before it is acted on.
- **Priority**: **P0** · **Reason**: Required before edit/status/reassign.
- **Dependencies**: US-003-01.

**B. Quality validation**
- **INVEST** — all ✔ (single read).
- **3Cs** — Card ✔ (stands alone: "open one task → full detail, scope-checked"); Conversation ✔ (surfaced malformed-id handling and displaying a task whose assignee was deactivated — see Edge cases); Confirmation ✔ (the four Given/When/Then scenarios cover success, an unknown id, out-of-scope, and the TeamMember status-only indicator — sufficient to call this story done).
- **7Cs** — Clear ✔; Concise ✔; Concrete ✔ (exact fields returned, exact 200/403/404 codes); Correct ✔ (matches FR-013); Coherent ✔ (explicitly states which control is enabled for a TeamMember, foreshadowing US-003-05's narrower right rather than leaving it implicit); Complete ✔ (success, two denial modes, and the role-specific UI hint are all covered); Courteous n/a (a read-only view with no story-specific copy).
- **Given/When/Then**
  1. **Given** a task they may see, **When** a user opens it, **Then** **200** with title, description, status, priority, due date, parent project, assignee, and timestamps.
  2. **Given** a task id that **does not exist**, **When** requested, **Then** **404**.
  3. **Given** a task that exists but is **outside the caller's scope**, **When** requested, **Then** **403**.
  4. **Given** a TeamMember viewing their assigned task, **When** the response renders, **Then** it indicates that only **status** is editable by them.
- **Edge cases**: malformed (non-GUID) id → **400**; task whose assignee was deactivated (still returned, assignee flagged inactive); task in a project the caller once owned but no longer does → **403**; concurrent deletion between list and open → **404**.
- **Audit/security**: scope checked before the entity is returned; **403-vs-404** follows 002's convention (403 by default, maskable to 404 by configuration).
- **Configurability**: whether out-of-scope reads return **403** or are masked as **404**.

**C. UI** — **F003-S03 Task Detail** (read-only view). All fields plus project, assignee, and timestamps. Edit / Reassign / Delete actions render only for roles permitted to use them; for a TeamMember only the status control is enabled. Loading/error/not-found/forbidden states explicit.

**D. API** — `GET /api/tasks/{id}` · `[Authorize]` (all three roles) · **200** · **403** out of scope · **404** unknown id.

**E. DB** — reads **`tasks`** (+ `projects`, `users` for display); joins `projects` for ProjectManager scope.

**F. Separation** — UI: detail view + states. Backend: `ITaskService.GetByIdAsync` → `CanReadAsync`. DB: single read + scope join. QA: 200/403/404 matrix per role, malformed id, deleted-in-flight.

---

### US-003-04 — Edit a task

**A. Summary**
- **Story ID**: US-003-04 · **Title**: Update a task's details
- **Actor**: Admin (any) · ProjectManager (own projects only)
- **User story**: *As a ProjectManager, I want to edit the tasks in my projects, so that the plan stays accurate as scope, priority, and deadlines shift.*
- **Business value**: Keeps the authoritative work record correct; priority and due dates drive the dashboard.
- **Priority**: **P0** · **Reason**: Core maintenance.
- **Dependencies**: US-003-01. **Out of scope**: status-only updates (US-003-05) and reassignment (US-003-07) — deliberately separated because their permission rules differ.

**B. Quality validation**
- **INVEST** — all ✔ (field update only).
- **3Cs** — Card ✔ (stands alone: "full-edit a task in a project I own"); Conversation ✔ (surfaced the project-immutability rule and, centrally, whether the assignee may use this endpoint at all — see Edge cases and T.2); Confirmation ✔ (the five Given/When/Then scenarios include, in scenario 3, the defining test of the graduated model itself — confirmation here is stronger than in a typical edit story).
- **7Cs** — Clear ✔ (explicitly calls its own scenario 3 "the graduated model's defining test" rather than leaving the reader to infer it); Concise ✔; Concrete ✔ (exact 409/`xmin` mechanism, exact refusal distinct from the narrower right the same user retains); Correct ✔ (matches FR-006/FR-012); Coherent ✔ (deliberately separated from US-003-05/US-003-07 with the reason stated in Dependencies, not left implicit); Complete ✔ (update, ownership, the graduated refusal, validation, and concurrency are all covered); Courteous n/a (no story-specific copy beyond the generic conflict message).
- **Given/When/Then**
  1. **Given** a task in a project they own, **When** a ProjectManager updates it, **Then** the row is updated, `updated_at` refreshed, and an `activity_logs` entry (`TaskUpdated`, summary of changed fields) is written.
  2. **Given** a task in **someone else's** project, **When** a ProjectManager updates it, **Then** **403**.
  3. **Given** a **TeamMember** — even the task's own **assignee** — **When** they attempt a full edit, **Then** **403** from `CanMutateAsync(FullEdit)`. *This is the graduated model's defining test: the same user is allowed a `StatusChange` on the same row.*
  4. **Given** an invalid payload (blank title, due date outside the project window), **When** updating, **Then** **400** with field errors; nothing changes.
  5. **Given** a concurrent edit, **When** the second writer saves a stale row, **Then** **409 Conflict** (`xmin`, ADR-0004) — never a silent overwrite.
- **Edge cases**: changing `project_id` — **not permitted** through this endpoint (a task does not move between projects; see OQ-003-05); no-op update still refreshes `updated_at` and audits; unknown id → **404**; editing a task whose project is in a terminal status (permitted by default, configurable).
- **Audit/security**: every update audited with a change summary; scope **and** mutation kind re-checked at write time, never trusted from an earlier read.
- **Configurability**: which fields are editable once a task is `Done`; whether editing is blocked in a terminal-status project.

**C. UI** — **F003-S04 Edit Task**. Reactive form pre-populated from detail; same validators as create; unsaved-changes guard; on **409** the user is told the task changed and offered a reload. Not reachable for TeamMember (guard) — and refused by the API regardless.

**D. API** — `PUT /api/tasks/{id}` · `[Authorize(Roles = "Admin,ProjectManager")]` · **200** with the updated task · **400** / **403** / **404** / **409** as applicable.

**E. DB** — updates **`tasks`**; writes **`activity_logs`**.

**F. Separation** — UI: edit form + conflict UX. Backend: `ITaskService.UpdateAsync` → `CanMutateAsync(FullEdit)` → validate → persist → audit. DB: update + audit + `xmin` check. QA: cross-project 403, **assignee-refused-full-edit 403**, validation 400, concurrency 409.

---

### US-003-05 — Update task status (assignee-level)

**A. Summary**
- **Story ID**: US-003-05 · **Title**: Move my task's status forward
- **Actor**: TeamMember (own assigned tasks) · ProjectManager (own projects) · Admin (any)
- **User story**: *As a TeamMember, I want to update the status of a task assigned to me, so that I can report progress without needing permission to change anything else about it.*
- **Business value**: The single most frequent write in the product, and the one that makes the dashboard meaningful. Giving TeamMembers a **narrow** write keeps the record live without risking scope creep or accidental reassignment.
- **Priority**: **P0** · **Reason**: Without it, TeamMembers are read-only and the status data goes stale.
- **Dependencies**: US-003-01. **Out of scope**: full edit (US-003-04).

**B. Quality validation**
- **INVEST** — Independent ✔ (narrow, self-contained write); Valuable ✔; Testable ✔ (the permitted/refused pair on one row is directly assertable).
- **3Cs** — Card ✔ (stands alone: "the assignee moves their own task's status, nothing else"); Conversation ✔ (surfaced payload-widening attempts and, via Clarifications, whether `Done` is terminal for this right — see Edge cases); Confirmation ✔ (the five Given/When/Then scenarios include the assignee-allowed/non-assignee-refused pair on the same row plus the extra-fields-ignored proof — the strongest confirmation in either file, since this is the acceptance test for the feature's central claim).
- **7Cs** — Clear ✔ (states outright that this is "the single most frequent write in the product"); Concise ✔; Concrete ✔ (exact status-only DTO shape, exact from→to audit); Correct ✔ (matches FR-007/FR-008); Coherent ✔ (explicitly cross-referenced from US-003-04 as its counterpart, not a duplicate); Complete ✔ (assignee-allowed, non-assignee-refused, payload-widening, PM-override, and invalid-status are all covered); Courteous n/a (a status control with no story-specific copy).
- **Given/When/Then**
  1. **Given** a task assigned to them, **When** a TeamMember changes its status, **Then** the status is updated, `updated_at` refreshed, and an `activity_logs` entry (`TaskStatusChanged`, from→to) is written.
  2. **Given** a task assigned to **someone else**, **When** a TeamMember changes its status, **Then** **403** (scope gate).
  3. **Given** the same task, **When** the same TeamMember sends a payload also containing a new title or assignee, **Then** those fields are **ignored** — this endpoint accepts **status only**, so privilege cannot be widened by payload.
  4. **Given** a ProjectManager and a task in a project they own, **When** they change its status, **Then** it succeeds (their `FullEdit` right subsumes `StatusChange`).
  5. **Given** an invalid status value, **When** submitted, **Then** **400**.
- **Edge cases**: setting the status to its current value (no-op — still audited); an unassigned task (no TeamMember can reach it); transition rules between statuses — **not enforced in v1**, any status may move to any other, **including out of `Done`** — the assignee's `StatusChange` right is not lifecycle-restricted (see OQ-003-03; Clarifications 2026-07-22); concurrent status change → **409**.
- **Audit/security**: the narrow endpoint is the enforcement mechanism — it binds a **status-only DTO**, so extra fields are structurally impossible to apply, not merely rejected. `CanMutateAsync(StatusChange)` still runs.
- **Configurability**: the permitted status set; whether a status workflow (allowed transitions) is enforced.

**C. UI** — status control on **F003-S03 Task Detail** and inline in the task list (a dropdown or a drag-between-columns board). For a TeamMember this is the **only** enabled write control on the screen.

**D. API** — `PUT /api/tasks/{id}/status` · `[Authorize]` (all three roles) · body `{ "status": "InProgress" }` · **200** with the updated task · **400** / **403** / **404** / **409**.

**E. DB** — updates **`tasks.status`** only; writes **`activity_logs`**.

**F. Separation** — UI: status control only. Backend: `ITaskService.UpdateStatusAsync` → `CanMutateAsync(StatusChange)`; a dedicated status-only DTO. DB: single-column update + audit. QA: **assignee-allowed vs. non-assignee-refused on the same row**, extra-fields-ignored, invalid status 400, concurrency 409.

---

### US-003-06 — Delete a task

**A. Summary**
- **Story ID**: US-003-06 · **Title**: Delete a task
- **Actor**: Admin (any) · ProjectManager (own projects only)
- **User story**: *As a ProjectManager, I want to delete a task from a project I own, so that cancelled or mistaken work doesn't distort the plan or the reports.*
- **Business value**: Keeps the work record and its derived metrics honest.
- **Priority**: **P1** · **Reason**: Important, but builds on create/read.
- **Dependencies**: US-003-01. **Out of scope**: restore/undelete; bulk delete.

**B. Quality validation**
- **INVEST** — all ✔.
- **3Cs** — Card ✔ (stands alone: "delete a task in a project I own"); Conversation ✔ (surfaced deleting a `Done` task and the concurrent delete/update race — see Edge cases); Confirmation ✔ (the four Given/When/Then scenarios cover owner-delete, cross-project denial, TeamMember denial (including the assignee), and double-delete — sufficient to call this story done).
- **7Cs** — Clear ✔; Concise ✔; Concrete ✔ (exact 204 code, exact audit-before-removal ordering); Correct ✔ (matches FR-014); Coherent ✔ (consistent with 002's audit-survives-cascade pattern rather than inventing a new one); Complete ✔ (delete, scope, mutation-kind, and audit-survival are all covered); Courteous ✔ (the confirmation dialog names the specific task before deleting).
- **Given/When/Then**
  1. **Given** a task in a project they own, **When** a ProjectManager deletes it, **Then** the row is removed, **204 No Content** returned, and an `activity_logs` entry (`TaskDeleted`, snapshot summary) is written **before** removal so the audit survives.
  2. **Given** a task in someone else's project, **When** a ProjectManager deletes it, **Then** **403**.
  3. **Given** a **TeamMember** — including the task's assignee — **When** they attempt a delete, **Then** **403**.
  4. **Given** an unknown id, **When** deleting, **Then** **404**; deleting twice yields **404** on the second call.
- **Edge cases**: deleting a `Done` task (permitted — history lives in `activity_logs`); concurrent delete/update race (the loser observes **404**); deleting the last task in a project (project itself is unaffected).
- **Audit/security**: deletion audited with a pre-removal snapshot; scope and mutation kind re-checked at write time; `activity_logs` rows are **never** cascaded away.
- **Configurability**: hard delete vs. soft delete (default **hard delete**, consistent with 002 — OQ-003-06).

**C. UI** — delete action on **F003-S03 Task Detail** and in the list row menu, behind a confirmation dialog naming the task. Rendered only for permitted roles (UX only).

**D. API** — `DELETE /api/tasks/{id}` · `[Authorize(Roles = "Admin,ProjectManager")]` · **204 No Content** · **403** / **404**.

**E. DB** — deletes from **`tasks`**; writes **`activity_logs`** (retained).

**F. Separation** — UI: confirm dialog + list refresh. Backend: `ITaskService.DeleteAsync` → `CanMutateAsync(Delete)` → audit → delete. DB: delete + retained audit. QA: cross-project 403, assignee-refused 403, audit-before-delete, double-delete 404.

---

### US-003-07 — Reassign a task

**A. Summary**
- **Story ID**: US-003-07 · **Title**: Assign or reassign a task to a team member
- **Actor**: Admin (any) · ProjectManager (own projects only)
- **User story**: *As a ProjectManager, I want to assign or reassign a task to a team member on that project, so that work is distributed to the right person.*
- **Business value**: Distribution of work is the ProjectManager's core act; separating it from a general edit makes the permission boundary explicit and auditable.
- **Priority**: **P1** · **Reason**: Important; tasks may be created unassigned and assigned later.
- **Dependencies**: US-003-01; feature 004's team-member pool (read-only validation target). **Out of scope**: managing who is on the project's team (004).

**B. Quality validation**
- **INVEST** — all ✔ (single-field write with its own rule).
- **3Cs** — Card ✔ (stands alone: "assign or reassign a task to a valid team member"); Conversation ✔ (surfaced outside-pool candidates, deactivated-user assignment, and, via Clarifications, what happens to the previous assignee's access — see Edge cases); Confirmation ✔ (the five Given/When/Then scenarios cover successful reassignment, outside-pool denial, TeamMember denial, unassignment, and cross-project denial — sufficient to call this story done).
- **7Cs** — Clear ✔; Concise ✔; Concrete ✔ (exact from→to audit, exact 400-vs-403 distinction); Correct ✔ (matches FR-004); Coherent ✔ (validates against, but never mutates, feature 004's pool — stated consistently in Scope Summary, Dependencies, and here); Complete ✔ (assign, reassign, unassign, and the previous-assignee edge case are all covered); Courteous n/a (a picker control with no story-specific copy).
- **Given/When/Then**
  1. **Given** a task in a project they own and a candidate who **is** a team member on that project, **When** a ProjectManager reassigns it, **Then** `assignee_id` is updated and an `activity_logs` entry (`TaskReassigned`, from→to) is written.
  2. **Given** a candidate who is **not** a team member on that project, **When** reassigning, **Then** **400** (`ErrorKind.Validation`) — assignment outside the project's pool is refused.
  3. **Given** a **TeamMember**, **When** they attempt to reassign their own task (to themselves or anyone else), **Then** **403** from `CanMutateAsync(Reassign)`.
  4. **Given** a null assignee, **When** submitted, **Then** the task becomes **unassigned** (permitted) and consequently invisible to all TeamMembers.
  5. **Given** a task in someone else's project, **When** a ProjectManager reassigns it, **Then** **403**.
- **Edge cases**: reassigning to the current assignee (no-op — still audited); assigning to a **deactivated** user (**refused, 400**); concurrent reassignment → **409**. **The previous assignee's access is not cached or grace-windowed** — their very next `GET /api/tasks/{id}` re-evaluates scope fresh and returns **403**, the same as any other out-of-scope task (Clarifications 2026-07-22); no notification is sent.
- **Audit/security**: reassignment is audited from→to as a distinct action, so the transfer of access is traceable; validated against 004's pool without mutating it.
- **Configurability**: whether unassigning is permitted; whether assigning to a deactivated user is permitted (default **no**).

**C. UI** — assignee picker on **F003-S03 Task Detail**, limited to the parent project's team members; confirmation on change. Not rendered for TeamMember.

**D. API** — `PUT /api/tasks/{id}/assignee` · `[Authorize(Roles = "Admin,ProjectManager")]` · body `{ "assigneeId": "…" | null }` · **200** with the updated task · **400** / **403** / **404** / **409**.

**E. DB** — updates **`tasks.assignee_id`**; reads **`team_members`** (004) to validate; writes **`activity_logs`**.

**F. Separation** — UI: project-scoped assignee picker. Backend: `ITaskService.ReassignAsync` → `CanMutateAsync(Reassign)` → pool validation → persist → audit. DB: single-column update + validation join + audit. QA: outside-pool 400, TeamMember-refused 403, deactivated-user 400, unassign path, audit from→to.

---

## Consolidated Data Model (review-level; final physical schema at implementation)

> Code-First (EF Core 10 + Npgsql). PostgreSQL identifiers are **snake_case** (Constitution VIII.2). `users` / `activity_logs` (001), `projects` (002), and `team_members` (004) are **referenced, not redefined** — all five constitution entities are created in the initial migration; a feature owns an entity's API/UI/rules, not its table's existence. Migration name: `AddTasksTable`.
>
> **CLR naming note**: the entity type is **`TaskItem`**, not `Task`, to avoid colliding with `System.Threading.Tasks.Task` throughout an async codebase. It maps to the `tasks` table.

| Entity | Table | Purpose | Key fields (type · req/null) | Relationships |
|---|---|---|---|---|
| **TaskItem** (this feature) | `tasks` | The unit of work | `id` uuid PK; `project_id` uuid FK (req); `title` varchar(200) (req); `description` varchar(2000) (null); `status` (req · enum, default `ToDo`); `priority` (req · enum, default `Medium`); `due_date` date (null); `assignee_id` uuid FK (null); `created_at`/`updated_at` (req) | *→1 `projects` (**cascade**); *→1 `users` (assignee, **restrict**) |
| **Project** (from 002) | `projects` | Parent + ownership source for the ProjectManager scope | `id`, `owner_id`, `start_date`, `end_date` (referenced) | 1→* `tasks` |
| **User** (from 001) | `users` | Assignee reference | `id`, `is_active` (referenced) | 1→* `tasks` as assignee |
| **TeamMember** (from 004) | `team_members` | Read-only here — validates that an assignee belongs to the project | `project_id`, `user_id` | validation join only |
| **ActivityLog** (from 001) | `activity_logs` | Audit of every write | `actor_id`, `action`, `entity_type='Task'`, `entity_id`, `timestamp`, `change_summary` | references `tasks` by id |

**Cascade behaviour — decided, not implicit** (Constitution IV.3):
- **`tasks.project_id` → `projects` is `ON DELETE CASCADE`.** Deleting a project **does** delete its tasks. A task has no meaning without its project, and this matches the cascade 002 already declares and warns about in its delete confirmation dialog.
- **`tasks.assignee_id` → `users` is `ON DELETE RESTRICT`.** A user with assigned tasks cannot be deleted until their tasks are reassigned or removed — consistent with 002's treatment of project owners, and it prevents silently orphaning work.
- **`activity_logs` rows are never cascaded**, so a deleted task's (and a deleted project's) history survives for Reports (006).

## Consolidated API Catalog

> Base path `/api`; JSON over HTTPS; errors as **RFC 7807 Problem Details** produced by the shared `ErrorKind` mapper ([shared-contracts §1](../../docs/shared-contracts.md)); documented via **Swagger/OpenAPI**. Authenticated by default (001). Sub-resource routes use **nouns** (`/status`, `/assignee`), never verbs, per Constitution II.3.

| Method · Route | Purpose | Role gate | Service gate | Success | Failure |
|---|---|---|---|---|---|
| `GET /api/projects/{projectId}/tasks` | Tasks in one project, paged | `[Authorize]` (all 3) | `ApplyScope` | **200** `PagedResult<T>` | 400, 401, 403, 404 |
| `GET /api/tasks` | Cross-project list (e.g. "my work"), paged | `[Authorize]` (all 3) | `ApplyScope` | **200** `PagedResult<T>` | 400, 401 |
| `GET /api/tasks/{id}` | Task detail | `[Authorize]` (all 3) | `CanReadAsync` | **200** task DTO | 400, 401, 403, 404 |
| `POST /api/projects/{projectId}/tasks` | Create | `[Authorize(Roles="Admin,ProjectManager")]` | `CanMutateAsync(Create)` | **201** + `Location` | 400, 401, 403, 404 |
| `PUT /api/tasks/{id}` | Full edit | `[Authorize(Roles="Admin,ProjectManager")]` | `CanMutateAsync(FullEdit)` | **200** task DTO | 400, 401, 403, 404, **409** |
| `PUT /api/tasks/{id}/status` | Status only (assignee-level) | `[Authorize]` (all 3) | `CanMutateAsync(StatusChange)` | **200** task DTO | 400, 401, 403, 404, **409** |
| `PUT /api/tasks/{id}/assignee` | Assign / reassign | `[Authorize(Roles="Admin,ProjectManager")]` | `CanMutateAsync(Reassign)` | **200** task DTO | 400, 401, 403, 404, **409** |
| `DELETE /api/tasks/{id}` | Delete | `[Authorize(Roles="Admin,ProjectManager")]` | `CanMutateAsync(Delete)` | **204** | 401, 403, 404 |

---

## Technical Design — Graduated Authorization & Scoped Task Access

> The detailed solution: the components, exact requests/responses, how the mutation gate resolves, the step-by-step flows, failure handling, and security guarantees. Written so a developer can implement it directly.

### T.1 The roles (who is authority, who enforces)
- **The .NET API is the authority.** The controller declares the **role gate**; the **service layer** owns the scope gate, the mutation gate, and every business rule. Controllers do model binding, validation, and delegation only (Constitution II.2).
- **The Angular frontend is convenience.** Guards and conditionally enabled controls shape what a user *sees*; the API re-checks every request. A TeamMember who hand-crafts a `PUT /api/tasks/{id}` still receives **403**.

### T.2 The graduated mutation model (the heart of this feature)
Feature 002 established two layers — *role* (attribute) and *scope* (service). Tasks needs a **third**, because for the first time a user who passes the scope gate is still only allowed **part** of a write:

> A TeamMember assigned to task X **may** change X's status and **may not** change X's title, assignee, or existence.

Yes/no row access cannot express that, so the policy takes the **kind of mutation** as an input:

```csharp
public enum TaskMutation { Create, FullEdit, StatusChange, Reassign, Delete }
```

**Decision matrix inside `CanMutateAsync`:**

| `TaskMutation` | Admin | ProjectManager (owns parent project) | TeamMember (is assignee) |
|---|---|---|---|
| `Create` | allow | allow | **deny** |
| `FullEdit` | allow | allow | **deny** |
| `StatusChange` | allow | allow | **allow** |
| `Reassign` | allow | allow | **deny** |
| `Delete` | allow | allow | **deny** |

Two mechanisms enforce it together, deliberately belt-and-braces:
1. **Separate narrow endpoints** with **narrow DTOs** — `PUT /api/tasks/{id}/status` binds a status-only DTO, so a widened payload is *structurally* incapable of changing the title. Privilege escalation by extra JSON field is impossible, not merely rejected.
2. **The mutation gate** — `CanMutateAsync(task, mutation, caller)` is still consulted, so the rule is enforced even if a future endpoint reuses the service.

> **Why not one `[Authorize]` policy per operation?** The permitted set depends on *row facts* (who owns the parent project, who is assigned) that are unknown until the entity is loaded. Attributes run before that. Constitution II.2 also places business rules in Services. Hence the split: role in the attribute, everything row-dependent in `ITaskAccessPolicy`.

### T.3 The endpoints, with concrete examples

**(1) Create (within a project)**
```
POST /api/projects/4d9b1e77-…-c3/tasks     Authorization: Bearer eyJ…  (role=ProjectManager)
{ "title": "Draft rollout checklist", "description": "Cover regions A–C",
  "priority": "High", "dueDate": "2026-09-15", "assigneeId": "b81a…-77" }

→ 201 Created
Location: /api/tasks/9ac41f02-…-e5
{ "id": "9ac41f02-…-e5", "projectId": "4d9b1e77-…-c3",
  "title": "Draft rollout checklist", "description": "Cover regions A–C",
  "status": "ToDo", "priority": "High", "dueDate": "2026-09-15",
  "assignee": { "id": "b81a…-77", "fullName": "Sam Okafor" },
  "createdAt": "2026-07-22T10:05:00Z", "updatedAt": "2026-07-22T10:05:00Z" }
```
`projectId` comes from the **route**, never the body. An `assigneeId` outside the project's team pool → **400**.

**(2) List (role-scoped + paged)**
```
GET /api/tasks?status=InProgress&page=1&pageSize=20     Authorization: Bearer eyJ…  (role=TeamMember)

→ 200 OK
{ "items": [ { "id": "9ac41f02-…-e5", "projectId": "4d9b1e77-…-c3",
               "title": "Draft rollout checklist", "status": "InProgress",
               "priority": "High", "dueDate": "2026-09-15",
               "assignee": { "id": "b81a…-77", "fullName": "Sam Okafor" } } ],
  "page": 1, "pageSize": 20, "totalCount": 1, "totalPages": 1 }
```
`totalCount` is scoped to the caller — a TeamMember can never learn how many tasks exist beyond their own.

**(3) Full edit — refused for an assignee**
```
PUT /api/tasks/9ac41f02-…-e5      Authorization: Bearer eyJ…  (role=TeamMember, IS the assignee)
{ "title": "Renamed by assignee", "priority": "Low", … }

→ 403 Forbidden
{ "type": "…", "title": "Forbidden",
  "detail": "You may update the status of this task, but not its details.",
  "status": 403, "traceId": "…" }
```

**(4) Status change — permitted for the same user, same row**
```
PUT /api/tasks/9ac41f02-…-e5/status   Authorization: Bearer eyJ…  (role=TeamMember, IS the assignee)
{ "status": "InProgress" }

→ 200 OK      (only `status` and `updated_at` changed; TaskStatusChanged audited from→to)
→ 409 Conflict  (the row changed since it was read — stale xmin, ADR-0004)
```
Examples (3) and (4) are the same user acting on the same row: **this pair is the acceptance test for the graduated model.**

**(5) Reassign**
```
PUT /api/tasks/9ac41f02-…-e5/assignee    Authorization: Bearer eyJ…  (role=ProjectManager)
{ "assigneeId": "c04d…-19" }

→ 200 OK    (TaskReassigned audited from→to)
→ 400       { "title": "Validation failed",
              "errors": { "assigneeId": ["User is not a team member on this project."] } }
```

**(6) Delete**
```
DELETE /api/tasks/9ac41f02-…-e5      Authorization: Bearer eyJ…  (role=ProjectManager)

→ 204 No Content     (audit written before removal; audit row retained)
→ 403 / 404
```

### T.4 How a role-scoped task list is built (step by step)
1. The JWT is validated; `ICurrentUserService` materializes `CurrentUser(UserId, Email, Role)` — never from the body.
2. The role gate admits the caller (all three roles may list).
3. `ITaskAccessPolicy.ApplyScope(query, caller)` composes the scope **into the `IQueryable`**:
   - `Admin` → no predicate.
   - `ProjectManager` → `t => t.Project.OwnerId == caller.UserId`.
   - `TeamMember` → `t => t.AssigneeId == caller.UserId`.
4. Route/query filters (`projectId`, `status`, `assigneeId`, `search`, `sort`) are applied **on top of** the scoped query — a filter can only narrow, never widen.
5. `TotalCount` is computed from the scoped+filtered query, **then** `Skip/Take` is applied, so paging metadata never reveals out-of-scope rows. `pageSize` is **clamped** to the configured maximum.
6. One database round-trip returns the page; EF Core translates the whole composition to SQL — no in-memory filtering.

### T.5 How a write flows (step by step)
1. The role gate admits or refuses the caller (**403** before any data is touched for a disallowed role).
2. The controller binds the **operation-specific DTO** (full-edit / status-only / assignee-only) and runs its FluentValidation validator (ADR-0005), then delegates to `ITaskService`.
3. The service loads the task with its parent project. Not found → `ErrorKind.NotFound` → **404**.
4. `CanMutateAsync(task, mutation, caller)` evaluates the **scope and mutation** gates together against the matrix in T.2 → `ErrorKind.Forbidden` → **403** when refused.
5. Business validation that needs the database runs here: assignee ∈ project's team pool, assignee is active, `dueDate` within the project's start/end window → `ErrorKind.Validation` → **400**.
6. The change is applied; `IActivityLogService.LogAsync` writes the audit row (actor, action, `Task`, id, timestamp, change summary) — **for deletes, before removal**.
7. Change + audit commit in **one transaction/`SaveChanges`**; a stale `xmin` raises `DbUpdateConcurrencyException` → `ErrorKind.Conflict` → **409**.

### T.6 API behaviour rules
- **Resource-oriented, plural nouns**; sub-resources (`/status`, `/assignee`) are **nouns**, not verbs (Constitution VI.3). Creation and project-scoped listing are nested under the parent project; single-task operations are flat under `/api/tasks/{id}`.
- **Status codes** (Constitution VI.2): 201 + `Location` on create; 200 on read/update; 204 on delete; 400 validation/bad paging; 401 unauthenticated; 403 role, scope, or mutation denial; 404 unknown id; **409** stale row version; 500 server. All errors are Problem Details from the shared `ErrorKind` mapper (ADR-0003).
- **Pagination** (Constitution VI.4): both collection endpoints return `PagedResult<T>` with `?page`/`?pageSize`, default 20, maximum 100, **clamped** not rejected.
- **Versionable** — routes are designed so a future `/api/v1` prefix can be added without breaking clients (Constitution VI.1); not adopted now.

### T.7 Failure handling (fail-safe)
- **Unknown/expired token → 401** (001).
- **Disallowed role → 403** at the attribute, before data is touched.
- **Out of scope → 403**; **unknown id → 404**; out-of-scope reads return 403 by default (maskable to 404 by configuration, matching 002).
- **Permitted role, permitted row, wrong mutation kind → 403** with a message that names the narrower right the caller *does* have ("You may update the status of this task, but not its details").
- **Assignee outside the project pool, inactive assignee, or due date outside the project window → 400** with field errors.
- **Concurrent write → 409**; the UI offers reload-and-reapply. Silent last-write-wins is never acceptable (ADR-0004).
- Uncaught errors → **500** as Problem Details; the Angular `ErrorInterceptor` + global `ErrorHandler` surface them via the shared notification component (Constitution VII.7).

### T.8 Security guarantees
- Every endpoint requires a valid JWT; the role gate is **attribute-declared only** (Constitution V.1, V.2).
- **Privilege cannot be widened by payload**: narrow endpoints bind narrow DTOs, so a TeamMember's status request is structurally incapable of carrying a title or assignee change.
- **Out-of-scope rows are never loaded** — the scope predicate is part of the SQL, so nothing leaks through items, totals, or paging metadata.
- `project_id` is taken from the route on create, so a task cannot be smuggled into a project the caller does not own.
- Scope **and** mutation kind are re-checked at write time; a stale read can never authorize a later mutation.
- Reassignment is validated against feature 004's pool **without mutating it**, and audited from→to so transfers of access are traceable.
- Every write to Tasks is audited in the same transaction as the change; audit rows survive cascade deletion of the task or its project.
- All data access goes through EF Core; no raw SQL (Constitution IV.1).

---

## Implementation Blueprint (build-ready detail)

> Everything the team needs to build this feature: concrete schema, enums, service interfaces, configuration, error model, NFRs, the audit catalog, and the Definition of Done.

### B.1 Concrete schema (DDL-level intent; expressed as an EF Core migration)
> PostgreSQL 18 via Npgsql. snake_case identifiers. Timestamps `timestamptz` (UTC); calendar dates `date`. Migration name: `AddTasksTable`.

**`tasks`** (CLR type `TaskItem`)
- `id` uuid **PK**
- `project_id` uuid **NOT NULL** FK→`projects(id)` **ON DELETE CASCADE**
- `title` varchar(200) **NOT NULL**
- `description` varchar(2000) **NULL**
- `status` varchar(20) **NOT NULL DEFAULT 'ToDo'** (see B.2)
- `priority` varchar(20) **NOT NULL DEFAULT 'Medium'** (see B.2)
- `due_date` date **NULL** — application-level rule: within the parent project's `start_date`…`end_date` window when both are set
- `assignee_id` uuid **NULL** FK→`users(id)` **ON DELETE RESTRICT**
- `created_at` timestamptz **NOT NULL** · `updated_at` timestamptz **NOT NULL**
- **Concurrency**: PostgreSQL `xmin` mapped as an EF Core row-version token (ADR-0004); stale write → **409**
- **INDEX** (`project_id`), (`assignee_id`), (`status`), (`project_id`,`status`), (`assignee_id`,`status`); text index on `title` for search

**Referenced, not defined here**: `projects` (002), `users` and `activity_logs` (001), `team_members` (004, validation join only).

### B.2 Enumerations (fixed value sets)
- **TaskStatus**: `ToDo, InProgress, InReview, Done, Blocked` (default `ToDo`; persisted as string for readability and migration safety)
- **TaskPriority**: `Low, Medium, High, Critical` (default `Medium`)
- **TaskMutation** (authorization input, not persisted): `Create, FullEdit, StatusChange, Reassign, Delete`
- **AuditAction** (Task): `TaskCreated, TaskUpdated, TaskStatusChanged, TaskReassigned, TaskDeleted`

### B.3 Service interfaces & method signatures (C#; nullable reference types on)
```csharp
public interface ITaskService {
    Task<Result<PagedResult<TaskSummaryDto>>> ListAsync(TaskQuery query, CurrentUser caller, CancellationToken ct);
    Task<Result<TaskDetailDto>> GetByIdAsync(Guid taskId, CurrentUser caller, CancellationToken ct);
    // projectId comes from the ROUTE, never the body. Validates assignee pool + due-date window. Audits TaskCreated.
    Task<Result<TaskDetailDto>> CreateAsync(Guid projectId, CreateTaskRequest req, CurrentUser caller, CancellationToken ct);
    // Full edit — refused for a TeamMember even on their own assigned task. Audits TaskUpdated.
    Task<Result<TaskDetailDto>> UpdateAsync(Guid taskId, UpdateTaskRequest req, CurrentUser caller, CancellationToken ct);
    // Narrow write: status only. Permitted for the assignee. Audits TaskStatusChanged (from→to).
    Task<Result<TaskDetailDto>> UpdateStatusAsync(Guid taskId, UpdateTaskStatusRequest req, CurrentUser caller, CancellationToken ct);
    // Validates the candidate against the project's team pool (004) without mutating it. Audits TaskReassigned.
    Task<Result<TaskDetailDto>> ReassignAsync(Guid taskId, ReassignTaskRequest req, CurrentUser caller, CancellationToken ct);
    // Audits TaskDeleted BEFORE removal; the audit row is retained.
    Task<Result> DeleteAsync(Guid taskId, CurrentUser caller, CancellationToken ct);
}

// The scope AND mutation gates — the layer no [Authorize] attribute can express (T.2).
public interface ITaskAccessPolicy {
    IQueryable<TaskItem> ApplyScope(IQueryable<TaskItem> source, CurrentUser caller);   // Admin=all · PM=owned projects · TM=assigned
    Task<AccessDecision> CanReadAsync(TaskItem task, CurrentUser caller, CancellationToken ct);
    Task<AccessDecision> CanMutateAsync(TaskItem task, TaskMutation mutation, CurrentUser caller, CancellationToken ct);
}

// TaskQuery { int Page; int PageSize; Guid? ProjectId; TaskStatus? Status; Guid? AssigneeId; string? Search; string? Sort; }
// Result<T>, ErrorKind, CurrentUser, AccessDecision, and PagedResult<T> are defined once in
// docs/shared-contracts.md (ADR-0003) and reused verbatim — not redefined here.
// Entity → DTO mapping uses manual static extension methods; write DTOs are validated with
// FluentValidation (title, enum ranges, due-date window). See ADR-0005.
```
`IActivityLogService` is **reused from 001**; `IProjectService`/`Project` ownership comes from **002**. Neither is redefined here.

### B.4 Configuration (never hardcoded)
- `Tasks:Paging:{DefaultPageSize,MaxPageSize}` (20 / 100)
- `Tasks:DefaultStatus` (`ToDo`) · `Tasks:DefaultPriority` (`Medium`)
- `Tasks:EnforceStatusWorkflow` (default `false` — any status may move to any other; OQ-003-03)
- `Tasks:AllowUnassigned` (default `true`) · `Tasks:AllowAssignToInactiveUser` (default `false`)
- `Tasks:AllowWritesToTerminalStatusProject` (default `true`)
- `Tasks:MaskOutOfScopeAs404` (default `false` → return 403, matching 002)
- `Tasks:MaxTitleLength` / `MaxDescriptionLength`

### B.5 Error model (RFC 7807 Problem Details)
Produced by the shared `ErrorKind` → status mapper ([shared-contracts §1](../../docs/shared-contracts.md), ADR-0003):
`400` validation (per-field `errors` — title, enum range, due-date window, assignee not in pool, inactive assignee, paging bounds) · `401` `Authentication required` · `403` `Forbidden` (role gate) / scope denial / **mutation denial with the narrower right named** · `404` `Task not found` · `409` `Conflict` (stale row version) · `500` `Unexpected error`. Never leak an out-of-scope task's data in an error body.

### B.6 Non-functional requirements
- **Security:** deny-by-default; scope enforced in SQL; mutation kind enforced in the service and structurally by narrow DTOs; write-time re-check.
- **Performance:** list is a single round-trip with scope+filter+paging pushed to the database; indexes lead with `project_id`/`assignee_id`; no N+1 on project or assignee (projection or `Include`). Tasks is the highest-volume entity, so paging is mandatory from day one.
- **Observability:** structured logging via **Serilog**; authorization denials logged with actor, task id, attempted `TaskMutation`, and reason.
- **Testability (Constitution IX):** every `ITaskAccessPolicy` branch unit-tested — the full `TaskMutation` × role matrix (15 cells) is a table-driven xUnit test; each controller happy path + one error path via `WebApplicationFactory`; frontend `TasksService`, guards, and form validators via Jasmine+Karma.

### B.7 Audit event catalog (→ `activity_logs`, defined in 001)
Emit `(actor_id, action, entity_type='Task', entity_id, timestamp, change_summary)` for: **create** (`TaskCreated`), **full edit** (`TaskUpdated`, changed-field summary), **status change** (`TaskStatusChanged`, from→to), **reassignment** (`TaskReassigned`, from→to), **delete** (`TaskDeleted`, snapshot, written before removal). Reads are not audited. Append-only; audit rows are never cascaded away.

### B.8 Definition of Done
1. All eight routes exist and behave per the API-catalog status-code table; sub-resources use nouns, not verbs.
2. The three-role scope matrix is proven by integration tests: Admin sees all; a ProjectManager sees only tasks in owned projects; a TeamMember sees only assigned tasks.
3. **The graduated model is proven**: the *same* TeamMember on the *same* task receives **403** for `PUT /api/tasks/{id}` and **200** for `PUT /api/tasks/{id}/status`. The full `TaskMutation` × role matrix is table-driven tested.
4. A status request carrying extra fields (title, assigneeId) cannot alter them — proven by test, guaranteed by the narrow DTO.
5. `project_id` is taken from the route on create; a task cannot be created in a project the caller does not own.
6. Assignee validation holds: outside the project's team pool → 400; inactive user → 400; unassign permitted.
7. Due date outside the parent project's window → 400.
8. Out-of-scope rows never appear in items, `totalCount`, or paging metadata; `pageSize` is clamped; invalid paging → 400.
9. Every write produces an `activity_logs` row in the same transaction; delete audits before removal and the audit survives; deleting a **project** cascades its tasks while their audit rows remain.
10. Concurrent writes to one task are rejected with **409** (stale `xmin`), proven by an integration test — never a silent overwrite.
11. The Angular `tasks` route group is lazy-loaded with standalone components (no `@NgModule`); all HTTP lives in `TasksService`; create/edit use Reactive Forms with explicit validators; functional role guards are the only navigation block.
12. Errors are RFC 7807 via the shared mapper; all endpoints appear in Swagger; backend compiles warnings-as-errors with nullable enabled; frontend compiles in strict mode.
13. Unit + integration tests pass (Constitution IX.3 — no merge on failing tests).

### B.9 Open questions for review
| # | Question | Recommendation | Blocks build? |
|---|---|---|---|
| OQ-003-01 | Is the cross-project `GET /api/tasks` endpoint wanted, in addition to the nested project route? | **Resolved (Clarifications 2026-07-22):** yes — both endpoints are kept; see Clarifications for rationale | — |
| OQ-003-02 | Should `TaskItem` carry a separate `progress_percent` alongside `status`? | **No for v1** — `status` is the progress signal; add `progress_percent` only if the Dashboard needs finer granularity | No |
| OQ-003-03 | Enforce a status workflow (allowed transitions), or allow any status → any status? | **Resolved (Clarifications 2026-07-22): any → any** for v1, behind `Tasks:EnforceStatusWorkflow`, applying uniformly to Admin/ProjectManager/assignee — `Done` is not terminal for anyone; revisit if the demo needs a strict board | — |
| OQ-003-04 | Is the `TaskStatus` set right (`ToDo/InProgress/InReview/Done/Blocked`)? | Adopt as listed; confirm against the demo script and the Dashboard's chart buckets | No |
| OQ-003-05 | May a task move between projects? | **No** — `project_id` is immutable after creation; delete and recreate instead. Revisit if users ask | No |
| OQ-003-06 | Hard delete, or soft delete/archive? | **Hard delete**, consistent with 002; `activity_logs` preserves history | No |
| OQ-003-07 | `assignee_id` on user deletion — RESTRICT (chosen) or SET NULL? | **RESTRICT**, consistent with 002's `owner_id`; forces deliberate reassignment rather than silently unassigning work | No |
| OQ-003-08 | Should a TeamMember be able to change a task's **priority** as well as its status? | **No** — priority is planning, which belongs to the ProjectManager. Trivial to widen later via the `TaskMutation` matrix | No |

---

## Functional Requirements

- **FR-001**: The system MUST expose `GET /api/projects/{projectId}/tasks`, `GET /api/tasks`, `GET /api/tasks/{id}`, `POST /api/projects/{projectId}/tasks`, `PUT /api/tasks/{id}`, `PUT /api/tasks/{id}/status`, `PUT /api/tasks/{id}/assignee`, and `DELETE /api/tasks/{id}`.
- **FR-002**: A Task MUST have a title, description, status, priority, due date, parent project, and assignee; `title`, `project_id`, `status`, and `priority` are required; `description`, `due_date`, and `assignee_id` are optional.
- **FR-003**: On create, `project_id` MUST be taken from the route and MUST NOT be accepted from the request body; `project_id` MUST be immutable thereafter.
- **FR-004**: An assignee MUST be a team member on the task's project and MUST be active; violations return **400**.
- **FR-005**: A task's `due_date`, when set, MUST fall within the parent project's start/end window; violations return **400**.
- **FR-006**: Role checks MUST be declared with `[Authorize(Roles = "...")]` attributes only; scope (project ownership / assignment) and **mutation-kind** checks MUST be enforced in the service layer via `AccessDecision`, never in the controller.
- **FR-007**: TeamMember MUST be denied create, full edit, reassign, and delete with **403**, **and** MUST be permitted to change the status of tasks assigned to them.
- **FR-008**: Status updates MUST use a dedicated status-only endpoint and DTO so that no other field can be altered by that request.
- **FR-009**: Listing MUST be role-scoped server-side: Admin → all; ProjectManager → tasks in owned projects; TeamMember → tasks assigned to them. Query filters MUST only narrow the scope, never widen it.
- **FR-010**: The scope filter MUST be applied inside the database query so out-of-scope tasks never appear in items, `totalCount`, or paging metadata.
- **FR-011**: Both collection endpoints MUST return the shared `PagedResult<T>` envelope with `?page`/`?pageSize` (default 20, maximum 100, clamped not rejected) per Constitution VI.4.
- **FR-012**: Update, status change, reassignment, and delete MUST re-check scope and mutation kind at write time.
- **FR-013**: An unknown task id MUST return **404**; an existing task outside the caller's scope MUST return **403** (maskable to 404 by configuration).
- **FR-014**: Every write to Tasks MUST create an `activity_logs` entry (actor, action, entity type, entity id, timestamp, change summary) in the same transaction; deletes MUST audit before removal and the entry MUST survive.
- **FR-015**: Deleting a **project** MUST cascade-delete its tasks; deleting a **user** who is assigned tasks MUST be restricted until those tasks are reassigned or removed.
- **FR-016**: Updating a task MUST use optimistic concurrency (`xmin`); a stale write MUST return **409 Conflict** rather than silently overwriting (ADR-0004).
- **FR-017**: Errors MUST be RFC 7807 Problem Details produced by the shared `ErrorKind` mapper; all endpoints MUST be documented via Swagger/OpenAPI.
- **FR-018**: The Angular `tasks` feature area MUST be lazy-loaded via route-level code splitting with standalone components (ADR-0001); all HTTP MUST live in a dedicated `TasksService` (never in components); create/edit MUST use Reactive Forms with explicit validators; a functional role-based route guard MUST be the only mechanism blocking navigation.
- **FR-019**: All persistence MUST go through EF Core with a Code-First migration; no raw SQL and no manual DDL (Constitution IV.1, IV.2).

## Non-Functional Requirements
- **NFR-001**: Server-side enforcement is authoritative; the frontend never filters or gates for security.
- **NFR-002**: List queries execute as a single round-trip with scope, filter, and paging translated to SQL; no in-memory filtering and no N+1 on project or assignee.
- **NFR-003**: Structured logging (Serilog); authorization denials logged with actor, task id, attempted mutation, and reason.
- **NFR-004**: Nullable reference types + warnings-as-errors (backend); TypeScript strict mode (frontend).
- **NFR-005**: Indexes lead with `project_id`/`assignee_id` so role-scoped listing stays performant as Tasks becomes the largest table.

## Configurability Rules
- **CFG-001**: Default and maximum `pageSize`; default sort (due date ascending).
- **CFG-002**: Default status and priority on create.
- **CFG-003**: Whether a status workflow (allowed transitions) is enforced.
- **CFG-004**: Whether unassigned tasks are permitted; whether assignment to an inactive user is permitted.
- **CFG-005**: Whether writes are permitted on tasks in a terminal-status project.
- **CFG-006**: Out-of-scope reads return 403 or are masked as 404.
- **CFG-007**: Max lengths for `title` and `description`.

## Security Rules
- Authenticated by default; role gate via attributes only; scope **and** mutation gates in the service layer.
- Narrow endpoints bind narrow DTOs, so privilege cannot be widened by payload.
- `project_id` from the route on create; immutable thereafter.
- Out-of-scope rows excluded inside the query — no leakage via counts or paging.
- Scope and mutation kind re-checked at write time; deny by default.
- Assignment validated against feature 004's pool without mutating it; reassignment audited from→to.

## Audit / Compliance Expectations
Audit every write to Tasks — create, full edit, status change, reassignment, delete — with actor, action, entity type (`Task`), entity id, timestamp, and change summary to `activity_logs` (defined in 001), written in the same transaction as the change. Status changes and reassignments record **from→to** so progress and transfers of access are reconstructable. Reads are not audited. Append-only; audit rows survive cascade deletion of the task or its parent project, keeping deleted work reportable by 006.

## Assumptions
- 001 Auth & RBAC and 002 Projects are implemented: `users`, the three-role model, the single JWT `role` claim, `ICurrentUserService`, `IActivityLogService`, and `Project.owner_id` all exist and are consumed here.
- All five constitution entities are created in the initial migration; a feature owns an entity's API/UI/rules, not its table's existence — so the `team_members` validation join is available even though feature 004 owns assignment management. Until assignments exist, `assigneeId` validation legitimately rejects every candidate and tasks are created unassigned.
- The CLR entity is named `TaskItem` to avoid colliding with `System.Threading.Tasks.Task`.
- Tasks is expected to be the highest-volume table, so pagination is mandatory from day one (Constitution VI.4).
- A task belongs to exactly one project and has at most one assignee.

## Dependencies
- **Depends on**: [001 Auth & RBAC](../001-auth-rbac/spec.md) — Users, role model, JWT claims, `CurrentUser`, `IActivityLogService`. · [002 Projects](../002-projects/spec.md) — the `Project` entity and the ownership rule that defines a ProjectManager's scope.
- **Reads (does not mutate)**: feature 004's `team_members` pool, to validate assignees.
- **Consumed by**: 005 Dashboard (task counts/aggregates by status, assignee, project) · 006 Reports (task exports). Both inherit this feature's scoping rules.
- **Infrastructure**: PostgreSQL 18 via EF Core 10 + Npgsql; Serilog; Swagger/OpenAPI.

## Out of Scope
Managing the team-member/assignment pool — creating or removing `team_members` rows (004); the Project entity and ownership rule (002); authentication and the role model (001); dashboard aggregation (005); report export (006); Gantt/timeline views, task dependencies, sub-tasks, comments, attachments, time tracking, and recurring tasks (bonus scope, Constitution I.2).

---

## Sequence Note

This is the **third** module in the sequence (001 Auth & RBAC and 002 Projects complete). It follows the structural template set by [001](../001-auth-rbac/spec.md) and the merged-file convention. **004 Team** follows next and owns the `team_members` pool this spec validates against; this file's `Task` entity then feeds **005 Dashboard** and **006 Reports**.
