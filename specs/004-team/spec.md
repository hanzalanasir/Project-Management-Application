# Feature Specification: Team Management

**Feature Number**: 004
**Feature Name**: Team Management (Project Team Membership)
**Phase**: Phase 1 — Foundation
**Priority**: Must Have (Critical)
**Complexity**: Medium
**Type**: Core domain / membership join + resource-level authorization
**Depends On**: **001 Auth & RBAC** (Users, role model, JWT claims, `CurrentUser`, `IActivityLogService`) · **002 Projects** (the `Project` entity and its ownership rule) — both **referenced, not redefined**
**Enables**: 005 Dashboard · 006 Reports (both aggregate team/membership data)
**Backs**: **003 Tasks'** "an assignee must be a valid team member on the project" constraint — this feature owns the membership records that constraint validates against (see Dependencies)
**Created**: 2026-07-22
**Status**: Draft — Ready for Planning
**Governed By**: Project Constitution v1.1.1 (Principles II Architecture, III Stack, IV Data Access, V Security & Authorization, VI API Design, VII Frontend, VIII Code Quality, IX Testing)
**Cross-cutting contracts**: [docs/shared-contracts.md](../../docs/shared-contracts.md) — `Result<T>`, `ErrorKind`, `CurrentUser`, `AccessDecision`, error→HTTP mapping (`PagedResult<T>` and `xmin` concurrency evaluated and deliberately **not** applied — see Technical Design T.6 and Implementation Blueprint B.1) · ADRs [0001](../../docs/adr/0001-angular-standalone-components.md) standalone Angular · [0002](../../docs/adr/0002-same-origin-hosting.md) same-origin hosting · [0003](../../docs/adr/0003-result-error-contract.md) error contract · [0004](../../docs/adr/0004-optimistic-concurrency.md) concurrency · [0005](../../docs/adr/0005-mapping-and-validation.md) mapping/validation
**Generated Via**: `/speckit.specify` (merged requirements + solution design, per project convention)

---

## Purpose

Own **project team membership** — the record that a given user is on a given project's team. This feature provides the three membership operations (add a member, list a project's team, remove a member) and, importantly, establishes a deliberately **minimal** authorization shape: membership is a **link, not a role**. The `TeamMember` entity records *that* a user belongs to a project's team; it grants no permissions of its own. What a member may do is entirely determined by their existing global role from [001 Auth & RBAC](../001-auth-rbac/spec.md).

This feature is also the missing piece under a constraint 003 Tasks already relies on: "a task's assignee must be a team member on that project." Until now that pool was referenced but unpopulated; this feature is what fills and maintains it.

This is a **merged specification**: requirements (Purpose → User Stories → Functional Requirements) and solution design (Technical Design → Implementation Blueprint) live in one file by project convention, so the team reviews *what* and *how* together.

## Business Value

Projects are staffed by people, and the product needs a first-class record of who is on which team — for scoping a TeamMember's visibility (002/003 already scope by it), for validating task assignees (003), and for the roster views the Dashboard and Reports will present. By keeping membership a pure link with no separate per-project role, the model stays honest to the brief's three-role RBAC design: there is exactly one place a user's permissions come from (their global role), so there is no second, divergent permission system to reason about, test, or keep in sync. Every add and remove is audited, so the history of who was staffed on what — and by whom — is answerable.

## Actors

**Primary Actors**
- **Admin** — full management of team membership on **any** project, regardless of who owns it.
- **ProjectManager** — adds and removes members only on **projects they own** (ownership resolved through feature 002's `Project.owner_id`).
- **TeamMember** — **read-only**: can view the roster of a project's team **they belong to**; no add or remove actions.

**Secondary Actors**
- **Consuming features (non-actor)** — 003 Tasks reads this membership pool to validate assignees; 005 Dashboard and 006 Reports read it for roster and staffing views. All inherit this feature's project-scoped visibility rules.

## Scope Summary

**In scope**: the `TeamMember` entity — a join between a `Project` and a `User`, recording membership only (**no role or permission field**) — and its Code-First migration; the three endpoints following the 002/003 pattern per Constitution VI.6 (`GET /api/projects/{projectId}/team`, `POST /api/projects/{projectId}/team`, `DELETE /api/projects/{projectId}/team/{userId}`); project-scoped authorization (Admin any; ProjectManager on owned projects; TeamMember read-only on projects they belong to); a uniqueness guarantee that a user appears at most once on a project's team; an `activity_logs` entry on every add/remove; the lazy-loaded Angular **`team` route group** (standalone components per ADR-0001) with a dedicated `TeamService`, a Reactive Form for the add-member action, and functional role guards.

**Out of scope**: per-project custom roles or permission overrides beyond a user's existing global role (this feature deliberately adds **no** new role dimension); bulk import of team members; the `Project` entity and its ownership rule (002); the `Task` entity and assignee validation logic itself (003 — this feature supplies the membership records 003 validates against, but the validation lives in 003); authentication and the role model (001); dashboard aggregation (005) and report export (006).

## Access Logic (applies to every story)

Enforced **server-side**, in this order:

1. **Authenticated by default** — every endpoint requires a valid JWT (inherited from 001). No/invalid/expired token → **401**.
2. **Role gate (controller, attribute-only)** — `[Authorize(Roles = "...")]`. Team **add and remove** permit `Admin,ProjectManager`; the roster **read** permits all three roles. A role that is not permitted → **403**. Ad-hoc role checks in method bodies remain prohibited (Constitution V.2).
3. **Project-scope gate (service)** — every team operation is governed by the **parent project**, identified by the route's `projectId`:
   - **View** the roster: Admin any; ProjectManager if they own the project **or are a member of it**; TeamMember if they are a member of it. Otherwise → **403**.
   - **Manage** (add/remove): Admin any; ProjectManager only if they own the project. Otherwise → **403**.
   This is a **binary** authorization — there is no graduated, field-level mutation model here (that was Tasks-specific), because a membership row has no editable fields to partially permit. The check lives in the service, never the controller (Constitution II.2).
4. **Identity from the token** — the acting user comes from `ICurrentUserService` reading the validated JWT, never from the request body.
5. **Deny by default** — if project scope cannot be established, the request is denied. Frontend guards and conditionally rendered controls are convenience only.

## Role & Permission Model

The three roles are defined in [001 Auth & RBAC](../001-auth-rbac/spec.md) — each user holds **exactly one**, carried as a single JWT `role` claim. **This feature introduces no new role dimension.** A `TeamMember` record does **not** carry a role; a member's authority is whatever their global role already is. Being "on a team" changes what a user can *see* (project and task visibility, via 002/003 scoping), not what a user *is*.

| Operation | Admin | ProjectManager | TeamMember |
|---|---|---|---|
| `GET /api/projects/{projectId}/team` (view roster) | Any project | Projects they own **or are a member of** | Projects they are a member of |
| `POST /api/projects/{projectId}/team` (add member) | ✔ any project | ✔ owned projects | ✘ **403** |
| `DELETE /api/projects/{projectId}/team/{userId}` (remove member) | ✔ any project | ✔ owned projects | ✘ **403** |

**Membership** is the basis of a TeamMember's project visibility, exactly as **ownership** is a ProjectManager's. The two concepts stay separate: a ProjectManager may (subject to the eligibility rule under clarification) also appear as a *member* of a project they do not own — in which case they can view that project and its team as a member, but still cannot manage its team.

---

## Clarifications

### Session 2026-07-22

- Q: Which users are eligible to be added to a project's team — any user regardless of global role, or TeamMembers only? → A: **Any active user, regardless of global role.** Membership records visibility, not permission, so a ProjectManager or an Admin may be added as a *contributor* on a project they do not own without it changing what they are. The only add-time gate is that the user is **active** (a deactivated user is refused, 400). This keeps membership orthogonal to role — the coupling this feature exists to avoid.
- Q: Removing a member who still has open tasks assigned to them in that project — block the removal, or auto-unassign their tasks first? → A: **Block with 409 Conflict** and a dependency message listing the blocking tasks. Removal is refused while the member has any open assigned task in the project; the manager must reassign or close those tasks first, then remove the person. This is a **fixed invariant, not configurable** — it preserves the guarantee that a task's assignee is always a current team member (backing 003), mirrors 002's dependency-aware delete, and never silently drops assigned work to unassigned.

---

## User Stories

> Story IDs `US-004-01..03`. Each story: **A** Summary · **B** Quality Validation (INVEST · 3Cs · 7Cs · Given-When-Then · edge cases · audit/security · configurability) · **C** UI · **D** API · **E** DB · **F** Separation of concerns. Consolidated schema, API catalog, technical design, and implementation blueprint follow the stories.

---

### US-004-01 — Add a user to a project's team

**A. Summary**
- **Story ID**: US-004-01 · **Title**: Add a user to a project's team
- **Actor**: ProjectManager (own projects) · Admin (any project)
- **User story**: *As a ProjectManager, I want to add a user to my project's team, so that they gain visibility of the project and become eligible to be assigned its tasks.*
- **Business value**: Staffs a project; also the act that makes a user a valid task assignee (003 validates against exactly these records).
- **Priority**: **P0** · **Reason**: Without membership, no one but the owner and Admin can see or be assigned work on a project.
- **Dependencies**: 001 (Users), 002 (parent project + ownership). **Out of scope**: bulk add; assigning tasks (003).

**B. Quality validation**
- **INVEST** — Independent ✔ (adding a member stands alone); Negotiable ✔ (the eligibility rule was the negotiated decision — any active user, OQ-004-01, Clarifications 2026-07-22); Valuable ✔; Estimable ✔; Small ✔ (one join row + audit); Testable ✔ (row created, duplicate rejected, TeamMember blocked, audit written).
- **3Cs** — Card ✔ (stands alone: "add a user to a project's team → they can now see it and be assigned to it"); Conversation ✔ (surfaced the two genuinely open questions — *which* users are eligible (OQ-004-01) and whether a deactivated user may be added — plus concurrent duplicate-add; see Edge cases and Open Questions); Confirmation ✔ (the Given/When/Then scenarios cover owner-adds, cross-project denial, TeamMember denial, duplicate, and unknown ids — sufficient to call this story done once OQ-004-01 is settled).
- **7Cs** — Clear ✔ (states plainly that adding a member grants *visibility*, not a role); Concise ✔; Concrete ✔ (exact 201/403/409/404 codes, exact `Location` header); Correct ✔ (matches FR-003 and the membership-is-a-link model); Coherent ✔ (the pool this story writes is exactly the pool 003 validates assignees against); Complete ✔ (add, uniqueness, role gate, and audit are all covered, with the one genuinely open rule flagged rather than silently guessed); Courteous n/a (no user-facing copy specified in this story).
- **Given/When/Then**
  1. **Given** a ProjectManager and a project **they own**, and a user who is eligible (per OQ-004-01) and not yet a member, **When** they add the user, **Then** a `team_members` row is created (`project_id` from the route, `user_id` from the body, `added_by` = the caller), a `TeamMemberAdded` `activity_logs` entry is written **in the same transaction**, and the response is **201 Created** with `Location: /api/projects/{projectId}/team/{userId}`.
  2. **Given** a ProjectManager and a project **owned by someone else**, **When** they add a user, **Then** **403** and nothing is written.
  3. **Given** a **TeamMember**, **When** they attempt to add a user, **Then** **403** at the role gate.
  4. **Given** a user who is **already a member** of the project, **When** they are added again, **Then** **409 Conflict** (Constitution VI.2) and no duplicate row is created.
  5. **Given** an unknown `projectId` or an unknown `userId`, **When** adding, **Then** **404**.
- **Edge cases**: **any active user is eligible regardless of global role** (Clarifications 2026-07-22) — a ProjectManager or Admin may be added as a member of a project they do not own; adding a **deactivated** user is **refused, 400** (the only add-time eligibility gate, mirroring 003's treatment of assignees); a ProjectManager adding **themselves** to their own project (permitted — a manager may also be a contributor); concurrent duplicate adds of the same `(project, user)` (the unique constraint lets one succeed with **201**, the other resolves to **409**); adding a user to a project already in a terminal status (permitted by default, configurable).
- **Audit/security**: the add is audited with the acting user (`added_by`); `project_id` comes from the **route**, never the body, so a user cannot be slipped onto a different project's team; the record carries **no role/permission field**, so adding a member can never elevate anyone.
- **Configurability**: whether a deactivated user may be added (default no); whether members may be added to a terminal-status project (default yes). (Eligibility is resolved: any active user regardless of role — Clarifications 2026-07-22.)

**C. UI** — **F004-S02 Add Team Member** (standalone component / dialog in the lazy-loaded `team` route group). A Reactive Form with a single user picker (a searchable select of **any active user** not already on the team, regardless of global role — Clarifications 2026-07-22); explicit validators (a user must be selected); errors via the shared error-display component; reachable only from a project the caller may manage.

**D. API** — `POST /api/projects/{projectId}/team` · `[Authorize(Roles = "Admin,ProjectManager")]` · body `{ "userId": "…" }` · **201 Created** + `Location: /api/projects/{projectId}/team/{userId}` + the created membership DTO.

**E. DB** — writes **`team_members`** (FK `project_id` → `projects`, FK `user_id` → `users`, `added_by` → `users`), **`activity_logs`**.

**F. Separation** — UI: add dialog + eligible-user picker. Backend: `ITeamService.AddAsync` → `ITeamAccessPolicy.CanManageTeamAsync` → eligibility + not-already-member + user-active validation → persist → audit. DB: unique-constrained insert + audit row in one transaction. QA: cross-project 403, TeamMember 403, duplicate 409, unknown ids 404, audit written.

---

### US-004-02 — List a project's team

**A. Summary**
- **Story ID**: US-004-02 · **Title**: View the roster of a project's team
- **Actor**: Admin (any) · ProjectManager (owned or member) · TeamMember (member)
- **User story**: *As someone connected to a project, I want to see who else is on its team, so that I know who I'm working with and (as a manager) who is available to be assigned work.*
- **Business value**: The roster view every downstream feature and screen builds on; also the picker source for 003's assignee selection.
- **Priority**: **P0** · **Reason**: Required before anyone can meaningfully add, remove, or assign.
- **Dependencies**: US-004-01. **Out of scope**: showing each member's task load (005 Dashboard).

**B. Quality validation**
- **INVEST** — Independent ✔; Negotiable ✔ (which member fields are shown); Valuable ✔; Estimable ✔; Small ✔ (single bounded read); Testable ✔ (roster returned iff the caller may view the project, per the three-role matrix).
- **3Cs** — Card ✔ (stands alone: "view a project's team roster"); Conversation ✔ (surfaced the bounded-list / no-pagination reasoning and the display of a deactivated member — see Edge cases and T.6); Confirmation ✔ (the Given/When/Then scenarios cover a permitted view, a denied view, an unknown project, and an empty roster — sufficient to call this story done).
- **7Cs** — Clear ✔ (states outright that the member's shown `role` is their *global* role, not a per-project one); Concise ✔; Concrete ✔ (exact 200/403/404 outcomes, exact returned fields); Correct ✔ (matches FR-006 and the visibility rules in Access Logic); Coherent ✔ (visibility aligns exactly with 002's project-read scope — a TeamMember sees the team of precisely the projects they can see); Complete ✔ (all three roles, the empty case, and the unknown-project case are covered); Courteous n/a (a roster view with only standard empty/loading states).
- **Given/When/Then**
  1. **Given** a project the caller may view (Admin any; ProjectManager if owner or member; TeamMember if member), **When** they request the roster, **Then** **200** with every member's user id, name, email, **global role** (for display), and added-at timestamp.
  2. **Given** a project the caller may **not** view (a ProjectManager who neither owns nor is a member; a TeamMember who is not a member), **When** they request the roster, **Then** **403** (maskable to **404** by configuration, matching 002/003).
  3. **Given** an unknown `projectId`, **When** requested, **Then** **404**.
  4. **Given** a project with **no members**, **When** the owner or an Admin requests the roster, **Then** **200** with an **empty array** (not a 404, not an error).
- **Edge cases**: a TeamMember who is a member sees the **full roster**, not just themselves; a member whose user account was **deactivated** still appears (flagged inactive, per the include-inactive config); a large team — the roster is **bounded and small**, so it is returned as a plain array without pagination (reasoning in T.6); the parent project in a terminal status (roster still viewable).
- **Audit/security**: roster **reads are not audited** (Constitution IV.4 audits writes only); visibility is enforced by `CanViewTeamAsync` on the parent project before any rows are returned; the `role` shown is a read-only reflection of the user's global role — this endpoint never assigns or implies a per-project role.
- **Configurability**: whether deactivated members are included in the roster (default include, flagged); whether out-of-scope reads return **403** or are masked as **404** (default 403, matching 002/003).

**C. UI** — **F004-S01 Project Team** (roster table under the project's detail area). Columns: member name, email, global role, added-at, and a remove action (rendered only for Admin/owner). Search/filter is client-side over the bounded list. Empty, loading, error, and forbidden states are explicit. The "Add member" action is hidden for TeamMember (UX only — the API still enforces 403).

**D. API** — `GET /api/projects/{projectId}/team` · `[Authorize]` (all three roles) · **200** with a plain array of member DTOs (no paging envelope — see T.6) · **403** out of scope · **404** unknown project.

**E. DB** — reads **`team_members`** joined to **`users`** (for name/email/role/active) and, for the ProjectManager/TeamMember scope decision, checks ownership on **`projects`** and membership in **`team_members`**.

**F. Separation** — UI: roster table + client-side filter + states. Backend: `ITeamService.ListAsync` → `ITeamAccessPolicy.CanViewTeamAsync` → projection. DB: bounded read + scope check. QA: three-role visibility matrix, empty roster, deactivated-member display, out-of-scope 403.

---

### US-004-03 — Remove a user from a project's team

**A. Summary**
- **Story ID**: US-004-03 · **Title**: Remove a user from a project's team
- **Actor**: Admin (any) · ProjectManager (own projects only)
- **User story**: *As a ProjectManager, I want to remove a user from my project's team, so that people no longer working on the project lose access to it and stop appearing as available for assignment.*
- **Business value**: Keeps a project's staffing accurate and revokes access when someone rolls off; the counterpart to adding.
- **Priority**: **P1** · **Reason**: Important, but builds on add/list.
- **Dependencies**: US-004-01; interacts with 003 Tasks' assignments (see OQ-004-02). **Out of scope**: deleting the user account (001).

**B. Quality validation**
- **INVEST** — Independent ✔ (removal stands alone); Negotiable ✔ (the open-tasks behaviour was the negotiated decision — block, OQ-004-02, Clarifications 2026-07-22); Valuable ✔; Estimable ✔; Small ✔ (one row delete + audit); Testable ✔ (row removed, non-member 404, TeamMember blocked, open-tasks removal 409, audit written before removal).
- **3Cs** — Card ✔ (stands alone: "remove a user from a project's team → they lose access to it"); Conversation ✔ (surfaced the headline open question — what happens when the removed member still has open assigned tasks in that project (OQ-004-02), the same class of decision 002/003 already resolved for project/user deletion — plus double-remove and self-removal; see Edge cases and Open Questions); Confirmation ✔ (the Given/When/Then scenarios cover owner-removes, cross-project denial, TeamMember denial, non-member, and the open-tasks block — sufficient to call this story done now that OQ-004-02 is settled).
- **7Cs** — Clear ✔ (states the open-tasks rule outright — block with 409, Clarifications 2026-07-22 — rather than leaving it ambiguous); Concise ✔; Concrete ✔ (exact 204/403/404/409 codes, exact audit-before-removal ordering); Correct ✔ (matches FR-007 and FR-017); Coherent ✔ (the open-tasks block is the same shape as 002's dependency-aware delete and preserves 003's "assignee is always a member" invariant); Complete ✔ (removal, scope, the open-tasks block, and audit are all covered); Courteous ✔ (the confirmation dialog names the specific member and warns that they will lose access to the project).
- **Given/When/Then**
  1. **Given** a ProjectManager and a project **they own**, and a member with **no blocking condition** (per OQ-004-02), **When** they remove the member, **Then** the `team_members` row is deleted, a `TeamMemberRemoved` `activity_logs` entry is written **before** removal, and the response is **204 No Content**.
  2. **Given** a ProjectManager and a project **owned by someone else**, **When** they remove a member, **Then** **403** and nothing changes.
  3. **Given** a **TeamMember**, **When** they attempt a removal, **Then** **403** at the role gate.
  4. **Given** a `userId` who is **not a member** of the project (or an unknown project), **When** removing, **Then** **404**.
  5. **Given** a member who still has **open tasks assigned to them** in that project, **When** they are removed, **Then** the removal is **blocked with 409 Conflict** and a dependency message listing the blocking tasks (Clarifications 2026-07-22); nothing is deleted. The manager reassigns or closes those tasks first, then the removal succeeds with **204**.
- **Edge cases**: removing **oneself** from a project (permitted for an owner/Admin acting on their own membership); removing the **last** member (the project itself is unaffected); **double remove** (the second call observes **404**); concurrent removal (idempotent outcome — one 204, the other 404); the removed member's loss of access is immediate on their next request (their project/task reads re-evaluate scope and return 403, consistent with 003's reassignment behaviour).
- **Audit/security**: removal is audited **before** the row is deleted so the record survives; scope re-checked at write time; removing a member **revokes that user's visibility** of the project and its tasks (002/003 scope). Because a member with open assigned tasks is **blocked** (409, Clarifications 2026-07-22), a successful removal never leaves a task assigned to a non-member — the "assignee is always a current member" invariant holds without any silent task mutation.
- **Configurability**: whether removing a member is permitted while the project is in a terminal status (default yes). (The open-tasks rule is a **fixed invariant** — block with 409 — not a configurable knob; Clarifications 2026-07-22.)

**C. UI** — remove action on **F004-S01 Project Team** (row action), behind a confirmation dialog that names the member and warns they will lose access to the project. If the API returns **409** (the member has open assigned tasks), the dialog surfaces the blocking-tasks message and points the manager to reassign or close them first. Rendered only for Admin and the project owner (UX only — the API still enforces 403).

**D. API** — `DELETE /api/projects/{projectId}/team/{userId}` · `[Authorize(Roles = "Admin,ProjectManager")]` · **204 No Content** · **403** / **404** · **409** if the member has open assigned tasks in the project (blocked; Clarifications 2026-07-22).

**E. DB** — reads **`tasks`** (003) to detect open assignments (the 409 block); deletes from **`team_members`** and writes **`activity_logs`** (retained) only when unblocked.

**F. Separation** — UI: confirm dialog + roster refresh. Backend: `ITeamService.RemoveAsync` → `ITeamAccessPolicy.CanManageTeamAsync` → open-tasks check (block with 409) → audit → delete. DB: open-tasks read + delete + retained audit. QA: cross-project 403, TeamMember 403, non-member 404, **open-tasks removal 409**, audit-before-delete.

---

## Consolidated Data Model (review-level; final physical schema at implementation)

> Code-First (EF Core 10 + Npgsql). PostgreSQL identifiers are **snake_case** (Constitution VIII.2). `users` / `activity_logs` (001), `projects` (002), and `tasks` (003) are **referenced, not redefined** — all five constitution entities are created in the initial migration; a feature owns an entity's API/UI/rules, not its table's existence. Migration name: `AddTeamMembersTable`.

| Entity | Table | Purpose | Key fields (type · req/null) | Relationships |
|---|---|---|---|---|
| **TeamMember** (this feature) | `team_members` | Records that a user is on a project's team — **membership only, no role** | `id` uuid PK; `project_id` uuid FK (req); `user_id` uuid FK (req); `added_by` uuid FK (null = system/seed); `created_at` (req) | *→1 `projects` (**cascade**); *→1 `users` (member, **cascade**); *→1 `users` (`added_by`, **set null**) |
| **Project** (from 002) | `projects` | Parent + ownership source for scope | `id`, `owner_id` (referenced) | 1→* `team_members` |
| **User** (from 001) | `users` | Member + adder reference | `id`, `is_active`, global role (referenced) | 1→* `team_members` as member |
| **TaskItem** (from 003) | `tasks` | Read on removal to block (409) if the member has open assigned tasks in the project (Clarifications 2026-07-22) | `project_id`, `assignee_id` (referenced) | — |
| **ActivityLog** (from 001) | `activity_logs` | Audit of every add/remove | `actor_id`, `action`, `entity_type='TeamMember'`, `entity_id`, `timestamp`, `change_summary` | references `team_members` by id |

**No role/permission column — by design.** A `team_members` row records *membership*, full stop. A member's permissions come from their global role in 001. This is the single most important schema fact of the feature and the reason "per-project roles" are explicitly out of scope.

**No mutable field → no optimistic concurrency.** A membership is added or removed, never edited in place — there is no column to update — so the entity carries **no `updated_at` and no `xmin` row-version token** (unlike `projects` and `tasks`). Concurrency safety comes instead from a **unique constraint on `(project_id, user_id)`**: a race to add the same member resolves to one **201** and one **409**, and removal is naturally idempotent. See T.6 / B.1 for the full reasoning.

**Cascade behaviour — decided, not implicit** (Constitution IV.3):
- **`team_members.project_id` → `projects` is `ON DELETE CASCADE`.** Deleting a project removes its team rows — consistent with the cascade 002 declares (and the delete-confirmation warning it shows) and with 003's task cascade.
- **`team_members.user_id` → `users` is `ON DELETE CASCADE`.** A membership is a meaningless link once its user is gone; deleting a user removes their memberships. (A user who is a *task assignee* is separately protected from deletion by 003's `RESTRICT` on `tasks.assignee_id`, so live work is never orphaned by this cascade.)
- **`team_members.added_by` → `users` is `ON DELETE SET NULL`.** The historical fact that a membership exists must survive the deletion of whoever added it; the audit trail in `activity_logs` retains the fuller record regardless.
- **Indexes**: unique(`project_id`, `user_id`); (`project_id`); (`user_id`).

## Consolidated API Catalog

> Base path `/api`; JSON over HTTPS; errors as **RFC 7807 Problem Details** produced by the shared `ErrorKind` mapper ([shared-contracts §1](../../docs/shared-contracts.md)); documented via **Swagger/OpenAPI**. Authenticated by default (001). Routes nest team under the parent project, matching 002/003 and Constitution VI.6.

| Method · Route | Purpose | Role gate | Service gate | Success | Failure |
|---|---|---|---|---|---|
| `GET /api/projects/{projectId}/team` | View the roster (plain array, unpaged) | `[Authorize]` (all 3) | `CanViewTeamAsync` | **200** member DTO array | 401, 403, 404 |
| `POST /api/projects/{projectId}/team` | Add a member | `[Authorize(Roles="Admin,ProjectManager")]` | `CanManageTeamAsync` | **201** + `Location` | 400 (ineligible/inactive), 401, 403, 404, **409** (already a member) |
| `DELETE /api/projects/{projectId}/team/{userId}` | Remove a member | `[Authorize(Roles="Admin,ProjectManager")]` | `CanManageTeamAsync` | **204** | 401, 403, 404, **409** (member has open assigned tasks) |

---

## Technical Design — Project-Scoped Membership

> The detailed solution: the components, exact requests/responses, the step-by-step flows, why this feature is deliberately simpler than 003, failure handling, and the security guarantees. Written so a developer can implement it directly.

### T.1 The roles (who is authority, who enforces)
- **The .NET API is the authority.** The controller declares the **role gate**; the **service layer** owns the project-scope gate and every business rule. Controllers do model binding, validation, and delegation only (Constitution II.2).
- **The Angular frontend is convenience.** Guards and conditionally rendered controls shape what a user *sees*; the API re-checks every request. A TeamMember who hand-crafts a `POST …/team` still receives **403**.

### T.2 Membership is a link, not a role (the heart of this feature)
Feature 002 introduced *ownership* and 003 introduced a *graduated* per-field mutation model. Team management deliberately introduces **neither** a new role nor a graduated model:

> A `team_members` row means "this user is on this project's team." It grants nothing on its own. What the member may do is decided entirely by their **global** role (001).

Two consequences shape the whole design:
1. **No per-project role field.** There is exactly one source of a user's permissions. This keeps RBAC single-sourced and testable, and it is why "per-project custom roles" are out of scope. A member's `role` appears in the roster DTO purely as a **read-only reflection** of their global role, for display.
2. **Binary authorization, not graduated.** Because a membership row has no editable fields, there is no "partial write" to permit — you can add it or remove it. So the policy needs only two decisions, not a mutation matrix:

```csharp
public interface ITeamAccessPolicy {
    // The CanReadAsync half (shared-contracts §3): may the caller view this project's team?
    // Admin any; ProjectManager if owner OR member; TeamMember if member.
    Task<AccessDecision> CanViewTeamAsync(Project project, CurrentUser caller, CancellationToken ct);
    // The CanMutateAsync half: may the caller add/remove members of this project?
    // Admin any; ProjectManager if owner; TeamMember deny.
    Task<AccessDecision> CanManageTeamAsync(Project project, CurrentUser caller, CancellationToken ct);
}
```

> **Why no `ApplyScope`?** The shared contract's `ApplyScope` folds a scope predicate into a cross-collection `IQueryable`. Here every team read is already pinned to a **single** project by the route (`/projects/{projectId}/team`), so there is no cross-project collection to scope — the read authorization collapses to one `CanViewTeamAsync(project, caller)` decision. Using `ApplyScope` would be machinery with nothing to filter.

### T.3 The endpoints, with concrete examples

**(1) List the roster**
```
GET /api/projects/4d9b1e77-…-c3/team     Authorization: Bearer eyJ…  (role=ProjectManager, owns it)

→ 200 OK
[ { "membershipId": "7f2c…-a4", "userId": "b81a…-77", "fullName": "Sam Okafor",
    "email": "sam@example.com", "role": "TeamMember", "addedAt": "2026-07-20T09:00:00Z" },
  { "membershipId": "9d51…-b2", "userId": "c04d…-19", "fullName": "Priya Nair",
    "email": "priya@example.com", "role": "ProjectManager", "addedAt": "2026-07-21T14:30:00Z" } ]
```
`role` is each member's **global** role (001), shown for context — not a per-project role. The response is a **plain array**, not a `PagedResult<T>` envelope (see T.6).

**(2) Add a member**
```
POST /api/projects/4d9b1e77-…-c3/team     Authorization: Bearer eyJ…  (role=ProjectManager)
{ "userId": "b81a…-77" }

→ 201 Created
Location: /api/projects/4d9b1e77-…-c3/team/b81a…-77
{ "membershipId": "7f2c…-a4", "userId": "b81a…-77", "fullName": "Sam Okafor",
  "email": "sam@example.com", "role": "TeamMember", "addedAt": "2026-07-22T10:20:00Z" }
→ 409 Conflict   { "title": "Conflict", "detail": "User is already a member of this project." }
→ 400            (user is deactivated — the only add-time eligibility gate; any active user of any role is otherwise eligible)
```

**(3) Remove a member**
```
DELETE /api/projects/4d9b1e77-…-c3/team/b81a…-77   Authorization: Bearer eyJ…  (role=ProjectManager)

→ 204 No Content     (membership removed; TeamMemberRemoved audited before deletion)
→ 409 Conflict       { "title": "Conflict",
                       "detail": "Cannot remove: the member has 2 open task(s) assigned in this project.
                                  Reassign or close them first." }
→ 403 / 404
```
Removing a member with open assigned tasks is **blocked (409)** — a fixed invariant (Clarifications 2026-07-22), mirroring 002's dependency-aware delete — so a task's assignee is always a current member.

### T.4 How a roster read is authorized (step by step)
1. The JWT is validated; `ICurrentUserService` materializes `CurrentUser(UserId, Email, Role)` — never from the body.
2. The role gate admits the caller (all three roles may read a roster).
3. The service loads the parent project by `projectId`. Not found → `ErrorKind.NotFound` → **404**.
4. `CanViewTeamAsync(project, caller)`:
   - `Admin` → allow.
   - `ProjectManager` → allow if `project.OwnerId == caller.UserId`, else allow if the caller has a `team_members` row for this project, else `ErrorKind.Forbidden` → **403**.
   - `TeamMember` → allow only if the caller has a `team_members` row for this project, else **403**.
5. The service projects the project's `team_members` joined to `users` into the roster DTO array and returns it. There is no paging — the set is bounded (T.6).

### T.5 How a write flows — add / remove (step by step)
1. The role gate admits or refuses the caller (**403** before any data is touched for a TeamMember).
2. The controller binds the operation DTO (`{ userId }` for add; `userId` from the route for remove) and runs its FluentValidation validator (ADR-0005), then delegates to `ITeamService`.
3. The service loads the parent project. Not found → **404**. `CanManageTeamAsync(project, caller)` → **403** if the caller is not the owner (or Admin).
4. **Add:** validate the target user exists and is eligible (OQ-004-01) and active → else `ErrorKind.Validation` → **400**; check they are not already a member → else `ErrorKind.Conflict` → **409**; insert the `team_members` row.
   **Remove:** confirm the target is a member → else **404**; if the member has **any open task assigned to them in this project** (read from `tasks`, 003), **block with 409** and a dependency message (Clarifications 2026-07-22); otherwise delete the `team_members` row.
5. `IActivityLogService.LogAsync` writes the audit row (`TeamMemberAdded` / `TeamMemberRemoved`, actor, `TeamMember`, membership id, timestamp, summary) — **for remove, before the row is deleted**.
6. The mutation + audit commit in **one transaction/`SaveChanges`**. A concurrent duplicate add is caught by the unique constraint and mapped to **409**; no `xmin` is involved because there is no in-place update (T.6 / B.1).

### T.6 API behaviour rules
- **Resource-oriented, nested under the parent** (`/projects/{projectId}/team`, `…/team/{userId}`), matching 002/003 and Constitution VI.6; verbs are HTTP verbs, not URL segments.
- **Status codes** (Constitution VI.2): 201 + `Location` on add; 200 on roster read; 204 on remove; 400 validation (ineligible/inactive user); 401 unauthenticated; 403 role or project-scope denial; 404 unknown project or non-member; **409** duplicate membership, or a removal blocked because the member has open assigned tasks; 500 server. All errors are Problem Details from the shared `ErrorKind` mapper (ADR-0003).
- **No pagination — and here is why.** A project's team is a **bounded, human-scale** collection: the people staffed on one project, realistically a handful to a few dozen. It is not the kind of unbounded, ever-growing result set Constitution VI.4 targets ("when the result set can grow beyond 50 items"). So `GET …/team` returns a **plain array**, not a `PagedResult<T>` envelope, and client-side search over the small list is sufficient. The design does **not** preclude adding `?page`/`?pageSize` later without breaking clients (VI.1) should a deployment expect unusually large teams — but mandating it now would be ceremony without benefit.
- **No optimistic concurrency — and here is why.** The `team_members` row is a **pure join with no mutable field**; a membership is present or absent, added or removed, never edited. There is nothing to "update", so `xmin` optimistic concurrency (ADR-0004) does not apply. A **unique constraint on `(project_id, user_id)`** makes concurrent duplicate adds safe (one 201, one 409) and remove is naturally idempotent (a second remove → 404). This is the deliberate, stated reason this entity — unlike `projects` and `tasks` — carries no row-version token.
- **Versionable** — routes are designed so a future `/api/v1` prefix can be added without breaking clients (Constitution VI.1); not adopted now.

### T.7 Failure handling (fail-safe)
- **Unknown/expired token → 401** (001).
- **Disallowed role → 403** at the attribute, before data is touched.
- **Out of project scope → 403**; **unknown project or non-member target → 404**; out-of-scope roster reads return 403 by default (maskable to 404 by configuration, matching 002/003).
- **Ineligible or deactivated user on add → 400** with a field error; **already a member → 409**.
- **Open-tasks removal → 409** with a dependency message (Clarifications 2026-07-22): removal is blocked while the member has open assigned tasks, so assigned work is never silently orphaned or dropped to unassigned.
- Uncaught errors → **500** as Problem Details; the Angular `ErrorInterceptor` + global `ErrorHandler` surface them via the shared notification component (Constitution VII.7).

### T.8 Security guarantees
- Every endpoint requires a valid JWT; the role gate is **attribute-declared only** (Constitution V.1, V.2).
- **Membership grants no permissions**: a `team_members` row carries no role/permission field, so adding or removing a member can never elevate or de-elevate anyone — it only changes project/task *visibility* via 002/003 scoping.
- `project_id` is taken from the route, so a member cannot be added to or removed from a project the caller does not manage.
- Project scope is re-checked at write time; a stale read can never authorize a later add/remove.
- The unique `(project_id, user_id)` constraint prevents duplicate memberships at the database level, not merely in application code.
- Every add/remove is audited in the same transaction as the change; the `activity_logs` row survives cascade deletion of the membership, the project, or the user.
- All data access goes through EF Core; no raw SQL (Constitution IV.1).

---

## Implementation Blueprint (build-ready detail)

> Everything the team needs to build this feature: concrete schema, enums, service interfaces, configuration, error model, NFRs, the audit catalog, and the Definition of Done.

### B.1 Concrete schema (DDL-level intent; expressed as an EF Core migration)
> PostgreSQL 18 via Npgsql. snake_case identifiers. Timestamps `timestamptz` (UTC). Migration name: `AddTeamMembersTable`.

**`team_members`**
- `id` uuid **PK**
- `project_id` uuid **NOT NULL** FK→`projects(id)` **ON DELETE CASCADE**
- `user_id` uuid **NOT NULL** FK→`users(id)` **ON DELETE CASCADE**
- `added_by` uuid **NULL** FK→`users(id)` **ON DELETE SET NULL** (who performed the add; null for seed/system)
- `created_at` timestamptz **NOT NULL**
- **No `updated_at`, no `xmin`** — the row has no mutable field (see T.6); membership is add/remove only
- **UNIQUE** (`project_id`, `user_id`) — one membership per user per project, enforced at the database
- **INDEX** (`project_id`), (`user_id`)

**Referenced, not defined here**: `projects` (002), `users` and `activity_logs` (001), `tasks` (003 — read only for the OQ-004-02 open-tasks check).

### B.2 Enumerations (fixed value sets)
- **AuditAction** (TeamMember): `TeamMemberAdded, TeamMemberRemoved`
- *(No status or role enum — the entity has neither; a member's role is the global `Role` enum owned by 001.)*

### B.3 Service interfaces & method signatures (C#; nullable reference types on)
```csharp
public interface ITeamService {
    // Bounded roster — returns a plain list, not PagedResult<T> (see T.6). CanViewTeamAsync first.
    Task<Result<IReadOnlyList<TeamMemberDto>>> ListAsync(Guid projectId, CurrentUser caller, CancellationToken ct);
    // Validates eligibility (OQ-004-01) + user-active + not-already-a-member (409). Audits TeamMemberAdded.
    Task<Result<TeamMemberDto>> AddAsync(Guid projectId, AddTeamMemberRequest req, CurrentUser caller, CancellationToken ct);
    // Blocks with 409 if the member has open assigned tasks in the project (Clarifications 2026-07-22). Audits TeamMemberRemoved BEFORE removal.
    Task<Result> RemoveAsync(Guid projectId, Guid userId, CurrentUser caller, CancellationToken ct);
}

// Binary authorization — no graduated mutation model is needed (see T.2).
public interface ITeamAccessPolicy {
    Task<AccessDecision> CanViewTeamAsync(Project project, CurrentUser caller, CancellationToken ct);   // Admin · owner · member
    Task<AccessDecision> CanManageTeamAsync(Project project, CurrentUser caller, CancellationToken ct); // Admin · owner only
}

// AddTeamMemberRequest { Guid UserId; }
// TeamMemberDto { Guid MembershipId; Guid UserId; string FullName; string Email; string Role; DateTimeOffset AddedAt; }
//   -> Role is the member's GLOBAL role (001), read-only, for display — NOT a per-project role.
// Result<T>, ErrorKind, CurrentUser, and AccessDecision are defined once in docs/shared-contracts.md
// (ADR-0003) and reused verbatim — not redefined here. PagedResult<T> is intentionally not used (T.6).
// Entity → DTO mapping uses manual static extension methods; the add DTO is validated with
// FluentValidation (userId present). See ADR-0005.
```
`IActivityLogService` is **reused from 001**; `Project`/ownership comes from **002**; the open-tasks check on removal reads/updates `tasks` owned by **003**. None is redefined here.

### B.4 Configuration (never hardcoded)
- `Team:AllowAddInactiveUser` (default `false`) — the sole add-time eligibility gate; **any active user is eligible regardless of global role** (Clarifications 2026-07-22), so there is no role-based eligibility setting
- `Team:AllowManageOnTerminalStatusProject` (default `true`)
  *(There is no remove-with-open-tasks toggle: blocking with 409 is a fixed invariant, not configurable — Clarifications 2026-07-22.)*
- `Team:IncludeInactiveMembersInRoster` (default `true`, flagged inactive)
- `Team:MaskOutOfScopeAs404` (default `false` → return 403, matching 002/003)

### B.5 Error model (RFC 7807 Problem Details)
Produced by the shared `ErrorKind` → status mapper ([shared-contracts §1](../../docs/shared-contracts.md), ADR-0003):
`400` validation (ineligible user per OQ-004-01, deactivated user, missing `userId`) · `401` `Authentication required` · `403` `Forbidden` (role gate or project-scope denial) · `404` `Project not found` / `User is not a member of this project` · `409` `Conflict` (already a member; or removal blocked because the member has open assigned tasks in the project) · `500` `Unexpected error`. Never leak the roster of a project the caller may not view in an error body.

### B.6 Non-functional requirements
- **Security:** deny-by-default; project scope enforced in the service; membership grants no permissions; unique constraint enforced at the database.
- **Performance:** the roster is a single bounded read (one join, no paging); add/remove are single-row writes; indexes on `(project_id)`, `(user_id)`, and the unique `(project_id, user_id)` keep membership lookups (including 003's assignee validation) O(1).
- **Observability:** structured logging via **Serilog**; authorization denials logged with actor, project id, and reason.
- **Testability (Constitution IX):** every `ITeamAccessPolicy` branch unit-tested — the three-role view matrix and the owner-only manage rule; each controller happy path + one error path via `WebApplicationFactory`; frontend `TeamService`, guards, and the add-member validator via Jasmine+Karma.

### B.7 Audit event catalog (→ `activity_logs`, defined in 001)
Emit `(actor_id, action, entity_type='TeamMember', entity_id, timestamp, change_summary)` for: **add** (`TeamMemberAdded`, with the added user and project) and **remove** (`TeamMemberRemoved`, snapshot written before deletion). Roster reads are not audited. A removal blocked by open assigned tasks (409) makes no change and writes no audit row; because removal is blocked rather than cascading, no task mutation is ever triggered from here. Append-only; audit rows are never cascaded away.

### B.8 Definition of Done
1. All three routes exist and behave per the API-catalog status-code table; team is nested under the parent project per Constitution VI.6.
2. The three-role matrix is proven by integration tests: Admin manages any team; a ProjectManager manages only owned-project teams and is refused others with **403**; a TeamMember can view the roster of a project they belong to but is refused every management action with **403**.
3. The `team_members` row carries **no role/permission field** — verified by schema test; a member's roster `role` is a read-only reflection of their global role.
4. A user can be a member of a project **at most once** — the unique `(project_id, user_id)` constraint is enforced and a duplicate add returns **409** (not a duplicate row).
5. `project_id` is taken from the route; a member cannot be added to or removed from a project the caller does not manage.
6. No `xmin`/row-version exists on `team_members`, and the absence is deliberate and documented; concurrent duplicate adds are resolved by the unique constraint, and removal is idempotent.
7. Every add/remove produces an `activity_logs` row in the same transaction; remove audits **before** deletion and the audit survives; deleting a project or a user cascades the membership while its audit rows remain.
8. This feature's membership records back 003's assignee validation — an integration test confirms 003 accepts an assignee **iff** a matching `team_members` row exists.
9. The Angular `team` route group is lazy-loaded with standalone components (no `@NgModule`); all HTTP lives in `TeamService`; the add-member form uses a Reactive Form with an explicit validator; functional role guards are the only navigation block.
10. Errors are RFC 7807 via the shared mapper; all endpoints appear in Swagger; backend compiles warnings-as-errors with nullable enabled; frontend compiles in strict mode.
11. Both clarified decisions are covered by tests (Clarifications 2026-07-22): **add** accepts any *active* user of any global role and refuses a deactivated user with 400; **remove** is refused with **409** and a dependency message while the member has an open assigned task in the project, and succeeds once those tasks are reassigned or closed — proving the "assignee is always a current member" invariant that backs 003.
12. Unit + integration tests pass (Constitution IX.3 — no merge on failing tests).

### B.9 Open questions for review
| # | Question | Recommendation (not yet a decision) | Status |
|---|---|---|---|
| OQ-004-01 | **Which users are eligible to be added to a project's team** — any user regardless of global role, or TeamMembers only? | **Resolved (Clarifications 2026-07-22): any active user, regardless of global role.** Deactivated users are the only ones refused; membership stays orthogonal to role. | **Resolved** |
| OQ-004-02 | **Removing a member who has open tasks assigned in that project** — block the removal, or auto-unassign their tasks first? | **Resolved (Clarifications 2026-07-22): block with 409 + a dependency message**, a fixed invariant (not configurable). Manager reassigns/closes the tasks first. Preserves the "assignee is always a member" guarantee (backing 003). | **Resolved** |
| OQ-004-03 | May a user be added to a project already in a terminal status (`Completed`/`Cancelled`)? | Yes by default (`Team:AllowManageOnTerminalStatusProject`), for late corrections | No |
| OQ-004-04 | Are deactivated members shown in the roster? | Yes, flagged inactive (`Team:IncludeInactiveMembersInRoster`) | No |
| OQ-004-05 | Out-of-scope roster read → **403** or masked **404**? | **403** (matching 002/003), config flag to mask as 404 | No |
| OQ-004-06 | Surrogate `id` PK + unique `(project_id, user_id)`, or a composite PK? | Surrogate `id` (keeps `activity_logs.entity_id` a clean single uuid, consistent with the other entities) | No |

---

## Functional Requirements

- **FR-001**: The system MUST expose exactly `GET /api/projects/{projectId}/team`, `POST /api/projects/{projectId}/team`, and `DELETE /api/projects/{projectId}/team/{userId}` (Constitution VI.6).
- **FR-002**: A `TeamMember` record MUST reference exactly one project and one user, and MUST NOT carry any role or permission field; a member's authority is their global role from 001.
- **FR-003**: Adding a member MUST take `project_id` from the route (never the body), MUST record who performed the add, and MUST create at most one membership per user per project (a duplicate add returns **409**).
- **FR-004**: A user MUST be a member of a project **at most once**, enforced by a unique `(project_id, user_id)` database constraint.
- **FR-005**: The roster read MUST be project-scoped server-side: Admin any; ProjectManager if owner or member; TeamMember if member. An out-of-scope read MUST return **403** (maskable to **404** by configuration).
- **FR-006**: The roster MUST return each member's user identity and **global** role for display, and MUST return an **empty array** (not 404) for a project with no members.
- **FR-007**: Removing a member MUST be permitted only to Admin or the project owner; a TeamMember MUST be refused with **403**; removing a non-member MUST return **404**.
- **FR-008**: Role checks MUST be declared with `[Authorize(Roles = "...")]` attributes only; project-ownership/membership checks MUST be enforced in the service layer via `AccessDecision`, never in the controller.
- **FR-009**: Every add and remove MUST create an `activity_logs` entry (actor, action, entity type `TeamMember`, entity id, timestamp, change summary) in the same transaction; remove MUST audit before deletion and the entry MUST survive.
- **FR-010**: Deleting a project or a user MUST cascade-delete the associated `team_members` rows; `added_by` MUST be set null when the adding user is deleted; `activity_logs` rows MUST NOT be cascaded away.
- **FR-011**: The `team_members` entity MUST NOT carry an optimistic-concurrency token; concurrency safety MUST come from the unique `(project_id, user_id)` constraint and idempotent removal (this is the deliberate, documented exception to ADR-0004).
- **FR-012**: The roster endpoint MUST return a plain array (no pagination envelope), justified by the bounded size of a project team; the design MUST NOT preclude adding pagination later without breaking clients.
- **FR-013**: Errors MUST be RFC 7807 Problem Details produced by the shared `ErrorKind` mapper; all endpoints MUST be documented via Swagger/OpenAPI.
- **FR-014**: The Angular `team` feature area MUST be lazy-loaded via route-level code splitting with standalone components (ADR-0001); all HTTP MUST live in a dedicated `TeamService` (never in components); the add-member form MUST use a Reactive Form with an explicit validator; a functional role-based route guard MUST be the only mechanism blocking navigation.
- **FR-015**: All persistence MUST go through EF Core with a Code-First migration; no raw SQL and no manual DDL (Constitution IV.1, IV.2).
- **FR-016**: Any **active** user MUST be eligible to be added to a project's team **regardless of their global role** (Clarifications 2026-07-22); the only add-time eligibility gate is that the user is active (a deactivated user is refused with **400**). Membership MUST NOT be restricted by, or coupled to, the user's role.
- **FR-017**: Removing a member who has **open tasks assigned to them in that project** MUST be **blocked with 409 Conflict** and a dependency message identifying the blocking tasks (Clarifications 2026-07-22); the manager MUST reassign or close those tasks before the removal can succeed. This is a fixed invariant (not configurable), preserves the guarantee that a task's assignee is always a current team member (backing 003), and MUST NOT silently orphan or unassign work.

## Non-Functional Requirements
- **NFR-001**: Server-side enforcement is authoritative; the frontend never gates for security.
- **NFR-002**: Roster reads and membership lookups (including 003's assignee validation) are O(1)/indexed; add/remove are single-row writes; no N+1 on member user data.
- **NFR-003**: Structured logging (Serilog); authorization denials logged with actor, project id, and reason.
- **NFR-004**: Nullable reference types + warnings-as-errors (backend); TypeScript strict mode (frontend).
- **NFR-005**: The unique `(project_id, user_id)` constraint is the source of truth for membership uniqueness; application checks are an optimization, not the guarantee.

## Configurability Rules
- **CFG-001**: Whether a deactivated user may be added (`Team:AllowAddInactiveUser`, default no) — the only add-time eligibility gate; any active user of any role is eligible (resolved, Clarifications 2026-07-22).
- **CFG-002**: *(none — removing a member with open assigned tasks is a fixed invariant, not configurable: it is blocked with 409; Clarifications 2026-07-22.)*
- **CFG-003**: Whether a deactivated user may be added (default no).
- **CFG-004**: Whether team management is allowed on a terminal-status project (default yes).
- **CFG-005**: Whether deactivated members appear in the roster (default yes, flagged).
- **CFG-006**: Out-of-scope roster reads return 403 or are masked as 404 (default 403).

## Security Rules
- Authenticated by default; role gate via attributes only; project-scope gate in the service layer.
- Membership carries no role/permission field — adding/removing a member never changes what anyone *is*, only what they can *see*.
- `project_id` from the route; a member cannot be added to or removed from an unmanaged project.
- Uniqueness enforced at the database, not just in code; scope re-checked at write time; deny by default.
- Every add/remove audited; audit rows retained through cascade deletes.

## Audit / Compliance Expectations
Audit every write to team membership — add and remove — with actor, action, entity type (`TeamMember`), entity id, timestamp, and change summary to `activity_logs` (defined in 001), written in the same transaction as the change; remove audits before deletion. Roster reads are not audited. A removal blocked by open assigned tasks (409) changes nothing and writes no audit row; because removal blocks rather than cascading, it never triggers a task mutation. Append-only; audit rows survive cascade deletion of the membership, its project, or its user, keeping staffing history reportable by 006.

## Assumptions
- 001 Auth & RBAC and 002 Projects are implemented: `users`, the three-role model, the single JWT `role` claim, `ICurrentUserService`, `IActivityLogService`, and `Project.owner_id` all exist and are consumed here.
- All five constitution entities are created in the initial migration; a feature owns an entity's API/UI/rules, not its table's existence — so 003's assignee-validation join against `team_members` is satisfied by the records this feature manages.
- A project's team is a bounded, human-scale collection (realistically well under 50), so the roster is returned unpaged (Constitution VI.4 does not require paging here); the design leaves room to add paging later.
- The `team_members` row has no mutable field, so it needs no optimistic-concurrency token (the deliberate exception to ADR-0004).
- Membership records visibility, not permissions; a member's role is always their global role.

## Dependencies
- **Depends on**: [001 Auth & RBAC](../001-auth-rbac/spec.md) — Users, role model, JWT claims, `CurrentUser`, `IActivityLogService`. · [002 Projects](../002-projects/spec.md) — the `Project` entity and the ownership rule that governs who may manage a team.
- **Backs (retroactive)**: [003 Tasks](../003-tasks/spec.md) already assumes "a task's assignee must be a valid team member on the project" and validates assignees against the `team_members` pool. **This feature is what populates and maintains that pool** — 003's assignee validation depends, at runtime, on the membership records created here. The interaction is resolved (Clarifications 2026-07-22): removing a member who has open assigned tasks is **blocked with 409** until those tasks are reassigned or closed, so a task's `assignee_id` can never point at a non-member — the invariant 003 relies on holds at all times, consistent with 002's dependency-aware delete.
- **Consumed by**: 005 Dashboard (staffing/roster aggregates) · 006 Reports (team exports). Both inherit this feature's project-scoped visibility.
- **Infrastructure**: PostgreSQL 18 via EF Core 10 + Npgsql; Serilog; Swagger/OpenAPI.

## Out of Scope
Per-project custom roles or permission overrides beyond a user's existing global role (this feature adds **no** new role dimension); bulk import of team members; the `Project` entity and its ownership rule (002); the `Task` entity and the assignee-validation logic itself (003 — this feature only supplies the membership records that logic reads); authentication and the role model (001); dashboard aggregation (005); report export (006).

---

## Sequence Note

This is the **fourth** module in the sequence (001 Auth & RBAC, 002 Projects, and 003 Tasks complete). It follows the structural template set by [001](../001-auth-rbac/spec.md)/[003](../003-tasks/spec.md) and the merged-file convention, and it closes the last read-only dependency in the domain by backing 003's assignee-validation pool. **005 Dashboard** and **006 Reports** follow last, since both aggregate data from every module before them.
