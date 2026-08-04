# Implementation Plan: 004 Team Management

**Branch**: `004-team` | **Date**: 2026-07-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/004-team/spec.md`

**Governed by**: Project Constitution **v1.3.0** · **Governance §5 gate**: spec 004 was revised against
v1.3.0 on 2026-07-29 — **the gate is satisfied and planning may proceed.**

> **This plan inherits, it does not re-derive.** Solution layout, MediatR pipeline, and contract tooling
> come from [001](../001-auth-rbac/plan.md); access-policy placement and the indexes-only migration pattern
> from [002](../002-projects/plan.md); the cross-feature boundary discipline from [003](../003-tasks/plan.md).
> The four repo-wide conventions are now recorded in
> **[ADR-0007](../../docs/adr/0007-implementation-conventions.md)** and cited rather than restated.

---

## Summary

Own **project team membership** — the record that a user is on a project's team. Three operations (add,
view roster, remove) and, importantly, a deliberately **minimal** authorization shape: membership is a
**link, not a role**. A `team_members` row grants nothing on its own; what a member may do is decided
entirely by their global role from 001.

**Technical approach.** Three vertical slices under `Features/Team/<UseCase>/` in the existing
`.Application` project. `TeamAccessPolicy` implements the shared-kernel `ITeamAccessPolicy` — **binary,
with no `ApplyScope`**, because every operation is pinned to one project by the route. This feature is
defined as much by what it omits: **no `xmin`** (the row has no mutable field; the `UNIQUE
(project_id, user_id)` constraint is the concurrency mechanism), **no `PagedResult<T>`** (the roster is
bounded, so VI.4's threshold never fires), **no graduated mutation model**, and **no role column**. Each
absence is a recorded decision, not an oversight. The one genuinely subtle piece is removal: it is
**blocked with 409** while the member has open assigned tasks, implemented by *reading* the shared `tasks`
entity and never mutating it.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10 · TypeScript strict / Angular 22 — **inherited, unchanged**

**Primary Dependencies**: **no new packages**

**Storage**: PostgreSQL 18. **No new table** — `team_members` exists from 001's `InitialCreate`. One
migration: **`AddTeamMemberIndexes`** (the unique constraint + two indexes).

**Testing**: xUnit + Testcontainers PostgreSQL + Respawn (ADR-0007 §2). Two suites carry the feature's
guarantees: the **concurrent duplicate-add race** (one 201, one 409, never a 500) and the **open-tasks
removal block**. Both are provider behaviour and unprovable on EF InMemory.

**Target Platform / Project Type**: unchanged — existing solution, extended in place

**Performance Goals**: roster is a single bounded read (one join, no paging); add/remove are single-row
writes; the unique index keeps membership lookups — including **003's assignee validation** — O(1)

**Constraints**: `project_id` from the route, never the body · project scope re-checked at write time ·
every add/remove audited in the same transaction · **004 never mutates `tasks`**

**Scale/Scope**: **3 endpoints, 3 user stories, 2 UI surfaces** (roster table, add-member dialog).
The simplest feature in the product.

---

## Constitution Check

*GATE: must pass before Phase 0. Re-checked after Phase 1.* Rows marked **↩** are satisfied by inherited
infrastructure.

| Principle | Gate | Status |
|---|---|---|
| **I.1 / I.2** Scope fidelity | Brief's Team Management module; per-project roles and bulk import explicitly out of scope | ✅ |
| **II.1** Three-tier separation | Angular presentation-only | ✅ ↩ |
| **II.2** Vertical slice, thin controllers, Clean Architecture | 3 slices in `Features/Team/<UseCase>/`; `TeamAccessPolicy` in `.Application` (002 R-1 pattern) | ✅ |
| **II.3** Resource-oriented URLs | Team nested under the parent project; verbs are HTTP verbs | ✅ |
| **II.4** Real-time not precluded | Nothing blocks a later roster-changed push | ✅ |
| **III** Stack locked | No new library | ✅ |
| **IV.1** DbContext, no raw SQL | Handlers use `IApplicationDbContext`; no raw SQL in 004 | ✅ |
| **IV.2** Migrations, descriptive names | `AddTeamMemberIndexes` — spec corrected 2026-07-31 | ✅ |
| **IV.3** FKs, navigations, explicit cascade | CASCADE on project and user, SET NULL on `added_by` — declared in 001, **proven by 004's tests** | ✅ |
| **IV.4** Audit every write | `TeamMemberAdded`/`Removed` in the same `SaveChangesAsync`; remove audits **before** deletion; **a blocked removal writes nothing** | ✅ |
| **IV.5** Idempotent seed | 004 adds no seed data of its own | ➖ |
| **V.1 / V.4** Auth by default, secrets | Inherited | ✅ ↩ |
| **V.2** Roles via attributes only | `[Authorize(Roles="Admin,ProjectManager")]` on writes; project scope in the handler | ✅ |
| **V.5** Validation at the boundary | FluentValidation (`userId` present); the active-user and already-member checks need the database and live in the handler | ✅ |
| **V.6** CORS | Inherited | ✅ ↩ |
| **VI.1 / VI.3 / VI.5** Base path, RFC 7807, Swagger dev | Inherited | ✅ ↩ |
| **VI.2** Status codes | 201+`Location`, 200, 204, 400, 401, 403, 404, 409 — nothing outside the declared set | ✅ |
| **VI.4** Pagination | **Does not apply.** The rule targets collections that "can exceed 50 items"; a project team is bounded. Plain array is **compliance, not an exception** (research R-4) | ✅ |
| **VI.6** Endpoint pattern | Team nested under the parent project, matching 002/003 | ✅ |
| **VII.1–VII.2** Lazy standalone `team` group | `loadChildren` → `team.routes.ts` | ✅ |
| **VII.3** HTTP in services | `TeamService`; DTO types generated from `team.v1.yaml` | ✅ |
| **VII.4 / VII.7** Interceptors, global errors | Inherited; the 409 surfaces the blocking-tasks message | ✅ ↩ |
| **VII.5** Guards only | Functional role guard; "Add member" hidden for TeamMember (UX only) | ✅ |
| **VII.6** Reactive Forms | Add-member dialog with an explicit validator | ✅ |
| **VII.8** Export service | **N/A** | ➖ |
| **VIII.1–VIII.5** Code quality | Inherited | ✅ ↩ |
| **IX.1** xUnit handlers + `WebApplicationFactory` | Every `ITeamAccessPolicy` branch; the three-role matrix; the race test | ✅ |
| **IX.2 / IX.3 / IX.4** Frontend tests, no merge on red, builders | Inherited; fixtures from builders (ADR-0007 §4) | ✅ ↩ |
| **X.2** API-first | `docs/contracts/team.v1.yaml` authored **before** any handler; drift proof in quickstart V15 | ✅ |
| **X.3** ADRs | ADR-0001..0007 apply; **no new ADR needed** — 004 introduces no new repo-wide convention | ✅ |
| **XI** Deployment | Unchanged | ✅ ↩ |
| **Governance §5** | **Satisfied** — 004 revised 2026-07-29 | ✅ |

**Gate result: PASS.** One new Complexity Tracking entry; earlier features' carry forward unchanged.

---

## Project Structure

### Documentation (this feature)

```text
specs/004-team/
├── plan.md · research.md · data-model.md · quickstart.md
├── contracts/README.md      # Pointer → docs/contracts/team.v1.yaml
├── checklists/requirements.md
└── tasks.md                 # Phase 2 — /speckit.tasks

docs/contracts/team.v1.yaml  # THE CONTRACT — 3 operations, authored before any handler
```

### Source Code — **delta only**

```text
src/
├── ProjectManagementApp.Domain/Entities/TeamMember.cs   # EXISTS (001, table-only) — 004 owns its rules
│
├── ProjectManagementApp.Application/
│   ├── Common/Authorization/TeamAccessPolicy.cs         # NEW — implements shared-contracts §3 (binary)
│   └── Features/Team/                                   # NEW — three vertical slices, spec B.3
│       ├── ListTeam/          Query   · Handler   → IReadOnlyList<TeamMemberDto>   (no envelope)
│       ├── AddTeamMember/     Command · Validator · Handler
│       └── RemoveTeamMember/  Command · Handler   (open-tasks 409 block)
│
├── ProjectManagementApp.Infrastructure/Persistence/
│   ├── Configurations/TeamMemberConfiguration.cs        # EXTENDED — unique constraint + two indexes
│   └── Migrations/…_AddTeamMemberIndexes.cs             # NEW
│
├── ProjectManagementApp.Api/Controllers/TeamController.cs   # NEW — three thin endpoints
│
└── ProjectManagementApp.Web/src/app/
    ├── core/services/team.service.ts                    # NEW
    └── features/team/                                   # NEW — lazy standalone route group
        ├── roster/ · add-member-dialog/
        └── team.routes.ts

tests/                                                   # extended in place, no new project
├── …Application.Tests/Features/Team/                     # handler branches
├── …Application.Tests/Authorization/TeamAccessPolicyTests.cs   # view-vs-manage matrix
├── …Infrastructure.Tests/Team/                           # UNIQUE race (SQLSTATE 23505), cascades, SET NULL
└── …Api.Tests/Team/                                      # three-role matrix, 409 paths
```

**Structure Decision.** No new assembly, no new test project, no new shared abstraction. The only notable
placement is `TeamAccessPolicy` in `.Application` (authorization rules are business rules — 002 R-1). The
`ListTeam` handler returns `IReadOnlyList<TeamMemberDto>` directly rather than `PagedResult<T>`, which is
the structural expression of research R-4.

---

## Complexity Tracking

> 001's four, 002's two, and 003's one justified deviation carry forward unchanged.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| **`team_members` carries no `xmin`**, departing from ADR-0007 §3's otherwise-universal `ETag`/`If-Match` rule | Optimistic concurrency prevents **lost updates**. A membership row has **no mutable field** — it is inserted or deleted, never edited — so there is no update to lose. Concurrency is instead guaranteed by the `UNIQUE (project_id, user_id)` constraint (one 201, one 409) and by removal being naturally idempotent. shared-contracts §5 was extended to name this exclusion category. | *Add `xmin` for uniformity* — dead weight on an immutable row, implying a concurrency story the entity does not have, and requiring an `If-Match` header that would protect nothing. *Application-level existence check alone* — a TOCTOU race that yields a 500 on the losing side of a genuine concurrent add. |

> **Not** listed as violations, because they are not: the plain-array roster is **compliant** with VI.4
> (the ">50 items" trigger does not fire), and the absence of `ApplyScope`/`TaskMutation` reflects
> capabilities this entity genuinely lacks rather than rules being skipped.

---

## Post-Design Re-check (after Phase 1)

- **The directive's two "deliberately not applied" claims were verified, not assumed** — and one had a
  real gap. `shared-contracts.md` §5 listed the `xmin` entities and excluded "append-only/system tables",
  but **`team_members` appeared in neither list**, leaving this feature's central design claim unbacked by
  the shared contract. §5 now names a second exclusion category (*join tables with no mutable field*) and
  states the unique-constraint mechanism. Caught only because the claim was checked against the file.
- **VI.4 confirmed as compliance rather than exception** — re-reading the rule's own wording ("can exceed
  50 items") showed no waiver is needed, so the plain array required no Complexity Tracking entry.
- **IV.4 sharpened** — designing the removal flow made explicit that a **blocked** removal writes *no*
  audit row: nothing happened, so nothing is recorded (spec B.7).
- **ADR-0006's boundary rule held under its most tempting violation** — the open-tasks block reads the
  shared `tasks` entity and never calls 003's handlers, and never mutates tasks. The "assignee is always a
  member" invariant emerges from each feature enforcing its own half.
- **No new violations** beyond the one recorded above.

**Gate result after design: PASS.**

---

## Follow-ups

1. ~~**shared-contracts §5 did not account for `team_members`.**~~ **✅ RESOLVED 2026-07-31 (in this plan).**
   §5 now names a second exclusion category — *join tables with no mutable field* — states the
   unique-constraint mechanism, and asks future features to declare which category a new entity falls in. A
   stale pre-v1.3.0 "the service converts" was corrected to "the handler converts" in the same edit.
2. **004 is the natural point to run the cross-feature integration test with 003** (quickstart V13:
   003 accepts an assignee **iff** a matching `team_members` row exists). It belongs to whichever of
   003/004 lands second; recorded here so it is not lost between them.
3. **No new ADR.** 004 introduces no repo-wide convention — the one notable decision (no `xmin` here) is an
   *application* of ADR-0004/0007's reasoning to an entity that falls outside it, now documented where it
   belongs: shared-contracts §5.

---

## Phase status

| Phase | Output | Status |
|---|---|---|
| Phase 0 — Outline & Research | `research.md` | ✅ 11 inherited by citation, 5 newly derived, 0 unresolved |
| Phase 1 — Design & Contracts | `data-model.md`, `docs/contracts/team.v1.yaml`, `quickstart.md` | ✅ Complete |
| Phase 2 — Task breakdown | `tasks.md` | ⏳ Run `/speckit.tasks` |
