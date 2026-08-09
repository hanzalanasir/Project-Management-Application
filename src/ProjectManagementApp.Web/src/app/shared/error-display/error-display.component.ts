import { Component, input } from '@angular/core';

// Shared field-level validation error renderer (Constitution VII.6) — every reactive form
// surfaces errors through this component rather than ad-hoc markup per field.
@Component({
  selector: 'app-error-display',
  template: `
    @if (errors() && errors()!.length > 0) {
      <div class="error-display" role="alert">
        @for (error of errors(); track error) {
          <div class="error-display__message">{{ error }}</div>
        }
      </div>
    }
  `,
  styles: [`
    .error-display {
      color: #b3261e;
      font-size: 0.75rem;
      margin-top: 4px;
    }
  `]
})
export class ErrorDisplayComponent {
  readonly errors = input<string[] | null>(null);
}
