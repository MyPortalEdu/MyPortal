import {Me} from '../../core/types/me';

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

export interface NavItem {
  label: string;
  icon?: string;
  routerLink?: string | any[];
  items?: NavItem[];
  state?: { exact?: boolean };
}

export interface AppMenuContributor {
  supports(me: Me): boolean;
  getItems(me: Me): NavItem[];
}
