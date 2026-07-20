import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { MeService } from '../services/me-service';
import { redirectToLogin } from '../auth/redirect-to-login';

export const authErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const me = inject(MeService);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        me.clearCache();
        redirectToLogin(router.url);
      }
      return throwError(() => err);
    })
  );
};
