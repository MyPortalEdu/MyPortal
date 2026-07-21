import { LookupResponse } from './lookup';

export interface StudentRegistrationDetailsResponse {
  id: string;
  admissionNumber: number;
  enrolmentStatusId?: string | null;
  boarderStatusId?: string | null;
  dateStarting?: string | null;
  upn?: string | null;
  formerUpn?: string | null;
  upnUnknownReasonId?: string | null;
  uln?: string | null;
  laChildId?: string | null;
  isPartTime: boolean;
  enrolmentStatuses: LookupResponse[];
  boarderStatuses: LookupResponse[];
  upnUnknownReasons: LookupResponse[];
}

export interface GeneratedUpnResponse {
  upn: string;
}

export interface StudentRegistrationDetailsUpsertRequest {
  enrolmentStatusId?: string | null;
  boarderStatusId?: string | null;
  dateStarting?: string | null;
  upn?: string | null;
  formerUpn?: string | null;
  upnUnknownReasonId?: string | null;
  uln?: string | null;
  laChildId?: string | null;
  isPartTime: boolean;
}
