import { LookupResponse } from './lookup';

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
