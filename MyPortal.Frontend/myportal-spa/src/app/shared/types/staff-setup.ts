import { LookupResponse } from './lookup';

export interface VacancyResponse {
  id: string;
  startDate: string;
  endDate?: string | null;
  isAdvertised: boolean;
  isTemporarilyFilled: boolean;
  subjectId?: string | null;
  notes?: string | null;
}

export interface VacancyUpsertItem {
  id?: string | null;
  startDate: string | null;
  endDate?: string | null;
  isAdvertised: boolean;
  isTemporarilyFilled: boolean;
  subjectId?: string | null;
  notes?: string | null;
}

export interface PostResponse {
  id: string;
  reference: string;
  description: string;
  postCategoryId?: string | null;
  serviceTermId?: string | null;
  swrPostCode?: string | null;
  establishedFte?: number | null;
  contractCount: number;
  isVacant: boolean;
  vacancies: VacancyResponse[];
}

export interface PostUpsertRequest {
  reference: string;
  description: string;
  postCategoryId?: string | null;
  serviceTermId?: string | null;
  swrPostCode?: string | null;
  establishedFte?: number | null;
  vacancies: VacancyUpsertItem[];
}

export interface PostsResponse {
  posts: PostResponse[];
  postCategories: LookupResponse[];
  serviceTerms: LookupResponse[];
  subjects: LookupResponse[];
  canEdit: boolean;
}
