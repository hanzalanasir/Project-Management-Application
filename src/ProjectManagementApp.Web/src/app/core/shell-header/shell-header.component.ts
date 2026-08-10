import { Component, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { authFeature } from '../store/auth/auth.feature';
import { AuthActions } from '../store/auth/auth.actions';

// A core/ singleton, provided once at the app shell (Constitution VII) — the one place the
// logout control lives, dispatching the NgRx logout action rather than calling AuthService
// directly from a component.
@Component({
  selector: 'app-shell-header',
  imports: [RouterLink, MatToolbarModule, MatButtonModule],
  template: `
    @if (user()) {
      <mat-toolbar color="primary">
        <span>ProjectManagementApp</span>
        <a mat-button routerLink="/projects">Projects</a>
        <a mat-button routerLink="/tasks">Tasks</a>
        <span class="spacer"></span>
        <span>{{ user()?.fullName }}</span>
        <button mat-button (click)="logout()">Log out</button>
      </mat-toolbar>
    }
  `,
  styles: [`
    .spacer { flex: 1 1 auto; }
  `]
})
export class ShellHeaderComponent {
  private readonly store = inject(Store);
  protected readonly user = this.store.selectSignal(authFeature.selectUser);

  protected logout(): void {
    this.store.dispatch(AuthActions.logoutRequested());
  }
}
