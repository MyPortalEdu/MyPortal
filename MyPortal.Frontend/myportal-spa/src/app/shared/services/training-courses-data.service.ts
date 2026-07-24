import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IdResponse } from '../types/bulletin';
import { TrainingCourse, TrainingCourseUpsert } from '../types/training-course';

@Injectable({ providedIn: 'root' })
export class TrainingCoursesDataService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/v1/trainingcourses';

  list(): Observable<TrainingCourse[]> {
    return this.http.get<TrainingCourse[]>(this.base);
  }

  create(model: TrainingCourseUpsert): Observable<IdResponse> {
    return this.http.post<IdResponse>(this.base, model);
  }

  update(id: string, model: TrainingCourseUpsert): Observable<IdResponse> {
    return this.http.put<IdResponse>(`${this.base}/${id}`, model);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
