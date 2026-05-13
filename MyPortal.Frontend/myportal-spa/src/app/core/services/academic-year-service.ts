import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay } from 'rxjs';
import { AcademicYearSummary } from '../types/academic-year-summary';

@Injectable({ providedIn: 'root' })
export class AcademicYearService {
  private readonly http = inject(HttpClient);
  private current$?: Observable<AcademicYearSummary | null>;

  // The API returns 204 when no academic year covers today's date; HttpClient
  // surfaces that as a null body, so callers always get a nullable summary.
  getCurrent(): Observable<AcademicYearSummary | null> {
    if (!this.current$) {
      this.current$ = this.http
        .get<AcademicYearSummary | null>('/api/academicyears/current')
        .pipe(shareReplay(1));
    }
    return this.current$;
  }

  clearCache(): void {
    this.current$ = undefined;
  }
}
