import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { HttpErrorResponse } from '@angular/common/http';
import { TasksService } from '../../../core/services/tasks.service';
import { ErrorDisplayComponent } from '../../../shared/error-display/error-display.component';

// Cross-field due-date-window validator (Constitution VII.6, ADR-0005, mirrors 002's
// dateOrderValidator) — UX-only; the server (DueDateWindowValidator) is authoritative and this
// component has no knowledge of the parent project's actual start/end dates to check against
// client-side, so this only guards the trivially-checkable shape.
function requiredFieldsPresentValidator(group: AbstractControl): ValidationErrors | null {
  const title = group.get('title')?.value;
  return title && title.trim().length > 0 ? null : { titleRequired: true };
}

@Component({
  selector: 'app-create-task',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCardModule,
    ErrorDisplayComponent,
  ],
  templateUrl: './create-task.component.html',
  styleUrl: './create-task.component.scss',
})
export class CreateTaskComponent {
  private readonly fb = inject(FormBuilder);
  private readonly tasksService = inject(TasksService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly priorities = ['Low', 'Medium', 'High', 'Critical'];

  // Reached from a project's detail page ("New Task" button) via a projectId query param — the
  // route itself is /tasks/new (T006), not project-nested, since a task's creating UI needs no
  // other :id segment.
  protected readonly projectId = this.route.snapshot.queryParamMap.get('projectId') ?? '';

  protected readonly submitting = signal(false);
  protected readonly serverErrors = signal<string[] | null>(null);

  // assigneeId is a plain field, not a project-scoped picker: 004 (Team Management) — which owns
  // the roster endpoint this picker is meant to query — is not implemented yet. Mirrors 002's
  // CreateProjectComponent ownerId field, which has the identical forward-dependency shape.
  protected readonly form = this.fb.nonNullable.group(
    {
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(2000)]],
      priority: ['Medium'],
      dueDate: [''],
      assigneeId: [''],
    },
    { validators: requiredFieldsPresentValidator }
  );

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.serverErrors.set(null);
    const { title, description, priority, dueDate, assigneeId } = this.form.getRawValue();

    this.tasksService
      .create(this.projectId, {
        title,
        description: description || null,
        priority: priority as 'Low' | 'Medium' | 'High' | 'Critical',
        dueDate: dueDate || null,
        assigneeId: assigneeId || null,
      })
      .subscribe({
        next: ({ detail }) => {
          this.submitting.set(false);
          void this.router.navigate(['/tasks', detail.id]);
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          const errors = error.error?.errors;
          this.serverErrors.set(
            errors ? (Object.values(errors).flat() as string[]) : [error.error?.title ?? 'Could not create task.']
          );
        },
      });
  }
}
