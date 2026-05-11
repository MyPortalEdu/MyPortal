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
    return this.http.get<PageResult<BulletinSummaryResponse>>('/api/bulletins', { params });
  }

  getById(bulletinId: string): Observable<BulletinDetailsResponse> {
    return this.http.get<BulletinDetailsResponse>(`/api/bulletins/${bulletinId}`);
  }

  create(model: BulletinUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/bulletins', model);
  }

  update(bulletinId: string, model: BulletinUpsertRequest): Observable<void> {
    return this.http.put<void>(`/api/bulletins/${bulletinId}`, model);
  }

  pin(bulletinId: string, model: BulletinPinRequest): Observable<void> {
    return this.http.put<void>(`/api/bulletins/${bulletinId}/pin`, model);
  }

  acknowledge(bulletinId: string): Observable<void> {
    return this.http.post<void>(`/api/bulletins/${bulletinId}/acknowledge`, {});
  }

  delete(bulletinId: string): Observable<void> {
    return this.http.delete<void>(`/api/bulletins/${bulletinId}`);
  }

  // ─── Categories ──────────────────────────────────────────────────────────

  listCategories(includeInactive = false): Observable<BulletinCategoryResponse[]> {
    // Active-only is cached because every form-open and feed-refresh wants the
    // same set. The inactive variant skips the cache — it's for management
    // screens where you want fresh data after edits.
    if (includeInactive) {
      const params = new HttpParams().set('includeInactive', 'true');
      return this.http.get<BulletinCategoryResponse[]>('/api/bulletincategories', { params });
    }
    if (!this.activeCategories$) {
      this.activeCategories$ = this.http
        .get<BulletinCategoryResponse[]>('/api/bulletincategories')
        .pipe(shareReplay(1));
    }
    return this.activeCategories$;
  }

  createCategory(model: BulletinCategoryUpsertRequest): Observable<IdResponse> {
    return this.http.post<IdResponse>('/api/bulletincategories', model)
      .pipe(tap(() => this.invalidateCategoryCache()));
  }

  updateCategory(categoryId: string, model: BulletinCategoryUpsertRequest): Observable<void> {
    return this.http.put<void>(`/api/bulletincategories/${categoryId}`, model)
      .pipe(tap(() => this.invalidateCategoryCache()));
  }

  deleteCategory(categoryId: string): Observable<void> {
    return this.http.delete<void>(`/api/bulletincategories/${categoryId}`)
      .pipe(tap(() => this.invalidateCategoryCache()));
  }

  invalidateCategoryCache(): void {
    this.activeCategories$ = undefined;
  }

  // ─── Settings ────────────────────────────────────────────────────────────

  getSettings(): Observable<BulletinSettingsResponse> {
    return this.http.get<BulletinSettingsResponse>('/api/bulletins/settings');
  }

  updateSettings(model: BulletinSettingsUpdateRequest): Observable<void> {
    return this.http.put<void>('/api/bulletins/settings', model);
  }
}
