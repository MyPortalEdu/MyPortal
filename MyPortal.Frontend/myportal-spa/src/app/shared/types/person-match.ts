// Mirrors MyPortal.Contracts.Models.People.PersonMatchResponse — a candidate
// existing Person surfaced by the "new staff member" search. `existingStaffMemberId`
// /`isStaffMember` are set when the person already holds an active staff record, so
// the UI can block a duplicate and deep-link to the existing profile instead.
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

// Mirrors MyPortal.Contracts.Models.People.StaffMemberCreateForPersonRequest —
// attaches a staff role to an existing Person. Only the staff code is supplied;
// the person's bio is left untouched.
export interface StaffMemberCreateForPersonRequest {
  personId: string;
  code: string;
}
