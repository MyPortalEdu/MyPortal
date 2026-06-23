import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IdResponse, PageResult } from '../types/bulletin';
import { StaffMemberSummaryResponse } from '../types/staff-member';
import { StaffMemberHeaderResponse } from '../types/staff-member-header';
import {
  StaffBasicDetailsResponse,
  StaffBasicDetailsUpsertRequest,
} from '../types/staff-basic-details';
import {
  PersonMatchResponse,
  StaffMemberCreateForPersonRequest,
} from '../types/person-match';
import { QueryKitParams } from '../utils/primeng-querykit';

@Injectable({ providedIn: 'root' })
export class StaffMembersDataService {
  private readonly http = inject(HttpClient);

  list(params: QueryKitParams): Observable<PageResult<StaffMemberSummaryResponse>> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));
    if (params.filter) httpParams = httpParams.set('filter', params.filter);
    if (params.sort) httpParams = httpParams.set('sort', params.sort);

    return this.http.get<PageResult<StaffMemberSummaryResponse>>('/api/v1/people/staff', {
      params: httpParams,
    });
  }

  getHeader(staffMemberId: string): Observable<StaffMemberHeaderResponse> {
    return this.http.get<StaffMemberHeaderResponse>(`/api/v1/staffmembers/${staffMemberId}`);
  }

  getBasicDetails(staffMemberId: string): Observable<StaffBasicDetailsResponse> {
    return this.http.get<StaffBasicDetailsResponse>(
      `/api/v1/staffmembers/${staffMemberId}/basic-details`,
    );
  }

  updateBasicDetails(
    staffMemberId: string,
    payload: StaffBasicDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/basic-details`,
      payload,
    );
  }

  // Create takes the same basic-details shape as the update — joiner record
  // carries identity + Code only; equality / employment / professional / etc.
  // are populated post-creation via their area PUTs.
  create(payload: StaffBasicDetailsUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/staffmembers', payload);
  }

  // Search existing People for the create flow so a joiner already on file gets a
  // staff role attached to their existing Person rather than a duplicate. Blank /
  // too-short queries return [] server-side.
  searchPeople(query: string): Observable<PersonMatchResponse[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<PersonMatchResponse[]>('/api/v1/staffmembers/person-matches', {
      params,
    });
  }

  // Attach a staff role to an existing Person — supplies only the staff code; the
  // person's bio is left untouched.
  createForPerson(payload: StaffMemberCreateForPersonRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/staffmembers/for-person', payload);
  }

  delete(staffMemberId: string): Observable<void> {
    return this.http.delete<void>(`/api/v1/staffmembers/${staffMemberId}`);
  }
}
