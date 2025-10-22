import { MenuItem } from 'primeng/api';
import { LinkItem, MenuCategory } from './menu-types';

export function buildMenu(
  categories: MenuCategory[],
  has: (perm: string) => boolean
): MenuItem[] {
  const menu: MenuItem[] = [];

  menu.push({
    label: 'Home',
    icon: 'pi pi-home',
    routerLink: ['/']
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
        .map<MenuItem>((ch: LinkItem) => ({
          label: ch.label,
          icon: ch.icon,
          routerLink: ch.routerLink
        }));

      if (visibleChildren.length === 0) return null;

      return {
        label: cat.label,
        icon: cat.icon,
        items: visibleChildren
      } as MenuItem;
    })
    .filter((x): x is MenuItem => !!x);

  // 👇 3️⃣ Combine and return
  menu.push(...categoryItems);
  return menu;
}
