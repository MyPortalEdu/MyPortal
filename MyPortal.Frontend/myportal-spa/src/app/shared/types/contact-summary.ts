export interface ContactSummaryResponse {
  id: string;
  personId: string;
  title?: string | null;
  firstName: string;
  lastName: string;
  preferredFirstName?: string | null;
  preferredLastName?: string | null;
  jobTitle?: string | null;
  linkedStudentCount: number;
}
