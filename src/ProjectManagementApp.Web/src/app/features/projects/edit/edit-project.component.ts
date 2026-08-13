import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { HttpErrorResponse } from '@angular/common/http';
import { ProjectsService } from '../../../core/services/projects.service';
import { ErrorDisplayComponent } from '../../../shared/error-display/error-display.component';
import { authFeature } from '../../../core/store/auth/auth.feature';
import { fromDateOnlyString, toDateOnlyString } from '../../../core/utils/date-only.util';

// Mirrors CreateProjectComponent's cross-field validator (Constitution VII.6, ADR-0005).
function dateOrderValidator(group: AbstractControl): ValidationErrors | null {
  const startDate = group.get('startDate')?.value;
  const endDate = group.get('endDate')?.value;
  if (!startDate || !endDate) {
    return null;
  }
  return endDate >= startDate ? null : { dateOrder: true };
}

@Component({
  selector: 'app-edit-project',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCardModule,
    MatDatepickerModule,
    ErrorDisplayComponent,
  ],
  templateUrl: './edit-project.component.html',
  styleUrl: './edit-project.component.scss',
})
export class EditProjectComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly projectsService = inject(ProjectsService);
  private readonly store = inject(Store);

  protected readonly statuses = ['Planning', 'Active', 'OnHold', 'Completed', 'Cancelled'];
  protected readonly isAdmin = this.store.selectSignal(authFeature.selectUser)()?.role === 'Admin';

  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly serverErrors = signal<string[] | null>(null);
  protected readonly conflict = signal(false);

  private readonly projectId = this.route.snapshot.paramMap.get('id')!;
  private etag = '';
  private saved = false;

  protected readonly form = this.fb.nonNullable.group(
    {
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(2000)]],
      startDate: [null as Date | null, [Validators.required]],
      endDate: [null as Date | null],
      status: ['Planning'],
      ownerId: [''],
    },
    { validators: dateOrderValidator }
  );

  constructor() {
    this.projectsService.getById(this.projectId).subscribe({
      next: ({ detail, etag }) => {
        this.etag = etag;
        this.form.patchValue({
          name: detail.name,
          description: detail.description ?? '',
          startDate: fromDateOnlyString(detail.startDate),
          endDate: fromDateOnlyString(detail.endDate),
          status: detail.status,
          ownerId: detail.owner.id,
        });
        this.form.markAsPristine();
        this.loading.set(false);
      },
      error: () => {
        this.serverErrors.set(['Could not load this project.']);
        this.loading.set(false);
      },
    });
  }

  // Read by the `canDeactivate` route guard (projects.routes.ts) — an unsaved edit prompts before
  // navigating away, rather than silently discarding it.
  canDeactivate(): boolean {
    if (this.saved || this.form.pristine) {
      return true;
    }
    return confirm('You have unsaved changes. Leave without saving?');
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.serverErrors.set(null);
    this.conflict.set(false);
    const { name, description, startDate, endDate, status, ownerId } = this.form.getRawValue();

    this.projectsService
      .update(
        this.projectId,
        {
          name,
          description: description || null,
          startDate: toDateOnlyString(startDate),
          endDate: endDate ? toDateOnlyString(endDate) : null,
          status: status as 'Planning' | 'Active' | 'OnHold' | 'Completed' | 'Cancelled',
          ownerId: this.isAdmin && ownerId ? ownerId : null,
        },
        this.etag
      )
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.saved = true;
          void this.router.navigate(['/projects', this.projectId]);
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          if (error.status === 409) {
            // 409 surfaces as a reload-and-reapply prompt (VII.7) — never a silent retry.
            this.conflict.set(true);
            return;
          }
          const errors = error.error?.errors;
          this.serverErrors.set(
            errors ? (Object.values(errors).flat() as string[]) : [error.error?.detail ?? error.error?.title ?? 'Could not update project.']
          );
        },
      });
  }

  protected reloadAfterConflict(): void {
    this.conflict.set(false);
    this.loading.set(true);
    this.form.markAsPristine();
    this.projectsService.getById(this.projectId).subscribe(({ detail, etag }) => {
      this.etag = etag;
      this.form.patchValue({
        name: detail.name,
        description: detail.description ?? '',
        startDate: fromDateOnlyString(detail.startDate),
        endDate: fromDateOnlyString(detail.endDate),
        status: detail.status,
        ownerId: detail.owner.id,
      });
      this.form.markAsPristine();
      this.loading.set(false);
    });
  }
}
