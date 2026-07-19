import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';

import { BulletinAttachments } from './bulletin-attachments';
import { BulletinAttachmentsDataService } from '../../../services/bulletin-attachments-data.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { DocumentDetailsResponse, MAX_ATTACHMENT_BYTES } from '../../../types/document';

function makeDoc(overrides: Partial<DocumentDetailsResponse> = {}): DocumentDetailsResponse {
  return {
    id: 'doc-1',
    typeId: 't',
    typeDescription: '',
    directoryId: 'd1',
    createdById: 'u',
    createdByName: 'U',
    createdAt: '2026-05-01T00:00:00Z',
    lastModifiedById: 'u',
    lastModifiedByName: 'U',
    lastModifiedAt: '2026-05-01T00:00:00Z',
    storageKey: 'k',
    fileName: 'note.pdf',
    contentType: 'application/pdf',
    sizeBytes: 1024,
    hash: null,
    title: null,
    description: null,
    isPrivate: false,
    isDeleted: false,
    ...overrides,
  };
}

function makeFile(name: string, size: number, type = 'application/pdf'): File {
  return new File([new ArrayBuffer(size)], name, { type });
}

function fileInputEvent(files: File[]): Event {
  const list = Object.assign(
    Object.fromEntries(files.map((f, i) => [i, f])),
    { length: files.length, item: (i: number) => files[i] ?? null },
  ) as unknown as FileList;
  const target = { files: list, value: 'irrelevant' } as unknown as HTMLInputElement;
  return { target } as unknown as Event;
}

describe('BulletinAttachments', () => {
  let fixture: ComponentFixture<BulletinAttachments>;
  let component: BulletinAttachments;
  let data: jasmine.SpyObj<BulletinAttachmentsDataService>;
  let notify: jasmine.SpyObj<NotificationService>;
  let confirm: jasmine.SpyObj<ConfirmationDialog>;

  beforeEach(async () => {
    data = jasmine.createSpyObj<BulletinAttachmentsDataService>('BulletinAttachmentsDataService',
      ['listContents', 'upload', 'delete', 'downloadUrl']);
    notify = jasmine.createSpyObj<NotificationService>('NotificationService',
      ['success', 'error', 'warn', 'apiError']);
    confirm = jasmine.createSpyObj<ConfirmationDialog>('ConfirmationDialog', ['danger']);

    data.listContents.and.returnValue(of({
      directory: { id: 'd1', name: 'root', parentId: null },
      directories: [],
      documents: [],
    }));
    data.upload.and.callFake((_b, _d, f) =>
      of(makeDoc({ id: `doc-${f.name}`, fileName: f.name })),
    );
    data.delete.and.returnValue(of(void 0));
    data.downloadUrl.and.callFake((b, d) => `/api/v1/bulletins/${b}/attachments/documents/${d}/download`);

    const translocoStub = {
      translate: (key: string) => key,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(BulletinAttachments, { set: { template: '' } });

    await TestBed.configureTestingModule({
      imports: [BulletinAttachments],
      providers: [
        { provide: BulletinAttachmentsDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: ConfirmationDialog, useValue: confirm },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BulletinAttachments);
    component = fixture.componentInstance;
  });

  function setMode(mode: 'view' | 'edit' | 'stage', bulletinId: string | null = null, directoryId: string | null = null) {
    fixture.componentRef.setInput('mode', mode);
    fixture.componentRef.setInput('bulletinId', bulletinId);
    fixture.componentRef.setInput('directoryId', directoryId);
    fixture.detectChanges();
  }

  it('stage mode does NOT call listContents on init', () => {
    setMode('stage');
    expect(data.listContents).not.toHaveBeenCalled();
  });

  it('stage mode: handleFiles queues files into staged() and hasStaged() reflects it', () => {
    setMode('stage');
    component.onFileInputChange(fileInputEvent([makeFile('a.pdf', 10), makeFile('b.png', 20, 'image/png')]));

    expect(component.staged().map(f => f.name)).toEqual(['a.pdf', 'b.png']);
    expect(component.hasStaged()).toBeTrue();
    expect(data.upload).not.toHaveBeenCalled();
  });

  it('removeStaged() drops the entry at the given index', () => {
    setMode('stage');
    component.onFileInputChange(fileInputEvent([makeFile('a.pdf', 1), makeFile('b.pdf', 1), makeFile('c.pdf', 1)]));
    component.removeStaged(1);
    expect(component.staged().map(f => f.name)).toEqual(['a.pdf', 'c.pdf']);
  });

  it('uploadStaged() flushes the queue sequentially and clears staged on success', async () => {
    setMode('stage');
    component.onFileInputChange(fileInputEvent([makeFile('a.pdf', 10), makeFile('b.pdf', 20)]));

    const ok = await component.uploadStaged('b1', 'd1');

    expect(ok).toBeTrue();
    expect(data.upload).toHaveBeenCalledTimes(2);
    expect(component.staged()).toEqual([]);
    expect(component.uploading()).toBeFalse();
  });

  it('uploadStaged() stops on the first failure, leaves the queue intact, and toasts', async () => {
    setMode('stage');
    component.onFileInputChange(fileInputEvent([makeFile('a.pdf', 1), makeFile('b.pdf', 1)]));
    data.upload.and.returnValue(throwError(() => new Error('boom')));

    const ok = await component.uploadStaged('b1', 'd1');

    expect(ok).toBeFalse();
    expect(data.upload).toHaveBeenCalledTimes(1);
    expect(component.staged().length).toBe(2);
    expect(notify.apiError).toHaveBeenCalled();
  });

  it('edit mode loads existing documents on init and appends successful uploads', async () => {
    data.listContents.and.returnValue(of({
      directory: { id: 'd1', name: 'root', parentId: null },
      directories: [],
      documents: [makeDoc({ id: 'existing', fileName: 'old.pdf' })],
    }));

    setMode('edit', 'b1', 'd1');

    expect(component.documents().map(d => d.id)).toEqual(['existing']);

    component.onFileInputChange(fileInputEvent([makeFile('new.pdf', 10)]));
    await Promise.resolve();
    await Promise.resolve();

    expect(data.upload).toHaveBeenCalledWith('b1', 'd1', jasmine.any(File));
    expect(component.documents().map(d => d.fileName)).toEqual(['old.pdf', 'new.pdf']);
  });

  it('edit mode rejects oversize files with a warn toast and does not upload them', () => {
    setMode('edit', 'b1', 'd1');
    const oversize = makeFile('huge.bin', MAX_ATTACHMENT_BYTES + 1);

    component.onFileInputChange(fileInputEvent([oversize]));

    expect(notify.warn).toHaveBeenCalled();
    expect(data.upload).not.toHaveBeenCalled();
  });

  it('edit mode uploads only the accepted files when a mix is dropped', async () => {
    setMode('edit', 'b1', 'd1');
    component.onFileInputChange(fileInputEvent([
      makeFile('big.bin', MAX_ATTACHMENT_BYTES + 1),
      makeFile('ok.pdf', 10),
    ]));
    await Promise.resolve();
    await Promise.resolve();

    expect(notify.warn).toHaveBeenCalled();
    expect(data.upload).toHaveBeenCalledTimes(1);
  });

  it('deleteDocument prompts to confirm and removes the doc on success', async () => {
    confirm.danger.and.resolveTo(true);
    setMode('edit', 'b1', 'd1');
    component.documents.set([makeDoc({ id: 'd1' }), makeDoc({ id: 'd2', fileName: 'two.pdf' })]);

    await component.deleteDocument(makeDoc({ id: 'd1' }));

    expect(data.delete).toHaveBeenCalledWith('b1', 'd1');
    expect(component.documents().map(d => d.id)).toEqual(['d2']);
    expect(notify.success).toHaveBeenCalled();
  });

  it('deleteDocument does nothing when the user cancels the confirm prompt', async () => {
    confirm.danger.and.resolveTo(false);
    setMode('edit', 'b1', 'd1');
    component.documents.set([makeDoc({ id: 'd1' })]);

    await component.deleteDocument(makeDoc({ id: 'd1' }));

    expect(data.delete).not.toHaveBeenCalled();
    expect(component.documents().length).toBe(1);
  });

  it('switching to stage mode clears any previously-loaded server documents', () => {
    setMode('edit', 'b1', 'd1');
    component.documents.set([makeDoc()]);

    fixture.componentRef.setInput('mode', 'stage');
    fixture.detectChanges();

    expect(component.documents()).toEqual([]);
  });

  it('isEditable is false in view mode', () => {
    setMode('view', 'b1', 'd1');
    expect(component.isEditable()).toBeFalse();
    expect(component.isStage()).toBeFalse();
  });

  it('isEditable is true and isStage is false in edit mode', () => {
    setMode('edit', 'b1', 'd1');
    expect(component.isEditable()).toBeTrue();
    expect(component.isStage()).toBeFalse();
  });

  it('isStage is true in stage mode', () => {
    setMode('stage');
    expect(component.isStage()).toBeTrue();
    expect(component.isEditable()).toBeTrue();
  });

  it('downloadUrl falls back to "#" when no bulletinId is set', () => {
    setMode('stage');
    expect(component.downloadUrl(makeDoc())).toBe('#');
  });

  it('downloadUrl delegates to the service when a bulletinId is set', () => {
    setMode('view', 'b1', 'd1');
    const url = component.downloadUrl(makeDoc({ id: 'doc-42' }));
    expect(data.downloadUrl).toHaveBeenCalledWith('b1', 'doc-42');
    expect(url).toBe('/api/v1/bulletins/b1/attachments/documents/doc-42/download');
  });

  it('formatSize scales bytes into B / KB / MB / GB', () => {
    expect(component.formatSize(0)).toBe('0 B');
    expect(component.formatSize(500)).toBe('500 B');
    expect(component.formatSize(2048)).toBe('2.0 KB');
    expect(component.formatSize(5 * 1024 * 1024)).toBe('5.0 MB');
    expect(component.formatSize(undefined)).toBe('');
  });

  it('iconFor maps known MIME prefixes and falls back to a generic file icon', () => {
    expect(component.iconFor('image/png')).toBe('fa-solid fa-file-image');
    expect(component.iconFor('video/mp4')).toBe('fa-solid fa-file-video');
    expect(component.iconFor('audio/mp3')).toBe('fa-solid fa-file-audio');
    expect(component.iconFor('application/pdf')).toBe('fa-solid fa-file-pdf');
    expect(component.iconFor('application/msword')).toBe('fa-solid fa-file-word');
    expect(component.iconFor('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'))
      .toBe('fa-solid fa-file-excel');
    expect(component.iconFor('application/zip')).toBe('fa-solid fa-file-zipper');
    expect(component.iconFor('text/plain')).toBe('fa-solid fa-file');
    expect(component.iconFor('')).toBe('fa-solid fa-file');
  });
});
