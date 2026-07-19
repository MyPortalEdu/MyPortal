import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { SchoolDetailsPage } from './school-details-page';
import { SchoolsDataService } from '../../../../shared/services/schools-data.service';
import { LookupsDataService } from '../../../../shared/services/lookups-data.service';
import { SchoolService } from '../../../../core/services/school-service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { MeService } from '../../../../core/services/me-service';
import { Permissions } from '../../../../core/constants/permissions';
import { UserType } from '../../../../core/types/user-type';
import { SchoolDetailsResponse, SchoolUpsertRequest } from '../../../../shared/types/school';
import { Me } from '../../../../core/types/me';

interface SchoolFormModel {
  name: string;
  urn: string;
  uprn: string;
  establishmentNumber: number | null;
  schoolPhaseId: string | null;
  schoolTypeId: string | null;
  governanceTypeId: string | null;
  intakeTypeId: string | null;
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
}
interface Internals {
  model: { (): SchoolFormModel; update(fn: (m: Record<string, unknown>) => Record<string, unknown>): void };
  f: () => FieldLike;
}

function makeSchool(overrides: Partial<SchoolDetailsResponse> = {}): SchoolDetailsResponse {
  return {
    id: 's1',
    agencyId: 'a1',
    name: 'Test School',
    website: null,
    agencyTypeId: 'at1',
    urn: '100001',
    uprn: '200002',
    establishmentNumber: 3001,
    localAuthorityId: null,
    localAuthorityName: null,
    schoolPhaseId: 'phase-1',
    phase: 'Primary',
    schoolTypeId: 'type-1',
    type: 'Community',
    governanceTypeId: 'gov-1',
    intakeTypeId: 'intake-1',
    headTeacherId: null,
    headTeacherFullName: null,
    ukprn: null,
    payZoneId: null,
    payZoneName: null,
    lowestAge: null,
    highestAge: null,
    netCapacity: null,
    netCapacityAssessmentDate: null,
    isSpecialSchool: false,
    specialSchoolOrganisationId: null,
    specialSchoolOrganisationName: null,
    specialSchoolTypeId: null,
    specialSchoolTypeName: null,
    maxBoarders: null,
    telephone: null,
    email: null,
    isLocal: true,
    ...overrides,
  };
}

function makeMe(overrides: Partial<Me> = {}): Me {
  return {
    id: 'me',
    username: 'me',
    userType: UserType.Staff,
    isEnabled: true,
    isSystem: false,
    displayName: 'Me',
    permissions: [Permissions.Agencies.EditAgencies],
    ...overrides,
  };
}

describe('SchoolDetailsPage', () => {
  let fixture: ComponentFixture<SchoolDetailsPage>;
  let component: SchoolDetailsPage;
  let internals: Internals;
  let schools: SpyObj<SchoolsDataService>;
  let lookups: SpyObj<LookupsDataService>;
  let notify: SpyObj<NotificationService>;
  let me$: SpyObj<MeService>;
  let nameCache: SpyObj<SchoolService>;

  function configure(school: SchoolDetailsResponse | null = makeSchool(), me: Me = makeMe()) {
    schools = createSpyObj<SchoolsDataService>(['getLocalDetails', 'saveLocalDetails']);
    lookups = createSpyObj<LookupsDataService>([
      'governanceTypes', 'intakeTypes', 'schoolPhases', 'schoolTypes',
      'payZones', 'specialSchoolOrganisations', 'specialSchoolTypes',
    ]);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    me$ = createSpyObj<MeService>(['me']);
    nameCache = createSpyObj<SchoolService>(['clearCache']);

    schools.getLocalDetails.mockReturnValue(of(school));
    schools.saveLocalDetails.mockReturnValue(of({ id: 's1' }));
    for (const key of [
      'governanceTypes', 'intakeTypes', 'schoolPhases', 'schoolTypes',
      'payZones', 'specialSchoolOrganisations', 'specialSchoolTypes',
    ] as const) {
      lookups[key].mockReturnValue(of([]));
    }
    me$.me.mockReturnValue(of(me));

    const translocoStub = {
      translate: (k: string) => k,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(SchoolDetailsPage, { set: { template: '' } });

    TestBed.configureTestingModule({
      providers: [
        { provide: SchoolsDataService, useValue: schools },
        { provide: LookupsDataService, useValue: lookups },
        { provide: SchoolService, useValue: nameCache },
        { provide: NotificationService, useValue: notify },
        { provide: MeService, useValue: me$ },
        { provide: ConfirmationDialog, useValue: createSpyObj<ConfirmationDialog>(['confirm']) },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    });

    fixture = TestBed.createComponent(SchoolDetailsPage);
    component = fixture.componentInstance;
    internals = component as unknown as Internals;
    fixture.detectChanges();
  }

  it('hydrates the model from the loaded school and is valid', () => {
    configure();
    expect(internals.model().name).toBe('Test School');
    expect(internals.model().schoolPhaseId).toBe('phase-1');
    expect(internals.f().valid()).toBe(true);
  });

  it('is invalid when a required id (school phase) is missing', () => {
    configure();
    internals.model.update(m => ({ ...m, schoolPhaseId: null }));
    expect(internals.f().invalid()).toBe(true);
  });

  it('is invalid when the establishment number is cleared', () => {
    configure();
    internals.model.update(m => ({ ...m, establishmentNumber: null }));
    expect(internals.f().invalid()).toBe(true);
  });

  it('save() posts the trimmed upsert payload and clears the name cache on success', async () => {
    configure();
    internals.model.update(m => ({ ...m, name: '  Renamed School  ' }));

    await component.save();

    const payload = schools.saveLocalDetails.mock.calls.at(-1)![0] as SchoolUpsertRequest;
    expect(payload.name).toBe('Renamed School');
    expect(payload.urn).toBe('100001');
    expect(payload.schoolPhaseId).toBe('phase-1');
    expect(nameCache.clearCache).toHaveBeenCalled();
    expect(notify.success).toHaveBeenCalled();
  });

  it('save() nulls the special-school fields when isSpecialSchool is false', async () => {
    configure();
    internals.model.update(m => ({
      ...m,
      name: 'Changed',
      isSpecialSchool: false,
      specialSchoolOrganisationId: 'org-1',
      maxBoarders: 50,
    }));

    await component.save();

    const payload = schools.saveLocalDetails.mock.calls.at(-1)![0] as SchoolUpsertRequest;
    expect(payload.specialSchoolOrganisationId).toBeNull();
    expect(payload.maxBoarders).toBeNull();
  });

  it('save() surfaces an apiError toast on failure', async () => {
    configure();
    schools.saveLocalDetails.mockReturnValue(throwError(() => new Error('boom')));
    internals.model.update(m => ({ ...m, name: 'Changed' }));

    await component.save();

    expect(notify.apiError).toHaveBeenCalled();
  });

  it('read-only (no edit permission) blocks save', async () => {
    configure(makeSchool(), makeMe({ permissions: [] }));
    internals.model.update(m => ({ ...m, name: 'Changed' }));
    expect(component.canEdit()).toBe(false);

    await component.save();

    expect(schools.saveLocalDetails).not.toHaveBeenCalled();
  });
});
