import { LookupResponse } from './lookup';

export interface PayScalePointResponse {
  id: string;
  payScaleId: string;
  description: string;
  fullTimeSalary?: number | null;
}

export interface StaffContractResponse {
  id: string;
  contractTypeId: string;
  staffRoleId?: string | null;
  serviceTermId?: string | null;
  departmentId?: string | null;
  payScaleId?: string | null;
  payScalePointId?: string | null;
  postTitle: string;
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

export interface StaffContractUpsertItem {
  id?: string | null;
  contractTypeId: string | null;
  staffRoleId?: string | null;
  serviceTermId?: string | null;
  departmentId?: string | null;
  payScaleId?: string | null;
  payScalePointId?: string | null;
  postTitle: string;
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

export interface StaffEmploymentDetailsUpsertRequest {
  bankName?: string | null;
  bankAccount?: string | null;
  bankSortCode?: string | null;
  niNumber?: string | null;

  employments: StaffEmploymentUpsertItem[];
}
