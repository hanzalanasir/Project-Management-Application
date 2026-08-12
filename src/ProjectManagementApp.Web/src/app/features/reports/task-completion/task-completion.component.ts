import { Component, ElementRef, effect, inject, signal, viewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Chart, registerables } from 'chart.js';
import { ReportsService } from '../../../core/services/reports.service';
import { ReportExportService, ExportableReport } from '../../../core/services/report-export.service';
import type { components } from '../../../core/api/generated/reports.v1';

type TaskCompletionReport = components['schemas']['TaskCompletionReport'];

Chart.register(...registerables);

// T052/T079 — the historical trend Dashboard deliberately does not show. Renders the zero-filled,
// continuous bucket series exactly as the API returns it (no client-side re-bucketing). Export
// renders the SAME `report()` signal already on screen — never a fresh request (T075).
@Component({
  selector: 'app-task-completion',
  templateUrl: './task-completion.component.html',
  styleUrl: './task-completion.component.scss',
})
export class TaskCompletionComponent {
  private readonly reportsService = inject(ReportsService);
  private readonly route = inject(ActivatedRoute);
  private readonly exportService = inject(ReportExportService);

  protected readonly report = signal<TaskCompletionReport | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly exporting = signal(false);

  private readonly chartCanvas = viewChild<ElementRef<HTMLCanvasElement>>('trendChart');
  private chart?: Chart;

  constructor() {
    const params = this.route.snapshot.queryParamMap;
    const from = params.get('from') ?? '';
    const to = params.get('to') ?? '';
    const groupBy = (params.get('groupBy') ?? 'week') as 'day' | 'week' | 'month';
    const projectScope = params.get('projectScope') ?? undefined;
    const assigneeId = params.get('assigneeId') ?? undefined;

    this.reportsService.getTaskCompletion({ from, to, groupBy, projectScope, assigneeId }).subscribe({
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
      if (data && el) {
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
      columns: ['Period', 'Completed'],
      rows: data.buckets.map(b => [b.periodLabel, b.completedCount]),
    };
  }

  private renderChart(data: TaskCompletionReport, el: HTMLCanvasElement): void {
    if (!el.getContext('2d')) {
      return;
    }

    this.chart?.destroy();
    this.chart = new Chart(el, {
      type: 'line',
      data: {
        labels: data.buckets.map(b => b.periodLabel),
        datasets: [{ label: 'Completed', data: data.buckets.map(b => b.completedCount), borderColor: '#1e88e5', fill: false }],
      },
      options: { responsive: true, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } },
    });
  }
}
