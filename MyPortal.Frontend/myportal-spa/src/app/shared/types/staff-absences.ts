import { LookupResponse } from './lookup';

export interface StaffAbsenceCertificateResponse {
  id: string;
  dateReceived: string;
  dateSigned?: string | null;
  isSelfCertified: boolean;
  isReturnToWork: boolean;
  signedBy?: string | null;
  notes?: string | null;
}

export interface StaffAbsenceCertificateUpsertItem {
  id?: string | null;
  dateReceived: string | null;
  dateSigned?: string | null;
  isSelfCertified: boolean;
  isReturnToWork: boolean;
  signedBy?: string | null;
  notes?: string | null;
}

export interface StaffAbsenceResponse {
  id: string;
  absenceTypeId: string;
  illnessTypeId?: string | null;
  startDate: string;
  endDate: string;
  isConfidential: boolean;
  notes?: string | null;
  authorisedPayRateId?: string | null;
  payrollReasonId?: string | null;
  sspExcluded: boolean;
  workingDaysLost?: number | null;
  hoursLost?: number | null;
  isIndustrialInjury: boolean;
  certificates: StaffAbsenceCertificateResponse[];
}

export interface StaffAbsenceUpsertItem {
  id?: string | null;
  absenceTypeId: string | null;
  illnessTypeId?: string | null;
  startDate: string | null;
  endDate: string | null;
  isConfidential: boolean;
  notes?: string | null;
  authorisedPayRateId?: string | null;
  payrollReasonId?: string | null;
  sspExcluded: boolean;
  workingDaysLost?: number | null;
  hoursLost?: number | null;
  isIndustrialInjury: boolean;
  certificates: StaffAbsenceCertificateUpsertItem[];
}

export interface StaffAbsencesResponse {
  absences: StaffAbsenceResponse[];
  absenceTypes: LookupResponse[];
  illnessTypes: LookupResponse[];
  payRates: LookupResponse[];
  payrollReasons: LookupResponse[];
  canEditPayroll: boolean;
}

export interface StaffAbsencesUpsertRequest {
  absences: StaffAbsenceUpsertItem[];
}
