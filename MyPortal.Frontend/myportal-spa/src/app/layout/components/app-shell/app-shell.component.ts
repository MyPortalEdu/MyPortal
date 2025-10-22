import {Component, NgZone} from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {SidebarComponent} from '../sidebar/sidebar';
import {Topbar} from '../topbar/topbar';
import {Drawer} from 'primeng/drawer';
import {NgIf} from '@angular/common';

@Component({
  selector: 'app-app-shell',
  imports: [RouterOutlet, SidebarComponent, Topbar, Drawer, NgIf],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShell {
  isDesktop = false;
  sidebarOpen = false;

  private mq = window.matchMedia('(min-width: 1024px)'); // lg breakpoint
  private mqHandler = (e: MediaQueryListEvent) => {
    this.zone.run(() => this.setDesktop(e.matches));
  };

  constructor(private zone: NgZone) {}

  ngOnInit() {
    this.setDesktop(this.mq.matches);
    this.mq.addEventListener('change', this.mqHandler);
  }

  ngOnDestroy() {
    this.mq.removeEventListener('change', this.mqHandler);
  }

  setDesktop(flag: boolean) {
    this.isDesktop = flag;
    this.sidebarOpen = flag; // open by default on desktop, closed on mobile
  }

  toggleSidebar() { this.sidebarOpen = !this.sidebarOpen; }
  closeSidebar()  { if (!this.isDesktop) this.sidebarOpen = false; }
}
