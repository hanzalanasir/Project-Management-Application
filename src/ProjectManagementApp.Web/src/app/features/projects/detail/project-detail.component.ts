import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { HttpErrorResponse } from '@angular/common/http';
import { ProjectsService } from '../../../core/services/projects.service';
import { authFeature } from '../../../core/store/auth/auth.feature';
import type { components } from '../../../core/api/generated/projects.v1';

type ProjectDetail = components['schemas']['ProjectDetail'];

// Anchor screen every later feature extends (T055). The Team section is a placeholder note, not a
// live link — 004 (Team roster, features/team/roster/) has not been implemented yet in this
// codebase; design.md §3/§5 calls for it to attach here once it exists.
@Component({
  selector: 'app-project-detail',
  imports: [RouterLink, MatCardModule, MatButtonModule],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss',
})
export class ProjectDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly projectsService = inject(ProjectsService);
  private readonly store = inject(Store);

  protected readonly project = signal<ProjectDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly forbidden = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly currentUser = this.store.selectSignal(authFeature.selectUser);
  private readonly projectId = this.route.snapshot.paramMap.get('id')!;

  constructor() {
    this.reload();
  }

  protected canManage(): boolean {
    const user = this.currentUser();
    const project = this.project();
    if (!user || !project) return false;
    return user.role === 'Admin' || (user.role === 'ProjectManager' && project.owner.id === user.id);
  }

  protected deleteProject(): void {
    const project = this.project();
    if (!project) return;
    if (!confirm(`Delete "${project.name}"? Its tasks and team assignments will be removed too.`)) {
      return;
    }

    this.projectsService.delete(this.projectId).subscribe({
      next: () => void this.router.navigate(['/projects']),
      error: (err: HttpErrorResponse) => this.error.set(err.error?.detail ?? err.error?.title ?? 'Could not delete project.'),
    });
  }

  private reload(): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.forbidden.set(false);
    this.error.set(null);

    this.projectsService.getById(this.projectId).subscribe({
      next: ({ detail }) => {
        this.project.set(detail);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 404) {
          this.notFound.set(true);
        } else if (err.status === 403) {
          this.forbidden.set(true);
        } else {
          this.error.set('Could not load this project.');
        }
      },
    });
  }
}
