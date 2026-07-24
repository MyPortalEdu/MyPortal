import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StaffComplianceDashboard } from '../types/staff-compliance';

@Injectable({ providedIn: 'root' })
export class StaffComplianceDataService {
  private readonly http = inject(HttpClient);

  getDashboard(horizonDays: number): Observable<StaffComplianceDashboard> {
    const params = new HttpParams().set('horizonDays', horizonDays);
    return this.http.get<StaffComplianceDashboard>('/api/v1/staffcompliance', { params });
  }
}
