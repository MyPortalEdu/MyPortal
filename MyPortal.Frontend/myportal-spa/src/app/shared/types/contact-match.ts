export interface ContactMatchResponse {
  personId: string;
  title?: string | null;
  firstName: string;
  middleName?: string | null;
  lastName: string;
  preferredFirstName?: string | null;
  preferredLastName?: string | null;
  dob?: string | null;
  existingContactId?: string | null;
  isContact: boolean;
}

export interface ContactCreateForPersonRequest {
  personId: string;
}
