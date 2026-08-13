import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { ErrorDisplayComponent } from '../../../shared/error-display/error-display.component';

function passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { passwordMismatch: true };
}

// Mirrors the server's actual policy (ASP.NET Core Identity defaults, Infrastructure/
// DependencyInjection.cs: RequireUppercase/RequireLowercase/RequireDigit/RequireNonAlphanumeric
// all true) — the backend is still authoritative (Constitution V.5), this just stops a password
// that will only fail server-side from ever reaching the network.
const PASSWORD_COMPLEXITY_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).*$/;

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    ErrorDisplayComponent,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly serverErrors = signal<string[] | null>(null);

  protected readonly form = this.fb.nonNullable.group(
    {
      fullName: ['', [Validators.required, Validators.maxLength(200)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(PASSWORD_COMPLEXITY_PATTERN)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatchValidator }
  );

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.serverErrors.set(null);
    const { fullName, email, password, confirmPassword } = this.form.getRawValue();

    this.authService.register({ fullName, email, password, confirmPassword }).subscribe({
      next: () => {
        this.submitting.set(false);
        void this.router.navigateByUrl('/auth/login');
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        const errors = error.error?.errors;
        this.serverErrors.set(
          errors ? Object.values(errors).flat() as string[] : [error.error?.detail ?? error.error?.title ?? 'Registration failed.']
        );
      },
    });
  }
}
