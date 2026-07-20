import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { StaffPreEmploymentPanel } from './staff-pre-employment-panel';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import {
  StaffPreEmploymentChecksResponse,
  StaffPreEmploymentChecksUpsertRequest,
} from '../../../../../../shared/types/staff-pre-employment-checks';

interface DbsRow {
  dbsCheckTypeId: string | null;
  certificateNumber: string;
  issueDate: Date | null;
}
interface PreModel {
  identityCheckedDate: Date | null;
  dbsChecks: DbsRow[];
  references: { refereeName: string }[];
  overseasChecks: unknown[];
  rightToWorkChecks: unknown[];
  notes: string;
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
}
interface Internals {
  model: { (): PreModel; update(fn: (m: PreModel) => PreModel): void };
  f: () => FieldLike;
}

function makeResponse(overrides: Partial<StaffPreEmploymentChecksResponse> = {}): StaffPreEmploymentChecksResponse {
  return {
    identityCheckedDate: '2026-01-05T00:00:00.000Z',
    prohibitionFromTeachingCheckedDate: null,
    prohibitionFromManagementCheckedDate: null,
    childcareDisqualificationCheckedDate: null,
    medicalFitnessCheckedDate: null,
    qualificationsVerifiedDate: null,
    notes: null,
    dbsChecks: [],
    rightToWorkChecks: [],
    references: [],
    overseasChecks: [],
    dbsCheckTypes: [{ id: 'dbs-1', description: 'Enhanced' }],
    rightToWorkDocumentTypes: [],
    referenceTypes: [],
    referenceStatuses: [],
    countries: [],
  } as unknown as StaffPreEmploymentChecksResponse;
}

describe('StaffPreEmploymentPanel (array forms)', () => {
  let fixture: ComponentFixture<StaffPreEmploymentPanel>;
  let component: StaffPreEmploymentPanel;
  let internals: Internals;
  let data: SpyObj<StaffMembersDataService>;
  let notify: SpyObj<NotificationService>;

  function configure(perms: string[] = [Permissions.Staff.EditAllStaffPreEmploymentChecks]) {
    data = createSpyObj<StaffMembersDataService>(['getPreEmploymentChecks', 'updatePreEmploymentChecks']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    data.getPreEmploymentChecks.mockReturnValue(of(makeResponse()));
    data.updatePreEmploymentChecks.mockReturnValue(of({ id: 'staff-1' }));

    const translocoStub = {
      translate: (k: string) => k,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(StaffPreEmploymentPanel, { set: { template: '' } });

    TestBed.configureTestingModule({
      providers: [
        { provide: StaffMembersDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    });

    fixture = TestBed.createComponent(StaffPreEmploymentPanel);
    component = fixture.componentInstance;
    internals = component as unknown as Internals;
    fixture.componentRef.setInput('staffMemberId', 'staff-1');
    fixture.componentRef.setInput('permissions', new Set(perms));
    fixture.detectChanges();
  }

  it('hydrates summary dates (as Date) and is valid with no rows', () => {
    configure();
    expect(internals.model().identityCheckedDate instanceof Date).toBe(true);
    expect(internals.f().valid()).toBe(true);
    expect(component.dirty()).toBe(false);
  });

  it('a DBS check with a blank certificate number is invalid (applyEach)', () => {
    configure();
    (component as unknown as { addDbsCheck(): void }).addDbsCheck();
    fixture.detectChanges();
    expect(internals.f().invalid()).toBe(true);

    internals.model.update(m => ({
      ...m,
      dbsChecks: m.dbsChecks.map(d => ({
        ...d,
        dbsCheckTypeId: 'dbs-1',
        certificateNumber: 'C123',
        issueDate: new Date('2026-01-01T00:00:00.000Z'),
      })),
    }));
    expect(internals.f().valid()).toBe(true);
  });

  it('save() posts summary dates as ISO strings and toasts', async () => {
    configure();
    internals.model.update(m => ({ ...m, notes: 'checked' }));

    await component.save();

    const [id, payload] = data.updatePreEmploymentChecks.mock.calls.at(-1)! as [string, StaffPreEmploymentChecksUpsertRequest];
    expect(id).toBe('staff-1');
    expect(payload.identityCheckedDate).toBe('2026-01-05T00:00:00.000Z');
    expect(payload.notes).toBe('checked');
    expect(notify.success).toHaveBeenCalled();
  });

  it('save() surfaces an apiError toast on failure', async () => {
    configure();
    data.updatePreEmploymentChecks.mockReturnValue(throwError(() => new Error('boom')));
    internals.model.update(m => ({ ...m, notes: 'x' }));

    await component.save();

    expect(notify.apiError).toHaveBeenCalled();
  });

  it('no edit permission blocks save', async () => {
    configure([]);
    internals.model.update(m => ({ ...m, notes: 'x' }));
    expect(component.canEdit()).toBe(false);

    await component.save();

    expect(data.updatePreEmploymentChecks).not.toHaveBeenCalled();
  });
});
