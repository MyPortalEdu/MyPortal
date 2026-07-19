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

/**
 * A sidebar navigation node — a top-level link, or a category with nested link
 * children. Replaces PrimeNG's `MenuItem`; the sidebar is a routed nav tree, not
 * a command-driven popup menu, so it carries `routerLink`/`items`/`state` rather
 * than `command`/`separator`. Labels are Transloco keys.
 */
export interface NavItem {
  label: string;
  icon?: string;
  routerLink?: string | any[];
  items?: NavItem[];
  /** Router matching hint; `exact` marks the Home row so it doesn't prefix-match deeper routes. */
  state?: { exact?: boolean };
}

export interface AppMenuContributor {
  supports(me: Me): boolean;
  getItems(me: Me): NavItem[];
}
