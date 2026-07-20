import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IdResponse } from '../types/bulletin';
import { SchoolDetailsResponse, SchoolUpsertRequest } from '../types/school';

@Injectable({ providedIn: 'root' })
export class SchoolsDataService {
  private readonly http = inject(HttpClient);

  getLocalDetails(): Observable<SchoolDetailsResponse | null> {
    return this.http.get<SchoolDetailsResponse | null>('/api/v1/schools/local/details', {
      observe: 'body',
    });
  }

  saveLocalDetails(model: SchoolUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/schools/local/details', model);
  }
}
