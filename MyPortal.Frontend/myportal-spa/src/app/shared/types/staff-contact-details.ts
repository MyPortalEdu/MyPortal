// Mirrors MyPortal.Contracts.Models.People.* for the Contact Details area —
// a staff member's owned emails and phone numbers, plus the type options the
// editor's dropdowns need. Addresses are shared and land in a later slice.
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

export interface StaffContactDetailsResponse {
  emails: PersonEmailResponse[];
  phones: PersonPhoneResponse[];
  emailTypes: LookupResponse[];
  phoneTypes: LookupResponse[];
}

// Write payloads. A null/absent `id` is a new row; the server diffs the lists
// against what's stored (insert / update / soft-delete the dropped ones).
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

export interface StaffContactDetailsUpsertRequest {
  emails: PersonEmailUpsertItem[];
  phones: PersonPhoneUpsertItem[];
}
