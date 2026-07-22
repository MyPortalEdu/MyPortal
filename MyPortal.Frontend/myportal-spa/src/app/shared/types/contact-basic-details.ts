export interface ContactBasicDetailsResponse {
  id: string;
  personId: string;
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
  parentalBallot: boolean;
  placeOfWork?: string | null;
  jobTitle?: string | null;
  niNumber?: string | null;
}

export interface ContactBasicDetailsUpsertRequest {
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
  parentalBallot: boolean;
  placeOfWork?: string | null;
  jobTitle?: string | null;
  niNumber?: string | null;
}
