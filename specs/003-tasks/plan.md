# Implementation Plan: 003 Task Management

**Branch**: `003-tasks` | **Date**: 2026-07-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/003-tasks/spec.md`

**Governed by**: Project Constitution **v1.3.0** · **Governance §5 gate**: spec 003 was revised against
v1.3.0 on 2026-07-29 — **the gate is satisfied and planning may proceed.**

> **This plan inherits, it does not re-derive.** Solution layout, MediatR pipeline, contract tooling, test
> strategy, and `Result` → HTTP mapping come from [001's plan](../001-auth-rbac/plan.md); the access-policy
> placement, `ETag`/`If-Match` concurrency transport, search mechanism, and indexes-only migration pattern
> come from [002's plan](../002-projects/plan.md). See [research §A](research.md) for the inheritance table.

---

## Summary

Deliver task lifecycle management within a project, and — the reason this feature is High complexity —
the **graduated authorization model**: the first place in the product where a user who passes the scope
gate is still permitted only *part* of a write. A TeamMember assigned to a task may change its **status**
and nothing else.

**Technical approach.** Seven vertical slices under `Features/Tasks/<UseCase>/` in the existing
`.Application` project. Authorization resolves through the **shared-kernel `ITaskAccessPolicy`**
(confirmed present in `docs/shared-contracts.md` §3 — see research R-1), implemented as
`TaskAccessPolicy` in `.Application`, whose `CanMutateAsync` switches on a `TaskMutation` value to produce
the 5 × 3 matrix in one place. That rule is enforced **twice, deliberately**: narrow endpoints bind narrow
commands so a widened payload is structurally inert, and the policy is consulted regardless. `closed_at`
is a derived side effect of crossing the `Done` boundary and is never bindable, because 006 Reports
depends on it being untamperable. One handler serves both list routes so their scoping can never drift.
The `tasks` table already exists from 001's `InitialCreate`, so 003's only migration adds indexes.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10 · TypeScript strict / Angular 22 — **inherited, unchanged**

**Primary Dependencies**: **no new packages**. `pg_trgm` is already enabled by 002's migration.

**Storage**: PostgreSQL 18. **No new table** — `tasks` (including `closed_at`) exists from 001's
`InitialCreate`. One migration: **`AddTaskIndexes`** (six indexes).

**Testing**: xUnit + Testcontainers PostgreSQL + Respawn. Three suites are non-negotiable: the
**15-cell `TaskMutation` × role matrix**, the **graduated 403/200 pair on one row**, and the **`xmin`
concurrency path across three PUTs**.

**Target Platform / Project Type**: unchanged — existing solution, extended in place

**Performance Goals**: single round trip per list with scope + filters + paging in SQL; no N+1 on project
or assignee. **Tasks is the highest-volume table in the product**, so this is where NFR-005 is genuinely
exercised.

**Constraints**: scope in the query, never in memory · `project_id` from the route and immutable ·
mutation kind re-checked at write time · every write audited in the same transaction · `closed_at`
server-derived only

**Scale/Scope**: **8 endpoints, 7 user stories, 4 UI screens** (List, Create, Detail, Edit) plus the
inline status control.

---

## Constitution Check

*GATE: must pass before Phase 0. Re-checked after Phase 1.* Rows marked **↩** are satisfied by inherited
infrastructure and not re-argued.

| Principle | Gate | Status |
|---|---|---|
| **I.1 / I.2** Scope fidelity | Brief's Task Management module; sub-tasks/dependencies/comments/time-tracking out of scope | ✅ |
| **II.1** Three-tier separation | Angular presentation-only | ✅ ↩ |
| **II.2** Vertical slice, thin controllers, Clean Architecture | 7 slices in `Features/Tasks/<UseCase>/`; `TaskAccessPolicy` in `.Application` (002 R-1 pattern) | ✅ |
| **II.3** Nouns, not verbs | Sub-resources are `/status` and `/assignee` — **nouns** | ✅ |
| **II.4** Real-time not precluded | Nothing blocks a later task-updated push | ✅ |
| **III** Stack locked | No new library | ✅ |
| **IV.1** DbContext, no raw SQL | Handlers use `IApplicationDbContext`; no raw SQL at all in 003 (002 already enabled `pg_trgm`) | ✅ |
| **IV.2** Migrations, descriptive names | `AddTaskIndexes` — see Follow-up 2 re: spec B.1's stale name | ✅ |
| **IV.3** FKs, navigations, explicit cascade | `Project`/`Assignee` navigations; CASCADE from project, RESTRICT on assignee — proven by 003's tests | ✅ |
| **IV.4** Audit every write | Five audit actions, each committed in the same `SaveChangesAsync`; delete audits before removal; **no separate `closed_at` event** | ✅ |
| **IV.5** Idempotent seed | 003 completes IV.5's "demo projects **with tasks**" — 002 supplied the projects half | ✅ |
| **V.1 / V.4** Auth by default, secrets | Inherited | ✅ ↩ |
| **V.2** Roles via attributes only | `[Authorize(Roles=…)]` on create/edit/reassign/delete; status permits all three. **The graduated rule lives in the policy, never as an in-body role check** | ✅ |
| **V.5** Validation at the boundary | FluentValidation via the inherited behavior; database-dependent rules (assignee pool, due-date window) in the handler | ✅ |
| **V.6** CORS | Inherited | ✅ ↩ |
| **VI.1 / VI.3 / VI.5** Base path, RFC 7807, Swagger dev | Inherited | ✅ ↩ |
| **VI.2** Status codes | 201+`Location`, 200, 204, 400, 401, 403, 404, 409 — nothing outside the declared set | ✅ |
| **VI.4** Pagination | Both collection endpoints return `PagedResult<T>`, default 20 / max 100, **clamped** | ✅ |
| **VI.6** Endpoint pattern | Creation and project-scoped listing nested under the parent; single-task ops flat under `/api/tasks/{id}` | ✅ |
| **VII.1–VII.2** Lazy standalone `tasks` group | `loadChildren` → `tasks.routes.ts` | ✅ |
| **VII.3** HTTP in services | `TasksService`; DTO types generated from `tasks.v1.yaml` | ✅ |
| **VII.4 / VII.7** Interceptors, global errors | Inherited | ✅ ↩ |
| **VII.5** Guards only | Functional role guard; **for a TeamMember only the status control is enabled** — UX only, API re-checks | ✅ |
| **VII.6** Reactive Forms | Create/Edit with explicit validators incl. due-date window | ✅ |
| **VII.8** Export service | **N/A** | ➖ |
| **VIII.1–VIII.5** Code quality | Inherited | ✅ ↩ |
| **IX.1** xUnit handlers + `WebApplicationFactory` | 15-cell matrix table-driven; controller happy + error paths | ✅ |
| **IX.2 / IX.3 / IX.4** Frontend tests, no merge on red, builders | Inherited CI gate | ✅ ↩ |
| **X.2** API-first | `docs/contracts/tasks.v1.yaml` authored **before** any handler; drift proof in quickstart V16 | ✅ |
| **X.3** ADRs | ADR-0001..0006 apply; the `ETag` convention remains an ADR-0007 candidate (002 Follow-up 2) | ⚠️ |
| **XI** Deployment | Unchanged | ✅ ↩ |
| **Governance §5** | **Satisfied** — 003 revised 2026-07-29 | ✅ |

**Gate result: PASS.** One new Complexity Tracking entry; 001's and 002's carry forward unchanged.

---

## Project Structure

### Documentation (this feature)

```text
specs/003-tasks/
├── plan.md · research.md · data-model.md · quickstart.md
├── contracts/README.md      # Pointer → docs/contracts/tasks.v1.yaml
├── checklists/requirements.md
└── tasks.md                 # Phase 2 — /speckit.tasks

docs/contracts/tasks.v1.yaml # THE CONTRACT — 8 operations, authored before any handler
```

### Source Code — **delta only**

```text
src/
├── ProjectManagementApp.Domain/Entities/TaskItem.cs   # EXISTS (001, table-only) — 003 owns its rules
│
├── ProjectManagementApp.Application/
│   ├── Common/
│   │   ├── Models/TaskMutation.cs                     # ⚠️ MUST BE CREATED IN 001 — see Follow-up 1
│   │   └── Authorization/TaskAccessPolicy.cs          # NEW — implements shared-contracts §3
│   └── Features/Tasks/                                # NEW — seven vertical slices, spec B.3
│       ├── CreateTask/       Command · Validator · Handler · TaskDetailDto
│       ├── ListTasks/        Query   · Handler       ← serves BOTH list routes (research R-4)
│       ├── GetTaskById/      Query   · Handler
│       ├── UpdateTask/       Command · Validator · Handler        (FullEdit)
│       ├── UpdateTaskStatus/ Command · Handler                    (StatusChange + closed_at)
│       ├── ReassignTask/     Command · Handler                    (Reassign)
│       └── DeleteTask/       Command · Handler                    (Delete)
│
├── ProjectManagementApp.Infrastructure/Persistence/
│   ├── Configurations/TaskItemConfiguration.cs        # EXTENDED — six indexes
│   └── Migrations/…_AddTaskIndexes.cs                 # NEW
│
├── ProjectManagementApp.Api/Controllers/TasksController.cs   # NEW — eight thin endpoints
│
└── ProjectManagementApp.Web/src/app/
    ├── core/services/tasks.service.ts                 # NEW
    └── features/tasks/                                # NEW — lazy standalone route group
        ├── list/ · detail/ · create/ · edit/
        └── tasks.routes.ts

tests/                                                 # extended in place, no new project
├── …Application.Tests/Features/Tasks/                 # handler branches
├── …Application.Tests/Authorization/TaskAccessPolicyTests.cs   # 15-cell matrix, table-driven
├── …Infrastructure.Tests/Tasks/                       # indexes, cascade/RESTRICT, xmin, closed_at
└── …Api.Tests/Tasks/                                  # scope matrix, graduated 403/200 pair, ETag
```

**Structure Decision.** No new assembly, no new test project. Two placements carry weight:
`TaskAccessPolicy` in `.Application` (authorization rules are business rules — 002 R-1), and
**`TaskMutation` in `.Application/Common/Models/`** beside `AccessDecision`, since it is authorization
vocabulary rather than domain state — but it **must be created in 001**, because 001 authors the
shared-kernel interface that references it.

---

## Complexity Tracking

> 001's four and 002's two justified deviations carry forward unchanged and are not restated.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| **Four write endpoints for one entity** (`PUT /{id}`, `/status`, `/assignee`, `DELETE`) where REST convention suggests one `PUT` | The endpoint shapes **are** the authorization design. A narrow request body makes privilege escalation *structurally* impossible, not merely rejected — it backs up `CanMutateAsync` so the rule survives even a mass-assignment mistake. Spec T.2 calls this "belt-and-braces" and requires both halves. | *A single `PUT` with server-side field diffing* — reintroduces the mass-assignment surface these routes exist to remove, and turns "which fields did you change?" into a runtime question rather than a routing one. *Policy check alone* — leaves the rule implicit in DTO shape, invisible to a reader and silently lost if someone widens the DTO later. |

---

## Post-Design Re-check (after Phase 1)

- **The directive's confirmation was worth performing.** `ITaskAccessPolicy` does resolve in
  shared-contracts §3 — but verifying it surfaced that its signature references `TaskMutation`, **which 001
  never creates**. The shared-kernel interface would not compile at the end of 001's Foundational phase.
  Caught only because the check was done against the file rather than assumed (research R-1).
- **II.2 holds under the widest endpoint surface in the product** — eight routes, each still a single
  `MediatR.Send`, with all seven mutation decisions resolved in one policy method.
- **IV.4 tightened** — designing the audit catalog confirmed the `closed_at` transition rides on
  `TaskStatusChanged`'s `change_summary` rather than becoming a sixth audit action, as spec B.7 requires.
- **VI.4 discipline carried over** — `data-model.md` §5 pins the same scope → filter → **count** → sort →
  page order 002 fixed, because the identical `totalCount` leak is possible here.
- **No new violations** beyond the one recorded above.

**Gate result after design: PASS.**

---

## Follow-ups

1. ~~⚠️ **`TaskMutation` must be created in 001, not 003.**~~ **✅ RESOLVED 2026-07-31.** Fixed at all three
   levels so it cannot recur:
   - **001's T020** now creates `TaskMutation` in `Application/Common/Models/` alongside `AccessDecision`,
     with an inline note explaining why it cannot be deferred.
   - **001's data-model §3** lists it, with the placement rationale.
   - **`docs/shared-contracts.md` §3** no longer calls it "the enum owned by 003" — that phrasing was the
     root cause. It now states the *values* are 003's vocabulary but the **type belongs to the shared
     kernel and must be created in 001**, with the interfaces that reference it.
2. ~~**Spec 003 B.1 names a migration that must not be created.**~~ **✅ RESOLVED 2026-07-31.** Corrected in
   one sweep across all three affected specs, as recommended: 002 → **`AddProjectIndexes`**, 003 →
   **`AddTaskIndexes`**, 004 → **`AddTeamMemberIndexes`** (six occurrences total), each with a note that the
   table itself comes from 001's `InitialCreate`.
3. ~~**ADR-0007 candidate grows.**~~ **✅ RESOLVED 2026-07-31.** Written as
   **[`docs/adr/0007-implementation-conventions.md`](../../docs/adr/0007-implementation-conventions.md)**,
   covering all four repo-wide conventions in one file: contract drift gate, Testcontainers-only test
   database, `ETag`/`If-Match` transport (including 003's `DELETE` exemption), and builder-based fixtures.
4. ~~**Fixtures need a second TeamMember (`TM2`).**~~ **✅ RESOLVED 2026-07-31.** Recorded as ADR-0007 §4:
   test fixtures come from builders under `tests/**/Builders/`, never from the production seeder. 001's
   one-user-per-role seed contract is unchanged and stays a product concern.

---

## Phase status

| Phase | Output | Status |
|---|---|---|
| Phase 0 — Outline & Research | `research.md` | ✅ 12 inherited by citation, 7 newly derived, 0 unresolved |
| Phase 1 — Design & Contracts | `data-model.md`, `docs/contracts/tasks.v1.yaml`, `quickstart.md` | ✅ Complete |
| Phase 2 — Task breakdown | `tasks.md` | ⏳ Run `/speckit.tasks` |
