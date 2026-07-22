import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { StaffEqualityPanel } from './staff-equality-panel';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import {
  StaffEqualityDetailsResponse,
  StaffEqualityDetailsUpsertRequest,
} from '../../../../../../shared/types/staff-equality-details';

interface EqualityModel {
  hasDisability: boolean;
  disabilityDetails: string;
  declaredDisabilities: { disabilityId: string | null }[];
  ethnicityId: string | null;
}
interface Internals {
  model: { (): EqualityModel; update(fn: (m: EqualityModel) => EqualityModel): void };
}

function makeEquality(overrides: Partial<StaffEqualityDetailsResponse> = {}): StaffEqualityDetailsResponse {
  return {
    ethnicityId: 'eth-1',
    nationalityId: null,
    firstLanguageId: null,
    maritalStatusId: null,
    religionId: null,
    sexualOrientationId: null,
    genderIdentityId: null,
    hasDisability: false,
    disabilityDetails: null,
    declaredDisabilities: [],
    impairmentEffectId: null,
    disabilityNumber: null,
    ethnicities: [{ id: 'eth-1', description: 'Ethnicity 1' }],
    nationalities: [],
    languages: [],
    maritalStatuses: [],
    religions: [],
    sexualOrientations: [],
    genderIdentities: [],
    disabilities: [{ id: 'dis-1', description: 'Disability 1' }],
    impairmentEffects: [],
    ...overrides,
  } as StaffEqualityDetailsResponse;
}

describe('StaffEqualityPanel', () => {
  let fixture: ComponentFixture<StaffEqualityPanel>;
  let component: StaffEqualityPanel;
  let internals: Internals;
  let data: SpyObj<StaffMembersDataService>;
  let notify: SpyObj<NotificationService>;

  function configure(perms: string[] = [Permissions.Staff.EditAllStaffEqualityDetails], row = makeEquality()) {
    data = createSpyObj<StaffMembersDataService>(['getEqualityDetails', 'updateEqualityDetails']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    data.getEqualityDetails.mockReturnValue(of(row));
    data.updateEqualityDetails.mockReturnValue(of({ id: 'staff-1' }));

    const translocoStub = {
      translate: (k: string) => k,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(StaffEqualityPanel, { set: { template: '' } });

    TestBed.configureTestingModule({
      providers: [
        { provide: StaffMembersDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    });

    fixture = TestBed.createComponent(StaffEqualityPanel);
    component = fixture.componentInstance;
    internals = component as unknown as Internals;
    fixture.componentRef.setInput('staffMemberId', 'staff-1');
    fixture.componentRef.setInput('permissions', new Set(perms));
    fixture.detectChanges();
  }

  it('hydrates the model from the loaded equality details', () => {
    configure();
    expect(internals.model().ethnicityId).toBe('eth-1');
    expect(component.dirty()).toBe(false);
    expect(component.valid()).toBe(true);
  });

  it('clearing hasDisability wipes the disability fields (effect)', () => {
    configure(
      [Permissions.Staff.EditAllStaffEqualityDetails],
      makeEquality({
        hasDisability: true,
        declaredDisabilities: [
          { disabilityId: 'dis-1', dateAdvised: null, isLongTerm: false, affectsWorkingAbility: false, assistanceRequired: null },
        ],
        disabilityDetails: 'x',
      }),
    );
    expect(internals.model().declaredDisabilities.map(d => d.disabilityId)).toEqual(['dis-1']);

    internals.model.update(m => ({ ...m, hasDisability: false }));
    fixture.detectChanges();

    expect(internals.model().declaredDisabilities).toEqual([]);
    expect(internals.model().disabilityDetails).toBe('');
  });

  it('save() posts the payload, toasts, and leaves edit mode on success', async () => {
    configure();
    component.startEdit();
    internals.model.update(m => ({ ...m, ethnicityId: 'eth-2' }));

    await component.save();

    const [id, payload] = data.updateEqualityDetails.mock.calls.at(-1)! as [string, StaffEqualityDetailsUpsertRequest];
    expect(id).toBe('staff-1');
    expect(payload.ethnicityId).toBe('eth-2');
    expect(notify.success).toHaveBeenCalled();
    expect(component.editing()).toBe(false);
  });

  it('save() is a no-op when nothing is dirty', async () => {
    configure();
    await component.save();
    expect(data.updateEqualityDetails).not.toHaveBeenCalled();
  });

  it('save() surfaces an apiError toast on failure', async () => {
    configure();
    data.updateEqualityDetails.mockReturnValue(throwError(() => new Error('boom')));
    internals.model.update(m => ({ ...m, ethnicityId: 'eth-2' }));

    await component.save();

    expect(notify.apiError).toHaveBeenCalled();
  });

  it('no edit permission blocks save', async () => {
    configure([]);
    internals.model.update(m => ({ ...m, ethnicityId: 'eth-2' }));
    expect(component.canEdit()).toBe(false);

    await component.save();

    expect(data.updateEqualityDetails).not.toHaveBeenCalled();
  });
});
