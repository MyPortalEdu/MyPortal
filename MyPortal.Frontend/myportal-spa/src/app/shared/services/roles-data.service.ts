import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserType } from '../../core/types/user-type';
import { PageResult, IdResponse } from '../types/bulletin';
import { QueryKitParams } from '../utils/primeng-querykit';
import {
  RoleSummaryResponse,
  RoleDetailsResponse,
  RoleUpsertRequest,
  PermissionResponse,
} from '../types/role';

@Injectable({ providedIn: 'root' })
export class RolesDataService {
  private readonly http = inject(HttpClient);

  // A single large page — for callers that need every role (e.g. the role picker's audience filter).
  all(): Observable<PageResult<RoleSummaryResponse>> {
    const params = new HttpParams().set('page', '1').set('pageSize', '100');
    return this.http.get<PageResult<RoleSummaryResponse>>('/api/roles', { params });
  }

  // Server-paged/filtered/sorted — for the grid.
  list(params: QueryKitParams): Observable<PageResult<RoleSummaryResponse>> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));
    if (params.filter) httpParams = httpParams.set('filter', params.filter);
    if (params.sort) httpParams = httpParams.set('sort', params.sort);
    return this.http.get<PageResult<RoleSummaryResponse>>('/api/roles', { params: httpParams });
  }

  getById(id: string): Observable<RoleDetailsResponse> {
    return this.http.get<RoleDetailsResponse>(`/api/roles/${id}`);
  }

  create(model: RoleUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/roles', model);
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
