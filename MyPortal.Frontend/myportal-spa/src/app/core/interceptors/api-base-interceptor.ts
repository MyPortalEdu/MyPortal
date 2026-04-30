import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

/**
 * Rewrites `/api/...` calls onto `environment.apiUrl`. With `apiUrl: '/api'` (the default)
 * this is a no-op, but it lets a deployment retarget the API host without touching code.
 */
export const apiBaseInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('/api') && environment.apiUrl !== '/api') {
    const cloned = req.clone({ url: `${environment.apiUrl}${req.url.replace(/^\/api/, '')}` });
    return next(cloned);
  }
  return next(req);
};
