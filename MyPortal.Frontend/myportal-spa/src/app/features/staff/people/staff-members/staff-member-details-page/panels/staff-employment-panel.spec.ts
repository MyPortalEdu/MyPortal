import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { StaffEmploymentPanel } from './staff-employment-panel';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import {
  StaffContractUpsertItem,
  StaffEmploymentDetailsResponse,
  StaffEmploymentUpsertItem,
} from '../../../../../../shared/types/staff-employment-details';

function contract(startDate: string | null, endDate: string | null): StaffContractUpsertItem {
  return {
    id: null,
    contractTypeId: 'ct-1',
    staffRoleId: null,
    serviceTermId: null,
    departmentId: null,
    payScaleId: null,
    payScalePointId: null,
    postTitle: 'Teacher',
    startDate,
    endDate,
    fte: 1,
    hoursPerWeek: null,
    weeksPerYear: null,
    annualSalary: null,
    isAgencySupply: false,
    safeguardedSalary: false,
    dailyRate: false,
  } as unknown as StaffContractUpsertItem;
}

function emp(
  startDate: string | null,
  endDate: string | null,
  contracts: StaffContractUpsertItem[],
): StaffEmploymentUpsertItem {
  return {
    id: null,
    startDate,
    endDate,
    leavingReasonId: null,
    originId: null,
    destinationId: null,
    notes: null,
    contracts,
  } as unknown as StaffEmploymentUpsertItem;
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
  employments: { set(v: StaffEmploymentUpsertItem[]): void };
  employmentsOverlap(): boolean;
  contractOutOfRange(): boolean;
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
    internals.employments.set([emp('2026-09-01', null, [contract('2026-09-01', null)])]);
    expect(internals.employmentsOverlap()).toBe(false);
    expect(internals.contractOutOfRange()).toBe(false);
    expect(component.valid()).toBe(true);
  });

  it('rejects overlapping employment periods (open-ended prior spell runs forever)', () => {
    make();
    internals.employments.set([
      emp('2026-09-01', null, [contract('2026-09-01', null)]),
      emp('2027-01-01', null, [contract('2027-01-01', null)]),
    ]);
    expect(internals.employmentsOverlap()).toBe(true);
    expect(component.valid()).toBe(false);
  });

  it('rejects a contract that starts before its employment period', () => {
    make();
    internals.employments.set([
      emp('2026-09-01', '2027-08-31', [contract('2026-08-01', '2027-01-01')]),
    ]);
    expect(internals.contractOutOfRange()).toBe(true);
    expect(component.valid()).toBe(false);
  });

  it('rejects a contract that ends after a closed employment period', () => {
    make();
    internals.employments.set([
      emp('2026-09-01', '2027-08-31', [contract('2026-09-01', '2027-12-01')]),
    ]);
    expect(internals.contractOutOfRange()).toBe(true);
    expect(component.valid()).toBe(false);
  });
});
