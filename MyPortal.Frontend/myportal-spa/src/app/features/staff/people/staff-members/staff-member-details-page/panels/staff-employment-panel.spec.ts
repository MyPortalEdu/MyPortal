import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { StaffEmploymentPanel } from './staff-employment-panel';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StaffEmploymentDetailsResponse } from '../../../../../../shared/types/staff-employment-details';

interface ContractRow {
  startDate: Date | null;
  endDate: Date | null;
}
interface EmploymentRow {
  startDate: Date | null;
  endDate: Date | null;
  contracts: ContractRow[];
}
interface EmploymentModel {
  bankName: string;
  bankAccount: string;
  bankSortCode: string;
  niNumber: string;
  employments: EmploymentRow[];
}

function toDate(value: string | null): Date | null {
  return value ? new Date(value) : null;
}

function contract(startDate: string | null, endDate: string | null): unknown {
  return {
    id: null,
    contractTypeId: 'ct-1',
    staffRoleId: null,
    serviceTermId: null,
    departmentId: null,
    payScaleId: null,
    payScalePointId: null,
    postTitle: 'Teacher',
    startDate: toDate(startDate),
    endDate: toDate(endDate),
    fte: 1,
    hoursPerWeek: null,
    weeksPerYear: null,
    annualSalary: null,
    isAgencySupply: false,
    safeguardedSalary: false,
    dailyRate: false,
  };
}

function emp(startDate: string | null, endDate: string | null, contracts: unknown[]): EmploymentRow {
  return {
    id: null,
    startDate: toDate(startDate),
    endDate: toDate(endDate),
    leavingReasonId: null,
    originId: null,
    destinationId: null,
    notes: '',
    contracts,
  } as unknown as EmploymentRow;
}

function emptyResponse(): StaffEmploymentDetailsResponse {
  return {
    employments: [],
    leavingReasons: [],
    origins: [],
    destinations: [],
    contractTypes: [],
    staffRoles: [],
    serviceTerms: [],
    departments: [],
    payScales: [],
    payScalePoints: [],
  } as unknown as StaffEmploymentDetailsResponse;
}

interface Internals {
  model: { set(v: EmploymentModel): void };
  employmentsOverlap(): boolean;
  contractOutOfRange(): boolean;
}

function modelWith(employments: EmploymentRow[]): EmploymentModel {
  return { bankName: '', bankAccount: '', bankSortCode: '', niNumber: '', employments };
}

describe('StaffEmploymentPanel validation', () => {
  let fixture: ComponentFixture<StaffEmploymentPanel>;
  let component: StaffEmploymentPanel;
  let internals: Internals;
  let data: SpyObj<StaffMembersDataService>;

  function make() {
    data = createSpyObj<StaffMembersDataService>(['getEmploymentDetails', 'updateEmploymentDetails']);
    data.getEmploymentDetails.mockReturnValue(of(emptyResponse()));

    TestBed.configureTestingModule({
      providers: [
        { provide: StaffMembersDataService, useValue: data },
        { provide: NotificationService, useValue: createSpyObj<NotificationService>(['success', 'apiError']) },
        { provide: TranslocoService, useValue: { translate: (k: string) => k, getActiveLang: () => 'en' } },
      ],
    });
    TestBed.overrideComponent(StaffEmploymentPanel, { set: { template: '' } });
    fixture = TestBed.createComponent(StaffEmploymentPanel);
    component = fixture.componentInstance;
    internals = component as unknown as Internals;
    fixture.componentRef.setInput('staffMemberId', 'staff-1');
    fixture.componentRef.setInput('permissions', new Set([Permissions.Staff.EditAllStaffEmploymentDetails]));
    fixture.detectChanges();
  }

  it('a single well-formed employment is valid', () => {
    make();
    internals.model.set(modelWith([emp('2026-09-01', null, [contract('2026-09-01', null)])]));
    expect(internals.employmentsOverlap()).toBe(false);
    expect(internals.contractOutOfRange()).toBe(false);
    expect(component.valid()).toBe(true);
  });

  it('rejects overlapping employment periods (open-ended prior spell runs forever)', () => {
    make();
    internals.model.set(modelWith([
      emp('2026-09-01', null, [contract('2026-09-01', null)]),
      emp('2027-01-01', null, [contract('2027-01-01', null)]),
    ]));
    expect(internals.employmentsOverlap()).toBe(true);
    expect(component.valid()).toBe(false);
  });

  it('rejects a contract that starts before its employment period', () => {
    make();
    internals.model.set(modelWith([
      emp('2026-09-01', '2027-08-31', [contract('2026-08-01', '2027-01-01')]),
    ]));
    expect(internals.contractOutOfRange()).toBe(true);
    expect(component.valid()).toBe(false);
  });

  it('rejects a contract that ends after a closed employment period', () => {
    make();
    internals.model.set(modelWith([
      emp('2026-09-01', '2027-08-31', [contract('2026-09-01', '2027-12-01')]),
    ]));
    expect(internals.contractOutOfRange()).toBe(true);
    expect(component.valid()).toBe(false);
  });
});
