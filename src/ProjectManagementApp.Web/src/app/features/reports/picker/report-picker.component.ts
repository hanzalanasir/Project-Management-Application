import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { ReportsService } from '../../../core/services/reports.service';
import { toDateOnlyString } from '../../../core/utils/date-only.util';
import type { components } from '../../../core/api/generated/reports.v1';

type ReportDescriptor = components['schemas']['ReportDescriptor'];
type ReportType = ReportDescriptor['type'];

// Maps a report type to its route segment. This is the ONLY per-report knowledge this component
// carries — everything else (which parameters exist, whether they're required) comes from the
// descriptor at runtime (T030, quickstart V15): add a fifth report to the catalog and a new route
// entry, and the form for it appears here with no further edit.
const ROUTE_BY_TYPE: Record<ReportType, string> = {
  ProjectProgress: 'project-progress',
  TaskCompletion: 'task-completion',
  TeamPerformance: 'team-performance',
  Activity: 'activity',
};

@Component({
  selector: 'app-report-picker',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCardModule,
    MatDatepickerModule,
  ],
  templateUrl: './report-picker.component.html',
  styleUrl: './report-picker.component.scss',
})
export class ReportPickerComponent {
  private readonly reportsService = inject(ReportsService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  protected readonly descriptors = signal<ReportDescriptor[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly selected = signal<ReportDescriptor | null>(null);
  protected form: FormGroup | null = null;

  constructor() {
    this.reportsService.getCatalog().subscribe({
      next: descriptors => {
        this.descriptors.set(descriptors);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(err.error?.detail ?? err.error?.title ?? 'Could not load the report catalog.');
      },
    });
  }

  // Builds a fresh Reactive Form FROM the selected descriptor's parameter list — no per-report-type
  // form definition anywhere in this file.
  protected selectReport(descriptor: ReportDescriptor): void {
    this.selected.set(descriptor);

    const controls: Record<string, unknown> = {};
    for (const parameter of descriptor.parameters) {
      const initialValue = parameter.type === 'date' ? null : '';
      controls[parameter.name] = [initialValue, parameter.required ? [Validators.required] : []];
    }
    this.form = this.fb.group(controls);
  }

  protected isDateParam(parameterType: string): boolean {
    return parameterType === 'date';
  }

  protected inputTypeFor(parameterType: string): string {
    if (parameterType === 'uuid') return 'text';
    if (parameterType === 'integer') return 'number';
    return 'text';
  }

  protected optionsFor(parameterType: string): string[] | null {
    // "day|week|month", "User|Project|Task|TeamMember|Report" — a pipe-delimited type is a closed
    // enum, rendered as a select; everything else is free text. "all|projectIds" is deliberately
    // NOT treated as an enum — it's free text with "all" as the common value.
    if (parameterType === 'all|projectIds') return null;
    return parameterType.includes('|') ? parameterType.split('|') : null;
  }

  protected submit(): void {
    const descriptor = this.selected();
    if (!descriptor || !this.form) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const dateParamNames = new Set(descriptor.parameters.filter(p => p.type === 'date').map(p => p.name));
    const queryParams: Record<string, string> = {};
    for (const [key, value] of Object.entries(this.form.getRawValue() as Record<string, unknown>)) {
      if (value === '' || value === null || value === undefined) continue;
      queryParams[key] = dateParamNames.has(key) ? toDateOnlyString(value as Date) : (value as string);
    }

    void this.router.navigate(['/reports', ROUTE_BY_TYPE[descriptor.type]], { queryParams });
  }
}
