import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, shareReplay } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SchoolService {
  private readonly http = inject(HttpClient);
  private localName$?: Observable<string | null>;

  getLocalName(): Observable<string | null> {
    if (!this.localName$) {
      this.localName$ = this.http
        .get('/api/v1/schools/local/name', { responseType: 'text' })
        .pipe(
          map(raw => {
            if (!raw) return null;
            let value = raw.trim();
            if (value.length >= 2 && value.startsWith('"') && value.endsWith('"')) {
              try { value = JSON.parse(value) as string; } catch { }
            }
            return value.length > 0 ? value : null;
          }),
          shareReplay(1),
        );
    }
    return this.localName$;
  }

  clearCache(): void {
    this.localName$ = undefined;
  }
}
