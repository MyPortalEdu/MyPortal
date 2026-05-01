import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  CanMatch,
  Route,
  Router,
  RouterStateSnapshot,
  UrlSegment,
  UrlTree
} from '@angular/router';
import { firstValueFrom } from 'rxjs';
import {UserType} from '../enums/user-type';
import {MeService} from '../services/me-service';

type AuthData = {
  permissionsAll?: readonly string[];
  permissionsAny?: readonly string[];
  userTypes?: readonly (UserType | number)[];
};

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate, CanMatch {
  constructor(private me: MeService, private router: Router) {}

  // ---- CanActivate (components)
  async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean | UrlTree> {
    return this.checkAllowed(route.data as AuthData, state.url);
  }

  // ---- CanMatch (lazy modules)
  async canMatch(route: Route, segments: UrlSegment[]): Promise<boolean | UrlTree> {
    const url = '/' + segments.map(s => s.path).join('/');
    return this.checkAllowed((route.data ?? {}) as AuthData, url);
  }

  // ---- Core check
  private async checkAllowed(data: AuthData, returnUrl: string): Promise<boolean | UrlTree> {
    try {
      const me = await firstValueFrom(this.me.me());

      const needAllPerms = data.permissionsAll ?? [];
      const needAnyPerms = data.permissionsAny ?? [];
      const needTypes = data.userTypes ?? [];

      const hasPerms =
        (needAllPerms.length === 0 || needAllPerms.every(p => me.permissions?.includes(p))) &&
        (needAnyPerms.length === 0 || needAnyPerms.some(p => me.permissions?.includes(p)));

      const hasUserType =
        needTypes.length === 0 || needTypes.includes(me.userType);

      return (hasPerms && hasUserType) ? true : this.router.parseUrl('/');
    } catch (e: unknown) {
      if (e instanceof HttpErrorResponse) {
        if (e.status === 401) {
          // 401 is also handled by auth-error-interceptor; keep this path as a fallback
          // for callers that don't go through the interceptor.
          const candidate = returnUrl || (location.pathname + location.search + location.hash);
          const safe = AuthGuard.sanitizeReturnUrl(candidate);
          location.href = `/account/login?returnUrl=${encodeURIComponent(safe)}`;
          return false;
        }

        if (e.status === 403) {
          // Server says we're authenticated but not authorized. Bounce home.
          return this.router.parseUrl('/');
        }
      }

      // Network blip, 5xx, or anything else — log and bounce home rather than silently
      // collapsing into a login redirect (which would mask transient backend issues).
      console.error('AuthGuard: unexpected error while checking access', e);
      return this.router.parseUrl('/');
    }
  }

  // Reject anything that isn't a same-origin path (must start with a single '/').
  private static sanitizeReturnUrl(value: string): string {
    if (!value.startsWith('/')) return '/';
    if (value.startsWith('//') || value.startsWith('/\\')) return '/';
    return value;
  }
}
