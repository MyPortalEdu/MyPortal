import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { MeService } from '../services/me-service';

/**
 * On 401 from the API, clear the cached `Me` and bounce to the server login page so the
 * user can re-authenticate. Without this, expired sessions silently break the UI.
 */
export const authErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const me = inject(MeService);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        me.clearCache();
        const returnUrl = encodeURIComponent(router.url);
        // Use location for a top-level navigation — the login page is server-rendered.
        window.location.href = `/account/login?returnUrl=${returnUrl}`;
      }
      return throwError(() => err);
    })
  );
};
