import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '../api/generated/team.v1';

type TeamMemberDto = components['schemas']['TeamMemberDto'];
type AddTeamMemberRequest = components['schemas']['AddTeamMemberRequest'];

// No ETag handling anywhere in this service — this feature has none (research R-2: a
// team_members row has no mutable field, so there is nothing for If-Match to protect).
@Injectable({ providedIn: 'root' })
export class TeamService {
  private readonly http = inject(HttpClient);

  list(projectId: string): Observable<TeamMemberDto[]> {
    return this.http.get<TeamMemberDto[]>(`/api/projects/${projectId}/team`);
  }

  add(projectId: string, request: AddTeamMemberRequest): Observable<TeamMemberDto> {
    return this.http.post<TeamMemberDto>(`/api/projects/${projectId}/team`, request);
  }

  remove(projectId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`/api/projects/${projectId}/team/${userId}`);
  }
}
