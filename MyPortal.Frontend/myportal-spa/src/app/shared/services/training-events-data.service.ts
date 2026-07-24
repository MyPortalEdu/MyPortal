import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IdResponse } from '../types/bulletin';
import {
  TrainingEventDetails,
  TrainingEventSummary,
  TrainingEventUpsert,
} from '../types/training-events';

@Injectable({ providedIn: 'root' })
export class TrainingEventsDataService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/v1/trainingevents';

  list(): Observable<TrainingEventSummary[]> {
    return this.http.get<TrainingEventSummary[]>(this.base);
  }

  get(id: string): Observable<TrainingEventDetails> {
    return this.http.get<TrainingEventDetails>(`${this.base}/${id}`);
  }

  create(model: TrainingEventUpsert): Observable<IdResponse> {
    return this.http.post<IdResponse>(this.base, model);
  }

  update(id: string, model: TrainingEventUpsert): Observable<IdResponse> {
    return this.http.put<IdResponse>(`${this.base}/${id}`, model);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  bookAttendees(id: string, staffMemberIds: string[]): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/attendees`, { staffMemberIds });
  }

  removeAttendee(id: string, staffMemberId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}/attendees/${staffMemberId}`);
  }

  setAttendance(id: string, staffMemberId: string, attended: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/attendees/${staffMemberId}/attendance`, { attended });
  }
}
