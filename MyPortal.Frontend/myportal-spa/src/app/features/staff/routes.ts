import { Routes } from '@angular/router';
import { AppShell } from '../../layout/components/app-shell/app-shell.component';
import { AuthGuard } from '../../core/guards/auth-guard';
import { canDeactivateGuard } from '../../core/guards/can-deactivate.guard';
import { Home } from './home/home';
import { UserListPage } from './system/users/user-list-page/user-list-page';
import { UserDetailsPage } from './system/users/user-details-page/user-details-page';
import { RoleListPage } from './system/roles/role-list-page/role-list-page';
import { RoleDetailsPage } from './system/roles/role-details-page/role-details-page';
import { BulletinSettingsPage } from './system/bulletin-settings/bulletin-settings-page';
import { SchoolDetailsPage } from './school/school-details/school-details-page';
import { AcademicYearListPage } from './curriculum/academic-years/academic-year-list-page/academic-year-list-page';
import { StaffMemberListPage } from './people/staff-members/staff-member-list-page/staff-member-list-page';
import { StaffMemberDetailsPage } from './people/staff-members/staff-member-details-page/staff-member-details-page';
import { StaffCompliancePage } from './people/staff-compliance/staff-compliance-page';
import { ReportLibraryPage } from './reports/report-library/report-library-page';
import { SalaryInformationPage } from './reports/salary-information/salary-information-page';
import { ContractInformationPage } from './reports/contract-information/contract-information-page';
import { ContractAnalysisPage } from './reports/contract-analysis/contract-analysis-page';
import { TerminatingContractsPage } from './reports/terminating-contracts/terminating-contracts-page';
import { StaffAbsenceAnalysisPage } from './reports/staff-absence-analysis/staff-absence-analysis-page';
import { IndividualAbsencePage } from './reports/individual-absence/individual-absence-page';
import { LongTermAbsencePage } from './reports/long-term-absence/long-term-absence-page';
import { StaffTrainingPage } from './reports/staff-training/staff-training-page';
import { TrainingCoursePage } from './reports/training-course/training-course-page';
import { PostListPage } from './setup/posts/post-list-page/post-list-page';
import { ServiceTermListPage } from './setup/service-terms/service-term-list-page/service-term-list-page';
import { ServiceTermDetailsPage } from './setup/service-terms/service-term-details-page/service-term-details-page';
import { Permissions } from '../../core/constants/permissions';
export const STAFF_ROUTES: Routes = [
  {
    path: '',
    component: AppShell,
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
        path: 'system/users/new',
        component: UserDetailsPage,
        canActivate: [AuthGuard],
        canDeactivate: [canDeactivateGuard],
        data: {
          create: true,
          permissionsAny: [Permissions.SystemAdmin.EditUsers],
          breadcrumb: 'New User'
        }
      },
      {
        path: 'system/users/:id',
        component: UserDetailsPage,
        canActivate: [AuthGuard],
        canDeactivate: [canDeactivateGuard],
        data: {
          permissionsAny: [Permissions.SystemAdmin.ViewUsers, Permissions.SystemAdmin.EditUsers],
          breadcrumb: 'User'
        }
      },
      {
        path: 'system/roles',
        component: RoleListPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.SystemAdmin.ViewRoles, Permissions.SystemAdmin.EditRoles],
          breadcrumb: 'Roles'
        }
      },
      {
        path: 'system/roles/:id',
        component: RoleDetailsPage,
        canActivate: [AuthGuard],
        canDeactivate: [canDeactivateGuard],
        data: {
          permissionsAny: [Permissions.SystemAdmin.ViewRoles, Permissions.SystemAdmin.EditRoles],
          breadcrumb: 'Role'
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
        data: {
          permissionsAny: [Permissions.Curriculum.EditAcademicYears],
          breadcrumb: 'Academic Years'
        }
      },
      {
        path: 'setup/posts',
        component: PostListPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewStaffSetup, Permissions.Staff.EditStaffSetup],
          breadcrumb: 'Posts'
        }
      },
      {
        path: 'setup/service-terms',
        component: ServiceTermListPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewStaffSetup, Permissions.Staff.EditStaffSetup],
          breadcrumb: 'Service Terms'
        }
      },
      {
        path: 'setup/service-terms/:id',
        component: ServiceTermDetailsPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewStaffSetup, Permissions.Staff.EditStaffSetup],
          breadcrumb: 'Service Term'
        }
      },
      {
        path: 'reports',
        component: ReportLibraryPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffEmploymentDetails],
          breadcrumb: 'Reports'
        }
      },
      {
        path: 'reports/salary-information',
        component: SalaryInformationPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffEmploymentDetails],
          breadcrumb: 'Salary Information'
        }
      },
      {
        path: 'reports/contract-information',
        component: ContractInformationPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffEmploymentDetails],
          breadcrumb: 'Contract Information'
        }
      },
      {
        path: 'reports/contract-analysis',
        component: ContractAnalysisPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffEmploymentDetails],
          breadcrumb: 'Contract Analysis'
        }
      },
      {
        path: 'reports/terminating-contracts',
        component: TerminatingContractsPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffEmploymentDetails],
          breadcrumb: 'Terminating Contracts'
        }
      },
      {
        path: 'reports/staff-absence-analysis',
        component: StaffAbsenceAnalysisPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffAbsences],
          breadcrumb: 'Absence Analysis'
        }
      },
      {
        path: 'reports/individual-absence',
        component: IndividualAbsencePage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffAbsences],
          breadcrumb: 'Individual Absence'
        }
      },
      {
        path: 'reports/long-term-absence',
        component: LongTermAbsencePage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffAbsences],
          breadcrumb: 'Long-Term Absence'
        }
      },
      {
        path: 'reports/staff-training',
        component: StaffTrainingPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffEmploymentDetails],
          breadcrumb: 'Staff Training'
        }
      },
      {
        path: 'reports/training-course',
        component: TrainingCoursePage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffEmploymentDetails],
          breadcrumb: 'Training Course'
        }
      },
      {
        path: 'people/staff-members',
        component: StaffMemberListPage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffBasicDetails],
          breadcrumb: 'Staff'
        }
      },
      {
        path: 'people/compliance',
        component: StaffCompliancePage,
        canActivate: [AuthGuard],
        data: {
          permissionsAny: [Permissions.Staff.ViewAllStaffPreEmploymentChecks],
          breadcrumb: 'Compliance'
        }
      },
      {
        path: 'people/staff-members/:id',
        component: StaffMemberDetailsPage,
        canActivate: [AuthGuard],
        canDeactivate: [canDeactivateGuard],
        data: {
          permissionsAny: [
            Permissions.Staff.ViewOwnStaffBasicDetails,
            Permissions.Staff.ViewManagedStaffBasicDetails,
            Permissions.Staff.ViewAllStaffBasicDetails,
            Permissions.Staff.EditManagedStaffBasicDetails,
            Permissions.Staff.EditAllStaffBasicDetails
          ],
          breadcrumb: 'Staff profile'
        }
      }
    ]
  }
];
