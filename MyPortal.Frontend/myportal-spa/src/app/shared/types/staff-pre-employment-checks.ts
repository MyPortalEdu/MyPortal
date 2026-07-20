import { LookupResponse } from './lookup';

export interface DbsCheckResponse {
  id: string;
  dbsCheckTypeId: string;
  certificateNumber: string;
  issueDate: string;
  expiryDate?: string | null;
  updateServiceEnrolled: boolean;
  lastUpdateServiceCheck?: string | null;
  notes?: string | null;
}

export interface DbsCheckUpsertItem {
  id?: string | null;
  dbsCheckTypeId: string | null;
  certificateNumber: string;
  issueDate: string | null;
  expiryDate?: string | null;
  updateServiceEnrolled: boolean;
  lastUpdateServiceCheck?: string | null;
  notes?: string | null;
}

export interface RightToWorkCheckResponse {
  id: string;
  documentTypeId: string;
  documentNumber?: string | null;
  checkDate: string;
  documentExpiryDate?: string | null;
  followUpDate?: string | null;
  notes?: string | null;
}

export interface RightToWorkCheckUpsertItem {
  id?: string | null;
  documentTypeId: string | null;
  documentNumber?: string | null;
  checkDate: string | null;
  documentExpiryDate?: string | null;
  followUpDate?: string | null;
  notes?: string | null;
}

export interface StaffReferenceResponse {
  id: string;
  referenceTypeId?: string | null;
  referenceStatusId?: string | null;
  refereeName: string;
  refereeOrganisation?: string | null;
  refereeEmail?: string | null;
  requestedDate?: string | null;
  receivedDate?: string | null;
  notes?: string | null;
}

export interface StaffReferenceUpsertItem {
  id?: string | null;
  referenceTypeId?: string | null;
  referenceStatusId?: string | null;
  refereeName: string;
  refereeOrganisation?: string | null;
  refereeEmail?: string | null;
  requestedDate?: string | null;
  receivedDate?: string | null;
  notes?: string | null;
}

export interface StaffOverseasCheckResponse {
  id: string;
  nationalityId: string;
  checkedDate?: string | null;
  isClear: boolean;
  notes?: string | null;
}

export interface StaffOverseasCheckUpsertItem {
  id?: string | null;
  nationalityId: string | null;
  checkedDate?: string | null;
  isClear: boolean;
  notes?: string | null;
}

export interface StaffPreEmploymentChecksResponse {
  identityCheckedDate?: string | null;
  prohibitionFromTeachingCheckedDate?: string | null;
  prohibitionFromManagementCheckedDate?: string | null;
  childcareDisqualificationCheckedDate?: string | null;
  medicalFitnessCheckedDate?: string | null;
  qualificationsVerifiedDate?: string | null;
  notes?: string | null;

  dbsChecks: DbsCheckResponse[];
  rightToWorkChecks: RightToWorkCheckResponse[];
  references: StaffReferenceResponse[];
  overseasChecks: StaffOverseasCheckResponse[];

  dbsCheckTypes: LookupResponse[];
  rightToWorkDocumentTypes: LookupResponse[];
  referenceTypes: LookupResponse[];
  referenceStatuses: LookupResponse[];
  countries: LookupResponse[];
}

export interface StaffPreEmploymentChecksUpsertRequest {
  identityCheckedDate?: string | null;
  prohibitionFromTeachingCheckedDate?: string | null;
  prohibitionFromManagementCheckedDate?: string | null;
  childcareDisqualificationCheckedDate?: string | null;
  medicalFitnessCheckedDate?: string | null;
  qualificationsVerifiedDate?: string | null;
  notes?: string | null;

  dbsChecks: DbsCheckUpsertItem[];
  rightToWorkChecks: RightToWorkCheckUpsertItem[];
  references: StaffReferenceUpsertItem[];
  overseasChecks: StaffOverseasCheckUpsertItem[];
}
