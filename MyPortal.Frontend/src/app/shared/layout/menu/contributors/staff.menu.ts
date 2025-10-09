import {AppMenuContributor, MenuCategory} from '../menu.types';
import {MenuItem} from 'primeng/api';
import {buildMenu} from '../menu.util';
import {Me} from '../../../../core/interfaces/me';
import {UserType} from '../../../../core/enums/user-type';
import {Permissions} from '../../../../core/auth/permissions';

export class StaffMenuContributor implements AppMenuContributor {
  supports(user: Me): boolean {
    return user.userType === UserType.Staff;
  }

  getItems(user: Me): MenuItem[] {
    const has = (p: string) => user.permissions?.includes(p) ?? false;
    const cats: MenuCategory[] = [
      {
        label: 'Assessment',
        icon: 'pi pi-chart-bar',
        children: [
          {
            label: 'Marksheets',
            routerLink: ['/staff/assessment/marksheets'],
            permissionsAny: [Permissions.Assessment.ViewAllMarksheets, Permissions.Assessment.ViewOwnMarksheets]
          },
        ]
      },
      {
        label: 'Attendance',
        icon: 'pi pi-calendar-clock',
        children: [
          {
            label: 'Take Register',
            routerLink: ['/staff/attendance/register'],
            permissionsAll: [Permissions.Attendance.EditAttendanceMarks]
          },
        ]
      },
      {
        label: 'Behaviour',
        icon: 'pi pi-star',
        children: [
          {
            label: 'Detentions',
            routerLink: ['/behaviour/detentions'],
            permissionsAny: [Permissions.Behaviour.ViewDetentions, Permissions.Behaviour.EditDetentions]
          }
        ]
      },
      {
        label: 'Curriculum',
        icon: 'pi pi-graduation-cap',
        children: [
          {
            label: 'Study Topics',
            routerLink: ['/curriculum/study-topics'],
            permissionsAny: [Permissions.Curriculum.ViewStudyTopics, Permissions.Curriculum.EditStudyTopics]
          }
        ]
      },
      {
        label: 'System',
        icon: 'pi pi-cog',
        children: [
          {
            label: 'Users',
            routerLink: ['/system/users'],
            permissionsAny: [Permissions.System.ViewUsers, Permissions.System.EditUsers]
          }
        ]
      }
    ];
    return buildMenu(cats, has);
  }
}
