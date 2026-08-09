import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { ProjectsService } from '../../../core/services/projects.service';
import { authFeature } from '../../../core/store/auth/auth.feature';
import type { components } from '../../../core/api/generated/projects.v1';

type ProjectSummary = components['schemas']['ProjectSummary'];
type ProjectStatus = components['schemas']['ProjectStatus'];

// Renders exactly what the API returns — no client-side role filtering (quickstart V16 point 2);
// the scope gate lives server-side in ListProjectsQueryHandler, not here.
@Component({
  selector: 'app-project-list',
  imports: [RouterLink, FormsModule, MatTableModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatPaginatorModule],
  templateUrl: './project-list.component.html',
  styleUrl: './project-list.component.scss',
})
export class ProjectListComponent {
  private readonly projectsService = inject(ProjectsService);
  private readonly store = inject(Store);

  protected readonly statuses: ProjectStatus[] = ['Planning', 'Active', 'OnHold', 'Completed', 'Cancelled'];
  protected readonly isTeamMember = this.store.selectSignal(authFeature.selectUser)()?.role === 'TeamMember';
  protected readonly displayedColumns = this.isTeamMember
    ? ['name', 'status', 'startDate', 'endDate', 'owner']
    : ['name', 'status', 'startDate', 'endDate', 'owner', 'actions'];

  protected readonly items = signal<ProjectSummary[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageIndex = signal(0);
  protected readonly pageSize = signal(20);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected search = '';
  protected status: ProjectStatus | '' = '';

  constructor() {
    this.reload();
  }

  protected onSearchChange(): void {
    this.pageIndex.set(0);
    this.reload();
  }

  protected onStatusChange(): void {
    this.pageIndex.set(0);
    this.reload();
  }

  protected onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.reload();
  }

  // Behind a confirmation dialog naming the project and warning its tasks/assignments are removed
  // too (spec US-002-05, mirrored from the detail view's delete action).
  protected deleteProject(project: ProjectSummary): void {
    if (!confirm(`Delete "${project.name}"? Its tasks and team assignments will be removed too.`)) {
      return;
    }

    this.projectsService.delete(project.id).subscribe({
      next: () => this.reload(),
      error: () => this.error.set('Could not delete project.'),
    });
  }

  private reload(): void {
    this.loading.set(true);
    this.error.set(null);

    this.projectsService
      .list({
        page: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        search: this.search || undefined,
        status: this.status || undefined,
      })
      .subscribe({
        next: page => {
          this.items.set(page.items);
          this.totalCount.set(page.totalCount);
          this.pageSize.set(page.pageSize);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load projects.');
          this.loading.set(false);
        },
      });
  }
}
