import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '../api/generated/reports.v1';
import { toQueryParams } from '../http/query-params';

type ReportDescriptor = components['schemas']['ReportDescriptor'];
type ProjectProgressReport = components['schemas']['ProjectProgressReport'];
type TaskCompletionReport = components['schemas']['TaskCompletionReport'];
type TeamPerformanceReport = components['schemas']['TeamPerformanceReport'];
type ActivityReport = components['schemas']['ActivityReport'];

export interface ReportWindowQuery {
  from: string;
  to: string;
}

export interface ProjectProgressQuery extends ReportWindowQuery {
  projectScope?: string;
}

export interface TaskCompletionQuery extends ReportWindowQuery {
  groupBy: 'day' | 'week' | 'month';
  projectScope?: string;
  assigneeId?: string;
}

export interface TeamPerformanceQuery extends ReportWindowQuery {
  projectScope?: string;
  userId?: string;
}

export interface ActivityReportQuery extends ReportWindowQuery {
  projectId?: string;
  entityType?: string;
  actorId?: string;
  page?: number;
  pageSize?: number;
}

// Strictly read-only — no write methods (research R-1's one write happens server-side, never
// initiated from here). Every call is a fresh GET: reports are live per request, never cached
// client-side, matching 005's DashboardService precedent.
@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly http = inject(HttpClient);

  getCatalog(): Observable<ReportDescriptor[]> {
    return this.http.get<ReportDescriptor[]>('/api/reports/catalog');
  }

  getProjectProgress(query: ProjectProgressQuery): Observable<ProjectProgressReport> {
    return this.http.get<ProjectProgressReport>('/api/reports/project-progress', {
      params: toQueryParams(query),
    });
  }

  getTaskCompletion(query: TaskCompletionQuery): Observable<TaskCompletionReport> {
    return this.http.get<TaskCompletionReport>('/api/reports/task-completion', {
      params: toQueryParams(query),
    });
  }

  getTeamPerformance(query: TeamPerformanceQuery): Observable<TeamPerformanceReport> {
    return this.http.get<TeamPerformanceReport>('/api/reports/team-performance', {
      params: toQueryParams(query),
    });
  }

  getActivity(query: ActivityReportQuery): Observable<ActivityReport> {
    return this.http.get<ActivityReport>('/api/reports/activity', {
      params: toQueryParams(query),
    });
  }
}
