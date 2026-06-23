// Mirrors MyPortal.Contracts.Models.People.StaffBasicDetailsResponse — the
// basic-details area of the staff profile: person bio (excluding equality
// fields) + the staff code.
export interface StaffBasicDetailsResponse {
  id: string;
  personId: string;
  code: string;
  title?: string | null;
  firstName: string;
  middleName?: string | null;
  lastName: string;
  preferredFirstName?: string | null;
  preferredLastName?: string | null;
  photoId?: string | null;
  gender: string;
  dob?: string | null;
  deceased?: string | null;
  nationalityId?: string | null;
  firstLanguageId?: string | null;
  maritalStatusId?: string | null;
}

// Mirrors MyPortal.Contracts.Models.People.StaffBasicDetailsUpsertRequest —
// the write payload for the basic-details area. Touches only basic bio + Code.
export interface StaffBasicDetailsUpsertRequest {
  title?: string | null;
  firstName: string;
  middleName?: string | null;
  lastName: string;
  preferredFirstName?: string | null;
  preferredLastName?: string | null;
  photoId?: string | null;
  gender: string;
  dob?: string | null;
  deceased?: string | null;
  nationalityId?: string | null;
  firstLanguageId?: string | null;
  maritalStatusId?: string | null;
  code: string;
}
