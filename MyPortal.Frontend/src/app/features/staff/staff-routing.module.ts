import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {AuthGuard} from '../../core/guards/auth.guard';
import {AppShellComponent} from '../../shared/layout/components/app-shell/app-shell.component';
import {HomePageComponent} from './home/containers/home-page/home-page.component';

const routes: Routes = [
  {
    path: '',
    component: AppShellComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'home'
      },
      {
        path: 'home',
        component: HomePageComponent,
        data: { breadcrumb: 'Home' }
      },
    ]
  }
];

@NgModule({
  imports: [
    RouterModule.forChild(routes)
  ],
  exports: [RouterModule]
})
export class StaffRoutingModule { }
