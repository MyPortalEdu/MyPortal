import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PageResult, IdResponse } from '../types/bulletin';
import { QueryKitParams } from '../utils/primeng-querykit';
import { PermissionResponse } from '../types/role';
import {
  UserSummaryResponse,
  UserDetailsResponse,
  UserUpsertRequest,
  UserUpdateRequest,
  UserSetPasswordRequest,
  PersonSearchResponse,
} from '../types/user';

@Injectable({ providedIn: 'root' })
export class UsersDataService {
  private readonly http = inject(HttpClient);

  list(params: QueryKitParams): Observable<PageResult<UserSummaryResponse>> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));
    if (params.filter) httpParams = httpParams.set('filter', params.filter);
    if (params.sort) httpParams = httpParams.set('sort', params.sort);
    return this.http.get<PageResult<UserSummaryResponse>>('/api/users', { params: httpParams });
  }

  getById(id: string): Observable<UserDetailsResponse> {
    return this.http.get<UserDetailsResponse>(`/api/users/${id}`);
  }

  create(model: UserUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/users', model);
  }

  update(id: string, model: UserUpdateRequest): Observable<void> {
    return this.http.put<void>(`/api/users/${id}`, model);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/users/${id}`);
  }

  // Admin password reset (distinct from the self-service change-password on /api/me).
  setPassword(id: string, model: UserSetPasswordRequest): Observable<void> {
    return this.http.put<void>(`/api/users/${id}/password`, model);
  }

  // The user's effective permissions — the union across their assigned roles.
  effectivePermissions(id: string): Observable<PermissionResponse[]> {
    return this.http.get<PermissionResponse[]>(`/api/users/${id}/permissions`);
  }

  searchPeople(query: string): Observable<PersonSearchResponse[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<PersonSearchResponse[]>('/api/people/search', { params });
  }
}
