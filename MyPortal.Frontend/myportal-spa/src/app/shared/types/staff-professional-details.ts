import { LookupResponse } from './lookup';

export interface StaffQualificationResponse {
  id: string;
  qualificationLevelId?: string | null;
  title: string;
  subject?: string | null;
  awardingBody?: string | null;
  grade?: string | null;
  classOfDegreeId?: string | null;
  yearAwarded?: number | null;
}

export interface StaffQualificationUpsertItem {
  id?: string | null;
  qualificationLevelId?: string | null;
  title: string;
  subject?: string | null;
  awardingBody?: string | null;
  grade?: string | null;
  classOfDegreeId?: string | null;
  yearAwarded?: number | null;
}

export interface StaffProfessionalDetailsResponse {
  isTeachingStaff: boolean;
  teacherCategoryId?: string | null;
  teacherStatusId?: string | null;
  eligibleForSwr: boolean;
  hasQts: boolean;
  hasHlta: boolean;
  hasQtls: boolean;
  hasEyts: boolean;
  isSeniorLeadership: boolean;

  teacherReferenceNumber?: string | null;
  qtsRouteId?: string | null;
  qtsAwardedDate?: string | null;

  inductionStatusId?: string | null;
  inductionStartDate?: string | null;
  inductionCompletedDate?: string | null;

  qualificationsSummary?: string | null;

  qualifications: StaffQualificationResponse[];

  qtsRoutes: LookupResponse[];
  teacherCategories: LookupResponse[];
  teacherStatuses: LookupResponse[];
  inductionStatuses: LookupResponse[];
  qualificationLevels: LookupResponse[];
  classesOfDegree: LookupResponse[];
}

export interface StaffProfessionalDetailsUpsertRequest {
  isTeachingStaff: boolean;
  teacherCategoryId?: string | null;
  teacherStatusId?: string | null;
  eligibleForSwr: boolean;
  hasQts: boolean;
  hasHlta: boolean;
  hasQtls: boolean;
  hasEyts: boolean;
  isSeniorLeadership: boolean;

  teacherReferenceNumber?: string | null;
  qtsRouteId?: string | null;
  qtsAwardedDate?: string | null;

  inductionStatusId?: string | null;
  inductionStartDate?: string | null;
  inductionCompletedDate?: string | null;

  qualificationsSummary?: string | null;

  qualifications: StaffQualificationUpsertItem[];
}
