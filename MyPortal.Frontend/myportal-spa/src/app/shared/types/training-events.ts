export interface TrainingEventSummary {
  id: string;
  courseName: string;
  title: string;
  startTime: string;
  endTime: string | null;
  location: string | null;
  trainer: string | null;
  attendeeCount: number;
  capacity: number | null;
}

export interface TrainingEventAttendee {
  staffMemberId: string;
  staffName: string;
  staffCode: string;
  hasAttended: boolean | null;
}

export interface TrainingEventDetails {
  id: string;
  trainingCourseId: string;
  courseName: string;
  title: string;
  startTime: string;
  endTime: string | null;
  location: string | null;
  trainer: string | null;
  provider: string | null;
  hours: number | null;
  capacity: number | null;
  notes: string | null;
  attendees: TrainingEventAttendee[];
}

export interface TrainingEventUpsert {
  trainingCourseId: string;
  title: string;
  startTime: string;
  endTime: string | null;
  location: string | null;
  trainer: string | null;
  provider: string | null;
  hours: number | null;
  capacity: number | null;
  notes: string | null;
}
