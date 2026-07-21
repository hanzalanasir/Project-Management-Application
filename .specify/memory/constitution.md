<!--
v1.0.0 — ratified 2026-07-20. Initial adoption: principles I–XI + Governance.
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

2. The backend follows a layered architecture: Controllers handle HTTP; Services hold
   business logic; DbContext / repositories handle data access; Entities represent the
   domain. Controllers MUST NOT contain business logic beyond model validation and calling
   services.

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
- **ORM**: Entity Framework Core 10 with the Npgsql provider; Code-First workflow with
  migrations.
- **Authentication**: JWT bearer tokens issued by the API; ASP.NET Core Identity for user
  and role management.
- **Database**: PostgreSQL 18, accessed exclusively through EF Core.
- **Logging**: Serilog with structured logging to console and rolling files.

### IV. Data Access Principles

1. All database access goes through the EF Core DbContext. Raw SQL is PROHIBITED except
   for reporting queries where LINQ is demonstrably insufficient; in that case the SQL MUST
   be parameterized and reviewed.

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

1. Every API endpoint except registration, login, and health checks REQUIRES a valid JWT.
   Anonymous endpoints are explicitly marked with `[AllowAnonymous]`. The default is
   authenticated.

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

1. Feature modules are lazy-loaded. The initial bundle contains only the app shell,
   authentication, and the dashboard. Everything else loads on demand.

2. Modules required by the brief: DashboardModule, ProjectsModule (Project Management),
   TasksModule (Task Management), TeamModule (Team Management), ReportsModule, AuthModule —
   plus a SharedModule for reusable UI components and a CoreModule for singletons
   (interceptors, guards, services registered once).

3. HTTP calls live in dedicated service classes, never in components. Components consume
   services; services use HttpClient.

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

1. **Backend**: xUnit for unit tests targeting Services and business logic. Every service
   method with a conditional branch has at least one test per branch. Integration tests
   using `WebApplicationFactory` cover each controller's happy path and one error path.

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

2. API documentation is generated from Swagger/OpenAPI, not maintained by hand.

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

**Version**: 1.0.0 | **Ratified**: 2026-07-20 | **Last Amended**: 2026-07-20
