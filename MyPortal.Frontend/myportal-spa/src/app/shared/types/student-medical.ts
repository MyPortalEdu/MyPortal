import { LookupResponse } from './lookup';

export interface PersonConditionItem {
  medicalConditionId: string;
  requiresMedication: boolean;
  medication?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  infoReceivedDate?: string | null;
  notes?: string | null;
}

export interface StudentMedicalDetailsResponse {
  hasMedicalNeeds: boolean;
  conditions: PersonConditionItem[];
  dietaryRequirementIds: string[];
  disabilityIds: string[];
  medicalConditions: LookupResponse[];
  dietaryRequirements: LookupResponse[];
  disabilities: LookupResponse[];
}

export interface StudentMedicalDetailsUpsertRequest {
  hasMedicalNeeds: boolean;
  conditions: PersonConditionItem[];
  dietaryRequirementIds: string[];
  disabilityIds: string[];
}
