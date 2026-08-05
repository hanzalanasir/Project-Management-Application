# Quickstart & Validation: 001 Auth & RBAC

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Contract**:
[docs/contracts/auth.v1.yaml](../../docs/contracts/auth.v1.yaml)

How to run the feature and **prove it works end to end**. Each scenario below maps to a Definition-of-Done
item in spec 001 B.8. This is a validation guide — implementation code lives in the source tree, task
breakdown in `tasks.md`.

---

## Prerequisites

| Requirement | Version | Notes |
|---|---|---|
| .NET SDK | 10.x | `dotnet --version` |
| Node.js / npm | 24.x / 11.x | `node -v` |
| Docker | any recent | **Required for the test suite** — Testcontainers starts a real PostgreSQL ([research.md R-7](research.md)) |
| PostgreSQL | 18 | For running the app locally; the test suite provisions its own |
| `oasdiff` | latest | Contract drift check. `go install github.com/oasdiff/oasdiff@latest` or grab the release binary |

One-time tool restore:

```bash
dotnet tool restore          # Swashbuckle.AspNetCore.Cli, dotnet-ef
```

---

## Secrets (never committed — V.4)

```bash
cd src/ProjectManagementApp.Api

dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5432;Database=pma;Username=postgres;Password=<local>"
dotnet user-secrets set "Jwt:SigningKey" "<at least 32 random bytes, base64>"
dotnet user-secrets set "Seed:Admin:Password"          "<dev password>"
dotnet user-secrets set "Seed:ProjectManager:Password" "<dev password>"
dotnet user-secrets set "Seed:TeamMember:Password"     "<dev password>"
```

The app **must fail fast at startup** with a clear message if `Jwt:SigningKey` or the connection string is
missing — never fall back to a baked-in default.

---

## Run

```bash
# 1. Database schema (creates ALL FIVE constitution tables — see data-model.md §1)
dotnet ef database update -p src/ProjectManagementApp.Infrastructure -s src/ProjectManagementApp.Api

# 2. API  → https://localhost:7001 ; Swagger UI at /swagger (development only, VI.5)
dotnet run --project src/ProjectManagementApp.Api

# 3. Frontend (separate terminal) → http://localhost:4200, proxying /api (ADR-0002)
cd src/ProjectManagementApp.Web && npm install && npm start
```

Seeding runs automatically on startup against an empty database (`Seed:Enabled`, default on in
Development) and provisions one Admin, one ProjectManager, one TeamMember.

---

## Validation scenarios

> `$API` = `https://localhost:7001/api`. `-c cookies.txt -b cookies.txt` persists the httpOnly refresh
> cookie across calls, exactly as a browser would.

### V1 — Register → the account is always a TeamMember · *(DoD 1, FR-001)*

```bash
curl -sS -X POST $API/auth/register -H 'Content-Type: application/json' -d '{
  "fullName":"Dana Rivera","email":"dana@example.com",
  "password":"S3cure-P@ss","confirmPassword":"S3cure-P@ss"}' -i
```

**Expect** `201 Created` · a `Location: /api/users/{id}` header · body is a `UserDto` with
`"role": "TeamMember"`. **Assert the body contains no `password` or `passwordHash` key.**

Now attempt escalation — send `"role":"Admin"` in the payload:

**Expect** the field is ignored; the created user is still `TeamMember`. *(FR-001, US-001-01 edge case)*

### V2 — Duplicate email → 409, not 400 · *(FR-002)*

Re-run V1 verbatim. **Expect** `409 Conflict`, `application/problem+json`, and **no** second row created.
Repeat with `DANA@EXAMPLE.COM` — **expect 409 too** (normalized-email uniqueness, not raw string).

### V3 — Weak password → 400 with per-field errors · *(FR-001)*

Register with `"password":"abc"`. **Expect** `400`, a `ValidationProblemDetails` body whose `errors` object
keys the failure under `password`, and **nothing persisted**.

### V4 — Login issues a token pair, refresh token only as a cookie · *(DoD 1, FR-003, FR-016)*

```bash
curl -sS -X POST $API/auth/login -c cookies.txt -i \
  -H 'Content-Type: application/json' \
  -d '{"email":"dana@example.com","password":"S3cure-P@ss"}'
```

**Expect** `200` · body `{ accessToken, expiresAt, user }` · a `Set-Cookie: refresh_token=…` carrying
**`HttpOnly; Secure; SameSite=Strict; Path=/api/auth`**.

**The critical assertion: the response body contains no refresh token.** Decode the `accessToken` at
[jwt.io](https://jwt.io) and confirm exactly one `role` claim plus `sub`, `email`, `exp`.

### V5 — Wrong credentials are indistinguishable from unknown email · *(FR-003, no enumeration)*

Send a bad password, then a nonexistent email. **Expect both** to return `401` with the **byte-identical**
generic body (`"Invalid credentials"`). Any difference in message, status, or timing shape is a user-
enumeration leak and a test failure.

### V6 — Deactivated account cannot log in *or* refresh · *(FR-004)*

Set `is_active = false` for the user, then retry login and refresh. **Expect `401` from both**, even with a
correct password and a still-valid refresh token.

### V7 — Protected endpoint requires a valid token · *(DoD 2, FR-007)*

```bash
curl -sS $API/auth/me                                   # → 401
curl -sS $API/auth/me -H "Authorization: Bearer $TOKEN" # → 200 UserDto
```

**Expect** `401` bare, `200` with a token. Then confirm the four anonymous endpoints
(`register`, `login`, `refresh`, `health`) succeed **without** a token — and that **no other endpoint does**.

### V8 — The 401/403 role matrix · *(DoD 3, FR-008)*

Against an Admin-only probe endpoint, using seeded accounts:

| Caller | Expect |
|---|---|
| no token | **401** |
| TeamMember token | **403** |
| ProjectManager token | **403** |
| Admin token | **200** |

**401 ≠ 403 is the point**: unauthenticated vs. authenticated-but-not-permitted. Also grep the codebase —
**zero** `if (role == …)` checks in method bodies; the gate must be `[Authorize(Roles=…)]` only *(V.2)*.

### V9 — Refresh rotates single-use tokens; replay is rejected · *(DoD 4, FR-006)*

```bash
curl -sS -X POST $API/auth/refresh -b cookies.txt -c cookies.txt -i   # → 200 + NEW cookie
```

Then **replay the previous cookie value**. **Expect `401`** — the old token was revoked and its
`replaced_by_token` points at the successor. Verify in the database that `refresh_tokens.token_hash` is a
hash, **never the raw token**.

### V10 — Logout revokes server-side and is idempotent · *(FR-005)*

`POST /auth/logout` → **expect `204`**, `revoked_at` set, cookie cleared. Call it **again** → still
succeeds (idempotent). Then try refreshing with the revoked token → **401**.

### V11 — Seeding is idempotent · *(DoD 5, FR-012)*

```bash
dotnet run --project src/ProjectManagementApp.Api    # start, stop, start again
psql -d pma -c "SELECT email, COUNT(*) FROM users GROUP BY email HAVING COUNT(*) > 1;"
```

**Expect zero rows.** Then delete one seeded user and restart — **expect only the missing one recreated**
(partial-state repair). Confirm no seed password appears anywhere in source control.

### V12 — Every user write is audited · *(DoD 6, FR-013)*

```sql
SELECT action, entity_type, actor_id, change_summary FROM activity_logs ORDER BY timestamp DESC LIMIT 10;
```

**Expect** `UserRegistered`, `UserLoggedIn`, `TokenRefreshed`, `UserLoggedOut` rows with actor, entity, and
timestamp. **Assert no row contains a password or a raw token.** Seed rows carry `actor_id = NULL` (system).

### V13 — The contract gate actually fails on drift · *(DoD 9, X.2)*

Prove the guard works rather than assuming it:

```bash
dotnet build -p:CheckApiContract=true          # → passes
```

Now **temporarily** rename a response property in a handler DTO (e.g. `accessToken` → `token`) and rebuild.

**Expect the build to FAIL** with an `oasdiff` breaking-change report. Revert. A gate that has never been
seen to fail is not a gate.

### V14 — Frontend: guards, interceptors, single-flight refresh · *(DoD 8, FR-009/010/011)*

In the browser at `http://localhost:4200`:
1. Navigate to a protected route while logged out → **the route guard redirects to login**, and the lazy
   chunk is **never fetched** (check the Network tab — `CanMatch` prevents the download).
2. Log in → `accessToken` lives in NgRx/memory. **Open DevTools → Application → Local Storage: empty.**
   The refresh cookie must show `HttpOnly ✓`.
3. Wait for access-token expiry (or set `Jwt:AccessTokenMinutes=1`), then trigger **several concurrent**
   API calls. **Expect exactly one** `POST /auth/refresh` in the Network tab (single-flight), and every
   original request retried and succeeding.
4. Revoke the refresh token server-side, then trigger a call → session cleared, redirected to login.

### V15 — Admin sees every user, including deactivated ones · *(DoD 11, FR-017)*

```bash
curl -sS $API/users -H "Authorization: Bearer $ADMIN_TOKEN" -i     # → 200 PagedResult<AdminUserSummary>
curl -sS $API/users -H "Authorization: Bearer $TEAMMEMBER_TOKEN"   # → 403
```

**Expect** the Admin's response to include **all three** seeded accounts, each with `isActive` shown
(`true` for all, at this point). Deactivate one (V17 below), re-list, and **expect it still appears**,
now `"isActive": false` — never filtered out. `GET /api/users/{id}` on the ProjectManager returns **200**
with an `ETag` header; save it as `$ETAG` for V16.

### V16 — Role change: self-refusal and the last-Admin invariant · *(DoD 12, FR-018)*

```bash
curl -sS -X PUT $API/users/$TEAMMEMBER_ID/role -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "If-Match: $ETAG" -H 'Content-Type: application/json' -d '{"role":"ProjectManager"}' -i
```

**Expect** `200`, the updated `AdminUserDetail` with `"role":"ProjectManager"`, and a `UserRoleChanged`
row in `activity_logs` recording `TeamMember → ProjectManager`.

Now, as the Admin, attempt to change your **own** role:

```bash
curl -sS -X PUT $API/users/$ADMIN_ID/role -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "If-Match: $ADMIN_ETAG" -d '{"role":"ProjectManager"}' -i
```

**Expect `409 Conflict`** — `"You cannot change your own role."` — and nothing changes. Since the seed
provisions exactly **one** Admin, this same request is *also* the only way to reach the last-Admin
invariant via the live API, and the self-check refuses it first. **The independent last-Admin count check
(R-12) is proven at the handler-unit-test level** (`ChangeUserRoleCommandHandlerTests`), not by a second
curl step here — see research R-12 for why no distinct-caller HTTP scenario for it exists under the
current one-Admin seed.

### V17 — Deactivation revokes active sessions immediately; self-deactivation refused · *(DoD 13, FR-019)*

```bash
# Log in as the (now) ProjectManager from V16 first, to hold a live refresh cookie:
curl -sS -X POST $API/auth/login -c pm-cookies.txt -d '{"email":"...","password":"..."}' \
  -H 'Content-Type: application/json'

# Admin deactivates that user:
curl -sS -X PUT $API/users/$TEAMMEMBER_ID/status -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "If-Match: $ETAG2" -d '{"isActive":false}' -i
```

**Expect** `200`, `is_active = false` in the database, and a `UserDeactivated` audit row. Now attempt to
refresh using the cookie captured above:

```bash
curl -sS -X POST $API/auth/refresh -b pm-cookies.txt -i
```

**Expect `401`** — the token was revoked the moment deactivation committed, not merely on its own next
natural expiry (proving the bulk-revoke side effect, not just FR-004's existing login/refresh gate).

Then, as the Admin, attempt to deactivate **yourself**: **expect `409 Conflict`**
(`"You cannot deactivate your own account."`), nothing changes.

### V18 — Reactivation does not resurrect the old session · *(DoD 13, FR-019)*

```bash
curl -sS -X PUT $API/users/$TEAMMEMBER_ID/status -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "If-Match: $ETAG3" -d '{"isActive":true}' -i
```

**Expect** `200`, `is_active = true`, and a `UserReactivated` audit row. Now replay the **same** pre-
deactivation refresh cookie from V17:

```bash
curl -sS -X POST $API/auth/refresh -b pm-cookies.txt -i    # → still 401
```

**Expect `401` still** — reactivation restores the ability to **log in fresh**; it does not revive a
token revoked before it. The user must call `/auth/login` again to obtain a new pair.

---

## Test suite

```bash
dotnet test                                                   # all three projects (Docker required)
dotnet test tests/ProjectManagementApp.Application.Tests      # handlers, validators, token service
dotnet test tests/ProjectManagementApp.Infrastructure.Tests   # migrations, xmin, cascades, seeder
dotnet test tests/ProjectManagementApp.Api.Tests              # WebApplicationFactory, HTTP, 401/403 matrix

cd src/ProjectManagementApp.Web && npm test                   # Jasmine + Karma (IX.2)
```

Merging with a failing test is prohibited (IX.3).

---

## Definition-of-Done cross-reference

| DoD (spec B.8) | Proven by |
|---|---|
| 1 — end-to-end auth, password never returned | V1, V4 |
| 2 — authenticated by default; 4 anonymous endpoints | V7 |
| 3 — role matrix, no in-body role checks | V8 |
| 4 — expiry, single-use rotation, replay rejected, transparent refresh | V9, V14.3 |
| 5 — idempotent seeding, partial repair, no hardcoded secrets | V11 |
| 6 — every user write audited | V12 |
| 7 — CORS allow-list; secrets from user-secrets/env | Secrets section, config review |
| 8 — lazy standalone `auth` group, functional interceptors/guards, NgRx | V14 |
| 9 — API-first contract authored before handlers, code validated against it | V13 |
| 10 — unit + integration tests pass | Test suite |
| 11 — Admin sees every user incl. deactivated, flagged; non-Admin refused 403 | V15 |
| 12 — role change audited, self-refused, last-Admin-refused | V16 (last-Admin also `ChangeUserRoleCommandHandlerTests`, per R-12) |
| 13 — deactivation revokes active sessions immediately; reactivation does not restore them; self-deactivation refused | V17, V18 |
