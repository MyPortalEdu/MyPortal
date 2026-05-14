import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IdResponse } from '../types/bulletin';
import { SchoolDetailsResponse, SchoolUpsertRequest } from '../types/school';

@Injectable({ providedIn: 'root' })
export class SchoolsDataService {
  private readonly http = inject(HttpClient);

  /**
   * Get the full local-school details payload for the school details page.
   * Returns null when no school has been configured yet (the API replies 204).
   */
  getLocalDetails(): Observable<SchoolDetailsResponse | null> {
    return this.http.get<SchoolDetailsResponse | null>('/api/schools/local/details', {
      observe: 'body',
    });
  }

  saveLocalDetails(model: SchoolUpsertRequest): Observable<IdResponse> {
    // The backend returns a Guid (the school id) — wrap as IdResponse for
    // FE ergonomics. JSON-encoded Guid is a bare quoted string, but ASP.NET's
    // Ok(value) wraps to the body verbatim and Angular will parse it as JSON.
    return this.http.post<IdResponse>('/api/schools/local/details', model);
  }
}
