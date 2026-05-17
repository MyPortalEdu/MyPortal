import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PageResult } from '../types/bulletin';
import { StaffMemberSummaryResponse } from '../types/staff-member';
import { QueryKitParams } from '../utils/primeng-querykit';

@Injectable({ providedIn: 'root' })
export class StaffMembersDataService {
  private readonly http = inject(HttpClient);

  list(params: QueryKitParams): Observable<PageResult<StaffMemberSummaryResponse>> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));
    if (params.filter) httpParams = httpParams.set('filter', params.filter);
    if (params.sort) httpParams = httpParams.set('sort', params.sort);

    return this.http.get<PageResult<StaffMemberSummaryResponse>>('/api/v1/people/staff', {
      params: httpParams,
    });
  }
}
