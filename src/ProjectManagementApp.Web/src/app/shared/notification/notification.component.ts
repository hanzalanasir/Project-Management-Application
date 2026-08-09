import { Component, inject } from '@angular/core';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { NotificationService } from './notification.service';

// Global snackbar/toast sink — HTTP errors (via ErrorInterceptor) and uncaught exceptions
// (via GlobalErrorHandler) both funnel here (Constitution VII.7).
@Component({
  selector: 'app-notification',
  imports: [MatSnackBarModule],
  template: `
    @for (message of notifications.messages(); track message.id) {
      <div class="notification" [class.notification--error]="message.level === 'error'">
        {{ message.text }}
      </div>
    }
  `,
  styles: [`
    .notification {
      position: fixed;
      bottom: 16px;
      right: 16px;
      padding: 12px 16px;
      border-radius: 4px;
      background: #323232;
      color: white;
      margin-top: 8px;
      z-index: 1000;
    }
    .notification--error {
      background: #b3261e;
    }
  `]
})
export class NotificationComponent {
  protected readonly notifications = inject(NotificationService);
}
