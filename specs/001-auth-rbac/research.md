# Phase 0 Research: 001 Auth & RBAC (Foundational)

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Date**: 2026-07-31
**Constitution**: v1.3.0 · **Governs 002–006**: every decision here is the project-wide default; later
features cite this file rather than re-deciding.

> Format per decision: **Decision** → **Rationale** → **Alternatives considered**.
> All Technical Context unknowns are resolved here; none remain marked NEEDS CLARIFICATION.

---

## R-1 — Solution layout: one `.csproj` per Clean Architecture layer

**Decision.** Four source projects — `ProjectManagementApp.Domain`, `.Application`, `.Infrastructure`,
`.Api` — plus the Angular workspace at `src/ProjectManagementApp.Web/`, and three test projects
(`.Application.Tests`, `.Infrastructure.Tests`, `.Api.Tests`). Vertical-slice folders
(`Features/Auth/<UseCase>/`) live **inside `.Application`**, exactly as spec 001 B.3 lays out.

Project reference graph (compile-time enforcement of Constitution II.2's inward rule):

```
Domain          → (no project references)
Application     → Domain
Infrastructure  → Application, Domain
Api             → Application, Infrastructure, Domain     ← composition root only
```

**Rationale.** II.2 mandates that "Domain depends on nothing; a slice's handler (Application) depends
only on Domain and on abstractions it defines; Infrastructure implements those abstractions." Separate
assemblies make that a **compiler error** rather than a code-review convention — the single cheapest
enforcement mechanism available. `Api → Infrastructure` is the standard composition-root exception: only
`Program.cs` touches Infrastructure types, to register DI.

**Alternatives considered.**
- *Single project with folders* — cheaper to set up, but the dependency rule becomes unenforceable; a
  handler could reference `DbContext` concretely and nothing would fail. Rejected: the rule is the point.
- *Slices as their own assemblies* (`Features.Auth.csproj`) — maximum isolation, but 6 features × N
  slices is assembly sprawl for a six-entity app, and cross-slice shared kernel access gets awkward.
  Rejected as over-engineering.

**No `Domain.Tests` project.** 001's Domain is entities + enums with no behavior; a test project would
be empty. **Add `ProjectManagementApp.Domain.Tests` the first time Domain gains a behavioral rule** —
the nearest candidate is 003's `closed_at` set/clear invariant if it is modeled as a domain method.

---

## R-2 — ASP.NET Core Identity entities vs. "Domain depends on nothing"

**Decision.** `ApplicationUser : IdentityUser<Guid>` and `ApplicationRole : IdentityRole<Guid>` live in
**`.Domain`**, and `.Domain` takes a single package reference on **`Microsoft.Extensions.Identity.Stores`**
(not `Microsoft.AspNetCore.Identity`, not EF Core, not ASP.NET Core).

**Rationale.** This is the sharpest tension in the whole plan and it deserves a recorded answer.
Constitution III locks ASP.NET Core Identity for user/role management, while II.2 says Domain depends on
nothing. Two facts force the resolution:
1. **IV.3 requires navigation properties** ("Relationships are modeled with foreign keys *and navigation
   properties*"). `Project.Owner`, `TaskItem.Assignee`, and `TeamMember.User` are Domain-to-Domain
   navigations — so the user entity **must** be visible from Domain. Putting it in Infrastructure would
   force every Domain entity to hold a bare `Guid OwnerId` with no navigation, violating IV.3.
2. `Microsoft.Extensions.Identity.Stores` is a **storage- and framework-agnostic abstraction package**
   (it contains `IdentityUser<TKey>`, `IdentityRole<TKey>`, and the store interfaces). It pulls in no web
   stack, no EF Core, no database. The dependency is on a POCO shape, not on infrastructure.

This is a **narrow, documented deviation** from a literal reading of "depends on nothing" and is recorded
in plan.md's Complexity Tracking table.

**Alternatives considered.**
- *Pure Domain `User` + separate Infrastructure `ApplicationUser`, mapped* — the textbook-purist answer.
  Rejected: it creates two user identities to keep in sync, breaks IV.3 navigations, and adds a mapping
  tax to all six features for a purity win that no requirement asks for.
- *`ApplicationUser` in Infrastructure, `Guid` FKs only in Domain* — rejected for the IV.3 reason above.
- *Drop Identity, hand-roll auth* — rejected outright: III locks Identity, and V.3 wants its hasher.

---

## R-3 — How an Application handler reaches the DbContext (and why this is not a Repository)

**Decision.** `.Application` defines `IApplicationDbContext` exposing the `DbSet<T>` properties and
`SaveChangesAsync`; `.Infrastructure`'s `ApplicationDbContext` implements it. Slice handlers inject
`IApplicationDbContext` and **compose LINQ against `DbSet<T>` directly**.

> **Anchored in the shared kernel (2026-07-31).** This interface is now declared in
> **[`docs/shared-contracts.md` §7 — Persistence access](../../docs/shared-contracts.md)**, with the same
> shared-kernel status as `ICurrentUserService` (§2), the access policies (§3), and `IActivityLogService`
> (§6). Features 002–006 depend on it directly and do **not** re-derive how a handler crosses the
> Application/Infrastructure boundary. Conformance is verified by task **T023** in Foundational.

```csharp
public interface IApplicationDbContext {
    DbSet<ApplicationUser> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<Project> Projects { get; }        // present from InitialCreate; owned by 002
    DbSet<TaskItem> Tasks { get; }          // owned by 003
    DbSet<TeamMember> TeamMembers { get; }  // owned by 004
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

**Rationale.** This satisfies both constraints simultaneously. IV.1 says "a slice's handler MAY call the
DbContext directly as its default persistence path; a separate Repository is optional, not required" —
and it does: the handler writes `_db.Users.Where(...)`, full LINQ, no abstraction over queries. II.2 says
Application may depend only on abstractions it defines — and `IApplicationDbContext` is exactly that.

**This is explicitly *not* a Repository**, and a reviewer should not flag it as one. A repository
abstracts *queries* behind named methods (`GetActiveUsersAsync()`), hiding the query model. This
interface hides nothing: it exposes `IQueryable` surface identically to the concrete context. Its only
job is to point the compile-time dependency arrow inward. `.Application` still references `EFCore` (for
`DbSet`/LINQ) — accepted and noted in Complexity Tracking.

**Critically, `ApplyScope` needs this.** `IProjectAccessPolicy.ApplyScope(IQueryable<Project>, …)` folds a
predicate into a live `IQueryable` (shared-contracts §3). That is impossible behind a repository that
returns materialized lists — the scope filter would move into memory and out-of-scope rows would be
loaded, breaking 002 FR-007 / NFR-002. The no-repository decision is therefore **load-bearing for a
security property**, not a style preference.

**Alternatives considered.**
- *Inject `ApplicationDbContext` concretely into handlers* — simplest, but Application→Infrastructure
  reverses the dependency arrow. Rejected.
- *Repository + Unit of Work* — already rejected in ADR-0006; additionally breaks `ApplyScope` as above.

---

## R-4 — MediatR pipeline: behaviors, order, and registration

**Decision.** Two open-generic behaviors registered in `.Application`, in this order:

```
Request → LoggingBehavior<TReq,TRes> → ValidationBehavior<TReq,TRes> → Handler
```

- **`LoggingBehavior`** — outermost. Opens a Serilog `LogContext` scope with `RequestName`, `UserId`
  (from `ICurrentUserService`, `null` when anonymous), and `CorrelationId`; stopwatches the inner call
  and logs `Completed {RequestName} in {ElapsedMs}ms` (or the failure `ErrorKind`). Never logs the
  request body — `RegisterCommand`/`LoginCommand` carry plaintext passwords (V.3, NFR-003).
- **`ValidationBehavior`** — inner. Resolves `IEnumerable<IValidator<TRequest>>`, runs them, and on
  failure **short-circuits with `Result.Failure(ErrorKind.Validation, fields)` — it does not throw.**
  Handlers therefore never call a validator, per your directive.

**Rationale for the order.** Logging outermost means a request rejected by validation is still logged
with its duration — otherwise validation failures become invisible in telemetry, which is precisely the
class of failure you most want to see. Validation inside means the handler is only ever entered with a
structurally valid request.

**Rationale for returning rather than throwing.** ADR-0003 states services "never throw for expected
outcomes; they return a `Result`". A validation failure is the most expected outcome there is. Throwing
`ValidationException` and catching it in middleware would work, but it routes an expected result through
the exception path and splits error mapping across two mechanisms.

**Behaviors requiring a non-generic response.** `ValidationBehavior` must construct a failed
`TResponse`. Constrain it to `where TResponse : Result` and use the static factory — every request in
this codebase returns `Result` or `Result<T>`, so the constraint holds and is enforced at compile time.

**Alternatives considered.**
- *Validation outermost* — rejected; validation failures would escape the logging scope.
- *`ValidationException` + middleware* — rejected per ADR-0003 (see above).
- *Manual `_validator.ValidateAsync()` in each handler* — rejected by your directive and because it is
  forgettable; a behavior cannot be forgotten.
- *Adding a `PerformanceBehavior` / `UnhandledExceptionBehavior` now* — deferred. Serilog request logging
  plus the global exception middleware cover it; add only if a real need appears.

---

## R-5 — API-first contract authoring and drift detection (X.2)

**Decision.** Three parts:

1. **Authoring.** The contract is hand-written YAML at **`docs/contracts/auth.v1.yaml`**, in
   **OpenAPI 3.0.3**, reviewed and merged *before* any handler is implemented. One file per feature area
   (`auth.v1.yaml`, then `projects.v1.yaml`, `tasks.v1.yaml`, …).
2. **Generation.** `Swashbuckle.AspNetCore.Cli` (as a local dotnet tool) emits the code-derived document:
   `dotnet swagger tofile --output artifacts/openapi/generated.json <Api.dll> v1`.
3. **Diffing.** **`oasdiff`** compares authored vs. generated and fails on breaking changes:
   `oasdiff breaking docs/contracts/auth.v1.yaml artifacts/openapi/generated.json --fail-on ERR`.

Wired as an MSBuild target in `ProjectManagementApp.Api.csproj`:

```xml
<Target Name="CheckApiContract" AfterTargets="Build" Condition="'$(CheckApiContract)' == 'true'">
```

**CI always sets `-p:CheckApiContract=true`; local builds do not by default.** Developers opt in with
`dotnet build -p:CheckApiContract=true`, and there is a `npm run check:contract`-style script documented
in quickstart.md.

**Rationale.**
- *OpenAPI 3.0.3, not 3.1* — Swashbuckle's default emission is 3.0.x. Authoring in the same version means
  the diff reports **real** differences instead of version-dialect noise. Revisit if Swashbuckle's 3.1
  output stabilizes.
- *`oasdiff`* — a single cross-platform Go binary, reads YAML and JSON interchangeably, and has
  first-class *breaking-change* classification with exit codes (`--fail-on ERR`). That distinction
  matters: adding an optional response field should not fail the build; removing a field or tightening a
  required set should.
- *CI-only by default* — running a build → emit → diff cycle on every local `dotnet build` would add
  seconds to the inner loop and require the tool installed on every machine. The build that gates merges
  is the one that must be authoritative.

**Alternatives considered.**
- *Generate the contract from code (Swashbuckle as source of truth)* — this is exactly the direction
  X.2 reversed, and ADR-0006 rejected it. Not reconsidered.
- *Generate server stubs from the YAML (openapi-generator aspnetcore)* — rejected: its ASP.NET output
  fights the vertical-slice layout and MediatR wiring. We hand-write thin controllers and *verify* them
  against the contract instead. (Note: the **frontend** does generate from the contract — see R-6.)
- *`openapi-diff` (OpenAPITools, Java)* — equivalent capability, but a JRE dependency on a .NET/Node
  toolchain. Rejected on toolchain weight.
- *Spectral* — a linter, not a differ. Complementary, not a substitute; may be added later for style.

---

## R-6 — Frontend: Angular Material, workspace location, and contract-generated services

**Decision.** Angular Material as the UI library (Constitution III's stated default, committed now —
Bootstrap MUST NOT also be introduced, III forbids mixing). The workspace lives at
`src/ProjectManagementApp.Web/`. Production build output is emitted into the API's `wwwroot/`, and
`ng serve` proxies `/api` to the API — the same-origin arrangement ADR-0002 requires so the refresh
cookie can stay `SameSite=Strict` in every environment.

Per VII.3 (as amended in v1.3.0), the typed HTTP client for `ProjectManagementApp.Api` **may be generated
from `docs/contracts/auth.v1.yaml`** via `openapi-generator` (`typescript-angular`) into
`src/app/core/api/generated/`. **For 001 the generated client is adopted for DTO *types* only**; the
hand-written `AuthService` wraps it, because the refresh flow needs `withCredentials`, single-flight
de-duplication, and NgRx dispatch that a generated client does not express.

**Rationale.** Generating the whole service layer sounds appealing but the auth service is precisely the
one service with non-mechanical behavior (cookie transport, single-flight refresh, interceptor
interplay). Taking the generated *types* removes the DTO drift risk — the actual failure mode — without
surrendering control of the behavior. 002–006's services are mechanical CRUD and can adopt generated
clients more fully; that is re-evaluated per feature.

**Alternatives considered.**
- *Fully hand-written client* — rejected: DTO drift is the exact risk X.2 exists to kill.
- *Fully generated client incl. auth* — rejected for the behavioral reasons above.
- *Bootstrap* — permitted by III as an alternative but rejected: Material has first-party Angular 22
  support, CDK a11y primitives, and a form-field/error idiom that matches VII.6's shared error-display
  component requirement.

---

## R-7 — Test strategy: real PostgreSQL via Testcontainers wherever EF is involved

**Decision.**
- **`.Application.Tests`** — xUnit. Handler tests run against **real PostgreSQL** through a shared
  Testcontainers fixture. Pure unit tests with no database (validators, `ITokenService` behavior, mapping
  extensions) run in-process with no container.
- **`.Infrastructure.Tests`** — Testcontainers PostgreSQL: migrations apply cleanly, Npgsql type mapping,
  `xmin` concurrency, cascade/restrict behavior, seed idempotency.
- **`.Api.Tests`** — `WebApplicationFactory` (IX.1) over the same Testcontainers PostgreSQL: full HTTP
  stack, auth handshake, the 401/403 matrix, ProblemDetails shape.
- Isolation between tests: `Respawn` to truncate between tests; one container per test run via a shared
  `ICollectionFixture`.
- Test data via builders/factories (IX.4), never inline literals.

**Rationale — this is the most consequential testing call in the plan.** The tempting cheap option, the
EF Core **InMemory provider, cannot express the things this system's correctness depends on**:
- **`xmin` optimistic concurrency (ADR-0004) does not exist** outside PostgreSQL. 002 DoD #10 requires an
  integration test proving a stale write yields 409. InMemory cannot produce it.
- **Scope predicates must be proven to translate to SQL.** 002 FR-007/NFR-002 assert out-of-scope rows are
  never loaded, counted, or paged. That is a claim *about the generated SQL*. InMemory evaluates LINQ in
  memory, so a fetch-then-filter bug — the precise security bug the requirement guards against — would
  **pass** the test suite. This alone decides it.
- **Cascade / RESTRICT semantics** (`ON DELETE CASCADE` to tasks/team_members, `RESTRICT` on owner) are
  relational-provider behavior; InMemory has no referential integrity at all.

Fidelity here is not gold-plating; it is the difference between a test suite that can and cannot detect a
data-leak regression.

**Cost accepted:** Docker becomes a prerequisite for running the test suite, and the suite is slower than
in-memory. Mitigated by one container per run (not per test) and by keeping DB-free unit tests DB-free.

**Alternatives considered.**
- *EF Core InMemory* — rejected for the three reasons above; it is unsound for this codebase's assertions.
- *SQLite in-memory* — relational, supports FKs, fast, no Docker. Genuinely tempting, and rejected: no
  `xmin`, differing collation/case-sensitivity semantics (email normalization is a correctness concern
  here), and it introduces a **second provider**, meaning a class of bug that only appears against the
  provider you actually ship on. Two-provider divergence is a known-bad trade.
- *A shared developer database* — rejected: tests become order-dependent and mutually destructive.

---

## R-8 — Result → HTTP mapping, and where `ICurrentUserService` is implemented

**Decision.** A single `ToActionResult()` extension in `.Api/Common/` converts `Result`/`Result<T>` to
`ActionResult` with an RFC 7807 body, implementing shared-contracts §1's mapping table verbatim. It is
**the only place** that mapping exists. Thin controllers read:

```csharp
[HttpPost("register"), AllowAnonymous]
public async Task<IActionResult> Register(RegisterCommand command, CancellationToken ct)
    => (await _mediator.Send(command, ct)).ToActionResult(onSuccess: 201);
```

`ICurrentUserService` (shared-contracts §2) is implemented in **`.Api/Services/CurrentUserService.cs`**,
backed by `IHttpContextAccessor`, registered scoped.

**Rationale.** Placing `CurrentUserService` in Api keeps `IHttpContextAccessor` — an HTTP concern — out of
Infrastructure entirely; Infrastructure stays about persistence and external systems. Application depends
only on the interface, so nothing about the layering changes.

**Genuine exceptions** (real bugs, infrastructure failures) are caught by exception-handling middleware
and emitted as a 500 ProblemDetails — never as a `Result`.

**Alternatives considered.**
- *Controllers building `ActionResult`s inline* — rejected: duplicates the mapping and lets status codes
  drift per endpoint, which is the thing shared-contracts §1 exists to prevent.
- *`CurrentUserService` in Infrastructure* — workable, but drags HTTP abstractions into the persistence
  assembly for no gain.

---

## R-9 — Seeding: where it runs and how it stays idempotent

**Decision.** `IDataSeeder` is defined in `.Application/Common/Interfaces/`, implemented in
`.Infrastructure/Services/DataSeeder.cs`, and invoked from `Program.cs` **after** `db.Database.MigrateAsync()`
inside a startup scope, gated by `Seed:Enabled` (default `true` in Development, `false` in Production).
Idempotency comes from existence checks **plus** the unique indexes (`normalized_email`,
`normalized_name`) — so two instances racing at startup resolve to one winner and one no-op rather than
duplicate rows.

**Rationale.** IV.5 requires seeding be idempotent; relying on application-level existence checks alone is
a TOCTOU race in a multi-instance startup, which US-001-06's edge cases explicitly call out. The database
constraint is the real guarantee; the check is the optimization.

**Alternatives considered.**
- *EF `HasData` model seeding* — rejected: it bakes seed rows into migrations, cannot hash passwords
  through Identity's hasher at migration time, and makes credentials part of schema history.
- *A separate seed CLI* — rejected for v1: the brief and IV.5 want a fresh environment usable on startup
  with no extra step. Revisit if production seeding needs to be operator-triggered.

---

## R-10 — `InitialCreate` migration covers all five constitution entities

**Decision.** The single `InitialCreate` migration authored during 001 creates **all five** constitution
entities — `users`/`roles`/`user_roles`, `refresh_tokens`, `activity_logs`, **and** `projects`, `tasks`,
`team_members` — with their FKs and cascade rules, even though 001 only owns the behavior of the first
group.

**Rationale.** This is not 001's own preference; it is a **cross-spec obligation** that 002, 003, and 004
each record in their Assumptions: *"all five constitution entities are created in the initial migration —
a feature owns an entity's API/UI/rules, not its table's existence."* 002's TeamMember-scoping join and
003's assignee-validation join must compile and be testable on day one. IV.3 also defines the five
entities as one model. Missing this would silently break 002's very first scope test.

Cascade rules fixed here (IV.3, from 002/003/004's data models): `tasks.project_id` → CASCADE;
`team_members.project_id` → CASCADE; `team_members.user_id` → CASCADE; `team_members.added_by` → SET NULL;
`projects.owner_id` → RESTRICT; `tasks.assignee_id` → RESTRICT; `refresh_tokens.user_id` → CASCADE;
`activity_logs` never cascaded.

**Alternatives considered.**
- *One migration per feature, each adding its own table* — the intuitive reading of "002 owns projects".
  Rejected: it contradicts three specs' stated assumptions and leaves 002's scope predicate uncompilable
  until 004 ships.

---

## Resolved Technical Context unknowns

| Unknown | Resolution | Source |
|---|---|---|
| Language/runtime versions | .NET 10, C# nullable enabled, Angular 22 / Node 24 / npm 11, PostgreSQL 18, EF Core 10 + Npgsql | Constitution III (locked) |
| Where slices live | `.Application/Features/Auth/<UseCase>/` | R-1, spec B.3 |
| Domain purity vs Identity | `ApplicationUser` in Domain + `Microsoft.Extensions.Identity.Stores` | R-2 |
| Handler → database path | `IApplicationDbContext`, direct LINQ, no repository | R-3 |
| Validation invocation | `ValidationBehavior`, short-circuits with `Result` | R-4 |
| Contract format/location/tooling | OpenAPI 3.0.3 YAML at `docs/contracts/auth.v1.yaml`; Swashbuckle CLI + `oasdiff`, CI-gated | R-5 |
| UI library | Angular Material | R-6 |
| Test database | Testcontainers PostgreSQL + Respawn | R-7 |
| Performance targets | Auth endpoints p95 < 200 ms (dev hardware); protected-read authorization does **zero** DB round-trips (stateless JWT) | NFR-002, derived |
| Scale assumptions | Hundreds of users, thousands of projects/tasks; pagination mandatory from day one | 002 Assumptions, VI.4 |

**No unresolved NEEDS CLARIFICATION items remain.** Phase 1 may proceed.
