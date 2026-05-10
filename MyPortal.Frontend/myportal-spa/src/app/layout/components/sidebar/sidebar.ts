import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Tooltip } from 'primeng/tooltip';
import { MenuService } from '../../services/menu-service';

@Component({
  selector: 'mp-sidebar',
  imports: [RouterLink, RouterLinkActive, Tooltip],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class SidebarComponent implements OnInit {
  @Input() collapsed = false;
  @Output() expandRequested = new EventEmitter<void>();

  items: MenuItem[] = [];
  expanded = new Set<string>();

  constructor(private menu: MenuService, private router: Router) {}

  async ngOnInit(): Promise<void> {
    this.items = await this.menu.getMenu();
    this.autoExpandActiveGroup();
  }

  // In rail mode the group button has no room for sub-items, so clicking it asks
  // the shell to expand the sidebar and pre-opens the group the user clicked.
  toggle(item: MenuItem): void {
    if (this.collapsed) {
      this.expandRequested.emit();
      this.expanded.add(item.label!);
      return;
    }
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

  // Whether any child of a group matches the current URL. Used in rail mode to
  // highlight the parent icon since the active child isn't visible.
  hasActiveChild(item: MenuItem): boolean {
    if (!item.items?.length) return false;
    const url = this.router.url;
    return item.items.some(child => {
      const link = Array.isArray(child.routerLink) ? child.routerLink.join('/') : child.routerLink;
      return typeof link === 'string' && link.length > 1 && url.startsWith(link);
    });
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
