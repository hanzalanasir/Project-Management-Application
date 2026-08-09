import { ErrorHandler, Injectable, inject } from '@angular/core';
import { NotificationService } from '../shared/notification/notification.service';

// Catches uncaught exceptions anywhere in the app and funnels them to the shared notification
// component, alongside the HTTP ErrorInterceptor (Constitution VII.7).
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly notifications = inject(NotificationService);

  handleError(error: unknown): void {
    console.error(error);
    this.notifications.notifyError('Something went wrong. Please try again.');
  }
}
