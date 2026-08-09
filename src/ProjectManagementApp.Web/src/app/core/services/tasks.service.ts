import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import type { components } from '../api/generated/tasks.v1';

type PagedTaskSummary = components['schemas']['PagedTaskSummary'];
type TaskDetail = components['schemas']['TaskDetail'];
type CreateTaskRequest = components['schemas']['CreateTaskRequest'];
type UpdateTaskRequest = components['schemas']['UpdateTaskRequest'];
type UpdateTaskStatusRequest = components['schemas']['UpdateTaskStatusRequest'];
type ReassignTaskRequest = components['schemas']['ReassignTaskRequest'];

export interface TaskListQuery {
  page?: number;
  pageSize?: number;
  status?: string;
  assigneeId?: string;
  search?: string;
  sort?: string;
}

export interface TaskDetailWithETag {
  detail: TaskDetail;
  etag: string;
}

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient);

  listByProject(projectId: string, query: TaskListQuery = {}): Observable<PagedTaskSummary> {
    return this.http.get<PagedTaskSummary>(`/api/projects/${projectId}/tasks`, {
      params: { ...query } as Record<string, string | number>,
    });
  }

  list(query: TaskListQuery & { projectId?: string } = {}): Observable<PagedTaskSummary> {
    return this.http.get<PagedTaskSummary>('/api/tasks', { params: { ...query } as Record<string, string | number> });
  }

  create(projectId: string, request: CreateTaskRequest): Observable<TaskDetailWithETag> {
    return this.http
      .post<TaskDetail>(`/api/projects/${projectId}/tasks`, request, { observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  getById(id: string): Observable<TaskDetailWithETag> {
    return this.http
      .get<TaskDetail>(`/api/tasks/${id}`, { observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  update(id: string, request: UpdateTaskRequest, ifMatch: string): Observable<TaskDetailWithETag> {
    return this.http
      .put<TaskDetail>(`/api/tasks/${id}`, request, { headers: { 'If-Match': ifMatch }, observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  updateStatus(id: string, request: UpdateTaskStatusRequest, ifMatch: string): Observable<TaskDetailWithETag> {
    return this.http
      .put<TaskDetail>(`/api/tasks/${id}/status`, request, { headers: { 'If-Match': ifMatch }, observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  reassign(id: string, request: ReassignTaskRequest, ifMatch: string): Observable<TaskDetailWithETag> {
    return this.http
      .put<TaskDetail>(`/api/tasks/${id}/assignee`, request, { headers: { 'If-Match': ifMatch }, observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/tasks/${id}`);
  }

  private withETag(response: HttpResponse<TaskDetail>): TaskDetailWithETag {
    return { detail: response.body!, etag: response.headers.get('ETag') ?? '' };
  }
}
