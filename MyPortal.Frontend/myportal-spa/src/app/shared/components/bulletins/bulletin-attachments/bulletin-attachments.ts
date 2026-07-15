import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { BulletinAttachmentsDataService } from '../../../services/bulletin-attachments-data.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../core/services/notification.service';
import {
  DocumentDetailsResponse,
  MAX_ATTACHMENT_BYTES,
  MAX_ATTACHMENT_LABEL,
} from '../../../types/document';

export type BulletinAttachmentsMode = 'view' | 'edit' | 'stage';

/**
 * Bulletin attachments widget. Three modes:
 *
 * - **view**: read-only file list (detail dialog). Needs bulletinId + directoryId.
 * - **edit**: list + drop-to-upload + delete (form dialog, edit mode). Needs
 *   bulletinId + directoryId; uploads fire immediately.
 * - **stage**: collect files in memory pre-publish (form dialog, create mode).
 *   No bulletin exists yet; the parent calls `uploadStaged()` after publish
 *   to flush the queue against the freshly-created bulletin.
 *
 * The component owns staged state so the parent doesn't have to babysit File
 * objects. `hasStaged()` and `uploadStaged()` are the contract for the form
 * dialog's post-publish flow.
 */
@Component({
  selector: 'mp-bulletin-attachments',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TranslocoDirective, TranslocoPipe],
  providers: [provideTranslocoScope('bulletins')],
  templateUrl: './bulletin-attachments.html',
  host: { class: 'block' },
})
export class BulletinAttachments {
  private readonly data = inject(BulletinAttachmentsDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly mode = input.required<BulletinAttachmentsMode>();
  /** Required for view/edit. Null for stage (no bulletin exists yet). */
  readonly bulletinId = input<string | null>(null);
  /** Required for view/edit. The bulletin's root directory id. */
  readonly directoryId = input<string | null>(null);

  private readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('fileInput');

  readonly documents = signal<DocumentDetailsResponse[]>([]);
  readonly staged = signal<File[]>([]);
  readonly loading = signal(false);
  readonly uploading = signal(false);
  readonly dragOver = signal(false);

  // Exposed for the dropzone hint; the size check itself lives in handleFiles.
  readonly sizeLabel = MAX_ATTACHMENT_LABEL;

  readonly isEditable = computed(() => this.mode() === 'edit' || this.mode() === 'stage');
  readonly isStage = computed(() => this.mode() === 'stage');
  readonly hasAnything = computed(() => this.documents().length > 0 || this.staged().length > 0);

  constructor() {
    effect(() => {
      const mode = this.mode();
      const bulletinId = this.bulletinId();
      const directoryId = this.directoryId();
      untracked(() => {
        if (mode === 'stage') {
          // Stage mode means no server-side bulletin exists yet — clear any
          // previously-loaded documents so we don't render leftovers.
          this.documents.set([]);
          return;
        }
        if (bulletinId && directoryId) {
          this.refresh(bulletinId, directoryId);
        }
      });
    });
  }

  onDragOver(event: DragEvent): void {
    if (!this.isEditable()) return;
    event.preventDefault();
    this.dragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver.set(false);
    if (!this.isEditable()) return;
    const files = event.dataTransfer?.files;
    if (files && files.length) this.handleFiles(Array.from(files));
  }

  onBrowseClick(): void {
    if (!this.isEditable()) return;
    this.fileInput()?.nativeElement.click();
  }

  onFileInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length) {
      this.handleFiles(Array.from(input.files));
      // Reset the input so the same file can be picked again later.
      input.value = '';
    }
  }

  hasStaged(): boolean {
    return this.staged().length > 0;
  }

  /**
   * Flush staged files against the freshly-created bulletin. Sequential to
   * avoid hammering the API; the first failure stops the queue and surfaces
   * a toast. Resolves true on full success, false otherwise.
   */
  async uploadStaged(bulletinId: string, directoryId: string): Promise<boolean> {
    const queue = this.staged();
    if (queue.length === 0) return true;
    this.uploading.set(true);

    for (const file of queue) {
      try {
        await this.uploadOne(bulletinId, directoryId, file);
      } catch (err) {
        this.uploading.set(false);
        this.notify.apiError(err,
          this.transloco.translate('bulletins.attachments.errorUpload', { name: file.name }));
        return false;
      }
    }

    this.staged.set([]);
    this.uploading.set(false);
    return true;
  }

  removeStaged(index: number): void {
    this.staged.update(s => s.filter((_, i) => i !== index));
  }

  async deleteDocument(doc: DocumentDetailsResponse): Promise<void> {
    const bulletinId = this.bulletinId();
    if (!bulletinId) return;
    const ok = await this.confirm.danger({
      message: this.transloco.translate('bulletins.attachments.deleteConfirm',
        { name: doc.fileName }),
    });
    if (!ok) return;
    this.data.delete(bulletinId, doc.id).subscribe({
      next: () => {
        this.documents.update(d => d.filter(x => x.id !== doc.id));
        this.notify.success(this.transloco.translate('bulletins.attachments.deletedToast'));
      },
      error: err => this.notify.apiError(err,
        this.transloco.translate('bulletins.attachments.errorDelete')),
    });
  }

  downloadUrl(doc: DocumentDetailsResponse): string {
    const bulletinId = this.bulletinId();
    return bulletinId ? this.data.downloadUrl(bulletinId, doc.id) : '#';
  }

  // Pretty file size — 1.2 KB / 3.4 MB. Locale-agnostic; the unit suffixes
  // are short enough that translation isn't worth the i18n weight.
  formatSize(bytes?: number | null): string {
    if (bytes == null) return '';
    const units = ['B', 'KB', 'MB', 'GB'];
    let value = bytes;
    let unit = 0;
    while (value >= 1024 && unit < units.length - 1) {
      value /= 1024;
      unit++;
    }
    return `${value.toFixed(unit === 0 ? 0 : 1)} ${units[unit]}`;
  }

  // Map MIME type to a Font Awesome icon. Falls back to a generic file icon.
  iconFor(contentType: string): string {
    if (!contentType) return 'fa-solid fa-file';
    if (contentType.startsWith('image/'))      return 'fa-solid fa-file-image';
    if (contentType.startsWith('video/'))      return 'fa-solid fa-file-video';
    if (contentType.startsWith('audio/'))      return 'fa-solid fa-file-audio';
    if (contentType === 'application/pdf')     return 'fa-solid fa-file-pdf';
    if (contentType.includes('word'))          return 'fa-solid fa-file-word';
    if (contentType.includes('excel') || contentType.includes('spreadsheet')) return 'fa-solid fa-file-excel';
    if (contentType.includes('zip') || contentType.includes('compressed'))    return 'fa-solid fa-file-zipper';
    return 'fa-solid fa-file';
  }

  private handleFiles(files: File[]): void {
    // Filter oversize files out client-side and toast for each. The server
    // also rejects them (Kestrel/MVC size limits on the upload action) but
    // by then the connection has reset and the browser shows a generic
    // ERR_CONNECTION_RESET with no useful detail.
    const accepted: File[] = [];
    for (const f of files) {
      if (f.size > MAX_ATTACHMENT_BYTES) {
        this.notify.warn(
          this.transloco.translate('bulletins.attachments.errorTooLarge',
            { name: f.name, limit: MAX_ATTACHMENT_LABEL }),
        );
        continue;
      }
      accepted.push(f);
    }
    if (accepted.length === 0) return;

    if (this.mode() === 'stage') {
      this.staged.update(s => [...s, ...accepted]);
      return;
    }
    const bulletinId = this.bulletinId();
    const directoryId = this.directoryId();
    if (this.mode() === 'edit' && bulletinId && directoryId) {
      void this.uploadSequentially(bulletinId, directoryId, accepted);
    }
  }

  // Sequential to keep failure handling simple (no parallel toast spam) and
  // to avoid hammering the upload endpoint with N concurrent 30MB requests.
  private async uploadSequentially(bulletinId: string, directoryId: string, files: File[]): Promise<void> {
    this.uploading.set(true);
    for (const file of files) {
      try {
        const doc = await this.uploadOne(bulletinId, directoryId, file);
        this.documents.update(d => [...d, doc]);
      } catch (err) {
        this.notify.apiError(err,
          this.transloco.translate('bulletins.attachments.errorUpload', { name: file.name }));
      }
    }
    this.uploading.set(false);
  }

  private async uploadOne(bulletinId: string, directoryId: string, file: File): Promise<DocumentDetailsResponse> {
    return new Promise((resolve, reject) => {
      this.data.upload(bulletinId, directoryId, file).subscribe({
        next: doc => resolve(doc),
        error: err => reject(err),
      });
    });
  }

  private refresh(bulletinId: string, directoryId: string): void {
    this.loading.set(true);
    this.data.listContents(bulletinId, directoryId).subscribe({
      next: contents => {
        this.documents.set(contents.documents ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err,
          this.transloco.translate('bulletins.attachments.errorLoad'));
      },
    });
  }
}
