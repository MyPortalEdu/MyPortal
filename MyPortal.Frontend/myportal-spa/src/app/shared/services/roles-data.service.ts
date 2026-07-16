import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserType } from '../../core/types/user-type';
import { PageResult } from '../types/bulletin';
import {
  RoleSummaryResponse,
  RoleDetailsResponse,
  RoleUpsertRequest,
  PermissionResponse,
} from '../types/role';

@Injectable({ providedIn: 'root' })
export class RolesDataService {
  private readonly http = inject(HttpClient);

  // Roles are few — fetch a single large page rather than wiring server-side paging.
  list(): Observable<PageResult<RoleSummaryResponse>> {
    const params = new HttpParams().set('page', '1').set('pageSize', '100');
    return this.http.get<PageResult<RoleSummaryResponse>>('/api/roles', { params });
  }

  getById(id: string): Observable<RoleDetailsResponse> {
    return this.http.get<RoleDetailsResponse>(`/api/roles/${id}`);
  }

  create(model: RoleUpsertRequest): Observable<void> {
    return this.http.post<void>('/api/roles', model);
  }

  update(id: string, model: RoleUpsertRequest): Observable<void> {
    return this.http.put<void>(`/api/roles/${id}`, model);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/roles/${id}`);
  }

  // The permission catalogue for one portal — the role editor only offers permissions matching the
  // role's audience.
  permissions(userType: UserType): Observable<PermissionResponse[]> {
    const params = new HttpParams().set('userType', String(userType));
    return this.http.get<PermissionResponse[]>('/api/permissions', { params });
  }
}
