export interface PersonMatchResponse {
  personId: string;
  title?: string | null;
  firstName: string;
  middleName?: string | null;
  lastName: string;
  preferredFirstName?: string | null;
  preferredLastName?: string | null;
  dob?: string | null;
  existingStaffMemberId?: string | null;
  isStaffMember: boolean;
}

export interface StaffMemberCreateForPersonRequest {
  personId: string;
  code: string;
}
