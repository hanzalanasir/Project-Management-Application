# ADR-0001: Standalone Angular components instead of NgModules

**Status**: Accepted · **Date**: 2026-07-22 · **Amends**: Constitution VII.1–VII.2 (v1.0.0 → v1.1.0)

## Context

Constitution v1.0.0 VII.2 named six `@NgModule` classes (`DashboardModule`, `ProjectsModule`,
`TasksModule`, `TeamModule`, `ReportsModule`, `AuthModule`) plus `SharedModule` and `CoreModule`.
The locked stack (Constitution III) is **Angular 22**, where standalone components are the default
and `@NgModule` is retained only for backward compatibility. Building a greenfield Angular 22 app on
NgModules would adopt a legacy idiom on day one and diverge from current Angular documentation,
tooling defaults, and hiring expectations.

## Decision

Feature areas are **standalone components with route-level lazy loading**. Each brief-mandated area
(Dashboard, Projects, Tasks, Team, Reports, Auth) is a directory with its own `*.routes.ts`, loaded
via `loadChildren: () => import('./projects/projects.routes')`. Consequently:

- Interceptors are **functional** (`HttpInterceptorFn`) registered with `provideHttpClient(withInterceptors([...]))`.
- Guards are **functional** (`CanActivateFn` / `CanMatchFn`) — still the only navigation-blocking mechanism (VII.5).
- NgRx is wired with `provideStore()` / `provideEffects()` in the application config; feature state uses `createFeature`.
- `shared/` holds reusable presentational components; `core/` holds singletons provided once in `appConfig`.

## Alternatives rejected

- **Classic NgModules** — literal VII.2 compliance, no constitution change, but a dated architecture for Angular 22.
- **Hybrid** (NgModule shells wrapping standalone components) — satisfies the old wording on paper while mixing two
  idioms in one codebase; the worst of both.

## Consequences

The word "module" in the brief and in feature specs now means a **lazy-loaded route group**, not an
`@NgModule`. The six named areas, the lazy-loading requirement, and the initial-bundle constraint all
survive unchanged — only the mechanism differs.

## Backward compatibility

None required: no frontend code exists yet, so no compliant work is invalidated. This is why the
amendment is MINOR (v1.1.0) rather than MAJOR under Constitution XII.3.
