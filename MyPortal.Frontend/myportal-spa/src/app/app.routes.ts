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
    // canMatch short-circuits the lazy load on access denied (same rationale as /staff).
    // No userTypes data — the portal area (account settings, etc.) is for every user type.
    canMatch: [AuthGuard],
    loadChildren: () => import('./features/portal/routes').then(m => m.PORTAL_ROUTES)
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
