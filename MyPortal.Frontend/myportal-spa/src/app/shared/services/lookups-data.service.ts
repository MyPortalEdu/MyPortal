import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay } from 'rxjs';
import { LookupResponse } from '../types/lookup';

/**
 * Reference-data fetches for the dropdowns on the school details page (and
 * any future page that needs the same catalogues). Each list is small,
 * mostly-static, and re-used across edit screens — so we shareReplay once
 * per page-lifetime rather than re-hitting the API on every dialog open.
 */
@Injectable({ providedIn: 'root' })
export class LookupsDataService {
  private readonly http = inject(HttpClient);

  private agencyTypes$?: Observable<LookupResponse[]>;
  private governanceTypes$?: Observable<LookupResponse[]>;
  private intakeTypes$?: Observable<LookupResponse[]>;
  private schoolPhases$?: Observable<LookupResponse[]>;
  private schoolTypes$?: Observable<LookupResponse[]>;

  agencyTypes(): Observable<LookupResponse[]> {
    return this.agencyTypes$ ??= this.http
      .get<LookupResponse[]>('/api/agencytypes')
      .pipe(shareReplay(1));
  }

  governanceTypes(): Observable<LookupResponse[]> {
    return this.governanceTypes$ ??= this.http
      .get<LookupResponse[]>('/api/governancetypes')
      .pipe(shareReplay(1));
  }

  intakeTypes(): Observable<LookupResponse[]> {
    return this.intakeTypes$ ??= this.http
      .get<LookupResponse[]>('/api/intaketypes')
      .pipe(shareReplay(1));
  }

  schoolPhases(): Observable<LookupResponse[]> {
    return this.schoolPhases$ ??= this.http
      .get<LookupResponse[]>('/api/schoolphases')
      .pipe(shareReplay(1));
  }

  schoolTypes(): Observable<LookupResponse[]> {
    return this.schoolTypes$ ??= this.http
      .get<LookupResponse[]>('/api/schooltypes')
      .pipe(shareReplay(1));
  }

  invalidate(): void {
    this.agencyTypes$ = undefined;
    this.governanceTypes$ = undefined;
    this.intakeTypes$ = undefined;
    this.schoolPhases$ = undefined;
    this.schoolTypes$ = undefined;
  }
}
