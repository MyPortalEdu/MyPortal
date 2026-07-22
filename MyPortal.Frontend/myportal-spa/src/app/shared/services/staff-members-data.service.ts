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
  SetStaffLineManagerRequest,
  StaffManagementResponse,
} from '../types/staff-management';
import {
  StaffContactDetailsResponse,
  StaffContactDetailsUpsertRequest,
} from '../types/staff-contact-details';
import {
  StaffEqualityDetailsResponse,
  StaffEqualityDetailsUpsertRequest,
} from '../types/staff-equality-details';
import {
  StaffProfessionalDetailsResponse,
  StaffProfessionalDetailsUpsertRequest,
} from '../types/staff-professional-details';
import {
  StaffEmploymentDetailsResponse,
  StaffEmploymentDetailsUpsertRequest,
} from '../types/staff-employment-details';
import {
  StaffPreEmploymentChecksResponse,
  StaffPreEmploymentChecksUpsertRequest,
} from '../types/staff-pre-employment-checks';
import {
  StaffAbsencesResponse,
  StaffAbsencesUpsertRequest,
} from '../types/staff-absences';
import {
  StaffResponsibilitiesResponse,
  StaffResponsibilitiesUpsertRequest,
} from '../types/staff-responsibilities';
import {
  StaffNextOfKinAreaResponse,
  StaffNextOfKinAreaUpsertRequest,
} from '../types/staff-next-of-kin';
import { StaffCalendarResponse } from '../types/staff-timetable';
import {
  StaffPerformanceResponse,
  StaffPerformanceUpsertRequest,
} from '../types/staff-performance';
import {
  AddressListResponse,
  AddressMatchResponse,
  PersonAddressUpdateRequest,
  PersonAddressUpsertRequest,
} from '../types/staff-address';
import {
  PersonMatchResponse,
  StaffMemberCreateForPersonRequest,
} from '../types/person-match';
import { QueryKitParams } from '../utils/querykit';
import { environment } from '../../../environments/environment';

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

  getManagement(staffMemberId: string): Observable<StaffManagementResponse> {
    return this.http.get<StaffManagementResponse>(
      `/api/v1/staffmembers/${staffMemberId}/management`,
    );
  }

  setLineManager(
    staffMemberId: string,
    payload: SetStaffLineManagerRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/line-manager`,
      payload,
    );
  }

  photoUrl(staffMemberId: string, photoId: string): string {
    return `${environment.apiUrl}/v1/staffmembers/${staffMemberId}/photo?v=${photoId}`;
  }

  uploadPhoto(staffMemberId: string, file: File): Observable<IdResponse> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.put<IdResponse>(`/api/v1/staffmembers/${staffMemberId}/photo`, form);
  }

  deletePhoto(staffMemberId: string): Observable<IdResponse> {
    return this.http.delete<IdResponse>(`/api/v1/staffmembers/${staffMemberId}/photo`);
  }

  getContactDetails(staffMemberId: string): Observable<StaffContactDetailsResponse> {
    return this.http.get<StaffContactDetailsResponse>(
      `/api/v1/staffmembers/${staffMemberId}/contact-details`,
    );
  }

  updateContactDetails(
    staffMemberId: string,
    payload: StaffContactDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/contact-details`,
      payload,
    );
  }

  getEqualityDetails(staffMemberId: string): Observable<StaffEqualityDetailsResponse> {
    return this.http.get<StaffEqualityDetailsResponse>(
      `/api/v1/staffmembers/${staffMemberId}/equality-details`,
    );
  }

  updateEqualityDetails(
    staffMemberId: string,
    payload: StaffEqualityDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/equality-details`,
      payload,
    );
  }

  getProfessionalDetails(staffMemberId: string): Observable<StaffProfessionalDetailsResponse> {
    return this.http.get<StaffProfessionalDetailsResponse>(
      `/api/v1/staffmembers/${staffMemberId}/professional-details`,
    );
  }

  updateProfessionalDetails(
    staffMemberId: string,
    payload: StaffProfessionalDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/professional-details`,
      payload,
    );
  }

  getEmploymentDetails(staffMemberId: string): Observable<StaffEmploymentDetailsResponse> {
    return this.http.get<StaffEmploymentDetailsResponse>(
      `/api/v1/staffmembers/${staffMemberId}/employment-details`,
    );
  }

  updateEmploymentDetails(
    staffMemberId: string,
    payload: StaffEmploymentDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/employment-details`,
      payload,
    );
  }

  getPreEmploymentChecks(staffMemberId: string): Observable<StaffPreEmploymentChecksResponse> {
    return this.http.get<StaffPreEmploymentChecksResponse>(
      `/api/v1/staffmembers/${staffMemberId}/pre-employment-checks`,
    );
  }

  updatePreEmploymentChecks(
    staffMemberId: string,
    payload: StaffPreEmploymentChecksUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/pre-employment-checks`,
      payload,
    );
  }

  getNextOfKin(staffMemberId: string): Observable<StaffNextOfKinAreaResponse> {
    return this.http.get<StaffNextOfKinAreaResponse>(
      `/api/v1/staffmembers/${staffMemberId}/next-of-kin`,
    );
  }

  updateNextOfKin(
    staffMemberId: string,
    payload: StaffNextOfKinAreaUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/next-of-kin`,
      payload,
    );
  }

  getTimetable(
    staffMemberId: string,
    from: string,
    to: string,
  ): Observable<StaffCalendarResponse> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<StaffCalendarResponse>(
      `/api/v1/staffmembers/${staffMemberId}/timetable`,
      { params },
    );
  }

  getMyTimetable(from: string, to: string): Observable<StaffCalendarResponse> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<StaffCalendarResponse>('/api/v1/me/timetable', { params });
  }

  getPerformance(staffMemberId: string): Observable<StaffPerformanceResponse> {
    return this.http.get<StaffPerformanceResponse>(
      `/api/v1/staffmembers/${staffMemberId}/performance`,
    );
  }

  updatePerformance(
    staffMemberId: string,
    payload: StaffPerformanceUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/performance`,
      payload,
    );
  }

  getAbsences(staffMemberId: string): Observable<StaffAbsencesResponse> {
    return this.http.get<StaffAbsencesResponse>(
      `/api/v1/staffmembers/${staffMemberId}/absences`,
    );
  }

  updateAbsences(
    staffMemberId: string,
    payload: StaffAbsencesUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/absences`,
      payload,
    );
  }

  getResponsibilities(staffMemberId: string): Observable<StaffResponsibilitiesResponse> {
    return this.http.get<StaffResponsibilitiesResponse>(
      `/api/v1/staffmembers/${staffMemberId}/responsibilities`,
    );
  }

  updateResponsibilities(
    staffMemberId: string,
    payload: StaffResponsibilitiesUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/responsibilities`,
      payload,
    );
  }

  getAddresses(staffMemberId: string): Observable<AddressListResponse> {
    return this.http.get<AddressListResponse>(`/api/v1/staffmembers/${staffMemberId}/addresses`);
  }

  searchAddressMatches(staffMemberId: string, query: string): Observable<AddressMatchResponse[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<AddressMatchResponse[]>(
      `/api/v1/staffmembers/${staffMemberId}/address-matches`,
      { params },
    );
  }

  addAddress(staffMemberId: string, payload: PersonAddressUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>(`/api/v1/staffmembers/${staffMemberId}/addresses`, payload);
  }

  updateAddress(
    staffMemberId: string,
    addressPersonId: string,
    payload: PersonAddressUpdateRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/staffmembers/${staffMemberId}/addresses/${addressPersonId}`,
      payload,
    );
  }

  removeAddress(staffMemberId: string, addressPersonId: string): Observable<void> {
    return this.http.delete<void>(
      `/api/v1/staffmembers/${staffMemberId}/addresses/${addressPersonId}`,
    );
  }

  create(payload: StaffBasicDetailsUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/staffmembers', payload);
  }

  searchPeople(query: string): Observable<PersonMatchResponse[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<PersonMatchResponse[]>('/api/v1/staffmembers/person-matches', {
      params,
    });
  }

  createForPerson(payload: StaffMemberCreateForPersonRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/staffmembers/for-person', payload);
  }

  delete(staffMemberId: string): Observable<void> {
    return this.http.delete<void>(`/api/v1/staffmembers/${staffMemberId}`);
  }
}
