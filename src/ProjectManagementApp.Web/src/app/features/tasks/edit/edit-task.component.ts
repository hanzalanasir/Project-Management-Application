import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { HttpErrorResponse } from '@angular/common/http';
import { TasksService } from '../../../core/services/tasks.service';
import { ErrorDisplayComponent } from '../../../shared/error-display/error-display.component';

// Mirrors EditProjectComponent's unsaved-changes-guard / 409-conflict pattern. Not reachable for a
// TeamMember (roleGuard in tasks.routes.ts) — the API refuses them regardless via CanMutateAsync.
@Component({
  selector: 'app-edit-task',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCardModule,
    ErrorDisplayComponent,
  ],
  templateUrl: './edit-task.component.html',
  styleUrl: './edit-task.component.scss',
})
export class EditTaskComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly tasksService = inject(TasksService);

  protected readonly priorities = ['Low', 'Medium', 'High', 'Critical'];

  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly serverErrors = signal<string[] | null>(null);
  protected readonly conflict = signal(false);

  private readonly taskId = this.route.snapshot.paramMap.get('id')!;
  private etag = '';
  private saved = false;

  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    priority: ['Medium'],
    dueDate: [''],
  });

  constructor() {
    this.tasksService.getById(this.taskId).subscribe({
      next: ({ detail, etag }) => {
        this.etag = etag;
        this.form.patchValue({
          title: detail.title,
          description: detail.description ?? '',
          priority: detail.priority,
          dueDate: detail.dueDate ?? '',
        });
        this.form.markAsPristine();
        this.loading.set(false);
      },
      error: () => {
        this.serverErrors.set(['Could not load this task.']);
        this.loading.set(false);
      },
    });
  }

  // Read by the `canDeactivate` route guard (tasks.routes.ts).
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
    const { title, description, priority, dueDate } = this.form.getRawValue();

    this.tasksService
      .update(
        this.taskId,
        {
          title,
          description: description || null,
          priority: priority as 'Low' | 'Medium' | 'High' | 'Critical',
          dueDate: dueDate || null,
        },
        this.etag
      )
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.saved = true;
          void this.router.navigate(['/tasks', this.taskId]);
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          if (error.status === 409) {
            // 409 surfaces as a reload-and-reapply prompt — never a silent retry.
            this.conflict.set(true);
            return;
          }
          const errors = error.error?.errors;
          this.serverErrors.set(
            errors ? (Object.values(errors).flat() as string[]) : [error.error?.title ?? 'Could not update task.']
          );
        },
      });
  }

  protected reloadAfterConflict(): void {
    this.conflict.set(false);
    this.loading.set(true);
    this.form.markAsPristine();
    this.tasksService.getById(this.taskId).subscribe(({ detail, etag }) => {
      this.etag = etag;
      this.form.patchValue({
        title: detail.title,
        description: detail.description ?? '',
        priority: detail.priority,
        dueDate: detail.dueDate ?? '',
      });
      this.form.markAsPristine();
      this.loading.set(false);
    });
  }
}
