import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { DashboardService } from '../../../core/services/dashboard.service';
import type { components } from '../../../core/api/generated/dashboard.v1';

type ActivityEntry = components['schemas']['ActivityEntry'];

// T039: the paginated, role-scoped activity feed. Strictly read-only — no "mark read", no row
// actions, ever (research R-6). Pager over PagedResult<T>, matching the app's existing
// project/task list convention rather than infinite scroll.
@Component({
  selector: 'app-dashboard-activity-feed',
  imports: [DatePipe, MatPaginatorModule],
  templateUrl: './activity-feed.component.html',
  styleUrl: './activity-feed.component.scss',
})
export class ActivityFeedComponent {
  private readonly dashboardService = inject(DashboardService);

  protected readonly items = signal<ActivityEntry[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageIndex = signal(0);
  protected readonly pageSize = signal(20);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.reload();
  }

  protected onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.reload();
  }

  private reload(): void {
    this.loading.set(true);
    this.error.set(null);

    this.dashboardService.getActivity({ page: this.pageIndex() + 1, pageSize: this.pageSize() }).subscribe({
      next: page => {
        this.items.set(page.items);
        this.totalCount.set(page.totalCount);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(err.error?.detail ?? err.error?.title ?? 'Could not load recent activity.');
      },
    });
  }
}
