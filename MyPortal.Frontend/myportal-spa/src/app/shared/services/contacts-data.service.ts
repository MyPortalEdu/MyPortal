import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IdResponse, PageResult } from '../types/bulletin';
import { ContactSummaryResponse } from '../types/contact-summary';
import { ContactHeaderResponse } from '../types/contact-header';
import {
  ContactBasicDetailsResponse,
  ContactBasicDetailsUpsertRequest,
} from '../types/contact-basic-details';
import { ContactMatchResponse, ContactCreateForPersonRequest } from '../types/contact-match';
import { ContactStudentResponse } from '../types/contact-student';
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
import { QueryKitParams } from '../utils/querykit';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ContactsDataService {
  private readonly http = inject(HttpClient);

  list(params: QueryKitParams): Observable<PageResult<ContactSummaryResponse>> {
    let httpParams = new HttpParams()
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));
    if (params.filter) httpParams = httpParams.set('filter', params.filter);
    if (params.sort) httpParams = httpParams.set('sort', params.sort);

    return this.http.get<PageResult<ContactSummaryResponse>>('/api/v1/contacts', {
      params: httpParams,
    });
  }

  getHeader(contactId: string): Observable<ContactHeaderResponse> {
    return this.http.get<ContactHeaderResponse>(`/api/v1/contacts/${contactId}`);
  }

  getBasicDetails(contactId: string): Observable<ContactBasicDetailsResponse> {
    return this.http.get<ContactBasicDetailsResponse>(
      `/api/v1/contacts/${contactId}/basic-details`,
    );
  }

  updateBasicDetails(
    contactId: string,
    payload: ContactBasicDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/contacts/${contactId}/basic-details`, payload);
  }

  photoUrl(contactId: string, photoId: string): string {
    return `${environment.apiUrl}/v1/contacts/${contactId}/photo?v=${photoId}`;
  }

  uploadPhoto(contactId: string, file: File): Observable<IdResponse> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.put<IdResponse>(`/api/v1/contacts/${contactId}/photo`, form);
  }

  deletePhoto(contactId: string): Observable<IdResponse> {
    return this.http.delete<IdResponse>(`/api/v1/contacts/${contactId}/photo`);
  }

  getContactDetails(contactId: string): Observable<PersonContactDetailsResponse> {
    return this.http.get<PersonContactDetailsResponse>(
      `/api/v1/contacts/${contactId}/contact-details`,
    );
  }

  updateContactDetails(
    contactId: string,
    payload: PersonContactDetailsUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/contacts/${contactId}/contact-details`, payload);
  }

  getAddresses(contactId: string): Observable<AddressListResponse> {
    return this.http.get<AddressListResponse>(`/api/v1/contacts/${contactId}/addresses`);
  }

  searchAddressMatches(contactId: string, query: string): Observable<AddressMatchResponse[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<AddressMatchResponse[]>(
      `/api/v1/contacts/${contactId}/address-matches`,
      { params },
    );
  }

  addAddress(contactId: string, payload: PersonAddressUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>(`/api/v1/contacts/${contactId}/addresses`, payload);
  }

  updateAddress(
    contactId: string,
    addressPersonId: string,
    payload: PersonAddressUpdateRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(
      `/api/v1/contacts/${contactId}/addresses/${addressPersonId}`,
      payload,
    );
  }

  removeAddress(contactId: string, addressPersonId: string): Observable<void> {
    return this.http.delete<void>(
      `/api/v1/contacts/${contactId}/addresses/${addressPersonId}`,
    );
  }

  getAssociatedStudents(contactId: string): Observable<ContactStudentResponse[]> {
    return this.http.get<ContactStudentResponse[]>(`/api/v1/contacts/${contactId}/students`);
  }

  create(payload: ContactBasicDetailsUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/contacts', payload);
  }

  searchPeople(query: string): Observable<ContactMatchResponse[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<ContactMatchResponse[]>('/api/v1/contacts/person-matches', {
      params,
    });
  }

  createForPerson(payload: ContactCreateForPersonRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/contacts/for-person', payload);
  }

  delete(contactId: string): Observable<void> {
    return this.http.delete<void>(`/api/v1/contacts/${contactId}`);
  }
}
