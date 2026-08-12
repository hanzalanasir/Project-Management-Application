# Reports Deliverable Confirmation (006, T090)

Confirms the brief's Reports module deliverable is met **end to end**, not just covered by
individual unit/integration tests in isolation. Written at the close of 006's final stage, once all
six user stories, the cross-cutting drift/audit/export contracts, and a live quickstart pass existed
together.

## The brief's ask

> reporting + charts + export

Broken into three parts, each confirmed below: (1) four report types, (2) charts via Chart.js, (3)
export via jsPDF + papaparse through a single shared service.

## 1. Four report types

| Report | Backend slice | Frontend component | Chart |
|---|---|---|---|
| Project Progress | `Features/Reports/GetProjectProgress` | `features/reports/project-progress/` | Chart.js progress bar |
| Task Completion | `Features/Reports/GetTaskCompletion` | `features/reports/task-completion/` | Chart.js line/bar trend |
| Team Performance | `Features/Reports/GetTeamPerformance` | `features/reports/team-performance/` | Chart.js bar comparison (Admin/PM) / single-member card (TeamMember) |
| Activity | `Features/Reports/GetActivityReport` | `features/reports/activity/` | Table only (no chart specified by spec — the audit-facing report is inherently tabular) |

All four are described by `GET /api/reports/catalog`, driving the frontend's parameter forms
dynamically rather than hard-coding them (verified: adding a parameter to a `ReportDescriptor`
changes the rendered form with no component edit — quickstart V15).

## 2. Charts via Chart.js

`package.json` pins `chart.js@^4.5.1` — the only charting library referenced anywhere in
`features/reports/`. No D3 import exists (Constitution III permits D3 only when a chart needs
capabilities Chart.js cannot provide; no report in 006 invokes that escape hatch). Confirmed via:

```
grep -rl "chart.js" src/ProjectManagementApp.Web/src/app/features/reports/
  -> project-progress.component.ts, task-completion.component.ts, team-performance.component.ts
```

## 3. Export via jsPDF + papaparse, one shared service

`package.json` pins `jspdf@^4.2.1` and `papaparse@^5.5.4`. Exactly **one**
`ReportExportService` (`core/services/report-export.service.ts`) wraps both — `toPdf()` and
`toCsv()` — and every one of the four report views (including Activity) calls into that same
service rather than importing jsPDF/papaparse directly (Constitution VII.8):

```
grep -rl "ReportExportService" src/ProjectManagementApp.Web/src/app/features/reports/*/*.ts
  -> activity, project-progress, task-completion, team-performance — all four
```

Proven network-free (T075, `export-network.spec.ts`): triggering either export issues **zero** HTTP
requests — export renders the JSON a fetch already returned, it does not re-query. Proven
architecturally server-side-absent (T082, `ExportArchitectureTests`): no `?format` parameter, no
export route, and no server-side PDF/CSV package referenced by any backend project.

## Cross-cutting guarantees, confirmed together

- **No new domain entity, table, or migration** (T086, quickstart V13) — `ls
  src/ProjectManagementApp.Infrastructure/Persistence/Migrations/` shows nothing 006-authored.
- **Exactly one audit write per generation, zero for catalog/400/403/re-export** (T081, `AuditContractTests`)
  — proven live via direct `psql` against the dev database in this stage's quickstart run (V12), not
  just against a Testcontainers instance.
- **Dashboard/Reports value parity** (T083, `DashboardReportParityTests`; quickstart V6) — 005's
  `overdueTaskCount` equals the sum of 006's `rows[].overdueTasks` for the same caller, because both
  import the same `MetricDefinitions.IsOverdue` predicate from the shared kernel rather than
  re-implementing it.
- **The contract gate catches real drift** (T084) — confirmed by deliberately removing an enum value
  from a query parameter and watching `dotnet build -p:CheckApiContract=true` fail, then reverting.
  One caveat, disclosed rather than hidden: the literal proof quickstart.md V14 and this stage's own
  instructions describe — adding an unadvertised `?format` parameter — does **not** fail the gate, because
  `oasdiff breaking`'s default ruleset treats an additive optional query parameter as non-breaking by
  design (old clients are unaffected by a parameter they never send). The gate's real protection is
  against removing/narrowing existing contract elements (enum values, response media types, response
  headers), which the enum-removal proof exercises directly.

## What this file does not claim

No headless-browser click-through of the Angular UI was performed in this stage — Chart.js
rendering, the PDF/CSV download prompts, and the catalog-driven form behavior are confirmed via the
frontend automated test suite (`*.spec.ts` under `features/reports/`, 37+ passing) plus static
confirmation of imports/dependencies above, not via a live browser session. This mirrors the same
disclosed limitation noted for 005's dashboard charts.
