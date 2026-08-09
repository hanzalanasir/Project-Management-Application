import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { AdminUsersService } from '../../../../core/services/admin-users.service';
import type { components } from '../../../../core/api/generated/auth.v1';

type AdminUserSummary = components['schemas']['AdminUserSummary'];

@Component({
  selector: 'app-admin-users-list',
  imports: [RouterLink, MatTableModule, MatChipsModule],
  templateUrl: './admin-users-list.component.html',
  styleUrl: './admin-users-list.component.scss',
})
export class AdminUsersListComponent {
  private readonly adminUsersService = inject(AdminUsersService);

  protected readonly users = signal<AdminUserSummary[]>([]);
  protected readonly displayedColumns = ['fullName', 'email', 'role', 'status'];

  constructor() {
    this.adminUsersService.list().subscribe(page => this.users.set(page.items));
  }
}
