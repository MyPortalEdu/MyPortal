import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { StaffAbsencesPanel } from './staff-absences-panel';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StaffAbsencesResponse, StaffAbsencesUpsertRequest } from '../../../../../../shared/types/staff-absences';

interface AbsenceRow {
  id: string | null;
  absenceTypeId: string | null;
  startDate: Date | null;
  endDate: Date | null;
  notes: string;
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
}
interface Internals {
  model: {
    (): { absences: AbsenceRow[] };
    set(v: { absences: AbsenceRow[] }): void;
    update(fn: (m: { absences: AbsenceRow[] }) => { absences: AbsenceRow[] }): void;
  };
  f: () => FieldLike;
}

function makeAbsences(overrides: Partial<StaffAbsencesResponse> = {}): StaffAbsencesResponse {
  return {
    absences: [
      {
        id: 'a1',
        absenceTypeId: 'type-1',
        illnessTypeId: null,
        startDate: '2026-01-10T00:00:00.000Z',
        endDate: '2026-01-12T00:00:00.000Z',
        isConfidential: false,
        notes: null,
      },
    ],
    absenceTypes: [{ id: 'type-1', description: 'Sickness' }],
    illnessTypes: [],
    ...overrides,
  } as StaffAbsencesResponse;
}

describe('StaffAbsencesPanel (array forms)', () => {
  let fixture: ComponentFixture<StaffAbsencesPanel>;
  let component: StaffAbsencesPanel;
  let internals: Internals;
  let data: SpyObj<StaffMembersDataService>;
  let notify: SpyObj<NotificationService>;

  function configure(perms: string[] = [Permissions.Staff.EditAllStaffAbsences], row = makeAbsences()) {
    data = createSpyObj<StaffMembersDataService>(['getAbsences', 'updateAbsences']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    data.getAbsences.mockReturnValue(of(row));
    data.updateAbsences.mockReturnValue(of({ id: 'staff-1' }));

    const translocoStub = {
      translate: (k: string) => k,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(StaffAbsencesPanel, { set: { template: '' } });

    TestBed.configureTestingModule({
      providers: [
        { provide: StaffMembersDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    });

    fixture = TestBed.createComponent(StaffAbsencesPanel);
    component = fixture.componentInstance;
    internals = component as unknown as Internals;
    fixture.componentRef.setInput('staffMemberId', 'staff-1');
    fixture.componentRef.setInput('permissions', new Set(perms));
    fixture.detectChanges();
  }

  it('hydrates the model (dates as Date) and is valid for a complete row', () => {
    configure();
    expect(internals.model().absences.length).toBe(1);
    expect(internals.model().absences[0].startDate instanceof Date).toBe(true);
    expect(internals.f().valid()).toBe(true);
    expect(component.dirty()).toBe(false);
  });

  it('adding a blank row makes the form invalid (applyEach per-item required)', () => {
    configure();
    (component as unknown as { addAbsence(): void }).addAbsence();
    fixture.detectChanges();
    expect(internals.model().absences.length).toBe(2);
    expect(internals.f().invalid()).toBe(true);
  });

  it('removing rows updates the model', () => {
    configure();
    (component as unknown as { removeAbsence(i: number): void }).removeAbsence(0);
    fixture.detectChanges();
    expect(internals.model().absences.length).toBe(0);
    expect(internals.f().valid()).toBe(true);
  });

  it('an end date before the start date is invalid (cross-field per item)', () => {
    configure();
    internals.model.update(m => ({
      absences: m.absences.map(a => ({ ...a, endDate: new Date('2026-01-01T00:00:00.000Z') })),
    }));
    expect(internals.f().invalid()).toBe(true);
  });

  it('save() posts the payload with ISO date strings and toasts', async () => {
    configure();
    await component.save();
    expect(data.updateAbsences).not.toHaveBeenCalled();

    internals.model.update(m => ({
      absences: m.absences.map(a => ({ ...a, notes: 'flu' })),
    }));
    await component.save();

    const [id, payload] = data.updateAbsences.mock.calls.at(-1)! as [string, StaffAbsencesUpsertRequest];
    expect(id).toBe('staff-1');
    expect(payload.absences[0].startDate).toBe('2026-01-10T00:00:00.000Z');
    expect(payload.absences[0].notes).toBe('flu');
    expect(notify.success).toHaveBeenCalled();
  });

  it('save() surfaces an apiError toast on failure', async () => {
    configure();
    data.updateAbsences.mockReturnValue(throwError(() => new Error('boom')));
    internals.model.update(m => ({ absences: m.absences.map(a => ({ ...a, notes: 'x' })) }));

    await component.save();

    expect(notify.apiError).toHaveBeenCalled();
  });

  it('no edit permission blocks save', async () => {
    configure([]);
    internals.model.update(m => ({ absences: m.absences.map(a => ({ ...a, notes: 'x' })) }));
    expect(component.canEdit()).toBe(false);

    await component.save();

    expect(data.updateAbsences).not.toHaveBeenCalled();
  });
});
