import { Component, ElementRef, effect, inject, signal, viewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Chart, registerables } from 'chart.js';
import { ReportsService } from '../../../core/services/reports.service';
import { ReportExportService, ExportableReport } from '../../../core/services/report-export.service';
import type { components } from '../../../core/api/generated/reports.v1';

type ProjectProgressReport = components['schemas']['ProjectProgressReport'];

Chart.register(...registerables);

// T041/T079 — the flagship report. Parameters arrive as query params from the catalog-driven
// picker (T030); this view is purely a renderer over the JSON the API returns — no client-side
// recomputation of completion %, overdue, or projected completion (those are server truths, and
// this view must not risk drifting from them). Export renders the SAME `report()` signal already
// on screen — never a fresh request (T075).
@Component({
  selector: 'app-project-progress',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './project-progress.component.html',
  styleUrl: './project-progress.component.scss',
})
export class ProjectProgressComponent {
  private readonly reportsService = inject(ReportsService);
  private readonly route = inject(ActivatedRoute);
  private readonly exportService = inject(ReportExportService);

  protected readonly report = signal<ProjectProgressReport | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly exporting = signal(false);

  private readonly chartCanvas = viewChild<ElementRef<HTMLCanvasElement>>('progressChart');
  private chart?: Chart;

  constructor() {
    const params = this.route.snapshot.queryParamMap;
    const from = params.get('from') ?? '';
    const to = params.get('to') ?? '';
    const projectScope = params.get('projectScope') ?? undefined;

    this.reportsService.getProjectProgress({ from, to, projectScope }).subscribe({
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

  // The busy flag prevents a double-click from re-entering jsPDF/papaparse mid-render — both
  // calls are synchronous, so this is a same-tick guard, not a network debounce.
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
      columns: ['Project', 'Status', 'Total', 'Open', 'Closed', 'Overdue', 'Completion %', 'Projected Completion'],
      rows: data.rows.map(r => [
        r.projectName, r.status, r.totalTasks, r.openTasks, r.closedTasks, r.overdueTasks,
        r.completionPercent, r.projectedCompletion ?? '',
      ]),
    };
  }

  private renderChart(data: ProjectProgressReport, el: HTMLCanvasElement): void {
    // jsdom (unit tests) has no canvas backend — skip rendering rather than fail unrelated specs.
    if (!el.getContext('2d')) {
      return;
    }

    this.chart?.destroy();
    this.chart = new Chart(el, {
      type: 'bar',
      data: {
        labels: data.rows.map(r => r.projectName),
        datasets: [{ label: 'Completion %', data: data.rows.map(r => r.completionPercent), backgroundColor: '#1e88e5' }],
      },
      options: { responsive: true, scales: { y: { min: 0, max: 100 } } },
    });
  }
}
