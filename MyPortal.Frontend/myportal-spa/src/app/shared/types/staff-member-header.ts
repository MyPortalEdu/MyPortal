// Mirrors MyPortal.Contracts.Models.People.StaffStatus — lifecycle badge derived
// server-side from the member's employment spells (Archived wins if soft-deleted).
export type StaffStatus = 'Active' | 'Future' | 'Leaver' | 'None' | 'Archived';

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
