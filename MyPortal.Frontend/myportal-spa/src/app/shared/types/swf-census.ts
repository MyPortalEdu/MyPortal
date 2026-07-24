export interface SwfCensusReadinessIssue {
  staffMemberId: string;
  staffName: string;
  field: string;
  detail: string;
}

export interface SwfCensusMemberSummary {
  staffMemberId: string;
  name: string;
  trn: string | null;
  postCode: string | null;
  roleCode: string | null;
  hasContract: boolean;
  absenceCount: number;
  issueCount: number;
}

export interface SwfCensusPreview {
  referenceDate: string;
  year: number;
  laNumber: string | null;
  estab: string | null;
  urn: string | null;
  memberCount: number;
  issueCount: number;
  issues: SwfCensusReadinessIssue[];
  members: SwfCensusMemberSummary[];
}
