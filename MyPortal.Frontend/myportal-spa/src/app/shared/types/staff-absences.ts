import { LookupResponse } from './lookup';

// Mirrors MyPortal.Contracts.Models.People.StaffAbsenceResponse.
export interface StaffAbsenceResponse {
  id: string;
  absenceTypeId: string;
  illnessTypeId?: string | null;
  startDate: string;
  endDate: string;
  isConfidential: boolean;
  notes?: string | null;
}

// Mirrors MyPortal.Contracts.Models.People.StaffAbsenceUpsertItem — null id is a
// new row; populated id updates; omitted rows are removed server-side.
export interface StaffAbsenceUpsertItem {
  id?: string | null;
  absenceTypeId: string | null;
  illnessTypeId?: string | null;
  startDate: string | null;
  endDate: string | null;
  isConfidential: boolean;
  notes?: string | null;
}

// Mirrors MyPortal.Contracts.Models.People.StaffAbsencesResponse.
export interface StaffAbsencesResponse {
  absences: StaffAbsenceResponse[];
  absenceTypes: LookupResponse[];
  illnessTypes: LookupResponse[];
}

// Mirrors MyPortal.Contracts.Models.People.StaffAbsencesUpsertRequest.
export interface StaffAbsencesUpsertRequest {
  absences: StaffAbsenceUpsertItem[];
}
