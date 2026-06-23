import {MenuItem} from 'primeng/api';
import {AppMenuContributor, MenuCategory} from '../../layout/menu/menu-types';
import {Me} from '../../core/types/me';
import {UserType} from '../../core/types/user-type';
import {Permissions} from '../../core/constants/permissions';
import {buildMenu} from '../../layout/menu/menu-util';

// Menu contributors return Transloco keys rather than literal text so the
// sidebar can re-render labels on language change without each contributor
// having to depend on the i18n service. The keys live in the root locale file
// (public/i18n/<lang>.json) under `nav.*`.
export class StaffMenuContributor implements AppMenuContributor {
  supports(user: Me): boolean {
    return user.userType === UserType.Staff;
  }

  getItems(user: Me): MenuItem[] {
    const has = (p: string) => user.permissions?.includes(p) ?? false;
    const cats: MenuCategory[] = [
      {
        label: 'nav.people',
        icon: 'fa-users',
        children: [
          {
            label: 'nav.staffMembers',
            routerLink: ['/staff/people/staff-members'],
            permissionsAny: [Permissions.Staff.ViewAllStaffBasicDetails]
          }
        ]
      },
      {
        label: 'nav.school',
        icon: 'fa-school',
        children: [
          {
            label: 'nav.schoolDetails',
            routerLink: ['/staff/school/details'],
            permissionsAny: [Permissions.Agencies.ViewAgencies, Permissions.Agencies.EditAgencies]
          }
        ]
      },
      {
        label: 'nav.curriculum',
        icon: 'fa-graduation-cap',
        children: [
          {
            label: 'nav.academicYears',
            routerLink: ['/staff/curriculum/academic-years'],
            permissionsAny: [Permissions.Curriculum.EditAcademicYears]
          }
        ]
      },
      {
        label: 'nav.system',
        icon: 'fa-gear',
        children: [
          {
            label: 'nav.users',
            routerLink: ['/staff/system/users'],
            permissionsAny: [Permissions.SystemAdmin.ViewUsers, Permissions.SystemAdmin.EditUsers]
          },
          {
            label: 'nav.bulletinSettings',
            routerLink: ['/staff/system/bulletin-settings'],
            permissionsAny: [Permissions.SystemAdmin.BulletinSettings]
          }
        ]
      }
    ];

    return buildMenu(cats, has, ['/staff']);
  }
}
