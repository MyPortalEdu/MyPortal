import { Injectable } from '@angular/core';
import {Observable, shareReplay} from 'rxjs';
import {Me} from '../interfaces/me';
import {HttpClient} from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class MeService {
  private me$?: Observable<Me>;

  constructor(private http: HttpClient) { }

  me(): Observable<Me> {
    if (!this.me$) {
      this.me$ = this.http.get<Me>('/api/me').pipe(shareReplay(1));
    }
    return this.me$;
  }

  clearCache(): void {
    this.me$ = undefined;
  }
}
