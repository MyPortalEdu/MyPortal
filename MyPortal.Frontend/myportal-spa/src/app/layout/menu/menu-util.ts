import { LinkItem, MenuCategory, NavItem } from './menu-types';

export function buildMenu(
  categories: MenuCategory[],
  has: (perm: string) => boolean,
  homeLink: string | any[] = ['/']
): NavItem[] {
  const menu: NavItem[] = [];

  // Home routes to the caller's portal root (e.g. '/staff', '/student'). It's marked
  // exact-match so it doesn't also highlight when a deeper portal route is active
  // (e.g. /staff/system/users would otherwise prefix-match /staff).
  // Label is a Transloco key — see public/i18n/<lang>.json `pages.home.title`.
  //
  // Icon stored as the FA glyph name only ("fa-house"); the sidebar prefixes
  // `fa-regular` or `fa-solid` based on the row's active state to give the menu
  // a "you are here" weight cue.
  menu.push({
    label: 'pages.home.title',
    icon: 'fa-house',
    routerLink: homeLink,
    state: { exact: true }
  });

  const categoryItems = categories
    .map(cat => {
      const visibleChildren = cat.children
        .filter(ch => {
          const any = ch.permissionsAny ?? [];
          const all = ch.permissionsAll ?? [];

          const anyOk = any.length === 0 || any.some(has);
          const allOk = all.length === 0 || all.every(has);

          return anyOk && allOk;
        })
        .map<NavItem>((ch: LinkItem) => ({
          label: ch.label,
          icon: ch.icon,
          routerLink: ch.routerLink
        }));

      if (visibleChildren.length === 0) return null;

      return {
        label: cat.label,
        icon: cat.icon,
        items: visibleChildren
      } as NavItem;
    })
    .filter((x): x is NavItem => !!x);

  menu.push(...categoryItems);
  return menu;
}
