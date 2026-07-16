// Mirrors MyPortal.Contracts.Models.People.StaffCalendarEventResponse — one flattened,
// read-only calendar entry. `category` is the source bucket used for colour/legend.
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
  colourCode?: string | null;
}

// Mirrors MyPortal.Contracts.Models.People.StaffCalendarResponse.
export interface StaffCalendarResponse {
  events: StaffCalendarEvent[];
}
