import { LookupResponse } from './lookup';

export interface CareEpisodeResponse {
  id: string;
  caringAuthorityId: string;
  livingArrangementId?: string | null;
  startDate: string;
  endDate?: string | null;
  comment?: string | null;
}

export interface PepContributorResponse {
  id: string;
  personId: string;
  personName: string;
}

export interface PepResponse {
  id: string;
  startDate: string;
  endDate?: string | null;
  comment?: string | null;
  contributors: PepContributorResponse[];
}

export interface ChildProtectionPlanResponse {
  id: string;
  localAuthorityId?: string | null;
  startDate: string;
  endDate?: string | null;
  comment?: string | null;
}

export interface StudentWelfareDetailsResponse {
  studentId: string;
  postLookedAfterArrangementId?: string | null;
  serviceChildIndicatorId?: string | null;
  youngCarerIndicatorId?: string | null;
  kinshipCareIndicatorId?: string | null;
  careEpisodes: CareEpisodeResponse[];
  peps: PepResponse[];
  childProtectionPlans: ChildProtectionPlanResponse[];
  livingArrangements: LookupResponse[];
  caringAuthorities: LookupResponse[];
  postLookedAfterArrangements: LookupResponse[];
  serviceChildIndicators: LookupResponse[];
  youngCarerIndicators: LookupResponse[];
  kinshipCareIndicators: LookupResponse[];
}

export interface WelfareIndicatorsUpsertRequest {
  postLookedAfterArrangementId?: string | null;
  serviceChildIndicatorId?: string | null;
  youngCarerIndicatorId?: string | null;
  kinshipCareIndicatorId?: string | null;
}

export interface CareEpisodeUpsertRequest {
  id?: string | null;
  caringAuthorityId: string;
  livingArrangementId?: string | null;
  startDate: string;
  endDate?: string | null;
  comment?: string | null;
}

export interface PepUpsertRequest {
  id?: string | null;
  startDate: string;
  endDate?: string | null;
  comment?: string | null;
  contributorPersonIds: string[];
}

export interface ChildProtectionPlanUpsertRequest {
  id?: string | null;
  localAuthorityId?: string | null;
  startDate: string;
  endDate?: string | null;
  comment?: string | null;
}
