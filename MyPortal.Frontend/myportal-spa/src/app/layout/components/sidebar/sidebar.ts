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
      console.error('Sidebar: failed to build menu', err);
      this.items.set([]);
    }
  }

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

  isExactMatch(item: NavItem): boolean {
    return item.state?.['exact'] === true;
  }

  hasActiveChild(item: NavItem): boolean {
    if (!item.items?.length) return false;
    const url = this.router.url;
    return item.items.some(child => {
      const link = Array.isArray(child.routerLink) ? child.routerLink.join('/') : child.routerLink;
      return typeof link === 'string' && link.length > 1 && url.startsWith(link);
    });
  }

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
