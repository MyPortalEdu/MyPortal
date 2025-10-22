import {MenuItem} from 'primeng/api';
import {AppMenuContributor, MenuCategory} from '../../layout/menu/menu-types';
import {Me} from '../../core/interfaces/me';
import {UserType} from '../../core/enums/user-type';
import {Permissions} from '../../core/constants/permissions';
import {buildMenu} from '../../layout/menu/menu-util';

export class StaffMenuContributor implements AppMenuContributor {
  supports(user: Me): boolean {
    return user.userType === UserType.Staff;
  }

  getItems(user: Me): MenuItem[] {
    const has = (p: string) => user.permissions?.includes(p) ?? false;
    const cats: MenuCategory[] = [
      {
        label: 'System',
        icon: 'pi pi-cog',
        children: [
          {
            label: 'Users',
            routerLink: ['system/users'],
            permissionsAny: [Permissions.System.ViewUsers, Permissions.System.EditUsers]
          }
        ]
      }
    ];

    return  buildMenu(cats, has);
  }
}
