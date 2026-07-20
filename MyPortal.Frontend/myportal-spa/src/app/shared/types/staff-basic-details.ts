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
}

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
  code: string;
}
