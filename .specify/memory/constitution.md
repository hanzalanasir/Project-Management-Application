<!--
v1.3.0 — ratified 2026-07-20, last amended 2026-07-29.
  v1.0.0  Initial adoption: principles I–XI + Governance.
  v1.1.0  VII.1/VII.2 amended — Angular feature areas are standalone + route-level lazy
          loading instead of @NgModule. Rationale, alternatives, and backward-compatibility
          note in docs/adr/0001-angular-standalone-components.md. MINOR: guidance materially
          changed; no existing compliant work invalidated (no frontend code written yet).
  v1.1.1  PATCH — two wording fixes, no principle redefined: (a) V.1 exempts token refresh
          from the JWT requirement alongside registration/login/health checks, since refresh
          issues a new token after the old one has expired and cannot itself require a valid
          one; (b) VI.2 adds 409 for conflict (duplicate resource or a stale concurrency
          token), already implied by IV.3's "explicit and intentional" cascade/concurrency
          handling and used by specs 001–003. No existing compliant work invalidated.
  v1.2.0  MINOR — three architecture principles added/redefined to adopt Vertical Slice
          Architecture, Clean Architecture, and an API-first workflow. (a) II.2 REPLACED:
          the layered Controllers/Services/Repositories description → self-contained vertical
          slices under Features/<Area>/<UseCase>/ with Clean Architecture inward-pointing
          dependencies; controllers become thin MediatR.Send() endpoint mappings. (b) IV.1
          amended: a slice's handler MAY use the DbContext directly; a Repository is optional,
          not required. (c) X.2 REPLACED: the OpenAPI contract is authored and reviewed
          before its handler (API-first), versioned under /docs/contracts/; code is validated
          against the contract. (d) VII.3 amended: frontend HTTP service classes MAY be
          generated from the contract via openapi-generator. (e) III gains MediatR in the
          backend stack. Rationale, alternatives, and backward-compatibility note in
          docs/adr/0006-vertical-slice-clean-architecture-api-first.md. MINOR (not MAJOR):
          II.2 is redefined, but the Governance §3 MAJOR trigger also requires that the change
          "invalidates existing compliant work" — none exists yet (specs 001–006 are designs,
          not implemented code; no backend code written), consistent with the v1.1.0 precedent.
  OUTSTANDING (v1.2.0): specs 001–006 were drafted under the prior layered II.2 wording
          (Controllers/Services/Repositories) and MUST get a revision pass against the new
          vertical-slice / Clean Architecture / API-first principle BEFORE /speckit.plan is run
          against them. This amendment does NOT rewrite those specs; the follow-up is open.
          (v1.3.0 promoted this from a comment note to enforceable Governance §5.)
  v1.3.0  MINOR — testing wording realigned + the pre-v1.2.0 spec-revision follow-up made
          enforceable. (a) IX.1 reworded: backend unit tests now target Handlers (and shared
          cross-cutting services such as ITokenService/IActivityLogService) rather than
          "Services and business logic", tracking the v1.2.0 II.2 move of business logic into
          vertical-slice handlers; the WebApplicationFactory integration-test sentence is
          unchanged (a thin controller is still the tested entry point). (b) New Governance §5
          adds a blocking compliance-gate rule: specs 001–006 MUST be revised against the
          vertical-slice/Clean/API-first wording (II.2/IV.1/X.2/VII.3) — or waived in writing —
          before /speckit.plan runs against them. MINOR: a new Governance sub-item is added
          (§3's own definition of MINOR); IX.1 alone is a clarification, but the higher bump
          governs. No existing compliant work invalidated (no code written).
On amendment: bump the version (semver) and "Last Amended" date below, and re-check
.specify/templates/{plan,spec,tasks}-template.md for consistency.
-->

# ProjectManagementApp Constitution

ProjectManagementApp is a full-stack project management application (Angular + .NET Web API
+ PostgreSQL; JWT/RBAC auth; IIS deployment). This constitution governs **how** it is built;
the assignment brief governs **what** it does.

## Core Principles

### I. Scope Fidelity

1. The source of truth for what this application must do is the assignment brief. This
   constitution governs *how* it is built, not *what* it does. Deviation from the brief's
   required features (Dashboard, Projects, Tasks, Team, Reports, Authentication modules;
   JWT + RBAC; PostgreSQL tables Users / Projects / Tasks / TeamMembers / ActivityLogs;
   IIS deployment) MUST have explicit written justification.

2. Bonus features called out in the brief — Gantt charts, notifications, Slack/email
   integration, advanced search, role-based dashboards, real-time updates — are OUT OF
   SCOPE for the initial delivery, but architecture decisions MUST NOT preclude them.
   Do not build them; do not build in a way that would require rewriting to add them later.

3. "Nice to have" items in the brief (Docker, CI/CD, cloud deployment) are aspirational.
   If implemented, they MUST NOT weaken any principle in this constitution.

### II. Architecture Principles

1. Three-tier separation is strict: Angular is presentation only, the .NET API owns all
   business logic and authorization, PostgreSQL is the sole source of persistent state.
   The frontend MUST NOT talk to the database directly. No business rules live in the
   frontend.

2. Each feature/use-case is a self-contained **vertical slice** under
   `Features/<Area>/<UseCase>/`, holding its request, handler, validator, and response shape
   — not split horizontally across shared Controllers/Services/Repositories folders.
   Controllers are thin endpoint mappings only, routing one HTTP verb to a `MediatR.Send()`
   call, and MUST NOT contain business logic. Within and across slices, dependencies point
   inward per **Clean Architecture**: Domain depends on nothing; a slice's handler
   (Application) depends only on Domain and on abstractions it defines; Infrastructure
   implements those abstractions. See
   `docs/adr/0006-vertical-slice-clean-architecture-api-first.md`.

3. All communication between frontend and backend is JSON over HTTPS. The API is RESTful
   with predictable resource-oriented URLs (`/api/projects`, `/api/projects/{id}/tasks`).
   Verbs are HTTP verbs, not URL segments.

4. The architecture MUST NOT preclude adding real-time updates (SignalR or similar) later,
   even though real-time is bonus scope.

### III. Technology Stack (Locked)

The following stack is locked. Alternative libraries for the same purpose require an
amendment to this constitution or an ADR.

- **Frontend**: Angular 22, Node.js 24, npm 11, TypeScript strict mode, SCSS.
- **UI component library**: Angular Material is the default choice; Bootstrap is an
  acceptable alternative, but only one MUST be used — the two MUST NOT be mixed.
- **State management**: A state management library is REQUIRED for cross-module state
  (auth session, current user, active project). NgRx is the default; alternatives (NGXS,
  Akita, signal-based store) are acceptable if justified in an ADR.
- **Forms**: Reactive Forms only; template-driven forms are PROHIBITED.
- **Charts**: Chart.js is the default; D3.js is acceptable if a chart requires
  capabilities Chart.js cannot provide.
- **Export**: jsPDF for PDF export; a lightweight CSV utility (papaparse or hand-rolled)
  for CSV.
- **Backend**: .NET 10 SDK, ASP.NET Core Web API, C# with nullable reference types enabled.
- **Mediation**: MediatR, for the command/query + handler pattern used by vertical slices.
- **ORM**: Entity Framework Core 10 with the Npgsql provider; Code-First workflow with
  migrations.
- **Authentication**: JWT bearer tokens issued by the API; ASP.NET Core Identity for user
  and role management.
- **Database**: PostgreSQL 18, accessed exclusively through EF Core.
- **Logging**: Serilog with structured logging to console and rolling files.

### IV. Data Access Principles

1. All database access goes through the EF Core DbContext. Raw SQL is PROHIBITED except
   for reporting queries where LINQ is demonstrably insufficient; in that case the SQL MUST
   be parameterized and reviewed. A slice's handler MAY call the DbContext directly as its
   default persistence path; a separate Repository is optional, not required, and this
   supersedes any contrary reading of III/II.2's prior wording.

2. Every schema change is expressed as an EF Core migration. Manual DDL (`ALTER TABLE`,
   `CREATE TABLE` run by hand against the DB) is PROHIBITED. Migration names are descriptive
   (e.g., `AddProjectStatusColumn`), not timestamps alone.

3. The five domain entities are Users, Projects, Tasks, TeamMembers, and ActivityLogs,
   matching the brief. Relationships are modeled with foreign keys and navigation
   properties. Cascade behavior is explicit and intentional.

4. Every write operation to a domain entity MUST create an ActivityLog entry capturing the
   actor, action, entity type, entity id, timestamp, and a brief change summary. The audit
   log is a first-class feature, not a bolt-on.

5. Seed data is REQUIRED. A dedicated seeding routine runs on empty databases, populating
   at least one Admin user, one ProjectManager user, one TeamMember user, and a small set
   of demo projects with tasks. Seeding MUST be idempotent — running it twice does not
   duplicate data.

### V. Security and Authorization

1. Every API endpoint except registration, login, token refresh, and health checks REQUIRES
   a valid JWT. Anonymous endpoints are explicitly marked with `[AllowAnonymous]`. The
   default is authenticated. (Token refresh is necessarily anonymous: its purpose is to
   issue a new token once the caller's current one has expired, so it cannot itself demand
   a valid one — the refresh token, not the JWT, is what it validates.)

2. Authorization is role-based with at minimum three roles: Admin, ProjectManager,
   TeamMember. Endpoint role requirements are declared with `[Authorize(Roles = "...")]`
   attributes. Ad-hoc role checks inside method bodies are a code smell — prefer attributes
   and policies.

3. Passwords are never stored in plaintext, never logged, and never returned by any
   endpoint. ASP.NET Core Identity handles hashing.

4. Secrets (JWT signing key, database connection string, third-party keys) are never
   committed to source control. Development uses .NET user secrets; production uses
   environment variables or a secret store.

5. Input validation happens at the API boundary using data annotations (FluentValidation
   is acceptable but not required). The frontend also validates for UX purposes, but the
   frontend is NOT trusted — server-side validation is authoritative.

6. CORS is configured with an explicit allow-list of origins. Wildcard origins are
   PROHIBITED outside local development.

### VI. API Design Conventions

1. Base path is `/api`. Resource-oriented URLs, plural nouns (`/api/projects`,
   `/api/tasks`). URL versioning is not required by the brief and is not adopted for the
   initial release, but the API MUST be designed such that versioning can be added later
   without breaking existing clients.

2. Standard REST verbs and status codes: GET returns 200 or 404; POST returns 201 with a
   `Location` header; PUT returns 200 or 204; DELETE returns 204; validation failures
   return 400; auth failures return 401; forbidden returns 403; server errors return 500.
   Conflict (duplicate resource, or a stale concurrency token) returns 409.

3. Errors are returned as RFC 7807 Problem Details JSON, never as plain strings or HTML
   fragments.

4. Endpoints that return collections support pagination via `?page` and `?pageSize` query
   parameters when the result set can grow beyond 50 items. Filtering and sorting
   parameters are added as needed.

5. All endpoints are documented via Swagger/OpenAPI. Swagger UI is enabled in development.

6. The exact endpoints named in the brief (`GET /projects`, `GET /projects/{id}`,
   `POST /projects`, `PUT /projects/{id}`, `DELETE /projects/{id}`) MUST exist. Task and
   team management follow the same pattern. Reporting endpoints are separate under
   `/api/reports`.

### VII. Frontend Conventions

1. Feature areas are lazy-loaded via route-level code splitting (`loadChildren` pointing at a
   feature's route file). The initial bundle contains only the app shell, authentication, and
   the dashboard. Everything else loads on demand.

2. Components are **standalone** (Angular 22's default); `@NgModule` MUST NOT be used for
   feature organization. The feature areas required by the brief are Dashboard, Projects
   (Project Management), Tasks (Task Management), Team (Team Management), Reports, and Auth —
   each a lazy-loaded route group in its own directory. Reusable presentational components
   live under `shared/`; application-wide singletons (interceptors, guards, services provided
   once) are registered under `core/` through the application config providers.
   See `docs/adr/0001-angular-standalone-components.md`.

3. HTTP calls live in dedicated service classes, never in components. Components consume
   services; services use HttpClient. These service classes MAY now be generated from the
   OpenAPI contract via openapi-generator rather than hand-written.

4. An HTTP interceptor attaches the JWT to outgoing requests automatically. A second
   interceptor handles 401 responses by clearing the session and redirecting to login.

5. Route guards protect authenticated and role-restricted routes. Guards are the only
   mechanism for blocking navigation — component-level "if not logged in, redirect" logic
   is PROHIBITED.

6. Reactive forms are used throughout. Every form has explicit validators; validation
   errors surface next to the offending field via a consistent error-display component.

7. Global error handling is centralized: an HTTP ErrorInterceptor catches API errors; a
   global Angular ErrorHandler catches uncaught exceptions; both funnel to a notification
   component (snackbar or toast).

8. Reports support export to PDF and CSV as required by the brief. The export logic lives
   in a report-export service, not in individual report components.

### VIII. Code Quality Standards

1. C# compiles with warnings-as-errors and nullable reference types enabled. TypeScript
   compiles in strict mode with no `any` unless justified by a comment.

2. Naming: PascalCase for C# types and members; camelCase for TypeScript variables and
   functions; kebab-case for Angular selectors, filenames, and URL segments; snake_case for
   PostgreSQL identifiers.

3. Public C# APIs (controllers, services) and public TypeScript service methods have doc
   comments explaining purpose, parameters, and return values.

4. No commented-out code is committed. No `console.log` is left in TypeScript. No
   `Console.WriteLine` is left in C# — use the logger.

5. Commit messages follow Conventional Commits (`feat:`, `fix:`, `refactor:`, `docs:`,
   `test:`, `chore:`).

### IX. Testing Standards

1. **Backend**: xUnit for unit tests targeting Handlers (and any shared cross-cutting
   services, e.g. `ITokenService` / `IActivityLogService`) — every Handler with a conditional
   branch has at least one test per branch. Integration tests using `WebApplicationFactory`
   cover each controller's happy path and one error path (a controller is still the tested
   entry point even though it is now a thin `MediatR.Send()` wrapper).

2. **Frontend**: Jasmine + Karma unit tests for services, guards, and reactive form
   validators. Component tests are required for any component with logic beyond template
   rendering.

3. Merging code with failing tests is PROHIBITED.

4. Test data is created via builders or factories, not inline object literals scattered
   across files.

### X. Documentation Requirements

1. The repository root contains a `README.md` with: project overview, prerequisites, setup
   for backend and frontend, migration commands, test commands, and end-to-end run
   instructions.

2. The OpenAPI contract is authored and reviewed as part of a feature's spec/plan, **before**
   its handler is implemented, versioned under `/docs/contracts/`. Code is validated against
   the contract, not the other way around. (VI.5's Swagger UI requirement is unaffected — it
   still applies for local exploration; only the authoring direction changes.)

3. A technical/architecture document under `/docs` describes the overall architecture (as
   the brief's deliverables require). Significant architectural decisions (choice of state
   management library, choice of UI framework, deviations from the brief) are recorded as
   short ADR files under `/docs/adr/`.

4. The database schema is documented via an entity-relationship diagram checked into
   `/docs` and updated when migrations change relationships.

5. A demo script or video walkthrough is prepared as required by the brief's Demo
   deliverable.

### XI. Deployment

1. The primary deployment target is IIS on Windows Server, per the brief. The API is
   published as a self-contained deployment; the Angular app is built via
   `ng build --configuration production` and served either as static files by IIS or by the
   API's static file middleware.

2. Environment-specific configuration comes from `appsettings.{Environment}.json` for the
   backend and `environment.{name}.ts` for the frontend. Hardcoded environment values are
   PROHIBITED.

3. Deployment instructions are part of the required documentation deliverable.

4. Dockerization, CI/CD via GitHub Actions, and cloud deployment (Azure/AWS/GCP) are
   nice-to-haves. If implemented, they MUST NOT weaken any principle above.

## Governance

1. This constitution is the highest authority in the project. Any code, dependency, or
   design choice violating a principle above MUST either be corrected or the constitution
   MUST be formally amended first — never both silently.

2. Amendments require: (a) a written rationale, (b) an explicit description of what changes
   and why, (c) a backward-compatibility note for existing code, and (d) an update of this
   file with an incremented version number and date.

3. **Versioning policy** — this constitution is versioned with semantic versioning:
   - **MAJOR**: Backward-incompatible governance changes — a principle is removed or
     redefined in a way that invalidates existing compliant work.
   - **MINOR**: A new principle or section is added, or existing guidance is materially
     expanded.
   - **PATCH**: Clarifications, wording, and typo fixes that do not change meaning.

4. **Compliance gates** — when Spec-Kit runs constitution checks during `/speckit.plan`
   and `/speckit.implement`, any MUST violation blocks progression until resolved.
   Complexity that appears to conflict with a principle MUST be justified in the plan's
   Complexity Tracking section or corrected.

5. **Pre-v1.2.0 spec revision gate** — specs 001–006 were drafted under the pre-v1.2.0
   layered wording of II.2 (Controllers / Services / Repositories) and code-first Swagger
   (old X.2). Each such spec MUST get a revision pass reconciling it with the vertical-slice
   II.2, the direct-DbContext IV.1, the API-first X.2, and the contract-generated-services
   VII.3 wording **before** `/speckit.plan` may run against it. Until a spec is either revised
   or explicitly waived in writing (rationale recorded in the plan's Complexity Tracking
   section), running `/speckit.plan` against it is a **blocking MUST violation** under the §4
   compliance gate.

**Version**: 1.3.0 | **Ratified**: 2026-07-20 | **Last Amended**: 2026-07-29
