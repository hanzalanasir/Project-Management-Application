# Phase 0 Research: 002 Project Management

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Date**: 2026-07-31
**Constitution**: v1.3.0 · **Inherits**: [001's research](../001-auth-rbac/research.md)

> **Scope of this file.** Per the planning directive, 001's foundational decisions are **reused, not
> re-derived**. §A records what is inherited by citation. §B derives only what is genuinely new to 002 —
> six decisions, all concerning the two-layer authorization model, concurrency transport, and querying.

---

## §A — Inherited from 001 (cited, not re-decided)

| Decision | Source | Applies to 002 as |
|---|---|---|
| Four-assembly solution layout; slices inside `.Application` | [001 R-1](../001-auth-rbac/research.md) | `Features/Projects/<UseCase>/` — **no new project** is added |
| `ApplicationUser` in Domain + `Microsoft.Extensions.Identity.Stores` | 001 R-2 | `Project.Owner` navigation resolves against it (IV.3) |
| `IApplicationDbContext`, direct LINQ, **no repository** | 001 R-3 · [shared-contracts §7](../../docs/shared-contracts.md) | Load-bearing here — see **R-1** below |
| MediatR pipeline `Logging → Validation → Handler`; handlers never call validators | 001 R-4 | Reused verbatim; 002 adds validators only |
| Contract hand-authored in OpenAPI 3.0.3 + Swashbuckle CLI + `oasdiff`, CI-gated | 001 R-5 | New file `docs/contracts/projects.v1.yaml`; same gate, same tooling |
| Angular Material; generated DTO types, hand-written services | 001 R-6 | `projects` route group follows the `auth` pattern |
| **Testcontainers PostgreSQL** everywhere EF is involved | 001 R-7 | **Mandatory here** — see R-1 and R-2; InMemory cannot prove either |
| `Result` → `ActionResult` in one mapper; `ICurrentUserService` in `.Api` | 001 R-8 | Reused verbatim |
| Seeding at startup, idempotent | 001 R-9 | 002 adds demo projects to the existing seeder (IV.5) |
| `InitialCreate` creates **all five** constitution entities | 001 R-10 | Directly changes 002's migration story — see **R-4** below |

**Nothing in §A is reopened.** Where 002's spec appears to conflict with an inherited decision, R-4
resolves it explicitly rather than silently diverging.

---

## §B — New decisions for 002

## R-1 — `IProjectAccessPolicy` is implemented in `.Application`, not `.Infrastructure`

**Decision.** The interface is declared in the shared kernel
([shared-contracts §3](../../docs/shared-contracts.md)); 002 implements it at
**`src/ProjectManagementApp.Application/Common/Authorization/ProjectAccessPolicy.cs`**, injecting
**`IApplicationDbContext`** (shared-contracts §7).

```csharp
public sealed class ProjectAccessPolicy(IApplicationDbContext db) : IProjectAccessPolicy
{
    public IQueryable<Project> ApplyScope(IQueryable<Project> source, CurrentUser caller) => caller.Role switch {
        nameof(Role.Admin)          => source,
        nameof(Role.ProjectManager) => source.Where(p => p.OwnerId == caller.UserId),
        _                           => source.Where(p => p.TeamMembers.Any(tm => tm.UserId == caller.UserId)),
    };
    // CanReadAsync / CanMutateAsync evaluate the same facts for a single loaded entity.
}
```

**Rationale.** Scope rules are **business rules**, and Constitution II.2 places business rules in the
Application layer, not Infrastructure. The only reason to push this into Infrastructure would be that it
needs database access — and **it does not**, because `IApplicationDbContext` is now a shared-kernel
Application abstraction (shared-contracts §7). `ApplyScope` is a pure `IQueryable` transformation that
touches no provider type; `CanReadAsync`/`CanMutateAsync` query `db.TeamMembers` through the same
Application-owned interface.

> **This is the first concrete payoff of anchoring `IApplicationDbContext` in the shared kernel.** Without
> §7, the policy would have had to live in Infrastructure purely to reach the database, splitting the
> authorization rules away from the layer that owns business logic.

**Alternatives considered.**
- *Implementation in `.Infrastructure`* — the conventional Clean Architecture reflex for anything that
  queries. Rejected: it would relocate the single most security-critical rule in the product into the
  persistence layer, and it is unnecessary now that §7 exists.
- *ASP.NET Core resource-based authorization handlers* — already evaluated and rejected in spec 002
  T.2/OQ-002-02: attributes run **before** the entity is loaded, and scope must fold into the `IQueryable`
  for correct paging. Not reopened.

---

## R-2 — The `xmin` concurrency token travels as an HTTP **ETag / `If-Match`**

**Decision.** `GET /api/projects/{id}` returns the row version as a **weak-free strong `ETag`** header.
`PUT /api/projects/{id}` **requires** `If-Match`. The handler assigns the supplied value to EF's original
row-version value so PostgreSQL performs the check:

- `If-Match` present and current → update proceeds
- `If-Match` present but stale → `DbUpdateConcurrencyException` → `ErrorKind.Conflict` → **409**
- `If-Match` **absent** → **400** with a field-style error (`"If-Match header is required."`)

**Rationale.** Spec 002 mandates 409-on-stale (FR-017, T.7, B.5) but is deliberately silent on *transport*,
so this is a genuine open decision.

1. **A header applies uniformly across every write endpoint without touching a single DTO.** 003 Tasks has
   **four** concurrent-write endpoints (full edit, status, assignee, delete); a body field would have to be
   added to four request shapes and could be forgotten in any of them.
2. **Absent-header → 400 makes the check mandatory.** A body field that a client omits degrades silently to
   last-write-wins, which ADR-0004 says is "never acceptable". A required header cannot be silently
   omitted — the request fails loudly.
3. It keeps an infrastructural concern (row version) out of the domain-facing DTO.
4. 400 (not 428 Precondition Required) is chosen deliberately so 002 introduces **no status code outside
   the set its spec and Constitution VI.2 already declare**.

**Alternatives considered.**
- *`version` field in the request body* — simpler to implement and common in .NET codebases. Rejected on
  points 1–3 above; the silent-degradation failure mode is the decisive one.
- *Re-read then compare inside the handler* — rejected outright: it is a check-then-act race, not
  optimistic concurrency, and would report success on a genuinely lost update.
- *428 Precondition Required for a missing `If-Match`* — semantically the most precise HTTP answer, and
  rejected only because it adds a status code neither the spec nor VI.2 lists. Revisit repo-wide if 003–006
  want it; changing it later is a contract-diff-visible, deliberate act.

**Testability note.** This is provable only against real PostgreSQL — `xmin` does not exist elsewhere
(001 R-7). The 409 path is an integration test, not a unit test.

**Implementation-vehicle note (2026-08-06).** The **design decision** above — `ETag`/`If-Match`, not a body
field, 400-absent/409-stale — is unchanged and still 002's to apply first. But the **shared helper class**
that implements it, `ETagExtensions.cs`, is now **created by 001** (001 T117, 001 research R-15), not by
002: 001 added its own `xmin`-guarded mutating endpoints (Admin user management, added 2026-08-05) and,
being built first, became the feature that actually needs this machinery before 002 does. 002's T017/T018
were corrected accordingly — see 002 tasks.md T017/T018 and 002 plan.md's Source Code listing. This is a
credit-and-location change only; 002 still relies on this exact transport for `ProjectsController`'s own
writes (T033, T057, T062, T065).

---

## R-3 — Search uses `ILIKE` over a **`pg_trgm` GIN index**; sorting uses a **whitelist**

**Decision.**
- **Search** (`?search=`) matches `name` and `description` with case-insensitive substring semantics via
  `EF.Functions.ILike(p.Name, $"%{term}%")`, backed by a **`pg_trgm` GIN index** on `name`.
- **Sorting** (`?sort=`) maps a **closed whitelist** of field names to expressions; an unrecognized value
  returns **400** rather than being interpolated into a query.

**Rationale.**
- 002 B.1 asks for "a text index on `name` for search" without naming a mechanism. A plain B-tree index
  **cannot serve a leading-wildcard `%term%` predicate** — the index would simply not be used, and the
  query degrades to a sequential scan as the table grows. `pg_trgm` GIN is the PostgreSQL answer to
  substring search and keeps NFR-005 ("indexes keep role-scoped listing performant") honest.
- Full-text search (`tsvector`) was the other candidate and is **semantically wrong here**: it matches
  whole words with stemming, so searching `apo` would not find "Apollo". Users searching a project list
  expect substring behavior.
- A sort whitelist is required because a user-supplied sort field otherwise reaches the query builder as a
  string. Even though EF Core parameterizes values, dynamic *ordering by an arbitrary column name* is the
  classic injection vector in this shape of endpoint.

**Enabling `pg_trgm`.** The extension is enabled from the migration
(`migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;")`), which is a **permitted** use of raw
SQL: Constitution IV.1 prohibits raw SQL for *data access*, and DDL for an extension is neither a query nor
a bypass of the DbContext. Testcontainers provisions a stock `postgres` image that includes `pg_trgm`.

**Alternatives considered.**
- *B-tree on `name`* — rejected; unusable for `%term%`, giving a false sense of index coverage.
- *`tsvector` full-text* — rejected; wrong matching semantics for a name filter (see above).
- *In-memory filtering after the scoped query* — rejected outright; violates FR-007/NFR-002 and would leak
  `totalCount`.

---

## R-4 — 002 adds **no table-creating migration**; it adds indexes only

**Decision.** The `projects` table **already exists** — created by 001's `InitialCreate`, which builds all
five constitution entities ([001 R-10](../001-auth-rbac/research.md)). 002 therefore adds a single
migration **`AddProjectIndexes`** carrying only the indexes and the `pg_trgm` extension:
`(owner_id)`, `(status)`, `(owner_id, status)`, and the GIN trigram index on `name`.

> **If 001 has not yet been implemented**, fold these indexes into `InitialCreate` instead and 002 needs
> **no migration at all**. Prefer that — it is one fewer migration for the same result.

**This resolves an internal inconsistency in spec 002**, which should be corrected when convenient:

| Spec 002 says | Where | Status |
|---|---|---|
| "Migration name: `AddProjectsTable`" | B.1 header | ❌ **Vestigial** — predates the all-five-in-InitialCreate decision |
| "all five constitution entities … are created in the **initial EF Core migration**" | Assumptions | ✅ **Authoritative** — echoed identically in 003 and 004, and already acted on by 001's plan |

The Assumptions statement wins: it is the load-bearing one, it is repeated across three specs, and 001's
plan has already implemented it. `AddProjectsTable` is a leftover name from before that decision.

**Rationale for surfacing rather than silently diverging.** Following B.1 literally would produce a
migration that tries to create a table that already exists — a hard failure on first run. Following
Assumptions silently would leave a reader of B.1 confused about why no such migration exists.

> **✅ RESOLVED 2026-07-31.** Both occurrences in spec 002 now read **`AddProjectIndexes`**, and the same
> vestigial name was corrected in 003 and 004 in one sweep. The table `Status` column above is retained as
> the record of what was found.

**Alternatives considered.**
- *Have 002 create the table (revert 001 R-10)* — rejected: it would break 002's own TeamMember scope join
  and 003's assignee validation, both of which need `team_members`/`projects` present from day one.

---

## R-5 — Cascade delete is **configured in 001 but proven in 002**

**Decision.** The delete behaviors (`tasks.project_id` → CASCADE, `team_members.project_id` → CASCADE,
`projects.owner_id` → RESTRICT) are declared in 001's EF configuration
([001 data-model §4](../001-auth-rbac/data-model.md)). **002 owns the tests that prove them**, in
`tests/ProjectManagementApp.Infrastructure.Tests/Projects/CascadeBehaviorTests.cs`.

**Rationale.** 002 is the first feature with a delete endpoint, so it is the first point at which cascade
behavior is *observable*. Configuration without a test that exercises it is an assumption, and this
particular assumption destroys data if wrong. 002's DoD #7 already requires it ("Deleting a project
cascades to dependent tasks/assignments; deleting a user is restricted while they own projects").

Delete ordering is fixed by IV.4 and 002 US-005: the `ProjectDeleted` audit row is written **before** the
row is removed, in the same transaction, and `activity_logs` is never cascaded away.

**Alternatives considered.**
- *Application-level cascade (load children, delete each)* — rejected: slower, racy, and it would bypass
  the database-level guarantee that Constitution IV.3 asks to be "explicit and intentional".

---

## R-6 — Owner-role validation runs in the handler, against the shared `users` table

**Decision.** The rule *"a project's owner MUST hold ProjectManager or Admin"* (spec Clarifications
2026-07-22, FR-003) is enforced in `CreateProjectCommandHandler` and `UpdateProjectCommandHandler` by
checking the candidate's role via **`UserManager<ApplicationUser>.GetRolesAsync(user)`**, returning
`ErrorKind.Validation` → **400** on violation.

**Rationale.** It is a **validation** rule about a referenced entity, not an authorization rule about the
caller, so it belongs in the handler rather than in `IProjectAccessPolicy` — mixing it into the policy
would conflate "may this caller act?" with "is this payload valid?". It cannot live in the
FluentValidation validator either, because it requires a database lookup and validators run in the
pipeline behavior before the handler's unit of work is in play.

It applies on **both** paths the spec names: Admin-specified `ownerId` at create, and Admin-only ownership
transfer at update. A ProjectManager's client-supplied `ownerId` is **ignored entirely** (owner is taken
from the token), so the rule cannot be reached by escalation.

**The data path, stated explicitly** *(added 2026-08-04, closing analyze finding G2)*: use
**`UserManager<ApplicationUser>.GetRolesAsync(user)`**. A direct query on `user_roles` is **not available** —
`IApplicationDbContext` (shared-contracts §7) exposes six `DbSet`s and **neither `Roles` nor `UserRoles`**
is among them. `UserManager` is reachable from `.Application` through the transitive
`Microsoft.Extensions.Identity.Stores` → `.Core` reference that `.Domain` carries, and 001 T060 already
uses it from a handler, so the precedent exists.

**Alternatives considered.**
- *Direct `user_roles` join via `IApplicationDbContext`* — **not possible**; no such `DbSet` is exposed.
  Adding one would widen the shared kernel to satisfy a single validation rule.
- *Database CHECK constraint* — impossible; the role lives in a join table, not a column on `projects`.
- *Enforce in `IProjectAccessPolicy`* — rejected; conflates validation with authorization (see above).

---

## Resolved Technical Context unknowns

| Unknown | Resolution | Source |
|---|---|---|
| Where the scope policy is implemented | `.Application/Common/Authorization/`, injecting `IApplicationDbContext` | R-1 |
| How the row version reaches the server | `ETag` on GET, required `If-Match` on PUT; absent → 400, stale → 409 | R-2 |
| Search mechanism and index type | `ILIKE` + `pg_trgm` GIN; sort via closed whitelist | R-3 |
| Which migration 002 adds | `AddProjectIndexes` only — the table already exists | R-4 |
| Who proves cascade behavior | 002's Infrastructure tests | R-5 |
| Where owner-role eligibility is checked | Create/Update handlers → 400 | R-6 |
| Everything else (layout, pipeline, contract tooling, tests, mapping) | **Inherited from 001** | §A |

**No unresolved NEEDS CLARIFICATION items remain.** Phase 1 may proceed.
