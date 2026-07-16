import { Routes } from '@angular/router';
import { AppShell } from '../../layout/components/app-shell/app-shell.component';
import { AuthGuard } from '../../core/guards/auth-guard';
import { canDeactivateGuard } from '../../core/guards/can-deactivate.guard';
import { Home } from './home/home';
import { UserListPage } from './system/users/user-list-page/user-list-page';
import { RoleListPage } from './system/roles/role-list-page/role-list-page';
import { BulletinSettingsPage } from './system/bulletin-settings/bulletin-settings-page';
import { SchoolDetailsPage } from './school/school-details/school-details-page';
import { AcademicYearListPage } from './curriculum/academic-years/academic-year-list-page/academic-year-list-page';
import { Permissions } from '../../core/constants/permissions';
export const STAFF_ROUTES: Routes = [
  {
    path: '',
    component: AppShell,
    // No canActivate here — the parent canMatch in app.routes already gated entry to /staff.
    // Apply canActivate only on children that have additional permission requirements.
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
          permissionsAny: [Permissions.SystemAdmin.ViewUsers, Permissions.SystemAdmin.EditUsers],
          breadcrumb: 'Users'
        }
      },
      {
        path: 'system/roles',
        component: RoleListPage,
        canActivate: [AuthGuard],
        // Gated on View — the listing is read-viewable; create/edit/delete actions within enforce
        // EditRoles server-side.
        data: {
          permissionsAny: [Permissions.SystemAdmin.ViewRoles, Permissions.SystemAdmin.EditRoles],
          breadcrumb: 'Roles'
        }
      },
      {
        path: 'system/bulletin-settings',
        component: BulletinSettingsPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.SystemAdmin.BulletinSettings],
          breadcrumb: 'Bulletin Settings'
        }
      },
      {
        path: 'school/details',
        component: SchoolDetailsPage,
        canActivate: [AuthGuard],
        canDeactivate: [canDeactivateGuard],
        data: {
          permissionsAny: [Permissions.Agencies.ViewAgencies, Permissions.Agencies.EditAgencies],
          breadcrumb: 'School Details'
        }
      },
      {
        path: 'curriculum/academic-years',
        component: AcademicYearListPage,
        canActivate: [AuthGuard],
        // Gated on Edit (not View) by design — the listing page is the entry point for
        // create/edit/delete. A read-only view variant can be added later if needed.
        data: {
          permissionsAny: [Permissions.Curriculum.EditAcademicYears],
          breadcrumb: 'Academic Years'
        }
      }
    ]
  }
];
