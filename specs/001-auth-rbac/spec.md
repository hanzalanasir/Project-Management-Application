# Feature Specification: Authentication & Role-Based Access Control

**Feature Number**: 001
**Feature Name**: Authentication & Role-Based Access Control (Auth & RBAC)
**Phase**: Phase 1 — Foundation
**Priority**: Must Have (Critical)
**Complexity**: High
**Type**: Foundation / Cross-cutting security
**Depends On**: — (none; this is the foundational security feature)
**Enables**: 002 Projects · 003 Tasks · 004 Team · 005 Dashboard · 006 Reports (every other feature consumes this feature's authentication and role checks)
**Created**: 2026-07-22
**Status**: Draft — Ready for Planning
**Governed By**: Project Constitution v1.1.0 (Principles II Architecture, III Stack, V Security & Authorization, VI API Design, VII Frontend, VIII Code Quality, IX Testing)
**Cross-cutting contracts**: [docs/shared-contracts.md](../../docs/shared-contracts.md) — `Result<T>`, `CurrentUser`, error→HTTP mapping · ADRs [0001](../../docs/adr/0001-angular-standalone-components.md) standalone Angular · [0002](../../docs/adr/0002-same-origin-hosting.md) same-origin hosting · [0003](../../docs/adr/0003-result-error-contract.md) error contract · [0005](../../docs/adr/0005-mapping-and-validation.md) mapping/validation
**Generated Via**: `/speckit.specify` (merged requirements + solution design, per project convention)

---

## Purpose

Own the authoritative **identity and authorization layer** for ProjectManagementApp: who a user is (registration, login, logout), what role they hold (Admin, ProjectManager, TeamMember), and how every other feature proves and enforces access. The API issues JWT bearer tokens, ASP.NET Core Identity manages users/roles/password hashing, and role checks are declared with `[Authorize(Roles = "...")]` attributes. This feature also provisions the initial account set so a freshly created database is immediately usable.

This is a **merged specification**: requirements (Purpose → User Stories → Functional Requirements) and solution design (Technical Design → Implementation Blueprint) live in one file by project convention, so the team reviews *what* and *how* together.

## Business Value

Authentication and authorization are the gate every other module sits behind. A correct, auditable, attribute-driven security layer means the Dashboard, Projects, Tasks, Team, and Reports features never re-implement access logic — they declare a role requirement and rely on this feature. Server-side enforcement (the frontend is never trusted) keeps the application safe even when a client is tampered with, and the audit trail on every user write makes account changes traceable from day one. Idempotent seeding removes the "empty database, cannot log in" bootstrap problem and gives every environment a known Admin/ProjectManager/TeamMember set to demo and test against.

## Actors

**Primary Actors**
- **Admin** — manages all users and all projects; the only role that can create/deactivate users beyond self-registration and administer any project.
- **ProjectManager** — creates and manages their own projects and assigns team members to them (project-scoped authority, enforced by later features).
- **TeamMember** — views and updates only the tasks assigned to them (task-scoped authority, enforced by later features).

**Secondary Actors**
- **Anonymous visitor** — may only reach registration, login, and health-check endpoints.
- **System (non-actor)** — the seeding routine that provisions initial accounts on an empty database; runs at application startup, not on behalf of a human.

## Scope Summary

**In scope**: user registration; login with JWT issuance; logout with refresh-token revocation; access-token and refresh-token lifetimes with a token-refresh flow; role-based protection of both Angular routes (guards) and API endpoints (`[Authorize]` attributes); the three-role model (Admin, ProjectManager, TeamMember) and its role/claim representation in the JWT; the lazy-loaded Angular **`auth` route group** (standalone components per ADR-0001 — reactive forms, functional JWT interceptor, functional 401 interceptor, functional route guards, NgRx auth session); ASP.NET Core Identity configuration (password hashing, password policy); the `Users`, `Roles`, `RefreshTokens`, and `ActivityLogs` persistence for this feature; idempotent seed provisioning of one Admin, one ProjectManager, one TeamMember; audit of every write to `Users`.

**Out of scope**: SSO / external identity providers; social login (Google/GitHub/etc.); multi-factor authentication (MFA); the business rules of Projects/Tasks/Team/Dashboard/Reports (those features own their own resource-level authorization and merely *declare* which role they require); password-reset email flows and account email verification (not in the brief; may be added later without reworking this design); user-profile management beyond what authentication requires.

## Access Logic (applies to every story)

Enforced **server-side**, in this order:

1. **Authenticated by default** — every API endpoint requires a valid JWT. The only anonymous endpoints are **register**, **login**, **token refresh**, and **health checks**, each explicitly marked `[AllowAnonymous]`. A request with no/invalid/expired token → **401**.
2. **Role gate** — endpoints that are role-restricted declare `[Authorize(Roles = "Admin")]` (or the relevant role list). A valid token whose role claim does not satisfy the requirement → **403**. Role checks are **attribute-only**; ad-hoc `if (user.Role == ...)` inside method bodies is prohibited (Constitution V.2).
3. **Frontend is convenience, never the boundary** — Angular route guards decide what a user *sees/navigates to*; they never decide what is *allowed*. The API re-checks every request. Hiding a button is UX, not security.
4. **Identity from the token** — the acting user id and roles are read from the validated JWT, never from the request body or query string.

## Role & Permission Model

Three roles, seeded at startup and represented as ASP.NET Core Identity roles and a **single** JWT `role` claim per user (each user holds exactly one role):

| Role | Authority (enforced across features via `[Authorize(Roles=...)]`) |
|---|---|
| **Admin** | Full administration: manage all users, all projects, all tasks, all reports. |
| **ProjectManager** | Create/manage own projects; assign team members to them. (Project ownership enforced by feature 002.) |
| **TeamMember** | View/update only tasks assigned to them. (Task assignment enforced by feature 003.) |

Within *this* feature, the role model exists so that: registration assigns a role; login embeds role claims in the JWT; and route/endpoint guards can restrict by role. Resource-level rules ("a ProjectManager may only edit *their own* project") are owned by the feature that owns the resource — this feature guarantees the role claim is present, trustworthy, and enforced at the endpoint.

---

## Clarifications

### Session 2026-07-22

- Q: Registration model — open self-signup vs Admin-only provisioning? → A: **Open self-registration**; self-signup accounts are **always** created as `TeamMember` and any client-supplied role is ignored. Creating `ProjectManager`/`Admin` accounts (or elevating a role) is an **Admin-only** action owned by user administration (feature 004), not by the public `register` endpoint.
- Q: Refresh-token client storage & transport? → A: Access token held in memory/NgRx; the **refresh token is delivered only as an httpOnly, Secure, SameSite cookie** (never JavaScript-readable, never in the response body). `/refresh` and `/logout` read the cookie and are **CSRF-protected** (SameSite + anti-forgery). Single-use rotation replaces the cookie on each refresh.
- Q: HTTP status for a duplicate-email registration attempt? → A: **409 Conflict** (resource-conflict semantics, distinct from field-level **400** validation errors). Confirms the existing draft; the constitution's status-code list (VI.2) is representative, not exhaustive, and does not prohibit 409.
- Q: How many roles can a user hold? → A: **Exactly one role per user** (a single `role` claim: Admin, ProjectManager, or TeamMember). ASP.NET Core Identity's `user_roles` join table exists physically, but a business rule enforces one role; endpoints may still permit several roles (`Roles="Admin,ProjectManager"`) and the user's single role must match one of them; seed provisions one account per role.

---

## User Stories

> Story IDs `US-001-01..06`. Each story: **A** Summary · **B** Quality Validation (INVEST · Given-When-Then · edge cases · audit/security · configurability) · **C** UI · **D** API · **E** DB · **F** Separation of concerns. Consolidated schema, API catalog, technical design, and implementation blueprint follow the stories.

---

### US-001-01 — Register a new account

**A. Summary**
- **Story ID**: US-001-01 · **Title**: Register a new user account
- **Actor**: Anonymous visitor (self-registration → always TeamMember). Privileged-role account creation is Admin-only and owned by user administration (feature 004).
- **User story**: *As a new user, I want to register with my details and a password, so that I have a TeamMember account and can sign in.*
- **Business value**: Entry point to the whole application; creates the authoritative user record.
- **Priority**: **P0** · **Reason**: Nothing works without an account.
- **Dependencies**: US-001-06 (roles must exist before a role can be assigned). **Out of scope**: email verification, password reset.

**B. Quality validation**
- **INVEST** — Independent ✔ (registration stands alone); Negotiable ✔ (required-field set, default role); Valuable ✔; Estimable ✔; Small ✔ (record create + audit only); Testable ✔ (account exists, password never returned, audit row written).
- **Given/When/Then**
  1. **Given** a unique email and a policy-valid password, **When** the visitor registers, **Then** a `users` row is created with a hashed password, the **`TeamMember`** role, and an `activity_logs` entry (actor, action `UserRegistered`, entity `User`, entity id, timestamp, summary) is written.
  2. **Given** an email that already exists, **When** registering, **Then** the request is rejected with **409 Conflict** (RFC 7807 Problem Details) and no row is created.
  3. **Given** a password that fails policy (length/complexity per Identity options), **When** registering, **Then** **400** with field-level validation errors; nothing is stored.
  4. **Given** a successful registration, **When** the response is returned, **Then** it contains the new user id and profile fields but **never** the password or password hash.
- **Edge cases**: duplicate email differing only by case (normalized-email uniqueness); whitespace/oversized inputs; **self-registration ignores any client-supplied role and always assigns `TeamMember`** — a self-signup request can never elevate its own role.
- **Audit/security**: password hashed by ASP.NET Core Identity, never logged, never returned; registration audited; endpoint `[AllowAnonymous]`.
- **Configurability**: password policy (length, complexity) via Identity options; a flag to enable/disable public self-registration (default: enabled). The self-signup role is **fixed to `TeamMember`** (not configurable).

**C. UI** — **F001-S01 Register** (standalone component in the lazy-loaded `auth` route group). Reactive form: `fullName`, `email`, `password`, `confirmPassword`; explicit validators (required, email format, min length, password match); errors surfaced through the shared error-display component; submit disabled while pending; success routes to login.

**D. API** — `POST /api/auth/register` · `[AllowAnonymous]` · returns **201 Created** + `Location: /api/users/{id}` + safe user DTO.

**E. DB** — writes **`users`** (Identity), **`user_roles`** (role assignment), **`activity_logs`** (audit).

**F. Separation** — UI: register form + inline validation. Backend: `IAuthService.RegisterAsync` (validation, Identity `CreateAsync`, role assignment, audit). DB: user + role link + audit row. QA: duplicate reject, password never returned, weak-password reject, audit written.

---

### US-001-02 — Log in and receive a token

**A. Summary**
- **Story ID**: US-001-02 · **Title**: Authenticate and receive a JWT
- **Actor**: Any registered user (Admin / ProjectManager / TeamMember)
- **User story**: *As a registered user, I want to log in with my email and password, so that I receive a token that authorizes my subsequent requests.*
- **Business value**: Establishes the authenticated session every other feature depends on.
- **Priority**: **P0** · **Reason**: Foundational.
- **Dependencies**: US-001-01. **Out of scope**: MFA, social login.

**B. Quality validation**
- **INVEST** — all ✔ (credential exchange only).
- **Given/When/Then**
  1. **Given** valid credentials, **When** the user logs in, **Then** the API returns **200** with a short-lived **access token** (JWT with `sub`, `email`, `role` claims) and a **refresh token delivered as an httpOnly cookie**, and writes an `activity_logs` entry (`UserLoggedIn`).
  2. **Given** an incorrect password or unknown email, **When** logging in, **Then** **401** with a **generic** message ("Invalid credentials") that does not reveal which field was wrong (no user enumeration).
  3. **Given** a deactivated account (`is_active = false`), **When** logging in, **Then** **401**, even with the correct password.
  4. **Given** a successful login, **When** the token is inspected, **Then** it carries the user's single `role` claim and an expiry (`exp`) matching the configured access-token lifetime.
- **Edge cases**: repeated failed attempts (Identity lockout, configurable); case-insensitive email; clock skew on `exp`; login while already holding a valid token (issue a fresh pair).
- **Audit/security**: password verified via Identity hasher; failures audited without storing the attempted password; tokens signed with a key from user secrets / environment (never committed); generic failure text.
- **Configurability**: access-token lifetime; refresh-token lifetime; lockout threshold/duration; signing key + issuer/audience (all from configuration).

**C. UI** — **F001-S02 Login**. Reactive form: `email`, `password`; validators; error-display component shows the generic failure; on success the token pair is stored and the current user is placed in **NgRx** auth state, then the app routes to the dashboard.

**D. API** — `POST /api/auth/login` · `[AllowAnonymous]` · **200** with `{ accessToken, expiresAt, user }` + `Set-Cookie` httpOnly refresh token.

**E. DB** — reads **`users`**/**`user_roles`**; writes **`refresh_tokens`** (new token, stored hashed) and **`activity_logs`**.

**F. Separation** — UI: login form + NgRx dispatch. Backend: `IAuthService.LoginAsync` + `ITokenService.CreateAccessToken/CreateRefreshToken`. DB: refresh-token persist + audit. QA: generic 401, deactivated blocked, claims/expiry correct.

---

### US-001-03 — Log out

**A. Summary**
- **Story ID**: US-001-03 · **Title**: Log out and end the session
- **Actor**: Any authenticated user
- **User story**: *As a signed-in user, I want to log out, so that my refresh token can no longer be used and my session is cleared on this device.*
- **Business value**: Lets a user end a session deliberately; limits refresh-token lifetime on shared machines.
- **Priority**: **P1** · **Reason**: Important; builds on login.
- **Dependencies**: US-001-02.

**B. Quality validation**
- **INVEST** — all ✔.
- **Given/When/Then**
  1. **Given** a valid session, **When** the user logs out, **Then** the refresh token presented via the cookie is marked revoked (`revoked_at` set), the cookie is cleared, the API returns **204**, and an `activity_logs` entry (`UserLoggedOut`) is written.
  2. **Given** a revoked refresh token, **When** it is later used to refresh, **Then** the refresh is rejected with **401** (see US-001-05).
  3. **Given** logout, **When** the frontend handles the response, **Then** NgRx auth state is cleared and the user is routed to login.
- **Edge cases**: logout with an already-expired/absent refresh token (idempotent success — still clear client state); access token remains valid until its short `exp` (accepted trade-off, documented).
- **Audit/security**: logout audited; revocation is server-side (client-side clearing alone is not trusted); endpoint requires a valid JWT.
- **Configurability**: whether logout revokes only the current device's refresh token or all of the user's refresh tokens (config flag; default: current token only).

**C. UI** — logout control in the app shell (a `core/` singleton provided once in the application config) dispatches an NgRx logout action; the 401 interceptor also drives this path on token failure.

**D. API** — `POST /api/auth/logout` · `[Authorize]` · **204 No Content**.

**E. DB** — updates **`refresh_tokens`** (`revoked_at`); writes **`activity_logs`**.

**F. Separation** — UI: dispatch + guard-driven redirect. Backend: `IAuthService.LogoutAsync` (revoke). DB: refresh-token revoke + audit. QA: revoked token unusable, idempotent logout, state cleared.

---

### US-001-04 — Role-based route & endpoint protection

**A. Summary**
- **Story ID**: US-001-04 · **Title**: Protect routes and endpoints by role
- **Actor**: All roles (as subjects of protection); developers of features 002–006 (as consumers)
- **User story**: *As the system owner, I want every route and endpoint protected by authentication and, where needed, by role, so that users can only reach what their role permits.*
- **Business value**: The reusable guarantee that lets every other feature declare access instead of re-implementing it.
- **Priority**: **P0** · **Reason**: The core RBAC promise.
- **Dependencies**: US-001-02 (token with role claims). **Out of scope**: resource-level ownership rules (owned by the resource's feature).

**B. Quality validation**
- **INVEST** — all ✔ (cross-cutting but independently testable with sample protected endpoints).
- **Given/When/Then**
  1. **Given** an endpoint with no attribute, **When** any request arrives, **Then** it is authenticated-by-default (the global fallback policy requires an authenticated user) — an unauthenticated call → **401**.
  2. **Given** `[Authorize(Roles = "Admin")]`, **When** a TeamMember token calls it, **Then** **403**; **When** an Admin token calls it, **Then** it proceeds.
  3. **Given** an Angular route guarded for `Admin`, **When** a TeamMember navigates to it, **Then** the route guard blocks navigation and redirects — and the API still returns **403** if the request is forced.
  4. **Given** an `[AllowAnonymous]` endpoint (register/login/refresh/health), **When** called without a token, **Then** it succeeds.
- **Edge cases**: token valid but role claim missing/renamed; an endpoint permitting several roles (`Roles="Admin,ProjectManager"`) where the user's single role must match one of them; guard vs. API disagreement (API always wins); a route lazily loaded only after the guard passes.
- **Audit/security**: authorization failures return **403** as Problem Details; the default is deny (authenticated); route guards are the **only** client-side navigation-blocking mechanism (no component-level redirect logic, per Constitution VII.5).
- **Configurability**: role→route mapping in Angular route config; role→endpoint mapping via attributes; the global fallback policy (authenticated) set once in the API.

**C. UI** — Angular **functional route guards** (`CanActivateFn`/`CanMatchFn`) under `core/` read the current user's role from NgRx and allow/deny navigation; a lazy route group is only matched and loaded after the guard passes.

**D. API** — global fallback authorization policy = authenticated; per-endpoint `[Authorize(Roles = "...")]`; `[AllowAnonymous]` on the four public endpoints. Representative protected probe: `GET /api/auth/me` (`[Authorize]`) and an Admin-only probe used in tests.

**E. DB** — no writes; role claims resolved from the token (issued from `user_roles` at login).

**F. Separation** — UI: guards only. Backend: attributes + global policy; a middleware-free, attribute-declared model. DB: none at request time. QA: 401 vs 403 matrix per role × endpoint; guard-block test; anonymous-allow test.

---

### US-001-05 — Token expiry & refresh

**A. Summary**
- **Story ID**: US-001-05 · **Title**: Expire access tokens and refresh them safely
- **Actor**: Any authenticated user (transparently, via the frontend)
- **User story**: *As a signed-in user, I want my short-lived access token to be renewed automatically using a refresh token, so that I stay signed in without re-entering credentials, while expired/stolen tokens stop working.*
- **Business value**: Balances security (short access-token life) with UX (no constant re-login).
- **Priority**: **P0** · **Reason**: Sessions are unusable or insecure without it.
- **Dependencies**: US-001-02, US-001-03.

**B. Quality validation**
- **INVEST** — all ✔.
- **Given/When/Then**
  1. **Given** an expired access token, **When** a request is made, **Then** the API returns **401**; the frontend's 401 interceptor attempts a refresh once.
  2. **Given** a valid, non-revoked, non-expired refresh token, **When** refresh is called, **Then** the API returns a **new access token + new refresh token**, revokes/rotates the old refresh token, and the original request is retried.
  3. **Given** a refresh token that is expired, revoked, or unknown, **When** refresh is called, **Then** **401**; the frontend clears the session and redirects to login.
  4. **Given** a rotated (already-used) refresh token, **When** it is replayed, **Then** **401** (single-use rotation).
- **Edge cases**: two concurrent 401s racing to refresh (single-flight refresh on the client); refresh exactly at expiry boundary; refresh token valid but its user was deactivated → deny; clock skew tolerance on `exp`.
- **Audit/security**: refresh tokens stored **hashed** (never plaintext); single-use rotation with `replaced_by_token`; refresh audited (`TokenRefreshed`); the refresh endpoint is `[AllowAnonymous]` but validates the token itself.
- **Configurability**: access-token lifetime (default short, e.g. minutes); refresh-token lifetime (default longer, e.g. days); rotation on/off; skew tolerance — all from configuration, never hardcoded.

**C. UI** — the **401 HTTP interceptor** intercepts a 401, calls `POST /api/auth/refresh` once, updates NgRx with the new token pair, and retries the original request; on refresh failure it dispatches logout and the guard redirects to login. The **JWT interceptor** attaches the current access token to every outgoing request.

**D. API** — `POST /api/auth/refresh` · `[AllowAnonymous]` (reads the refresh cookie) · **200** with `{ accessToken, expiresAt }` + rotated `Set-Cookie`; invalid → **401**.

**E. DB** — reads/updates **`refresh_tokens`** (validate, revoke old, insert new); writes **`activity_logs`**.

**F. Separation** — UI: two interceptors (attach + refresh), single-flight refresh. Backend: `IAuthService.RefreshAsync` + `ITokenService.ValidateRefreshToken`. DB: rotation. QA: replay rejected, deactivated denied, single-flight, boundary expiry.

---

### US-001-06 — Idempotent seed data provisioning (system-level)

**A. Summary**
- **Story ID**: US-001-06 · **Title**: Seed initial roles and accounts on an empty database
- **Actor**: **System** (non-actor) — runs at application startup
- **User story**: *As the system, I want to provision the three roles and one account each for Admin, ProjectManager, and TeamMember on an empty database, so that every environment is immediately usable and demonstrable.*
- **Business value**: Removes the bootstrap deadlock (no users → cannot log in → cannot create users) and gives known credentials for demo/test.
- **Priority**: **P0** · **Reason**: Required by the constitution (IV.5); every other story is testable only once accounts exist.
- **Dependencies**: — (runs first). **Out of scope**: demo Projects/Tasks seeding (owned by features 002/003, though they follow this same idempotent pattern).

**B. Quality validation**
- **INVEST** — Independent ✔; Valuable ✔; Testable ✔ (run twice → identical state, no duplicates).
- **Given/When/Then**
  1. **Given** an empty database (post-migration), **When** the app starts, **Then** the three roles (Admin, ProjectManager, TeamMember) and one user per role are created, each with a hashed password, and a seed `activity_logs` entry is written (actor = system).
  2. **Given** an already-seeded database, **When** the app starts again, **Then** seeding detects existing roles/users and **creates nothing new** (idempotent — no duplicates).
  3. **Given** partial seed state (roles exist, a user missing), **When** the app starts, **Then** only the missing pieces are created.
- **Edge cases**: concurrent startup (two instances) racing to seed (guard via existence checks + unique constraints, so the loser no-ops); seed credentials must come from configuration/user-secrets, not hardcoded secrets in source.
- **Audit/security**: seed passwords hashed; seed credentials from configuration (dev: user secrets; prod: environment/secret store); seeding writes an audit entry with a system actor.
- **Configurability**: seed account emails and (initial) passwords via configuration; whether seeding runs (on by default in dev, gated in prod).

**C. UI** — none (startup routine).

**D. API** — none (invoked from the application's startup pipeline).

**E. DB** — writes **`roles`**, **`users`**, **`user_roles`**, **`activity_logs`**; guarded by existence checks + unique constraints for idempotency.

**F. Separation** — Startup: `IDataSeeder.SeedAsync` invoked once during app initialization. Backend: role ensure → user ensure (per role) → audit. DB: unique constraints back the idempotency. QA: run-twice-no-duplicate, partial-repair, no hardcoded secrets.

---

## Consolidated Data Model (review-level; final physical schema at implementation)

> Code-First (EF Core 10 + Npgsql). PostgreSQL identifiers are **snake_case** (Constitution VIII.2). ASP.NET Core Identity supplies `users`/`roles`/`user_roles` (mapped to snake_case); `refresh_tokens` and the shared `activity_logs` are custom. Every schema change is an EF Core migration with a descriptive name (Constitution IV.2).

| Entity | Table | Purpose | Key fields (type · req/null) | Relationships |
|---|---|---|---|---|
| **User** (`IdentityUser<Guid>`) | `users` | Authoritative user record | `id` uuid PK; `user_name`/`normalized_user_name`; `email`/`normalized_email` (req·unique); `password_hash` (req); `security_stamp`; `concurrency_stamp`; `full_name` (req); `is_active` bool (req·default true); `created_at`/`updated_at` (req) | parent of `refresh_tokens`; *↔* roles via `user_roles` |
| **Role** (`IdentityRole<Guid>`) | `roles` | The three RBAC roles | `id` uuid PK; `name` (req·unique: Admin/ProjectManager/TeamMember); `normalized_name` | *↔* users via `user_roles` |
| **UserRole** | `user_roles` | User ↔ role assignment | `user_id` FK; `role_id` FK (composite PK) | Identity join table; **business rule: exactly one role per user** |
| **RefreshToken** | `refresh_tokens` | Rotating refresh tokens | `id` uuid PK; `user_id` FK (req); `token_hash` (req — never store raw); `expires_at` (req); `created_at` (req); `revoked_at` (null); `replaced_by_token` (null·rotation link) | *→1 `users`; cascade on user delete |
| **ActivityLog** (constitution entity) | `activity_logs` | First-class audit of every write | `id` uuid PK; `actor_id` uuid (null = system); `action` (req — e.g. UserRegistered); `entity_type` (req — e.g. User); `entity_id` (req); `timestamp` (req·UTC); `change_summary` (req) | references acting `users` (soft FK) |

**Schema notes**: passwords exist only as `password_hash` (ASP.NET Core Identity, PBKDF2) — no plaintext column ever. Refresh tokens are stored as `token_hash`, so a database leak does not expose usable tokens. `activity_logs` is the shared audit table defined by the constitution and written by every feature; Auth writes the `User`-targeted rows. Cascade behavior is explicit (Constitution IV.3): deleting a user cascades its refresh tokens; `activity_logs` rows are retained (audit lineage) with `actor_id` nullable.

## Consolidated API Catalog

> Base path `/api`; JSON over HTTPS; errors as **RFC 7807 Problem Details**; documented via **Swagger/OpenAPI** (enabled in development). Authenticated-by-default; anonymous endpoints explicitly marked. Identity (user id, roles) is read from the validated token, never the body.

| Method · Route | Purpose | Auth | Success | Failure |
|---|---|---|---|---|
| `POST /api/auth/register` | Create an account | `[AllowAnonymous]` | **201** + `Location` + safe user DTO | 400 (validation), 409 (duplicate email) |
| `POST /api/auth/login` | Authenticate, issue token pair | `[AllowAnonymous]` | **200** `{ accessToken, expiresAt, user }` + `Set-Cookie` refresh | 401 (invalid/deactivated) |
| `POST /api/auth/refresh` | Rotate token pair | `[AllowAnonymous]` (reads refresh cookie) | **200** `{ accessToken, expiresAt }` + rotated cookie | 401 (expired/revoked/replayed) |
| `POST /api/auth/logout` | Revoke refresh token | `[Authorize]` | **204** | 401 |
| `GET /api/auth/me` | Current user + role | `[Authorize]` | **200** user DTO | 401 |
| `GET /api/health` | Liveness/readiness | `[AllowAnonymous]` | **200** | — |

---

## Technical Design — Server-Side Authentication & Authorization

> The detailed solution for how auth actually works: the components, the exact requests/responses, the step-by-step flows, failure handling, and the security guarantees. Written so a developer can implement it directly.

### T.1 The roles (who is authority, who enforces)
- **The .NET API is the authority.** It owns the user store (ASP.NET Core Identity + PostgreSQL), verifies credentials, issues and validates JWTs, and enforces `[Authorize]` on every endpoint. It never trusts the client for an authorization decision.
- **The Angular frontend is convenience.** Route guards and the NgRx auth state decide what the user *sees and navigates to*; they are never the security boundary. Every request is re-checked by the API.

### T.2 What the token carries
- The **access token** (JWT) is lightweight and short-lived: `sub` (user id), `email`, a single `role` claim (Admin, ProjectManager, or TeamMember), `jti`, `iat`, `exp`, plus `iss`/`aud`. It carries **no password, no secret, no refresh token**.
- The **refresh token** is an opaque, high-entropy random string, stored **hashed** server-side and delivered to the client **only as an httpOnly, Secure, SameSite cookie** (scoped to the auth endpoints). JavaScript can never read it; the browser sends it automatically to `POST /api/auth/refresh` and `POST /api/auth/logout`. It is never placed in the response body.
- The JWT **signing key**, issuer, and audience come from configuration (.NET user secrets in dev; environment variables / secret store in prod) and are **never committed** (Constitution V.4).

### T.3 The endpoints, with concrete examples

**(1) Register**
```
POST /api/auth/register
{ "fullName": "Dana Rivera", "email": "dana@example.com",
  "password": "S3cure-P@ss", "confirmPassword": "S3cure-P@ss" }

→ 201 Created
Location: /api/users/9f1c8e2a-...-b7
{ "id": "9f1c8e2a-...-b7", "fullName": "Dana Rivera",
  "email": "dana@example.com", "role": "TeamMember", "createdAt": "2026-07-22T09:12:00Z" }
```
The response never includes a password or hash. Duplicate email → `409`; weak password → `400` (both as Problem Details).

**(2) Login**
```
POST /api/auth/login
{ "email": "dana@example.com", "password": "S3cure-P@ss" }

→ 200 OK
Set-Cookie: refresh_token=b3f1...e9; HttpOnly; Secure; SameSite=Strict; Path=/api/auth
{ "accessToken": "eyJhbGciOiJIUzI1NiIs...",   // JWT, exp ~15 min; held in memory/NgRx
  "expiresAt": "2026-07-22T09:27:00Z",
  "user": { "id": "9f1c8e2a-...-b7", "fullName": "Dana Rivera",
            "email": "dana@example.com", "role": "TeamMember" } }
```
The refresh token is delivered **only** as the httpOnly cookie — never in the JSON body or readable by JavaScript. Wrong password or unknown email → `401 { "title": "Invalid credentials" }` (generic — no user enumeration).

**(3) Refresh**
```
POST /api/auth/refresh          Cookie: refresh_token=b3f1...e9   (sent automatically)
(no request body)

→ 200 OK
Set-Cookie: refresh_token=c7a2...f0; HttpOnly; Secure; SameSite=Strict; Path=/api/auth
{ "accessToken": "eyJ...new", "expiresAt": "..." }
→ 401      (expired / revoked / replayed refresh token)
```
The old refresh token is revoked and `replaced_by_token` points at the new one (single-use rotation); the rotated token replaces the cookie.

**(4) Check-protected (`me`)**
```
GET /api/auth/me      Authorization: Bearer eyJ...

→ 200 { "id": "...", "email": "dana@example.com", "role": "TeamMember" }
→ 401 (no/invalid/expired access token)
```

**(5) Logout**
```
POST /api/auth/logout   Authorization: Bearer eyJ...   Cookie: refresh_token=c7a2...f0
(no request body)

→ 204 No Content
Set-Cookie: refresh_token=; Max-Age=0        (cookie cleared; token revoked_at set server-side)
```

### T.4 How a protected request flows (step by step)
1. The user is logged in; the JWT interceptor attaches `Authorization: Bearer <accessToken>` to every outgoing request.
2. The request hits a feature endpoint, e.g. `DELETE /api/projects/{id}` annotated `[Authorize(Roles = "Admin,ProjectManager")]`.
3. ASP.NET Core validates the JWT signature + `exp` and materializes the `ClaimsPrincipal` (including role claims) **before** the action runs.
4. The authorization middleware evaluates the attribute: role satisfied → action runs; not satisfied → **403**; token missing/invalid → **401**. No `if`-checks inside the method.
5. If the action writes to `Users`, `IActivityLogService.LogAsync` records the change (actor from the token, action, entity, id, timestamp, summary).

### T.5 Token-expiry & refresh flow (client + server)
1. Access token expires; the next request returns **401**.
2. The Angular **401 interceptor** pauses, calls `POST /api/auth/refresh` **once** with `withCredentials` (the httpOnly refresh cookie is sent automatically; single-flight — concurrent 401s share the one refresh), and on success updates NgRx and **retries** the original request transparently.
3. Server validates the refresh token (exists, not expired, not revoked, user still active), rotates it, and returns a new pair.
4. If refresh fails (**401**), the interceptor dispatches logout; the route guard redirects to login. The user re-authenticates.

### T.6 API behaviour rules
- **Authenticated by default:** a global fallback authorization policy requires an authenticated user; only the four `[AllowAnonymous]` endpoints (register, login, refresh, health) are public.
- **Status codes** (Constitution VI.2): 201 + `Location` on create; 200 on read/success; 204 on logout; 400 validation; 401 auth; 403 role; 409 duplicate; 500 server. All error bodies are **Problem Details** JSON — never plain strings or HTML.
- **Idempotent/read-only where applicable:** `me`, `health` never mutate; `refresh` mutates only the token store.
- **Versionable:** URLs are designed so a future `/api/v1` prefix can be added without breaking clients (Constitution VI.1) — not adopted now.

### T.7 Failure handling (fail-safe)
- **Unknown/invalid/expired access token → 401** (never silently allowed).
- **Valid token, wrong role → 403.**
- **Refresh with a bad/replayed token → 401**, session cleared client-side.
- **Deactivated user** cannot log in *or* refresh, even with otherwise-valid credentials/tokens.
- **Uncaught server error → 500** as Problem Details; the global Angular `ErrorHandler` + HTTP `ErrorInterceptor` funnel it to the notification component (Constitution VII.7).

### T.8 Security guarantees
- Passwords hashed by ASP.NET Core Identity; **never** stored in plaintext, logged, or returned (Constitution V.3).
- Refresh tokens stored **hashed** and **single-use** (rotated); logout revokes them server-side. The refresh token is transported **only as an httpOnly, Secure, SameSite cookie** (never JS-readable, never in the body); the cookie-authenticated `/refresh` and `/logout` endpoints are **CSRF-protected** (SameSite + anti-forgery token).
- JWT signed with a secret from user secrets / environment; signing key and connection string never committed (Constitution V.4).
- Every endpoint authenticated by default; anonymous endpoints explicitly `[AllowAnonymous]` (Constitution V.1); role checks via attributes only (V.2).
- **CORS** restricted to an explicit allow-list of origins; no wildcard outside local development (Constitution V.6).
- Server-side validation is authoritative; the frontend validates only for UX (Constitution V.5).
- Every write to `Users` is audited to `activity_logs` (Constitution IV.4).

---

## Implementation Blueprint (build-ready detail)

> Everything the team needs to build this feature: concrete schema, enums, service interfaces, configuration, error model, NFRs, the audit catalog, and the Definition of Done.

### B.1 Concrete schema (DDL-level intent; expressed as EF Core migrations)
> PostgreSQL 18 via Npgsql. snake_case identifiers. All timestamps `timestamptz`, UTC. Identity tables shown with the custom columns this feature adds.

**`users`** (extends `IdentityUser<Guid>`)
- `id` uuid **PK**
- Identity columns: `user_name`, `normalized_user_name`, `email`, `normalized_email` (**unique index**), `password_hash`, `security_stamp`, `concurrency_stamp`, `email_confirmed`, `lockout_end`, `lockout_enabled`, `access_failed_count`
- `full_name` varchar(200) **NOT NULL**
- `is_active` boolean **NOT NULL DEFAULT true**
- `created_at` timestamptz **NOT NULL** · `updated_at` timestamptz **NOT NULL**
- **Concurrency**: PostgreSQL `xmin` mapped as an EF Core row-version token (ADR-0004); a stale write → **409**
- **INDEX** unique(`normalized_email`)

**`roles`** (`IdentityRole<Guid>`): `id` uuid PK · `name` · `normalized_name` (**unique**). Seeded rows: Admin, ProjectManager, TeamMember.

**`user_roles`**: `user_id` uuid FK→`users` · `role_id` uuid FK→`roles` · **PK**(`user_id`,`role_id`).

**`refresh_tokens`**
- `id` uuid **PK**
- `user_id` uuid **NOT NULL** FK→`users` (**ON DELETE CASCADE**)
- `token_hash` varchar(256) **NOT NULL** (SHA-256 of the opaque token)
- `expires_at` timestamptz **NOT NULL**
- `created_at` timestamptz **NOT NULL**
- `revoked_at` timestamptz **NULL**
- `replaced_by_token` varchar(256) **NULL** (hash of the successor)
- **INDEX** (`user_id`), unique(`token_hash`)

**`activity_logs`** (shared constitution entity; Auth writes User-targeted rows)
- `id` uuid **PK**
- `actor_id` uuid **NULL** (null = system/seed)
- `action` varchar(100) **NOT NULL**
- `entity_type` varchar(100) **NOT NULL**
- `entity_id` varchar(64) **NOT NULL**
- `timestamp` timestamptz **NOT NULL**
- `change_summary` varchar(1000) **NOT NULL**
- **INDEX** (`entity_type`,`entity_id`), (`actor_id`), (`timestamp`)

### B.2 Enumerations (fixed value sets)
- **Role**: `Admin, ProjectManager, TeamMember`
- **AuditAction** (User): `UserRegistered, UserLoggedIn, UserLoggedOut, TokenRefreshed, UserDeactivated, UserSeeded`
- **TokenType** (internal): `Access, Refresh`

### B.3 Service interfaces & method signatures (C#; nullable reference types on)
```csharp
public interface IAuthService {
    // Register: validate → Identity CreateAsync (hash) → assign role → audit. Never returns a password.
    Task<Result<UserDto>>      RegisterAsync(RegisterRequest req, CancellationToken ct);
    // Login: verify credentials → issue access+refresh pair → audit. Generic failure (no enumeration).
    Task<Result<AuthTokens>>   LoginAsync(LoginRequest req, CancellationToken ct);
    // Refresh: validate+rotate refresh token → new pair. Deactivated user → fail.
    Task<Result<AuthTokens>>   RefreshAsync(string refreshToken, CancellationToken ct);
    // Logout: revoke the presented refresh token (idempotent) → audit.
    Task<Result>               LogoutAsync(Guid userId, string refreshToken, CancellationToken ct);
}

public interface ITokenService {
    string      CreateAccessToken(User user, string role);                   // signs JWT (single role claim) from config key
    string      CreateRefreshToken();                                        // opaque; caller stores the hash
    Task<RefreshToken?> ValidateRefreshTokenAsync(string presented, CancellationToken ct); // null = invalid
}

public interface IActivityLogService {   // Constitution IV.4 — every write to Users audited
    Task LogAsync(Guid? actorId, string action, string entityType, string entityId,
                  string changeSummary, CancellationToken ct);
}

public interface IDataSeeder {           // US-001-06 — idempotent
    Task SeedAsync(CancellationToken ct); // ensure roles, then ensure one user per role; no duplicates
}
// AuthTokens { string AccessToken; string RefreshToken; DateTimeOffset ExpiresAt; UserDto User; }
// UserDto    { Guid Id; string FullName; string Email; string Role; } // exactly one role; never a password
// Transport: the controller reads the refresh token from the httpOnly cookie and writes the rotated
// token back as a Set-Cookie; AuthTokens.RefreshToken is never serialized into the response body.
// Result<T>, Error/ErrorKind, and CurrentUser are defined once in docs/shared-contracts.md (ADR-0003);
// a shared mapper converts ErrorKind to the status codes below — services never throw for expected outcomes.
```

### B.4 Configuration (never hardcoded; secrets never committed)
- `Jwt:SigningKey` (user secrets / env), `Jwt:Issuer`, `Jwt:Audience`
- `Jwt:AccessTokenMinutes` (default short, e.g. 15), `Jwt:RefreshTokenDays` (default e.g. 7)
- `Identity:Password:*` (min length, complexity), `Identity:Lockout:*`
- `Cors:AllowedOrigins` (explicit list; no wildcard). **Same-origin deployment** (ADR-0002): the API serves the Angular bundle and `ng serve` proxies `/api`, so CORS is inert in practice and the refresh cookie stays `SameSite=Strict` in every environment
- `RefreshCookie:{Name,SameSite,Secure,Path}` and `Csrf:*` (anti-forgery for the cookie-authenticated `/refresh` and `/logout`)
- `Seed:Enabled`, `Seed:Admin/ProjectManager/TeamMember:{Email,Password}` (dev via user secrets)
- Connection string via user secrets (dev) / environment (prod)

### B.5 Error model (RFC 7807 Problem Details)
`{ type, title, status, detail, traceId, errors? }`. Mapping: `400` validation (with per-field `errors`), `401` `Invalid credentials` / `Authentication required`, `403` `Forbidden`, `409` `Email already registered`, `500` `Unexpected error`. Never leak which credential field was wrong; never include stack traces in production responses.

### B.6 Non-functional requirements
- **Security:** password hashing (Identity/PBKDF2); tokens signed; refresh tokens hashed + rotated; deny-by-default; CORS allow-list; secrets out of source.
- **Performance:** login/refresh are single-round-trip DB operations; JWT validation is stateless (no DB hit on protected reads).
- **Observability:** structured logging via **Serilog** (console + rolling files); auth failures logged **without** credentials.
- **Testability (Constitution IX):** services unit-tested per branch (xUnit); each controller happy-path + one error-path via `WebApplicationFactory`; frontend guards/interceptors/validators via Jasmine+Karma.

### B.7 Audit event catalog (→ `activity_logs`)
Emit `(actor_id, action, entity_type=User, entity_id, timestamp, change_summary)` for: **register** (`UserRegistered`), **login** (`UserLoggedIn`), **logout** (`UserLoggedOut`), **refresh** (`TokenRefreshed`), **deactivation** (`UserDeactivated`), **seed** (`UserSeeded`, actor = system/null). Append-only; never contains passwords or raw tokens.

### B.8 Definition of Done
1. Register/login/logout/refresh work end-to-end; a user can authenticate and reach a protected endpoint; the password is never returned or logged.
2. Every endpoint is authenticated by default; register/login/refresh/health are the only anonymous endpoints (verified by test).
3. `[Authorize(Roles=...)]` enforces the 401/403 matrix across Admin/ProjectManager/TeamMember (integration-tested); no in-body role checks exist.
4. Access tokens expire; refresh rotates single-use tokens; replayed/revoked/expired refresh tokens are rejected; the frontend refreshes transparently (single-flight) and redirects to login on failure.
5. Seeding is idempotent — a second startup creates no duplicates; partial state is repaired; no secrets are hardcoded.
6. Every write to `Users` produces an `activity_logs` row.
7. CORS is an explicit allow-list; JWT key + connection string come from user secrets/env, not source control.
8. The Angular `auth` route group is lazy-loaded with standalone components (no `@NgModule`); functional JWT + 401 interceptors and functional route guards are in place; forms are Reactive with validators and the shared error-display component; auth state lives in NgRx.
9. Swagger/OpenAPI documents all endpoints; the backend compiles warnings-as-errors with nullable enabled; the frontend compiles in strict mode.
10. Unit + integration tests pass (Constitution IX.3 — no merge on failing tests).

### B.9 Open questions for review
| # | Question | Recommendation | Blocks build? |
|---|---|---|---|
| OQ-001-01 | Access / refresh token lifetimes? | 15 min access, 7 day refresh — configurable | No |
| OQ-001-02 | Is self-registration open, or Admin-only account creation? | **Resolved (Clarifications 2026-07-22):** open self-registration → always `TeamMember`; privileged-role creation is Admin-only, owned by feature 004 | — |
| OQ-001-03 | Refresh-token storage: hashed random string (chosen) vs JWT refresh? | Opaque random, stored **hashed**, single-use rotation | No |
| OQ-001-04 | Does logout revoke the current device's refresh token or all of them? | Current token only (config flag for all-devices) | No |
| OQ-001-05 | Where is the refresh token held on the client? | **Resolved (Clarifications 2026-07-22):** access token in memory/NgRx; refresh token in an httpOnly Secure SameSite cookie; `/refresh` & `/logout` CSRF-protected | — |
| OQ-001-06 | Account lockout thresholds? | Identity lockout on, e.g. 5 attempts / 15 min — configurable | No |
| OQ-001-07 | Should `activity_logs` be write-partitioned as it grows? | Defer; index by (`entity_type`,`entity_id`) for now | No |

---

## Functional Requirements

- **FR-001**: The system MUST let a visitor register with full name, email, and a policy-valid password; the password MUST be hashed (ASP.NET Core Identity) and MUST NOT be returned or logged. Self-registration MUST always assign the `TeamMember` role and MUST ignore any client-supplied role; creating `ProjectManager`/`Admin` accounts is an Admin-only action (feature 004).
- **FR-002**: Email MUST be unique (normalized); duplicate registration MUST return **409**.
- **FR-003**: The system MUST authenticate valid credentials and return a signed JWT access token (with `sub`, `email`, `role` claims) plus a refresh token; invalid credentials MUST return a **generic 401** (no user enumeration).
- **FR-004**: A deactivated account MUST NOT be able to log in or refresh.
- **FR-005**: The system MUST support logout that revokes the presented refresh token server-side and returns **204**.
- **FR-006**: Access tokens MUST expire; a valid refresh token MUST issue a new access+refresh pair with **single-use rotation**; expired/revoked/replayed refresh tokens MUST return **401**.
- **FR-007**: Every API endpoint MUST require a valid JWT by default; only register, login, refresh, and health checks MUST be `[AllowAnonymous]`.
- **FR-008**: Role-restricted endpoints MUST enforce roles via `[Authorize(Roles = "...")]` attributes; ad-hoc role checks in method bodies are PROHIBITED. Wrong role MUST return **403**. Each user MUST hold **exactly one** role (Admin, ProjectManager, or TeamMember).
- **FR-009**: Angular route guards MUST be the only client-side mechanism for blocking navigation; component-level redirect logic is PROHIBITED.
- **FR-010**: The `auth` feature area MUST be lazy-loaded via route-level code splitting (standalone components, no `@NgModule` — ADR-0001); a functional JWT interceptor MUST attach the token to outgoing requests; a second functional interceptor MUST clear the session and redirect to login on **401**.
- **FR-011**: Auth forms MUST use Reactive Forms with explicit validators, surfacing errors through a consistent error-display component; auth session/current user MUST be held in NgRx.
- **FR-012**: Seeding MUST provision three roles and one Admin, one ProjectManager, one TeamMember on an empty database, MUST be idempotent (no duplicates on re-run), and MUST take seed credentials from configuration, not hardcoded secrets.
- **FR-013**: Every write to `Users` MUST create an `activity_logs` entry (actor, action, entity type, entity id, timestamp, change summary).
- **FR-014**: Errors MUST be returned as RFC 7807 Problem Details JSON; endpoints MUST be documented via Swagger/OpenAPI.
- **FR-015**: CORS MUST use an explicit origin allow-list (no wildcard outside local dev); the JWT signing key and connection string MUST come from user secrets (dev) or environment/secret store (prod) and MUST NOT be committed.
- **FR-016**: The refresh token MUST be transported only as an httpOnly, Secure, SameSite cookie (never JavaScript-readable, never in the response body); the cookie-authenticated `/refresh` and `/logout` endpoints MUST be CSRF-protected (SameSite + anti-forgery).

## Non-Functional Requirements
- **NFR-001**: Server-side enforcement is authoritative; the frontend is never trusted for authorization.
- **NFR-002**: Protected-endpoint authorization is stateless (JWT validation, no DB round-trip) for read paths.
- **NFR-003**: Structured logging (Serilog) to console + rolling files; no credentials or raw tokens in logs.
- **NFR-004**: Nullable reference types + warnings-as-errors (backend); TypeScript strict mode (frontend).
- **NFR-005**: Secrets never in source control; configuration-driven lifetimes/policies/origins.

## Configurability Rules
- **CFG-001**: Access-token and refresh-token lifetimes (config).
- **CFG-002**: Password policy and lockout thresholds (Identity options).
- **CFG-003**: Default self-registration role and whether self-registration is open (config).
- **CFG-004**: Logout scope — current refresh token vs all of the user's tokens (config).
- **CFG-005**: CORS allowed origins (config, per environment).
- **CFG-006**: Seed enable flag + seed account emails/passwords (config / user secrets).

## Security Rules
- Authenticated-by-default; anonymous endpoints explicitly marked; roles via attributes only (no in-body checks).
- Passwords hashed, never logged/returned; refresh tokens hashed + single-use, delivered only via httpOnly Secure SameSite cookie; `/refresh` & `/logout` CSRF-protected; logout revokes server-side.
- JWT signed with a non-committed key; deactivated users denied; generic auth-failure messages.
- CORS allow-list; secrets from user secrets/env; server-side validation authoritative.

## Audit / Compliance Expectations
Audit every user write — register, login, logout, refresh, deactivation, seed — with actor, action, entity type/id, timestamp, and change summary to `activity_logs`. Append-only; no passwords, no raw tokens.

## Assumptions
- ASP.NET Core Identity + EF Core 10 + Npgsql against PostgreSQL 18 is the fixed stack (Constitution III); migrations are Code-First.
- The `activity_logs` table is the shared constitution audit entity; this feature writes User-targeted rows and other features write theirs.
- HTTPS terminates in front of the API; JSON is the sole content type (Constitution II.3).
- The app is deployed **same-origin** — the API serves the Angular bundle, and the dev server proxies `/api` (ADR-0002) — which is what lets the refresh cookie use `SameSite=Strict` everywhere.
- The frontend holds the access token in NgRx/memory; the refresh token is delivered as an httpOnly Secure SameSite cookie (resolved — Clarifications 2026-07-22).

## Dependencies
- **Consumed by** features 002 Projects, 003 Tasks, 004 Team, 005 Dashboard, 006 Reports — each declares its role requirement via `[Authorize(Roles=...)]` and relies on the JWT/role claims this feature issues.
- **Infrastructure**: PostgreSQL 18; Serilog; Swagger/OpenAPI; .NET user secrets (dev) / secret store (prod).

## Out of Scope
SSO / external identity providers; social login; MFA; password-reset and email-verification flows; user-profile management beyond authentication; resource-level ownership rules (owned by each resource's feature).

---

## Template Note

This file is the **structural template** for the ProjectManagementApp feature specs. On confirmation, the identical structure and the merged-file convention (requirements + solution design in one file) will be applied to **002 Projects, 003 Tasks, 004 Team, 005 Dashboard, 006 Reports**.
