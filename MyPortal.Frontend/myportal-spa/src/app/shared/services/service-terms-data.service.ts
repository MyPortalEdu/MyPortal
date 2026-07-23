import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IdResponse } from '../types/bulletin';
import {
  IncrementApplyRequest,
  IncrementPreviewRequest,
  IncrementPreviewResponse,
  IncrementScheduleRequest,
  ScheduledIncrement,
  PayAwardPreviewResponse,
  PayAwardRequest,
  ServiceTermPayResponse,
  ServiceTermPayUpsertRequest,
  ServiceTermUpsertRequest,
  ServiceTermsResponse,
} from '../types/staff-setup';

@Injectable({ providedIn: 'root' })
export class ServiceTermsDataService {
  private readonly http = inject(HttpClient);

  getServiceTerms(): Observable<ServiceTermsResponse> {
    return this.http.get<ServiceTermsResponse>('/api/v1/serviceterms');
  }

  create(payload: ServiceTermUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/serviceterms', payload);
  }

  update(serviceTermId: string, payload: ServiceTermUpsertRequest): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/serviceterms/${serviceTermId}`, payload);
  }

  delete(serviceTermId: string): Observable<IdResponse> {
    return this.http.delete<IdResponse>(`/api/v1/serviceterms/${serviceTermId}`);
  }

  getPay(serviceTermId: string, effectiveFrom?: string | null): Observable<ServiceTermPayResponse> {
    const params = effectiveFrom ? new HttpParams().set('effectiveFrom', effectiveFrom) : undefined;
    return this.http.get<ServiceTermPayResponse>(`/api/v1/serviceterms/${serviceTermId}/pay`, {
      params,
    });
  }

  updatePay(
    serviceTermId: string,
    payload: ServiceTermPayUpsertRequest,
  ): Observable<IdResponse> {
    return this.http.put<IdResponse>(`/api/v1/serviceterms/${serviceTermId}/pay`, payload);
  }

  previewAward(serviceTermId: string, payload: PayAwardRequest): Observable<PayAwardPreviewResponse> {
    return this.http.post<PayAwardPreviewResponse>(
      `/api/v1/serviceterms/${serviceTermId}/awards/preview`,
      payload,
    );
  }

  applyAward(serviceTermId: string, payload: PayAwardRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>(`/api/v1/serviceterms/${serviceTermId}/awards`, payload);
  }

  deletePayScale(payScaleId: string): Observable<IdResponse> {
    return this.http.delete<IdResponse>(`/api/v1/serviceterms/payscales/${payScaleId}`);
  }

  previewIncrement(
    serviceTermId: string,
    payload: IncrementPreviewRequest,
  ): Observable<IncrementPreviewResponse> {
    return this.http.post<IncrementPreviewResponse>(
      `/api/v1/serviceterms/${serviceTermId}/increment/preview`,
      payload,
    );
  }

  applyIncrement(serviceTermId: string, payload: IncrementApplyRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>(`/api/v1/serviceterms/${serviceTermId}/increment`, payload);
  }

  scheduleIncrement(
    serviceTermId: string,
    payload: IncrementScheduleRequest,
  ): Observable<IdResponse> {
    return this.http.post<IdResponse>(
      `/api/v1/serviceterms/${serviceTermId}/increment/schedule`,
      payload,
    );
  }

  getScheduledIncrements(serviceTermId: string): Observable<ScheduledIncrement[]> {
    return this.http.get<ScheduledIncrement[]>(
      `/api/v1/serviceterms/${serviceTermId}/increment/scheduled`,
    );
  }

  getDueIncrements(): Observable<ScheduledIncrement[]> {
    return this.http.get<ScheduledIncrement[]>('/api/v1/serviceterms/increment/due');
  }

  applyScheduledIncrement(scheduledId: string): Observable<IdResponse> {
    return this.http.post<IdResponse>(
      `/api/v1/serviceterms/increment/scheduled/${scheduledId}/apply`,
      {},
    );
  }

  cancelScheduledIncrement(scheduledId: string): Observable<IdResponse> {
    return this.http.delete<IdResponse>(`/api/v1/serviceterms/increment/scheduled/${scheduledId}`);
  }
}
