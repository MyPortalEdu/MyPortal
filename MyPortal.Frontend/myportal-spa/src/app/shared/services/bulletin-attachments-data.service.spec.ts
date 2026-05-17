import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { BulletinAttachmentsDataService } from './bulletin-attachments-data.service';
import { DirectoryContentsResponse, OTHER_DOCUMENT_TYPE_ID } from '../types/document';

describe('BulletinAttachmentsDataService', () => {
  let service: BulletinAttachmentsDataService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), BulletinAttachmentsDataService],
    });
    service = TestBed.inject(BulletinAttachmentsDataService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('listContents() GETs the inherited attachments directory-contents endpoint', () => {
    const expected: DirectoryContentsResponse = {
      directory: { id: 'd1', name: 'root', parentId: null },
      directories: [],
      documents: [],
    };
    service.listContents('b1', 'd1').subscribe(r => expect(r).toEqual(expected));

    const req = http.expectOne('/api/v1/bulletins/b1/attachments/directories/d1/contents');
    expect(req.request.method).toBe('GET');
    req.flush(expected);
  });

  it('upload() POSTs multipart with OTHER_DOCUMENT_TYPE_ID and file name as title', () => {
    const file = new File(['x'], 'pic.png', { type: 'image/png' });
    service.upload('b1', 'd1', file).subscribe();

    const req = http.expectOne('/api/v1/bulletins/b1/attachments/documents');
    expect(req.request.method).toBe('POST');
    const body = req.request.body as FormData;
    expect(body.get('TypeId')).toBe(OTHER_DOCUMENT_TYPE_ID);
    expect(body.get('DirectoryId')).toBe('d1');
    // The form uses the filename as the title — there's no separate title input
    // in the bulletin attachments UI.
    expect(body.get('Title')).toBe('pic.png');
    expect(body.get('IsPrivate')).toBe('false');
    expect(body.get('File')).toEqual(file);
    req.flush({});
  });

  it('delete() DELETEs the document endpoint', () => {
    service.delete('b1', 'doc-1').subscribe();

    const req = http.expectOne('/api/v1/bulletins/b1/attachments/documents/doc-1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('downloadUrl() composes the download path without hitting the network', () => {
    expect(service.downloadUrl('b1', 'doc-1'))
      .toBe('/api/v1/bulletins/b1/attachments/documents/doc-1/download');
    // No HTTP traffic for url helpers — http.verify() in afterEach asserts this.
  });
});
