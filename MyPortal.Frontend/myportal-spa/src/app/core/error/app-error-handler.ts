import { ErrorHandler, Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';

/**
 * Last-line error handler. Logs structured info so unhandled errors don't get swallowed
 * silently in production. Hook a toast / remote-logging service here as the app grows.
 */
@Injectable()
export class AppErrorHandler implements ErrorHandler {
  handleError(error: unknown): void {
    if (error instanceof HttpErrorResponse) {
      // 401s are intercepted globally and redirect; don't double-log.
      if (error.status === 401) return;
      console.error(`HTTP ${error.status} ${error.url ?? ''}`, error.message, error.error);
      return;
    }

    if (!environment.production) {
      console.error('Unhandled error', error);
    } else {
      // In prod, swallow stack details from the user-facing console but keep a breadcrumb.
      const message = error instanceof Error ? error.message : String(error);
      console.error('Unhandled error:', message);
    }
  }
}
