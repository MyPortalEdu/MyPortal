import {Component, NgZone, OnDestroy, OnInit} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar';
import { Topbar } from '../topbar/topbar';
import { Drawer } from 'primeng/drawer';
import { ButtonDirective } from 'primeng/button';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'mp-app-shell',
  imports: [RouterOutlet, SidebarComponent, Topbar, Drawer, ButtonDirective, TranslocoDirective],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShell implements OnInit, OnDestroy {
  isDesktop = false;
  sidebarOpen = false;
  sidebarCollapsed = false;

  private mq = window.matchMedia('(min-width: 1024px)');
  private mqHandler = (e: MediaQueryListEvent) => {
    this.zone.run(() => this.setDesktop(e.matches));
  };

  constructor(private zone: NgZone) {}

  ngOnInit() {
    this.setDesktop(this.mq.matches);
    this.mq.addEventListener('change', this.mqHandler);
    this.sidebarCollapsed = localStorage.getItem('mp:sidebar') === 'collapsed';
  }

  ngOnDestroy() {
    this.mq.removeEventListener('change', this.mqHandler);
  }

  setDesktop(flag: boolean) {
    this.isDesktop = flag;
    this.sidebarOpen = flag; // open by default on desktop, closed on mobile
  }

  // On desktop the burger collapses/expands the rail; on mobile it opens the drawer.
  toggleSidebar() {
    if (this.isDesktop) {
      this.sidebarCollapsed = !this.sidebarCollapsed;
      localStorage.setItem('mp:sidebar', this.sidebarCollapsed ? 'collapsed' : 'expanded');
    } else {
      this.sidebarOpen = !this.sidebarOpen;
    }
  }

  expandSidebar() {
    if (this.isDesktop && this.sidebarCollapsed) {
      this.sidebarCollapsed = false;
      localStorage.setItem('mp:sidebar', 'expanded');
    }
  }

  closeSidebar() { if (!this.isDesktop) this.sidebarOpen = false; }
}
