import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MenuService } from '../../services/menu-service';

@Component({
  selector: 'mp-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class SidebarComponent implements OnInit {
  items: MenuItem[] = [];
  expanded = new Set<string>();

  constructor(private menu: MenuService, private router: Router) {}

  async ngOnInit(): Promise<void> {
    this.items = await this.menu.getMenu();
    this.autoExpandActiveGroup();
  }

  toggle(item: MenuItem): void {
    const key = item.label!;
    if (this.expanded.has(key)) {
      this.expanded.delete(key);
    } else {
      this.expanded.add(key);
    }
  }

  isExpanded(item: MenuItem): boolean {
    return this.expanded.has(item.label!);
  }

  // Items can opt into exact-match via `state.exact` (set by buildMenu for the Home
  // row). Without it, RouterLinkActive does prefix matching, which would highlight
  // Home whenever any deeper portal route is active.
  isExactMatch(item: MenuItem): boolean {
    return item.state?.['exact'] === true;
  }

  // Open the group whose children contain the current URL on load, so the active
  // sub-item is visible without the user having to click to expand.
  private autoExpandActiveGroup(): void {
    const url = this.router.url;
    for (const item of this.items) {
      if (!item.items?.length) continue;
      const hasActiveChild = item.items.some(child => {
        const link = Array.isArray(child.routerLink) ? child.routerLink.join('/') : child.routerLink;
        return typeof link === 'string' && link.length > 1 && url.startsWith(link);
      });
      if (hasActiveChild) this.expanded.add(item.label!);
    }
  }
}
