import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { StaffProfessionalPanel } from './staff-professional-panel';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import {
  StaffProfessionalDetailsResponse,
  StaffProfessionalDetailsUpsertRequest,
} from '../../../../../../shared/types/staff-professional-details';

interface QualRow {
  title: string;
}
interface ProfModel {
  isTeachingStaff: boolean;
  hasQts: boolean;
  teacherReferenceNumber: string;
  qtsRouteId: string | null;
  inductionStatusId: string | null;
  qualifications: QualRow[];
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
}
interface Internals {
  model: { (): ProfModel; update(fn: (m: ProfModel) => ProfModel): void };
  f: () => FieldLike;
}

function makeResponse(overrides: Partial<StaffProfessionalDetailsResponse> = {}): StaffProfessionalDetailsResponse {
  return {
    isTeachingStaff: true,
    hasQts: true,
    hasHlta: false,
    hasQtls: false,
    hasEyts: false,
    isSeniorLeadership: false,
    teacherReferenceNumber: '1234567',
    qtsRouteId: 'route-1',
    qtsAwardedDate: null,
    inductionStatusId: 'induction-1',
    inductionStartDate: null,
    inductionCompletedDate: null,
    qualificationsSummary: null,
    qualifications: [],
    qtsRoutes: [{ id: 'route-1', description: 'Route' }],
    inductionStatuses: [{ id: 'induction-1', description: 'In progress' }],
    qualificationLevels: [],
    classesOfDegree: [],
  } as unknown as StaffProfessionalDetailsResponse;
}

describe('StaffProfessionalPanel', () => {
  let fixture: ComponentFixture<StaffProfessionalPanel>;
  let component: StaffProfessionalPanel;
  let internals: Internals;
  let data: SpyObj<StaffMembersDataService>;
  let notify: SpyObj<NotificationService>;

  function configure(perms: string[] = [Permissions.Staff.EditAllStaffProfessionalDetails], row = makeResponse()) {
    data = createSpyObj<StaffMembersDataService>(['getProfessionalDetails', 'updateProfessionalDetails']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    data.getProfessionalDetails.mockReturnValue(of(row));
    data.updateProfessionalDetails.mockReturnValue(of({ id: 'staff-1' }));

    const translocoStub = {
      translate: (k: string) => k,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(StaffProfessionalPanel, { set: { template: '' } });

    TestBed.configureTestingModule({
      providers: [
        { provide: StaffMembersDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    });

    fixture = TestBed.createComponent(StaffProfessionalPanel);
    component = fixture.componentInstance;
    internals = component as unknown as Internals;
    fixture.componentRef.setInput('staffMemberId', 'staff-1');
    fixture.componentRef.setInput('permissions', new Set(perms));
    fixture.detectChanges();
  }

  it('hydrates and is valid for a well-formed TRN', () => {
    configure();
    expect(internals.model().teacherReferenceNumber).toBe('1234567');
    expect(internals.f().valid()).toBe(true);
  });

  it('a non-7-digit TRN is invalid; a blank TRN is valid', () => {
    configure();
    internals.model.update(m => ({ ...m, teacherReferenceNumber: '12' }));
    expect(internals.f().invalid()).toBe(true);

    internals.model.update(m => ({ ...m, teacherReferenceNumber: '' }));
    expect(internals.f().valid()).toBe(true);
  });

  it('clearing isTeachingStaff cascades (clears TRN, hasQts, induction) via effects', () => {
    configure();
    internals.model.update(m => ({ ...m, isTeachingStaff: false }));
    fixture.detectChanges();
    const m = internals.model();
    expect(m.teacherReferenceNumber).toBe('');
    expect(m.hasQts).toBe(false);
    expect(m.qtsRouteId).toBeNull();
    expect(m.inductionStatusId).toBeNull();
  });

  it('a qualification with a blank title is invalid', () => {
    configure();
    (component as unknown as { addQualification(): void }).addQualification();
    fixture.detectChanges();
    expect(internals.f().invalid()).toBe(true);

    internals.model.update(m => ({ ...m, qualifications: m.qualifications.map(q => ({ ...q, title: 'BSc' })) }));
    expect(internals.f().valid()).toBe(true);
  });

  it('save() posts the payload and toasts on success', async () => {
    configure();
    internals.model.update(m => ({ ...m, teacherReferenceNumber: '7654321' }));

    await component.save();

    const [id, payload] = data.updateProfessionalDetails.mock.calls.at(-1)! as [string, StaffProfessionalDetailsUpsertRequest];
    expect(id).toBe('staff-1');
    expect(payload.teacherReferenceNumber).toBe('7654321');
    expect(notify.success).toHaveBeenCalled();
  });

  it('no edit permission blocks save', async () => {
    configure([]);
    internals.model.update(m => ({ ...m, teacherReferenceNumber: '7654321' }));
    expect(component.canEdit()).toBe(false);

    await component.save();

    expect(data.updateProfessionalDetails).not.toHaveBeenCalled();
  });
});
