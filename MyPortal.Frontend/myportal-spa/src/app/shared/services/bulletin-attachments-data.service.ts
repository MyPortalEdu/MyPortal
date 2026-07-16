import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  DirectoryContentsResponse,
  DocumentDetailsResponse,
  OTHER_DOCUMENT_TYPE_ID,
} from '../types/document';

/**
 * HTTP wrapper for the bulletin attachments endpoints inherited from
 * BaseDirectoryEntityController. The bulletin's "root directory" id is the
 * `directoryId` returned on BulletinDetailsResponse — callers pass that in.
 *
 * No client-side abstraction over subdirectories: the bulletin UI is flat,
 * everything lands directly in the root directory.
 */
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
    // Title is required by the form binder; fall back to filename when the
    // user hasn't supplied one (which is always, for bulletin attachments).
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

  /** URL the browser can navigate to / trigger a download against. Uses the
   *  same-origin cookie auth (or whatever the user's session is); no fetch
   *  needed because the API returns Content-Disposition: attachment.  */
  downloadUrl(bulletinId: string, documentId: string): string {
    return `/api/v1/bulletins/${bulletinId}/attachments/documents/${documentId}/download`;
  }
}
