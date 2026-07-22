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
import {
  PersonContactDetailsResponse,
  PersonContactDetailsUpsertRequest,
} from '../types/person-contact-details';
import {
  AddressListResponse,
  AddressMatchResponse,
  PersonAddressUpdateRequest,
  PersonAddressUpsertRequest,
} from '../types/staff-address';
import {
  StudentFamilyResponse,
  StudentContactRelationshipUpsertRequest,
} from '../types/student-family';
import {
  StudentCulturalDetailsResponse,
  StudentCulturalDetailsUpsertRequest,
} from '../types/student-cultural';
import {
  StudentMedicalDetailsResponse,
  StudentMedicalDetailsUpsertRequest,
} from '../types/student-medical';
import {
  StudentSenDetailsResponse,
  SetSenStatusRequest,
  SenNeedUpsertRequest,
  SenProvisionUpsertRequest,
  SenStatementUpsertRequest,
} from '../types/student-sen';
import {
  StudentWelfareDetailsResponse,
  WelfareIndicatorsUpsertRequest,
  CareEpisodeUpsertRequest,
  PepUpsertRequest,
  ChildProtectionPlanUpsertRequest,
} from '../types/student-welfare';
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

  getContactDetails(studentId: string): Observable<PersonContactDetailsResponse> {
    return this.http.get<PersonContactDetailsResponse>(
      `/api/v1/students/${studentId}/contact-details`,
    );
  }

  updateContactDetails(
    studentId: string,
    payload: PersonContactDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/contact-details`, payload);
  }

  getAddresses(studentId: string): Observable<AddressListResponse> {
    return this.http.get<AddressListResponse>(`/api/v1/students/${studentId}/addresses`);
  }

  searchAddressMatches(studentId: string, query: string): Observable<AddressMatchResponse[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<AddressMatchResponse[]>(
      `/api/v1/students/${studentId}/address-matches`,
      { params },
    );
  }

  addAddress(studentId: string, payload: PersonAddressUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>(`/api/v1/students/${studentId}/addresses`, payload);
  }

  updateAddress(
    studentId: string,
    addressPersonId: string,
    payload: PersonAddressUpdateRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/students/${studentId}/addresses/${addressPersonId}`,
      payload,
    );
  }

  removeAddress(studentId: string, addressPersonId: string): Observable<void> {
    return this.http.delete<void>(
      `/api/v1/students/${studentId}/addresses/${addressPersonId}`,
    );
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

  getCulturalDetails(studentId: string): Observable<StudentCulturalDetailsResponse> {
    return this.http.get<StudentCulturalDetailsResponse>(`/api/v1/students/${studentId}/cultural`);
  }

  updateCulturalDetails(
    studentId: string,
    payload: StudentCulturalDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/cultural`, payload);
  }

  getMedicalDetails(studentId: string): Observable<StudentMedicalDetailsResponse> {
    return this.http.get<StudentMedicalDetailsResponse>(`/api/v1/students/${studentId}/medical`);
  }

  updateMedicalDetails(
    studentId: string,
    payload: StudentMedicalDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/medical`, payload);
  }

  getSen(studentId: string): Observable<StudentSenDetailsResponse> {
    return this.http.get<StudentSenDetailsResponse>(`/api/v1/students/${studentId}/sen`);
  }

  setSenStatus(studentId: string, payload: SetSenStatusRequest): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/sen/status`, payload);
  }

  undoLatestSenStatus(studentId: string): Observable<IdResponse> {
    return this.http.delete<IdResponse>(`/api/v1/students/${studentId}/sen/status/current`);
  }

  updateSenNeeds(studentId: string, payload: SenNeedUpsertRequest[]): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/sen/needs`, payload);
  }

  updateSenProvisions(
    studentId: string,
    payload: SenProvisionUpsertRequest[],
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/sen/provisions`, payload);
  }

  updateSenStatements(
    studentId: string,
    payload: SenStatementUpsertRequest[],
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/sen/statements`, payload);
  }

  getWelfare(studentId: string): Observable<StudentWelfareDetailsResponse> {
    return this.http.get<StudentWelfareDetailsResponse>(`/api/v1/students/${studentId}/welfare`);
  }

  updateWelfareIndicators(
    studentId: string,
    payload: WelfareIndicatorsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/welfare/indicators`, payload);
  }

  updateWelfareCareEpisodes(
    studentId: string,
    payload: CareEpisodeUpsertRequest[],
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/welfare/care-episodes`, payload);
  }

  updateWelfarePeps(studentId: string, payload: PepUpsertRequest[]): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/students/${studentId}/welfare/peps`, payload);
  }

  updateWelfareChildProtectionPlans(
    studentId: string,
    payload: ChildProtectionPlanUpsertRequest[],
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/students/${studentId}/welfare/child-protection-plans`,
      payload,
    );
  }

  getFamily(studentId: string): Observable<StudentFamilyResponse> {
    return this.http.get<StudentFamilyResponse>(`/api/v1/students/${studentId}/family`);
  }

  addContactRelationship(
    studentId: string,
    payload: StudentContactRelationshipUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.post<IdResponse>(`/api/v1/students/${studentId}/family`, payload);
  }

  updateContactRelationship(
    studentId: string,
    relationshipId: string,
    payload: StudentContactRelationshipUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/students/${studentId}/family/${relationshipId}`,
      payload,
    );
  }

  removeContactRelationship(studentId: string, relationshipId: string): Observable<void> {
    return this.http.delete<void>(`/api/v1/students/${studentId}/family/${relationshipId}`);
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
