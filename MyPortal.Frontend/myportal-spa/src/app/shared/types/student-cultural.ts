import { LookupResponse } from './lookup';

export interface StudentCulturalDetailsResponse {
  ethnicityId?: string | null;
  firstLanguageId?: string | null;
  religionId?: string | null;
  nationalityId?: string | null;
  englishProficiencyId?: string | null;
  englishProficiencyDate?: string | null;
  ethnicities: LookupResponse[];
  languages: LookupResponse[];
  religions: LookupResponse[];
  nationalities: LookupResponse[];
  englishProficiencies: LookupResponse[];
}

export interface StudentCulturalDetailsUpsertRequest {
  ethnicityId?: string | null;
  firstLanguageId?: string | null;
  religionId?: string | null;
  nationalityId?: string | null;
  englishProficiencyId?: string | null;
  englishProficiencyDate?: string | null;
}
