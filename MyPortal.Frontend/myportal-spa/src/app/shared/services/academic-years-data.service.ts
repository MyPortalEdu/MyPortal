import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AcademicYearSummary } from '../../core/types/academic-year-summary';
import { AcademicYearUpsertRequest } from '../types/academic-year';
import { AcademicYearDetailsResponse } from '../types/academic-year-details';
import { IdResponse } from '../types/bulletin';

@Injectable({ providedIn: 'root' })
export class AcademicYearsDataService {
  private readonly http = inject(HttpClient);

  list(): Observable<AcademicYearSummary[]> {
    return this.http.get<AcademicYearSummary[]>('/api/academicyears');
  }

  getById(id: string): Observable<AcademicYearDetailsResponse> {
    return this.http.get<AcademicYearDetailsResponse>(`/api/academicyears/${id}`);
  }

  create(model: AcademicYearUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/academicyears', this.toWire(model));
  }

  update(id: string, model: AcademicYearUpsertRequest): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/academicyears/${id}`, this.toWire(model));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/academicyears/${id}`);
  }

  // Translate the wizard's Date-typed form model into the server's wire format.
  // Dates go out as local-component "YYYY-MM-DDT00:00:00" strings (no Z) so that
  // a date picked in BST doesn't shift back a day when the browser would
  // otherwise serialise via Date.toJSON()'s UTC conversion. The server's
  // DateTime parser reads these as local-tz unspecified, preserving the date.
  // Period times go out as "HH:mm:ss" to match the server's TimeOnly type.
  private toWire(model: AcademicYearUpsertRequest): unknown {
    return {
      ...model,
      academicTerms: model.academicTerms.map(t => ({
        ...t,
        startDate: formatLocalDate(t.startDate),
        endDate: formatLocalDate(t.endDate),
      })),
      attendancePeriods: model.attendancePeriods.map(p => ({
        ...p,
        startTime: formatLocalTime(p.startTime),
        endTime: formatLocalTime(p.endTime),
      })),
      schoolHolidays: model.schoolHolidays.map(h => ({
        ...h,
        startDate: formatLocalDate(h.startDate),
        endDate: formatLocalDate(h.endDate),
      })),
    };
  }
}

function formatLocalDate(d: Date | null): string | null {
  if (!d) return null;
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}T00:00:00`;
}

function formatLocalTime(d: Date | null): string | null {
  if (!d) return null;
  const h = String(d.getHours()).padStart(2, '0');
  const m = String(d.getMinutes()).padStart(2, '0');
  return `${h}:${m}:00`;
}
