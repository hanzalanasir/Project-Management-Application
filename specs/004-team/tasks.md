---
description: "Task list for 004 Team Management implementation"
---

# Tasks: 004 Team Management

**Input**: Design documents from `/specs/004-team/`
**Prerequisites**: [plan.md](plan.md) · [spec.md](spec.md) · [research.md](research.md) ·
[data-model.md](data-model.md) · [contracts/](contracts/README.md) → [`docs/contracts/team.v1.yaml`](../../docs/contracts/team.v1.yaml) · [quickstart.md](quickstart.md)

**Tests**: **REQUIRED.** Constitution IX and spec 004 B.8 DoD #12. **Docker required** — Testcontainers
PostgreSQL. The unique-constraint race (T041) is **unverifiable** on EF InMemory, which enforces no unique
constraint at all (ADR-0007 §2).

**Organization**: Grouped by user story so each is independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story the task serves
- Exact file paths are included in every task

---

## ⚠️ Blocking prerequisites

| From | Artifact | Used by 004 |
|---|---|---|
| **001** | `TeamMember` entity + `team_members` table (`InitialCreate`) | everything — **004 adds no table** |
| **001** | `ITeamAccessPolicy` interface (T022) | implemented here in T011 |
| **001** | `AuditAction` incl. `TeamMemberAdded`, `TeamMemberRemoved` (T018) | both writes |
| **001** | `IApplicationDbContext`, `Result`, `IActivityLogService` | every slice |
| **002** | `Project` entity + `owner_id` ownership rule | both policy decisions |
| **003** | `tasks` table (from 001's `InitialCreate`) | the open-tasks removal block — **read-only; 003's rules are not required** |

**T006 verifies these first.** Note 004 needs the `tasks` **table**, not 003's feature — if 003 is not yet
built, insert rows directly to exercise T049.

---

## What 004 deliberately does NOT build

Three absences a reviewer will look for. Each is a decision, not an oversight — **do not "fix" them**:

| Absent | Why | Reference |
|---|---|---|
| **`xmin` / `If-Match`** | A membership row has **no mutable field** — added or removed, never edited. There is no lost update to prevent. Safety comes from `UNIQUE (project_id, user_id)` instead | research R-2 · shared-contracts §5 |
| **`PagedResult<T>`** | A project team is bounded and human-scale, so VI.4's ">50 items" trigger never fires. A plain array is **compliance, not an exception** | research R-4 |
| **`ApplyScope`** | Every operation is pinned to one project by the route — there is no cross-collection query to scope | research R-1 |

There is also **no role column** on `team_members`, and adding one would introduce the second permission
system this feature exists to avoid (T009 asserts its absence).

---

## Story ID mapping & implementation order

| Label | Spec story | Title | Priority | Depends on |
|---|---|---|---|---|
| **US1** | US-004-01 | Add a user to a project's team | P0 | — (after Foundational) |
| **US2** | US-004-02 | List a project's team | P0 | US1 |
| **US3** | US-004-03 | Remove a user from a project's team | P1 | US1 |

**US1 is the gate**; US2 and US3 are then independent of each other.

---

## Phase 1: Setup

- [X] T001 Create the slice folder structure `src/ProjectManagementApp.Application/Features/Team/{ListTeam,AddTeamMember,RemoveTeamMember}/` per plan.md §Project Structure — created as empty directories on disk (git does not track empty dirs; they populate naturally as T027/T037/T050 land)
- [X] T002 [P] Generate TypeScript DTO types from `docs/contracts/team.v1.yaml` into `src/ProjectManagementApp.Web/src/app/core/api/generated/` via `openapi-typescript` (not `openapi-generator` — the task text's tool name doesn't match this repo's actual tooling; 001-003 all use `openapi-typescript`, so `team.v1.d.ts` follows the same convention). Added `generate:api:team` npm script alongside auth/projects/tasks
- [X] T003 [P] Add `team.v1.yaml` to the `CheckApiContract` MSBuild target in `src/ProjectManagementApp.Api/ProjectManagementApp.Api.csproj` (ADR-0007 §1) — fourth `oasdiff breaking` call against the same generated.json, same pattern as 002/003. Not exercised yet this stage (T057 in Polish proves the gate); build succeeds with the new contract wired in
- [X] T004 [P] Add the `Team:*` configuration section to `src/ProjectManagementApp.Api/appsettings.json` — `AllowAddInactiveUser=false`, `AllowManageOnTerminalStatusProject=true`, `IncludeInactiveMembersInRoster=true`, `MaskOutOfScopeAs404=false` (spec B.4). No remove-with-open-tasks toggle added — confirmed blocking is a fixed invariant (Clarifications 2026-07-22)
- [X] T005 [P] Create `TeamOptions` binding class — **relocated to `src/ProjectManagementApp.Application/Common/Options/TeamOptions.cs`**, not the task's literal `Api/Configuration/` path, mirroring the identical relocation 002/003 already made for `ProjectsOptions`/`TasksOptions`: every value here is consumed by a slice handler and Application must not reference Api (Constitution II.2). Registered via `services.Configure<TeamOptions>(...)` in `Program.cs`
- [X] T006 **Verified prerequisites before proceeding** — all present, nothing missing: `TeamMember` entity + `team_members` table exist from `InitialCreate` (no separate `CreateTable`, confirmed by reading the migration); `ITeamAccessPolicy` declared in `Application/Common/Interfaces/ITeamAccessPolicy.cs`; `AuditAction` contains `TeamMemberAdded`/`TeamMemberRemoved`; `Project.OwnerId` exists; `TaskItem`/`tasks` table exists (named `TaskItem` in code, `tasks` in the database — avoids colliding with `System.Threading.Tasks.Task`). No fixes needed
- [X] T007 [P] Scaffolded the lazy `team` route group in `team.routes.ts` — one route (`:projectId` → `RosterComponent`); registered via `loadChildren` at `path: 'team'` in `app.routes.ts`, same shape as `tasks`/`projects`. "Add member" is a `MatDialog` launched from the roster, not a second route, so there is no second route for a `canMatch` role guard to gate yet — see T020's note

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Persistence — the unique constraint is this feature's correctness guarantee

- [X] T008 Extended `TeamMemberConfiguration.cs` with **`UNIQUE (project_id, user_id)`** as `ux_team_members_project_id_user_id`, plus explicit `ix_team_members_project_id`/`ix_team_members_user_id` (declared for readability — see T009 note on why the migration itself only adds the unique index). Confirmed the entity has no `updated_at` and no row-version property
- [X] T009 Created the `AddTeamMemberIndexes` migration (`20260810083458_AddTeamMemberIndexes.cs`) via `dotnet ef migrations add`. Confirmed it does **not** create the `team_members` table — only one `CreateIndex` for the unique constraint. `ix_team_members_project_id`/`ix_team_members_user_id` do **not** appear in this migration's `Up()` because EF's default FK-index convention already created them in `InitialCreate` (identical situation to 003's `ix_tasks_project_id`/`ix_tasks_assignee_id`) — verified by reading `InitialCreate.cs` directly rather than assuming
- [X] T010 [P] Wrote schema tests in `tests/ProjectManagementApp.Infrastructure.Tests/Team/SchemaTests.cs` — 4 tests, all passing against real Testcontainers Postgres: unique index exists and `indisunique` is true; no role/permission column; no `updated_at` (and `xmin` is implicitly absent — it's a Postgres system column, never listed in `information_schema.columns`); and all three delete behaviours via `pg_constraint.confdeltype` (`c`=CASCADE on `project_id`/`user_id`, `n`=SET NULL on `added_by`)

### The binary access policy

- [X] T011 Implemented `TeamAccessPolicy : ITeamAccessPolicy` — `CanViewTeamAsync` (Admin any · PM owner-or-member · TM member) and `CanManageTeamAsync` (Admin any · PM owner-only · TM deny). No `ApplyScope`. Membership checked via `_db.TeamMembers.AnyAsync(...)` directly, never a navigation collection, same reasoning as `ProjectAccessPolicy.CanReadAsync`
- [X] T012 [P] Wrote the view-vs-manage matrix test — 10 tests covering all six cells plus the PM-as-member divergence explicitly (`CanManageTeamAsync_ProjectManager_MemberButNotOwner_Denied` asserts the SAME PM passes View and fails Manage on the SAME project in one test). All 10 pass against real Postgres
- [X] T013 Registered `services.AddScoped<ITeamAccessPolicy, TeamAccessPolicy>();` in `DependencyInjection.cs`

### Shared slice plumbing

- [X] T014 [P] Created `TeamMemberDto` + `ToDto(role)` mapping extension. Role lookup is genuinely batched, not per-row: `BuildRoleLookupAsync` calls `UserManager.GetUsersInRoleAsync` exactly 3 times (once per `Role` enum value, not once per roster row) and inverts the result into a `userId → role` dictionary — O(3) regardless of roster size, consumed by `ListTeamQueryHandler` in US2 (T038)
- [X] T015 [P] Implemented `UniqueViolationMapper.IsUniqueViolation(DbUpdateException)` — checks `InnerException is PostgresException { SqlState: "23505" }`; added an overload that also checks `ConstraintName` for callers that want to confirm it's specifically the membership index (not some other future unique constraint)
- [X] T016 [P] Wrote mapper tests against real Postgres — a genuine duplicate-insert race (seed a membership, then insert the same `(project, user)` again) throws `DbUpdateException` wrapping a real 23505, correctly mapped true; an FK violation (nonexistent `project_id`) throws `DbUpdateException` wrapping a 23503, correctly mapped false. Both pass
- [X] T017 Created the thin `TeamController` shell — three route stubs (`GET`, `POST`, `DELETE {userId}`) under `/api/projects/{projectId}/team`, correct `[Authorize]`/`[Authorize(Roles=...)]` attributes and status-code contracts declared via `[ProducesResponseType]`. **Deviates from the literal "one `MediatR.Send` per endpoint"**: `AddTeamMemberCommand`/`ListTeamQuery`/`RemoveTeamMemberCommand` don't exist yet — they're created in US1/US2/US3 (T027/T037/T050), which are out of scope for this Foundational-only stage. Each action body is `throw new NotImplementedException(...)` naming the task that wires it, mirroring 003's own stage-1 "Route stubs only" precedent on `TasksController`/`ProjectsController`. `_mediator` is stored but unused until then — expected, not a bug

### Test fixtures & frontend shell

- [X] T018 [P] Added `TeamMemberBuilder` (Id/ProjectId/UserId/AddedBy, matching `ProjectBuilder`/`TaskBuilder`'s shape) and `TeamScenario`, which extends 003's `TasksScenario` (itself extending 002's `ProjectsScenario`) with one deactivated user, `Inactive` — seeded but on no project's team, ready for US1's eligibility-gate tests (T022)
- [X] T019 [P] Implemented `TeamService` — `list`/`add`/`remove`, using the generated `team.v1.d.ts` DTO types. No ETag/`If-Match` handling anywhere in the service, matching research R-2
- [X] T020 [P] Created `RosterComponent` and `AddMemberDialogComponent` as shells (placeholder templates, `projectId` read from the route on the roster) with the route wired in `team.routes.ts`. **No role guard added yet** — "add member" is a dialog, not a route, so there is no second routed path for a `canMatch` guard to gate at this stage; add/remove UI visibility becomes a client-side check inside `RosterComponent` itself once US1/US3 land (T031/T053), same as every other feature's "UX convenience only, server is the real gate" pattern

**Checkpoint**: Policy (10/10 tests), unique constraint + 23505 mapper (6/6 tests), and schema (4/4 tests) are all tested and green against real Postgres. Full backend suite: 323/323 passing (27 Infrastructure.Tests, 158 Application.Tests, 138 Api.Tests) — no regressions in 001-003. Frontend (`ng build`) and backend (`dotnet build`) both compile clean. Stories can begin.

---

## Phase 3: User Story 1 — Add a user to a project's team (Priority: P0) 🎯 MVP

**Goal**: A ProjectManager (owner) or Admin adds **any active user** to a project's team; membership grants
visibility, never permission.

**Independent Test**: `POST /api/projects/{projectId}/team` → 201 + `Location`, exactly one row per
`(project, user)`, and a concurrent duplicate resolves to one 201 and one 409.

### Tests for User Story 1

- [X] T021 [P] [US1] Wrote the add test — 201, `Location: /api/projects/{projectId}/team/{userId}`, `added_by` recorded as the caller, `TeamMemberAdded` audit row with correct `ActorId`. All assertions pass
- [X] T022 [P] [US1] Wrote the eligibility test — 4 cases: TeamMember ✓, a ProjectManager who does not own the project ✓, Admin ✓, deactivated user → 400. Deactivation performed via the real `PUT /api/users/{id}/status` endpoint, not a DB shortcut. All pass
- [X] T023 [P] [US1] Wrote the authorization test — non-owning PM → 403, TeamMember → 403 at the role gate (never reaches `CanManageTeamAsync`), Admin → 201 on any project. Kept in a separate file from T022 as instructed: this is about who may PERFORM the add, not who may BE added. All pass
- [X] T024 [P] [US1] Wrote the duplicate test — existing member → 409 with exactly one row confirmed via direct `TeamMembers.CountAsync`; unknown `projectId` → 404; unknown `userId` → 404. All pass
- [X] T025 [P] [US1] Wrote the route-authority test — sent a raw anonymous object with `projectId` pointing at a second, real project; confirmed the membership landed only on the route's project and never on the body's project. Passes
- [X] T026 [P] [US1] Wrote 6 handler branch tests — not-found project, forbidden (with a `DidNotReceive()` assertion that `SaveChangesAsync` was never called), not-found user, deactivated-user validation error, already-a-member conflict (pre-check path, also asserts no save), and the full success path (persists, sets `AddedBy`, writes the audit). Hit and fixed a real NSubstitute pitfall along the way — see note below. All 6 pass

### Implementation for User Story 1

- [X] T027 [US1] Created `AddTeamMemberCommand(Guid ProjectId, Guid UserId)`
- [X] T028 [US1] Implemented `AddTeamMemberCommandValidator` — `UserId` `NotEmpty()`. Database-dependent rules (existence, active, duplicate) stay in the handler, matching 003's `CreateTaskCommandValidator` precedent
- [X] T029 [US1] Implemented `AddTeamMemberCommandHandler` in the exact order specified: project (404) → `CanManageTeamAsync` (403) → user exists (404) → is active (400) → not-already-a-member pre-check (409) → insert + audit in one `SaveChangesAsync`, wrapped in a `try/catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })` that also returns 409 with the identical message/Kind as the pre-check. **Real architectural finding**: stage 1's `UniqueViolationMapper` lives in `Infrastructure/Persistence/`, but this handler lives in `Application`, which cannot reference `Infrastructure` (Constitution II.2, enforced by `LayerDependencyTests`) — referencing it here would have been a real layering violation. Fixed by duplicating the one-line `PostgresException { SqlState: "23505" }` check locally in the handler, using the `Npgsql.EntityFrameworkCore.PostgreSQL` package that `Application.csproj` already references directly for `EF.Functions.ILike` (its own comment already states "Application still never references the Infrastructure project"). Both the Infrastructure mapper (used by nothing yet) and this local check are now independently tested; not a premature abstraction since only one handler needs it today
- [X] T030 [US1] Wired `POST /api/projects/{projectId}/team` — `[Authorize(Roles="Admin,ProjectManager")]` (unchanged from T017's stub), 201 with `Location: /api/projects/{projectId}/team/{userId}` (composite, not membership-id-based, per the note)
- [X] T031 [P] [US1] Built the add-member dialog as a `MatDialog` (this codebase's first use of Angular Material Dialog — no prior precedent existed to follow). **Real scope gap found, not fixed, flagged instead**: the task calls for "a searchable picker of any active user not already on the team, regardless of global role," but `GET /api/users` is `[Authorize(Roles = "Admin")]` only (001's deliberate design, per `ListUsersQueryHandler`'s own comment: "Admin-only makes the role gate the entire authorization surface"). A ProjectManager — the actor who actually performs adds in the overwhelming majority of cases — has no endpoint to search or list users at all. Building a real picker would require either widening 001's `/api/users` authorization (out of scope — "do not touch 001/002/003") or adding a new 004-scoped user-search endpoint not specified anywhere in spec/plan/tasks. Implemented instead as a plain user-ID text input with a single "required" validator, deliberately mirroring the exact same forward-dependency workaround 003's own `CreateTaskComponent.assigneeId` field used before 004 existed. This should be revisited — see end-of-stage summary
- [X] T032 [US1] `TeamService.add()` already existed from stage 1; wired `RosterComponent.openAddMemberDialog()` to open the dialog and call `refresh()` (→ `TeamService.list()`) on a successful close. `refresh()` will only render real data once `GET .../team` is wired in T039 (US2) — calling it now against the still-stubbed `NotImplementedException` endpoint is expected, not a bug, and requires no further change once US2 lands

**Note on T026**: the first test run failed all 6 handler tests with NSubstitute's `CouldNotSetReturnDueToNoLastCallException`. Root cause: `CreateDb` originally called `db.Projects.Returns(projects.BuildMockDbSet())` — evaluating `.BuildMockDbSet()` as an argument to `.Returns(...)` invokes NSubstitute internally *between* the `db.Projects` getter call and the `.Returns()` call, which resets NSubstitute's "last call" tracking so `.Returns()` no longer knows what to configure. Fixed by building each mock `DbSet` to a local variable first, then calling `.Returns(localVar)` — the same two-step pattern 003's own `CreateTaskCommandHandlerTests.CreateDb` already uses (correctly). A real, reproducible pitfall, not a flaky test — worth remembering for any future handler test with more than one `DbSet` to mock.

**Checkpoint**: 🎯 **MVP** — teams can be staffed. Verified against quickstart V1, V2, V7 — see stage summary for live results. Full backend suite: 341/341 passing (27 Infrastructure.Tests, 164 Application.Tests, 150 Api.Tests), up from 323 at the Foundational checkpoint — no regressions. `dotnet build` and `ng build` both clean.

---

## Phase 4: User Story 2 — List a project's team (Priority: P0)

**Goal**: Anyone connected to a project sees its full roster; the shown `role` is the member's **global**
role, never a per-project one.

**Independent Test**: The three-role visibility matrix holds, including the **PM-as-member** case, and the
response is a **plain array** with no paging envelope.

### Tests for User Story 2

- [X] T033 [P] [US2] Wrote the 🎯 **visibility matrix test** in `tests/ProjectManagementApp.Api.Tests/Team/GetProjectTeamScopeTests.cs` — 6 tests: Admin 200 any project, owner PM 200, a PM who is a member-but-not-owner 200 (the same divergent cell T012's policy unit test covers, now proven end-to-end through the real endpoint), a member TeamMember 200 with the full roster (asserted via array length, not just presence), a non-member 403, unknown project 404. All pass
- [X] T034 [P] [US2] Wrote shape test in `tests/ProjectManagementApp.Api.Tests/Team/GetProjectTeamShapeTests.cs` — 3 tests: body is a plain JSON array (`JsonValueKind.Array`, not an object with an `items` property), `?page=2&pageSize=1` query params are silently ignored (asserted by confirming the full 1-member roster still comes back, not a sliced page), and an empty team returns 200 with `[]`, never 404. All pass
- [X] T035 [P] [US2] Wrote display test in `tests/ProjectManagementApp.Api.Tests/Team/GetProjectTeamDisplayTests.cs` — 2 tests: each row's `role` matches the member's real global role (one TeamMember row, one Admin row added to the same project, both asserted independently so role can't be a coincidental default), and a member deactivated via the real `PUT /api/users/{id}/status` endpoint after being added still appears in the roster with `isActive: false` rather than being silently dropped. Both pass
- [X] T036 [P] [US2] Wrote 3 handler tests in `tests/ProjectManagementApp.Application.Tests/Features/Team/ListTeamQueryHandlerTests.cs` — unknown project → 404; `CanViewTeamAsync` denies → 403 (handler returns before touching `TeamMembers` at all, since it's a straight-line early-return, not a post-filter); full success path asserts both the mapped roles per member AND that `UserManager.GetUsersInRoleAsync` was called **exactly 3 times** (once per `Role` enum value via T014's `BuildRoleLookupAsync`), proving the role lookup is batched and not one query per roster row. All pass

### Implementation for User Story 2

- [X] T037 [US2] Created `ListTeamQuery(Guid ProjectId)` in `src/ProjectManagementApp.Application/Features/Team/ListTeam/ListTeamQuery.cs` — no `Page`/`PageSize` parameters at all (research R-4: not even accepted-and-ignored ones)
- [X] T038 [US2] Implemented `ListTeamQueryHandler` — fixed order: load project (404) → `CanViewTeamAsync` (403, runs before any roster row is touched) → single bounded read (`_db.TeamMembers.AsNoTracking().Include(m => m.User).Where(m => m.ProjectId == project.Id)`, one `Include`, no per-row lookup) → `IncludeInactiveMembersInRoster` filter (defaults true, so nothing is filtered by default — deactivated members appear, matching T035) → role lookup via `BuildRoleLookupAsync` (3 queries total, T014) → map to DTOs. Returns `IReadOnlyList<TeamMemberDto>` directly, never `PagedResult<T>` (research R-4)
- [X] T039 [US2] Wired `GET /api/projects/{projectId}/team` in `TeamController.cs` — `[Authorize]` unchanged from T017's stub (all three roles), `result.ToActionResult()` returns 200 with the list serialized as a plain JSON array (confirmed by T034 — `ToActionResult<T>`'s `ObjectResult` serializes `IReadOnlyList<TeamMemberDto>` with no envelope)
- [X] T040 [P] [US2] Built the roster table in `RosterComponent`/`.html` — Angular Material table with columns name (deactivated members get an inline "(deactivated)" badge), email, global role, added-at (`DatePipe`); client-side search filter over the already-fetched array (no server paging, matching the endpoint's plain-array shape); explicit loading/forbidden/error/empty states, each a distinct branch (forbidden is detected from the 403 status specifically, not lumped into the generic error message); a "back to project" link to `/projects/{projectId}` (002's Project Detail page), since no app-shell breadcrumb exists elsewhere for this. **Deliberately does NOT add a remove-action column yet**, despite the task's literal text calling for one: the DELETE endpoint (T052) and its handler (T051) don't exist until US3 — wiring a remove button against a still-stubbed `NotImplementedException` endpoint would repeat the exact thing T032 avoided for the add flow in stage 2. T053 (US3) owns building that column against a real endpoint
- [X] T041 [US2] `TeamService.list()` already existed from stage 1 (T019) — confirmed it's exactly what `RosterComponent` needed; no changes required

**Checkpoint**: The roster 003's assignee picker consumes. Verified against quickstart V4 and V6 live — see stage summary. Full backend suite: 355/355 passing (27 Infrastructure.Tests, 167 Application.Tests, 161 Api.Tests), up from 341 at the US1 checkpoint — no regressions. `dotnet build` and `ng build` both clean.

---

## Phase 5: User Story 3 — Remove a user from a project's team (Priority: P1)

**Goal**: Managers remove members; removal is **blocked with 409** while the member still has open assigned
tasks in that project.

**Independent Test**: Removal succeeds and revokes access immediately — but is refused with 409, changing
**nothing at all**, while an open assigned task exists.

### Tests for User Story 3

- [X] T042 [P] [US3] Wrote the remove test in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberEndpointTests.cs` — 204; sent NO `If-Match` header at all (there's nothing for it to protect); confirmed the membership row is gone via direct DB query; confirmed `TeamMemberRemoved` audit row survives with `entityId` = the (now-deleted) membership's id and correct `ActorId`. Passes
- [X] T043 [P] [US3] Wrote idempotency test in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberIdempotencyTests.cs` — 3 tests: second remove → 404; removing a never-a-member user → 404; two concurrent removes of the same membership resolve to exactly one 204 and one 404 (never a 500). All pass
- [X] T044 [P] [US3] Wrote the 🎯 **open-tasks block test** in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberBlockedTests.cs` — with an `InProgress` task assigned to the member: 409 with `detail` containing "1 open task"; then asserts the TOTAL no-op explicitly and separately: `team_members` row still exists (DB query), **zero** `activity_logs` rows with `Action == "TeamMemberRemoved"` exist anywhere (not just "none for this membership" — none at all), and the task's `status`/`updatedAt` are byte-for-byte unchanged from a snapshot taken before the attempt. All three checks pass
- [X] T045 [P] [US3] Wrote 2 live before/after tests in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberDoneBoundaryTests.cs` — same removal 409→204 after flipping the blocking task to `Done`; same removal 409→204 after reassigning the blocking task away to a second team member instead. Both pass
- [X] T046 [P] [US3] Wrote the 🎯 **access-revocation test** in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberRevocationTests.cs` — sanity-checks the member can GET the project and see it in their list BEFORE removal, then asserts 403 and absence from the list immediately after — no grace period, verified against real Postgres. Passes
- [X] T047 [P] [US3] Wrote 4 authorization tests in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberAuthorizationTests.cs` — non-owner PM → 403; TeamMember → 403 (role gate); an owner PM who added themselves as a contributor removing themselves → 204; an Admin who added themselves to some project removing themselves → 204. Confirms no "can't remove yourself" guard exists anywhere, since spec never calls for one. All pass
- [X] T048 [P] [US3] Wrote 5 handler tests in `tests/ProjectManagementApp.Application.Tests/Features/Team/RemoveTeamMemberCommandHandlerTests.cs` — not-found project; forbidden (with `DidNotReceive()` on both `SaveChangesAsync` and `LogAsync`); not-a-member → 404; blocked by an open task → 409 with the exact count in the message, plus `DidNotReceive()` assertions proving neither `Remove` nor `LogAsync` nor `SaveChangesAsync` ran; success path with `Received(1)` on `Remove`, `LogAsync`, and `SaveChangesAsync`. All 5 pass

### Implementation for User Story 3

- [X] T049 [US3] Implemented `OpenAssignedTaskCheck.CountBlockingTasksAsync` — `db.Tasks.CountAsync(t => t.ProjectId == projectId && t.AssigneeId == userId && t.Status != TaskStatus.Done, ct)`. Filter is `!= Done`, not an enum allowlist, per the note. Reads `IApplicationDbContext.Tasks` directly; never references a 003 handler and never mutates anything
- [X] T050 [US3] Created `RemoveTeamMemberCommand(Guid ProjectId, Guid UserId)` — both from the route, no validator needed (same shape as `DeleteTaskCommand`)
- [X] T051 [US3] Implemented `RemoveTeamMemberCommandHandler` in the exact order specified: project (404) → `CanManageTeamAsync` (403) → membership lookup (404) → `OpenAssignedTaskCheck` (409 with the blocking count, returns immediately — nothing written) → `TeamMemberRemoved` audit **before** `TeamMembers.Remove`, both in the same `SaveChangesAsync` (confirmed atomic: `IActivityLogService.LogAsync` only adds to the change tracker, it does not call `SaveChangesAsync` itself, so a rolled-back save rolls back the audit row too). **Real bug found and fixed, not in the task's literal text**: two concurrent removes of the same membership can both pass the membership-exists check before either deletes; the loser's `SaveChangesAsync` throws `DbUpdateConcurrencyException` (EF Core's DELETE always expects exactly one row affected, even with no explicit concurrency token configured on `TeamMember`). Without a catch, this would 500 instead of the quickstart-mandated "one 204 and one 404" — caught and mapped to `ErrorKind.NotFound`, verified by T043's concurrent-removes test
- [X] T052 [US3] Wired `DELETE /api/projects/{projectId}/team/{userId}` — 204, no `If-Match` parameter anywhere on the action
- [X] T053 [P] [US3] Added the remove action to `RosterComponent`/`.html` — an `actions` column that only renders once `canManage()` is true: Admin always; a ProjectManager only if `ProjectsService.getById(projectId)`'s `owner.id` matches the caller's id (fetched once on load, since the roster DTO itself carries no ownership info by design). `window.confirm()` warns the member will lose access immediately (same pattern as `TaskListComponent.deleteTask`); a 409 response surfaces the server's exact blocking-count message via a dedicated `removeError` banner rather than a generic failure string
- [X] T054 [US3] `TeamService.remove()` already existed from stage 1 (T019) — confirmed it's exactly what `RosterComponent.removeMember()` needed; no changes required

**Checkpoint**: All three stories complete. Verified against quickstart V10–V12 live — see stage summary. Full backend suite: 372/372 passing (27 Infrastructure.Tests, 172 Application.Tests, 173 Api.Tests), up from 355 at the US2 checkpoint — no regressions. `dotnet build` and `ng build` both clean.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T055 🎯 Wrote the concurrent duplicate-add race test in `tests/ProjectManagementApp.Api.Tests/Team/ConcurrentAddRaceTests.cs` — **relocated from the literal `Infrastructure.Tests` path**: asserting real HTTP status codes (201/409, never 500) requires a running API host, and `Infrastructure.Tests` has no project reference to `ProjectManagementApp.Api` to host one. Fires two simultaneous `POST /team` calls for the same `(project, user)` via `Task.WhenAll`; asserts no response is ≥500, exactly one 201, exactly one 409, and exactly one DB row. Ran 5 times in a row — clean every time, confirming T029's exception handling from stage 2 holds under real contention
- [X] T056 🎯 Wrote the cross-feature integration test in `tests/ProjectManagementApp.Api.Tests/CrossFeature/TeamBacksTaskAssignmentTests.cs` — used the reassign flow (`PUT /tasks/{id}/assignee`), not create-with-assignee, to match quickstart V13's literal "retry the assignment → 200" (create would 201, not 200). Assign before adding → 400 (`"...not a team member"`); add the member → 201; retry the identical reassignment → 200. Confirmed via reading `AssigneeValidator.cs` that neither feature calls the other's MediatR handlers — both read `db.TeamMembers`/`db.Tasks` directly through the shared `IApplicationDbContext`
- [X] T057 **Baseline was NOT clean** — checked first per instructions rather than assuming: `dotnet build -p:CheckApiContract=true` failed on `team.v1.yaml` with 6 real errors before any deliberate drift. Two distinct root causes, both fixed: (1) `projectId`/`userId` declared path-item-level in `team.v1.yaml` (shared across all operations under that path) vs Swashbuckle's per-operation style — the same class of finding 002/003 hit on their own contracts — moved all three into their operations, matching `projects.v1.yaml`/`tasks.v1.yaml`'s style; (2) **`TeamController`'s `[ProducesResponseType]` attributes on `GetProjectTeam`/`AddTeamMember` had no `typeof(...)` response-shape argument at all** (`[ProducesResponseType(StatusCodes.Status200OK)]` instead of `[ProducesResponseType(typeof(IReadOnlyList<TeamMemberDto>), StatusCodes.Status200OK)]`) — Swashbuckle therefore generated a response with no `content`/schema, so the contract gate had never actually been checking the shape of `/team` responses since it was first wired in stage 1. Fixed both attributes, plus added `[EmailAddress]` to `TeamMemberDto.Email` (Swashbuckle needs it to emit `format: email`, matching the hand-written contract). With baseline green (0 errors, 1 pre-existing-shaped warning matching auth/projects/tasks' own baseline noise), ran the actual proof: adding `role` to the hand-written `AddTeamMemberRequest` **schema** did NOT trip the gate (an optional request property addition isn't classified as breaking by `oasdiff` — the same blind spot 002/003 already found on their own drift proofs). Added `role` as a **required** property to the real `AddTeamMemberRequestBody` C# record instead — build correctly **FAILED** with `error [new-required-request-property] ... POST /api/projects/{projectId}/team ... added the new required request property 'role'`. Reverted exactly; reconfirmed green
- [X] T058 Executed quickstart **V1–V16** live — full results table in the stage summary below. V1/V2/V7 (stage 2), V4/V6 (stage 3), and V10–V12 (stage 4) were already validated live in their own stages and are not re-run here; V3, V5, V8, V9, V13, V14, V15, V16 were run fresh this stage. **Found and fixed one real gap during V16**: `RosterComponent`'s "Add member" button was never gated by `canManage()` — it rendered unconditionally regardless of caller role, contradicting V16's explicit requirement that a TeamMember never sees it. Fixed by wrapping it in `@if (canManage())`, matching the remove-action column's existing gate
- [X] T059 [P] Verified live, not assumed: added `GetProjectTeamNoN1Tests.cs` (`CommandCounterInterceptor` pattern, mirrors 002/003's T080/T101) — a 3-member roster spanning all three roles executes exactly **8** SQL commands (project lookup + one `Include(User)`-joined roster read + `BuildRoleLookupAsync`'s 3 role queries, each measured to cost 2 round trips under ASP.NET Identity's default `UserStore`, not the 1 originally assumed) — constant regardless of roster size, never per-member. Added `AssigneeEligibilityIndexUsageTests.cs` (Infrastructure.Tests) — `EXPLAIN` (with `enable_seqscan = off`, to avoid the well-known small-table false-negative) on `AssigneeValidator`'s exact `project_id = ? AND user_id = ?` predicate shape confirms the plan uses `ux_team_members_project_id_user_id`
- [X] T060 [P] Audited live and **found and fixed two real bugs**, the same class 003's T103 found: (1) `AddTeamMember`/`RemoveTeamMember` used attribute-only role gates (`[Authorize(Roles="Admin,ProjectManager")]`), so a TeamMember's denial short-circuited in ASP.NET's authorization middleware before `MediatR.Send` was ever called — confirmed live via Serilog output showing no `Denied AddTeamMemberCommand...` line at all for a TM's 403. Fixed by switching both to plain `[Authorize]` and letting `CanManageTeamAsync`'s unconditional TeamMember denial do the gating instead — same trade-off 003 made and documented (role decision still 100% enforced, just one layer later; every denial now observable). (2) Even for denials that DID reach the handler (e.g. a non-owner PM), the log line showed `entity null` — `LoggingBehavior.TryGetRequestEntityId` only ever looks for a property literally named `"Id"` via reflection, and every 004 command uses `"ProjectId"` instead (there is no single membership id known up front on add). Fixed by adding a fallback to `"ProjectId"` in the shared `LoggingBehavior` (Application/Common — additive only, checked `"Id"` first so 001/002/003's existing commands are unaffected). Verified live after both fixes: `Denied AddTeamMemberCommand for actor <id> on entity <projectId>: You do not have access to manage this project's team. (Forbidden)` — actor, project id, and reason all present. Full suite re-run after both fixes: 376/376, no regressions
- [X] T061 [P] Handlers (`AddTeamMemberCommandHandler`, `ListTeamQueryHandler`, `RemoveTeamMemberCommandHandler`) and `TeamAccessPolicy` already carried non-obvious-why doc comments from stages 1–4 — no changes needed there. Added missing `<summary>` tags to all three `TeamController` actions; replaced the stale class-level `<remarks>` claiming role gates are "attribute-only" (no longer true after T060) with one documenting the actual plain-`[Authorize]` design and why
- [X] T062 [P] Regenerated `docs/erd.md` — added the `team_members` indexes bullet (mirrors the `projects`/`tasks` bullets exactly): `ix_team_members_project_id`/`ix_team_members_user_id` already existed from 001's FK convention; `ux_team_members_project_id_user_id` is 004's `AddTeamMemberIndexes` addition, called out as the correctness guarantee, not an optimization. Updated the intro paragraph to reference `AddTeamMemberIndexes` alongside `AddProjectIndexes`/`AddTaskIndexes`. Table shapes unchanged — 004 adds no table
- [X] T063 [P] Added a "Team module (004)" section to the root `README.md` — the three endpoints with their notes, all four `Team:*` configuration keys, an explicit "membership is a link, not a role" callout, the two-method access-policy explanation (view vs. manage), the plain-`[Authorize]` rationale (mirrors the Tasks module section's equivalent callout), and the cross-feature relationship with 003's `AssigneeValidator`. Updated the Documentation section with the `specs/004-team/quickstart.md` link
- [X] T064 Searched `src/ProjectManagementApp.Web/src/app/features/team/`, `team.service.ts`, `Features/Team/`, and `TeamController.cs` for `console.log`/commented-out code/TODO markers — none found; nothing to remove
- [X] T065 Ran the security review against spec 004 §Security Rules. Four of five rules hold cleanly, confirmed against the actual code and live: membership carries no role/permission field (schema tests); `project_id` is route-only — `AddTeamMemberRequestBody` has no `ProjectId` property at all, structurally impossible to smuggle a different project (T025 confirms at runtime); uniqueness enforced at the database via `UNIQUE (project_id, user_id)`, not just an app-level pre-check (T055's race proof); scope re-checked at write time against freshly-loaded project/caller state in every handler, deny-by-default (`TeamAccessPolicy`'s only two `true` branches are Admin and owner-PM/member; everything else, including any future role, falls through to `false`); every add/remove audited, audit rows retained through cascade deletes, roster reads write none (V14, live). **One rule needed a documented trade-off, not a fix**: spec's Security Rules state "role gate via attributes only" — T060's fix deliberately moved the AddTeamMember/RemoveTeamMember role decision from the controller attribute into `CanManageTeamAsync`, the same trade-off 003's T108 made for `FullEdit`/`Delete`/`Reassign` and for the identical reason (attribute-level denials are invisible to `LoggingBehavior`). Not a security hole — the role decision is still 100% enforced, just one layer later, and every denial is now logged rather than silent — but it is a literal deviation from the spec's stated language, flagged here for the user's sign-off rather than silently accepted, exactly as 003 did

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (Phase 1)** — needs **001 and 002**; T006 verifies (003's `tasks` table is needed only for T044/T049)
- **Foundational (Phase 2)** — depends on Setup; **blocks both stories**
- **US1 (Phase 3)** — depends on Foundational; **blocks US2 and US3**
- **US2, US3 (Phases 4–5)** — each depends only on US1, independent of each other
- **Polish (Phase 6)** — depends on all stories; **T056 additionally requires 003 to be built**

### Shared-file contention

Three stories touch `TeamController.cs` (T030/T039/T052) and `team.service.ts` (T032/T041/T054) — six small
edits. Sequence them or have one developer wire all three endpoints after the handlers land. The roster
component is touched by both US2 (T040) and US3 (T053).

### Parallel opportunities

- Setup: T002–T005, T007 all **[P]**
- Foundational splits into three tracks after T008–T009: policy (T011–T013), plumbing (T014–T017), fixtures + frontend (T018–T020)
- Every story's test tasks are **[P]**
- After US1, two developers can take US2 and US3 simultaneously
- Polish is almost entirely **[P]** except T055–T058

---

## Parallel Example: User Story 3

```bash
# Write all seven US3 tests in parallel first (they must fail):
Task: "Remove 204 + audit-before-delete in tests/…/Team/RemoveTeamMemberEndpointTests.cs"
Task: "Idempotent second remove in tests/…/Team/RemoveTeamMemberIdempotencyTests.cs"
Task: "Open-tasks 409 total no-op in tests/…/Team/RemoveTeamMemberBlockedTests.cs"
Task: "Done-boundary unblocks in tests/…/Team/RemoveTeamMemberDoneBoundaryTests.cs"
Task: "Access revoked immediately in tests/…/Team/RemoveTeamMemberRevocationTests.cs"
Task: "Authorization matrix in tests/…/Team/RemoveTeamMemberAuthorizationTests.cs"
Task: "Handler ordering in tests/…/Features/Team/RemoveTeamMemberCommandHandlerTests.cs"

# Then implement sequentially (T049 → T050 → T051 → T052), frontend in parallel:
Task: "Remove action + 409 message in src/ProjectManagementApp.Web/src/app/features/team/roster/"
```

---

## Implementation Strategy

### MVP scope

**Phases 1 → 2 → 3 (US1).** Users can be added to a team — which immediately unblocks 003's assignee
validation, making this the smallest increment with cross-feature value.

Realistically **US1 + US2** is the first usable increment; adding members you cannot see is thin.

### Incremental delivery

1. Setup + Foundational → unique constraint and the 23505 mapper tested
2. **US1** → teams can be staffed → validate (V1, V2, V7)
3. **US2** → the roster renders, and 003's assignee picker has a source (V4, V6)
4. **US3** → removal with the open-tasks invariant (V10–V12)
5. Polish → **T055** (the race) and **T056** (the cross-feature proof) are the two that matter

### Critical warnings

- **T009 must not create the `team_members` table** — it exists from 001's `InitialCreate`.
- **T029 needs both the pre-check and the 23505 catch.** The pre-check alone yields a **500** on the losing
  side of a genuine race; ADR-0003 forbids an expected outcome escaping as an exception.
- **T044's real assertion is that the blocked removal changed _nothing_** — not just that it returned 409.
  Check the row, the absence of an audit entry, **and** that no task was touched.
- **Do not add `xmin`, `If-Match`, or a paging envelope** to "match" the other features. All three absences
  are recorded decisions (research R-2, R-4); adding them would be the regression.

---

## Notes

- **[P]** = different files, no dependency on an incomplete task
- Tests are written before implementation within each story; verify they fail first
- Commit per task or logical group, Conventional Commits (Constitution VIII.5)
- Every checkpoint is a safe stopping point for validation or demo
