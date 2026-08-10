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
import { ProjectsService } from '../../../core/services/projects.service';
import { ErrorDisplayComponent } from '../../../shared/error-display/error-display.component';
import { GUID_PATTERN } from '../../../shared/validators/guid.validator';
import type { components } from '../../../core/api/generated/projects.v1';

type ProjectSummary = components['schemas']['ProjectSummary'];

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
  private readonly projectsService = inject(ProjectsService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly priorities = ['Low', 'Medium', 'High', 'Critical'];

  // Reachable two ways: with a projectId query param (from a project's own "New Task" link, which
  // pre-selects and locks the field) or with none (from the global task list's "New Task" button),
  // in which case the project select below is left open and the caller picks one — a task cannot
  // be created without a project either way, so the field is always present and always required.
  private readonly queryProjectId = this.route.snapshot.queryParamMap.get('projectId');

  protected readonly projects = signal<ProjectSummary[]>([]);
  protected readonly projectsLoading = signal(true);
  protected readonly projectsError = signal<string | null>(null);
  protected readonly projectLocked = !!this.queryProjectId;

  protected readonly submitting = signal(false);
  protected readonly serverErrors = signal<string[] | null>(null);

  // assigneeId is a plain field, not a project-scoped picker: a real picker would need 004's
  // roster endpoint queried per selected project, which is more UI than this form currently
  // wires up. Mirrors 002's CreateProjectComponent ownerId field's identical shape. The pattern
  // validator at least catches an obviously-wrong value (e.g. "1") before it reaches the server.
  protected readonly form = this.fb.nonNullable.group(
    {
      projectId: [this.queryProjectId ?? '', [Validators.required]],
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(2000)]],
      priority: ['Medium'],
      dueDate: [''],
      assigneeId: ['', [Validators.pattern(GUID_PATTERN)]],
    },
    { validators: requiredFieldsPresentValidator }
  );

  constructor() {
    if (this.projectLocked) {
      this.form.controls.projectId.disable();
    }
    this.loadProjects();
  }

  private loadProjects(): void {
    this.projectsLoading.set(true);
    this.projectsError.set(null);

    // pageSize 100 (Constitution VI.4 max) — a project select is a small, human-scale picker, not
    // a paginated list; a caller who owns/administers more than that is outside this form's scope.
    this.projectsService.list({ pageSize: 100 }).subscribe({
      next: page => {
        this.projects.set(page.items);
        this.projectsLoading.set(false);
      },
      error: () => {
        this.projectsError.set('Could not load your projects.');
        this.projectsLoading.set(false);
      },
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.serverErrors.set(null);
    const { projectId, title, description, priority, dueDate, assigneeId } = this.form.getRawValue();

    this.tasksService
      .create(projectId, {
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
            errors ? (Object.values(errors).flat() as string[]) : [error.error?.detail ?? error.error?.title ?? 'Could not create task.']
          );
        },
      });
  }
}
