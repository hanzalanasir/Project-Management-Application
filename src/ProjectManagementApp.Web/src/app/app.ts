import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NotificationComponent } from './shared/notification/notification.component';
import { ShellHeaderComponent } from './core/shell-header/shell-header.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NotificationComponent, ShellHeaderComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
}
