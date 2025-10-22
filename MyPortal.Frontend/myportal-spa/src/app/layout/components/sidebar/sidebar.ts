import {Component, OnInit} from '@angular/core';
import {MenuItem} from 'primeng/api';
import {MenuService} from '../../services/menu-service';
import {PanelMenuModule} from 'primeng/panelmenu';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'mp-sidebar',
  imports: [PanelMenuModule, RouterLink],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss'
})
export class SidebarComponent implements OnInit {
  items: MenuItem[] = [];
  constructor(private menu: MenuService) {}
  async ngOnInit() { this.items = await this.menu.getMenu(); }
}
