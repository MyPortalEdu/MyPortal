import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { MeService } from '../../core/services/me-service';
import { AppMenuContributor, NavItem } from '../menu/menu-types';
import { MENU_CONTRIBUTORS } from '../menu/menu-token';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly me = inject(MeService);
  private readonly contributors = inject<AppMenuContributor[]>(MENU_CONTRIBUTORS, { optional: true }) ?? [];

  async getMenu(): Promise<NavItem[]> {
    const user = await firstValueFrom(this.me.me());
    const active = this.contributors.filter(c => c.supports(user));
    const chunks = await Promise.all(active.map(c => c.getItems(user)));
    return chunks.flat();
  }
}
