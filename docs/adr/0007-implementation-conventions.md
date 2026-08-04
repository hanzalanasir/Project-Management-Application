# ADR-0007: Repo-wide implementation conventions — contract drift gate, test database, concurrency transport, test fixtures, shared-kernel verification

**Status**: Accepted · **Date**: 2026-07-31 · **Amended**: 2026-08-04 (added §5, the shared-kernel
verification rule) · **Relates to**: Constitution IV.2, IX, X.2, X.3 · ADR-0004, ADR-0006 ·
**Supersedes nothing**

## Context

Planning 001, 002, and 003 produced four decisions that are **not feature-local**: each was derived once,
then reused unchanged by every later feature. They currently live only in `specs/00X/research.md` files —
which is ADR-shaped content sitting outside `docs/adr/`, contrary to Constitution X.3 ("significant
architectural decisions are recorded as short ADR files under `/docs/adr/`").

Each was deliberately *not* promoted at the time it was made, on the grounds that one feature is thin
evidence for a permanent record. That reservation no longer applies:

| Convention | Derived in | Confirmed to generalize by |
|---|---|---|
| Contract authoring + drift gate | 001 R-5 | 002 and 003 reused it verbatim, one contract file each |
| Testcontainers over EF InMemory | 001 R-7 | 002's scope-leak tests and 003's 15-cell matrix both depend on it |
| `ETag` / `If-Match` concurrency transport | 002 R-2 | 003 applied it to three endpoints and derived a principled exemption |
| Test fixtures from builders, not the production seeder | 002 FU-4, 003 FU-4 | Two features in a row needed fixture users the seeder does not provide |

This ADR records all four in one place rather than as three or four near-empty files, because they share a
single theme: **how the repo verifies itself.**

## Decision

### 1. The OpenAPI contract is hand-authored and enforced by a build-time diff

One contract per feature area at `docs/contracts/<area>.v1.yaml`, hand-written in **OpenAPI 3.0.3**,
reviewed and merged **before** the corresponding handler exists (Constitution X.2). CI emits the
code-derived document with `Swashbuckle.AspNetCore.Cli` and compares:

```bash
dotnet swagger tofile --output artifacts/openapi/generated.json <Api.dll> v1
oasdiff breaking docs/contracts/<area>.v1.yaml artifacts/openapi/generated.json --fail-on ERR
```

Wired as an MSBuild target gated on `-p:CheckApiContract=true`: **CI always sets it; local builds do not.**

- *3.0.3, not 3.1* — matches Swashbuckle's default emission, so the diff reports real differences rather
  than version-dialect noise.
- *`oasdiff`* — single cross-platform binary, reads YAML and JSON interchangeably, and distinguishes
  **breaking** from additive changes. That distinction is the point: adding an optional response field must
  not fail the build; removing a field or tightening a required set must.
- *CI-only by default* — a build → emit → diff cycle on every local `dotnet build` costs seconds of inner
  loop for a check that cannot reach `main` unnoticed anyway.

**The gate must be observed failing at least once per feature** (each feature's quickstart carries this
step). A gate never seen to fail is indistinguishable from a broken one.

### 2. Tests run against real PostgreSQL via Testcontainers — never EF InMemory

Every test that touches EF Core uses a **Testcontainers PostgreSQL** instance (one container per test run,
`Respawn` between tests). Tests with no database dependency — validators, token service, mapping
extensions — stay in-process.

**EF InMemory is prohibited in this repo**, because it cannot express the things this system's correctness
claims depend on:

- **`xmin` optimistic concurrency does not exist outside PostgreSQL.** ADR-0004's guarantee, and 002/003's
  409-on-stale tests, are unrepresentable.
- **Scope predicates must be proven to reach SQL.** 002 FR-007 and 003 FR-010 assert that out-of-scope rows
  are never loaded, counted, or paged. That is a claim *about generated SQL*. InMemory evaluates LINQ in
  memory, so a fetch-then-filter bug — the precise security defect those requirements guard against —
  would **pass** the suite.
- **Cascade and RESTRICT semantics** are relational-provider behaviour; InMemory has no referential
  integrity at all.

SQLite-in-memory was the strongest alternative (relational, fast, no Docker) and is rejected for the same
`xmin` gap plus the deeper objection that it introduces a **second provider**, admitting a class of bug
that appears only against the provider actually shipped.

**Cost accepted:** Docker is a prerequisite for the test suite.

### 3. Optimistic concurrency travels as `ETag` / `If-Match`, never as a body field

Entities carrying an `xmin` row version (`projects`, `tasks`, `users` — shared-contracts §5) expose it as a
strong **`ETag`** on reads. Mutating endpoints **require `If-Match`**:

| Case | Result |
|---|---|
| `If-Match` current | proceeds |
| `If-Match` stale | **409 Conflict** |
| `If-Match` absent | **400** (`"If-Match header is required."`) |

**`DELETE` is exempt.** Optimistic concurrency protects against *lost updates*; a delete has no lost-update
failure mode, and a second delete correctly observes 404.

Rationale:
1. A header applies uniformly across every write endpoint **without touching a single DTO**. 003 alone has
   three concurrent-write endpoints; a body field would have to be added to three request shapes and could
   be forgotten in any of them.
2. **Absent → 400 makes the check mandatory.** A body field a client omits degrades silently to
   last-write-wins, which ADR-0004 calls "never acceptable". A required header cannot be silently omitted.
3. It keeps an infrastructural concern out of the domain-facing DTO.

**400 rather than 428 Precondition Required** — deliberately, so no feature introduces a status code
outside the set Constitution VI.2 and the feature specs already declare. 428 is semantically better and may
be adopted repo-wide later; doing so would be a visible, contract-diffed change.

### 4. Test data comes from builders in the test projects, never the production seeder

Test fixtures are constructed by **builders/factories under `tests/**/Builders/`** (Constitution IX.4).
The production seeder (`IDataSeeder`, 001 US-006) provisions exactly **one user per role** and demo data —
that is a *product* concern, and it stays that way.

This is recorded because pressure ran the other way twice: 002's cross-owner denial test needs a **second
ProjectManager**, and 003's scope test needs a **second TeamMember** (its predicate is by *assignment*, not
membership, so proving a member sees only their own tasks requires two members on one project). Both are
test concerns. Growing the production seed to satisfy them would ship fixture accounts to production and
couple the seeder's contract to whichever assertions happen to exist.

### 5. Verify a shared-kernel member exists in its *owner's tasks* before citing it

**Rule.** Before a feature's plan or tasks cite a shared-kernel member — an interface, an enum, **or a
member of either** — that it needs but does not already see declared in `docs/shared-contracts.md`:

1. **Identify the owning feature** (almost always 001, which stands up the shared kernel).
2. **Open that feature's `tasks.md` and confirm a task actually creates it, exactly as needed** — not that
   the type exists, but that *the specific member you need* is in the task's description.
3. If it is missing, **patch the owner's spec/tasks and `shared-contracts.md` first.**
4. **Never add it locally.** A downstream feature that declares its own copy, or extends a shared enum from
   its own slice folder, breaks the boundary rule in ADR-0006's addendum and makes every later consumer
   depend on that feature's Application layer.

**Why this is a written rule rather than a habit: it has been rediscovered manually five times.**

| # | Missing member | Owner | Found during | Would have failed as |
|---|---|---|---|---|
| 1 | `TaskMutation` enum | 001 (T020) | 003 planning | `ITaskAccessPolicy` non-compiling in 001's own Foundational phase |
| 2 | `IActivityLogService.QueryScopedAsync` | 001 (T022/T033) | 005 planning | *"no legal way to build the activity feed"* — the workaround is forbidden by FR-006 |
| 3 | `IProjectAccessPolicy` · `ITaskAccessPolicy` · `ITeamAccessPolicy` | 001 (T022) | 005 planning sweep | Each feature declaring its own copy → 005/006 depending on 002/003's Application layer |
| 4 | `AuditAction` values for 002–006 | 001 (T018) | 006 pre-flight | 002's first audited write not compiling |
| 5 | `ErrorKind.UnprocessableContent` | 001 (T019/T036) | 006 `/speckit.analyze` | 006 extending a shared-kernel enum from its own feature |

Every one was **internally consistent** in the feature that found it — the gap only appeared when the
consumer was checked against the *owner's* task list. Four of the five were caught by a deliberate sweep or
audit; none by ordinary review.

**Mechanical check.** The sweep that catches these is one command per member — grep the owner's `tasks.md`
for the type name and read the surrounding task description:

```bash
grep -n '`YourType`' specs/001-auth-rbac/tasks.md      # must land on a CREATE/Define/Implement task
```

Note the two traps this rule exists to survive, both encountered for real:
- **Verification tasks are not creation tasks.** 001's T023 *checks* shared-kernel conformance; it creates
  nothing. Matching on it gives a false pass.
- **Prose mentions are not creation either.** `ITaskAccessPolicy` once matched only inside another task's
  explanatory sentence, reading as covered when no task created it.
- **Scope the sweep by artifact, not just by section.** The §2/§3/§6/§7 sweep missed `AuditAction` because
  it is a Domain enum, not a shared-contracts type. Enumerate **every type a downstream feature consumes**,
  wherever it lives.

**Corollary A — owners create the complete member set up front.** `AuditAction` holds all 18 actions across
001–006, and `ErrorKind` all seven kinds, even though each owner uses only some. This mirrors how 001
already creates `ProjectStatus`/`TaskStatus`/`TaskPriority` in full on behalf of 002/003. Consumers
**consume**; they never edit the owner's file.

**Corollary B — a shared member must also *live* in the shared kernel, not merely exist.**
*Added 2026-08-04 after instance #6 showed the rule above was insufficient.*

The four-step check asks "does the owner's task create it?" — which **passes** when a type exists but sits
in the wrong place. `MetricDefinitions` was created by 005's tasks, exactly as needed, in
`Features/Dashboard/Common/`; 006 was then told to import it *from there*. Every step above was satisfied,
and the result still violated ADR-0006's addendum, because a feature folder is a feature's Application
layer no matter how "common" its subfolder is named.

So the check has a **fifth step**:

5. **Confirm the member's *path* is shared-kernel** — `Application/Common/**`, declared in
   `docs/shared-contracts.md`. If two features must agree on it, `Features/<Anything>/Common/` is the wrong
   home, however tempting it is to leave it where it was first needed.

**Symptom to watch for:** a task that says *"import feature X's Y"*. That phrasing is the tell. A
shared-kernel import never needs to name a feature — it names a contract section.

| # | Member | Owner | Found during | Failure mode |
|---|---|---|---|---|
| 6 | `MetricDefinitions` | now 001 (T020), §8 | 005 `/speckit.analyze` (G1) | **Existed and was created correctly — but in 005's feature folder**, so 006's import crossed into another feature's Application layer |

## Alternatives considered

- **Three or four separate ADRs (0007–0010).** Rejected: each would be a few paragraphs, and they share one
  theme — repo self-verification. A reader looking for "how do we test / how do we stop drift" should find
  one file.
- **Leave the decisions in `research.md`.** Rejected: `specs/001-auth-rbac/research.md` is not where a
  reader looks for a repo-wide rule, and X.3 asks for `docs/adr/`. 004–006 would each re-derive or, worse,
  diverge.
- **Defer until 006.** Rejected: 004 is being planned next and would have to re-argue all four.

## Consequences

- **004–006 cite this ADR instead of re-deriving.** Their research files shrink to genuinely new decisions.
- The conventions become **checkable**: a feature that hand-writes an Angular service against an unwritten
  contract, uses InMemory, puts a row version in a request body, or extends the production seeder for a
  test is now violating a recorded decision rather than a habit.
- **Docker is a hard prerequisite** for running tests — stated in every quickstart.
- If 428 is ever adopted for missing `If-Match`, this ADR is amended and the contracts re-diffed. That is
  the intended change path.

## Backward compatibility

None required: no code exists yet. All four conventions are already reflected in 001–003's plans, research,
contracts, and quickstarts, so this ADR **records** current planning rather than changing it. The only
edits it triggers are the Follow-up closures in those three plans.
