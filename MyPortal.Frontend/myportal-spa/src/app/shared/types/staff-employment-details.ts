import { LookupResponse } from './lookup';

export interface PayScalePointResponse {
  id: string;
  payScaleId: string;
  pointValue: number;
  description: string;
  fullTimeSalary?: number | null;
}

export interface PayScaleResponse {
  id: string;
  serviceTermId: string;
  description: string;
}

export interface ServiceTermSchemeLink {
  serviceTermId: string;
  superannuationSchemeId: string;
  isMain: boolean;
}

export interface ServiceTermDefaults {
  serviceTermId: string;
  hoursPerWeek?: number | null;
  weeksPerYear?: number | null;
}

export interface SuperannuationSchemeResponse {
  id: string;
  description: string;
  employerRate?: number | null;
}

export interface StaffContractSuspensionResponse {
  id: string;
  startDate: string;
  endDate?: string | null;
  reason?: string | null;
}

export interface StaffContractSuspensionUpsertItem {
  id?: string | null;
  startDate: string | null;
  endDate?: string | null;
  reason?: string | null;
}

export interface StaffContractSalaryChangeResponse {
  id: string;
  oldPayScalePointId?: string | null;
  newPayScalePointId?: string | null;
  oldAnnualSalary?: number | null;
  newAnnualSalary?: number | null;
  changedAt: string;
  changedBy?: string | null;
}

export interface StaffContractAllowanceResponse {
  id: string;
  additionalPaymentTypeId: string;
  amount: number;
  payFactor?: number | null;
  startDate: string;
  endDate?: string | null;
  isSuperannuable: boolean;
  isSubjectToNi: boolean;
  isBenefitInKind: boolean;
  reason?: string | null;
}

export interface StaffContractAllowanceUpsertItem {
  id?: string | null;
  additionalPaymentTypeId: string | null;
  amount: number;
  payFactor?: number | null;
  startDate: string | null;
  endDate?: string | null;
  isSuperannuable: boolean;
  isSubjectToNi: boolean;
  isBenefitInKind: boolean;
  reason?: string | null;
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
  postId?: string | null;
  superannuationSchemeId?: string | null;
  niContractedOut: boolean;
  allowances: StaffContractAllowanceResponse[];
  suspensions: StaffContractSuspensionResponse[];
  salaryChanges: StaffContractSalaryChangeResponse[];
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
  postId?: string | null;
  superannuationSchemeId?: string | null;
  niContractedOut: boolean;
  allowances: StaffContractAllowanceUpsertItem[];
  suspensions: StaffContractSuspensionUpsertItem[];
  salaryChanges: StaffContractSalaryChangeResponse[];
}

export interface StaffEmploymentResponse {
  id: string;
  startDate: string;
  endDate?: string | null;
  continuousServiceStartDate?: string | null;
  localAuthorityStartDate?: string | null;
  leavingReasonId?: string | null;
  originId?: string | null;
  destinationId?: string | null;
  previousEmployer?: string | null;
  nextEmployer?: string | null;
  notes?: string | null;
  contracts: StaffContractResponse[];
}

export interface StaffEmploymentUpsertItem {
  id?: string | null;
  startDate: string | null;
  endDate?: string | null;
  continuousServiceStartDate?: string | null;
  localAuthorityStartDate?: string | null;
  leavingReasonId?: string | null;
  originId?: string | null;
  destinationId?: string | null;
  previousEmployer?: string | null;
  nextEmployer?: string | null;
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
  payScales: PayScaleResponse[];
  serviceTermSchemes: ServiceTermSchemeLink[];
  serviceTermDefaults: ServiceTermDefaults[];
  additionalPaymentTypes: LookupResponse[];
  superannuationSchemes: SuperannuationSchemeResponse[];
  posts: LookupResponse[];
  payScalePoints: PayScalePointResponse[];
}

export interface StaffEmploymentDetailsUpsertRequest {
  bankName?: string | null;
  bankAccount?: string | null;
  bankSortCode?: string | null;
  niNumber?: string | null;

  employments: StaffEmploymentUpsertItem[];
}
