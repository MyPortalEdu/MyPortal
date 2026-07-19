import { StaffStatus } from './staff-member-header';

export interface StaffMemberSummaryResponse {
  id: string;
  personId: string;
  code: string;
  title?: string | null;
  firstName: string;
  lastName: string;
  preferredFirstName?: string | null;
  preferredLastName?: string | null;
  status: StaffStatus;
}
