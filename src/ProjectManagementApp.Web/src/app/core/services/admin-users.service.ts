import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import type { components } from '../api/generated/auth.v1';

type PagedAdminUserSummary = components['schemas']['PagedAdminUserSummary'];
type AdminUserDetail = components['schemas']['AdminUserDetail'];
type ChangeUserRoleRequest = components['schemas']['ChangeUserRoleRequest'];
type ChangeUserStatusRequest = components['schemas']['ChangeUserStatusRequest'];

export interface AdminUserDetailWithETag {
  detail: AdminUserDetail;
  etag: string;
}

@Injectable({ providedIn: 'root' })
export class AdminUsersService {
  private readonly http = inject(HttpClient);

  list(page = 1, pageSize = 20): Observable<PagedAdminUserSummary> {
    return this.http.get<PagedAdminUserSummary>('/api/users', { params: { page, pageSize } });
  }

  getById(id: string): Observable<AdminUserDetailWithETag> {
    return this.http
      .get<AdminUserDetail>(`/api/users/${id}`, { observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  changeRole(id: string, request: ChangeUserRoleRequest, ifMatch: string): Observable<AdminUserDetailWithETag> {
    return this.http
      .put<AdminUserDetail>(`/api/users/${id}/role`, request, { headers: { 'If-Match': ifMatch }, observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  changeStatus(id: string, request: ChangeUserStatusRequest, ifMatch: string): Observable<AdminUserDetailWithETag> {
    return this.http
      .put<AdminUserDetail>(`/api/users/${id}/status`, request, { headers: { 'If-Match': ifMatch }, observe: 'response' })
      .pipe(map(response => this.withETag(response)));
  }

  private withETag(response: HttpResponse<AdminUserDetail>): AdminUserDetailWithETag {
    return { detail: response.body!, etag: response.headers.get('ETag') ?? '' };
  }
}
