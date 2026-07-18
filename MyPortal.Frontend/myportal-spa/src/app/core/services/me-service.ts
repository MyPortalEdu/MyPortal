import { Injectable, inject } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Me, MeChangePasswordRequest } from '../types/me';

@Injectable({ providedIn: 'root' })
export class MeService {
  private readonly http = inject(HttpClient);
  private me$?: Observable<Me>;

  me(): Observable<Me> {
    if (!this.me$) {
      this.me$ = this.http.get<Me>('/api/v1/me').pipe(shareReplay(1));
    }
    return this.me$;
  }

  // Self-service password change — the server verifies currentPassword before
  // applying the new one (204 on success, 400 ProblemDetails on failure).
  changePassword(model: MeChangePasswordRequest): Observable<void> {
    return this.http.put<void>('/api/me/password', model);
  }

  clearCache(): void {
    this.me$ = undefined;
  }
}
