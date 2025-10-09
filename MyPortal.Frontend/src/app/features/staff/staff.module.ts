import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StaffRoutingModule } from './staff-routing.module';
import {LayoutModule} from '../../shared/layout/layout.module';
import {SharedModule} from '../../shared/shared.module';
import {MENU_CONTRIBUTORS} from '../../shared/layout/menu/menu.token';
import {StaffMenuContributor} from '../../shared/layout/menu/contributors/staff.menu';
import { HomePageComponent } from './home/containers/home-page/home-page.component';


@NgModule({
  declarations: [
    HomePageComponent
  ],
  imports: [
    CommonModule,
    LayoutModule,
    SharedModule,
    StaffRoutingModule
  ],
  providers: [
    {provide: MENU_CONTRIBUTORS, useClass: StaffMenuContributor, multi: true},
  ],
})
export class StaffModule { }
