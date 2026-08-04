# Phase 0 Research: 004 Team Management

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Date**: 2026-07-31
**Constitution**: v1.3.0 · **Inherits**: [001](../001-auth-rbac/research.md) ·
[002](../002-projects/research.md) · [003](../003-tasks/research.md) ·
[ADR-0007](../../docs/adr/0007-implementation-conventions.md)

> **Scope.** 004 is the simplest feature in the product, and this is deliberately the shortest research
> file. Almost everything is inherited. What is genuinely new is defined by **what 004 does *not* do**:
> no `ApplyScope`, no `xmin`, no `PagedResult<T>`, no graduated mutation model, no role column. Each
> absence is a decision, and §B records why each is correct rather than an oversight.

---

## §A — Inherited (cited, not re-decided)

| Decision | Source | Applies to 004 as |
|---|---|---|
| Four-assembly layout; slices in `.Application` | 001 R-1 | `Features/Team/<UseCase>/` — no new project |
| `IApplicationDbContext`, direct LINQ, no repository | 001 R-3 · shared-contracts §7 | Handlers query `db.TeamMembers` directly |
| MediatR pipeline `Logging → Validation → Handler` | 001 R-4 | Reused; 004 adds one validator |
| Contract hand-authored 3.0.3 + `oasdiff` drift gate | 001 R-5 · **ADR-0007 §1** | New file `docs/contracts/team.v1.yaml` |
| Angular Material; generated DTO types | 001 R-6 | `team` route group |
| Testcontainers PostgreSQL; **EF InMemory prohibited** | 001 R-7 · **ADR-0007 §2** | Mandatory — the unique-constraint race is unprovable otherwise (R-3) |
| `Result` → `ActionResult` in one mapper | 001 R-8 | Reused verbatim |
| Access policy implemented in `.Application` | 002 R-1 | `TeamAccessPolicy` follows the identical placement |
| Test fixtures from builders, not the production seeder | **ADR-0007 §4** | 004 needs several members per project |
| Migration adds constraints/indexes only; table pre-exists | 002 R-4 / 003 R-7 | `AddTeamMemberIndexes` — spec already corrected |
| `ETag` / `If-Match` concurrency transport | 002 R-2 · **ADR-0007 §3** | **Not applicable here** — see R-2 |

**Nothing in §A is reopened.**

---

## §B — New decisions for 004

## R-1 — `ITeamAccessPolicy` resolves in the shared kernel, and is **binary by design**

**Verified**: declared in [`docs/shared-contracts.md` §3](../../docs/shared-contracts.md), line 107,
alongside `IProjectAccessPolicy` and `ITaskAccessPolicy`. 004 **implements** it — as
`TeamAccessPolicy` in `.Application/Common/Authorization/`, matching 002 R-1's placement — and declares no
new interface.

```csharp
public interface ITeamAccessPolicy {      // rules owned/implemented by 004
    Task<AccessDecision> CanViewTeamAsync(Project project, CurrentUser caller, CancellationToken ct);   // Admin · owner · member
    Task<AccessDecision> CanManageTeamAsync(Project project, CurrentUser caller, CancellationToken ct); // Admin · owner only
}
```

**Decision: two methods, and deliberately no `ApplyScope`.**

**Rationale.** `ApplyScope` folds a scope predicate into a **cross-collection `IQueryable`** so out-of-scope
rows are never loaded. Here there is no cross-collection read to scope: **every team operation is pinned to
a single project by the route** (`/projects/{projectId}/team`). Read authorization therefore collapses to
one decision — *may this caller view this project's team?* — evaluated once against the parent project.
Adding `ApplyScope` would be machinery with nothing to filter.

**Note the asymmetry between the two methods**, which is the whole feature in miniature:

| | Admin | ProjectManager | TeamMember |
|---|---|---|---|
| `CanViewTeamAsync` | any | **owner *or* member** | member |
| `CanManageTeamAsync` | any | **owner only** | deny |

A ProjectManager can be a *member* of a project they do not own (any active user is eligible — Clarifications
2026-07-22). Such a manager may **view** that team but **not manage** it. That case is real, not
hypothetical, and is the reason the two methods cannot be collapsed into one.

**Alternatives considered.**
- *A single `CanAccessTeamAsync(project, caller, isWrite)`* — a boolean parameter that changes the meaning
  of a method is the shape `TaskMutation` avoided in 003. Rejected for the same reason: it hides two rules
  inside one signature.
- *Reuse `IProjectAccessPolicy.CanReadAsync`* — tempting, since team visibility tracks project visibility.
  Rejected: they differ for the ProjectManager-as-member case above, and coupling them would silently
  change 002's semantics if either rule moved.

---

## R-2 — No `xmin` — the unique constraint **is** the concurrency mechanism

**Decision.** `team_members` carries **no `updated_at` and no row-version token**, and the endpoints take
**no `If-Match` header** — a deliberate departure from ADR-0007 §3, which every other write path follows.

**Rationale.** Optimistic concurrency protects against **lost updates**: two writers each holding stale
state, one silently overwriting the other. A `team_members` row **has no mutable field** — it is inserted
or deleted, never edited — so there is no update to lose and nothing for a row version to protect.

Concurrency safety comes from a different mechanism entirely, the **`UNIQUE (project_id, user_id)`
constraint**:

| Race | Outcome |
|---|---|
| Two concurrent adds of the same `(project, user)` | One **201**, one **409** — enforced by the database, not by application check-then-insert |
| Two concurrent removes | One **204**, one **404** — removal is naturally idempotent |
| Add racing a remove | Either order is consistent; the constraint holds regardless |

> **This is the load-bearing detail:** the guarantee is a database constraint, *not* an
> application-level "does it already exist?" check. That check is an optimization for a friendly error
> message; under concurrency it is a **TOCTOU race**. The handler must therefore catch the unique-violation
> from `SaveChangesAsync` and map it to `ErrorKind.Conflict` — see R-3.

**Shared-contract gap found and closed.** `shared-contracts.md` §5 previously listed the `xmin` entities and
excluded "append-only/system tables (`activity_logs`, `refresh_tokens`)" — **`team_members` appeared in
neither list**, leaving this feature's central design claim unbacked. §5 now names a second exclusion
category, *join tables with no mutable field*, and states the unique-constraint mechanism. Fixed
2026-07-31; recorded in plan.md §Follow-ups.

**Alternatives considered.**
- *Add `xmin` for uniformity* — rejected: a row version on an immutable row is dead weight that implies a
  concurrency story the entity does not have, and would require an `If-Match` header protecting nothing.
- *Application-level existence check alone* — rejected: TOCTOU under concurrent startup or double-click.

---

## R-3 — The duplicate-add race surfaces as a **caught unique violation**, not a pre-check

**Decision.** `AddTeamMemberCommandHandler` performs a friendly pre-check (*already a member?* → 409 with a
clear message), **and** wraps `SaveChangesAsync` to catch Npgsql's unique-violation
(`PostgresException.SqlState == "23505"`) on the `(project_id, user_id)` index, mapping it to
`ErrorKind.Conflict` → **409**. Both paths return the same ProblemDetails body.

**Rationale.** The pre-check gives the common case a good message without a database round-trip failure.
The catch is what makes the guarantee *true* under concurrency (R-2). Implementing only the pre-check would
produce a 500 on the losing side of a genuine race — an unhandled `DbUpdateException` — for what is a
perfectly ordinary, expected outcome. ADR-0003 is explicit that expected outcomes return a `Result`, never
an exception escaping to the error middleware.

**This must be tested against real PostgreSQL** (ADR-0007 §2): the unique constraint and its SQLSTATE are
provider behaviour. EF InMemory has no unique-constraint enforcement at all, so the test would pass
vacuously.

**Alternatives considered.**
- *Pre-check only* — 500 on a real race. Rejected.
- *Catch only, no pre-check* — correct but yields a worse message for the common non-concurrent case, and
  costs a failed round trip. Rejected as needlessly hostile.

---

## R-4 — The roster is a **plain array**, not `PagedResult<T>`

**Decision.** `GET /api/projects/{projectId}/team` returns a **plain JSON array** of member DTOs. No
`?page`/`?pageSize`, no envelope.

**Rationale — this is compliance, not an exception.** Constitution VI.4 and shared-contracts §4 require the
paging envelope for *"every collection endpoint whose result set **can exceed 50 items**"*. A project's team
is a **bounded, human-scale** collection — the people staffed on one project. It is not the unbounded
result set the rule targets, so the rule does not apply and no waiver is needed.

Client-side search over the returned list is sufficient. The design **does not preclude** adding
`?page`/`?pageSize` later without breaking clients (Constitution VI.1) — an added optional query parameter
plus an envelope would be a visible, contract-diffed change if a deployment ever expects unusually large
teams.

**Consistency check across the product:** 002 and 003 page (unbounded), 005's activity feed pages
(unbounded), 005's summary tiles do not (fixed-N), 006's catalog does not (four items), and 004's roster
does not (bounded). The rule is applied by *nature of the collection*, uniformly — not by feature.

**Alternatives considered.**
- *Paginate for uniformity* — rejected: ceremony with no benefit, and it would force every consumer
  (including 003's assignee picker) to unwrap an envelope around a handful of rows.
- *Cap the roster size* — rejected: an arbitrary limit solving a problem no deployment has reported.

---

## R-5 — Removal reads the shared `tasks` **entity** to block; it never mutates it

**Decision.** `RemoveTeamMemberCommandHandler` queries `db.Tasks` through `IApplicationDbContext` for
**open tasks assigned to this member in this project**, and returns **409** with a dependency message when
any exist. "Open" is defined as **`Status != TaskStatus.Done`** — the same terminal-state definition 003,
005, and 006 use.

**Rationale.**
- **Cross-feature boundary (ADR-0006 addendum).** `tasks` is a **shared Domain entity**, one of the two
  permitted forms of cross-feature dependency. 004 **reads** it and **never writes** it; 003 owns task
  mutation. Calling a 003 handler would be the prohibited form.
- **Block rather than cascade.** Clarifications 2026-07-22 fixed this as an invariant, not a config knob:
  the manager must reassign or close the tasks first. This preserves 003's guarantee that a task's
  `assignee_id` always points at a current team member — **without either feature mutating the other's
  data**. The invariant emerges from each side enforcing its own half.
- **`Done` as the boundary** matters: a member with only completed tasks *can* be removed, since their
  historical work stays attributed via `assignee_id` and the audit trail. Blocking on completed tasks
  would make members effectively unremovable.

**A blocked removal writes nothing** — no `team_members` change and **no audit row** (spec B.7). Nothing
happened, so nothing is recorded.

**Alternatives considered.**
- *Auto-unassign the member's tasks* — rejected by Clarifications: it silently drops assigned work to
  unassigned, and it would mean 004 mutating 003's entity, crossing the boundary rule.
- *Cascade-delete the tasks* — rejected outright: destroys work as a side effect of a staffing change.
- *Block on **any** assigned task including `Done`* — rejected; see above.

---

## Resolved Technical Context unknowns

| Unknown | Resolution | Source |
|---|---|---|
| Does `ITeamAccessPolicy` resolve in the shared kernel? | **Yes — §3, verified.** Binary; no `ApplyScope`, by design | R-1 |
| Why no `xmin`? | No mutable field; `UNIQUE (project_id, user_id)` is the mechanism. §5 gap closed | R-2 |
| How does the duplicate-add race surface? | Pre-check **and** caught SQLSTATE 23505 → 409 | R-3 |
| Why no `PagedResult<T>`? | Bounded collection — VI.4's ">50 items" trigger does not fire. Compliance, not exception | R-4 |
| How is the open-tasks block implemented without coupling to 003? | Read the shared `tasks` entity; `Status != Done`; never mutate | R-5 |
| Which migration does 004 add? | `AddTeamMemberIndexes` (unique + two indexes) — spec already corrected | 002 R-4 pattern |
| Everything else | **Inherited from 001/002/003 + ADR-0007** | §A |

**No unresolved NEEDS CLARIFICATION items remain.** Phase 1 may proceed.
