import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppShellComponent } from './components/app-shell/app-shell.component';
import { TopbarComponent } from './components/topbar/topbar.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import {SharedModule} from '../shared.module';
import {RouterModule} from '@angular/router';
import {MenubarModule} from 'primeng/menubar';
import {PanelMenuModule} from 'primeng/panelmenu';
import {BreadcrumbModule} from 'primeng/breadcrumb';
import {AvatarModule} from 'primeng/avatar';
import {DividerModule} from 'primeng/divider';
import {ButtonModule} from 'primeng/button';



@NgModule({
  declarations: [
    AppShellComponent,
    TopbarComponent,
    SidebarComponent
  ],
  imports: [
    SharedModule,
    RouterModule,
    CommonModule,
    MenubarModule,
    PanelMenuModule,
    BreadcrumbModule,
    AvatarModule,
    DividerModule,
    ButtonModule
  ],
  exports: [
    AppShellComponent,
    SidebarComponent,
    TopbarComponent,
  ]
})
export class LayoutModule { }
