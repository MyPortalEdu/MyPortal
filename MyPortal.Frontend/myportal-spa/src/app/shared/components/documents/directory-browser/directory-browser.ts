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
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { MpSelect, MpDialog, MpButton, MpInput, MpSpinner, MpTable, MpTableHeader, MpTableBody } from '@myportal/ui';

import { DirectoryDataService } from '../../../services/directory-data.service';
import { LookupResponse } from '../../../types/lookup';
import { NotificationService } from '../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import {
  DirectoryContentsResponse,
  DirectoryDetailsResponse,
  DirectoryTreeResponse,
  DocumentDetailsResponse,
  MAX_ATTACHMENT_BYTES,
  MAX_ATTACHMENT_LABEL,
  OTHER_DOCUMENT_TYPE_ID,
} from '../../../types/document';
import { fileIcon, formatFileSize } from '../../../utils/file-format';

type DirRow = { kind: 'dir'; dir: DirectoryDetailsResponse };
type FileRow = { kind: 'file'; doc: DocumentDetailsResponse };
type Row = DirRow | FileRow;

type FlatDir = { dir: DirectoryDetailsResponse; depth: number };

/**
 * Reusable directory browser for any entity that owns an attachments tree.
 * Generic via the `baseUrl` input (e.g. `/api/v1/staffmembers/{id}/attachments`)
 * — the route shape is identical across entities, so one component + one data
 * service serve staff, students, bulletins, etc. The entity's root directory is
 * the browser's root; navigation never escapes it (the server scopes by subtree).
 *
 * Self-contained: loads its own data and commits every action immediately.
 * `canEdit` toggles the mutating affordances (new folder / upload / rename /
 * move / delete); the server enforces regardless.
 */
@Component({
  selector: 'mp-directory-browser',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, FormsModule, MpButton, MpDialog, MpInput, MpSpinner, MpSelect, MpTable, MpTableHeader, MpTableBody, TranslocoDirective],
  providers: [provideTranslocoScope('documents')],
  templateUrl: './directory-browser.html',
  host: { class: 'block' },
})
export class DirectoryBrowser {
  private readonly data = inject(DirectoryDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly baseUrl = input.required<string>();
  readonly canEdit = input<boolean>(false);
  /** Friendly label for the root crumb (the raw root dir name is an internal slug). */
  readonly rootLabel = input<string>('');
  /**
   * Whether folders/files created here are private (staff-only). Defaults to false
   * for a neutral file-browser. Set true where the tree is sensitive and shared —
   * e.g. staff documents hang off the Person directory, which a student/parent
   * portal could surface, so HR records must stay staff-only.
   */
  readonly defaultPrivate = input<boolean>(false);
  /**
   * Document types offered for upload classification (already facet-filtered by
   * the caller, e.g. Staff types only). When empty, the type picker / column are
   * hidden and uploads fall back to whatever the server defaults to. Keeping the
   * fetch in the host lets each entity scope the catalogue to its own facet.
   */
  readonly documentTypes = input<LookupResponse[]>([]);

  private readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('fileInput');

  // The type applied to the next upload batch. Toolbar-level (not per-file) to
  // stay low-friction; per-row "change type" handles reclassifying afterwards.
  readonly selectedTypeId = signal<string | null>(null);
  readonly hasTypes = computed(() => this.documentTypes().length > 0);
  private readonly typeNameById = computed(
    () => new Map(this.documentTypes().map(t => [t.id, t.description])),
  );

  typeLabel(doc: DocumentDetailsResponse): string {
    return this.typeNameById().get(doc.typeId) ?? doc.typeDescription;
  }

  // Breadcrumb chain root→current; the current directory is the last element.
  readonly path = signal<DirectoryDetailsResponse[]>([]);
  // Browser-style history: each entry is a whole path snapshot.
  private readonly backStack = signal<DirectoryDetailsResponse[][]>([]);
  private readonly fwdStack = signal<DirectoryDetailsResponse[][]>([]);

  readonly directories = signal<DirectoryDetailsResponse[]>([]);
  readonly documents = signal<DocumentDetailsResponse[]>([]);
  readonly loading = signal(false);
  readonly uploading = signal(false);
  readonly dragOver = signal(false);

  readonly sizeLabel = MAX_ATTACHMENT_LABEL;
  protected readonly fileIcon = fileIcon;
  protected readonly formatFileSize = formatFileSize;

  readonly current = computed(() => this.path().at(-1) ?? null);
  readonly canBack = computed(() => this.backStack().length > 0);
  readonly canForward = computed(() => this.fwdStack().length > 0);
  readonly canUp = computed(() => this.path().length > 1);

  readonly rows = computed<Row[]>(() => [
    ...this.directories().map(d => ({ kind: 'dir', dir: d }) as DirRow),
    ...this.documents().map(d => ({ kind: 'file', doc: d }) as FileRow),
  ]);

  readonly crumbs = computed(() => {
    const root = this.rootLabel() || this.transloco.translate('documents.rootLabel');
    return this.path().map((dir, i) => ({ dir, label: i === 0 ? root : dir.name }));
  });

  readonly nameDialogOpen = signal(false);
  readonly nameDialogMode = signal<'new' | 'rename'>('new');
  readonly nameValue = signal('');
  private readonly renameTarget = signal<DirectoryDetailsResponse | null>(null);
  readonly nameSaving = signal(false);

  readonly moveDialogOpen = signal(false);
  readonly moveTarget = signal<Row | null>(null);
  readonly moveTree = signal<FlatDir[]>([]);
  readonly moveSelectedId = signal<string | null>(null);
  readonly moveSaving = signal(false);
  readonly moveTargetName = computed(() => {
    const r = this.moveTarget();
    if (!r) return '';
    return r.kind === 'dir' ? r.dir.name : (r.doc.title ?? r.doc.fileName);
  });
  // The container the item already lives in — offered as a disabled "(current)" row.
  readonly moveCurrentContainerId = computed(() => {
    const r = this.moveTarget();
    if (!r) return null;
    return r.kind === 'dir' ? (r.dir.parentId ?? null) : r.doc.directoryId;
  });

  constructor() {
    effect(() => {
      const base = this.baseUrl();
      untracked(() => {
        if (base) this.loadRoot(base);
      });
    });

    // Default the upload type once the catalogue arrives: prefer "Other" (the
    // catch-all), else the first available. Only sets when nothing is chosen yet
    // so it doesn't clobber a user's selection.
    effect(() => {
      const types = this.documentTypes();
      untracked(() => {
        if (this.selectedTypeId() || !types.length) return;
        const other = types.find(t => t.id === OTHER_DOCUMENT_TYPE_ID);
        this.selectedTypeId.set(other?.id ?? types[0].id);
      });
    });
  }

  private loadRoot(base: string): void {
    this.loading.set(true);
    this.data.rootContents(base).subscribe({
      next: c => {
        this.path.set([c.directory]);
        this.backStack.set([]);
        this.fwdStack.set([]);
        this.applyContents(c);
        this.loading.set(false);
      },
      error: e => {
        this.loading.set(false);
        this.notify.apiError(e, this.tr('loadError'));
      },
    });
  }

  private loadContents(dir: DirectoryDetailsResponse): void {
    this.loading.set(true);
    this.data.contents(this.baseUrl(), dir.id).subscribe({
      next: c => {
        this.applyContents(c);
        this.loading.set(false);
      },
      error: e => {
        this.loading.set(false);
        this.notify.apiError(e, this.tr('loadError'));
      },
    });
  }

  private applyContents(c: DirectoryContentsResponse): void {
    this.directories.set([...c.directories].sort((a, b) => a.name.localeCompare(b.name)));
    this.documents.set([...c.documents]);
  }

  private reload(): void {
    const cur = this.current();
    if (cur) this.loadContents(cur);
  }

  private setPath(newPath: DirectoryDetailsResponse[]): void {
    this.backStack.update(s => [...s, this.path()]);
    this.fwdStack.set([]);
    this.path.set(newPath);
    const last = newPath.at(-1);
    if (last) this.loadContents(last);
  }

  openFolder(dir: DirectoryDetailsResponse): void {
    this.setPath([...this.path(), dir]);
  }

  goToCrumb(index: number): void {
    if (index < this.path().length - 1) this.setPath(this.path().slice(0, index + 1));
  }

  up(): void {
    const p = this.path();
    if (p.length > 1) this.setPath(p.slice(0, -1));
  }

  back(): void {
    const b = this.backStack();
    if (!b.length) return;
    const prev = b[b.length - 1];
    this.backStack.set(b.slice(0, -1));
    this.fwdStack.update(s => [this.path(), ...s]);
    this.path.set(prev);
    const last = prev.at(-1);
    if (last) this.loadContents(last);
  }

  forward(): void {
    const f = this.fwdStack();
    if (!f.length) return;
    const next = f[0];
    this.fwdStack.set(f.slice(1));
    this.backStack.update(s => [...s, this.path()]);
    this.path.set(next);
    const last = next.at(-1);
    if (last) this.loadContents(last);
  }

  openNewFolder(): void {
    this.nameDialogMode.set('new');
    this.nameValue.set('');
    this.renameTarget.set(null);
    this.nameDialogOpen.set(true);
  }

  openRename(dir: DirectoryDetailsResponse): void {
    this.nameDialogMode.set('rename');
    this.nameValue.set(dir.name);
    this.renameTarget.set(dir);
    this.nameDialogOpen.set(true);
  }

  async submitName(): Promise<void> {
    const name = this.nameValue().trim();
    if (!name) return;
    this.nameSaving.set(true);
    const isNew = this.nameDialogMode() === 'new';
    try {
      if (isNew) {
        const cur = this.current();
        if (!cur) return;
        await firstValueFrom(this.data.createFolder(this.baseUrl(), cur.id, name, this.defaultPrivate()));
        this.notify.success(this.tr('toast.folderCreated'));
      } else {
        const target = this.renameTarget();
        if (!target) return;
        await firstValueFrom(this.data.updateFolder(this.baseUrl(), target, { name }));
        this.notify.success(this.tr('toast.folderRenamed'));
      }
      this.nameDialogOpen.set(false);
      this.reload();
    } catch (e) {
      this.notify.apiError(e, this.tr(isNew ? 'error.createFolder' : 'error.renameFolder'));
    } finally {
      this.nameSaving.set(false);
    }
  }

  async deleteFolder(dir: DirectoryDetailsResponse): Promise<void> {
    const ok = await this.confirm.danger({
      header: this.tr('deleteFolder.header'),
      message: this.transloco.translate('documents.deleteFolder.confirm', { name: dir.name }),
    });
    if (!ok) return;
    try {
      await firstValueFrom(this.data.deleteFolder(this.baseUrl(), dir.id));
      this.notify.success(this.tr('toast.folderDeleted'));
      this.reload();
    } catch (e) {
      this.notify.apiError(e, this.tr('error.deleteFolder'));
    }
  }

  async deleteFile(doc: DocumentDetailsResponse): Promise<void> {
    const name = doc.title ?? doc.fileName;
    const ok = await this.confirm.danger({
      header: this.tr('deleteFile.header'),
      message: this.transloco.translate('documents.deleteFile.confirm', { name }),
    });
    if (!ok) return;
    try {
      await firstValueFrom(this.data.deleteDocument(this.baseUrl(), doc.id));
      this.documents.update(d => d.filter(x => x.id !== doc.id));
      this.notify.success(this.tr('toast.fileDeleted'));
    } catch (e) {
      this.notify.apiError(e, this.tr('error.deleteFile'));
    }
  }

  openMove(row: Row): void {
    this.moveTarget.set(row);
    this.moveSelectedId.set(null);
    this.moveTree.set([]);
    this.moveDialogOpen.set(true);

    const rootId = this.path()[0]?.id;
    if (!rootId) return;
    const excludeId = row.kind === 'dir' ? row.dir.id : null;
    this.data.tree(this.baseUrl(), rootId).subscribe({
      next: t => {
        const out: FlatDir[] = [];
        this.flattenTree(t, 0, excludeId, out);
        this.moveTree.set(out);
      },
      error: e => this.notify.apiError(e, this.tr('loadError')),
    });
  }

  // Flatten the tree to an indented list, skipping `excludeId` and its subtree
  // (a folder can't be moved into itself or a descendant).
  private flattenTree(
    node: DirectoryTreeResponse,
    depth: number,
    excludeId: string | null,
    out: FlatDir[],
  ): void {
    if (excludeId && node.directory.id === excludeId) return;
    out.push({ dir: node.directory, depth });
    for (const child of node.directories) this.flattenTree(child, depth + 1, excludeId, out);
  }

  moveCrumbLabel(flat: FlatDir): string {
    return flat.depth === 0
      ? this.rootLabel() || this.transloco.translate('documents.rootLabel')
      : flat.dir.name;
  }

  async submitMove(): Promise<void> {
    const row = this.moveTarget();
    const target = this.moveSelectedId();
    if (!row || !target) return;
    this.moveSaving.set(true);
    try {
      if (row.kind === 'dir') {
        await firstValueFrom(this.data.updateFolder(this.baseUrl(), row.dir, { parentId: target }));
      } else {
        await firstValueFrom(this.data.moveDocument(this.baseUrl(), row.doc, target));
      }
      this.notify.success(this.tr('toast.moved'));
      this.moveDialogOpen.set(false);
      this.reload();
    } catch (e) {
      this.notify.apiError(e, this.tr('error.move'));
    } finally {
      this.moveSaving.set(false);
    }
  }

  readonly typeDialogOpen = signal(false);
  readonly typeTarget = signal<DocumentDetailsResponse | null>(null);
  readonly typeSelectedId = signal<string | null>(null);
  readonly typeSaving = signal(false);
  readonly typeTargetName = computed(() => {
    const d = this.typeTarget();
    return d ? (d.title ?? d.fileName) : '';
  });

  openChangeType(doc: DocumentDetailsResponse): void {
    this.typeTarget.set(doc);
    this.typeSelectedId.set(doc.typeId);
    this.typeDialogOpen.set(true);
  }

  async submitChangeType(): Promise<void> {
    const doc = this.typeTarget();
    const typeId = this.typeSelectedId();
    if (!doc || !typeId || typeId === doc.typeId) {
      this.typeDialogOpen.set(false);
      return;
    }
    this.typeSaving.set(true);
    try {
      await firstValueFrom(this.data.changeDocumentType(this.baseUrl(), doc, typeId));
      this.notify.success(this.tr('toast.typeChanged'));
      this.typeDialogOpen.set(false);
      this.reload();
    } catch (e) {
      this.notify.apiError(e, this.tr('error.changeType'));
    } finally {
      this.typeSaving.set(false);
    }
  }

  readonly uploadDialogOpen = signal(false);
  readonly stagedFiles = signal<File[]>([]);

  openUpload(): void {
    if (!this.canEdit()) return;
    this.stagedFiles.set([]);
    this.uploadDialogOpen.set(true);
  }

  onDragOver(event: DragEvent): void {
    if (!this.canEdit()) return;
    event.preventDefault();
    this.dragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragOver.set(false);
  }

  // Dropping on the listing opens the upload dialog with the files staged, so a
  // type can be chosen before committing (rather than uploading immediately).
  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver.set(false);
    if (!this.canEdit()) return;
    const files = event.dataTransfer?.files;
    if (files && files.length) {
      this.uploadDialogOpen.set(true);
      this.stageFiles(Array.from(files));
    }
  }

  onBrowseClick(): void {
    this.fileInput()?.nativeElement.click();
  }

  onFileInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length) {
      this.stageFiles(Array.from(input.files));
      input.value = '';
    }
  }

  removeStaged(index: number): void {
    this.stagedFiles.update(s => s.filter((_, i) => i !== index));
  }

  downloadUrl(doc: DocumentDetailsResponse): string {
    return this.data.downloadUrl(this.baseUrl(), doc.id);
  }

  // Validate size client-side and add to the pending list (deduping by name+size
  // so a double drop/browse doesn't queue the same file twice).
  private stageFiles(files: File[]): void {
    const accepted: File[] = [];
    for (const f of files) {
      if (f.size > MAX_ATTACHMENT_BYTES) {
        this.notify.warn(
          this.transloco.translate('documents.error.tooLarge', {
            name: f.name,
            limit: MAX_ATTACHMENT_LABEL,
          }),
        );
        continue;
      }
      accepted.push(f);
    }
    if (!accepted.length) return;
    this.stagedFiles.update(s => {
      const seen = new Set(s.map(f => `${f.name}:${f.size}`));
      return [...s, ...accepted.filter(f => !seen.has(`${f.name}:${f.size}`))];
    });
  }

  // Sequential to keep failure handling simple and avoid N concurrent large uploads.
  async confirmUpload(): Promise<void> {
    const cur = this.current();
    const files = this.stagedFiles();
    if (!cur || !files.length) return;
    this.uploading.set(true);
    const typeId = this.selectedTypeId() ?? OTHER_DOCUMENT_TYPE_ID;
    for (const file of files) {
      try {
        await firstValueFrom(
          this.data.upload(this.baseUrl(), cur.id, file, this.defaultPrivate(), typeId),
        );
        this.notify.success(this.transloco.translate('documents.toast.uploaded', { name: file.name }));
      } catch (e) {
        this.notify.apiError(e, this.transloco.translate('documents.error.upload', { name: file.name }));
      }
    }
    this.uploading.set(false);
    this.uploadDialogOpen.set(false);
    this.stagedFiles.set([]);
    this.reload();
  }

  private tr(key: string): string {
    return this.transloco.translate(`documents.${key}`);
  }
}
