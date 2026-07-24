import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { SidebarComponent } from '../sidebar/sidebar';
import { Topbar } from '../topbar/topbar';
import { MpButton, MpDrawer } from '@myportal/ui';
import { TranslocoDirective } from '@jsverse/transloco';
import { SelectedAcademicYearService } from '../../../core/services/selected-academic-year-service';

@Component({
  selector: 'mp-app-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, SidebarComponent, Topbar, MpDrawer, MpButton, TranslocoDirective],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShell implements OnInit, OnDestroy {
  private readonly selectedYear = inject(SelectedAcademicYearService);
  private readonly router = inject(Router);

  readonly isDesktop = signal(false);
  readonly sidebarOpen = signal(false);
  readonly sidebarCollapsed = signal(false);

  // Transient peek: while the rail is collapsed, hover/focus floats the full sidebar over the
  // content without disturbing the collapsed preference. Held while the pointer stays over the
  // sidebar (even across navigation) and cleared only on mouse-leave / focus-out.
  readonly hoverExpanded = signal(false);

  private mq = window.matchMedia('(min-width: 1024px)');
  private mqHandler = (e: MediaQueryListEvent) => this.setDesktop(e.matches);

  constructor() {
    this.router.events
      .pipe(
        filter(e => e instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.closeSidebar());
  }

  ngOnInit() {
    this.setDesktop(this.mq.matches);
    this.mq.addEventListener('change', this.mqHandler);
    this.sidebarCollapsed.set(localStorage.getItem('mp:sidebar') === 'collapsed');
    this.selectedYear.init();
  }

  ngOnDestroy() {
    this.mq.removeEventListener('change', this.mqHandler);
  }

  setDesktop(flag: boolean) {
    this.isDesktop.set(flag);
    this.sidebarOpen.set(flag);
  }

  toggleSidebar() {
    if (this.isDesktop()) {
      const next = !this.sidebarCollapsed();
      this.sidebarCollapsed.set(next);
      localStorage.setItem('mp:sidebar', next ? 'collapsed' : 'expanded');
    } else {
      this.sidebarOpen.update(v => !v);
    }
  }

  expandSidebar() {
    if (this.isDesktop() && this.sidebarCollapsed()) {
      this.sidebarCollapsed.set(false);
      localStorage.setItem('mp:sidebar', 'expanded');
    }
  }

  onSidebarEnter() {
    if (this.isDesktop() && this.sidebarCollapsed()) this.hoverExpanded.set(true);
  }

  onSidebarLeave() {
    this.hoverExpanded.set(false);
  }

  onSidebarFocusOut(event: FocusEvent) {
    const host = event.currentTarget as HTMLElement;
    const next = event.relatedTarget as Node | null;
    if (!next || !host.contains(next)) this.hoverExpanded.set(false);
  }

  closeSidebar() {
    if (!this.isDesktop()) this.sidebarOpen.set(false);
  }
}
