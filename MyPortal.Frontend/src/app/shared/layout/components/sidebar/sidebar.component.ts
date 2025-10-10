import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import {MenuService} from '../../services/menu.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  providers: [MenuService], // instance per shell
  standalone: false
})
export class SidebarComponent implements OnInit {
  items: MenuItem[] = [];
  constructor(private menu: MenuService) {}
  async ngOnInit() { this.items = await this.menu.getMenu(); }
}
