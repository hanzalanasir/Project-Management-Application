import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { TasksService } from '../../../core/services/tasks.service';
import { authFeature } from '../../../core/store/auth/auth.feature';
import type { components } from '../../../core/api/generated/tasks.v1';

type TaskDetail = components['schemas']['TaskDetail'];
type TaskStatus = components['schemas']['TaskStatus'];

// The screen every write action launches from. Edit is rendered only for Admin/ProjectManager
// (the graduated model's FullEdit cell, data-model.md §3); the status control is always rendered
// and is the ONLY write control a TeamMember can actually use — the API's CanMutateAsync is what
// enforces this, this component just avoids offering controls that would only ever 403.
@Component({
  selector: 'app-task-detail',
  imports: [RouterLink, MatCardModule, MatButtonModule, MatFormFieldModule, MatSelectModule, MatInputModule, FormsModule],
  templateUrl: './task-detail.component.html',
  styleUrl: './task-detail.component.scss',
})
export class TaskDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly tasksService = inject(TasksService);
  private readonly store = inject(Store);

  protected readonly statuses: TaskStatus[] = ['ToDo', 'InProgress', 'InReview', 'Done', 'Blocked'];

  protected readonly task = signal<TaskDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly forbidden = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly statusError = signal<string | null>(null);
  protected readonly reassignError = signal<string | null>(null);

  // No 004 roster endpoint exists yet — plain id input, same forward-dependency shape as
  // CreateTaskComponent's assigneeId field (T035). The backend AssigneeValidator enforces the real
  // rule against team_members regardless of what this input offers.
  protected reassignTargetId = '';

  private readonly currentUser = this.store.selectSignal(authFeature.selectUser);
  private readonly taskId = this.route.snapshot.paramMap.get('id')!;
  private etag = '';

  constructor() {
    this.reload();
  }

  protected canManage(): boolean {
    const role = this.currentUser()?.role;
    return role === 'Admin' || role === 'ProjectManager';
  }

  protected onStatusChange(status: TaskStatus): void {
    this.statusError.set(null);
    this.tasksService.updateStatus(this.taskId, { status }, this.etag).subscribe({
      next: ({ detail, etag }) => {
        this.task.set(detail);
        this.etag = etag;
      },
      error: (err: HttpErrorResponse) => {
        this.statusError.set(err.error?.detail ?? err.error?.title ?? 'Could not update status.');
        // Reload to resync the select with the actual server state after a refused change.
        this.reload();
      },
    });
  }

  protected reassign(): void {
    const t = this.task();
    if (!t) return;
    const assigneeId = this.reassignTargetId.trim() || null;
    if (!confirm(assigneeId ? 'Reassign this task?' : 'Unassign this task?')) {
      return;
    }

    this.reassignError.set(null);
    this.tasksService.reassign(this.taskId, { assigneeId }, this.etag).subscribe({
      next: ({ detail, etag }) => {
        this.task.set(detail);
        this.etag = etag;
        this.reassignTargetId = '';
      },
      error: (err: HttpErrorResponse) => {
        this.reassignError.set(err.error?.detail ?? err.error?.title ?? 'Could not reassign this task.');
      },
    });
  }

  protected deleteTask(): void {
    const t = this.task();
    if (!t) return;
    if (!confirm(`Delete "${t.title}"?`)) {
      return;
    }

    this.tasksService.delete(this.taskId).subscribe({
      next: () => void this.router.navigate(['/tasks']),
      error: (err: HttpErrorResponse) => this.error.set(err.error?.detail ?? err.error?.title ?? 'Could not delete task.'),
    });
  }

  private reload(): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.forbidden.set(false);
    this.error.set(null);

    this.tasksService.getById(this.taskId).subscribe({
      next: ({ detail, etag }) => {
        this.task.set(detail);
        this.etag = etag;
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 404) {
          this.notFound.set(true);
        } else if (err.status === 403) {
          this.forbidden.set(true);
        } else {
          this.error.set('Could not load this task.');
        }
      },
    });
  }
}
