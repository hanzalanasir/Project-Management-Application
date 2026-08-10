import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { TasksService } from '../../../core/services/tasks.service';
import { authFeature } from '../../../core/store/auth/auth.feature';
import type { components } from '../../../core/api/generated/tasks.v1';

type TaskSummary = components['schemas']['TaskSummary'];
type TaskStatus = components['schemas']['TaskStatus'];
type TaskPriority = components['schemas']['TaskPriority'];

// Renders exactly what the API returns — no client-side role filtering (same discipline as
// ProjectListComponent); the scope gate lives server-side in ListTasksQueryHandler, not here.
@Component({
  selector: 'app-task-list',
  imports: [RouterLink, FormsModule, MatTableModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatPaginatorModule],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss',
})
export class TaskListComponent {
  private readonly tasksService = inject(TasksService);
  private readonly store = inject(Store);

  protected readonly statuses: TaskStatus[] = ['ToDo', 'InProgress', 'InReview', 'Done', 'Blocked'];
  protected readonly priorities: TaskPriority[] = ['Low', 'Medium', 'High', 'Critical'];
  private readonly role = this.store.selectSignal(authFeature.selectUser)()?.role;
  protected readonly isTeamMember = this.role === 'TeamMember';
  protected readonly canManage = this.role === 'Admin' || this.role === 'ProjectManager';
  protected readonly displayedColumns = this.canManage
    ? ['title', 'status', 'priority', 'dueDate', 'assignee', 'actions']
    : ['title', 'status', 'priority', 'dueDate', 'assignee'];

  protected readonly items = signal<TaskSummary[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageIndex = signal(0);
  protected readonly pageSize = signal(20);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected search = '';
  protected status: TaskStatus | '' = '';

  constructor() {
    this.reload();
  }

  protected onSearchChange(): void {
    this.pageIndex.set(0);
    this.reload();
  }

  protected onStatusChange(): void {
    this.pageIndex.set(0);
    this.reload();
  }

  protected onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.reload();
  }

  protected deleteTask(task: TaskSummary): void {
    if (!confirm(`Delete "${task.title}"?`)) {
      return;
    }

    this.tasksService.delete(task.id).subscribe({
      next: () => this.reload(),
      error: (err: HttpErrorResponse) => this.error.set(err.error?.detail ?? err.error?.title ?? 'Could not delete task.'),
    });
  }

  private reload(): void {
    this.loading.set(true);
    this.error.set(null);

    this.tasksService
      .list({
        page: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        search: this.search || undefined,
        status: this.status || undefined,
      })
      .subscribe({
        next: page => {
          this.items.set(page.items);
          this.totalCount.set(page.totalCount);
          this.pageSize.set(page.pageSize);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load tasks.');
          this.loading.set(false);
        },
      });
  }
}
