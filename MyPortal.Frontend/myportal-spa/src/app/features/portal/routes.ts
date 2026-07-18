import { Routes } from '@angular/router';
import { AppShell } from '../../layout/components/app-shell/app-shell.component';
import { AuthGuard } from '../../core/guards/auth-guard';
import { canDeactivateGuard } from '../../core/guards/can-deactivate.guard';
import { PortalRedirect } from '../../core/components/portal-redirect/portal-redirect';
import { AccountSettingsPage } from './account-settings/account-settings-page';

// Shared portal area for every authenticated user, regardless of user type.
// The parent canMatch in app.routes already gated entry, so no userTypes data here.
export const PORTAL_ROUTES: Routes = [
  {
    // Exact /portal bounces the user to their type-specific home (/staff, /student, …).
    // pathMatch: 'full' so it doesn't swallow /portal/settings below.
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
