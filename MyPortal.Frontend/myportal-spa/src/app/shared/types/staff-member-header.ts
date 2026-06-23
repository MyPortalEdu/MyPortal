// Mirrors MyPortal.Contracts.Models.People.StaffStatus.
export type StaffStatus = 'Active' | 'Inactive';

// Mirrors MyPortal.Contracts.Models.People.StaffRelationship — the viewer's
// relationship to a staff member, used by the FE to compose sidebar visibility
// alongside the permission claim.
export type StaffRelationship = 'Unrelated' | 'LineManaged' | 'Self';

// Mirrors MyPortal.Contracts.Models.People.StaffMemberHeaderResponse — identity
// row at the top of the profile page. Roles, line manager, start date, FTE,
// site, and the Leaver status state land with their respective section slices.
export interface StaffMemberHeaderResponse {
  id: string;
  personId: string;
  code: string;
  displayName: string;
  preferredName?: string | null;
  photoId?: string | null;
  status: StaffStatus;
  relationship: StaffRelationship;
}
