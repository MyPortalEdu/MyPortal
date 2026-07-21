export interface StudentBasicDetailsResponse {
  id: string;
  personId: string;
  admissionNumber: number;
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

export interface StudentBasicDetailsUpsertRequest {
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
