export type StaffTypeFilter = 'All' | 'Teaching' | 'Support';

export interface SalaryInformationReportItem {
  staffCode: string;
  staffName: string;
  serviceTerm: string | null;
  postTitle: string | null;
  payScale: string | null;
  payPoint: string | null;
  fte: number;
  fullTimeSalary: number | null;
  actualSalary: number | null;
  pensionScheme: string | null;
  contractStartDate: string | null;
}

export interface ContractInformationReportItem {
  staffCode: string;
  staffName: string;
  serviceTerm: string | null;
  postTitle: string | null;
  role: string | null;
  contractType: string | null;
  fte: number;
  hoursPerWeek: number | null;
  weeksPerYear: number | null;
  payScale: string | null;
  payPoint: string | null;
  startDate: string | null;
  endDate: string | null;
}

export interface ContractAnalysisReportItem {
  serviceTerm: string;
  contractCount: number;
  staffCount: number;
  teachingCount: number;
  supportCount: number;
  totalFte: number;
}

export interface TerminatingContractReportItem {
  staffCode: string;
  staffName: string;
  postTitle: string | null;
  contractType: string | null;
  serviceTerm: string | null;
  fte: number;
  endDate: string;
}

export interface ReportOption {
  id: string;
  name: string;
}

export interface IndividualAbsenceReportItem {
  absenceType: string;
  illnessType: string | null;
  startDate: string;
  endDate: string | null;
  workingDaysLost: number | null;
  hoursLost: number | null;
  notes: string | null;
}

export interface StaffAbsenceAnalysisReportItem {
  serviceTerm: string;
  absenceCount: number;
  staffCount: number;
  totalWorkingDaysLost: number;
}

export interface LongTermAbsenceReportItem {
  staffCode: string;
  staffName: string;
  absenceType: string;
  startDate: string;
  endDate: string | null;
  workingDaysLost: number | null;
}

export interface StaffTrainingReportItem {
  staffCode: string;
  staffName: string;
  course: string | null;
  status: string | null;
  completedDate: string | null;
  expiryDate: string | null;
  hours: number | null;
  provider: string | null;
}

export interface TrainingCourseAttendeeReportItem {
  staffCode: string;
  staffName: string;
  status: string | null;
  completedDate: string | null;
  expiryDate: string | null;
  hours: number | null;
}
