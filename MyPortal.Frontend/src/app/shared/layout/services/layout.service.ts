import { Injectable, NgZone } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LayoutService {
  private mq = window.matchMedia('(min-width: 1024px) and (hover: hover)'); // lg
  isDesktop = this.mq.matches;
  sidebarOpen = this.isDesktop;

  constructor(private zone: NgZone) {
    // keep state synced on resize/orientation changes
    this.mq.addEventListener('change', e => {
      this.zone.run(() => this.setDesktop(e.matches));
    });
  }

  setDesktop(flag: boolean) {
    this.isDesktop = flag;
    this.sidebarOpen = flag;
  }

  toggleSidebar() { this.sidebarOpen = !this.sidebarOpen; }
  closeSidebar()  { this.sidebarOpen = false; }
}
