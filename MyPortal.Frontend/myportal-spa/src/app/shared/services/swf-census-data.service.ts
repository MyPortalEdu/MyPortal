import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SwfCensusPreview } from '../types/swf-census';

@Injectable({ providedIn: 'root' })
export class SwfCensusDataService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/v1/swfcensus';

  getPreview(referenceDate: string): Observable<SwfCensusPreview> {
    const params = new HttpParams().set('referenceDate', referenceDate);
    return this.http.get<SwfCensusPreview>(this.base, { params });
  }

  downloadXml(referenceDate: string): Observable<Blob> {
    const params = new HttpParams().set('referenceDate', referenceDate);
    return this.http.get(`${this.base}/xml`, { params, responseType: 'blob' });
  }
}
