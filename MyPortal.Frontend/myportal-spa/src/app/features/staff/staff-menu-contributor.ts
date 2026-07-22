import {AppMenuContributor, MenuCategory, NavItem} from '../../layout/menu/menu-types';
import {Me} from '../../core/types/me';
import {UserType} from '../../core/types/user-type';
import {Permissions} from '../../core/constants/permissions';
import {buildMenu} from '../../layout/menu/menu-util';

export class StaffMenuContributor implements AppMenuContributor {
  supports(user: Me): boolean {
    return user.userType === UserType.Staff;
  }

  getItems(user: Me): NavItem[] {
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
        label: 'nav.staffSetup',
        icon: 'fa-sliders',
        children: [
          {
            label: 'nav.posts',
            routerLink: ['/staff/setup/posts'],
            permissionsAny: [Permissions.Staff.ViewStaffSetup, Permissions.Staff.EditStaffSetup]
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
            label: 'nav.roles',
            routerLink: ['/staff/system/roles'],
            permissionsAny: [Permissions.SystemAdmin.ViewRoles, Permissions.SystemAdmin.EditRoles]
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
