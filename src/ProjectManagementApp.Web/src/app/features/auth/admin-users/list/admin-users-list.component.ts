import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AdminUsersService } from '../../../../core/services/admin-users.service';
import type { components } from '../../../../core/api/generated/auth.v1';

type AdminUserSummary = components['schemas']['AdminUserSummary'];

@Component({
  selector: 'app-admin-users-list',
  imports: [RouterLink, MatTableModule, MatButtonModule, MatIconModule],
  templateUrl: './admin-users-list.component.html',
  styleUrl: './admin-users-list.component.scss',
})
export class AdminUsersListComponent {
  private readonly adminUsersService = inject(AdminUsersService);

  protected readonly users = signal<AdminUserSummary[]>([]);
  protected readonly displayedColumns = ['fullName', 'userId', 'email', 'role', 'status'];

  // Briefly shows a checkmark on the button just copied, so "did that work?" has a visible answer.
  protected readonly copiedId = signal<string | null>(null);

  constructor() {
    this.adminUsersService.list().subscribe(page => this.users.set(page.items));
  }

  protected copyId(id: string): void {
    void navigator.clipboard.writeText(id);
    this.copiedId.set(id);
    setTimeout(() => {
      if (this.copiedId() === id) {
        this.copiedId.set(null);
      }
    }, 1500);
  }
}
