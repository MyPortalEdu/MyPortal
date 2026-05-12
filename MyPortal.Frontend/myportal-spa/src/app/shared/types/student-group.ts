// Matches MyPortal.Common.Enums.StudentGroupKind on the backend (TINYINT, 0..4).
export enum StudentGroupKind {
  Other = 0,
  House = 1,
  YearGroup = 2,
  RegGroup = 3,
  CurriculumGroup = 4,
}

export interface StudentGroupSummaryResponse {
  id: string;
  code: string;
  name: string;
  active: boolean;
  kind: StudentGroupKind;
}
