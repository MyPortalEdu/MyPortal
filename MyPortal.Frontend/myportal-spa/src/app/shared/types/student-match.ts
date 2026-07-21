export interface StudentMatchResponse {
  personId: string;
  title?: string | null;
  firstName: string;
  middleName?: string | null;
  lastName: string;
  preferredFirstName?: string | null;
  preferredLastName?: string | null;
  dob?: string | null;
  existingStudentId?: string | null;
  isStudent: boolean;
}

export interface StudentCreateForPersonRequest {
  personId: string;
}
