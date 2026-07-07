import { StaffStatus } from './staff-member-header';

// Mirrors MyPortal.Contracts.Models.People.StaffMemberSummaryResponse.
// `id` is the StaffMember id (the key for the staff profile / CRUD endpoints);
// `personId` is the underlying Person id — the value to write into person-FK
// columns like HeadTeacherId.
export interface StaffMemberSummaryResponse {
  id: string;
  personId: string;
  code: string;
  title?: string | null;
  firstName: string;
  lastName: string;
  preferredFirstName?: string | null;
  preferredLastName?: string | null;
  // Employment-derived lifecycle badge (never 'Archived' in the list).
  status: StaffStatus;
}
