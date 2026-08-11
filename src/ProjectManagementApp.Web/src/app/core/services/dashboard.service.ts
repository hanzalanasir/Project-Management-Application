import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '../api/generated/dashboard.v1';
import { toQueryParams } from '../http/query-params';

type DashboardSummary = components['schemas']['DashboardSummary'];
type PagedActivityEntry = components['schemas']['PagedActivityEntry'];

export interface ActivityQuery {
  page?: number;
  pageSize?: number;
}

// Strictly read-only — no write methods on this service, matching the feature (research R-6).
// Both calls refetch on navigation rather than caching: values are live per request in v1.
@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  getSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>('/api/dashboard/summary');
  }

  getActivity(query: ActivityQuery = {}): Observable<PagedActivityEntry> {
    return this.http.get<PagedActivityEntry>('/api/dashboard/activity', {
      params: toQueryParams(query),
    });
  }
}
