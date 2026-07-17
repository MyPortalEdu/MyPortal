import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { SidebarComponent } from '../sidebar/sidebar';
import { Topbar } from '../topbar/topbar';
import { Drawer } from 'primeng/drawer';
import { ButtonDirective } from 'primeng/button';
import { TranslocoDirective } from '@jsverse/transloco';
import { SelectedAcademicYearService } from '../../../core/services/selected-academic-year-service';

@Component({
  selector: 'mp-app-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, SidebarComponent, Topbar, Drawer, ButtonDirective, TranslocoDirective],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShell implements OnInit, OnDestroy {
  private readonly selectedYear = inject(SelectedAcademicYearService);
  private readonly router = inject(Router);

  readonly isDesktop = signal(false);
  readonly sidebarOpen = signal(false);
  readonly sidebarCollapsed = signal(false);

  private mq = window.matchMedia('(min-width: 1024px)');
  private mqHandler = (e: MediaQueryListEvent) => this.setDesktop(e.matches);

  constructor() {
    // The mobile drawer is an overlay — without this it stays open on top of the
    // page the user just navigated to.
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
    // The shell mounts once inside the authenticated portal area (auth-guard
    // upstream), so this is the right place to seed the user's selected AY.
    this.selectedYear.init();
  }

  ngOnDestroy() {
    this.mq.removeEventListener('change', this.mqHandler);
  }

  setDesktop(flag: boolean) {
    this.isDesktop.set(flag);
    this.sidebarOpen.set(flag); // open by default on desktop, closed on mobile
  }

  // On desktop the burger collapses/expands the rail; on mobile it opens the drawer.
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

  closeSidebar() {
    if (!this.isDesktop()) this.sidebarOpen.set(false);
  }
}
