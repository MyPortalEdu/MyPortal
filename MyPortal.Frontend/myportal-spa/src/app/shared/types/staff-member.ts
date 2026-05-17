// Mirrors MyPortal.Contracts.Models.People.StaffMemberSummaryResponse. `id`
// is the underlying Person.Id — the same value any HeadTeacherId / person-FK
// column expects.
export interface StaffMemberSummaryResponse {
  id: string;
  code: string;
  title?: string | null;
  firstName: string;
  lastName: string;
  preferredFirstName?: string | null;
  preferredLastName?: string | null;
}
