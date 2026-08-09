import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { HttpErrorResponse } from '@angular/common/http';
import { ProjectsService } from '../../../core/services/projects.service';
import { ErrorDisplayComponent } from '../../../shared/error-display/error-display.component';
import { authFeature } from '../../../core/store/auth/auth.feature';

// Cross-field date-order validator (Constitution VII.6, ADR-0005) — mirrors the server-side rule
// in CreateProjectCommandValidator; the server is authoritative, this is UX only.
function dateOrderValidator(group: AbstractControl): ValidationErrors | null {
  const startDate = group.get('startDate')?.value;
  const endDate = group.get('endDate')?.value;
  if (!startDate || !endDate) {
    return null;
  }
  return endDate >= startDate ? null : { dateOrder: true };
}

@Component({
  selector: 'app-create-project',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCardModule,
    ErrorDisplayComponent,
  ],
  templateUrl: './create-project.component.html',
  styleUrl: './create-project.component.scss',
})
export class CreateProjectComponent {
  private readonly fb = inject(FormBuilder);
  private readonly projectsService = inject(ProjectsService);
  private readonly router = inject(Router);
  private readonly store = inject(Store);

  protected readonly statuses = ['Planning', 'Active', 'OnHold', 'Completed', 'Cancelled'];
  protected readonly isAdmin = this.store.selectSignal(authFeature.selectUser)()?.role === 'Admin';

  protected readonly submitting = signal(false);
  protected readonly serverErrors = signal<string[] | null>(null);

  protected readonly form = this.fb.nonNullable.group(
    {
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(2000)]],
      startDate: ['', [Validators.required]],
      endDate: [''],
      status: ['Planning'],
      ownerId: [''],
    },
    { validators: dateOrderValidator }
  );

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.serverErrors.set(null);
    const { name, description, startDate, endDate, status, ownerId } = this.form.getRawValue();

    this.projectsService
      .create({
        name,
        description: description || null,
        startDate,
        endDate: endDate || null,
        status: status as 'Planning' | 'Active' | 'OnHold' | 'Completed' | 'Cancelled',
        ownerId: this.isAdmin && ownerId ? ownerId : null,
      })
      .subscribe({
        next: ({ detail }) => {
          this.submitting.set(false);
          void this.router.navigate(['/projects', detail.id]);
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          const errors = error.error?.errors;
          this.serverErrors.set(
            errors ? (Object.values(errors).flat() as string[]) : [error.error?.title ?? 'Could not create project.']
          );
        },
      });
  }
}
