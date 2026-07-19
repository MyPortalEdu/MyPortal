import { ChangeDetectionStrategy, Component, OnInit, inject, input, output, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MpTooltip } from '@myportal/ui';
import { TranslocoPipe } from '@jsverse/transloco';
import { MenuService } from '../../services/menu-service';
import { NavItem } from '../../menu/menu-types';

@Component({
  selector: 'mp-sidebar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, MpTooltip, TranslocoPipe],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class SidebarComponent implements OnInit {
  private readonly menu = inject(MenuService);
  private readonly router = inject(Router);

  readonly collapsed = input(false);
  readonly expandRequested = output<void>();

  readonly items = signal<NavItem[]>([]);
  readonly expanded = signal<ReadonlySet<string>>(new Set());

  async ngOnInit(): Promise<void> {
    try {
      const items = await this.menu.getMenu();
      this.items.set(items);
      this.autoExpandActiveGroup();
    } catch (err) {
      // Menu fetch ultimately depends on /api/me; a transient failure shouldn't
      // crash the shell — just log and render an empty nav.
      console.error('Sidebar: failed to build menu', err);
      this.items.set([]);
    }
  }

  // In rail mode the group button has no room for sub-items, so clicking it asks
  // the shell to expand the sidebar and pre-opens the group the user clicked.
  toggle(item: NavItem): void {
    const key = item.label!;
    if (this.collapsed()) {
      this.expandRequested.emit();
      this.expanded.update(s => new Set(s).add(key));
      return;
    }
    this.expanded.update(s => {
      const next = new Set(s);
      if (next.has(key)) next.delete(key);
      else next.add(key);
      return next;
    });
  }

  isExpanded(item: NavItem): boolean {
    return this.expanded().has(item.label!);
  }

  // Items can opt into exact-match via `state.exact` (set by buildMenu for the Home
  // row). Without it, RouterLinkActive does prefix matching, which would highlight
  // Home whenever any deeper portal route is active.
  isExactMatch(item: NavItem): boolean {
    return item.state?.['exact'] === true;
  }

  // Whether any child of a group matches the current URL. Used in rail mode to
  // highlight the parent icon since the active child isn't visible.
  hasActiveChild(item: NavItem): boolean {
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
    const next = new Set<string>();
    for (const item of this.items()) {
      if (!item.items?.length) continue;
      const hasActiveChild = item.items.some(child => {
        const link = Array.isArray(child.routerLink) ? child.routerLink.join('/') : child.routerLink;
        return typeof link === 'string' && link.length > 1 && url.startsWith(link);
      });
      if (hasActiveChild) next.add(item.label!);
    }
    if (next.size > 0) this.expanded.set(next);
  }
}
