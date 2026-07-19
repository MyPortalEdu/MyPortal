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

export interface DocumentTypeFacets {
  general?: boolean;
  staff?: boolean;
  student?: boolean;
  contact?: boolean;
  send?: boolean;
}

const UPLOAD_POLICY_STAFF_ONLY = 0;

@Injectable({ providedIn: 'root' })
export class DirectoryDataService {
  private readonly http = inject(HttpClient);

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

  downloadUrl(baseUrl: string, documentId: string): string {
    return `${baseUrl}/documents/${documentId}/download`;
  }
}
