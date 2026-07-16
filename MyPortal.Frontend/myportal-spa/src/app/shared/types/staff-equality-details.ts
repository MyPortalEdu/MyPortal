import { LookupResponse } from './lookup';

// Mirrors MyPortal.Contracts.Models.People.StaffEqualityDetailsResponse — the
// Equality & Diversity area: person equality single-selects + the staff
// disability declaration, plus the option lists for every picker.
export interface StaffEqualityDetailsResponse {
  ethnicityId?: string | null;
  nationalityId?: string | null;
  firstLanguageId?: string | null;
  maritalStatusId?: string | null;
  religionId?: string | null;
  sexualOrientationId?: string | null;
  genderIdentityId?: string | null;

  hasDisability: boolean;
  disabilityDetails?: string | null;
  disabilityIds: string[];

  ethnicities: LookupResponse[];
  nationalities: LookupResponse[];
  languages: LookupResponse[];
  maritalStatuses: LookupResponse[];
  religions: LookupResponse[];
  sexualOrientations: LookupResponse[];
  genderIdentities: LookupResponse[];
  disabilities: LookupResponse[];
}

// Mirrors MyPortal.Contracts.Models.People.StaffEqualityDetailsUpsertRequest —
// the write payload (no option lists).
export interface StaffEqualityDetailsUpsertRequest {
  ethnicityId?: string | null;
  nationalityId?: string | null;
  firstLanguageId?: string | null;
  maritalStatusId?: string | null;
  religionId?: string | null;
  sexualOrientationId?: string | null;
  genderIdentityId?: string | null;

  hasDisability: boolean;
  disabilityDetails?: string | null;
  disabilityIds: string[];
}
