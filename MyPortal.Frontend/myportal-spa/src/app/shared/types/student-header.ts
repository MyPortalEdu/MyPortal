export type StudentStatus = 'Active' | 'Future' | 'Leaver' | 'None' | 'Archived';

export interface StudentHeaderResponse {
  id: string;
  personId: string;
  admissionNumber: number;
  displayName: string;
  preferredName?: string | null;
  photoId?: string | null;
  status: StudentStatus;
}
