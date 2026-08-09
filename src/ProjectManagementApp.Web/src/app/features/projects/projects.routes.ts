import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';
import type { EditProjectComponent } from './edit/edit-project.component';

// Populated incrementally as each Projects feature story lands (US1 create, US2 list, US3 detail,
// US4 edit, US5 delete — see specs/002-projects/tasks.md T024, T034, T046, T055, T066, T075).
// Create/Edit are Admin+ProjectManager only (UX convenience — the API is the real gate, spec
// Access Logic step 2); List/Detail are open to all three roles via the parent authGuard alone.
export const projectsRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./list/project-list.component').then(m => m.ProjectListComponent),
  },
  {
    path: 'new',
    canMatch: [roleGuard(['Admin', 'ProjectManager'])],
    loadComponent: () =>
      import('./create/create-project.component').then(m => m.CreateProjectComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./detail/project-detail.component').then(m => m.ProjectDetailComponent),
  },
  {
    path: ':id/edit',
    canMatch: [roleGuard(['Admin', 'ProjectManager'])],
    canDeactivate: [(component: EditProjectComponent) => component.canDeactivate()],
    loadComponent: () =>
      import('./edit/edit-project.component').then(m => m.EditProjectComponent),
  },
];
