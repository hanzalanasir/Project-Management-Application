# Phase 0 Research: 003 Task Management

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Date**: 2026-07-31
**Constitution**: v1.3.0 · **Inherits**: [001 research](../001-auth-rbac/research.md) ·
[002 research](../002-projects/research.md)

> **Scope.** Per the planning directive, 001's foundational decisions and 002's conventions are **reused,
> not re-derived**. §A records the inheritance. §B derives only what is new to 003 — which is dominated by
> one thing: the **graduated authorization model**, the first place in the product where a user who passes
> the scope gate is still permitted only *part* of a write.

---

## §A — Inherited (cited, not re-decided)

| Decision | Source | Applies to 003 as |
|---|---|---|
| Four-assembly layout; slices inside `.Application` | 001 R-1 | `Features/Tasks/<UseCase>/` — **no new project** |
| `IApplicationDbContext`, direct LINQ, no repository | 001 R-3 · [shared-contracts §7](../../docs/shared-contracts.md) | Load-bearing for `ApplyScope` (as in 002) |
| MediatR pipeline `Logging → Validation → Handler` | 001 R-4 | Reused; 003 adds validators only |
| Contract hand-authored 3.0.3 + Swashbuckle CLI + `oasdiff` | 001 R-5 | New file `docs/contracts/tasks.v1.yaml` |
| Angular Material; generated DTO types, hand-written services | 001 R-6 | `tasks` route group |
| Testcontainers PostgreSQL everywhere EF is involved | 001 R-7 | **Mandatory** — the 15-cell matrix and `xmin` need real SQL |
| `Result` → `ActionResult` in one mapper | 001 R-8 | Reused verbatim |
| `InitialCreate` creates all five entities | 001 R-10 | `tasks` table already exists — see **R-7** |
| **Access policy implemented in `.Application`**, not Infrastructure | **002 R-1** | `TaskAccessPolicy` follows the identical placement |
| **`ETag` / required `If-Match` for `xmin`** | **002 R-2** | Extended to **four** write endpoints — see **R-5** |
| `ILIKE` + `pg_trgm` GIN for search; closed sort whitelist | **002 R-3** | Same mechanism on `title`; extension already enabled |
| Migration adds indexes only, table pre-exists | **002 R-4** | Same situation — see **R-7** |

**Nothing in §A is reopened.**

---

## §B — New decisions for 003

## R-1 — ✅ `ITaskAccessPolicy` **resolves in the shared kernel** — and a compile-order gap it exposes

**Confirmation requested by the planning directive. Verified against the file, not assumed.**

`ITaskAccessPolicy` is declared in **[`docs/shared-contracts.md` §3](../../docs/shared-contracts.md)**
(line 101), alongside `IProjectAccessPolicy` and `ITeamAccessPolicy`, with the graduated signature intact:

```csharp
public interface ITaskAccessPolicy {      // rules owned/implemented by 003; reused by 005, 006
    IQueryable<TaskItem> ApplyScope(IQueryable<TaskItem> source, CurrentUser caller);
    Task<AccessDecision> CanReadAsync(TaskItem task, CurrentUser caller, CancellationToken ct);
    Task<AccessDecision> CanMutateAsync(TaskItem task, TaskMutation mutation, CurrentUser caller, CancellationToken ct);
}
```

**Decision: 003 *implements* this interface; it declares no new per-feature authorization interface.** The
implementation is `TaskAccessPolicy` in **`.Application/Common/Authorization/`**, matching 002's placement
(002 R-1) — injecting `IApplicationDbContext` so scope rules stay in the layer that owns business logic.
This is what lets 005 Dashboard and 006 Reports call `ITaskAccessPolicy.ApplyScope` without depending on
003's Application layer.

### ⚠️ The gap this verification uncovered

The shared-kernel signature references **`TaskMutation`** — and **001 does not create it.**

| Fact | Evidence |
|---|---|
| The shared interface depends on `TaskMutation` | shared-contracts §3, line 104 |
| `TaskMutation` is described as "owned by 003" | shared-contracts §3, line 113 |
| 001's `T022` authors the shared-kernel interfaces **in Phase 2** | 001 tasks.md T022 |
| 001's `T018` creates `Role, AuditAction, ProjectStatus, TaskStatus, TaskPriority` — **not `TaskMutation`** | 001 tasks.md T018 |
| `TaskMutation` appears nowhere in 001's plan, data-model, or tasks | grep: zero hits under `specs/001-auth-rbac/` |

**Consequence if unaddressed:** `Application/Common/Interfaces/ITaskAccessPolicy.cs` would **not compile**
at the end of 001's Foundational phase — 001 would be left in a non-building state until 003 shipped, which
inverts the dependency the shared kernel exists to establish.

**Decision:** `TaskMutation` must be created **in 001**, not 003. Recommended placement is
**`.Application/Common/Models/TaskMutation.cs`**, beside `AccessDecision` — its sibling in the very same
method signature — because it is an **authorization-vocabulary type that is never persisted** (003 B.2:
"authorization input, not persisted"), not a domain concept.

> **✅ RESOLVED 2026-07-31.** Fixed at all three levels: 001's **T020** now creates `TaskMutation`; 001's
> **data-model §3** lists it with the placement rationale; and **shared-contracts §3** no longer describes
> it as "owned by 003" — that phrasing was the root cause of the omission. See plan.md Follow-up 1.

> Deferring it to 003 is the one option that does **not** work: 001 authors the interface, so 001 must be
> able to compile it.

**Alternatives considered.**
- *Declare a 003-local `ITaskAccessPolicy`* — rejected outright; it is exactly the per-feature interface the
  directive asks to avoid, and 005/006 would then depend on 003's Application layer, violating the ADR-0006
  addendum boundary rule.
- *Put `TaskMutation` in `Domain/Enums/`* with the other enums — perfectly workable and arguably more
  consistent with T018's "enums live in Domain" habit. Rejected only on semantics: it is never persisted and
  models an authorization input, not domain state. **If you prefer the simpler rule, this is a fine
  substitute** — what matters is that it lands in 001, not where.

---

## R-2 — The graduated matrix is a `switch` in one place, enforced twice

**Decision.** `CanMutateAsync` resolves the spec's 5 × 3 matrix (T.2) in a single expression inside
`TaskAccessPolicy`; there is **no per-endpoint authorization logic anywhere else**.

| `TaskMutation` | Admin | ProjectManager (owns parent project) | TeamMember (is assignee) |
|---|---|---|---|
| `Create` · `FullEdit` · `Reassign` · `Delete` | allow | allow | **deny** |
| `StatusChange` | allow | allow | **allow** |

Enforcement is **deliberately belt-and-braces**, and both halves are required:

1. **Structural — narrow endpoints bind narrow commands.** `PUT /api/tasks/{id}/status` binds a
   **status-only** command. A widened payload carrying `title` or `assigneeId` is *structurally incapable*
   of changing them: there is no property to bind to. Privilege escalation by extra JSON field is
   impossible, not merely rejected.
2. **Behavioural — `CanMutateAsync` is still consulted**, so the rule survives a future slice that reuses
   the policy through a different route.

**Rationale.** The spec calls the assignee-allowed/assignee-refused pair on the *same row* "the acceptance
test for the graduated model" (T.3 examples 3–4). Mechanism 1 alone would leave the rule implicit in DTO
shape — invisible to a reader and silently lost if someone later widens the DTO. Mechanism 2 alone would
leave a mass-assignment surface. Together, the rule is both explicit and unbypassable.

**The 15-cell matrix is a table-driven xUnit test** (5 mutations × 3 roles), per spec B.6/DoD #3 — not
fifteen hand-written tests.

**Alternatives considered.**
- *One `[Authorize]` policy per operation* — rejected in spec T.2 and not reopened: the permitted set
  depends on row facts (who owns the parent project, who is assigned) unknown until the entity is loaded,
  and attributes run before that.
- *A single `PUT` with server-side field diffing* — rejected: it reintroduces the mass-assignment surface
  the narrow endpoints exist to remove, and makes "which fields did you actually change?" a runtime
  question instead of a routing one.

---

## R-3 — `closed_at` is a derived side effect, never bindable

**Decision.** `closed_at` is set to `now (UTC)` when a status change moves a task **to** `Done`, and cleared
to `null` when it moves **away** from `Done`. It is computed **inside `UpdateTaskStatusCommandHandler`** and
appears in **no** request command. A `Done → Done` no-op leaves it unchanged.

**Rationale.** 006 Reports depends on `closed_at` for completion-trend buckets and "closed" counts
precisely because — unlike `updated_at` — a later edit to a finished task will not move it. That guarantee
holds only if the column is unwritable by clients. Since the status endpoint already binds a status-only
command (R-2), `closed_at` is structurally unreachable from the API surface; the handler is the only writer.

The re-open rule (`closed_at` cleared) is what makes 006's counting rule coherent: a re-opened task drops
out of completed buckets until re-completed, uniformly across Project Progress and Task Completion.

**No new audit action.** The `TaskStatusChanged` entry's `change_summary` reflects the `closed_at`
transition; spec B.7 explicitly forbids inventing a separate event for it.

**Alternatives considered.**
- *Accept `closedAt` in the request* — rejected: it would let a client backdate completion, corrupting every
  006 metric, and it contradicts spec E.DB ("`closed_at` is derived, not accepted").
- *Compute it in 006 from the audit log* — rejected: an `activity_logs` scan per report row, and it would
  break if audit retention were ever trimmed.

---

## R-4 — One handler serves both list routes; the 403/404 asymmetry is intentional

**Decision.** `ListTasksQueryHandler` serves **both** `GET /api/projects/{projectId}/tasks` and
`GET /api/tasks`. The nested route simply pre-populates the `ProjectId` filter. Both compose the identical
`ITaskAccessPolicy.ApplyScope` predicate and return the same `PagedResult<T>` envelope.

Their **failure surfaces differ deliberately**, matching the spec's API catalog:

| Route | Failure codes | Why |
|---|---|---|
| `GET /api/projects/{projectId}/tasks` | 400, 401, **403, 404** | Names a specific project → an unknown id is 404, an out-of-scope one is 403 (002's convention) |
| `GET /api/tasks` | 400, 401 | Names no resource → scope shapes *content*; an empty result is `200` with an empty page |

**Rationale.** This is the same principle 005 Dashboard applies at feature scale: **naming a resource
invites a 403/404; not naming one cannot.** Keeping one handler guarantees the two routes can never drift
into divergent scoping logic — the Clarifications entry (2026-07-22) that kept both endpoints made exactly
this promise ("Both use the identical `ApplyScope` predicate and `PagedResult<T>` envelope").

**A filter can only narrow, never widen.** `?assigneeId=` supplied by a TeamMember for a colleague returns
an **empty page**, not 403 — the scope predicate is `AND`-ed, so it simply matches nothing. Returning 403
would confirm the colleague's task exists.

**Alternatives considered.**
- *Two handlers* — rejected: duplicated scope composition is exactly how the two routes would diverge.
- *Drop the cross-project route* — rejected by Clarifications 2026-07-22; a TeamMember's "my work across all
  projects" view needs it, and 005 Dashboard reuses it as-is.

---

## R-5 — Concurrency: `If-Match` on the three `PUT`s, **not** on `DELETE`

**Decision.** Inheriting 002 R-2, the `xmin` row version travels as an `ETag` on reads and a **required
`If-Match`** on `PUT /api/tasks/{id}`, `PUT /api/tasks/{id}/status`, and `PUT /api/tasks/{id}/assignee`
(absent → 400, stale → 409). **`DELETE /api/tasks/{id}` does not require it.**

**Rationale for exempting delete.** Optimistic concurrency protects against *lost updates* — two writers
each believing they have current state. A delete has no lost-update failure mode: the row is gone either
way, and the second caller correctly observes **404** (spec US-003-06 edge case: "deleting twice yields 404
on the second call"). Requiring `If-Match` there would add friction for no guarantee. This matches 002,
which also leaves `DELETE` unconditional.

**Note on the narrow endpoints.** All three `PUT`s contend on the *same row version* — a status change and a
reassignment both bump `xmin`. That is correct and intended: they are genuinely conflicting writes to one
row, and the loser should reload. The narrow-DTO design limits *which fields* a caller may change, not
*whether* they conflict.

**Alternatives considered.**
- *`If-Match` on `DELETE` too* — rejected as above; uniformity for its own sake at the cost of usability.
- *Per-field concurrency* — rejected: PostgreSQL's `xmin` is row-level, and field-level versioning is a
  large complication for a problem this app does not have.

---

## R-6 — Assignee validation reads the shared `team_members` **entity**, never 004's handlers

**Decision.** Assignee eligibility — *"the candidate must be a team member on this task's project, and must
be active"* — is checked inside `CreateTaskCommandHandler` and `ReassignTaskCommandHandler` by querying
**`db.TeamMembers`** through `IApplicationDbContext`. Violations return `ErrorKind.Validation` → **400**.

**Rationale — this is the ADR-0006 boundary rule in practice.** `team_members` is a **shared Domain
entity**, which the addendum names as one of the two permitted forms of cross-feature dependency. Calling a
004 handler would be the other, prohibited form. 003 **reads** the pool and **never mutates it**; 004 owns
add/remove.

The invariant holds from the other side too: 004 **blocks** (409) removing a member who still has open
assigned tasks, so a task's `assignee_id` can never point at a non-member. Neither feature mutates the
other's data; the guarantee emerges from each enforcing its own half.

**Validation, not authorization** — so it lives in the handler, not in `ITaskAccessPolicy`, exactly as 002
R-6 placed owner-role eligibility. Mixing it into the policy would conflate "may this caller act?" with "is
this payload valid?".

**Alternatives considered.**
- *Database FK to `team_members`* — impossible: the constraint would need to span `(project_id, assignee_id)`
  against a different table's composite key while `assignee_id` is independently nullable.
- *Validate in the FluentValidation validator* — rejected: it requires a database lookup, and validators run
  in the pipeline behavior before the handler's unit of work.

---

## R-7 — 003 adds **no table-creating migration**; indexes only

**Decision.** The `tasks` table — **including `closed_at`** — already exists from 001's `InitialCreate`
(001 R-10; 001 data-model §2 authors `TaskItem` "to the field lists in … 003 §Data Model (incl.
`closed_at`)"). 003 therefore adds one migration, **`AddTaskIndexes`**:

```
CREATE INDEX ix_tasks_project_id           ON tasks (project_id);
CREATE INDEX ix_tasks_assignee_id          ON tasks (assignee_id);
CREATE INDEX ix_tasks_status               ON tasks (status);
CREATE INDEX ix_tasks_project_id_status    ON tasks (project_id, status);
CREATE INDEX ix_tasks_assignee_id_status   ON tasks (assignee_id, status);
CREATE INDEX ix_tasks_title_trgm           ON tasks USING gin (title gin_trgm_ops);
```

`pg_trgm` is **already enabled** by 002's `AddProjectIndexes`; EF applies migrations in order, so 003 does
not re-issue `CREATE EXTENSION`. (Harmless to repeat with `IF NOT EXISTS` if 002 is ever reordered.)

**Same spec inconsistency as 002** — spec 003 B.1 said *"Migration name: `AddTasksTable`"* while its own
Assumptions say all five entities come from the initial migration. Assumptions was authoritative.

> **✅ RESOLVED 2026-07-31 — swept across all three specs at once**, as recommended: 002 →
> `AddProjectIndexes`, 003 → `AddTaskIndexes`, 004 → `AddTeamMemberIndexes` (six occurrences), each with an
> explicit note that the table itself comes from 001's `InitialCreate`. **004's plan will not need to
> re-flag this.**

**Index rationale.** `(assignee_id)` and `(assignee_id, status)` serve the TeamMember scope predicate and
005's personal-slice aggregate — Tasks is the highest-volume table, so this is where NFR-005 is actually
tested. The GIN trigram on `title` serves `?search=` for the same reason as 002 R-3.

---

## Resolved Technical Context unknowns

| Unknown | Resolution | Source |
|---|---|---|
| Does `ITaskAccessPolicy` resolve in the shared kernel? | **Yes — shared-contracts §3, verified.** 003 implements, declares nothing | **R-1** |
| Where does `TaskMutation` live? | `.Application/Common/Models/` — and it must be created **in 001** | **R-1** ⚠️ |
| How is the graduated matrix enforced? | One `switch` in `TaskAccessPolicy` + narrow commands; 15-cell table-driven test | R-2 |
| How is `closed_at` set? | Derived in the status handler; never bindable | R-3 |
| Two list routes — one handler or two? | One handler; 403/404 only on the nested route | R-4 |
| Concurrency across four write endpoints | `If-Match` on the three `PUT`s; `DELETE` exempt | R-5 |
| Assignee validation without coupling to 004 | Read the shared `team_members` entity via `IApplicationDbContext` | R-6 |
| Which migration does 003 add? | `AddTaskIndexes` only — the table pre-exists | R-7 |
| Everything else | **Inherited from 001/002** | §A |

**No unresolved NEEDS CLARIFICATION items remain.** Phase 1 may proceed.
