import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { MeService } from '../services/me.service';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class PermissionsGuard implements CanActivate {
  constructor(private me: MeService, private router: Router) {}

  async canActivate(route: ActivatedRouteSnapshot): Promise<boolean> {
    const required = route.data['permissions'] as string[] || [];
    const me = await firstValueFrom(this.me.me());

    const hasAll = required.every(p => me.permissions?.includes(p));
    if (!hasAll) {
      await this.router.navigateByUrl('/unauthorized');
      return false;
    }
    return true;
  }
}
