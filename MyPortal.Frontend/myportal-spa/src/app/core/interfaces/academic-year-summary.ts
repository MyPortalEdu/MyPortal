export interface AcademicYearSummary {
  id: string;
  name: string;
  isLocked: boolean;
  timetableCycleLength: number;
  schoolWeekLength: number;
  startDate?: string | null;
  endDate?: string | null;
}
