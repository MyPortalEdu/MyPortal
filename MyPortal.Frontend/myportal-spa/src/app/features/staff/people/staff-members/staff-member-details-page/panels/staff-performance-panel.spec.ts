import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { StaffPerformancePanel } from './staff-performance-panel';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StaffPerformanceResponse, StaffPerformanceUpsertRequest } from '../../../../../../shared/types/staff-performance';

interface ObjectiveRow {
  title: string;
}
interface ObservationRow {
  observerId: string | null;
}
interface PerfModel {
  reviews: unknown[];
  objectives: ObjectiveRow[];
  observations: ObservationRow[];
  trainingRecords: unknown[];
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
}
interface Internals {
  model: { (): PerfModel; update(fn: (m: PerfModel) => PerfModel): void };
  f: () => FieldLike;
}

function makePerformance(overrides: Partial<StaffPerformanceResponse> = {}): StaffPerformanceResponse {
  return {
    reviews: [],
    objectives: [],
    observations: [],
    trainingRecords: [],
    reviewStatuses: [],
    objectiveStatuses: [],
    objectiveCategories: [],
    outcomes: [],
    staff: [],
    trainingCourses: [],
    trainingStatuses: [],
    ...overrides,
  } as StaffPerformanceResponse;
}

describe('StaffPerformancePanel (multi-array forms)', () => {
  let fixture: ComponentFixture<StaffPerformancePanel>;
  let component: StaffPerformancePanel;
  let internals: Internals;
  let data: SpyObj<StaffMembersDataService>;
  let notify: SpyObj<NotificationService>;

  function configure(perms: string[] = [Permissions.Staff.EditAllStaffPerformanceDetails], row = makePerformance()) {
    data = createSpyObj<StaffMembersDataService>(['getPerformance', 'updatePerformance']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    data.getPerformance.mockReturnValue(of(row));
    data.updatePerformance.mockReturnValue(of({ id: 'staff-1' }));

    const translocoStub = {
      translate: (k: string) => k,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(StaffPerformancePanel, { set: { template: '' } });

    TestBed.configureTestingModule({
      providers: [
        { provide: StaffMembersDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    });

    fixture = TestBed.createComponent(StaffPerformancePanel);
    component = fixture.componentInstance;
    internals = component as unknown as Internals;
    fixture.componentRef.setInput('staffMemberId', 'staff-1');
    fixture.componentRef.setInput('permissions', new Set(perms));
    fixture.detectChanges();
  }

  it('an empty performance record hydrates valid (no rows to validate)', () => {
    configure();
    expect(internals.f().valid()).toBe(true);
    expect(component.dirty()).toBe(false);
  });

  it('adding an objective with a blank title is invalid (applyEach on objectives)', () => {
    configure();
    (component as unknown as { addObjective(): void }).addObjective();
    fixture.detectChanges();
    expect(internals.f().invalid()).toBe(true);

    internals.model.update(m => ({ ...m, objectives: m.objectives.map(o => ({ ...o, title: 'Improve X' })) }));
    expect(internals.f().valid()).toBe(true);
  });

  it('an observation missing its required observer is invalid', () => {
    configure();
    (component as unknown as { addObservation(): void }).addObservation();
    fixture.detectChanges();
    expect(internals.f().invalid()).toBe(true);
  });

  it('save() posts the four arrays and toasts on success', async () => {
    configure();
    (component as unknown as { addObjective(): void }).addObjective();
    internals.model.update(m => ({ ...m, objectives: m.objectives.map(o => ({ ...o, title: 'Goal' })) }));

    await component.save();

    const [id, payload] = data.updatePerformance.mock.calls.at(-1)! as [string, StaffPerformanceUpsertRequest];
    expect(id).toBe('staff-1');
    expect(payload.objectives.length).toBe(1);
    expect(payload.objectives[0].title).toBe('Goal');
    expect(notify.success).toHaveBeenCalled();
  });

  it('save() surfaces an apiError toast on failure', async () => {
    configure();
    data.updatePerformance.mockReturnValue(throwError(() => new Error('boom')));
    (component as unknown as { addObjective(): void }).addObjective();
    internals.model.update(m => ({ ...m, objectives: m.objectives.map(o => ({ ...o, title: 'Goal' })) }));

    await component.save();

    expect(notify.apiError).toHaveBeenCalled();
  });

  it('no edit permission blocks save', async () => {
    configure([]);
    (component as unknown as { addObjective(): void }).addObjective();
    internals.model.update(m => ({ ...m, objectives: m.objectives.map(o => ({ ...o, title: 'Goal' })) }));
    expect(component.canEdit()).toBe(false);

    await component.save();

    expect(data.updatePerformance).not.toHaveBeenCalled();
  });
});
