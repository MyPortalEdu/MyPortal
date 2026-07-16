// Mirrors MyPortal.Contracts.Models.Documents.DocumentDetailsResponse — only
// the fields the attachments UI cares about.
export interface DocumentDetailsResponse {
  id: string;
  typeId: string;
  typeDescription: string;
  directoryId: string;
  createdById: string;
  createdByName: string;
  createdAt: string;
  lastModifiedById: string;
  lastModifiedByName: string;
  lastModifiedAt: string;
  storageKey: string;
  fileName: string;
  contentType: string;
  sizeBytes?: number | null;
  hash?: string | null;
  title?: string | null;
  description?: string | null;
  isPrivate: boolean;
  isDeleted: boolean;
}

export interface DirectoryDetailsResponse {
  id: string;
  name: string;
  parentId?: string | null;
  // Returned by the API; needed so rename/move can preserve them rather than
  // resetting. isPrivate = staff-only visibility; uploadPolicy is the
  // DirectoryUploadPolicy enum (numeric on the wire).
  isPrivate?: boolean;
  uploadPolicy?: number;
}

export interface DirectoryContentsResponse {
  directory: DirectoryDetailsResponse;
  directories: DirectoryDetailsResponse[];
  documents: DocumentDetailsResponse[];
}

// Mirrors MyPortal.Contracts.Models.Documents.DirectoryTreeResponse — the
// recursive form, used to populate the "move to…" folder picker.
export interface DirectoryTreeResponse {
  directory: DirectoryDetailsResponse;
  directories: DirectoryTreeResponse[];
  documents: DocumentDetailsResponse[];
}

// Hard-coded TypeId for the "Other" document type, seeded in migration
// 20251101000300_seed_uk_data.sql. Bulletin attachments don't have a
// dedicated DocumentType yet — using "Other" keeps the upload path moving
// without seeding a new row. Replace with a dedicated id when one is added.
export const OTHER_DOCUMENT_TYPE_ID = '5D7555DE-0C38-4FCC-BB54-C3C4A7E81201';

/**
 * Per-file upload cap, enforced client-side so oversize files produce an
 * immediate toast instead of a TCP reset mid-upload. Matches Kestrel's
 * default `MaxRequestBodySize` of 30,000,000 bytes — bump Kestrel (and this
 * constant) in lockstep if a larger cap is ever needed.
 */
export const MAX_ATTACHMENT_BYTES = 30_000_000;
export const MAX_ATTACHMENT_LABEL = '30 MB';
