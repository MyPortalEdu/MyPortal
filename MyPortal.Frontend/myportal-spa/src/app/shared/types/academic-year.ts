// Mirrors MyPortal.Common.Enums.SchoolHolidayType. ASP.NET serialises C# enums
// as their underlying numeric values by default, so the wire contract is the
// declaration order on the server (HalfTerm=0, TeacherTraining=1, PublicHoliday=2).
export enum SchoolHolidayType {
  HalfTerm = 0,
  TeacherTraining = 1,
  PublicHoliday = 2,
}

// Date fields are held as Date | null in the wizard so PrimeNG's datepicker can
// round-trip without us creating fresh Date instances on every render (which
// triggered an ngModelChange feedback loop). The data service converts to the
// server-side wire format (date-only ISO without a timezone marker) at submit
// time so picking a date in BST doesn't shift by one day under UTC serialisation.
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
  // Times are held as Date | null in the wizard for the same reason as term
  // dates — store the picker's instance verbatim and serialise to the server's
  // TimeOnly format ("HH:mm:ss") at the wire boundary.
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
