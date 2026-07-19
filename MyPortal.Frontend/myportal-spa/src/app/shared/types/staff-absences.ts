import { LookupResponse } from './lookup';

export interface StaffAbsenceResponse {
  id: string;
  absenceTypeId: string;
  illnessTypeId?: string | null;
  startDate: string;
  endDate: string;
  isConfidential: boolean;
  notes?: string | null;
}

export interface StaffAbsenceUpsertItem {
  id?: string | null;
  absenceTypeId: string | null;
  illnessTypeId?: string | null;
  startDate: string | null;
  endDate: string | null;
  isConfidential: boolean;
  notes?: string | null;
}

export interface StaffAbsencesResponse {
  absences: StaffAbsenceResponse[];
  absenceTypes: LookupResponse[];
  illnessTypes: LookupResponse[];
}

export interface StaffAbsencesUpsertRequest {
  absences: StaffAbsenceUpsertItem[];
}
