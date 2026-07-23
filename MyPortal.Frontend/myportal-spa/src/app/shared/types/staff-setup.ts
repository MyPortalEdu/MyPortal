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

export interface ServiceTermSchemeItem {
  superannuationSchemeId: string;
  isMain: boolean;
}

export interface ServiceTermResponse {
  id: string;
  code: string;
  description: string;
  active: boolean;
  isTeacher: boolean;
  salaried: boolean;
  spinalProgression: boolean;
  singlePaySpine: boolean;
  termTimeOnlyPossible: boolean;
  incrementMonth?: number | null;
  incrementDay?: number | null;
  minimumPoint?: number | null;
  maximumPoint?: number | null;
  pointInterval?: number | null;
  hoursPerWeek?: number | null;
  weeksPerYear?: number | null;
  contractCount: number;
  postCount: number;
  superannuationSchemes: ServiceTermSchemeItem[];
}

export interface ServiceTermUpsertRequest {
  code: string;
  description: string;
  active: boolean;
  isTeacher: boolean;
  salaried: boolean;
  spinalProgression: boolean;
  termTimeOnlyPossible: boolean;
  incrementMonth?: number | null;
  incrementDay?: number | null;
  hoursPerWeek?: number | null;
  weeksPerYear?: number | null;
  superannuationSchemes: ServiceTermSchemeItem[];
}

export interface ServiceTermsResponse {
  serviceTerms: ServiceTermResponse[];
  superannuationSchemes: LookupResponse[];
  canEdit: boolean;
}

export interface PayScalePointItem {
  id: string;
  code: string;
  description: string;
  pointValue: number;
  contractCount: number;
}

export interface PointSalaryItem {
  pointValue: number;
  payZoneId: string;
  annualSalary: number;
}

export interface PayScaleItem {
  id: string;
  code: string;
  description: string;
  active: boolean;
  minimumPoint?: number | null;
  maximumPoint?: number | null;
  pointInterval?: number | null;
  contractCount: number;
  points: PayScalePointItem[];
}

export interface PayScaleSalariesItem {
  payScaleId: string;
  salaries: PointSalaryItem[];
}

export interface PayScaleGenerationItem {
  effectiveFrom: string;
  effectiveTo?: string | null;
  rateCount: number;
  isCurrent: boolean;
}

export interface ServiceTermPayResponse {
  serviceTermId: string;
  singlePaySpine: boolean;
  minimumPoint?: number | null;
  maximumPoint?: number | null;
  pointInterval?: number | null;
  spinePoints: PayScalePointItem[];
  scales: PayScaleItem[];
  payZones: LookupResponse[];
  localPayZoneId?: string | null;
  generations: PayScaleGenerationItem[];
  selectedEffectiveFrom?: string | null;
  spineSalaries: PointSalaryItem[];
  scaleSalaries: PayScaleSalariesItem[];
  canEdit: boolean;
}

export interface PayScaleUpsertItem {
  id?: string | null;
  code: string;
  description: string;
  active: boolean;
  minimumPoint?: number | null;
  maximumPoint?: number | null;
  pointInterval?: number | null;
  salaries: PointSalaryItem[];
}

export interface ServiceTermPayUpsertRequest {
  effectiveFrom: string;
  singlePaySpine: boolean;
  minimumPoint?: number | null;
  maximumPoint?: number | null;
  pointInterval?: number | null;
  scales: PayScaleUpsertItem[];
  spineSalaries: PointSalaryItem[];
}

export interface PayAwardScaleOverride {
  payScaleId: string;
  percentage: number;
}

export interface PayAwardRequest {
  effectiveFrom: string;
  sourceEffectiveFrom: string;
  defaultPercentage: number;
  scaleOverrides: PayAwardScaleOverride[];
}

export interface PayAwardPreviewItem {
  payScalePointId: string;
  payScaleId?: string | null;
  pointValue: number;
  payZoneId: string;
  previousAnnualSalary: number;
  annualSalary: number;
}

export interface PayAwardPreviewResponse {
  effectiveFrom: string;
  sourceEffectiveFrom: string;
  rates: PayAwardPreviewItem[];
}

export interface IncrementPreviewRequest {
  effectiveFrom: string;
}

export interface IncrementApplyRequest {
  effectiveFrom: string;
  contractIds: string[];
}

export interface IncrementItem {
  contractId: string;
  staffMemberId: string;
  staffName: string;
  staffCode: string;
  scaleCode: string;
  currentPointCode: string;
  currentPointValue: number;
  currentSalary?: number | null;
  nextPointId?: string | null;
  nextPointCode?: string | null;
  nextPointValue?: number | null;
  newSalary?: number | null;
  atMaximum: boolean;
  missingRate: boolean;
  alreadyIncremented: boolean;
}

export interface IncrementPreviewResponse {
  serviceTermId: string;
  effectiveFrom: string;
  eligibleCount: number;
  items: IncrementItem[];
}

export interface IncrementScheduleRequest {
  effectiveFrom: string;
}

export interface ScheduledIncrement {
  id: string;
  serviceTermId: string;
  serviceTermCode: string;
  serviceTermDescription: string;
  effectiveDate: string;
  status: string;
  completedAt?: string | null;
  appliedCount?: number | null;
  scheduledBy?: string | null;
  isDue: boolean;
}
