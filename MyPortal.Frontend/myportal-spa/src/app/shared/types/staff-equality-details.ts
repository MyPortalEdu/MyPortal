import { LookupResponse } from './lookup';

export interface StaffDisabilityItem {
  disabilityId: string | null;
  dateAdvised?: string | null;
  isLongTerm: boolean;
  affectsWorkingAbility: boolean;
  assistanceRequired?: string | null;
}

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
  declaredDisabilities: StaffDisabilityItem[];
  impairmentEffectId?: string | null;
  disabilityNumber?: string | null;

  ethnicities: LookupResponse[];
  nationalities: LookupResponse[];
  languages: LookupResponse[];
  maritalStatuses: LookupResponse[];
  religions: LookupResponse[];
  sexualOrientations: LookupResponse[];
  genderIdentities: LookupResponse[];
  disabilities: LookupResponse[];
  impairmentEffects: LookupResponse[];
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
  declaredDisabilities: StaffDisabilityItem[];
  impairmentEffectId?: string | null;
  disabilityNumber?: string | null;
}
