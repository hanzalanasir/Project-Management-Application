import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { HttpErrorResponse } from '@angular/common/http';
import { TeamService } from '../../../core/services/team.service';
import { ErrorDisplayComponent } from '../../../shared/error-display/error-display.component';
import { authFeature } from '../../../core/store/auth/auth.feature';
import { GUID_PATTERN } from '../../../shared/validators/guid.validator';
import type { components } from '../../../core/api/generated/team.v1';

type TeamMemberDto = components['schemas']['TeamMemberDto'];

export interface AddMemberDialogData {
  projectId: string;
}

// A single "user must be selected" validator, per T031 — but a real SEARCHABLE picker of "any
// active user, regardless of global role" cannot be built within 004's scope: GET /api/users is
// Admin-only by design (001, spec US-001-07 7Cs), so a ProjectManager — the caller who actually
// performs adds — has no endpoint to search/list users. This mirrors the exact same forward-
// dependency workaround 003's CreateTaskComponent used for assigneeId before 004 existed: a plain
// user-id input rather than a picker. Flagged rather than silently faked.
@Component({
  selector: 'app-add-member-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, ErrorDisplayComponent],
  templateUrl: './add-member-dialog.component.html',
  styleUrl: './add-member-dialog.component.scss',
})
export class AddMemberDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly teamService = inject(TeamService);
  private readonly dialogRef = inject(MatDialogRef<AddMemberDialogComponent>);
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  protected readonly data = inject<AddMemberDialogData>(MAT_DIALOG_DATA);

  // Only an Admin can actually reach the Users page (its route is Admin-only, auth.routes.ts) —
  // a PM/TeamMember sees plain instruction text instead of a link that would 404 for them.
  protected readonly isAdmin = this.store.selectSignal(authFeature.selectUser)()?.role === 'Admin';

  protected readonly submitting = signal(false);
  protected readonly serverErrors = signal<string[] | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    userId: ['', [Validators.required, Validators.pattern(GUID_PATTERN)]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.serverErrors.set(null);
    const { userId } = this.form.getRawValue();

    this.teamService.add(this.data.projectId, { userId }).subscribe({
      next: (member: TeamMemberDto) => {
        this.submitting.set(false);
        this.dialogRef.close(member);
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        const errors = error.error?.errors;
        this.serverErrors.set(
          errors ? (Object.values(errors).flat() as string[]) : [error.error?.detail ?? error.error?.title ?? 'Could not add member.']
        );
      },
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }

  protected openUsersPage(): void {
    this.dialogRef.close();
    void this.router.navigateByUrl('/auth/admin-users');
  }
}
