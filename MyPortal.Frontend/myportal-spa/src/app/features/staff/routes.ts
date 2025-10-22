import { Routes } from '@angular/router';
import {AppShell} from '../../layout/components/app-shell/app-shell.component';
import {AuthGuard} from '../../core/guards/auth-guard';
import {Home} from './home/home';
import {UserListPage} from './system/users/user-list-page/user-list-page';
import {Permissions} from '../../core/constants/permissions';
export const STAFF_ROUTES: Routes = [
  {
    path: '',
    component: AppShell,
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        component: Home,
        data: { breadcrumb: 'Home' }
      },
      {
        path: 'system/users',
        component: UserListPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.System.ViewUsers, Permissions.System.EditUsers],
          breadcrumb: 'Users'
        }
      }
    ]
  }
];
