import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private http: HttpClient, private router: Router) {}

  async canActivate(): Promise<boolean | UrlTree> {
    try {
      await firstValueFrom(this.http.get('/api/me'));
      return true;
    } catch (e: any) {
      if (e?.status === 401) {
        const returnUrl = encodeURIComponent(location.pathname + location.search + location.hash);
        location.href = `/account/login?returnUrl=${returnUrl}`;
        return false;
      }
      return this.router.parseUrl('/'); // fallback
    }
  }
}
