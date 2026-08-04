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

- [ ] T001 Create the slice folder structure `src/ProjectManagementApp.Application/Features/Team/{ListTeam,AddTeamMember,RemoveTeamMember}/` per plan.md §Project Structure
- [ ] T002 [P] Generate TypeScript DTO types from `docs/contracts/team.v1.yaml` into `src/ProjectManagementApp.Web/src/app/core/api/generated/` via `openapi-generator` (types only)
- [ ] T003 [P] Add `team.v1.yaml` to the `CheckApiContract` MSBuild target in `src/ProjectManagementApp.Api/ProjectManagementApp.Api.csproj` (ADR-0007 §1)
- [ ] T004 [P] Add the `Team:*` configuration section to `src/ProjectManagementApp.Api/appsettings.json` — `AllowAddInactiveUser=false`, `AllowManageOnTerminalStatusProject=true`, `IncludeInactiveMembersInRoster=true`, `MaskOutOfScopeAs404=false` (spec B.4). **There is deliberately no remove-with-open-tasks toggle** — blocking is a fixed invariant (Clarifications 2026-07-22)
- [ ] T005 [P] Create `TeamOptions` binding class in `src/ProjectManagementApp.Api/Configuration/TeamOptions.cs` and register it in `Program.cs`
- [ ] T006 **Verify prerequisites before proceeding**: `TeamMember` entity and `team_members` table exist from `InitialCreate`; `ITeamAccessPolicy` is declared in `Application/Common/Interfaces/`; `AuditAction` contains `TeamMemberAdded` and `TeamMemberRemoved`; `Project.OwnerId` is available; the `tasks` table exists for the removal block. **Stop and fix the owning feature if any is missing** (ADR-0006 addendum)
- [ ] T007 [P] Scaffold the lazy `team` route group in `src/ProjectManagementApp.Web/src/app/features/team/team.routes.ts` and register it with `loadChildren` in `app.routes.ts` (standalone, ADR-0001)

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Persistence — the unique constraint is this feature's correctness guarantee

- [ ] T008 Extend `src/ProjectManagementApp.Infrastructure/Persistence/Configurations/TeamMemberConfiguration.cs` with **`UNIQUE (project_id, user_id)`** plus the `(project_id)` and `(user_id)` indexes (data-model.md §4). Confirm the entity has **no `updated_at` and no row-version property**
- [ ] T009 Create the `AddTeamMemberIndexes` migration in `src/ProjectManagementApp.Infrastructure/Persistence/Migrations/`. **Must NOT create the `team_members` table** — it exists from 001's `InitialCreate`. The unique index is added here rather than in 001 because it is *this feature's* correctness mechanism and only this feature inserts into the table (research R-2)
- [ ] T010 [P] Write schema tests in `tests/ProjectManagementApp.Infrastructure.Tests/Team/SchemaTests.cs` asserting: the unique index exists; **`team_members` has no role/permission column**; **no `updated_at` and no `xmin`**; and the three delete behaviours hold — `project_id` CASCADE, `user_id` CASCADE, `added_by` **SET NULL** (DoD 3, 6, 7)

### The binary access policy

- [ ] T011 Implement `TeamAccessPolicy : ITeamAccessPolicy` in `src/ProjectManagementApp.Application/Common/Authorization/TeamAccessPolicy.cs`, injecting `IApplicationDbContext` — **`CanViewTeamAsync`** (Admin any · PM if **owner _or_ member** · TM if member) and **`CanManageTeamAsync`** (Admin any · PM **owner only** · TM deny). **No `ApplyScope`** — every operation is pinned to one project by the route (research R-1). Lives in `.Application` (002 R-1 pattern)
- [ ] T012 [P] Write the view-vs-manage matrix test in `tests/ProjectManagementApp.Application.Tests/Authorization/TeamAccessPolicyTests.cs` — cover all six role × operation cells, and **specifically the ProjectManager-as-member case**: a PM who is a *member* of a project they do not own must pass `CanViewTeamAsync` and **fail** `CanManageTeamAsync`. That divergence is why the two methods cannot be one
- [ ] T013 Register `TeamAccessPolicy` as the `ITeamAccessPolicy` implementation in `src/ProjectManagementApp.Application/DependencyInjection.cs`

### Shared slice plumbing

- [ ] T014 [P] Create `TeamMemberDto` and its mapping extension in `src/ProjectManagementApp.Application/Features/Team/TeamMemberDto.cs`, matching the contract — `membershipId`, `userId`, `fullName`, `email`, **`role` as a read-only reflection of the member's _global_ role**, `isActive`, `addedAt`. **Read the role via `UserManager<ApplicationUser>.GetRolesAsync(user)`** — `IApplicationDbContext` (shared-contracts §7) exposes **no `Roles`/`UserRoles` `DbSet`**, so a join is not available. Batch the lookup for the roster rather than calling per row (analyze finding G2)
- [ ] T015 [P] Implement the **duplicate-violation mapper** in `src/ProjectManagementApp.Infrastructure/Persistence/UniqueViolationMapper.cs` — translate Npgsql `PostgresException` with `SqlState == "23505"` on the `team_members` unique index into `ErrorKind.Conflict`. **This, not the application pre-check, is what makes the concurrency guarantee true** (research R-3)
- [ ] T016 [P] Write unique-violation mapper tests in `tests/ProjectManagementApp.Infrastructure.Tests/Team/UniqueViolationMapperTests.cs` — a 23505 on the membership index maps to Conflict; unrelated exceptions propagate untouched
- [ ] T017 Create the thin `TeamController` shell in `src/ProjectManagementApp.Api/Controllers/TeamController.cs` with the three route stubs nested under `/api/projects/{projectId}/team` and their `[Authorize]` attributes — read permits all three roles, add/remove permit `Admin,ProjectManager`. **No logic; one `MediatR.Send` per endpoint**. **No `If-Match` parameter on any endpoint** (research R-2)

### Test fixtures & frontend shell

- [ ] T018 [P] Extend the fixture set in `tests/ProjectManagementApp.Application.Tests/Builders/` with `TeamMemberBuilder` and a **deactivated user** (`INACTIVE`), reusing 002's `PM`/`PM2` and 003's `TM`/`TM2`. From builders, never the production seeder (ADR-0007 §4)
- [ ] T019 [P] Implement `TeamService` in `src/ProjectManagementApp.Web/src/app/core/services/team.service.ts` — the three calls using generated DTO types. **No ETag handling** — this feature has none
- [ ] T020 [P] Create the roster and add-member-dialog component shells in `src/ProjectManagementApp.Web/src/app/features/team/{roster,add-member-dialog}/` with routes wired and a functional role guard in `team.routes.ts`

**Checkpoint**: Policy, unique constraint, and the 23505 mapper are tested. Stories can begin.

---

## Phase 3: User Story 1 — Add a user to a project's team (Priority: P0) 🎯 MVP

**Goal**: A ProjectManager (owner) or Admin adds **any active user** to a project's team; membership grants
visibility, never permission.

**Independent Test**: `POST /api/projects/{projectId}/team` → 201 + `Location`, exactly one row per
`(project, user)`, and a concurrent duplicate resolves to one 201 and one 409.

### Tests for User Story 1

- [ ] T021 [P] [US1] Write the add test in `tests/ProjectManagementApp.Api.Tests/Team/AddTeamMemberEndpointTests.cs` — **201**, `Location: /api/projects/{projectId}/team/{userId}`, `added_by` recorded as the caller, and a `TeamMemberAdded` audit row
- [ ] T022 [P] [US1] Write the 🎯 **eligibility test** in `tests/ProjectManagementApp.Api.Tests/Team/AddTeamMemberEligibilityTests.cs` — **any active user of any global role** is eligible: a TeamMember ✓, **a ProjectManager who does not own the project ✓**, an Admin ✓; a **deactivated** user → **400** (the only add-time gate, FR-016)
- [ ] T023 [P] [US1] Write authorization test in `tests/ProjectManagementApp.Api.Tests/Team/AddTeamMemberAuthorizationTests.cs` — a PM who does **not own** the project → **403**; a TeamMember → **403** at the role gate; Admin → succeeds on any project
- [ ] T024 [P] [US1] Write duplicate test in `tests/ProjectManagementApp.Api.Tests/Team/AddTeamMemberDuplicateTests.cs` — adding an existing member → **409**, exactly one row remains; unknown `projectId` or `userId` → **404**
- [ ] T025 [P] [US1] Write route-authority test in `tests/ProjectManagementApp.Api.Tests/Team/AddTeamMemberRouteAuthorityTests.cs` — a body `projectId` pointing elsewhere is **ignored**; the membership lands on the route's project
- [ ] T026 [P] [US1] Write handler branch tests in `tests/ProjectManagementApp.Application.Tests/Features/Team/AddTeamMemberCommandHandlerTests.cs` covering every branch and the audit write

### Implementation for User Story 1

- [ ] T027 [US1] Create `AddTeamMemberCommand` in `src/ProjectManagementApp.Application/Features/Team/AddTeamMember/AddTeamMemberCommand.cs` — `ProjectId` **from the route** plus `UserId`; the contract's `AddTeamMemberRequest` deliberately has no `projectId`
- [ ] T028 [US1] Implement `AddTeamMemberCommandValidator` in `src/ProjectManagementApp.Application/Features/Team/AddTeamMember/AddTeamMemberCommandValidator.cs` — `userId` present and well-formed
- [ ] T029 [US1] Implement `AddTeamMemberCommandHandler` in `src/ProjectManagementApp.Application/Features/Team/AddTeamMember/AddTeamMemberCommandHandler.cs` — load the project (404), `CanManageTeamAsync` (403), confirm the user exists (404) and **is active** (400), friendly not-already-a-member pre-check (409), insert + `TeamMemberAdded` audit in one `SaveChangesAsync`, **and wrap the save so a 23505 unique violation also maps to 409** (research R-3). **Both paths must return the same ProblemDetails body**
- [ ] T030 [US1] Wire `POST /api/projects/{projectId}/team` in `src/ProjectManagementApp.Api/Controllers/TeamController.cs` — `[Authorize(Roles="Admin,ProjectManager")]`, **201** + the composite `Location` header
- [ ] T031 [P] [US1] Build the add-member dialog in `src/ProjectManagementApp.Web/src/app/features/team/add-member-dialog/` — Material Reactive Form with a **searchable picker of any active user not already on the team, regardless of global role**; a single "user must be selected" validator; errors via the shared error-display component
- [ ] T032 [US1] Implement `TeamService.add()` and refresh the roster on success in `src/ProjectManagementApp.Web/src/app/core/services/team.service.ts`

**Checkpoint**: 🎯 **MVP** — teams can be staffed. Verify against quickstart V1, V2, V7.

---

## Phase 4: User Story 2 — List a project's team (Priority: P0)

**Goal**: Anyone connected to a project sees its full roster; the shown `role` is the member's **global**
role, never a per-project one.

**Independent Test**: The three-role visibility matrix holds, including the **PM-as-member** case, and the
response is a **plain array** with no paging envelope.

### Tests for User Story 2

- [ ] T033 [P] [US2] Write the 🎯 **visibility matrix test** in `tests/ProjectManagementApp.Api.Tests/Team/GetProjectTeamScopeTests.cs` — Admin **200** any project; owner PM **200**; **a PM who is a _member but not owner_ → 200**; a member TeamMember → **200 with the full roster** (not just themselves); a non-member → **403**; unknown project → **404**
- [ ] T034 [P] [US2] Write shape test in `tests/ProjectManagementApp.Api.Tests/Team/GetProjectTeamShapeTests.cs` — the body is a **plain JSON array**, **not** a `PagedResult` envelope, and accepts no `?page`/`?pageSize`; a project with no members returns **`200` with `[]`**, never 404 (research R-4, FR-012)
- [ ] T035 [P] [US2] Write display test in `tests/ProjectManagementApp.Api.Tests/Team/GetProjectTeamDisplayTests.cs` — each row carries the member's **global** role read-only; a **deactivated** member still appears, flagged (per `IncludeInactiveMembersInRoster`)
- [ ] T036 [P] [US2] Write handler test in `tests/ProjectManagementApp.Application.Tests/Features/Team/ListTeamQueryHandlerTests.cs` — `CanViewTeamAsync` runs **before** any row is returned; single bounded read with no N+1 on user data

### Implementation for User Story 2

- [ ] T037 [US2] Create `ListTeamQuery` in `src/ProjectManagementApp.Application/Features/Team/ListTeam/ListTeamQuery.cs` — `ProjectId` only
- [ ] T038 [US2] Implement `ListTeamQueryHandler` in `src/ProjectManagementApp.Application/Features/Team/ListTeam/ListTeamQueryHandler.cs` — load the project (404), `CanViewTeamAsync` (403), project `team_members` joined to `users` into `IReadOnlyList<TeamMemberDto>`. **Return the list directly — not `PagedResult<T>`** (research R-4)
- [ ] T039 [US2] Wire `GET /api/projects/{projectId}/team` in `src/ProjectManagementApp.Api/Controllers/TeamController.cs` — `[Authorize]` (all three roles)
- [ ] T040 [P] [US2] Build the roster table in `src/ProjectManagementApp.Web/src/app/features/team/roster/` — columns: name, email, **global role**, added-at, and a remove action rendered only for Admin/owner. **Client-side search over the bounded list** (no server paging). Explicit empty/loading/error/forbidden states; "Add member" hidden for TeamMember (UX only)
- [ ] T041 [US2] Implement `TeamService.list()` in `src/ProjectManagementApp.Web/src/app/core/services/team.service.ts`

**Checkpoint**: The roster 003's assignee picker consumes. Verify against quickstart V4, V6.

---

## Phase 5: User Story 3 — Remove a user from a project's team (Priority: P1)

**Goal**: Managers remove members; removal is **blocked with 409** while the member still has open assigned
tasks in that project.

**Independent Test**: Removal succeeds and revokes access immediately — but is refused with 409, changing
**nothing at all**, while an open assigned task exists.

### Tests for User Story 3

- [ ] T042 [P] [US3] Write the remove test in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberEndpointTests.cs` — **204**; the `TeamMemberRemoved` audit row written **before** deletion and retained; **no `If-Match` required**
- [ ] T043 [P] [US3] Write idempotency test in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberIdempotencyTests.cs` — a second remove → **404**; removing a non-member → **404**; concurrent removes → one 204 and one 404
- [ ] T044 [P] [US3] Write the 🎯 **open-tasks block test** in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberBlockedTests.cs` — with an `InProgress` task assigned to the member in that project: **409** with a dependency message naming the count, **and assert the no-op is total**: the `team_members` row still exists, **no `activity_logs` row was written**, and **the task was not modified** (spec B.7, research R-5)
- [ ] T045 [P] [US3] Write the `Done`-boundary test in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberDoneBoundaryTests.cs` — a member whose tasks are **all `Done`** *can* be removed (**204**); set the blocking task to `Done` and the same removal now succeeds; reassigning it away also unblocks
- [ ] T046 [P] [US3] Write the 🎯 **access-revocation test** in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberRevocationTests.cs` — after removal the user's next `GET /api/projects/{id}` returns **403** and the project disappears from their list. No grace period
- [ ] T047 [P] [US3] Write authorization test in `tests/ProjectManagementApp.Api.Tests/Team/RemoveTeamMemberAuthorizationTests.cs` — non-owner PM → **403**; TeamMember → **403**; owner/Admin removing **themselves** → permitted
- [ ] T048 [P] [US3] Write handler tests in `tests/ProjectManagementApp.Application.Tests/Features/Team/RemoveTeamMemberCommandHandlerTests.cs` — ordering: project load → policy → membership check → **open-tasks check** → audit → delete

### Implementation for User Story 3

- [ ] T049 [US3] Implement the **open-tasks check** in `src/ProjectManagementApp.Application/Features/Team/RemoveTeamMember/OpenAssignedTaskCheck.cs` — query `db.Tasks` for rows with this `project_id`, this `assignee_id`, and **`status != TaskStatus.Done`**, returning the blocking count. **Reads the shared `tasks` entity via `IApplicationDbContext`; never calls a 003 handler and never mutates a task** (research R-5, ADR-0006 addendum)
- [ ] T050 [US3] Create `RemoveTeamMemberCommand` in `src/ProjectManagementApp.Application/Features/Team/RemoveTeamMember/RemoveTeamMemberCommand.cs` — `ProjectId` and `UserId`, both from the route
- [ ] T051 [US3] Implement `RemoveTeamMemberCommandHandler` in `src/ProjectManagementApp.Application/Features/Team/RemoveTeamMember/RemoveTeamMemberCommandHandler.cs` — load the project (404), `CanManageTeamAsync` (403), confirm membership (404), run `OpenAssignedTaskCheck` → **`ErrorKind.Conflict` with the blocking count** if any, otherwise write `TeamMemberRemoved` **before** deleting, in one transaction. **A blocked removal writes nothing — no membership change and no audit row**
- [ ] T052 [US3] Wire `DELETE /api/projects/{projectId}/team/{userId}` in `src/ProjectManagementApp.Api/Controllers/TeamController.cs` returning **204**, **without** an `If-Match` requirement
- [ ] T053 [P] [US3] Add the remove action to the roster row in `src/ProjectManagementApp.Web/src/app/features/team/roster/` — confirmation dialog naming the member and warning they will lose access; on **409**, surface the blocking-tasks message and point the manager to reassign or close them first. Rendered only for Admin and the project owner
- [ ] T054 [US3] Implement `TeamService.remove()` and refresh the roster on success in `src/ProjectManagementApp.Web/src/app/core/services/team.service.ts`

**Checkpoint**: All three stories complete. Verify against quickstart V10–V12.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T055 🎯 **Write the concurrent duplicate-add race test** in `tests/ProjectManagementApp.Infrastructure.Tests/Team/ConcurrentAddRaceTests.cs` — fire two simultaneous adds of the same `(project, user)`; assert **exactly one 201 and one 409, never a 500**, and exactly one row. This is the assertion that separates a real guarantee from a TOCTOU pre-check, and it is **meaningless on EF InMemory** (quickstart V8, DoD 4/6)
- [ ] T056 🎯 **Write the cross-feature integration test** in `tests/ProjectManagementApp.Api.Tests/CrossFeature/TeamBacksTaskAssignmentTests.cs` — 003 accepts an assignee **iff** a matching `team_members` row exists: assign before adding → **400**; add the member → **201**; retry the assignment → **200**. This proves the pool 004 maintains is exactly the pool 003 validates against, **with neither feature calling the other's handlers** (DoD 8, closing 004 plan Follow-up 2)
- [ ] T057 **Prove the contract gate fails**: temporarily add a `role` property to `AddTeamMemberRequest`, run `dotnet build -p:CheckApiContract=true`, confirm the build **fails**, then revert. That specific drift is worth rehearsing — a per-project role field would quietly introduce the second permission system this feature exists to avoid (quickstart V15)
- [ ] T058 Execute the full quickstart validation **V1–V16** in `specs/004-team/quickstart.md` and record results
- [ ] T059 [P] Verify the roster read is a **single bounded query** with no N+1 on member user data, and that membership lookups (including 003's assignee validation) use the unique index (NFR-002)
- [ ] T060 [P] Audit Serilog output for the team endpoints — authorization denials logged with actor, project id, and reason (NFR-003)
- [ ] T061 [P] Add XML doc comments to `TeamController`, the three handlers, and `TeamAccessPolicy` (Constitution VIII.3)
- [ ] T062 [P] Regenerate the ERD in `docs/erd.md` after `AddTeamMemberIndexes` (Constitution X.4)
- [ ] T063 [P] Update the root `README.md` with the team module — the three endpoints, configuration keys, and a note that **membership is a link, not a role**
- [ ] T064 Remove commented-out code and any `console.log` across `src/ProjectManagementApp.Web/src/app/features/team/` and the 004 backend slices (Constitution VIII.4)
- [ ] T065 Run a security review against spec 004 §Security Rules — attribute-only role gates, project scope in the handler, `project_id` from the route, uniqueness enforced at the database, every add/remove audited

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
