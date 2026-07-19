import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  DirectoryContentsResponse,
  DocumentDetailsResponse,
  OTHER_DOCUMENT_TYPE_ID,
} from '../types/document';

@Injectable({ providedIn: 'root' })
export class BulletinAttachmentsDataService {
  private readonly http = inject(HttpClient);

  listContents(bulletinId: string, directoryId: string): Observable<DirectoryContentsResponse> {
    return this.http.get<DirectoryContentsResponse>(
      `/api/v1/bulletins/${bulletinId}/attachments/directories/${directoryId}/contents`,
    );
  }

  upload(bulletinId: string, directoryId: string, file: File): Observable<DocumentDetailsResponse> {
    const form = new FormData();
    form.append('TypeId', OTHER_DOCUMENT_TYPE_ID);
    form.append('DirectoryId', directoryId);
    form.append('Title', file.name);
    form.append('IsPrivate', 'false');
    form.append('File', file, file.name);

    return this.http.post<DocumentDetailsResponse>(
      `/api/v1/bulletins/${bulletinId}/attachments/documents`,
      form,
    );
  }

  delete(bulletinId: string, documentId: string): Observable<void> {
    return this.http.delete<void>(
      `/api/v1/bulletins/${bulletinId}/attachments/documents/${documentId}`,
    );
  }

  downloadUrl(bulletinId: string, documentId: string): string {
    return `/api/v1/bulletins/${bulletinId}/attachments/documents/${documentId}/download`;
  }
}
