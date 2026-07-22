# Feature Specification: Project Management

**Feature Number**: 002
**Feature Name**: Project Management (Projects CRUD & Role-Scoped Access)
**Phase**: Phase 1 — Foundation
**Priority**: Must Have (Critical)
**Complexity**: Medium-High
**Type**: Core domain / CRUD + resource-level authorization
**Depends On**: **001 Auth & RBAC** — Users, the three-role model, JWT `role` claim, `[Authorize]` conventions, and the ActivityLog audit pattern are defined there and are **referenced, not redefined**
**Enables**: 003 Tasks · 004 Team · 005 Dashboard · 006 Reports (all anchor on the Project entity)
**Created**: 2026-07-22
**Status**: Draft — Ready for Planning
**Governed By**: Project Constitution v1.1.0 (Principles II Architecture, III Stack, IV Data Access, V Security & Authorization, VI API Design, VII Frontend, VIII Code Quality, IX Testing)
**Cross-cutting contracts**: [docs/shared-contracts.md](../../docs/shared-contracts.md) — `Result<T>`, `CurrentUser`, `AccessDecision`, `PagedResult<T>`, error→HTTP mapping · ADRs [0001](../../docs/adr/0001-angular-standalone-components.md) standalone Angular · [0002](../../docs/adr/0002-same-origin-hosting.md) same-origin hosting · [0003](../../docs/adr/0003-result-error-contract.md) error contract · [0004](../../docs/adr/0004-optimistic-concurrency.md) concurrency · [0005](../../docs/adr/0005-mapping-and-validation.md) mapping/validation
**Generated Via**: `/speckit.specify` (merged requirements + solution design, per project convention)

---

## Purpose

Own the **Project** — the anchor entity of ProjectManagementApp. This feature provides full lifecycle management of projects (create, list/search, view, edit, delete) and, critically, establishes the **resource-level authorization** pattern the rest of the application follows: a role gate declared with `[Authorize(Roles = "...")]` at the controller, plus an **ownership/assignment check in the service layer** for scoping that a role attribute alone cannot express.

Every other domain feature hangs off this one — Tasks belong to a Project, TeamMembers are assigned to a Project, the Dashboard aggregates Projects, and Reports export them.

This is a **merged specification**: requirements (Purpose → User Stories → Functional Requirements) and solution design (Technical Design → Implementation Blueprint) live in one file by project convention, so the team reviews *what* and *how* together.

## Business Value

Projects are the unit of work the entire product organizes around; without them there is nothing to assign tasks to, staff with team members, chart on a dashboard, or export in a report. Beyond the CRUD, this feature delivers the **two-layer authorization model** that keeps a ProjectManager confined to their own projects and a TeamMember confined to projects they are assigned to — enforced server-side, so the guarantee holds even when the client is bypassed. Auditing every write to Projects makes ownership and lifecycle changes traceable, and role-scoped listing means each user sees a workspace that is already filtered to what they are permitted to act on.

## Actors

**Primary Actors**
- **Admin** — full CRUD on **all** projects regardless of owner; may create a project on behalf of any owner and reassign ownership.
- **ProjectManager** — creates projects (and **becomes the owner**); reads, updates, and deletes **only projects they own**.
- **TeamMember** — **read-only** access to projects they are assigned to via TeamMembers; no create, update, or delete.

**Secondary Actors**
- **Consuming features (non-actor)** — 003 Tasks, 004 Team, 005 Dashboard, 006 Reports read the Project entity and rely on this feature's scoping rules as their anchor.

## Scope Summary

**In scope**: the `Project` entity (name, description, start date, end date, status) and its Code-First migration; the five endpoints named in the brief and Constitution VI.6 (`GET /api/projects`, `GET /api/projects/{id}`, `POST /api/projects`, `PUT /api/projects/{id}`, `DELETE /api/projects/{id}`); role-scoped listing (Admin → all, ProjectManager → owned, TeamMember → assigned) with `?page`/`?pageSize` pagination plus search and status filtering; project ownership (`owner_id`) and the service-layer ownership check; the two-layer authorization model (role attribute + service-layer scope); an `ActivityLog` entry on every write to Projects; optimistic concurrency on the Project row (ADR-0004); the lazy-loaded Angular **`projects` route group** (standalone components per ADR-0001) with a dedicated `ProjectsService`, Reactive Forms for create/edit, and a role-based functional route guard.

**Out of scope**: task details and task management (feature 003 — Tasks reference Projects as their anchor); team-member assignment management, i.e. creating/removing the TeamMembers rows themselves (feature 004 — this feature only *reads* those rows to scope a TeamMember's visibility); dashboard aggregation (005) and report export (006); anything already owned by 001 Auth & RBAC (authentication, the role model, JWT issuance, the `users` table, the `activity_logs` table definition); project templates, cloning, archiving/restore, attachments, comments, and Gantt charts (bonus scope per Constitution I.2).

## Access Logic (applies to every story)

Enforced **server-side**, in this order:

1. **Authenticated by default** — every endpoint in this feature requires a valid JWT (inherited from 001; the global fallback policy requires an authenticated user). No token / invalid / expired → **401**.
2. **Role gate (controller, attribute-only)** — declared with `[Authorize(Roles = "...")]`. Write endpoints permit `Admin,ProjectManager`; read endpoints permit all three roles. A role that is not permitted → **403**. Ad-hoc role checks in method bodies remain prohibited (Constitution V.2).
3. **Ownership / assignment gate (service layer)** — the role attribute cannot express *"only the projects you own"* or *"only the projects you're assigned to"*, so the **service layer** applies it: Admin is unscoped; ProjectManager is scoped to `owner_id == caller`; TeamMember is scoped to projects with a matching TeamMembers assignment. A permitted role acting on a project outside its scope → **403**. This check lives in the service, never in the controller (Constitution II.2).
4. **Identity from the token** — the acting user id and role come from the validated JWT. `owner_id` is **derived server-side**, never accepted from the request body for a ProjectManager.
5. **Deny by default** — if scope cannot be established, the request is denied. Frontend route guards and role-scoped list views are convenience only; the API re-checks every request.

## Role & Permission Model

The three roles are defined in [001 Auth & RBAC](../001-auth-rbac/spec.md) — each user holds **exactly one** role, carried as a single JWT `role` claim. This feature adds no new roles; it maps the existing three onto Project operations:

| Operation | Admin | ProjectManager | TeamMember |
|---|---|---|---|
| `GET /api/projects` (list) | All projects | Only projects they own | Only projects they are assigned to |
| `GET /api/projects/{id}` | Any project | Only if owner | Only if assigned |
| `POST /api/projects` | ✔ (may set any owner) | ✔ (becomes the owner) | ✘ **403** |
| `PUT /api/projects/{id}` | Any project | Only if owner | ✘ **403** |
| `DELETE /api/projects/{id}` | Any project | Only if owner | ✘ **403** |

**Ownership** is a first-class concept introduced by this feature: `Project.owner_id` references the `users` table from 001. A ProjectManager's authority is defined by ownership; a TeamMember's read access is defined by assignment (the TeamMembers link owned by feature 004).

---

## User Stories

> Story IDs `US-002-01..05`. Each story: **A** Summary · **B** Quality Validation (INVEST · Given-When-Then · edge cases · audit/security · configurability) · **C** UI · **D** API · **E** DB · **F** Separation of concerns. Consolidated schema, API catalog, technical design, and implementation blueprint follow the stories.

---

### US-002-01 — Create a project

**A. Summary**
- **Story ID**: US-002-01 · **Title**: Create a new project
- **Actor**: ProjectManager (becomes the owner); Admin (may create for any owner)
- **User story**: *As a ProjectManager, I want to create a project with its name, description, dates, and status, so that I have a container to plan work, assign people, and track progress against.*
- **Business value**: The entry point to the entire domain — nothing else in the product exists without a project.
- **Priority**: **P0** · **Reason**: Every downstream feature anchors on a Project.
- **Dependencies**: 001 (authenticated caller + role claim). **Out of scope**: adding tasks (003) or team members (004) during creation.

**B. Quality validation**
- **INVEST** — Independent ✔ (creation stands alone); Negotiable ✔ (field set, default status); Valuable ✔; Estimable ✔; Small ✔ (single record + audit); Testable ✔ (row created, owner correct, audit written, TeamMember blocked).
- **Given/When/Then**
  1. **Given** an authenticated ProjectManager and a valid payload, **When** they create a project, **Then** a `projects` row is created with `owner_id` = **the caller** (taken from the token, not the body), and an `activity_logs` entry (actor, action `ProjectCreated`, entity `Project`, entity id, timestamp, summary) is written.
  2. **Given** an Admin supplying an explicit `ownerId`, **When** they create a project, **Then** the project is created with that owner; if `ownerId` is omitted the Admin becomes the owner.
  3. **Given** a **TeamMember**, **When** they attempt to create a project, **Then** the request is rejected with **403** and nothing is written.
  4. **Given** a payload where `endDate` precedes `startDate`, or `name` is missing/blank, **When** creating, **Then** **400** with field-level validation errors (RFC 7807); nothing is stored.
  5. **Given** a successful creation, **When** the response is returned, **Then** it is **201 Created** with a `Location: /api/projects/{id}` header and the created project.
- **Edge cases**: a ProjectManager attempting to set `ownerId` to another user (**ignored — always self**); `ownerId` referencing a non-existent or non-ProjectManager user (Admin path → **400**); omitted `status` → defaults to `Planning`; omitted `endDate` (allowed — open-ended project); oversized `name`/`description`; duplicate project names (**permitted** — names are not unique).
- **Audit/security**: creation audited; `owner_id` derived from the token for ProjectManager (privilege escalation impossible); role gate `[Authorize(Roles = "Admin,ProjectManager")]`.
- **Configurability**: default status on create; max lengths for `name`/`description`; whether an Admin may assign ownership to a non-ProjectManager user.

**C. UI** — **F002-S02 Create Project** (standalone component in the lazy-loaded `projects` route group). Reactive form: `name`, `description`, `startDate`, `endDate`, `status`; explicit validators (required name, date-order cross-field validator, max lengths); errors surfaced via the shared error-display component; the owner field is shown only to Admin; submit disabled while pending; on success routes to the new project's detail view.

**D. API** — `POST /api/projects` · `[Authorize(Roles = "Admin,ProjectManager")]` · **201 Created** + `Location: /api/projects/{id}` + project DTO.

**E. DB** — writes **`projects`** (with `owner_id` FK → `users`), **`activity_logs`** (audit).

**F. Separation** — UI: create form + validators. Backend: `IProjectService.CreateAsync` (validation, owner resolution from token, persist, audit) — controller only binds/validates and delegates. DB: project row + audit row. QA: owner-from-token, TeamMember 403, date-order 400, 201 + Location, audit written.

---

### US-002-02 — List and search projects (role-scoped)

**A. Summary**
- **Story ID**: US-002-02 · **Title**: List, search, and page through the projects I may see
- **Actor**: Admin · ProjectManager · TeamMember (each with a different scope)
- **User story**: *As any authenticated user, I want a searchable, paginated list of the projects I'm permitted to see, so that I can find and act on the right project quickly without seeing others' work.*
- **Business value**: The workspace entry point; also the feature that proves role-scoping works.
- **Priority**: **P0** · **Reason**: Every project workflow starts from find/list.
- **Dependencies**: US-002-01; feature 004 for TeamMember assignment rows (read-only here). **Out of scope**: bulk actions, saved filters.

**B. Quality validation**
- **INVEST** — Independent ✔; Negotiable ✔ (filter/sort set); Valuable ✔; Estimable ✔; Small ✔ (read path); Testable ✔ (scope per role is directly assertable).
- **Given/When/Then**
  1. **Given** projects owned by several managers, **When** an **Admin** lists, **Then** **all** projects are returned (subject to paging).
  2. **Given** the same data, **When** a **ProjectManager** lists, **Then** **only projects they own** are returned — another manager's projects never appear.
  3. **Given** the same data, **When** a **TeamMember** lists, **Then** **only projects they are assigned to** (via TeamMembers) are returned; with no assignments the result is an **empty list, not an error**.
  4. **Given** `?page=2&pageSize=20`, **When** listing, **Then** the correct slice is returned along with total-count metadata, and the scope filter is applied **before** paging (so counts are per-scope).
  5. **Given** `?search=` and/or `?status=`, **When** listing, **Then** results match the term (name/description) and status **within** the caller's scope.
- **Edge cases**: `pageSize` above the configured maximum (clamped); `page` beyond the last page (empty items, valid metadata); non-numeric/negative paging values (**400**); search term with special characters; no matches (empty state, not 404); the TeamMembers table not yet populated (feature 004 pending) → TeamMember sees an empty list.
- **Audit/security**: the scope filter is applied **server-side in the query**, never by the client; reads are not audited (only writes are, per Constitution IV.4); no project outside scope may be inferred from counts or paging metadata.
- **Configurability**: default `pageSize`, maximum `pageSize`, default sort order, searchable fields.

**C. UI** — **F002-S01 Project List**. Table with search box, status filter, sort, and paginator; the view simply renders what the API returns — **it applies no client-side role filtering** (the API is already scoped). Empty, loading, and error states are explicit. "New Project" action is hidden for TeamMember (UX only — the API still enforces 403).

**D. API** — `GET /api/projects?page=&pageSize=&search=&status=&sort=` · `[Authorize]` (all three roles) · **200** with a paged envelope.

**E. DB** — reads **`projects`**; joins **`team_members`** (owned by feature 004) for TeamMember scoping. Indexes lead with `owner_id` and `status`.

**F. Separation** — UI: list + filters + paginator + states. Backend: `IProjectAccessPolicy.ApplyScope` composes the scope predicate; `IProjectService.ListAsync` applies search/filter/sort/paging on top. DB: indexed scope + paging query. QA: three-role scope matrix (the primary acceptance test), paging metadata, empty state, clamped `pageSize`.

---

### US-002-03 — View project detail

**A. Summary**
- **Story ID**: US-002-03 · **Title**: View a single project's details
- **Actor**: Admin (any) · ProjectManager (owned) · TeamMember (assigned)
- **User story**: *As a permitted user, I want to open a project and see its full detail, so that I understand its scope, dates, status, and owner before acting on it.*
- **Business value**: The anchor screen that Tasks, Team, and Reports will later extend.
- **Priority**: **P0** · **Reason**: Required before edit/delete and by every downstream feature.
- **Dependencies**: US-002-01. **Out of scope**: showing the project's tasks (003) or team roster (004) — those features add their own panels later.

**B. Quality validation**
- **INVEST** — all ✔ (single read).
- **Given/When/Then**
  1. **Given** a project they may see, **When** a user opens it, **Then** **200** with name, description, start/end dates, status, owner, and created/updated timestamps.
  2. **Given** a project id that **does not exist**, **When** requested, **Then** **404**.
  3. **Given** a project that exists but is **outside the caller's scope** (a ProjectManager who is not the owner, or an unassigned TeamMember), **When** requested, **Then** **403**.
  4. **Given** an Admin, **When** they request any project id, **Then** it is returned regardless of owner.
- **Edge cases**: malformed (non-GUID) id → **400**; a project whose owner has been deactivated (still returned, owner shown as inactive); concurrent deletion between list and open → **404**.
- **Audit/security**: scope enforced in the service before the entity is returned; the **403-vs-404 distinction is deliberate** (see OQ-002-03) — a 403 confirms existence to a permitted role, which is acceptable within a single organization's workspace.
- **Configurability**: whether out-of-scope reads return **403** or are masked as **404** (config flag; default **403**).

**C. UI** — **F002-S03 Project Detail** (read-only view). Shows all fields plus owner and timestamps; Edit/Delete actions render only for users whose role permits them (UX only). Loading/error/not-found/forbidden states are explicit.

**D. API** — `GET /api/projects/{id}` · `[Authorize]` (all three roles) · **200** project DTO · **403** out of scope · **404** unknown id.

**E. DB** — reads **`projects`** (+ `users` for owner display); joins **`team_members`** for TeamMember scope resolution.

**F. Separation** — UI: detail view + states. Backend: `IProjectService.GetByIdAsync` → `IProjectAccessPolicy.CanReadAsync`. DB: single read + scope join. QA: 200/403/404 matrix per role, malformed id, deleted-in-flight.

---

### US-002-04 — Edit a project

**A. Summary**
- **Story ID**: US-002-04 · **Title**: Update a project's details
- **Actor**: Admin (any project) · ProjectManager (own projects only)
- **User story**: *As a ProjectManager, I want to update my project's details and status, so that the record reflects reality as the work progresses.*
- **Business value**: Keeps the authoritative project record accurate; status changes drive the dashboard and reports.
- **Priority**: **P0** · **Reason**: Core maintenance.
- **Dependencies**: US-002-01. **Out of scope**: editing tasks (003) or the team roster (004).

**B. Quality validation**
- **INVEST** — all ✔ (field update only).
- **Given/When/Then**
  1. **Given** a project they own, **When** a ProjectManager updates it with a valid payload, **Then** the row is updated, `updated_at` is refreshed, and an `activity_logs` entry (`ProjectUpdated`) records the change summary.
  2. **Given** a project owned by **someone else**, **When** a ProjectManager updates it, **Then** **403** and nothing changes.
  3. **Given** any project, **When** an **Admin** updates it, **Then** it succeeds regardless of owner.
  4. **Given** a **TeamMember**, **When** they attempt an update, **Then** **403** (blocked at the role gate).
  5. **Given** an invalid payload (blank name, `endDate` before `startDate`), **When** updating, **Then** **400** with field errors; nothing changes.
- **Edge cases**: updating a project to a terminal status (`Completed`/`Cancelled`) — permitted, no cascade in this feature; changing `owner_id` — **Admin only** (a ProjectManager cannot transfer away or claim ownership); no-op update (identical values) still refreshes `updated_at` and audits; unknown id → **404**; **two users editing the same project — the second write is rejected with 409** (stale `xmin` row version, ADR-0004) rather than silently overwriting.
- **Audit/security**: every update audited with a change summary; ownership re-checked **at write time** in the service (not trusted from the earlier read); `owner_id` changes rejected for non-Admin.
- **Configurability**: which fields are editable after a terminal status; whether ownership transfer is permitted (Admin-only by default).

**C. UI** — **F002-S04 Edit Project**. Reactive form pre-populated from the detail response; same validators as create; the owner field is editable only for Admin; unsaved-changes guard; errors via the shared error-display component.

**D. API** — `PUT /api/projects/{id}` · `[Authorize(Roles = "Admin,ProjectManager")]` · **200** with the updated project · **403** / **404** / **400** / **409** (stale row version) as applicable.

**E. DB** — updates **`projects`**; writes **`activity_logs`**.

**F. Separation** — UI: edit form + guard. Backend: `IProjectService.UpdateAsync` → `IProjectAccessPolicy.CanMutateAsync` → persist → audit. DB: update + audit. QA: cross-owner 403, TeamMember 403, validation 400, ownership-transfer rule, audit summary.

---

### US-002-05 — Delete a project

**A. Summary**
- **Story ID**: US-002-05 · **Title**: Delete a project
- **Actor**: Admin (any project) · ProjectManager (own projects only)
- **User story**: *As a ProjectManager, I want to delete a project I own, so that abandoned or mistaken projects don't clutter the workspace.*
- **Business value**: Keeps the workspace clean; the audit trail preserves the record of what was removed.
- **Priority**: **P1** · **Reason**: Important, but builds on create/read.
- **Dependencies**: US-002-01. **Out of scope**: restore/undelete; bulk delete.

**B. Quality validation**
- **INVEST** — all ✔.
- **Given/When/Then**
  1. **Given** a project they own, **When** a ProjectManager deletes it, **Then** the row is removed, **204 No Content** is returned, and an `activity_logs` entry (`ProjectDeleted`) is written **before** the row disappears (audit survives the delete).
  2. **Given** a project owned by someone else, **When** a ProjectManager deletes it, **Then** **403** and nothing is removed.
  3. **Given** any project, **When** an **Admin** deletes it, **Then** it succeeds regardless of owner.
  4. **Given** a **TeamMember**, **When** they attempt a delete, **Then** **403**.
  5. **Given** an unknown id, **When** deleting, **Then** **404**.
- **Edge cases**: deleting a project that has dependent Tasks (003) and TeamMembers (004) rows — **cascade is explicit and intentional** (Constitution IV.3): dependents are removed with the project; deleting twice (second call → **404**); concurrent delete/update race (last writer observes **404**).
- **Audit/security**: deletion audited with a summary snapshot of the project **before** removal; ownership re-checked at write time; `activity_logs` rows are **retained** (they are not cascaded away) so the history of a deleted project survives.
- **Configurability**: hard delete vs. soft delete/archive (default **hard delete with cascade** — see OQ-002-01); whether delete is blocked when dependent tasks exist.

**C. UI** — delete action on **F002-S03 Project Detail** and in the list row menu, behind a confirmation dialog naming the project and warning that its tasks and assignments will be removed. Rendered only for permitted roles (UX only).

**D. API** — `DELETE /api/projects/{id}` · `[Authorize(Roles = "Admin,ProjectManager")]` · **204 No Content** · **403** / **404** as applicable.

**E. DB** — deletes from **`projects`** (cascading to dependent `tasks` / `team_members` rows owned by features 003/004); writes **`activity_logs`** (retained).

**F. Separation** — UI: confirm dialog + list refresh. Backend: `IProjectService.DeleteAsync` → `CanMutateAsync` → audit → delete. DB: cascade declared explicitly in the EF model. QA: cross-owner 403, cascade verified, audit-before-delete, double-delete 404.

---

## Consolidated Data Model (review-level; final physical schema at implementation)

> Code-First (EF Core 10 + Npgsql). PostgreSQL identifiers are **snake_case** (Constitution VIII.2). `users` and `activity_logs` are defined by [001 Auth & RBAC](../001-auth-rbac/spec.md) and are **referenced here, not redefined**. Every schema change is an EF Core migration with a descriptive name (Constitution IV.2), e.g. `AddProjectsTable`.

| Entity | Table | Purpose | Key fields (type · req/null) | Relationships |
|---|---|---|---|---|
| **Project** (this feature) | `projects` | The anchor domain entity | `id` uuid PK; `name` varchar(200) (req); `description` varchar(2000) (null); `start_date` date (req); `end_date` date (null); `status` (req · enum, default `Planning`); `owner_id` uuid FK→`users` (req); `created_at`/`updated_at` timestamptz (req) | *→1 `users` (owner); 1→* `tasks` (003); 1→* `team_members` (004) |
| **User** (from 001) | `users` | Owner reference only | `id` uuid PK (referenced) | 1→* `projects` as owner |
| **TeamMember** (from 004) | `team_members` | Read-only here — resolves a TeamMember's project scope | `project_id` FK; `user_id` FK | *→1 `projects`; *→1 `users` |
| **ActivityLog** (from 001) | `activity_logs` | First-class audit of every write | `actor_id`, `action`, `entity_type='Project'`, `entity_id`, `timestamp`, `change_summary` | references `projects` (soft, by id) |

**Schema notes**: `owner_id` is **required** — a project always has exactly one owner. Deleting a **project** cascades to its dependent `tasks` and `team_members` rows (explicit and intentional, Constitution IV.3); deleting a **user** does **not** cascade to projects (restricted — ownership must be reassigned first, so projects are never orphaned or silently destroyed). `activity_logs` rows are never cascaded away, so a deleted project's history survives. Indexes: `(owner_id)`, `(status)`, `(owner_id, status)`, and a text index supporting `name` search.

## Consolidated API Catalog

> Base path `/api`; JSON over HTTPS; errors as **RFC 7807 Problem Details**; documented via **Swagger/OpenAPI**. Authenticated by default (001). The five routes below are named explicitly in the brief and **Constitution VI.6** and must exist exactly as written.

| Method · Route | Purpose | Role gate | Service-layer scope | Success | Failure |
|---|---|---|---|---|---|
| `GET /api/projects` | List/search, paged | `[Authorize]` (all 3) | Admin all · PM owned · TM assigned | **200** paged envelope | 400 (bad paging), 401 |
| `GET /api/projects/{id}` | Project detail | `[Authorize]` (all 3) | Admin any · PM owned · TM assigned | **200** project DTO | 400, 401, 403, 404 |
| `POST /api/projects` | Create | `[Authorize(Roles="Admin,ProjectManager")]` | owner = caller (PM) / any (Admin) | **201** + `Location` | 400, 401, 403 |
| `PUT /api/projects/{id}` | Update | `[Authorize(Roles="Admin,ProjectManager")]` | Admin any · PM owned | **200** updated DTO | 400, 401, 403, 404, **409** (stale row version) |
| `DELETE /api/projects/{id}` | Delete (cascades) | `[Authorize(Roles="Admin,ProjectManager")]` | Admin any · PM owned | **204** | 401, 403, 404 |

---

## Technical Design — Two-Layer Authorization & Role-Scoped Data Access

> The detailed solution: the components, the exact requests/responses, how scoping is composed into the query, the step-by-step write flow, failure handling, and the security guarantees. Written so a developer can implement it directly.

### T.1 The roles (who is authority, who enforces)
- **The .NET API is the authority.** The controller declares the **role gate**; the **service layer** owns the ownership/assignment gate and every business rule. The controller never contains business logic beyond model binding, validation, and delegation (Constitution II.2).
- **The Angular frontend is convenience.** Route guards and conditionally rendered buttons shape what a user *sees*; the API re-checks every request. A TeamMember who hand-crafts a `POST /api/projects` still gets **403**.

### T.2 The two-layer model (the heart of this feature)
Role and scope are **different questions**, and only the first fits in an attribute:

- **Layer 1 — Role gate (attribute, controller).** *"May this kind of user perform this kind of operation at all?"* Expressed declaratively: `[Authorize(Roles = "Admin,ProjectManager")]` on write endpoints. This is the only place a role is checked — no `if (role == ...)` in method bodies (Constitution V.2).
- **Layer 2 — Scope gate (service).** *"May this specific user act on **this specific project**?"* Ownership and assignment are **row-level facts**, not roles, so no attribute can express them. The service resolves them in two complementary ways:
  - **For reads of collections** — a **scope predicate composed into the query** (`ApplyScope`), so out-of-scope rows are never loaded, counted, or paged. Filtering after the fact would leak totals.
  - **For single-entity reads and all writes** — an explicit **decision check** (`CanReadAsync` / `CanMutateAsync`) evaluated **at the moment of the operation**, so a stale earlier read cannot authorize a later write.

> **Why not a policy/requirement handler?** ASP.NET Core resource-based authorization is a valid alternative, but the constitution places business rules in Services (II.2), and scope here is a query concern (it must fold into the `IQueryable` for correct paging). Keeping both halves in one `IProjectAccessPolicy` used by the service keeps the rule in exactly one place. Recorded as an ADR-worthy decision (OQ-002-02).

### T.3 The endpoints, with concrete examples

**(1) Create**
```
POST /api/projects        Authorization: Bearer eyJ...   (role=ProjectManager, sub=7c2f...a1)
{ "name": "Apollo Rollout", "description": "Regional launch",
  "startDate": "2026-08-01", "endDate": "2026-11-30", "status": "Planning" }

→ 201 Created
Location: /api/projects/4d9b1e77-...-c3
{ "id": "4d9b1e77-...-c3", "name": "Apollo Rollout", "description": "Regional launch",
  "startDate": "2026-08-01", "endDate": "2026-11-30", "status": "Planning",
  "owner": { "id": "7c2f...a1", "fullName": "Priya Nair" },
  "createdAt": "2026-07-22T09:40:00Z", "updatedAt": "2026-07-22T09:40:00Z" }
```
`ownerId` in the body is **ignored** for a ProjectManager — the owner is taken from the token's `sub`. A TeamMember calling this receives **403** at the role gate.

**(2) List (role-scoped + paged)**
```
GET /api/projects?page=1&pageSize=20&search=apollo&status=Planning
Authorization: Bearer eyJ...

→ 200 OK
{ "items": [ { "id": "4d9b1e77-...-c3", "name": "Apollo Rollout", "status": "Planning",
               "startDate": "2026-08-01", "endDate": "2026-11-30",
               "owner": { "id": "7c2f...a1", "fullName": "Priya Nair" } } ],
  "page": 1, "pageSize": 20, "totalCount": 1, "totalPages": 1 }
```
`totalCount` is the count **within the caller's scope** — a ProjectManager can never learn how many projects exist overall.

**(3) Detail**
```
GET /api/projects/4d9b1e77-...-c3     Authorization: Bearer eyJ...

→ 200  (full project DTO)
→ 403  { "title": "Forbidden", "detail": "You do not have access to this project." }
→ 404  (no project with that id)
```

**(4) Update**
```
PUT /api/projects/4d9b1e77-...-c3     Authorization: Bearer eyJ...
{ "name": "Apollo Rollout", "description": "Regional launch — phase 2",
  "startDate": "2026-08-01", "endDate": "2026-12-15", "status": "Active" }

→ 200 OK   (updated project DTO; updated_at refreshed)
→ 403      (a ProjectManager who does not own it)
```

**(5) Delete**
```
DELETE /api/projects/4d9b1e77-...-c3  Authorization: Bearer eyJ...

→ 204 No Content    (project removed; dependent tasks/assignments cascade; audit row retained)
→ 403 / 404         (not owner / unknown id)
```

### T.4 How a role-scoped list is built (step by step)
1. The JWT is validated; `sub` and the single `role` claim are materialized (001).
2. The role gate admits the caller (all three roles may list).
3. `IProjectAccessPolicy.ApplyScope(query, caller)` composes the scope **into the `IQueryable`**:
   - `Admin` → no predicate (all rows).
   - `ProjectManager` → `p => p.OwnerId == caller.UserId`.
   - `TeamMember` → `p => p.TeamMembers.Any(tm => tm.UserId == caller.UserId)`.
4. Search (`name`/`description`), `status` filter, and sort are applied **on top of** the scoped query.
5. `totalCount` is computed from the scoped+filtered query, **then** `Skip/Take` paging is applied — so paging metadata never reveals out-of-scope rows.
6. A single database round-trip returns the page; EF Core translates the whole composition to SQL (no in-memory filtering).

### T.5 How a write flows (step by step)
1. The role gate admits the caller (`Admin` or `ProjectManager`); a TeamMember is rejected **403** before the action runs.
2. The controller binds the model, runs its **FluentValidation** validator (including the cross-field `endDate >= startDate` rule, ADR-0005), and delegates to `IProjectService` — no business logic in the controller.
3. The service loads the target project (update/delete). Not found → **404**.
4. `CanMutateAsync(project, caller)` evaluates ownership **now**: `Admin` → allow; `ProjectManager` → allow only if `project.OwnerId == caller.UserId`; otherwise → **403**.
5. The service applies the change (or resolves `owner_id` from the token on create).
6. `IActivityLogService.LogAsync` writes the audit row (actor, action, `Project`, id, timestamp, change summary) — **for deletes this happens before the row is removed**.
7. The service and audit write commit in **one transaction/`SaveChanges`**, so a project change can never exist without its audit entry.

### T.6 API behaviour rules
- **Exact routes** — the five routes are fixed by the brief and Constitution VI.6; resource-oriented, plural noun, verbs are HTTP verbs.
- **Status codes** (Constitution VI.2): 201 + `Location` on create; 200 on read/update; 204 on delete; 400 validation/bad paging; 401 unauthenticated; 403 role or scope denial; 404 unknown id; **409 stale row version** (ADR-0004); 500 server. All errors are **Problem Details** JSON, mapped from `ErrorKind` by the shared mapper (ADR-0003).
- **Pagination** (Constitution VI.4): `?page` and `?pageSize` on the collection endpoint, with a configured default and a hard maximum; `pageSize` above the maximum is clamped rather than rejected.
- **Filtering/sorting** — `?search`, `?status`, `?sort` added as needed, always applied inside the caller's scope.
- **Versionable** — routes are designed so a future `/api/v1` prefix can be added without breaking clients (Constitution VI.1); not adopted now.

### T.7 Failure handling (fail-safe)
- **Unknown/expired token → 401** (inherited from 001).
- **Disallowed role → 403** at the attribute, before any data is touched.
- **In-scope failure → 403** from the service, after existence is confirmed; **unknown id → 404**. Out-of-scope reads return **403** by default (configurable to mask as **404** — OQ-002-03).
- **Validation failure → 400** with per-field errors; nothing is persisted.
- **Concurrent delete during update → 404**; the transaction rolls back with no partial write and no orphan audit row.
- **Concurrent update (stale `xmin`) → 409** with Problem Details; the UI prompts the user to reload and reapply. Silent last-write-wins is never acceptable (ADR-0004).
- Uncaught errors → **500** as Problem Details; the Angular `ErrorInterceptor` + global `ErrorHandler` surface them through the shared notification component (Constitution VII.7).

### T.8 Security guarantees
- Every endpoint requires a valid JWT; the role gate is **attribute-declared only** (Constitution V.1, V.2).
- **Ownership can never be forged**: `owner_id` is derived from the token for a ProjectManager and any client-supplied `ownerId` is ignored.
- **Out-of-scope rows are never loaded** — the scope predicate is part of the SQL, so they cannot leak via items, totals, or paging metadata.
- Write-time re-checking means a stale read can never authorize a later mutation.
- Every write to Projects is audited to `activity_logs` (Constitution IV.4), and audit rows survive deletion of the project.
- Server-side validation is authoritative; the frontend validates only for UX (Constitution V.5).
- All data access goes through EF Core; no raw SQL (Constitution IV.1).

---

## Implementation Blueprint (build-ready detail)

> Everything the team needs to build this feature: concrete schema, enums, service interfaces, configuration, error model, NFRs, the audit catalog, and the Definition of Done.

### B.1 Concrete schema (DDL-level intent; expressed as an EF Core migration)
> PostgreSQL 18 via Npgsql. snake_case identifiers. Timestamps `timestamptz` (UTC); calendar dates `date`. Migration name: `AddProjectsTable`.

**`projects`**
- `id` uuid **PK**
- `name` varchar(200) **NOT NULL**
- `description` varchar(2000) **NULL**
- `start_date` date **NOT NULL**
- `end_date` date **NULL** — **CHECK** (`end_date IS NULL OR end_date >= start_date`)
- `status` varchar(20) **NOT NULL DEFAULT 'Planning'** (see B.2)
- `owner_id` uuid **NOT NULL** FK→`users(id)` **ON DELETE RESTRICT** (ownership must be reassigned before a user can be removed)
- `created_at` timestamptz **NOT NULL** · `updated_at` timestamptz **NOT NULL**
- **Concurrency**: PostgreSQL `xmin` mapped as an EF Core row-version token (ADR-0004) — no extra column; a stale update → **409**
- **INDEX** (`owner_id`), (`status`), (`owner_id`,`status`); text index on `name` for search

**Referenced, not defined here**: `users` and `activity_logs` (behaviour owned by 001), `team_members` (management owned by 004), `tasks` (owned by 003). All five constitution entities are created in the **initial migration** — a feature owns an entity's API/UI/rules, not its table's existence — so the TeamMember scoping join is available to this feature from day one. `tasks.project_id` and `team_members.project_id` are **ON DELETE CASCADE** so deleting a project removes its dependents (Constitution IV.3).

### B.2 Enumerations (fixed value sets)
- **ProjectStatus**: `Planning, Active, OnHold, Completed, Cancelled` (default `Planning`; persisted as string for readability and migration safety)
- **AuditAction** (Project): `ProjectCreated, ProjectUpdated, ProjectDeleted, ProjectOwnerChanged`

### B.3 Service interfaces & method signatures (C#; nullable reference types on)
```csharp
public interface IProjectService {
    // Scope is applied inside the query; totalCount is per-scope.
    Task<Result<PagedResult<ProjectSummaryDto>>> ListAsync(ProjectQuery query, CurrentUser caller, CancellationToken ct);
    // 404 if unknown; 403 if outside the caller's scope.
    Task<Result<ProjectDetailDto>> GetByIdAsync(Guid projectId, CurrentUser caller, CancellationToken ct);
    // owner_id resolved from the token for ProjectManager; Admin may supply ownerId. Audits ProjectCreated.
    Task<Result<ProjectDetailDto>> CreateAsync(CreateProjectRequest req, CurrentUser caller, CancellationToken ct);
    // Re-checks ownership at write time. Ownership transfer is Admin-only. Audits ProjectUpdated.
    Task<Result<ProjectDetailDto>> UpdateAsync(Guid projectId, UpdateProjectRequest req, CurrentUser caller, CancellationToken ct);
    // Audits ProjectDeleted BEFORE removal; dependents cascade. Audits survive.
    Task<Result> DeleteAsync(Guid projectId, CurrentUser caller, CancellationToken ct);
}

// Layer 2 of the authorization model — the ownership/assignment gate that no role attribute can express.
public interface IProjectAccessPolicy {
    IQueryable<Project> ApplyScope(IQueryable<Project> source, CurrentUser caller);        // Admin=all · PM=owned · TM=assigned
    Task<AccessDecision> CanReadAsync(Project project, CurrentUser caller, CancellationToken ct);
    Task<AccessDecision> CanMutateAsync(Project project, CurrentUser caller, CancellationToken ct); // Admin any · PM own only
}

// ProjectQuery      { int Page; int PageSize; string? Search; ProjectStatus? Status; string? Sort; }
// Result<T>, Error/ErrorKind, CurrentUser, AccessDecision, and PagedResult<T> are defined once in
// docs/shared-contracts.md (ADR-0003) and reused verbatim by every feature — not redefined here.
// Entity → DTO mapping uses manual static extension methods; write DTOs are validated with
// FluentValidation (incl. the cross-field endDate >= startDate rule). See ADR-0005.
```
`IActivityLogService` is **reused from 001** — this feature adds Project-targeted rows, it does not define a new audit service.

### B.4 Configuration (never hardcoded)
- `Projects:Paging:{DefaultPageSize,MaxPageSize}` (e.g. 20 / 100)
- `Projects:DefaultStatus` (default `Planning`)
- `Projects:MaskOutOfScopeAs404` (default `false` → return 403; see OQ-002-03)
- `Projects:AllowOwnershipTransfer` (default `true`, Admin-only)
- `Projects:MaxNameLength` / `MaxDescriptionLength`

### B.5 Error model (RFC 7807 Problem Details)
`{ type, title, status, detail, traceId, errors? }` — produced by the shared `ErrorKind` → status mapper ([shared-contracts §1](../../docs/shared-contracts.md), ADR-0003). Mapping: `400` validation (per-field `errors`, incl. date-order and paging bounds), `401` `Authentication required`, `403` `Forbidden` (role gate) / `You do not have access to this project` (scope gate), `404` `Project not found`, `409` `Conflict` (stale row version), `500` `Unexpected error`. Never leak an out-of-scope project's data in an error body.

### B.6 Non-functional requirements
- **Security:** deny-by-default; scope enforced in SQL; ownership derived from the token; write-time re-check.
- **Performance:** list is a single round-trip with scope+filter+paging pushed to the database; indexes lead with `owner_id`/`status`; no N+1 on owner (projection or `Include`).
- **Observability:** structured logging via **Serilog** (console + rolling files); authorization denials logged with actor, project id, and reason.
- **Testability (Constitution IX):** every `IProjectAccessPolicy` and `IProjectService` branch unit-tested (xUnit) — notably the three-role scope matrix; each controller happy path + one error path via `WebApplicationFactory`; frontend `ProjectsService`, guard, and form validators via Jasmine+Karma.

### B.7 Audit event catalog (→ `activity_logs`, defined in 001)
Emit `(actor_id, action, entity_type='Project', entity_id, timestamp, change_summary)` for: **create** (`ProjectCreated`), **update** (`ProjectUpdated`, summary of changed fields), **delete** (`ProjectDeleted`, snapshot summary, written before removal), **ownership change** (`ProjectOwnerChanged`). Reads are not audited. Append-only; audit rows are never cascaded away.

### B.8 Definition of Done
1. The five routes exist **exactly** as named in Constitution VI.6 and behave per the status-code table.
2. The three-role scope matrix is proven by integration tests: Admin sees all; a ProjectManager sees/mutates only owned projects; a TeamMember sees only assigned projects and gets **403** on every write.
3. Ownership cannot be forged — a ProjectManager's client-supplied `ownerId` is ignored; ownership transfer is Admin-only.
4. Out-of-scope rows never appear in items, `totalCount`, or paging metadata (asserted directly).
5. `?page`/`?pageSize` work with a default and a clamped maximum; invalid paging returns **400**.
6. Every write to Projects produces an `activity_logs` row in the same transaction; delete audits before removal and the audit row survives.
7. Deleting a project cascades to dependent tasks/assignments; deleting a user is **restricted** while they own projects.
8. The Angular `projects` route group is lazy-loaded with standalone components; all HTTP lives in `ProjectsService` (none in components); create/edit use Reactive Forms with explicit validators incl. date-order; a functional role-based route guard is the only navigation block.
9. Errors are RFC 7807; all endpoints appear in Swagger; backend compiles warnings-as-errors with nullable enabled; frontend compiles in strict mode.
10. Concurrent updates to the same project are rejected with **409** (stale `xmin`), proven by an integration test — never a silent overwrite.
11. Unit + integration tests pass (Constitution IX.3 — no merge on failing tests).

### B.9 Open questions for review
| # | Question | Recommendation | Blocks build? |
|---|---|---|---|
| OQ-002-01 | Hard delete with cascade, or soft delete/archive? | **Hard delete with explicit cascade** for v1; `activity_logs` preserves history. Revisit if Reports need deleted projects | No |
| OQ-002-02 | Scope gate as `IProjectAccessPolicy` in the service vs. ASP.NET resource-based authorization handlers? | Service-layer policy (scope must fold into the `IQueryable` for correct paging); record as an ADR | No |
| OQ-002-03 | Out-of-scope read → **403** or masked **404**? | **403** (single-org workspace; existence disclosure is acceptable); config flag to mask as 404 | No |
| OQ-002-04 | Is the `ProjectStatus` set correct (`Planning/Active/OnHold/Completed/Cancelled`)? | Adopt as listed; confirm with the demo script's needs | No |
| OQ-002-05 | May an Admin assign ownership to a non-ProjectManager (e.g. a TeamMember) user? | No — owner must hold the ProjectManager or Admin role; validate on create/transfer | No |
| OQ-002-06 | Must project names be unique (globally or per owner)? | **Not unique** — duplicates permitted; disambiguated by id and owner | No |
| OQ-002-07 | Should delete be blocked when a project has open tasks? | Not blocked for v1 (cascade + confirmation dialog); revisit once Tasks (003) lands | No |

---

## Functional Requirements

- **FR-001**: The system MUST expose exactly `GET /api/projects`, `GET /api/projects/{id}`, `POST /api/projects`, `PUT /api/projects/{id}`, and `DELETE /api/projects/{id}` (Constitution VI.6).
- **FR-002**: A Project MUST have a name, description, start date, end date, and status; `name`, `start_date`, `status`, and `owner_id` are required; `end_date` is optional and, when present, MUST NOT precede `start_date`.
- **FR-003**: Creating a project MUST set `owner_id` from the authenticated caller for a ProjectManager, ignoring any client-supplied owner; an Admin MAY specify the owner.
- **FR-004**: `POST /api/projects` MUST return **201** with a `Location` header; `PUT` MUST return **200**; `DELETE` MUST return **204**.
- **FR-005**: TeamMember MUST be denied create, update, and delete with **403**.
- **FR-006**: Listing MUST be role-scoped server-side: Admin → all projects; ProjectManager → only owned; TeamMember → only projects they are assigned to via TeamMembers.
- **FR-007**: The scope filter MUST be applied within the database query so out-of-scope projects never appear in items, `totalCount`, or paging metadata.
- **FR-008**: `GET /api/projects` MUST support `?page` and `?pageSize` with a configured default and maximum (Constitution VI.4), and SHOULD support `?search` and `?status`.
- **FR-009**: Role checks MUST be declared with `[Authorize(Roles = "...")]` attributes only; ownership/assignment checks MUST be enforced in the **service layer**, never in the controller.
- **FR-010**: Update and delete MUST re-check ownership at write time; a ProjectManager acting on a project they do not own MUST receive **403**.
- **FR-011**: An unknown project id MUST return **404**; an existing project outside the caller's scope MUST return **403** (maskable to 404 by configuration).
- **FR-012**: Every write to Projects (create, update, delete, ownership change) MUST create an `activity_logs` entry (actor, action, entity type, entity id, timestamp, change summary) in the same transaction; for deletes the entry MUST be written before removal and MUST survive it.
- **FR-013**: Deleting a project MUST cascade to its dependent tasks and team-member assignments; deleting a user who owns projects MUST be restricted until ownership is reassigned.
- **FR-014**: Errors MUST be RFC 7807 Problem Details; all endpoints MUST be documented via Swagger/OpenAPI.
- **FR-015**: The Angular `projects` feature area MUST be lazy-loaded via route-level code splitting with standalone components (ADR-0001); all HTTP calls MUST live in a dedicated `ProjectsService` (never in components); create/edit MUST use Reactive Forms with explicit validators; a functional role-based route guard MUST be the only mechanism blocking navigation.
- **FR-016**: All persistence MUST go through EF Core with a Code-First migration; no raw SQL and no manual DDL (Constitution IV.1, IV.2).
- **FR-017**: Updating a project MUST use optimistic concurrency (PostgreSQL `xmin` row-version token); a stale write MUST return **409 Conflict** as Problem Details rather than silently overwriting (ADR-0004).

## Non-Functional Requirements
- **NFR-001**: Server-side enforcement is authoritative; the frontend never filters for security.
- **NFR-002**: List queries execute as a single round-trip with scope, filter, and paging translated to SQL; no in-memory filtering and no N+1 on owner.
- **NFR-003**: Structured logging (Serilog); authorization denials logged with actor, project id, and reason.
- **NFR-004**: Nullable reference types + warnings-as-errors (backend); TypeScript strict mode (frontend).
- **NFR-005**: Indexes lead with `owner_id`/`status` so role-scoped listing stays performant as project count grows.

## Configurability Rules
- **CFG-001**: Default and maximum `pageSize`; default sort order.
- **CFG-002**: Default project status on create.
- **CFG-003**: Out-of-scope reads return 403 or are masked as 404.
- **CFG-004**: Whether ownership transfer is permitted (Admin-only when enabled).
- **CFG-005**: Max lengths for `name` and `description`.
- **CFG-006**: Whether delete is blocked when a project has dependent tasks (default: not blocked — cascade).

## Security Rules
- Authenticated by default; role gate via attributes only; scope gate in the service layer.
- Ownership derived from the token and never accepted from the body for a ProjectManager.
- Out-of-scope rows excluded inside the query — no leakage via counts or paging.
- Ownership re-checked at write time; deny by default when scope cannot be established.
- Every write audited; audit rows retained through cascade deletes.

## Audit / Compliance Expectations
Audit every write to Projects — create, update, delete, ownership change — with actor, action, entity type (`Project`), entity id, timestamp, and change summary to `activity_logs` (defined in 001). Reads are not audited. Append-only; audit rows are never cascaded away, so the history of a deleted project remains queryable by Reports (006).

## Assumptions
- 001 Auth & RBAC is implemented: `users`, the three-role model, the single JWT `role` claim, the authenticated-by-default policy, and `IActivityLogService` all exist and are consumed here.
- **Entity existence vs. feature ownership**: all five constitution entities (Users, Projects, Tasks, TeamMembers, ActivityLogs) are created in the **initial EF Core migration** — the constitution defines them as one model (IV.3). A *feature* owns an entity's **API, UI, and business rules**, not the existence of its table. So `team_members` exists from day one, this feature's TeamMember scoping query compiles and is testable immediately, and feature 004 later owns assignment management. Until assignments exist, a TeamMember's list is legitimately **empty** (not an error).
- `tasks.project_id` and `team_members.project_id` are declared **ON DELETE CASCADE** in the shared model so deleting a project removes its dependents (Constitution IV.3).
- One owner per project; ownership is the sole basis of ProjectManager authority.
- Project volume is expected to exceed 50 rows, so pagination is required from day one (Constitution VI.4).

## Dependencies
- **Depends on**: [001 Auth & RBAC](../001-auth-rbac/spec.md) — Users, role model, JWT claims, `[Authorize]` conventions, ActivityLog pattern.
- **Consumed by**: 003 Tasks (tasks belong to a project) · 004 Team (assignments target a project) · 005 Dashboard (aggregates projects) · 006 Reports (exports projects). Each inherits this feature's scoping rules as its anchor.
- **Infrastructure**: PostgreSQL 18 via EF Core 10 + Npgsql; Serilog; Swagger/OpenAPI.

## Out of Scope
Task details and task management (003); creating/removing team-member assignments (004); dashboard aggregation (005); report export (006); authentication and the role model itself (001); project templates, cloning, archive/restore, attachments, comments, and Gantt charts (bonus scope, Constitution I.2).

---

## Sequence Note

This is the **second** module in the sequence (001 Auth & RBAC complete). It follows the structural template set by [001](../001-auth-rbac/spec.md) and the merged-file convention. **003 Tasks** and **004 Team** follow the identical structure and depend on this file's `Project` entity as their anchor.
