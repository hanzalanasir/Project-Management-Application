# Phase 0 Research: 005 Dashboard

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Date**: 2026-07-31
**Constitution**: v1.3.0 · **Inherits**: [001](../001-auth-rbac/research.md) ·
[002](../002-projects/research.md) · [003](../003-tasks/research.md) · [004](../004-team/research.md) ·
[ADR-0007](../../docs/adr/0007-implementation-conventions.md)

> **Scope.** 005 is the first **strictly read-only** feature: no new entity, no table, no migration, no
> write path. Almost everything is inherited. §B derives six new decisions — two of which are **gaps in
> upstream artifacts** that this feature cannot work around.

---

## §A — Inherited (cited, not re-decided)

| Decision | Source | Applies to 005 as |
|---|---|---|
| Four-assembly layout; slices in `.Application` | 001 R-1 | `Features/Dashboard/<UseCase>/` — **query slices only, no commands** |
| `IApplicationDbContext`, direct LINQ, no repository | 001 R-3 · shared-contracts §7 | **Load-bearing** — aggregates compose over `IQueryable` (R-4) |
| MediatR pipeline `Logging → Validation → Handler` | 001 R-4 | Reused; 005 adds one paging validator |
| Contract hand-authored 3.0.3 + `oasdiff` gate | 001 R-5 · ADR-0007 §1 | New file `docs/contracts/dashboard.v1.yaml` |
| Angular Material; generated DTO types | 001 R-6 | `dashboard` route group; Chart.js per III |
| Testcontainers PostgreSQL; EF InMemory prohibited | 001 R-7 · ADR-0007 §2 | **Mandatory** — filter-at-source is a claim about SQL (R-4) |
| `Result` → `ActionResult` in one mapper | 001 R-8 | Reused verbatim |
| Access policies implemented in `.Application` | 002 R-1 | 005 **consumes** `ApplyScope`, implements no policy |
| `ETag`/`If-Match` concurrency transport | 002 R-2 · ADR-0007 §3 | **Not applicable** — no writes, no `xmin` |
| Pagination by nature of the collection | 004 R-4 | Feed pages, tiles do not — see R-5 |
| Cross-feature deps via shared abstractions or Domain entities only | ADR-0006 addendum | 005 depends on `IProjectAccessPolicy`/`ITaskAccessPolicy`/`IActivityLogService`, never on 001–004 handlers |

**Nothing in §A is reopened.**

---

## §B — New decisions for 005

## R-1 — ⚠️ `IActivityLogService` needs a **scoped read** that 001 does not define

**The feed cannot be built without this, and the workaround is explicitly forbidden.**

| Fact | Evidence |
|---|---|
| 001 defines the interface with **`LogAsync` only** | 001 spec B.3, lines 515–518 |
| 001's plan, data-model, and tasks never mention a read | grep `QueryScoped\|scoped read` under `specs/001-auth-rbac/` → **zero hits** |
| shared-contracts §6 documents only the **write** | §6 text |
| 005 **must** read the audit log for the activity feed | 005 FR-006 |
| 005 is **forbidden** from querying `activity_logs` directly | 005 FR-006: *"MUST be read **through** 001's `IActivityLogService` (never by a direct `activity_logs` query)"* |
| 006 has the identical need | 006 FR-007, same prohibition |

**Decision.** `IActivityLogService` gains a **scoped read** method, declared in **001** with the rest of
the interface, in the shared kernel:

```csharp
public interface IActivityLogService {
    Task LogAsync(Guid? actorId, string action, string entityType, string entityId,
                  string changeSummary, CancellationToken ct);                        // existing

    // NEW — consumed by 005 (feed) and 006 (Activity Report). Scope is derived from the caller,
    // never passed from a request body.
    Task<PagedResult<ActivityEntry>> QueryScopedAsync(
        ActivityScope scope, int page, int pageSize, CancellationToken ct);
}
```

**Rationale.** 001 **owns** `activity_logs` and its service, so exposing a read on it is within 001's own
contract — not a retroactive change to 002/003, which is exactly how both 005 and 006 already frame it in
their Assumptions. The alternative — each read-side feature querying the table directly — is prohibited by
two specs for a good reason: it would scatter audit-scoping logic across features and bypass 001's
no-secrets guarantee.

> **This is the same failure mode as 003's `TaskMutation`**: a shared-kernel type whose *consumer* is a
> later feature, so the *authoring* feature never planned for it. Two instances now suggest the pattern is
> worth a general check — see plan.md Follow-up 3.

**Required amendments (they touch 001):**
1. **001's T022** — add `QueryScopedAsync` to the `IActivityLogService` declaration.
2. **001's T033** — implement it in `ActivityLogService` (a scoped, paginated read).
3. **shared-contracts §6** — document that the audit service exposes both a write and a scoped read.
4. `ActivityScope` and `ActivityEntry` belong beside the interface, in `Application/Common/`.

> **✅ RESOLVED 2026-07-31 — all four applied.** `QueryScopedAsync` is declared in 001 spec B.3, created by
> T022, implemented by T033 (scope filter, stable newest-first ordering, clamped `pageSize`, scope-limited
> `totalCount`); `ActivityScope`/`ActivityEntry` added to T020; shared-contracts §6 now documents both
> members. See plan.md Follow-up 1.

**Alternatives considered.**
- *005 declares its own read interface over `activity_logs`* — rejected: two abstractions over one table,
  and it violates the ADR-0006 addendum by making 005 own part of 001's entity's access.
- *Query `activity_logs` directly from the dashboard handler* — rejected: explicitly forbidden by FR-006.

---

## R-2 — ⚠️ Overdue's timezone must be **UTC, fixed** — 005's "unless configured" breaks 006

**Decision.** The overdue boundary is evaluated in **UTC, fixed and non-configurable**, matching 006.
`Dashboard:OverdueBoundary` remains as a knob for the *comparison* (strictly-before-today), but **the
timezone is not a knob**.

**Rationale — this is a genuine cross-feature contradiction, not a preference.**

| Spec | Says |
|---|---|
| **005** Assumptions | *"before today" is evaluated against a documented timezone assumption (**server/UTC unless configured**)* |
| **006** Clarifications | *Timezone … → **UTC (server zone), fixed**. … not a v1 configurable* |
| **006** NFR-002 | *Shared metrics are **value-identical to Dashboard** for the same caller/window (**a cross-feature test guards this**)* |

If 005's timezone is configurable and any deployment moves it off UTC, the same caller's overdue count
differs between the Dashboard tile and the Project Progress report — and **006's parity test, which is a
stated hard requirement, becomes unsatisfiable by configuration**. A requirement that a config flag can
break is not a requirement.

005's wording predates 006's clarification; 006 resolved the question for the product. **Fixing 005 to UTC
is the minimal change that makes both specs simultaneously satisfiable.**

> **✅ RESOLVED 2026-07-31.** Both locations in 005's spec corrected: Assumptions now states *"evaluated in
> UTC — fixed, not configurable"* with the parity reasoning inline, and B.4 scopes
> `Dashboard:OverdueBoundary` to the date **comparison** only, explicitly noting the timezone is not a
> knob. 006's NFR-002 is now satisfiable by construction. See plan.md Follow-up 2.

**Alternatives considered.**
- *Make 006 configurable too* — rejected: 006 chose fixed UTC deliberately so a report is **reproducible
  across viewers** (the same parameters yield the same exported PDF regardless of who runs it). Making it
  configurable would surrender that.
- *Leave both and let the parity test pin it* — rejected: the test would pass on a default-configured
  machine and fail in a deployment that exercised the flag. Latent, environment-dependent breakage.

---

## R-3 — **No 403 anywhere** — content-scoping instead, the deliberate exception to 002's convention

**Decision.** No dashboard endpoint returns **403** or **404**. A caller whose visible-project set is empty
receives **200** with all counts zero and all breakdowns present-but-empty. The only error statuses are
**400** (bad paging on the feed) and **401**.

**Rationale.** 002 established the app-wide convention that an out-of-scope resource returns 403
(Clarifications 2026-07-22), and 003/004/006 all inherit it. 005 is the **stated exception**, and the
distinction is principled rather than arbitrary:

> **403 is the right answer when a caller *names* a resource they may not have. The dashboard names
> nothing.** It asks "summarize what I can see" — a question that always has a valid answer, including
> "nothing". Returning 403 to a TeamMember on no teams would be saying *"you are forbidden from your own
> empty dashboard"*, which is incoherent.

This is the same rule 003 applies at endpoint granularity (nested task route can 403; cross-project route
cannot — 003 R-4) and 006 applies at parameter granularity (a **named** `projectId` outside scope → 403;
`projectScope=all` → silently narrowed). One principle, three consistent applications: **naming invites a
403; not naming cannot.**

**A zero-scope dashboard must still return the full contract shape** — every `ProjectStatus` and
`TaskStatus` key present with `0` — so the frontend renders an empty state rather than crashing on missing
keys (R-4).

**Alternatives considered.**
- *403 for a caller with no visible projects* — rejected as incoherent (above), and it would make the
  landing page of a newly-onboarded user an error screen.
- *404 for an empty dashboard* — rejected: the dashboard always exists for an authenticated caller.

---

## R-4 — Aggregation: scope as an `IQueryable<Guid>` subquery, `GROUP BY` pushed to SQL

**Decision.** The visible-project set is built as an **un-materialized `IQueryable<Guid>`** via
`IProjectAccessPolicy.ApplyScope(...).Select(p => p.Id)`, and every aggregate is composed against it:

```csharp
// each metric = one grouped aggregate, scope pushed into SQL as a subquery
db.Tasks.Where(t => visibleProjectIds.Contains(t.ProjectId))
        .GroupBy(t => t.Status)
        .Select(g => new { Status = g.Key, Count = g.Count() })
```

Enum-keyed maps are **seeded with every enum value at zero** before the query results are merged in, so the
response is a **stable typed contract**, not a free-form dictionary whose keys vary with the data.

**Rationale.**
- **Materializing the project ids first** (`ToListAsync()` then `Contains`) would work for small data and
  is the tempting shortcut — but it round-trips ids to the app and back, and for an Admin it is the entire
  projects table. Keeping it an `IQueryable` lets PostgreSQL evaluate it as a subquery, which is the whole
  point of the no-repository decision (001 R-3, shared-contracts §7).
- **Zero-seeding the enum maps** is what makes the contract stable. Without it, a status with no rows
  simply vanishes from the JSON, and every consumer needs null-guards — precisely the "free-form stat
  dictionary" the spec forbids.
- **This is only verifiable against real PostgreSQL.** InMemory would satisfy the assertions while
  evaluating everything in memory, hiding a fetch-then-filter regression (ADR-0007 §2).

**The TeamMember task tiles use the personal predicate** (`assignee_id == caller`), identical to the
US-005-03 slice — one number to compute and test, not two (Clarifications 2026-07-22).

**Alternatives considered.**
- *One query returning raw rows, aggregated in C#* — rejected: loads out-of-scope-adjacent volume into
  memory and violates NFR-002.
- *A single mega-query producing every metric* — rejected: unreadable, and PostgreSQL plans the small
  grouped queries well independently.

---

## R-5 — Tiles are not paginated; the feed is

**Decision.** `GET /api/dashboard/summary` returns a **fixed-N** payload with no paging. `GET
/api/dashboard/activity` returns `PagedResult<T>` (default 20, max 100, clamped).

**Rationale.** Same rule as 004 R-4, applied to two collections in one feature: paging follows the
**nature of the collection**, not the feature. The summary is a bounded set of scalar and enum-keyed
metrics — there is nothing to page. The feed grows without bound and must page (VI.4).

This also drives the **endpoint-shape decision**: paginating a sub-list embedded inside a summary payload
is awkward and couples two very different cache/lifetime profiles, which is why the feed is its own
endpoint rather than a property of the summary (spec T.4).

**The personal-task slice rides inside the summary** rather than being a third endpoint — it is fixed-N,
wanted in the same paint, and saves a round trip.

---

## R-6 — Read-only posture: `CanMutateAsync`, `xmin`, and the audit catalog are **intentionally absent**

**Decision.** 005 implements no write path. Consequently:

| Contract | Status in 005 | Why |
|---|---|---|
| `ApplyScope` (§3) | ✅ used | the entire feature |
| `CanReadAsync` (§3) | ✅ used | single-entity reads where needed |
| **`CanMutateAsync`** (§3) | ❌ **not used anywhere** | nothing to authorize a mutation for |
| **`xmin`** (§5) | ❌ not applicable | nothing is updated |
| **`ETag`/`If-Match`** (ADR-0007 §3) | ❌ not applicable | no writes |
| **Audit catalog** (§6, IV.4) | ❌ **intentionally empty** | IV.4 audits *writes*; 005 has none, and reads are not audited |

**Rationale for recording absences explicitly.** Constitution IV.4 requires an audit entry for every write
to a domain entity. A reviewer scanning 005 for an audit catalog and finding none could reasonably flag it
as an omission. It is not: **the dashboard writes nothing**, so it has nothing to audit, and its own reads
are not audited — consistent with 001–004, which audit writes only. The activity the dashboard *displays*
was audited by whichever feature performed each change.

**This is the deliberate contrast with 006**, which is also read-only over domain data but writes exactly
one `ReportGenerated` audit row per generation. 005 writes **zero**. Stating both makes the difference
legible rather than looking like an inconsistency.

**A test asserts no write occurs** — exercising every endpoint and confirming `activity_logs` gains no row
(spec B.6/DoD 8).

---

## Resolved Technical Context unknowns

| Unknown | Resolution | Source |
|---|---|---|
| How is the audit log read? | `IActivityLogService.QueryScopedAsync` — **must be added to 001** | **R-1** ⚠️ |
| What timezone for overdue? | **UTC, fixed** — 005's "unless configured" contradicts 006's parity requirement | **R-2** ⚠️ |
| Does the dashboard ever 403? | **No.** Content-scoping; empty scope → 200 with zeros | R-3 |
| How are aggregates computed? | Un-materialized scope subquery + `GROUP BY` in SQL; enum maps zero-seeded | R-4 |
| What pages and what does not? | Feed pages; tiles and personal slice are fixed-N | R-5 |
| What about `CanMutateAsync`/`xmin`/audit? | Intentionally absent, recorded so absence ≠ omission | R-6 |
| Everything else | **Inherited** | §A |

**No unresolved NEEDS CLARIFICATION items remain**, but **R-1 and R-2 require amendments outside this
feature** — see plan.md §Follow-ups. Phase 1 may proceed.
