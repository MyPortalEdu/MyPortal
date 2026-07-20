import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PageResult } from '../types/bulletin';
import { StudentGroupSummaryResponse } from '../types/student-group';
import { QueryKitParams } from '../utils/querykit';

@Injectable({ providedIn: 'root' })
export class StudentGroupsDataService {
  private readonly http = inject(HttpClient);

  list(academicYearId: string, params: QueryKitParams): Observable<PageResult<StudentGroupSummaryResponse>> {
    let httpParams = new HttpParams()
      .set('academicYearId', academicYearId)
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));
    if (params.filter) httpParams = httpParams.set('filter', params.filter);
    if (params.sort) httpParams = httpParams.set('sort', params.sort);

    return this.http.get<PageResult<StudentGroupSummaryResponse>>('/api/v1/studentgroups', { params: httpParams });
  }
}
