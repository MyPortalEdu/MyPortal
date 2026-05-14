// Wire shapes for the school details edit page. Mirrors
// MyPortal.Contracts.Models.School.* on the backend.

export interface SchoolDetailsResponse {
  id: string;
  agencyId: string;
  name: string;
  website?: string | null;
  agencyTypeId: string;
  urn: string;
  uprn: string;
  establishmentNumber: number;
  localAuthorityId?: string | null;
  localAuthorityName?: string | null;
  schoolPhaseId: string;
  phase: string;
  schoolTypeId: string;
  type: string;
  governanceTypeId: string;
  intakeTypeId: string;
  headTeacherId?: string | null;
  headTeacherFullName?: string | null;
  isLocal: boolean;
}

export interface SchoolUpsertRequest {
  name: string;
  website?: string | null;
  expectedVersion: number;
  localAuthorityId?: string | null;
  establishmentNumber: number;
  urn: string;
  uprn: string;
  schoolPhaseId: string;
  schoolTypeId: string;
  governanceTypeId: string;
  intakeTypeId: string;
  headTeacherId?: string | null;
}

export interface LocalAuthoritySummaryResponse {
  id: string;
  leaCode: number;
  name: string;
}
