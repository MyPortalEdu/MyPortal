import { LookupResponse } from './lookup';
import {
  PersonEmailResponse,
  PersonEmailUpsertItem,
  PersonPhoneResponse,
  PersonPhoneUpsertItem,
} from './staff-contact-details';

export interface StaffNextOfKinResponse {
  id: string;
  contactId: string;
  personId: string;
  title?: string | null;
  firstName: string;
  middleName?: string | null;
  lastName: string;
  relationshipTypeId?: string | null;
  contactOrder: number;
  notes?: string | null;
  phones: PersonPhoneResponse[];
  emails: PersonEmailResponse[];
}

export interface StaffNextOfKinAreaResponse {
  contacts: StaffNextOfKinResponse[];
  relationshipTypes: LookupResponse[];
  phoneTypes: LookupResponse[];
  emailTypes: LookupResponse[];
}

export interface StaffNextOfKinUpsertItem {
  id?: string | null;
  personId?: string | null;
  title?: string | null;
  firstName: string;
  middleName?: string | null;
  lastName: string;
  gender?: string | null;
  relationshipTypeId?: string | null;
  contactOrder: number;
  notes?: string | null;
  phones: PersonPhoneUpsertItem[];
  emails: PersonEmailUpsertItem[];
}

export interface StaffNextOfKinAreaUpsertRequest {
  contacts: StaffNextOfKinUpsertItem[];
}
