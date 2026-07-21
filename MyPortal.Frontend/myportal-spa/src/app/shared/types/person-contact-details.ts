import { LookupResponse } from './lookup';

export interface PersonEmailResponse {
  id: string;
  typeId: string;
  address: string;
  isMain: boolean;
  notes?: string | null;
}

export interface PersonPhoneResponse {
  id: string;
  typeId: string;
  number: string;
  isMain: boolean;
}

export interface PersonContactDetailsResponse {
  emails: PersonEmailResponse[];
  phones: PersonPhoneResponse[];
  emailTypes: LookupResponse[];
  phoneTypes: LookupResponse[];
}

export interface PersonEmailUpsertItem {
  id?: string | null;
  typeId: string;
  address: string;
  isMain: boolean;
  notes?: string | null;
}

export interface PersonPhoneUpsertItem {
  id?: string | null;
  typeId: string;
  number: string;
  isMain: boolean;
}

export interface PersonContactDetailsUpsertRequest {
  emails: PersonEmailUpsertItem[];
  phones: PersonPhoneUpsertItem[];
}
