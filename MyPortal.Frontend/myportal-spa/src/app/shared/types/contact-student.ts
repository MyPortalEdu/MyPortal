export interface ContactStudentResponse {
  id: string;
  studentId: string;
  studentName: string;
  admissionNumber: number;
  relationshipTypeId: string;
  relationshipTypeName?: string | null;
  hasCorrespondence: boolean;
  hasParentalResponsibility: boolean;
  hasPupilReport: boolean;
  hasCourtOrder: boolean;
  contactOrder: number;
}
