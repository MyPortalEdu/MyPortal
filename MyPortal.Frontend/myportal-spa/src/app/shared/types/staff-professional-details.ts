import { LookupResponse } from './lookup';

// Mirrors MyPortal.Contracts.Models.People.StaffQualificationResponse — one
// structured qualification held by a staff member.
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

// Mirrors MyPortal.Contracts.Models.People.StaffQualificationUpsertItem — a null
// id is a new row; a populated id updates the existing one; rows omitted are
// soft-deleted server-side.
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

// Mirrors MyPortal.Contracts.Models.People.StaffProfessionalDetailsResponse —
// teaching status + QTS/induction fields, the structured qualifications, and the
// option lists for every picker.
export interface StaffProfessionalDetailsResponse {
  isTeachingStaff: boolean;
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
  inductionStatuses: LookupResponse[];
  qualificationLevels: LookupResponse[];
  classesOfDegree: LookupResponse[];
}

// Mirrors MyPortal.Contracts.Models.People.StaffProfessionalDetailsUpsertRequest —
// the write payload (no option lists).
export interface StaffProfessionalDetailsUpsertRequest {
  isTeachingStaff: boolean;
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
