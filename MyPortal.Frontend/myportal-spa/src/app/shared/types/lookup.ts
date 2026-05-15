// Mirrors MyPortal.Contracts.Models.LookupResponse — the wire shape every
// `LookupEntity`-backed catalogue endpoint returns (school types, governance
// types, etc.).
export interface LookupResponse {
  id: string;
  description: string;
}
