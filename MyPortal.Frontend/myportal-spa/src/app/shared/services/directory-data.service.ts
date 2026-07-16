import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  DirectoryContentsResponse,
  DirectoryDetailsResponse,
  DirectoryTreeResponse,
  DocumentDetailsResponse,
} from '../types/document';
import { LookupResponse } from '../types/lookup';

/** Which document-type facets to include — maps to DocumentTypeFilter on the API. */
export interface DocumentTypeFacets {
  general?: boolean;
  staff?: boolean;
  student?: boolean;
  contact?: boolean;
  send?: boolean;
}

// DirectoryUploadPolicy.StaffOnly. The enum has no JSON string converter on the
// server, so it's wire-serialised as its numeric value.
const UPLOAD_POLICY_STAFF_ONLY = 0;

/**
 * Generic HTTP wrapper for the attachments endpoints exposed by any
 * BaseDirectoryEntityController subclass. The route shape is identical across
 * entities (`{base}/attachments/...`), so a single service serves staff,
 * students, bulletins, etc. — callers pass the entity's attachments base URL
 * (e.g. `/api/v1/staffmembers/{id}/attachments`).
 */
@Injectable({ providedIn: 'root' })
export class DirectoryDataService {
  private readonly http = inject(HttpClient);

  /**
   * Document type catalogue, narrowed to the facets the caller cares about.
   * The API's DocumentTypeFilter is OR-based, so passing `{ staff: true }`
   * returns every type tagged Staff (plus the all-purpose "Other", which is
   * flagged on every facet). Always Active-filtered server-side.
   */
  getDocumentTypes(facets: DocumentTypeFacets): Observable<LookupResponse[]> {
    let params = new HttpParams();
    if (facets.general) params = params.set('General', 'true');
    if (facets.staff) params = params.set('Staff', 'true');
    if (facets.student) params = params.set('Student', 'true');
    if (facets.contact) params = params.set('Contact', 'true');
    if (facets.send) params = params.set('Send', 'true');
    return this.http.get<LookupResponse[]>(`/api/v1/documenttypes`, { params });
  }

  rootContents(baseUrl: string): Observable<DirectoryContentsResponse> {
    return this.http.get<DirectoryContentsResponse>(`${baseUrl}/root/contents`);
  }

  contents(baseUrl: string, directoryId: string): Observable<DirectoryContentsResponse> {
    return this.http.get<DirectoryContentsResponse>(
      `${baseUrl}/directories/${directoryId}/contents`,
    );
  }

  tree(baseUrl: string, directoryId: string): Observable<DirectoryTreeResponse> {
    return this.http.get<DirectoryTreeResponse>(`${baseUrl}/directories/${directoryId}/tree`);
  }

  createFolder(
    baseUrl: string,
    parentId: string,
    name: string,
    isPrivate: boolean,
  ): Observable<DirectoryDetailsResponse> {
    return this.http.post<DirectoryDetailsResponse>(`${baseUrl}/directories`, {
      parentId,
      name,
      isPrivate,
      uploadPolicy: UPLOAD_POLICY_STAFF_ONLY,
    });
  }

  /** Rename and/or move a folder. The server treats Name + ParentId on one PUT;
   *  privacy / upload policy are preserved from the existing folder. */
  updateFolder(
    baseUrl: string,
    dir: DirectoryDetailsResponse,
    changes: { name?: string; parentId?: string },
  ): Observable<DirectoryDetailsResponse> {
    return this.http.put<DirectoryDetailsResponse>(`${baseUrl}/directories/${dir.id}`, {
      parentId: changes.parentId ?? dir.parentId,
      name: changes.name ?? dir.name,
      isPrivate: dir.isPrivate ?? false,
      uploadPolicy: dir.uploadPolicy ?? UPLOAD_POLICY_STAFF_ONLY,
    });
  }

  deleteFolder(baseUrl: string, directoryId: string): Observable<void> {
    return this.http.delete<void>(`${baseUrl}/directories/${directoryId}`);
  }

  upload(
    baseUrl: string,
    directoryId: string,
    file: File,
    isPrivate: boolean,
    typeId: string,
  ): Observable<DocumentDetailsResponse> {
    const form = new FormData();
    form.append('TypeId', typeId);
    form.append('DirectoryId', directoryId);
    form.append('Title', file.name);
    form.append('IsPrivate', String(isPrivate));
    form.append('File', file, file.name);

    return this.http.post<DocumentDetailsResponse>(`${baseUrl}/documents`, form);
  }

  /** Re-classify an existing document. Re-sends required metadata (no File =
   *  content preserved server-side), changing only TypeId. */
  changeDocumentType(
    baseUrl: string,
    doc: DocumentDetailsResponse,
    typeId: string,
  ): Observable<DocumentDetailsResponse> {
    const form = new FormData();
    form.append('TypeId', typeId);
    form.append('DirectoryId', doc.directoryId);
    form.append('Title', doc.title ?? doc.fileName);
    if (doc.description) form.append('Description', doc.description);
    form.append('IsPrivate', String(doc.isPrivate));

    return this.http.put<DocumentDetailsResponse>(`${baseUrl}/documents/${doc.id}`, form);
  }

  /** Move a document to another directory. Re-sends required metadata; no File =
   *  content is preserved server-side. */
  moveDocument(
    baseUrl: string,
    doc: DocumentDetailsResponse,
    targetDirectoryId: string,
  ): Observable<DocumentDetailsResponse> {
    const form = new FormData();
    form.append('TypeId', doc.typeId);
    form.append('DirectoryId', targetDirectoryId);
    form.append('Title', doc.title ?? doc.fileName);
    if (doc.description) form.append('Description', doc.description);
    form.append('IsPrivate', String(doc.isPrivate));

    return this.http.put<DocumentDetailsResponse>(`${baseUrl}/documents/${doc.id}`, form);
  }

  deleteDocument(baseUrl: string, documentId: string): Observable<void> {
    return this.http.delete<void>(`${baseUrl}/documents/${documentId}`);
  }

  /** Same-origin URL the browser navigates to for download (cookie auth +
   *  Content-Disposition: attachment on the server). */
  downloadUrl(baseUrl: string, documentId: string): string {
    return `${baseUrl}/documents/${documentId}/download`;
  }
}
