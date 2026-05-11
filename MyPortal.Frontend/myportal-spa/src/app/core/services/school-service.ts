import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, shareReplay } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SchoolService {
  private localName$?: Observable<string | null>;

  constructor(private http: HttpClient) {}

  // Reads as text so it works whether ASP.NET responds with JSON (`"Name"`) or
  // plain text (`Name`). Returns null for an empty or missing name so callers do
  // one null check.
  getLocalName(): Observable<string | null> {
    if (!this.localName$) {
      this.localName$ = this.http
        .get('/api/schools/local/name', { responseType: 'text' })
        .pipe(
          map(raw => {
            if (!raw) return null;
            let value = raw.trim();
            if (value.length >= 2 && value.startsWith('"') && value.endsWith('"')) {
              try { value = JSON.parse(value) as string; } catch { /* leave as-is */ }
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
