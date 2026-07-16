import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import {
  BulletinCategoryResponse,
  BulletinCategoryUpsertRequest,
  BulletinDetailsResponse,
  BulletinPinRequest,
  BulletinSettingsResponse,
  BulletinSettingsUpdateRequest,
  BulletinSummaryResponse,
  BulletinUpsertRequest,
  IdResponse,
  PageResult,
} from '../types/bulletin';

@Injectable({ providedIn: 'root' })
export class BulletinsDataService {
  private readonly http = inject(HttpClient);

  // Cached active-categories observable so the feed widget and the form dialog
  // share a single round-trip. Invalidated on any category mutation.
  private activeCategories$?: Observable<BulletinCategoryResponse[]>;

  list(page = 1, pageSize = 25): Observable<PageResult<BulletinSummaryResponse>> {
    const params = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));
    return this.http.get<PageResult<BulletinSummaryResponse>>('/api/v1/bulletins', { params });
  }

  getById(bulletinId: string): Observable<BulletinDetailsResponse> {
    return this.http.get<BulletinDetailsResponse>(`/api/v1/bulletins/${bulletinId}`);
  }

  create(model: BulletinUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/bulletins', model);
  }

  update(bulletinId: string, model: BulletinUpsertRequest): Observable<void> {
    return this.http.put<void>(`/api/v1/bulletins/${bulletinId}`, model);
  }

  pin(bulletinId: string, model: BulletinPinRequest): Observable<void> {
    return this.http.put<void>(`/api/v1/bulletins/${bulletinId}/pin`, model);
  }

  acknowledge(bulletinId: string): Observable<void> {
    return this.http.post<void>(`/api/v1/bulletins/${bulletinId}/acknowledge`, {});
  }

  delete(bulletinId: string): Observable<void> {
    return this.http.delete<void>(`/api/v1/bulletins/${bulletinId}`);
  }

  listCategories(includeInactive = false): Observable<BulletinCategoryResponse[]> {
    // Active-only is cached because every form-open and feed-refresh wants the
    // same set. The inactive variant skips the cache — it's for management
    // screens where you want fresh data after edits.
    if (includeInactive) {
      const params = new HttpParams().set('includeInactive', 'true');
      return this.http.get<BulletinCategoryResponse[]>('/api/v1/bulletincategories', { params });
    }
    if (!this.activeCategories$) {
      this.activeCategories$ = this.http
        .get<BulletinCategoryResponse[]>('/api/v1/bulletincategories')
        .pipe(shareReplay(1));
    }
    return this.activeCategories$;
  }

  createCategory(model: BulletinCategoryUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/v1/bulletincategories', model)
      .pipe(tap(() => this.invalidateCategoryCache()));
  }

  updateCategory(categoryId: string, model: BulletinCategoryUpsertRequest): Observable<void> {
    return this.http.put<void>(`/api/v1/bulletincategories/${categoryId}`, model)
      .pipe(tap(() => this.invalidateCategoryCache()));
  }

  deleteCategory(categoryId: string): Observable<void> {
    return this.http.delete<void>(`/api/v1/bulletincategories/${categoryId}`)
      .pipe(tap(() => this.invalidateCategoryCache()));
  }

  invalidateCategoryCache(): void {
    this.activeCategories$ = undefined;
  }

  getSettings(): Observable<BulletinSettingsResponse> {
    return this.http.get<BulletinSettingsResponse>('/api/v1/bulletins/settings');
  }

  updateSettings(model: BulletinSettingsUpdateRequest): Observable<void> {
    return this.http.put<void>('/api/v1/bulletins/settings', model);
  }
}
