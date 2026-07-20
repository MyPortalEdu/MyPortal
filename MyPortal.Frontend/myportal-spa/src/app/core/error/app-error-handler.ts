import { ErrorHandler, Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable()
export class AppErrorHandler implements ErrorHandler {
  handleError(error: unknown): void {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 401) return;
      console.error(`HTTP ${error.status} ${error.url ?? ''}`, error.message, error.error);
      return;
    }

    if (!environment.production) {
      console.error('Unhandled error', error);
    } else {
      const message = error instanceof Error ? error.message : String(error);
      console.error('Unhandled error:', message);
    }
  }
}
