import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { AuthActions } from '../../../core/store/auth/auth.actions';
import { ErrorDisplayComponent } from '../../../shared/error-display/error-display.component';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    ErrorDisplayComponent,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);
  private readonly actions$ = inject(Actions);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly serverErrors = signal<string[] | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  constructor() {
    this.actions$.pipe(ofType(AuthActions.loginSuccess)).subscribe(() => {
      this.submitting.set(false);
      void this.router.navigateByUrl('/');
    });

    this.actions$.pipe(ofType(AuthActions.loginFailure)).subscribe(({ error }) => {
      this.submitting.set(false);
      // Deliberately generic — the backend already collapses wrong-password/unknown-email
      // into one message; the frontend must not add distinguishing detail (no enumeration).
      this.serverErrors.set([error]);
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.serverErrors.set(null);
    const { email, password } = this.form.getRawValue();
    this.store.dispatch(AuthActions.loginSubmitted({ email, password }));
  }
}
