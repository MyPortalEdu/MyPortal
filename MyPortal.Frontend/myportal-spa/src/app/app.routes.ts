import { Routes } from '@angular/router';
import { UserType } from './core/types/user-type';
import {AuthGuard} from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'portal'
  },
  {
    path: 'portal',
    canMatch: [AuthGuard],
    loadChildren: () => import('./features/portal/routes').then(m => m.PORTAL_ROUTES)
  },
  {
    path: 'staff',
    canMatch: [AuthGuard],
    data: { userTypes: [UserType.Staff] },
    loadChildren: () => import('./features/staff/routes').then(m => m.STAFF_ROUTES)
  }
];
