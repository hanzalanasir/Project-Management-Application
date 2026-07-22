# Shared Contracts

Cross-cutting types and conventions every feature specification references. Defined **once** here so
that 001–006 stay consistent; a feature spec cites this file rather than redefining these shapes.

**Governed by**: Constitution v1.1.0 · **Decisions**: [ADR-0003](adr/0003-result-error-contract.md)
(error contract), [ADR-0004](adr/0004-optimistic-concurrency.md) (concurrency),
[ADR-0005](adr/0005-mapping-and-validation.md) (mapping/validation).

---

## 1. Service result contract

Services never throw for expected outcomes; they return a `Result`. A single shared mapper converts
it to an `ActionResult` with an RFC 7807 ProblemDetails body, so status-code behaviour is identical
across all six modules.

```csharp
public enum ErrorKind { Validation, Unauthenticated, Forbidden, NotFound, Conflict, Unexpected }

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

## 3. Resource-scope decision

Row-level authorization (ownership, assignment) that no `[Authorize(Roles=…)]` attribute can express.
Enforced in the **service layer**, never the controller (Constitution II.2).

```csharp
public sealed record AccessDecision(bool Allowed, string? Reason = null);
```

Each feature that owns a scoped resource provides an access policy exposing two halves:
`ApplyScope(IQueryable<T>, CurrentUser)` for collection reads — folded into the query so out-of-scope
rows are never loaded, counted, or paged — and `CanReadAsync` / `CanMutateAsync` for single-entity
reads and all writes, evaluated at the moment of the operation.

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
raises `DbUpdateConcurrencyException`, which the service converts to `ErrorKind.Conflict` → **409**.
Append-only/system tables (`activity_logs`, `refresh_tokens`) are excluded. See ADR-0004.

## 6. Audit

Every write to a domain entity writes one `activity_logs` row **in the same transaction** as the
change (Constitution IV.4): `actor_id`, `action`, `entity_type`, `entity_id`, `timestamp`,
`change_summary`. Deletes audit **before** removal, and audit rows are never cascaded away. Reads are
not audited. The table and `IActivityLogService` are defined by feature 001 and reused by all others.

## 7. Frontend conventions (Angular 22, standalone — ADR-0001)

- Feature areas are lazy-loaded route groups (`loadChildren` → `*.routes.ts`); no `@NgModule`.
- HTTP lives in dedicated services (`ProjectsService`, …), never in components (Constitution VII.3).
- Interceptors are functional (`HttpInterceptorFn`) via `provideHttpClient(withInterceptors([...]))`.
- Guards are functional (`CanActivateFn` / `CanMatchFn`) and are the **only** navigation-blocking
  mechanism (Constitution VII.5).
- Reactive Forms only, with explicit validators; errors surface through one shared error-display
  component (Constitution VII.6).
- Auth session / current user lives in NgRx (`provideStore` / `createFeature`).
- Dev server proxies `/api` to the API so development is same-origin, matching production (ADR-0002).
