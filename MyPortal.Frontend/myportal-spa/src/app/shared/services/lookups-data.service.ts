import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay } from 'rxjs';
import { LookupResponse } from '../types/lookup';

@Injectable({ providedIn: 'root' })
export class LookupsDataService {
  private readonly http = inject(HttpClient);

  private agencyTypes$?: Observable<LookupResponse[]>;
  private governanceTypes$?: Observable<LookupResponse[]>;
  private intakeTypes$?: Observable<LookupResponse[]>;
  private schoolPhases$?: Observable<LookupResponse[]>;
  private schoolTypes$?: Observable<LookupResponse[]>;
  private payZones$?: Observable<LookupResponse[]>;
  private specialSchoolOrganisations$?: Observable<LookupResponse[]>;
  private specialSchoolTypes$?: Observable<LookupResponse[]>;

  agencyTypes(): Observable<LookupResponse[]> {
    return this.agencyTypes$ ??= this.http
      .get<LookupResponse[]>('/api/v1/agencytypes')
      .pipe(shareReplay(1));
  }

  governanceTypes(): Observable<LookupResponse[]> {
    return this.governanceTypes$ ??= this.http
      .get<LookupResponse[]>('/api/v1/governancetypes')
      .pipe(shareReplay(1));
  }

  intakeTypes(): Observable<LookupResponse[]> {
    return this.intakeTypes$ ??= this.http
      .get<LookupResponse[]>('/api/v1/intaketypes')
      .pipe(shareReplay(1));
  }

  schoolPhases(): Observable<LookupResponse[]> {
    return this.schoolPhases$ ??= this.http
      .get<LookupResponse[]>('/api/v1/schoolphases')
      .pipe(shareReplay(1));
  }

  schoolTypes(): Observable<LookupResponse[]> {
    return this.schoolTypes$ ??= this.http
      .get<LookupResponse[]>('/api/v1/schooltypes')
      .pipe(shareReplay(1));
  }

  payZones(): Observable<LookupResponse[]> {
    return this.payZones$ ??= this.http
      .get<LookupResponse[]>('/api/v1/payzones')
      .pipe(shareReplay(1));
  }

  specialSchoolOrganisations(): Observable<LookupResponse[]> {
    return this.specialSchoolOrganisations$ ??= this.http
      .get<LookupResponse[]>('/api/v1/specialschoolorganisations')
      .pipe(shareReplay(1));
  }

  specialSchoolTypes(): Observable<LookupResponse[]> {
    return this.specialSchoolTypes$ ??= this.http
      .get<LookupResponse[]>('/api/v1/specialschooltypes')
      .pipe(shareReplay(1));
  }

  invalidate(): void {
    this.agencyTypes$ = undefined;
    this.governanceTypes$ = undefined;
    this.intakeTypes$ = undefined;
    this.schoolPhases$ = undefined;
    this.schoolTypes$ = undefined;
    this.payZones$ = undefined;
    this.specialSchoolOrganisations$ = undefined;
    this.specialSchoolTypes$ = undefined;
  }
}
