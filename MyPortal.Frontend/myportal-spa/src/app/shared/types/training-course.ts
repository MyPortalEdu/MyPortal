export interface TrainingCourse {
  id: string;
  code: string;
  name: string;
  description: string | null;
  active: boolean;
  inUse: boolean;
}

export interface TrainingCourseUpsert {
  code: string;
  name: string;
  description: string | null;
  active: boolean;
}
