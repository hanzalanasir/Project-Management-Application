import { Component, ElementRef, effect, inject, signal, viewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { Chart, registerables } from 'chart.js';
import { ReportsService } from '../../../core/services/reports.service';
import { ReportExportService, ExportableReport } from '../../../core/services/report-export.service';
import { authFeature } from '../../../core/store/auth/auth.feature';
import type { components } from '../../../core/api/generated/reports.v1';

type TeamPerformanceReport = components['schemas']['TeamPerformanceReport'];

Chart.register(...registerables);

// T062/T079 — the sharpest least-privilege boundary in the product, made visible in the UI too: a
// TeamMember's response body is always a single row (server-enforced, T060), so this view renders
// that shape as a personal card rather than a comparison chart — there is nothing to compare.
// Admin/PM get the bar comparison across every in-scope member. Export renders the SAME `report()`
// signal already on screen — never a fresh request (T075).
@Component({
  selector: 'app-team-performance',
  templateUrl: './team-performance.component.html',
  styleUrl: './team-performance.component.scss',
})
export class TeamPerformanceComponent {
  private readonly reportsService = inject(ReportsService);
  private readonly route = inject(ActivatedRoute);
  private readonly store = inject(Store);
  private readonly exportService = inject(ReportExportService);

  protected readonly report = signal<TeamPerformanceReport | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly exporting = signal(false);
  protected readonly isTeamMember = this.store.selectSignal(authFeature.selectUser)()?.role === 'TeamMember';

  private readonly chartCanvas = viewChild<ElementRef<HTMLCanvasElement>>('comparisonChart');
  private chart?: Chart;

  constructor() {
    const params = this.route.snapshot.queryParamMap;
    const from = params.get('from') ?? '';
    const to = params.get('to') ?? '';
    const projectScope = params.get('projectScope') ?? undefined;
    const userId = params.get('userId') ?? undefined;

    this.reportsService.getTeamPerformance({ from, to, projectScope, userId }).subscribe({
      next: report => {
        this.report.set(report);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(err.error?.detail ?? err.error?.title ?? 'Could not load the report.');
      },
    });

    effect(() => {
      const data = this.report();
      const el = this.chartCanvas()?.nativeElement;
      if (data && el && !this.isTeamMember) {
        this.renderChart(data, el);
      }
    });
  }

  protected exportPdf(): void {
    this.runExport(() => this.exportService.toPdf(this.toExportable()));
  }

  protected exportCsv(): void {
    this.runExport(() => this.exportService.toCsv(this.toExportable()));
  }

  private runExport(action: () => void): void {
    if (this.exporting() || !this.report()) {
      return;
    }
    this.exporting.set(true);
    try {
      action();
    } finally {
      this.exporting.set(false);
    }
  }

  private toExportable(): ExportableReport {
    const data = this.report()!;
    return {
      reportType: data.reportType,
      windowFrom: data.window.from,
      windowTo: data.window.to,
      columns: ['Member', 'Active', 'Throughput', 'Workload', 'Overdue'],
      rows: data.rows.map(r => [r.fullName, r.isActive ? 'Yes' : 'No', r.throughput, r.workload, r.overdueCount]),
    };
  }

  private renderChart(data: TeamPerformanceReport, el: HTMLCanvasElement): void {
    if (!el.getContext('2d')) {
      return;
    }

    this.chart?.destroy();
    this.chart = new Chart(el, {
      type: 'bar',
      data: {
        labels: data.rows.map(r => r.fullName),
        datasets: [
          { label: 'Throughput', data: data.rows.map(r => r.throughput), backgroundColor: '#1e88e5' },
          { label: 'Workload', data: data.rows.map(r => r.workload), backgroundColor: '#f9a825' },
          { label: 'Overdue', data: data.rows.map(r => r.overdueCount), backgroundColor: '#c62828' },
        ],
      },
      options: { responsive: true, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } },
    });
  }
}
