export interface ContactHeaderResponse {
  id: string;
  personId: string;
  displayName: string;
  preferredName?: string | null;
  photoId?: string | null;
}
