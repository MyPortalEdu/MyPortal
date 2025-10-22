import { MenuItem } from 'primeng/api';
import {Me} from '../../core/interfaces/me';

export interface LinkItem {
  label: string;
  icon?: string;
  routerLink: string | any[];
  permissionsAny?: readonly string[];
  permissionsAll?: readonly string[];
}
export interface MenuCategory {
  label: string;
  icon?: string;
  children: LinkItem[];
}
export interface AppMenuContributor {
  supports(me: Me): boolean;
  getItems(me: Me): MenuItem[];
}
