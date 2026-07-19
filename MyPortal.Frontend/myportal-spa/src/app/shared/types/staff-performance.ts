import { LookupResponse } from './lookup';

export interface PerformanceReviewResponse {
  id: string;
  cycleName?: string | null;
  reviewerId?: string | null;
  statusId?: string | null;
  reviewDate?: string | null;
  nextReviewDate?: string | null;
  overallRatingId?: string | null;
  summary?: string | null;
}

export interface PerformanceReviewUpsertItem {
  id?: string | null;
  cycleName?: string | null;
  reviewerId?: string | null;
  statusId?: string | null;
  reviewDate?: string | null;
  nextReviewDate?: string | null;
  overallRatingId?: string | null;
  summary?: string | null;
}

export interface StaffObjectiveResponse {
  id: string;
  reviewId?: string | null;
  categoryId?: string | null;
  title: string;
  description?: string | null;
  successCriteria?: string | null;
  dueDate?: string | null;
  statusId?: string | null;
  progressNotes?: string | null;
}

export interface StaffObjectiveUpsertItem {
  id?: string | null;
  reviewId?: string | null;
  categoryId?: string | null;
  title: string;
  description?: string | null;
  successCriteria?: string | null;
  dueDate?: string | null;
  statusId?: string | null;
  progressNotes?: string | null;
}

export interface StaffObservationResponse {
  id: string;
  date: string;
  observerId: string;
  outcomeId: string;
  focus?: string | null;
  subjectObserved?: string | null;
  strengths?: string | null;
  areasForDevelopment?: string | null;
  notes?: string | null;
}

export interface StaffObservationUpsertItem {
  id?: string | null;
  date: string | null;
  observerId: string | null;
  outcomeId: string | null;
  focus?: string | null;
  subjectObserved?: string | null;
  strengths?: string | null;
  areasForDevelopment?: string | null;
  notes?: string | null;
}

export interface StaffTrainingRecordResponse {
  id: string;
  trainingCourseId: string;
  statusId: string;
  completedDate?: string | null;
  expiryDate?: string | null;
  provider?: string | null;
  hours?: number | null;
  certificateReference?: string | null;
}

export interface StaffTrainingRecordUpsertItem {
  id?: string | null;
  trainingCourseId: string | null;
  statusId: string | null;
  completedDate?: string | null;
  expiryDate?: string | null;
  provider?: string | null;
  hours?: number | null;
  certificateReference?: string | null;
}

export interface StaffPerformanceResponse {
  reviews: PerformanceReviewResponse[];
  objectives: StaffObjectiveResponse[];
  observations: StaffObservationResponse[];
  trainingRecords: StaffTrainingRecordResponse[];

  reviewStatuses: LookupResponse[];
  objectiveStatuses: LookupResponse[];
  objectiveCategories: LookupResponse[];
  outcomes: LookupResponse[];
  staff: LookupResponse[];
  trainingCourses: LookupResponse[];
  trainingStatuses: LookupResponse[];
}

export interface StaffPerformanceUpsertRequest {
  reviews: PerformanceReviewUpsertItem[];
  objectives: StaffObjectiveUpsertItem[];
  observations: StaffObservationUpsertItem[];
  trainingRecords: StaffTrainingRecordUpsertItem[];
}
