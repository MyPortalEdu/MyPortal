export type StaffStatus = 'Active' | 'Future' | 'Leaver' | 'None' | 'Archived';

export type StaffRelationship = 'Unrelated' | 'LineManaged' | 'Self';

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
