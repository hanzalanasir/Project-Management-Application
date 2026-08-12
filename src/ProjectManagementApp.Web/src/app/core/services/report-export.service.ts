import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import Papa from 'papaparse';

/**
 * A report already flattened to a plain table by its own view component — the export service is
 * deliberately report-shape-agnostic (Constitution VII.8: ONE service, never duplicated per
 * component), so ProjectProgressComponent/TaskCompletionComponent/TeamPerformanceComponent/
 * ActivityReportComponent each map their own typed rows/buckets/items into this shape rather than
 * the service knowing about four different report DTOs.
 */
export interface ExportableReport {
  reportType: string;
  windowFrom: string;
  windowTo: string;
  columns: string[];
  rows: (string | number)[][];
}

// T078 — the one service every report view's Export controls (T079) call. Both methods render
// EXCLUSIVELY from the ExportableReport passed in — the same JSON object the on-screen preview
// already holds — never a fresh HTTP call (T075) and therefore never a second audit row (T076).
@Injectable({ providedIn: 'root' })
export class ReportExportService {
  toPdf(report: ExportableReport): void {
    const doc = new jsPDF();
    const title = `${report.reportType} (${report.windowFrom} – ${report.windowTo})`;
    doc.setFontSize(12);
    doc.text(title, 10, 12);

    doc.setFontSize(9);
    let y = 22;
    const lineHeight = 6;
    const pageHeight = doc.internal.pageSize.getHeight();

    const writeLine = (text: string): void => {
      if (y > pageHeight - 10) {
        doc.addPage();
        y = 12;
      }
      doc.text(text, 10, y);
      y += lineHeight;
    };

    writeLine(report.columns.join(' | '));
    if (report.rows.length === 0) {
      writeLine('(no data in this window)');
    } else {
      for (const row of report.rows) {
        writeLine(row.map(cell => String(cell)).join(' | '));
      }
    }

    doc.save(this.filename(report, 'pdf'));
  }

  toCsv(report: ExportableReport): void {
    const csv = Papa.unparse({ fields: report.columns, data: report.rows });
    this.downloadTextFile(csv, this.filename(report, 'csv'), 'text/csv;charset=utf-8;');
  }

  private filename(report: ExportableReport, extension: 'pdf' | 'csv'): string {
    const type = report.reportType.replace(/([a-z])([A-Z])/g, '$1-$2').toLowerCase();
    return `${type}_${report.windowFrom}_${report.windowTo}.${extension}`;
  }

  private downloadTextFile(content: string, filename: string, mimeType: string): void {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = filename;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
