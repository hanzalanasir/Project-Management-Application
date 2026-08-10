import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { TeamService } from '../../../core/services/team.service';
import { ProjectsService } from '../../../core/services/projects.service';
import { authFeature } from '../../../core/store/auth/auth.feature';
import { AddMemberDialogComponent } from '../add-member-dialog/add-member-dialog.component';
import type { components } from '../../../core/api/generated/team.v1';

type TeamMemberDto = components['schemas']['TeamMemberDto'];

// The roster table (T040/T053). No client-side paging — research R-4: a team is bounded/human-
// scale, so the whole array renders at once (client-side search only, no server paging, matching
// the endpoint's own plain-array shape). The remove column only renders once we know whether the
// caller can manage this project's team: Admin always can; a ProjectManager only if they own it,
// which this component learns by fetching the project detail once (the roster DTO itself carries
// no ownership info — membership is a link, not a role, so there is nothing to check on each row).
@Component({
  selector: 'app-team-roster',
  imports: [RouterLink, FormsModule, DatePipe, MatDialogModule, MatButtonModule, MatTableModule],
  templateUrl: './roster.component.html',
  styleUrl: './roster.component.scss',
})
export class RosterComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly teamService = inject(TeamService);
  private readonly projectsService = inject(ProjectsService);
  private readonly dialog = inject(MatDialog);
  private readonly store = inject(Store);

  protected readonly projectId = this.route.snapshot.paramMap.get('projectId') ?? '';
  private readonly currentUser = this.store.selectSignal(authFeature.selectUser);

  protected readonly members = signal<TeamMemberDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly forbidden = signal(false);
  protected readonly canManage = signal(false);
  protected readonly removeError = signal<string | null>(null);
  protected search = '';

  protected get displayedColumns(): string[] {
    return this.canManage()
      ? ['fullName', 'email', 'role', 'addedAt', 'actions']
      : ['fullName', 'email', 'role', 'addedAt'];
  }

  constructor() {
    this.refresh();
    this.loadOwnership();
  }

  protected get filteredMembers(): TeamMemberDto[] {
    const term = this.search.trim().toLowerCase();
    if (!term) {
      return this.members();
    }

    return this.members().filter(
      m => m.fullName.toLowerCase().includes(term) || m.email.toLowerCase().includes(term)
    );
  }

  protected refresh(): void {
    this.loading.set(true);
    this.error.set(null);
    this.forbidden.set(false);

    this.teamService.list(this.projectId).subscribe({
      next: members => {
        this.members.set(members);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 403) {
          this.forbidden.set(true);
        } else {
          this.error.set(err.error?.detail ?? err.error?.title ?? 'Could not load the team.');
        }
      },
    });
  }

  protected openAddMemberDialog(): void {
    const dialogRef = this.dialog.open(AddMemberDialogComponent, {
      data: { projectId: this.projectId },
    });

    dialogRef.afterClosed().subscribe((result: TeamMemberDto | undefined) => {
      if (result) {
        this.refresh();
      }
    });
  }

  protected removeMember(member: TeamMemberDto): void {
    if (!confirm(`Remove ${member.fullName} from this project's team? They will lose access immediately.`)) {
      return;
    }

    this.removeError.set(null);

    this.teamService.remove(this.projectId, member.userId).subscribe({
      next: () => this.refresh(),
      error: (err: HttpErrorResponse) => {
        this.removeError.set(
          err.status === 409
            ? (err.error?.detail ?? 'Cannot remove: this member has open tasks assigned in this project. Reassign or close them first.')
            : (err.error?.detail ?? err.error?.title ?? 'Could not remove this member.')
        );
      },
    });
  }

  private loadOwnership(): void {
    const user = this.currentUser();
    if (!user) {
      return;
    }

    if (user.role === 'Admin') {
      this.canManage.set(true);
      return;
    }

    if (user.role !== 'ProjectManager') {
      return;
    }

    this.projectsService.getById(this.projectId).subscribe({
      next: ({ detail }) => this.canManage.set(detail.owner.id === user.id),
      error: () => this.canManage.set(false),
    });
  }
}
