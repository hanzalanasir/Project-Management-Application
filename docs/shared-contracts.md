# Shared Contracts

Cross-cutting types and conventions every feature specification references. Defined **once** here so
that 001–006 stay consistent; a feature spec cites this file rather than redefining these shapes.

**Standing rule (Clean Architecture, Constitution II.2 / ADR-0006):** cross-cutting services and
scope-authorization policies live **here**, in the shared kernel — not inside any single feature's
Application layer. A feature folder may depend on this file's contracts, **never** on another feature's
own Application layer (its slice handlers) directly. Cross-feature reuse always goes through a shared
abstraction declared here or a shared Domain entity — this is what any future feature (007+) is checked
against, without re-deriving the reasoning each time.

**Governed by**: Constitution v1.3.0 · **Decisions**: [ADR-0003](adr/0003-result-error-contract.md)
(error contract), [ADR-0004](adr/0004-optimistic-concurrency.md) (concurrency),
[ADR-0005](adr/0005-mapping-and-validation.md) (mapping/validation),
[ADR-0006](adr/0006-vertical-slice-clean-architecture-api-first.md)
(vertical slice + Clean Architecture + API-first),
[ADR-0007](adr/0007-implementation-conventions.md) (contract drift gate · Testcontainers-only test
database · `ETag`/`If-Match` concurrency transport · builder-based test fixtures).

---

## 1. Service result contract

Services never throw for expected outcomes; they return a `Result`. A single shared mapper converts
it to an `ActionResult` with an RFC 7807 ProblemDetails body, so status-code behaviour is identical
across all six modules.

```csharp
public enum ErrorKind { Validation, Unauthenticated, Forbidden, NotFound, Conflict,
                        UnprocessableContent, Unexpected }

public sealed record Error(ErrorKind Kind, string Message, IReadOnlyDictionary<string, string[]>? Fields = null);

public class Result {
    public bool IsSuccess { get; }
    public Error? Error { get; }
    public static Result Success();
    public static Result Failure(Error error);
}

public sealed class Result<T> : Result {
    public T? Value { get; }
    public static Result<T> Success(T value);
    public static new Result<T> Failure(Error error);
}
```

### Error → HTTP mapping (the only place this mapping exists)

| `ErrorKind` | Status | ProblemDetails `title` |
|---|---|---|
| `Validation` | **400** | `Validation failed` (with per-field `errors`) |
| `Unauthenticated` | **401** | `Authentication required` |
| `Forbidden` | **403** | `Forbidden` |
| `NotFound` | **404** | `<Entity> not found` |
| `Conflict` | **409** | `Conflict` (duplicate, or stale concurrency token) |
| `UnprocessableContent` | **422** | `Unprocessable — result set too large` (the request is well-formed and every parameter individually valid; the *result size* cannot be processed — 006 Reports) |
| `Unexpected` | **500** | `Unexpected error` (no stack trace in production) |

Creates additionally return **201** with a `Location` header; deletes return **204** (Constitution VI.2).
Genuine exceptions (bugs, infrastructure failure) are caught by exception-handling middleware and
emitted as a 500 ProblemDetails.

## 2. Caller identity

Materialized from the validated JWT (feature 001) by a scoped `ICurrentUserService` backed by
`IHttpContextAccessor`. **Never** populated from a request body or query string.

```csharp
public sealed record CurrentUser(Guid UserId, string Email, string Role);   // exactly one role — see 001

public interface ICurrentUserService { CurrentUser Current { get; } }
```

## 3. Resource-scope decision (scope-authorization policies — shared kernel)

Row-level authorization (ownership, assignment) that no `[Authorize(Roles=…)]` attribute can express.
Enforced in the **slice handler**, never the controller (Constitution II.2 / ADR-0006).

```csharp
public sealed record AccessDecision(bool Allowed, string? Reason = null);
```

A scope-authorization policy exposes two halves: `ApplyScope(IQueryable<T>, CurrentUser)` for collection
reads — folded into the query so out-of-scope rows are never loaded, counted, or paged — and
`CanReadAsync` / `CanMutateAsync` for single-entity reads and all writes, evaluated at the moment of the
operation.

**These policy interfaces are shared-kernel abstractions — they live here, not inside any single
feature's Application layer** — so any feature's handler may depend on them exactly the way it depends on
`ICurrentUserService` (§2) or `IActivityLogService` (§6). The owning feature supplies the *scope rules*
(the implementation); the **interface is shared**, which is precisely what lets a cross-cutting reader
(005 Dashboard, 006 Reports) reuse `IProjectAccessPolicy.ApplyScope` / `ITaskAccessPolicy.ApplyScope`
**without** taking a dependency on 002's or 003's Application layer.

```csharp
// Shared-kernel scope-authorization policies. The interface lives here; the owning feature implements the rules.
public interface IProjectAccessPolicy {   // rules owned/implemented by 002; reused by 005, 006
    IQueryable<Project> ApplyScope(IQueryable<Project> source, CurrentUser caller);   // Admin=all · PM=owned · TM=assigned
    Task<AccessDecision> CanReadAsync(Project project, CurrentUser caller, CancellationToken ct);
    Task<AccessDecision> CanMutateAsync(Project project, CurrentUser caller, CancellationToken ct);
}

public interface ITaskAccessPolicy {      // rules owned/implemented by 003; reused by 005, 006
    IQueryable<TaskItem> ApplyScope(IQueryable<TaskItem> source, CurrentUser caller);  // Admin=all · PM=owned projects · TM=assigned
    Task<AccessDecision> CanReadAsync(TaskItem task, CurrentUser caller, CancellationToken ct);
    Task<AccessDecision> CanMutateAsync(TaskItem task, TaskMutation mutation, CurrentUser caller, CancellationToken ct);
}

public interface ITeamAccessPolicy {      // rules owned/implemented by 004
    Task<AccessDecision> CanViewTeamAsync(Project project, CurrentUser caller, CancellationToken ct);   // Admin · owner · member
    Task<AccessDecision> CanManageTeamAsync(Project project, CurrentUser caller, CancellationToken ct); // Admin · owner only
}
```

**`TaskMutation`** (`Create, FullEdit, StatusChange, Reassign, Delete`) is the graduated-mutation enum. Its
*values* are 003's vocabulary, but the **type is part of this shared kernel** — it appears in the
`ITaskAccessPolicy` signature above, so it must be declared alongside `AccessDecision` (in
`Application/Common/Models/`) and **created in 001**, with the interfaces. Treating it as "003's enum" and
deferring it would leave this interface non-compiling from the moment it is authored.

`ITeamAccessPolicy` is deliberately **binary** — no `ApplyScope`, because every team read is
pinned to a single project by the route (see 004 T.2). `Project` / `TaskItem` are shared Domain entities
(created in the initial migration), which any feature may read directly — the second permitted form of
cross-feature dependency alongside the shared abstractions above.

## 4. Pagination envelope

Every collection endpoint whose result set can exceed 50 items (Constitution VI.4) accepts `?page`
and `?pageSize` and returns this envelope. `TotalCount` is always **scoped to the caller**.

```csharp
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);
```

```json
{ "items": [ … ], "page": 1, "pageSize": 20, "totalCount": 37, "totalPages": 2 }
```

Defaults: `page` = 1, `pageSize` = 20, maximum `pageSize` = 100 (configurable). A `pageSize` above the
maximum is **clamped**, not rejected; non-numeric or negative paging values return **400**.

## 5. Concurrency

`projects`, `tasks`, and `users` map PostgreSQL `xmin` as an EF Core row-version token. A stale write
raises `DbUpdateConcurrencyException`, which the handler converts to `ErrorKind.Conflict` → **409**. The
row version travels as an `ETag` on reads and a required `If-Match` on writes — never as a body field
([ADR-0007](adr/0007-implementation-conventions.md) §3).

**Two categories are deliberately excluded**, and the distinction matters:

- **Append-only / system tables** — `activity_logs`, `refresh_tokens`. Rows are inserted and never
  contended-updated.
- **Join tables with no mutable field** — `team_members`. The row is added or removed, never edited in
  place, so there is nothing for a row version to protect. Concurrency safety comes instead from the
  **unique constraint `(project_id, user_id)`**: a race to add the same member resolves to one **201** and
  one **409**, and removal is naturally idempotent (a second remove observes 404). See 004 T.6.

A feature introducing a new entity states which category it falls in. See ADR-0004.

## 6. Audit

Every write to a domain entity writes one `activity_logs` row **in the same transaction** as the
change (Constitution IV.4): `actor_id`, `action`, `entity_type`, `entity_id`, `timestamp`,
`change_summary`. Deletes audit **before** removal, and audit rows are never cascaded away. Reads are
not audited. The table and `IActivityLogService` are defined by feature 001 and reused by all others.

**The service exposes a write *and* a scoped read.** Both live here, on the service that owns the table:

```csharp
public interface IActivityLogService {
    // WRITE — every feature that mutates a domain entity.
    Task LogAsync(Guid? actorId, string action, string entityType, string entityId,
                  string changeSummary, CancellationToken ct);

    // SCOPED READ — 005 (activity feed) and 006 (Activity Report).
    Task<PagedResult<ActivityEntry>> QueryScopedAsync(
        ActivityScope scope, int page, int pageSize, CancellationToken ct);
}

public sealed record ActivityScope(IReadOnlyCollection<Guid> VisibleProjectIds, bool Unscoped);
public sealed record ActivityEntry(Guid Id, Guid? ActorId, string ActorName, string Action,
                                   string EntityType, string EntityId,
                                   DateTimeOffset Timestamp, string ChangeSummary);
```

**Reading the audit log through this service — never by a direct `activity_logs` query — is a hard rule**
(005 FR-006, 006 FR-007). It keeps audit scoping in one place and inherits 001's guarantee that entries
carry no secrets. `ActivityScope` and `ActivityEntry` are shared-kernel types declared alongside
`AccessDecision` and `TaskMutation`, and — like them — are **created in 001 with the interfaces that
reference them**, even though only later features consume them.

## 7. Persistence access

How a slice's handler reaches the database. Declared in the **Application** layer (shared kernel) and
implemented by Infrastructure's `ApplicationDbContext`. Every feature's handlers depend on this interface;
**no feature redefines it**, and no repository sits on top of it.

```csharp
public interface IApplicationDbContext {
    DbSet<ApplicationUser> Users        { get; }
    DbSet<RefreshToken>    RefreshTokens{ get; }
    DbSet<ActivityLog>     ActivityLogs { get; }
    DbSet<Project>         Projects     { get; }
    DbSet<TaskItem>        Tasks        { get; }
    DbSet<TeamMember>      TeamMembers  { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

Handlers compose LINQ **directly** against `DbSet<T>` — this is Constitution IV.1's *"a slice's handler MAY
call the DbContext directly as its default persistence path; a separate Repository is optional, not
required."* It carries the same shared-kernel status as `ICurrentUserService` (§2), the access policies
(§3), and `IActivityLogService` (§6).

**This is not a Repository, and should not be reviewed as one.** A repository abstracts *queries* behind
named methods (`GetActiveUsersAsync()`), hiding the query model. This interface hides nothing — it exposes
the identical `IQueryable` surface as the concrete context. Its only job is to point the compile-time
dependency arrow inward so Application never references Infrastructure.

**It is load-bearing for §3.** `ApplyScope` folds a predicate into a **live `IQueryable`**. Behind a
repository returning materialized lists, scope filtering would move into memory and out-of-scope rows
*would be loaded* — breaking 002 FR-007 / NFR-002, which are security properties, not style preferences.
The no-repository decision therefore protects a guarantee, not an aesthetic (ADR-0006).

**Consequences for every feature:**

- A feature that introduces a new entity adds its `DbSet<T>` here — it does **not** create a parallel
  context or a feature-local persistence abstraction.
- The §6 audit rule rides on this interface: a domain change and its `activity_logs` row are written
  through the same `IApplicationDbContext` and commit in **one** `SaveChangesAsync`.
- Application therefore references EF Core (for `DbSet`/LINQ). That is accepted and deliberate; the
  dependency that matters — Application → Infrastructure — remains absent.

## 8. Shared metric definitions

The predicates behind every count that appears on more than one surface. Declared here — **not inside the
feature that first needed them** — because 005 Dashboard and 006 Reports are *required* to produce
identical values for the same caller (006 NFR-002, guarded by a cross-feature test).

```csharp
// Application/Common/Metrics/MetricDefinitions.cs — shared kernel, created with the rest of it.
public static class MetricDefinitions
{
    // "today" is UTC, FIXED. Not configurable — see below.
    public static Expression<Func<TaskItem, bool>> IsOverdue(DateOnly todayUtc) =>
        t => t.DueDate != null && t.DueDate < todayUtc && t.Status != TaskStatus.Done;

    public static Expression<Func<TaskItem, bool>> IsClosed =>
        t => t.Status == TaskStatus.Done;            // ⇔ closed_at is not null (003's invariant)

    public static decimal CompletionRate(int closed, int total) =>
        total == 0 ? 0m : (decimal)closed / total;   // 0 when total is 0 — never a divide-by-zero

    public static Expression<Func<TaskItem, bool>> ClosedInWindow(DateTimeOffset from, DateTimeOffset to) =>
        t => t.ClosedAt != null && t.ClosedAt >= from && t.ClosedAt <= to;   // throughput
}
```

**Consumers:** 005 (summary tiles, personal slice) and 006 (Project Progress, Task Completion, Team
Performance). Both **import these; neither re-implements them.**

**`todayUtc` is UTC and fixed.** A configurable timezone would let a deployment produce different overdue
counts on the Dashboard and in Reports — breaking 006's NFR-002 by configuration rather than by defect.
005's spec was corrected to state this.

**A re-opened task is excluded uniformly.** 003 clears `closed_at` on re-open, so such a task fails both
`IsClosed` and `ClosedInWindow` — dropping out of Project Progress's closed count *and* Task Completion's
buckets, by construction rather than by two matching implementations.

> **Why this lives here rather than in 005.** It was originally placed in `Features/Dashboard/Common/`, and
> 006 was told to import it from there — which would have made 006 depend on 005's Application layer, the
> one thing [ADR-0006's addendum](adr/0006-vertical-slice-clean-architecture-api-first.md) forbids. Any
> definition two features must agree on belongs in the shared kernel; see
> [ADR-0007 §5](adr/0007-implementation-conventions.md).

## 9. Frontend conventions (Angular 22, standalone — ADR-0001)

- Feature areas are lazy-loaded route groups (`loadChildren` → `*.routes.ts`); no `@NgModule`.
- HTTP lives in dedicated services (`ProjectsService`, …), never in components (Constitution VII.3).
- Interceptors are functional (`HttpInterceptorFn`) via `provideHttpClient(withInterceptors([...]))`.
- Guards are functional (`CanActivateFn` / `CanMatchFn`) and are the **only** navigation-blocking
  mechanism (Constitution VII.5).
- Reactive Forms only, with explicit validators; errors surface through one shared error-display
  component (Constitution VII.6).
- Auth session / current user lives in NgRx (`provideStore` / `createFeature`).
- Dev server proxies `/api` to the API so development is same-origin, matching production (ADR-0002).
