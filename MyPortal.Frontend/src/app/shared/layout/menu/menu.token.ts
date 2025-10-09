import { InjectionToken } from '@angular/core';
import { AppMenuContributor } from './menu.types';

export const MENU_CONTRIBUTORS = new InjectionToken<AppMenuContributor[]>('MENU_CONTRIBUTORS');
