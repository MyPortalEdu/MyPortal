import { LookupResponse } from './lookup';

export interface StaffResponsibilityResponse {
  id: string;
  responsibilityTypeId: string;
  startDate: string;
  endDate?: string | null;
  notes?: string | null;
}

export interface StaffResponsibilitiesResponse {
  responsibilities: StaffResponsibilityResponse[];
  responsibilityTypes: LookupResponse[];
}

export interface StaffResponsibilityUpsertItem {
  id?: string | null;
  responsibilityTypeId: string | null;
  startDate: string | null;
  endDate?: string | null;
  notes?: string | null;
}

export interface StaffResponsibilitiesUpsertRequest {
  responsibilities: StaffResponsibilityUpsertItem[];
}
