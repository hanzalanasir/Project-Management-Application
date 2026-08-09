import { Injectable, signal } from '@angular/core';

export interface NotificationMessage {
  id: number;
  text: string;
  level: 'error' | 'info';
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 0;
  readonly messages = signal<NotificationMessage[]>([]);

  notifyError(text: string): void {
    this.push(text, 'error');
  }

  notifyInfo(text: string): void {
    this.push(text, 'info');
  }

  dismiss(id: number): void {
    this.messages.update(list => list.filter(m => m.id !== id));
  }

  private push(text: string, level: 'error' | 'info'): void {
    const id = this.nextId++;
    this.messages.update(list => [...list, { id, text, level }]);
    setTimeout(() => this.dismiss(id), 6000);
  }
}
