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
  isPrivate?: boolean;
  uploadPolicy?: number;
}

export interface DirectoryContentsResponse {
  directory: DirectoryDetailsResponse;
  directories: DirectoryDetailsResponse[];
  documents: DocumentDetailsResponse[];
}

export interface DirectoryTreeResponse {
  directory: DirectoryDetailsResponse;
  directories: DirectoryTreeResponse[];
  documents: DocumentDetailsResponse[];
}

export const OTHER_DOCUMENT_TYPE_ID = '5D7555DE-0C38-4FCC-BB54-C3C4A7E81201';

export const MAX_ATTACHMENT_BYTES = 30_000_000;
export const MAX_ATTACHMENT_LABEL = '30 MB';
