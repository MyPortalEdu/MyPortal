import { LookupResponse } from './lookup';

export interface SenStatusHistoryResponse {
  id: string;
  senStatusId: string;
  startDate: string;
  endDate?: string | null;
}

export interface SenNeedResponse {
  id: string;
  senTypeId: string;
  description?: string | null;
  startDate: string;
  endDate?: string | null;
  rank: number;
}

export interface SenProvisionResponse {
  id: string;
  senProvisionTypeId: string;
  startDate: string;
  endDate?: string | null;
  frequency?: string | null;
  cost?: number | null;
  note: string;
}

export interface SenStatementResponse {
  id: string;
  isEhcp: boolean;
  assessmentRequestDate: string;
  parentConsultDate?: string | null;
  finalisedDate?: string | null;
  ceasedDate?: string | null;
  statutoryAssessmentAgreedId?: string | null;
  statutoryAssessmentOutcomeId?: string | null;
  subjectToTribunal: boolean;
  undergoingMediation: boolean;
  appealNotes?: string | null;
  temporaryDisapplicationSubjects?: string | null;
  permanentDisapplicationSubjects?: string | null;
  localAuthorityId?: string | null;
  comments?: string | null;
}

export interface StudentSenDetailsResponse {
  studentId: string;
  currentSenStatusId?: string | null;
  senStartDate?: string | null;
  senUnitMember: boolean;
  resourcedProvisionMember: boolean;
  statusHistory: SenStatusHistoryResponse[];
  needs: SenNeedResponse[];
  provisions: SenProvisionResponse[];
  statements: SenStatementResponse[];
  senStatuses: LookupResponse[];
  senTypes: LookupResponse[];
  senProvisionTypes: LookupResponse[];
  statutoryAssessmentAgreedOptions: LookupResponse[];
  statutoryAssessmentOutcomeOptions: LookupResponse[];
}

export interface SetSenStatusRequest {
  senStatusId: string;
  startDate: string;
}

export interface SenNeedUpsertRequest {
  id?: string | null;
  senTypeId: string;
  description?: string | null;
  startDate: string;
  endDate?: string | null;
  rank: number;
}

export interface SenProvisionUpsertRequest {
  id?: string | null;
  senProvisionTypeId: string;
  startDate: string;
  endDate?: string | null;
  frequency?: string | null;
  cost?: number | null;
  note: string;
}

export interface SenStatementUpsertRequest {
  id?: string | null;
  isEhcp: boolean;
  assessmentRequestDate: string;
  parentConsultDate?: string | null;
  finalisedDate?: string | null;
  ceasedDate?: string | null;
  statutoryAssessmentAgreedId?: string | null;
  statutoryAssessmentOutcomeId?: string | null;
  subjectToTribunal: boolean;
  undergoingMediation: boolean;
  appealNotes?: string | null;
  temporaryDisapplicationSubjects?: string | null;
  permanentDisapplicationSubjects?: string | null;
  localAuthorityId?: string | null;
  comments?: string | null;
}
