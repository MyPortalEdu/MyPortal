import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export const apiBaseInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('/api') && environment.apiUrl !== '/api') {
    const cloned = req.clone({ url: `${environment.apiUrl}${req.url.replace(/^\/api/, '')}` });
    return next(cloned);
  }
  return next(req);
};
