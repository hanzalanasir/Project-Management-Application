import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { HttpErrorResponse } from '@angular/common/http';
import { AdminUsersService } from '../../../../core/services/admin-users.service';
import { ErrorDisplayComponent } from '../../../../shared/error-display/error-display.component';
import type { components } from '../../../../core/api/generated/auth.v1';

type AdminUserDetail = components['schemas']['AdminUserDetail'];
type Role = components['schemas']['Role'];

@Component({
  selector: 'app-admin-user-detail',
  imports: [MatCardModule, MatFormFieldModule, MatSelectModule, MatButtonModule, ErrorDisplayComponent],
  templateUrl: './admin-user-detail.component.html',
  styleUrl: './admin-user-detail.component.scss',
})
export class AdminUserDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly adminUsersService = inject(AdminUsersService);

  protected readonly detail = signal<AdminUserDetail | null>(null);
  protected readonly errors = signal<string[] | null>(null);
  private etag = '';
  private readonly userId = this.route.snapshot.paramMap.get('id')!;

  protected readonly roles: Role[] = ['Admin', 'ProjectManager', 'TeamMember'];

  constructor() {
    this.reload();
  }

  private reload(): void {
    this.adminUsersService.getById(this.userId).subscribe(({ detail, etag }) => {
      this.detail.set(detail);
      this.etag = etag;
    });
  }

  protected changeRole(newRole: Role): void {
    if (!confirm(`Change this user's role to ${newRole}?`)) {
      return;
    }

    this.errors.set(null);
    this.adminUsersService.changeRole(this.userId, { role: newRole }, this.etag).subscribe({
      next: ({ detail, etag }) => {
        this.detail.set(detail);
        this.etag = etag;
      },
      error: (error: HttpErrorResponse) => this.errors.set([error.error?.detail ?? error.error?.title ?? 'Role change failed.']),
    });
  }

  protected toggleStatus(): void {
    const current = this.detail();
    if (!current) return;

    const willDeactivate = current.isActive;
    const message = willDeactivate
      ? `Deactivate ${current.fullName}? Their active sessions will end immediately.`
      : `Reactivate ${current.fullName}?`;
    if (!confirm(message)) {
      return;
    }

    this.errors.set(null);
    this.adminUsersService.changeStatus(this.userId, { isActive: !current.isActive }, this.etag).subscribe({
      next: ({ detail, etag }) => {
        this.detail.set(detail);
        this.etag = etag;
      },
      error: (error: HttpErrorResponse) => this.errors.set([error.error?.detail ?? error.error?.title ?? 'Status change failed.']),
    });
  }
}
