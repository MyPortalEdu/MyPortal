import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PageResult } from '../types/bulletin';
import { StudentGroupSummaryResponse } from '../types/student-group';
import { QueryKitParams } from '../utils/primeng-querykit';

@Injectable({ providedIn: 'root' })
export class StudentGroupsDataService {
  private readonly http = inject(HttpClient);

  /**
   * Cross-subtype paged student-group listing. `params` is whatever
   * `toQueryKitParams(event)` returned from a PrimeNG table lazy-load —
   * including the optional pre-encoded base64url filter/sort strings.
   * `academicYearId` is required: student groups are scoped per AY.
   */
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
