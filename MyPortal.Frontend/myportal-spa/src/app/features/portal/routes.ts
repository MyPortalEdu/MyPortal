import { Routes } from '@angular/router';
import { AppShell } from '../../layout/components/app-shell/app-shell.component';
import { AuthGuard } from '../../core/guards/auth-guard';
import { canDeactivateGuard } from '../../core/guards/can-deactivate.guard';
import { PortalRedirect } from '../../core/components/portal-redirect/portal-redirect';
import { AccountSettingsPage } from './account-settings/account-settings-page';

export const PORTAL_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: PortalRedirect,
  },
  {
    path: '',
    component: AppShell,
    children: [
      {
        path: 'settings',
        component: AccountSettingsPage,
        canActivate: [AuthGuard],
        canDeactivate: [canDeactivateGuard],
        data: { breadcrumb: 'Account settings' },
      },
    ],
  },
];
