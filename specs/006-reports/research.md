# Phase 0 Research: 006 Reports

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Date**: 2026-07-31
**Constitution**: v1.3.0 · **Inherits**: [001](../001-auth-rbac/research.md) ·
[002](../002-projects/research.md) · [003](../003-tasks/research.md) · [004](../004-team/research.md) ·
[005](../005-dashboard/research.md) · [ADR-0007](../../docs/adr/0007-implementation-conventions.md)

> **Scope.** Final feature. Everything structural is inherited; §B derives six decisions specific to
> parameterized reporting and export. 006 is the **second read-only feature**, but unlike 005 it writes
> exactly one thing — and the difference between "writes nothing" and "writes one audit row" is one of the
> decisions recorded here.

---

## §A — Inherited (cited, not re-decided)

| Decision | Source | Applies to 006 as |
|---|---|---|
| Four-assembly layout; slices in `.Application` | 001 R-1 | `Features/Reports/<UseCase>/` — **query-only**, no new project |
| `IApplicationDbContext`, direct LINQ, no repository | 001 R-3 · shared-contracts §7 | Load-bearing — aggregates compose over `IQueryable` |
| MediatR pipeline `Logging → Validation → Handler` | 001 R-4 | Reused; 006 adds parameter validators |
| Contract hand-authored 3.0.3 + `oasdiff` gate | 001 R-5 · ADR-0007 §1 | One file, `docs/contracts/reports.v1.yaml`, covering catalog + four reports |
| Testcontainers PostgreSQL; InMemory prohibited | 001 R-7 · ADR-0007 §2 | Mandatory — parity and filter-at-source are claims about SQL |
| `Result` → `ActionResult` in one mapper | 001 R-8 | Reused verbatim |
| Access policies consumed, not implemented | 002 R-1 · 005 §A | 006 uses `ApplyScope`; implements no policy |
| Pagination by nature of the collection | 004 R-4 · 005 R-5 | Activity Report pages; catalog and the three bounded reports do not |
| **`IActivityLogService.QueryScopedAsync`** | **005 R-1** (added to 001) | The Activity Report's only legal read path — **now exists** |
| **Overdue evaluated in UTC, fixed** | **005 R-2** (corrected in 005) | Precondition for R-4's parity requirement |
| Angular Material; Chart.js; generated DTO types | 001 R-6 · 005 | `reports` route group |
| Cross-feature deps via shared abstractions or Domain entities only | ADR-0006 addendum | 006 reads `projects`/`tasks`/`team_members` entities directly; calls no feature's handlers |

**Nothing in §A is reopened.** Two of these — `QueryScopedAsync` and fixed-UTC — were **gaps closed during
005's planning pass specifically so 006 would not inherit them**.

---

## §B — New decisions for 006

## R-1 — The one audit write: a read-side feature that writes, deliberately

**Decision.** Each successful report **data** request writes **exactly one** `activity_logs` row via
`IActivityLogService.LogAsync`:

```
actor_id = caller · action = ReportGenerated · entity_type = 'Report'
entity_id = <generated run id> · timestamp · change_summary = <report type + serialized parameters>
```

The **catalog endpoint is not audited** (metadata only, exposes no data). No domain entity is touched.

**Rationale.** Constitution IV.4 requires auditing *domain writes*; report generation is not one. But *"who
ran the team-performance numbers last quarter, with what parameters"* is a **security-relevant access
event**, and the audit log is the only place that record belongs. This is the single deliberate exception
in the product to "read features don't write" — and it is stated three times (spec Purpose, T.1, B.7)
precisely so it reads as intentional rather than as a leak in the read-only posture.

**The contrast with 005 is the point.** Both are read-only over domain data; 005 writes **zero** audit rows,
006 writes **one per generation**. Stating both makes the difference legible instead of looking like an
inconsistency between sibling features.

**Two implementation consequences worth naming:**
1. **`entity_id` must accept a non-uuid value.** It is `varchar(64)` and 001's data-model already
   anticipated this — *"string, not uuid, so a logical target like `Report` (006) fits"*. No change needed.
2. **The audit row is the *only* thing in the transaction.** `LogAsync` writes into the caller's unit of
   work (001 T033), and a query handler has no other change pending — so `SaveChangesAsync` commits one
   row. That is correct, not a smell; it just means the "same transaction as the change" phrasing in IV.4
   degenerates here because there is no change.

**Mild reflexivity, accepted:** running the Activity Report writes a `ReportGenerated` row that will appear
in *future* Activity Reports. Correct behaviour, and called out in the spec so it is not reported as a bug.

**Alternatives considered.**
- *Audit only on explicit export* — rejected as the default (`Reports:AuditOnGeneration` keeps it
  configurable): the security-relevant fact is that the **data was produced**, and export is a client-side
  render of data already released.
- *Log to Serilog instead of `activity_logs`* — rejected: the organization's answer to "who accessed what"
  should live with every other audit record, queryable by the Activity Report itself.

---

## R-2 — Resource-per-report-type endpoints; the catalog drives the UI

**Decision.** Five `GET`s: `/api/reports/catalog` plus one per report type. **No** `POST /api/reports
{type, params}` discriminator, and **no** `?format=` parameter.

**Rationale.** Each report has a genuinely **distinct typed parameter contract** — Project Progress takes a
project scope; Task Completion adds `groupBy`; Team Performance adds `userId`; Activity adds
`entityType`/`actorId`/paging. Collapsing them into one polymorphic body would produce exactly the
weakly-typed payload that 005's "stable typed contract, not a dictionary" principle rejects, and would make
the OpenAPI contract — and therefore the drift gate — nearly useless for these endpoints.

The **catalog** solves the resulting UI problem: rather than hard-coding four parameter forms, the frontend
builds them from descriptors (`type`, `title`, ordered `parameters` with name/type/required, supported
`formats`). Adding a fifth report later extends the catalog without a contract change to existing routes.

**The catalog is role-annotated** — a TeamMember sees Team Performance marked "self only" — but it exposes
no project or task data, which is why it is unaudited (R-1).

**Alternatives considered.**
- *Single `POST /api/reports` with a discriminator* — rejected above; also wrong semantics (a report
  generation is a read, not a creation).
- *Server-side `?format=pdf|csv`* — rejected, see R-3.

---

## R-3 — Export is entirely client-side; the API returns JSON only

**Decision.** The API returns **JSON only**. PDF is rendered client-side with **jsPDF**, CSV with a
**papaparse**-based utility, both from the *same* JSON the on-screen preview already fetched, both behind
the single Angular **`ReportExportService`** (Constitution VII.8 — export logic in a service, never per
component). There is **no export endpoint and no `?format` behaviour on the API**.

**Rationale.**
- **PDF is client-side by mandate.** Constitution III locks **jsPDF**, a browser library. The only way to
  honour that lock is to render in the browser; a server-side PDF engine would be a stack deviation
  requiring an amendment.
- **CSV follows for consistency**, though a server-side CSV was defensible. Keeping both in one
  `ReportExportService` means PDF and CSV are two *representations of one JSON payload*, which is exactly
  the brief's framing ("alternate representations of the same data, not separate endpoints"). Two code
  paths would invite the two formats to drift.
- **No new backend export infrastructure** — no streaming writer, no temp files, no content negotiation.

**The bounded-window guard is what makes this safe** (R-5): client-side rendering is only viable because no
report can return an unbounded row set.

**Designed-for, not built:** a future `?format=csv` server representation could be added without changing
the JSON contract if a genuine need appears.

**Alternatives considered.**
- *Server-side PDF (QuestPDF/wkhtmltopdf)* — rejected: contradicts III's locked library.
- *Server-side CSV streaming* — rejected for v1 (OQ-006-02 resolved to forced narrowing instead); it would
  be the fallback if the threshold ever proves too restrictive.

---

## R-4 — Value parity with 005 is a **shared definition**, not a coincidence

**Decision.** Metrics appearing in both surfaces — overdue count, open/closed counts — are computed from
**the same predicates and the same definitions** 005 uses, and a **cross-feature integration test** asserts
the numbers are identical for the same caller and window (006 NFR-002).

The shared definitions, fixed:

| Concept | Definition | Shared with |
|---|---|---|
| **closed** | `status = Done` (equivalently `closed_at` not null) | 003, 005 |
| **re-opened** | `closed_at` cleared → **excluded** from closed counts *and* Task Completion buckets | 003 |
| **overdue** | `due_date < today (UTC)` **and** `status != Done` | 005 |
| **today** | **UTC, fixed** | 005 (corrected in its planning pass) |
| **throughput** | tasks whose `closed_at` falls in the window | 003's `closed_at` |

**Rationale.** A manager seeing one overdue count on the dashboard and a different one in the report loses
confidence in both. The only durable way to prevent it is to make the definitions *shared* rather than
*similar* — which is why 005's configurable-timezone wording had to be fixed before this feature could be
planned (005 R-2). Had it survived, this requirement would have been breakable by configuration.

**The re-open rule is applied uniformly**, deliberately: the same exclusion governs Project Progress's
`closedTasks` and Task Completion's buckets. Divergent counting between two reports in the same feature
would be a worse inconsistency than 005/006 divergence.

**Alternatives considered.**
- *Extract the metric expressions into a shared helper consumed by both features* — **adopted 2026-08-04**,
  having originally been deferred here. The deferral was wrong on its own terms: this file said the
  cross-feature test alone would suffice, but T014 simultaneously instructed 006 to *import 005's*
  `MetricDefinitions` — which is extraction **without** relocation, giving the coupling without the shared
  home. `/speckit.analyze` flagged it as **G1**: importing `Features/Dashboard/Common/` makes 006 depend on
  005's Application layer, forbidden by ADR-0006's addendum.

> **✅ RESOLVED 2026-08-04.** `MetricDefinitions` is now a **shared-kernel** member — declared in
> [`docs/shared-contracts.md` §8](../../docs/shared-contracts.md), created by **001 T020** in
> `Application/Common/Metrics/`, and **imported** by both 005 (T012) and 006 (T014). Parity is now
> guaranteed **by construction**; T083 remains as the regression check rather than the sole guarantee.

---

## R-5 — The large-window guard: **422**, forced narrowing, checked before rendering

**Decision.** Before materializing any report, the handler estimates the result-set size. Exceeding
`Reports:LargeReportRowThreshold` (**default ~10,000 rows**) returns **422 Unprocessable Content** with a
"narrow the date range" message. Applies principally to the Activity Report.

**Rationale.** The entire export pipeline is client-side (R-3), so an unbounded result set is not merely
slow — it would attempt to render tens of thousands of rows into a PDF **in the user's browser**. Refusing
early with an actionable message is strictly better than a frozen tab. The guard fires on the **data
request**, before any client render is attempted.

### On introducing 422 — a status code Constitution VI.2 does not list

VI.2 enumerates 200/201/204/400/401/403/404/409/500. **422 is absent**, and this needs an explicit
justification because **002 R-2 declined 428 for precisely that reason**.

The two cases are genuinely different, and the distinction is the rule worth keeping:

| | 002's 428 | 006's 422 |
|---|---|---|
| Origin | **Invented at plan time** | **Declared in the spec** (API catalog, B.5) and clarified via OQ-006-02 |
| Reviewed? | No — a plan-time convenience | Yes — a resolved clarification with recorded rationale |
| Verdict | **Rejected** — a plan must not introduce status codes | **Accepted** |

001's Clarifications already established that VI.2's list is *"representative, not exhaustive, and does not
prohibit 409"* — the same reasoning that admitted 409 admits a spec-declared 422. **A plan may not invent a
status code; a spec may declare one.** Recorded in plan.md Complexity Tracking.

### How the size is estimated (added 2026-08-04, closing analyze finding C1)

The guard's mechanism was left as "estimate the result-set size", which is not implementable as written.
**Decision: an indexed `COUNT()` over the scoped **and** filtered query — the same predicate the real query
would use — executed before any paging or projection.**

```csharp
var scoped = /* scope + window + entityType/actorId/projectId filters */;
var estimated = await scoped.CountAsync(ct);          // ← the guard
if (estimated > options.LargeReportRowThreshold) return Result.Failure(ErrorKind.UnprocessableContent, …);
```

**Rationale.** It is exact rather than heuristic, so the threshold means what it says, and it reuses the
predicate already built — no second code path to keep in sync. **Cost:** one extra round trip that
PostgreSQL answers from the same indexes the real query uses (`activity_logs(timestamp)`,
`(entity_type, entity_id)`, `(actor_id)`). That is **orders of magnitude cheaper than the thing it
prevents** — materializing ~10,000 rows, serializing them, and handing them to a browser-side jsPDF render.

A `COUNT()` is not free on a very wide window, and that is accepted: the alternative is doing the expensive
thing *and then* discovering it was too big.

**Alternatives considered for the estimator.**
- *Window-size heuristic* (e.g. days × observed average rows/day) — cheap and index-free, but approximate in
  both directions: it would reject satisfiable reports and admit oversized ones. Rejected — a guard whose
  threshold is advisory is not a guard.
- *`EXPLAIN` row estimates* — uses planner statistics, so it is fast but can be badly wrong on skewed data
  and couples the application to PostgreSQL's stats freshness. Rejected.
- *Fetch `threshold + 1` rows and check* — accurate and avoids a second query, but it **materializes up to
  the threshold** on every large request, which is most of the cost the guard exists to avoid. Rejected.

**Alternatives considered (for 422 itself).**
- *400 instead of 422* — semantically wrong: the request is well-formed and the parameters are individually
  valid; it is the *result size* that is unprocessable. 400 would conflate it with a malformed window.
- *Truncate silently at N rows* — rejected outright: a report that silently omits data is worse than one
  that refuses, especially for an audit-facing report.
- *Server-side streaming* — deferred (OQ-006-02); would sit behind the same threshold config.

---

## R-7 — `projectScope` accepts a **comma-separated list**, not just one id

*(Added 2026-08-04, closing analyze finding F1.)*

**Decision.** `projectScope` accepts either the literal `all` **or a comma-separated list of project ids**
(a single id being a list of one). If **any** named id lies outside the caller's scope, the request returns
**403 as a whole** — it is never partially fulfilled. A malformed id or an empty list returns **400**.

**Rationale.** The contract had typed this as a bare string, but the spec asks for a list in **two
independent places**: FR-002 (*"a project id, **a list**, or 'all visible'"*) and the catalog descriptor in
T.3 (`{"name":"projectScope","type":"projectIds|all"}` — **plural**). Consistent intent stated twice is a
requirement, not aspirational wording; the contract was the outlier. CSV is the encoding already used for
comparable multi-value parameters in this repo and needs no schema gymnastics.

**Why all-or-nothing on 403.** Silently dropping an out-of-scope id would return a report that *looks* like
it covers everything requested but quietly does not — the most dangerous failure mode for a document
someone files or presents. Failing the whole request preserves FR-004's "named out-of-scope → 403"
semantics unchanged: naming a resource you may not have is refused, whether you named one or five.

> **Note:** no user story, Given-When-Then, edge case, or DoD item exercises the multi-id case — which is
> precisely why the contract drifted to a single string without anyone noticing. The gap was in
> *validation*, not intent. T033 now covers it.

**Alternatives considered.**
- *Drop "a list" from FR-002 and keep a single id* — simplest, and defensible since nothing tests it.
  Rejected: it would contradict the catalog descriptor too, so two spec edits to remove a capability the
  spec asks for twice.
- *Repeated query parameter* (`?projectScope=a&projectScope=b`) — equally valid HTTP and avoids a delimiter,
  but inconsistent with how this repo's other multi-value parameters read, and awkward alongside the
  literal `all`.
- *Partial fulfilment with a warning field* — rejected; see the all-or-nothing rationale above.

---

## R-6 — TeamMember self-only returns their own row, **not 403**

**Decision.** For **Team Performance**, a TeamMember always receives **exactly one row — their own** —
regardless of any `userId` parameter supplied. Passing a colleague's `userId` does **not** return 403; it
returns the caller's own row.

**Rationale.** This looks inconsistent with 006's *named-out-of-scope → 403* rule (which applies to
`projectId` and, for Admin/PM, `userId`) — and the inconsistency is deliberate and is the **safer**
behaviour.

> A **403** on a colleague's `userId` would confirm that the colleague exists and is outside the caller's
> scope. For a *peer-comparison* report, that is exactly the inference a TeamMember should not be able to
> draw. Silently returning their own row leaks nothing.

The wider rule still holds: for **Admin and ProjectManager**, a named out-of-scope `userId` **does** return
403 — they are permitted to know their own scope boundary. The self-only clamp is a least-privilege rule
specific to the one role that must never see peer throughput.

This mirrors 003 R-4's filter behaviour (a TeamMember filtering by another assignee gets an **empty page**,
not 403) — same principle: **for the least-privileged role, silence beats a denial that confirms
existence.**

**Alternatives considered.**
- *403 for a TeamMember naming a colleague* — rejected: confirms existence, which is the leak.
- *400 "you may not filter by user"* — rejected: also confirms nothing but is needlessly hostile, and makes
  a legitimate client (which may send the caller's own id) fail.

---

## Resolved Technical Context unknowns

| Unknown | Resolution | Source |
|---|---|---|
| How is report access recorded? | One `ReportGenerated` audit row per data request; catalog unaudited | R-1 |
| Endpoint shape | Resource-per-report-type + catalog; no discriminator, no `?format` | R-2 |
| Where does export happen? | Entirely client-side (jsPDF + papaparse) in one `ReportExportService` | R-3 |
| How is parity with 005 guaranteed? | Shared definitions + a cross-feature test; enabled by 005's fixed-UTC correction | R-4 |
| What bounds a large report? | ~10,000 rows → **422**, forced narrowing, checked before render | R-5 |
| TeamMember naming a colleague? | Returns their **own** row — never 403 (which would confirm existence) | R-6 |
| Reading the audit log | `IActivityLogService.QueryScopedAsync` — **exists**, added during 005's pass | §A |
| Everything else | **Inherited** | §A |

**No unresolved NEEDS CLARIFICATION items remain.** All six OQ-006-01..06 were resolved in the spec's
Clarifications. Phase 1 may proceed.
