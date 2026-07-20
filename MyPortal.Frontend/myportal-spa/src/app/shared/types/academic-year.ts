export enum SchoolHolidayType {
  HalfTerm = 0,
  TeacherTraining = 1,
  PublicHoliday = 2,
}

export interface AcademicTermUpsertRequest {
  academicTermId?: string | null;
  name: string;
  startDate: Date | null;
  endDate: Date | null;
}

export interface AttendancePeriodUpsertRequest {
  attendancePeriodId?: string | null;
  cycleDayIndex: number;
  name: string;
  startTime: Date | null;
  endTime: Date | null;
  isAmReg: boolean;
  isPmReg: boolean;
  isLesson: boolean;
}

export interface SchoolHolidayUpsertRequest {
  schoolHolidayId?: string | null;
  name: string;
  type: SchoolHolidayType;
  startDate: Date | null;
  endDate: Date | null;
}

export interface AcademicYearUpsertRequest {
  timetableCycleLength: number;
  schoolWeekLength: number;
  firstWeekOffset: number;
  copyPeriodsFromAcademicYearId?: string | null;
  copyPastoralStructureFromAcademicYearId?: string | null;
  academicTerms: AcademicTermUpsertRequest[];
  attendancePeriods: AttendancePeriodUpsertRequest[];
  schoolHolidays: SchoolHolidayUpsertRequest[];
}
