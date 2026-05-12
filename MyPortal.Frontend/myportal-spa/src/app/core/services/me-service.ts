import { Injectable, inject } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Me } from '../interfaces/me';

@Injectable({ providedIn: 'root' })
export class MeService {
  private readonly http = inject(HttpClient);
  private me$?: Observable<Me>;

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
