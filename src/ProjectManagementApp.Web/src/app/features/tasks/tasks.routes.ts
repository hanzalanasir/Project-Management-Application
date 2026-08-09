import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';
import type { EditTaskComponent } from './edit/edit-task.component';

// Populated incrementally as each Tasks feature story lands (US1 create, US2 list, US3 detail,
// US4 edit, US5 status, US6 delete, US7 reassign — specs/003-tasks/tasks.md T024-T097). Create and
// Edit are Admin+ProjectManager only (UX convenience — the API's CanMutateAsync is the real gate);
// List/Detail are open to all three roles, and a TeamMember's only enabled write there is status.
export const tasksRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./list/task-list.component').then(m => m.TaskListComponent),
  },
  {
    path: 'new',
    canMatch: [roleGuard(['Admin', 'ProjectManager'])],
    loadComponent: () => import('./create/create-task.component').then(m => m.CreateTaskComponent),
  },
  {
    path: ':id',
    loadComponent: () => import('./detail/task-detail.component').then(m => m.TaskDetailComponent),
  },
  {
    path: ':id/edit',
    canMatch: [roleGuard(['Admin', 'ProjectManager'])],
    canDeactivate: [(component: EditTaskComponent) => component.canDeactivate()],
    loadComponent: () => import('./edit/edit-task.component').then(m => m.EditTaskComponent),
  },
];
