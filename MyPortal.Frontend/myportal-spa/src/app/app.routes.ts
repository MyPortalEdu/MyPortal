import { Routes } from '@angular/router';
import { UserType } from './core/enums/user-type';
import {AuthGuard} from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'portal'
  },
  {
    path: 'portal', canActivate: [AuthGuard],
    loadComponent: () => import('./core/components/portal-redirect/portal-redirect').then(m => m.PortalRedirectComponent)
  },
  {
    path: 'staff',
    // canMatch alone is enough at the lazy boundary — it short-circuits the lazy load on
    // access denied. Adding canActivate here would re-run the guard for no benefit.
    canMatch: [AuthGuard],
    data: { userTypes: [UserType.Staff] },
    loadChildren: () => import('./features/staff/routes').then(m => m.STAFF_ROUTES)
  }
];
