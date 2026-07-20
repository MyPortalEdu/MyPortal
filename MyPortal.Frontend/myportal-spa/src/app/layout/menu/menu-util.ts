import { LinkItem, MenuCategory, NavItem } from './menu-types';

export function buildMenu(
  categories: MenuCategory[],
  has: (perm: string) => boolean,
  homeLink: string | any[] = ['/']
): NavItem[] {
  const menu: NavItem[] = [];

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
