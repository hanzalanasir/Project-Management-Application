import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '../api/generated/auth.v1';

type RegisterRequest = components['schemas']['RegisterRequest'];
type LoginRequest = components['schemas']['LoginRequest'];
type UserDto = components['schemas']['UserDto'];
type AuthTokensResponse = components['schemas']['AuthTokensResponse'];

// HTTP lives here, never in components (Constitution VII.3). DTO types are generated from the
// contract (research.md R-6); this service itself stays hand-written.
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  register(request: RegisterRequest): Observable<UserDto> {
    return this.http.post<UserDto>('/api/auth/register', request);
  }

  login(request: LoginRequest): Observable<AuthTokensResponse> {
    // withCredentials so the refresh-token Set-Cookie response is honored consistently
    // with cross-context requests, matching the /api/auth/* cookie scope (ADR-0002).
    return this.http.post<AuthTokensResponse>('/api/auth/login', request, { withCredentials: true });
  }

  logout(): Observable<void> {
    return this.http.post<void>('/api/auth/logout', null, { withCredentials: true });
  }

  refresh(): Observable<AuthTokensResponse> {
    return this.http.post<AuthTokensResponse>('/api/auth/refresh', null, { withCredentials: true });
  }
}
