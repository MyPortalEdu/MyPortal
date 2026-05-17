import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LocalAuthoritySummaryResponse } from '../types/school';
import { PageResult } from '../types/bulletin';
import { QueryKitParams } from '../utils/primeng-querykit';

@Injectable({ providedIn: 'root' })
export class LocalAuthoritiesDataService {
  private readonly http = inject(HttpClient);

  list(params: QueryKitParams): Observable<PageResult<LocalAuthoritySummaryResponse>> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));
    if (params.filter) httpParams = httpParams.set('filter', params.filter);
    if (params.sort) httpParams = httpParams.set('sort', params.sort);

    return this.http.get<PageResult<LocalAuthoritySummaryResponse>>('/api/localauthorities', {
      params: httpParams,
    });
  }
}
