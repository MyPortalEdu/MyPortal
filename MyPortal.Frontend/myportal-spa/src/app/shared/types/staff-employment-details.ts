import { LookupResponse } from './lookup';

// Mirrors MyPortal.Contracts.Models.People.PayScalePointResponse — a pay-scale
// point carrying its parent payScaleId so the editor can cascade.
export interface PayScalePointResponse {
  id: string;
  payScaleId: string;
  description: string;
  fullTimeSalary?: number | null;
}

// Mirrors MyPortal.Contracts.Models.People.StaffContractResponse.
export interface StaffContractResponse {
  id: string;
  contractTypeId: string;
  staffRoleId?: string | null;
  serviceTermId?: string | null;
  departmentId?: string | null;
  payScaleId?: string | null;
  payScalePointId?: string | null;
  postTitle: string;
  spinePoint?: string | null;
  startDate: string;
  endDate?: string | null;
  fte: number;
  hoursPerWeek?: number | null;
  weeksPerYear?: number | null;
  annualSalary?: number | null;
  isAgencySupply: boolean;
  safeguardedSalary: boolean;
  dailyRate: boolean;
}

// Mirrors MyPortal.Contracts.Models.People.StaffContractUpsertItem — null id is a
// new row; populated id updates; omitted rows are soft-deleted server-side.
export interface StaffContractUpsertItem {
  id?: string | null;
  contractTypeId: string | null;
  staffRoleId?: string | null;
  serviceTermId?: string | null;
  departmentId?: string | null;
  payScaleId?: string | null;
  payScalePointId?: string | null;
  postTitle: string;
  spinePoint?: string | null;
  startDate: string | null;
  endDate?: string | null;
  fte: number;
  hoursPerWeek?: number | null;
  weeksPerYear?: number | null;
  annualSalary?: number | null;
  isAgencySupply: boolean;
  safeguardedSalary: boolean;
  dailyRate: boolean;
}

// Mirrors MyPortal.Contracts.Models.People.StaffEmploymentResponse.
export interface StaffEmploymentResponse {
  id: string;
  startDate: string;
  endDate?: string | null;
  leavingReasonId?: string | null;
  originId?: string | null;
  destinationId?: string | null;
  notes?: string | null;
  contracts: StaffContractResponse[];
}

// Mirrors MyPortal.Contracts.Models.People.StaffEmploymentUpsertItem.
export interface StaffEmploymentUpsertItem {
  id?: string | null;
  startDate: string | null;
  endDate?: string | null;
  leavingReasonId?: string | null;
  originId?: string | null;
  destinationId?: string | null;
  notes?: string | null;
  contracts: StaffContractUpsertItem[];
}

// Mirrors MyPortal.Contracts.Models.People.StaffEmploymentDetailsResponse —
// bank/NI + employment spells (with contracts) + the option lists.
export interface StaffEmploymentDetailsResponse {
  bankName?: string | null;
  bankAccount?: string | null;
  bankSortCode?: string | null;
  niNumber?: string | null;

  payZoneId?: string | null;
  payZoneName?: string | null;

  employments: StaffEmploymentResponse[];

  leavingReasons: LookupResponse[];
  origins: LookupResponse[];
  destinations: LookupResponse[];
  contractTypes: LookupResponse[];
  staffRoles: LookupResponse[];
  serviceTerms: LookupResponse[];
  departments: LookupResponse[];
  payScales: LookupResponse[];
  payScalePoints: PayScalePointResponse[];
}

// Mirrors MyPortal.Contracts.Models.People.StaffEmploymentDetailsUpsertRequest.
export interface StaffEmploymentDetailsUpsertRequest {
  bankName?: string | null;
  bankAccount?: string | null;
  bankSortCode?: string | null;
  niNumber?: string | null;

  employments: StaffEmploymentUpsertItem[];
}
