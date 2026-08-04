# Implementation Plan: 002 Project Management

**Branch**: `002-projects` | **Date**: 2026-07-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-projects/spec.md`

**Governed by**: Project Constitution **v1.3.0** · **Governance §5 gate**: spec 002 was revised against
v1.3.0 on 2026-07-29 (see its `**Revised**` header line) — **the gate is satisfied and planning may
proceed.**

> **This plan inherits, it does not re-derive.** The solution layout, MediatR pipeline behaviors, OpenAPI
> contract-authoring and drift-check approach, test strategy, and `Result` → HTTP mapping are fixed by
> [001's plan](../001-auth-rbac/plan.md) and are **reused unchanged**. Only what is genuinely new to 002 is
> designed here — see [research.md §A](research.md) for the inheritance table and §B for the six new
> decisions.

---

## Summary

Deliver full project lifecycle management (create, list/search, view, edit, delete) and — the part that
actually matters — establish the **two-layer authorization model** every later feature copies: a coarse
**role gate** declared by `[Authorize(Roles=…)]` on a thin controller, plus a fine-grained **ownership /
assignment scope gate** applied inside the slice handler via the shared-kernel `IProjectAccessPolicy`.

**Technical approach.** Five vertical slices under `Features/Projects/<UseCase>/` inside the **existing**
`.Application` project — no new assembly. `ProjectAccessPolicy` is implemented in `.Application` (not
Infrastructure) because `IApplicationDbContext` is now a shared-kernel abstraction, so authorization rules
stay in the layer that owns business logic. Scope is **composed into the `IQueryable` before counting and
before paging**, so out-of-scope projects are never loaded, counted, or leaked through paging metadata.
002 is the first feature to genuinely contend on `xmin`: the row version travels as an `ETag` on GET and a
**required** `If-Match` on PUT, so a stale write returns 409 and a *missing* header returns 400 rather than
degrading silently to last-write-wins. The `projects` table already exists from 001's `InitialCreate`, so
002's only migration adds indexes — including a `pg_trgm` GIN index, without which substring search cannot
use an index at all.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10, nullable + warnings-as-errors · TypeScript strict / Angular 22 —
**inherited from 001, unchanged**

**Primary Dependencies**: no new packages. MediatR, FluentValidation, EF Core 10 + Npgsql, Serilog,
Swashbuckle, Angular Material, NgRx — all already present. The only new *database* dependency is the
**`pg_trgm` extension**, enabled by 002's migration ([research R-3](research.md)).

**Storage**: PostgreSQL 18. **No new table** — `projects` exists from 001's `InitialCreate`
([research R-4](research.md)). One migration, `AddProjectIndexes`.

**Testing**: xUnit + **Testcontainers PostgreSQL** + Respawn (001 R-7). Two suites are non-negotiable here
and meaningless without real PostgreSQL: the **three-role scope matrix** (proves the predicate reaches SQL)
and the **`xmin` concurrency test** (409 on stale write).

**Target Platform**: unchanged — IIS, same-origin API + SPA (ADR-0002)

**Project Type**: Web application — **existing** solution, extended in place

**Performance Goals**: list is a **single round trip** with scope + filter + sort + paging translated to
SQL; no N+1 on owner (projection or `Include`); indexes lead with `owner_id`/`status` (NFR-002, NFR-005)

**Constraints**: scope enforced **in the query**, never in memory (FR-007) · ownership derived from the
token, never the body (FR-003) · mutation re-checked at write time (FR-010) · every write audited in the
same transaction (FR-012)

**Scale/Scope**: expected to exceed 50 projects, so pagination is mandatory from day one (VI.4). This
feature: **5 endpoints, 5 user stories, 4 UI screens** (List, Create, Detail, Edit).

---

## Constitution Check

*GATE: must pass before Phase 0. Re-checked after Phase 1 — see §Post-Design Re-check.*

Rows marked **↩ 001** are satisfied by inherited infrastructure and are not re-argued here.

| Principle | Gate | Status |
|---|---|---|
| **I.1 / I.2** Scope fidelity, no bonus features | Implements the brief's Project Management module; templates/cloning/archive/Gantt explicitly out of scope | ✅ |
| **II.1** Three-tier separation | Angular presentation-only; all authorization server-side | ✅ ↩ 001 |
| **II.2** Vertical slice, thin controllers, Clean Architecture | 5 slices in `Features/Projects/<UseCase>/`; controller is one `Send()`; **`ProjectAccessPolicy` in `.Application`** (research R-1) | ✅ |
| **II.3** Resource-oriented URLs | `/api/projects`, plural noun, HTTP verbs | ✅ |
| **II.4** Real-time not precluded | Nothing blocks a later SignalR project-updated push | ✅ |
| **III** Stack locked | **No new library.** `pg_trgm` is a PostgreSQL extension, not a library swap | ✅ |
| **IV.1** DbContext, no raw SQL | Handlers use `IApplicationDbContext` directly (shared-contracts §7). The one raw statement is `CREATE EXTENSION` **DDL in a migration** — not data access, so not the prohibited case | ✅ |
| **IV.2** Migrations, descriptive names | `AddProjectIndexes` — and see Follow-up 1 re: spec B.1's stale name | ✅ |
| **IV.3** FKs, navigations, explicit cascade | `Project.Owner`, `.Tasks`, `.TeamMembers` navigations; CASCADE/RESTRICT declared in 001, **proven by 002's tests** (research R-5) | ✅ |
| **IV.4** Audit every write | Create/Update/Delete/OwnerChange each write `activity_logs` in the **same** `SaveChangesAsync`; delete audits **before** removal | ✅ |
| **IV.5** Idempotent seed | 002 adds demo projects to 001's existing seeder | ✅ |
| **V.1** Authenticated by default | Inherited global fallback policy | ✅ ↩ 001 |
| **V.2** Roles via attributes only | `[Authorize(Roles="Admin,ProjectManager")]` on writes; **scope logic lives in the handler, never an in-body role check** | ✅ |
| **V.3 / V.4** Passwords, secrets | Not touched by this feature | ➖ ↩ 001 |
| **V.5** Validation at the boundary | FluentValidation via the inherited `ValidationBehavior`, incl. the cross-field date-order rule (ADR-0005) | ✅ |
| **V.6** CORS allow-list | Inherited | ✅ ↩ 001 |
| **VI.1** `/api` base, versionable | Contract `servers: /api` | ✅ |
| **VI.2** Status codes | 201+`Location`, 200, 204, 400, 401, 403, 404, **409**; no code outside the declared set (research R-2 chose 400 over 428 for this reason) | ✅ |
| **VI.3** RFC 7807 | Single inherited mapper | ✅ ↩ 001 |
| **VI.4** Pagination | **First feature where this applies.** `PagedResult<T>`, `?page`/`?pageSize`, default 20, max 100, **clamped not rejected**; `totalCount` scoped to the caller | ✅ |
| **VI.5** Swagger UI in development | Inherited; exploration only, not the contract | ✅ ↩ 001 |
| **VI.6** Exact brief endpoints | The five named routes exist **exactly** as written | ✅ |
| **VII.1–VII.2** Lazy standalone `projects` group | `loadChildren` → `projects.routes.ts`, no `@NgModule` | ✅ |
| **VII.3** HTTP in services, contract-generatable | `ProjectsService`; DTO types generated from `projects.v1.yaml` | ✅ |
| **VII.4** Interceptors | Inherited JWT + 401 interceptors | ✅ ↩ 001 |
| **VII.5** Guards only | Functional role guard on the `projects` routes; **no component-level redirects** | ✅ |
| **VII.6** Reactive Forms + shared error display | Create/Edit forms with explicit validators incl. date-order | ✅ |
| **VII.7** Global error handling | Inherited; 409 surfaces a reload-and-reapply prompt | ✅ ↩ 001 |
| **VII.8** Export service | **N/A** — no reports in 002 | ➖ |
| **VIII.1–VIII.5** Code quality | Inherited `Directory.Build.props`, `.editorconfig`, Conventional Commits | ✅ ↩ 001 |
| **IX.1** xUnit on handlers + `WebApplicationFactory` | Every `ProjectAccessPolicy` and handler branch; scope matrix is table-driven | ✅ |
| **IX.2** Jasmine + Karma | `ProjectsService`, role guard, form validators | ✅ |
| **IX.3 / IX.4** No merge on red; builders | Inherited CI gate and test-data builders | ✅ ↩ 001 |
| **X.2** API-first | `docs/contracts/projects.v1.yaml` authored **before** any handler; same CI diff gate; proof step in quickstart V15 | ✅ |
| **X.3** ADRs | ADR-0001..0006 apply. **Research R-2 (ETag/`If-Match`) is a repo-wide convention 003–006 will inherit — ADR candidate, see Follow-up 2** | ⚠️ |
| **X.1 / X.4 / X.5** README, ERD, demo | ERD regenerated after `AddProjectIndexes`; README/demo at delivery | ✅ |
| **XI.1–XI.4** Deployment | Unchanged | ✅ ↩ 001 |
| **Governance §5** Spec revision gate | **Satisfied** — 002 revised 2026-07-29 against v1.3.0 | ✅ |

**Gate result: PASS.** No new Complexity Tracking entries beyond those 001 already justified, plus the two
recorded below.

---

## Project Structure

### Documentation (this feature)

```text
specs/002-projects/
├── plan.md              # This file
├── research.md          # Phase 0 — §A inherited (10 cited), §B new (6 derived)
├── data-model.md        # Phase 1 — entity rules, scope predicates, index migration
├── quickstart.md        # Phase 1 — 16 validation scenarios mapped to DoD
├── contracts/README.md  # Pointer → docs/contracts/projects.v1.yaml
├── checklists/
│   └── requirements.md  # Pre-existing spec-quality checklist
└── tasks.md             # Phase 2 — created by /speckit.tasks

docs/contracts/projects.v1.yaml   # THE CONTRACT — authored before any handler
```

### Source Code — **delta only** (the solution scaffold is 001's, unchanged)

```text
src/
├── ProjectManagementApp.Domain/
│   └── Entities/Project.cs                       # EXISTS (001, table-only) — 002 owns its rules
│
├── ProjectManagementApp.Application/             # ← no new project; extended in place
│   ├── Common/
│   │   └── Authorization/
│   │       └── ProjectAccessPolicy.cs            # NEW — implements shared-contracts §3,
│   │                                             #       injects IApplicationDbContext (§7). research R-1
│   └── Features/Projects/                        # NEW — five vertical slices, spec B.3
│       ├── CreateProject/    Command · Validator · Handler · ProjectDetailDto
│       ├── ListProjects/     Query   · Handler   → PagedResult<ProjectSummaryDto>
│       ├── GetProjectById/   Query   · Handler
│       ├── UpdateProject/    Command · Validator · Handler
│       └── DeleteProject/    Command · Handler
│
├── ProjectManagementApp.Infrastructure/
│   └── Persistence/
│       ├── Configurations/ProjectConfiguration.cs  # EXTENDED — add the four indexes
│       └── Migrations/…_AddProjectIndexes.cs       # NEW — indexes + CREATE EXTENSION pg_trgm
│
├── ProjectManagementApp.Api/
│   ├── Controllers/ProjectsController.cs         # NEW — five thin endpoints, one Send() each
│   └── Common/ETagExtensions.cs                  # NEW — read If-Match, write ETag (research R-2)
│
└── ProjectManagementApp.Web/src/app/
    ├── core/services/projects.service.ts         # NEW
    └── features/projects/                        # NEW — lazy standalone route group
        ├── list/ · detail/ · create/ · edit/
        └── projects.routes.ts

tests/                                            # ← no new test project; extended in place
├── ProjectManagementApp.Application.Tests/Features/Projects/     # handler + policy branches
├── ProjectManagementApp.Application.Tests/Authorization/         # scope matrix (table-driven)
├── ProjectManagementApp.Infrastructure.Tests/Projects/           # indexes, cascade/RESTRICT, xmin
└── ProjectManagementApp.Api.Tests/Projects/                      # scope matrix over HTTP, ETag flow
```

**Structure Decision.** 002 adds **no assembly and no test project** — it extends 001's solution in place,
which is what "the foundational plan" buying us reuse actually means in practice. The one structural
judgement is placing `ProjectAccessPolicy` in `.Application` rather than `.Infrastructure`
([research R-1](research.md)): scope rules are business rules (II.2), and they no longer need to live near
the database because `IApplicationDbContext` is a shared-kernel Application abstraction
([shared-contracts §7](../../docs/shared-contracts.md)).

---

## Complexity Tracking

> 001's four justified deviations (Domain's Identity package reference, Application referencing EF Core,
> CI-only contract diff, project count) **carry forward unchanged** and are not restated.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| **`CREATE EXTENSION pg_trgm` — a raw SQL statement in a migration**, where IV.1 prohibits raw SQL | IV.1's prohibition targets **data access** ("raw SQL is PROHIBITED except for reporting queries"). Enabling a database extension is **DDL in a migration**, the same category as `CREATE INDEX` — it bypasses no DbContext query path. Substring search cannot use an index without it. | *Omit the extension and B-tree the column* — the index would be **unusable** for `%term%`, silently degrading every search to a sequential scan while appearing indexed. *Full-text `tsvector`* — wrong semantics (word-boundary, stemmed): `pollo` would not match "Apollo". |
| **A required `If-Match` header adds a 400 path the spec's story text does not enumerate** | ADR-0004 forbids silent last-write-wins. A concurrency token the client may simply omit is optional in practice, so the guarantee would be advisory. Failing loudly is the only way FR-017 actually holds. | *Row version as a body field* — omittable, therefore silently degrades to last-write-wins; and it would have to be added to **four** write DTOs in 003. *428 Precondition Required* — semantically better, rejected only to avoid introducing a status code outside VI.2's declared set. |

---

## Post-Design Re-check (after Phase 1)

- **VI.4 verified in the design, not just asserted** — writing `data-model.md` §5 forced the query
  composition order to be pinned: scope → filter → **count** → sort → page. Counting before scoping, or
  paging before counting, leaks out-of-scope existence through `totalCount`. That ordering is now a
  testable requirement (quickstart V4 asserts counts, not just items).
- **II.2 strengthened** — designing `ProjectAccessPolicy` surfaced that it needs no Infrastructure
  dependency at all, which is a direct consequence of the shared-kernel §7 decision made during 001's
  follow-up. Authorization rules stay in Application.
- **IV.2 tension surfaced and resolved** — Phase 1 revealed that spec 002 B.1 names a migration
  (`AddProjectsTable`) that must not exist, since 001 already creates the table. Resolved in research R-4;
  recorded as Follow-up 1 rather than silently diverging.
- **IV.1 re-examined, not waved through** — the `CREATE EXTENSION` statement was checked against IV.1's
  actual wording rather than assumed benign; it falls outside the prohibition and is recorded in Complexity
  Tracking anyway so a reviewer sees the reasoning.
- **No new violations.**

**Gate result after design: PASS.**

---

## Follow-ups (not blockers for `/speckit.tasks`)

1. ~~**Spec 002 B.1 names a migration that must not be created.**~~ **✅ RESOLVED 2026-07-31.** Both
   occurrences in spec 002 (the B.1 header and the Consolidated Data Model's "e.g.") now read
   **`AddProjectIndexes`**, with an explicit note that the `projects` table comes from 001's
   `InitialCreate`. The same vestigial name was corrected in 003 (`AddTaskIndexes`) and 004
   (`AddTeamMemberIndexes`) in one sweep.
2. ~~**ADR-0007 candidate — the concurrency transport convention.**~~ **✅ RESOLVED 2026-07-31.** R-2's
   `ETag`/`If-Match` decision is recorded as §3 of
   **[`docs/adr/0007-implementation-conventions.md`](../../docs/adr/0007-implementation-conventions.md)**,
   alongside the contract drift gate, the Testcontainers rule, and test fixtures — one ADR, as recommended,
   rather than several near-empty ones. 003 confirmed it generalizes and contributed the `DELETE` exemption.
3. **Seed demo projects.** IV.5 asks the seeder to include "a small set of demo projects with tasks". 002
   adds the projects half; the tasks half arrives with 003. Handled as a task, noted here so it is not lost
   between features. *(Open — a real implementation task, not a defect.)*
4. ~~**A second seeded ProjectManager is required for testing.**~~ **✅ RESOLVED 2026-07-31.** The rule is
   now recorded as ADR-0007 §4: **test fixtures come from builders under `tests/**/Builders/`, never from
   the production seeder.** 002's second ProjectManager and 003's second TeamMember are both fixture
   concerns; the seeder's one-user-per-role contract stays a product concern and is unchanged.

---

## Phase status

| Phase | Output | Status |
|---|---|---|
| Phase 0 — Outline & Research | `research.md` | ✅ Complete — 10 inherited by citation, 6 newly derived, 0 unresolved |
| Phase 1 — Design & Contracts | `data-model.md`, `docs/contracts/projects.v1.yaml`, `quickstart.md` | ✅ Complete |
| Phase 2 — Task breakdown | `tasks.md` | ⏳ Run `/speckit.tasks` |
