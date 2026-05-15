import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AcademicYearSummary } from '../../core/types/academic-year-summary';

@Injectable({ providedIn: 'root' })
export class AcademicYearsDataService {
  private readonly http = inject(HttpClient);

  list(): Observable<AcademicYearSummary[]> {
    return this.http.get<AcademicYearSummary[]>('/api/academicyears');
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/academicyears/${id}`);
  }
}
