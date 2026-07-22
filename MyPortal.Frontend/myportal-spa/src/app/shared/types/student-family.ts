import { LookupResponse } from './lookup';

export interface StudentContactRelationshipResponse {
  id: string;
  contactId: string;
  contactName: string;
  jobTitle?: string | null;
  relationshipTypeId: string;
  relationshipTypeName?: string | null;
  hasCorrespondence: boolean;
  hasParentalResponsibility: boolean;
  hasPupilReport: boolean;
  hasCourtOrder: boolean;
  contactOrder: number;
}

export interface SiblingResponse {
  id: string;
  admissionNumber: number;
  displayName: string;
}

export interface StudentFamilyResponse {
  contacts: StudentContactRelationshipResponse[];
  siblings: SiblingResponse[];
  relationshipTypes: LookupResponse[];
}

export interface StudentContactRelationshipUpsertRequest {
  contactId: string;
  relationshipTypeId: string;
  hasCorrespondence: boolean;
  hasParentalResponsibility: boolean;
  hasPupilReport: boolean;
  hasCourtOrder: boolean;
  contactOrder: number;
}
