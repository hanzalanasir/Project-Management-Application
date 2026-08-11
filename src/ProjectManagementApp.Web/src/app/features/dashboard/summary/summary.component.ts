import { Component, ElementRef, effect, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { PercentPipe } from '@angular/common';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter } from 'rxjs';
import { Chart, registerables } from 'chart.js';
import { DashboardService } from '../../../core/services/dashboard.service';
import { ActivityFeedComponent } from '../activity-feed/activity-feed.component';
import type { components } from '../../../core/api/generated/dashboard.v1';

type DashboardSummary = components['schemas']['DashboardSummary'];

Chart.register(...registerables);

// T028 (tiles + charts) and T047 ("My Work" panel) together — the personal slice rides inside
// the same summary payload as the tiles (US3 has no endpoint of its own), so both live in one
// component rather than a separate route. Read-only throughout: no action controls anywhere.
@Component({
  selector: 'app-dashboard-summary',
  imports: [RouterLink, PercentPipe, ActivityFeedComponent],
  templateUrl: './summary.component.html',
  styleUrl: './summary.component.scss',
})
export class SummaryComponent {
  private readonly dashboardService = inject(DashboardService);
  private readonly router = inject(Router);

  protected readonly summary = signal<DashboardSummary | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  private readonly projectsCanvas = viewChild<ElementRef<HTMLCanvasElement>>('projectsChart');
  private readonly tasksCanvas = viewChild<ElementRef<HTMLCanvasElement>>('tasksChart');
  private projectsChart?: Chart;
  private tasksChart?: Chart;

  constructor() {
    this.refresh();

    // Refetch on navigation back to this route — Angular reuses the component instance when
    // re-navigating to the identical URL, so the constructor alone only covers the first visit.
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        filter(event => event.urlAfterRedirects.startsWith('/dashboard')),
        takeUntilDestroyed()
      )
      .subscribe(() => this.refresh());

    effect(() => {
      const data = this.summary();
      const projectsEl = this.projectsCanvas()?.nativeElement;
      const tasksEl = this.tasksCanvas()?.nativeElement;
      if (data && projectsEl && tasksEl) {
        this.renderCharts(data, projectsEl, tasksEl);
      }
    });
  }

  protected refresh(): void {
    this.loading.set(true);
    this.error.set(null);

    this.dashboardService.getSummary().subscribe({
      next: summary => {
        this.summary.set(summary);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(err.error?.detail ?? err.error?.title ?? 'Could not load the dashboard.');
      },
    });
  }

  protected statusEntries(counts: Record<string, number>): { label: string; count: number }[] {
    return Object.entries(counts).map(([label, count]) => ({ label, count }));
  }

  private renderCharts(data: DashboardSummary, projectsEl: HTMLCanvasElement, tasksEl: HTMLCanvasElement): void {
    // jsdom (unit tests) has no canvas backend — getContext returns null there. Chart.js throws
    // on a null context, so skip rendering rather than let a test environment quirk fail specs
    // that have nothing to do with charting.
    if (!projectsEl.getContext('2d') || !tasksEl.getContext('2d')) {
      return;
    }

    this.projectsChart?.destroy();
    this.tasksChart?.destroy();

    const palette = ['#1e88e5', '#0277bd', '#f9a825', '#2e7d32', '#c62828'];

    this.projectsChart = new Chart(projectsEl, {
      type: 'doughnut',
      data: {
        labels: Object.keys(data.projectsByStatus),
        datasets: [{ data: Object.values(data.projectsByStatus), backgroundColor: palette }],
      },
      options: { responsive: true, plugins: { legend: { position: 'bottom' } } },
    });

    this.tasksChart = new Chart(tasksEl, {
      type: 'doughnut',
      data: {
        labels: Object.keys(data.tasksByStatus),
        datasets: [{ data: Object.values(data.tasksByStatus), backgroundColor: palette }],
      },
      options: { responsive: true, plugins: { legend: { position: 'bottom' } } },
    });
  }
}
