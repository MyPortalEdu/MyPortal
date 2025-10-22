import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable()
export class ApiBaseInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // only prefix relative API calls (skip absolute http/https and non-api paths if you like)
    if (req.url.startsWith('/api')) {
      const cloned = req.clone({ url: `${environment.apiUrl}${req.url.replace(/^\/api/, '')}` });
      return next.handle(cloned);
    }
    return next.handle(req);
  }
}
