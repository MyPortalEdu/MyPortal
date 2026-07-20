import { SchoolHolidayType } from './academic-year';

export interface AcademicTermResponse {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
}

export interface AttendancePeriodResponse {
  id: string;
  name: string;
  cycleDayIndex: number;
  startTime: string;
  endTime: string;
  isAmReg: boolean;
  isPmReg: boolean;
  isLesson: boolean;
}

export interface SchoolHolidayResponse {
  id: string;
  name: string;
  type: SchoolHolidayType;
  startDate: string;
  endDate: string;
}

export interface AcademicYearDetailsResponse {
  id: string;
  name: string;
  isLocked: boolean;
  timetableCycleLength: number;
  schoolWeekLength: number;
  terms: AcademicTermResponse[];
  schoolHolidays: SchoolHolidayResponse[];
  attendancePeriods: AttendancePeriodResponse[];
}
