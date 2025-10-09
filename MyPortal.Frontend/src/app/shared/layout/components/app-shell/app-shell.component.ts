import { Component, OnInit } from '@angular/core';
import { LayoutService } from '../../services/layout.service';

@Component({
  selector: 'app-shell',
  templateUrl: './app-shell.component.html',
  styleUrls: ['./app-shell.component.scss'],
  standalone: false
})
export class AppShellComponent implements OnInit {
  constructor(public layout: LayoutService) {}

  ngOnInit(): void {
    // ensure initial state is set (already done in service constructor)
    // left here for clarity; no extra work needed
  }

  toggleSidebar() { this.layout.toggleSidebar(); }
  closeSidebar()  { if (!this.layout.isDesktop) this.layout.closeSidebar(); }
}
