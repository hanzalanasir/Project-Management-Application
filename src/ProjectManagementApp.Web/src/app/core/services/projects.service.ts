import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import type { components } from '../api/generated/projects.v1';

type PagedProjectSummary = components['schemas']['PagedProjectSummary'];
type ProjectDetail = components['schemas']['ProjectDetail'];
type CreateProjectRequest = components['schemas']['CreateProjectRequest'];
type UpdateProjectRequest = components['schemas']['UpdateProjectRequest'];

export interface ProjectListQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  sort?: string;
}

export interface ProjectDetailWithETag {
  detail: ProjectDetail;
  etag: string;
}

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  private readonly http = inject(HttpClient);

  list(query: ProjectListQuery = {}): Observable<PagedProjectSummary> {
    return this.http.get<PagedProjectSummary>('/api/projects', { params: { ...query } as Record<string, string | number> });
  }

  create(request: CreateProjectRequest): Observable<ProjectDetailWithETag> {
    return this.http
      .post<ProjectDetail>('/api/projects', request, { observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  getById(id: string): Observable<ProjectDetailWithETag> {
    return this.http
      .get<ProjectDetail>(`/api/projects/${id}`, { observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  update(id: string, request: UpdateProjectRequest, ifMatch: string): Observable<ProjectDetailWithETag> {
    return this.http
      .put<ProjectDetail>(`/api/projects/${id}`, request, { headers: { 'If-Match': ifMatch }, observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/projects/${id}`);
  }

  private withETag(response: HttpResponse<ProjectDetail>): ProjectDetailWithETag {
    return { detail: response.body!, etag: response.headers.get('ETag') ?? '' };
  }
}
