import { StudentStatus } from './student-header';

export interface StudentSummaryResponse {
  id: string;
  personId: string;
  admissionNumber: number;
  title?: string | null;
  firstName: string;
  lastName: string;
  preferredFirstName?: string | null;
  preferredLastName?: string | null;
  status: StudentStatus;
}
