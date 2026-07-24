export type StaffCalendarCategory =
  | 'Lesson'
  | 'Cover'
  | 'Detention'
  | 'Event'
  | 'Holiday'
  | 'NonContact'
  | 'ParentEvening'
  | 'Absence';

export interface StaffCalendarEvent {
  id: string;
  title: string;
  start: string;
  end: string;
  allDay: boolean;
  category: StaffCalendarCategory;
  location?: string | null;
  kind?: number | null;
  colourCode?: string | null;
}

export interface StaffCalendarResponse {
  events: StaffCalendarEvent[];
}
