import { Inject, Injectable, Optional } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { MenuItem } from 'primeng/api';
import {MeService} from '../../core/services/me-service';
import {AppMenuContributor} from '../menu/menu-types';
import {MENU_CONTRIBUTORS} from '../menu/menu-token';

@Injectable({ providedIn: 'root' })
export class MenuService {
  constructor(
    private me: MeService,
    @Optional() @Inject(MENU_CONTRIBUTORS) private contributors: AppMenuContributor[] = []
  ) {}
  async getMenu(): Promise<MenuItem[]> {
    const user = await firstValueFrom(this.me.me());
    const active = this.contributors.filter(c => c.supports(user));
    const chunks = await Promise.all(active.map(c => c.getItems(user)));
    return chunks.flat();
  }
}
