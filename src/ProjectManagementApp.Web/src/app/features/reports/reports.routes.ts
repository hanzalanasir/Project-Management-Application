import { Routes } from '@angular/router';

// Lazy standalone route group (Constitution VII.1-VII.2). No role is fully excluded from Reports
// (spec 006 Security Rules) — every authenticated caller may reach the picker; TeamMember's
// per-report least-privilege (self-only Team Performance, named out-of-scope 403s) is enforced
// server-side in the handlers, not by a route guard here. The authGuard at the app.routes.ts
// level (matching 005 Dashboard's own registration) is therefore the only navigation block.
export const reportsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./picker/report-picker.component').then(m => m.ReportPickerComponent),
  },
  {
    path: 'project-progress',
    loadComponent: () => import('./project-progress/project-progress.component').then(m => m.ProjectProgressComponent),
  },
  {
    path: 'task-completion',
    loadComponent: () => import('./task-completion/task-completion.component').then(m => m.TaskCompletionComponent),
  },
  {
    path: 'team-performance',
    loadComponent: () => import('./team-performance/team-performance.component').then(m => m.TeamPerformanceComponent),
  },
  {
    path: 'activity',
    loadComponent: () => import('./activity/activity-report.component').then(m => m.ActivityReportComponent),
  },
];
