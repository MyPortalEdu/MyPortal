export type ComplianceKind = 'Expired' | 'ExpiringSoon' | 'Missing';

export type ComplianceCategory =
  | 'Dbs'
  | 'RightToWork'
  | 'Training'
  | 'Contract'
  | 'PreEmployment';

export interface ComplianceItem {
  staffMemberId: string;
  staffName: string;
  staffCode: string;
  category: ComplianceCategory | string;
  detail: string;
  dueDate?: string | null;
  kind: ComplianceKind;
}

export interface StaffComplianceDashboard {
  horizonDays: number;
  expiredCount: number;
  expiringSoonCount: number;
  missingCount: number;
  items: ComplianceItem[];
}
