import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IdResponse, PageResult } from '../types/bulletin';
import { StudentSummaryResponse } from '../types/student-summary';
import { StudentHeaderResponse } from '../types/student-header';
import {
  StudentBasicDetailsResponse,
  StudentBasicDetailsUpsertRequest,
} from '../types/student-basic-details';
import { StudentMatchResponse, StudentCreateForPersonRequest } from '../types/student-match';
import {
  GeneratedUpnResponse,
  StudentRegistrationDetailsResponse,
  StudentRegistrationDetailsUpsertRequest,
} from '../types/student-registration';
import { QueryKitParams } from '../utils/querykit';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class StudentsDataService {
  private readonly http = inject(HttpClient);

  list(params: QueryKitParams): Observable<PageResult<StudentSummaryResponse>> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));
    if (params.filter) httpParams = httpParams.set('filter', params.filter);
    if (params.sort) httpParams = httpParams.set('sort', params.sort);

    return this.http.get<PageResult<StudentSummaryResponse>>('/api/v1/students', {
      params: httpParams,
    });
  }

  getHeader(studentId: string): Observable<StudentHeaderResponse> {
    return this.http.get<StudentHeaderResponse>(`/api/v1/students/${studentId}`);
  }

  getBasicDetails(studentId: string): Observable<StudentBasicDetailsResponse> {
    return this.http.get<StudentBasicDetailsResponse>(
      `/api/v1/students/${studentId}/basic-details`,
    );
  }

  updateBasicDetails(
    studentId: string,
    payload: StudentBasicDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/basic-details`, payload);
  }

  getRegistrationDetails(studentId: string): Observable<StudentRegistrationDetailsResponse> {
    return this.http.get<StudentRegistrationDetailsResponse>(
      `/api/v1/students/${studentId}/registration`,
    );
  }

  updateRegistrationDetails(
    studentId: string,
    payload: StudentRegistrationDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/registration`, payload);
  }

  generateUpn(): Observable<GeneratedUpnResponse> {
    return this.http.post<GeneratedUpnResponse>('/api/v1/students/generate-upn', {});
  }

  photoUrl(studentId: string, photoId: string): string {
    return `${environment.apiUrl}/v1/students/${studentId}/photo?v=${photoId}`;
  }

  uploadPhoto(studentId: string, file: File): Observable<IdResponse> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/photo`, form);
  }

  deletePhoto(studentId: string): Observable<IdResponse> {
    return this.http.delete<IdResponse>(`/api/v1/students/${studentId}/photo`);
  }

  create(payload: StudentBasicDetailsUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/students', payload);
  }

  searchPeople(query: string): Observable<StudentMatchResponse[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<StudentMatchResponse[]>('/api/v1/students/person-matches', {
      params,
    });
  }

  createForPerson(payload: StudentCreateForPersonRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/students/for-person', payload);
  }

  delete(studentId: string): Observable<void> {
    return this.http.delete<void>(`/api/v1/students/${studentId}`);
  }
}
