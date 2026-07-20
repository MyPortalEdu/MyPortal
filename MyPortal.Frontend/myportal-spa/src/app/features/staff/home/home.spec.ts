import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { Home } from './home';
import { MeService } from '../../../core/services/me-service';
import { SchoolService } from '../../../core/services/school-service';
import { AcademicYearsDataService } from '../../../shared/services/academic-years-data.service';
import { Permissions } from '../../../core/constants/permissions';
import { UserType } from '../../../core/types/user-type';
import { Me } from '../../../core/types/me';

function makeMe(permissions: string[]): Me {
  return {
    id: 'me',
    username: 'me',
    userType: UserType.Staff,
    isEnabled: true,
    isSystem: false,
    displayName: 'Me',
    permissions,
  };
}

describe('Home permission gating', () => {
  let me$: SpyObj<MeService>;
  let schools: SpyObj<SchoolService>;
  let academicYears: SpyObj<AcademicYearsDataService>;

  function configure(permissions: string[]) {
    me$ = createSpyObj<MeService>(['me']);
    schools = createSpyObj<SchoolService>(['getLocalName']);
    academicYears = createSpyObj<AcademicYearsDataService>(['list']);
    me$.me.mockReturnValue(of(makeMe(permissions)));
    schools.getLocalName.mockReturnValue(of('Test School'));
    academicYears.list.mockReturnValue(of([]));

    TestBed.overrideComponent(Home, { set: { template: '' } });
    TestBed.configureTestingModule({
      providers: [
        { provide: MeService, useValue: me$ },
        { provide: SchoolService, useValue: schools },
        { provide: AcademicYearsDataService, useValue: academicYears },
        { provide: TranslocoService, useValue: { translate: (k: string) => k, getActiveLang: () => 'en' } },
      ],
    });
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  it('a users/roles-only user does NOT fetch academic years and hides the bulletins feed', () => {
    const c = configure(['SystemAdmin.EditUsers', 'SystemAdmin.EditRoles']);
    expect(academicYears.list).not.toHaveBeenCalled();
    expect(c.canViewBulletins()).toBe(false);
    expect(c.canEditAcademicYears()).toBe(false);
  });

  it('an academic-year editor fetches the list (to drive the setup prompt)', () => {
    const c = configure([Permissions.Curriculum.EditAcademicYears]);
    expect(academicYears.list).toHaveBeenCalled();
    expect(c.canEditAcademicYears()).toBe(true);
  });

  it('a bulletins viewer shows the feed', () => {
    const c = configure([Permissions.School.ViewSchoolBulletins]);
    expect(c.canViewBulletins()).toBe(true);
    expect(academicYears.list).not.toHaveBeenCalled();
  });
});
