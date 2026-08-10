import { Routes } from '@angular/router';

// Populated incrementally as each Team feature story lands (US1 add, US2 list, US3 remove — see
// specs/004-team/tasks.md). The roster is the only routed page — "add member" is a MatDialog
// launched from the roster (AddMemberDialogComponent), not a separate route, so there is no
// second route here for a canMatch role guard to gate. Add/remove UI visibility (Admin/owner
// only) is decided inside RosterComponent itself once US1/US3 land; the real gate is always
// CanManageTeamAsync server-side.
export const teamRoutes: Routes = [
  {
    path: ':projectId',
    loadComponent: () => import('./roster/roster.component').then(m => m.RosterComponent),
  },
];
