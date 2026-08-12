import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ReportsService } from '../../../core/services/reports.service';
import { ReportExportService, ExportableReport } from '../../../core/services/report-export.service';
import type { components } from '../../../core/api/generated/reports.v1';

type ActivityReport = components['schemas']['ActivityReport'];

// T072/T079 — the one report that can be large, and therefore the one that can 422. The "narrow
// your range" prompt is shown INSTEAD of a table on 422 — never attempts to render partial data,
// and the export buttons only appear inside the `report(); as data` branch (T077) — when tooLarge
// is set, `report` stays null, so there is no previewed data an export could even reach.
@Component({
  selector: 'app-activity-report',
  templateUrl: './activity-report.component.html',
  styleUrl: './activity-report.component.scss',
})
export class ActivityReportComponent {
  private readonly reportsService = inject(ReportsService);
  private readonly route = inject(ActivatedRoute);
  private readonly exportService = inject(ReportExportService);

  protected readonly report = signal<ActivityReport | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly tooLarge = signal(false);
  protected readonly exporting = signal(false);

  constructor() {
    this.load();
  }

  private load(): void {
    const params = this.route.snapshot.queryParamMap;
    const from = params.get('from') ?? '';
    const to = params.get('to') ?? '';
    const projectId = params.get('projectId') ?? undefined;
    const entityType = params.get('entityType') ?? undefined;
    const actorId = params.get('actorId') ?? undefined;
    const page = params.get('page') ? Number(params.get('page')) : undefined;
    const pageSize = params.get('pageSize') ? Number(params.get('pageSize')) : undefined;

    this.loading.set(true);
    this.error.set(null);
    this.tooLarge.set(false);

    this.reportsService.getActivity({ from, to, projectId, entityType, actorId, page, pageSize }).subscribe({
      next: report => {
        this.report.set(report);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 422) {
          this.tooLarge.set(true);
        } else {
          this.error.set(err.error?.detail ?? err.error?.title ?? 'Could not load the report.');
        }
      },
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
      columns: ['When', 'Actor', 'Action', 'Entity', 'Summary'],
      rows: data.items.map(i => [i.timestamp, i.actorName ?? 'System', i.action, i.entityType, i.changeSummary]),
    };
  }
}
