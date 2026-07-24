import { LookupResponse } from './lookup';

export interface StaffDirectReportResponse {
  staffMemberId: string;
  displayName: string;
  code: string;
}

export interface StaffLineManagerHistoryResponse {
  id: string;
  lineManagerId: string;
  lineManagerName?: string | null;
  lineManagerCode?: string | null;
  startDate: string;
  endDate?: string | null;
}

export interface StaffManagementResponse {
  lineManagerId?: string | null;
  lineManagerName?: string | null;
  lineManagerCode?: string | null;
  directReports: StaffDirectReportResponse[];
  history: StaffLineManagerHistoryResponse[];
  canEdit: boolean;
  managerOptions: LookupResponse[];
}

export interface SetStaffLineManagerRequest {
  lineManagerId: string | null;
}
