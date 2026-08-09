import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

// Populated incrementally as each Auth feature story lands (US1 register, US2 login, ...).
export const authRoutes: Routes = [
  {
    path: 'register',
    loadComponent: () => import('./register/register.component').then(m => m.RegisterComponent),
  },
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'admin-users',
    canActivate: [authGuard],
    canMatch: [roleGuard(['Admin'])],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./admin-users/list/admin-users-list.component').then(m => m.AdminUsersListComponent),
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./admin-users/detail/admin-user-detail.component').then(m => m.AdminUserDetailComponent),
      },
    ],
  },
];
