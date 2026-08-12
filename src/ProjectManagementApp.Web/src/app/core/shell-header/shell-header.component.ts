import { Component, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { authFeature } from '../store/auth/auth.feature';
import { AuthActions } from '../store/auth/auth.actions';

// A core/ singleton, provided once at the app shell (Constitution VII) — the one place the
// logout control lives, dispatching the NgRx logout action rather than calling AuthService
// directly from a component. Admin Users is shown only for Admin (matches auth.routes' own
// canMatch role guard on /auth/admin-users — the nav link mirrors, not duplicates, that gate).
@Component({
  selector: 'app-shell-header',
  imports: [RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    @if (user(); as u) {
      <mat-toolbar color="primary" class="shell-toolbar">
        <a class="brand" routerLink="/dashboard">
          <mat-icon>dashboard</mat-icon>
          <span>ProjectManagementApp</span>
        </a>

        <nav class="shell-nav">
          <a mat-button routerLink="/dashboard" routerLinkActive="active-link">Dashboard</a>
          <a mat-button routerLink="/projects" routerLinkActive="active-link">Projects</a>
          <a mat-button routerLink="/tasks" routerLinkActive="active-link">Tasks</a>
          <a mat-button routerLink="/reports" routerLinkActive="active-link">Reports</a>
          @if (u.role === 'Admin') {
            <a mat-button routerLink="/auth/admin-users" routerLinkActive="active-link">Users</a>
          }
        </nav>

        <span class="spacer"></span>

        <span class="chip" [attr.data-role]="u.role">{{ u.role }}</span>
        <span class="user-name">{{ u.fullName }}</span>
        <button mat-stroked-button (click)="logout()">Log out</button>
      </mat-toolbar>
    }
  `,
  styles: [`
    .shell-toolbar {
      position: sticky;
      top: 0;
      z-index: 10;
      display: flex;
      flex-wrap: wrap;
      gap: 8px 16px;
      row-gap: 8px;
      height: auto;
      min-height: 64px;
      padding: 8px 16px;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 8px;
      color: inherit;
      text-decoration: none;
      font-weight: 700;
      white-space: nowrap;
    }

    .shell-nav {
      display: flex;
      gap: 4px;
      flex-wrap: wrap;
    }

    .active-link {
      background: rgba(255, 255, 255, 0.16);
      border-radius: 4px;
    }

    .spacer { flex: 1 1 auto; }

    .user-name {
      white-space: nowrap;
    }

    .chip[data-role] {
      /* on the primary-colored toolbar, keep the role chip legible against a dark ground */
      background: rgba(255, 255, 255, 0.85);
    }
  `]
})
export class ShellHeaderComponent {
  private readonly store = inject(Store);
  protected readonly user = this.store.selectSignal(authFeature.selectUser);

  protected logout(): void {
    this.store.dispatch(AuthActions.logoutRequested());
  }
}
