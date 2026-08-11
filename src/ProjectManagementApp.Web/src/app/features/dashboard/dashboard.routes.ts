import { Routes } from '@angular/router';

// One routed page: the summary component composes the role-scoped tiles, the "My Work" panel
// (US3), and the activity-feed widget (US2) into a single dashboard view — there is no second
// route for the feed (plan.md §Project Structure lists one route group, two component folders).
export const dashboardRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./summary/summary.component').then(m => m.SummaryComponent),
  },
];
