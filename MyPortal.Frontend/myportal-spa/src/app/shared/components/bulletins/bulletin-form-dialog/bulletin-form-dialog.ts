import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MpSelect, MpDialog, MpButton, MpInput, MpTextarea, MpCheckbox, MpDatePicker } from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { BulletinsDataService } from '../../../services/bulletins-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { MeService } from '../../../../core/services/me-service';
import { Permissions } from '../../../../core/constants/permissions';
import { BulletinAttachments } from '../bulletin-attachments/bulletin-attachments';
import {
  BulletinAllowedGroupResponse,
  BulletinAudienceKind,
  BulletinAudienceRequest,
  BulletinAudienceResponse,
  BulletinCategoryResponse,
  BulletinDetailsResponse,
  BulletinUpsertRequest,
} from '../../../types/bulletin';

interface AudienceChoice {
  key: string;            // dedup key
  labelKey: string;       // bulletins.audience.<key>
  fallbackLabel?: string; // for student groups whose names come from the API
  kind: BulletinAudienceKind;
  studentGroupId?: string;
}

type FormSnapshot = {
  title: string;
  detail: string;
  categoryId: string | null;
  isPinned: boolean;
  requiresAck: boolean;
  // ISO string rather than Date so JSON.stringify is deterministic across
  // re-renders without relying on Date instance identity.
  expiresAt: string | null;
  // Sets aren't JSON-stringifiable (serialise as {}); flatten to a sorted
  // array so the snapshot compare is stable regardless of insertion order.
  audienceKeys: string[];
};

@Component({
  selector: 'mp-bulletin-form-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpDialog, MpButton, MpInput, MpTextarea, MpSelect, MpCheckbox, MpDatePicker, TranslocoDirective, TranslocoPipe, BulletinAttachments],
  providers: [provideTranslocoScope('bulletins')],
  templateUrl: './bulletin-form-dialog.html',
})
export class BulletinFormDialog {
  private readonly data = inject(BulletinsDataService);
  private readonly meService = inject(MeService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly confirmDialog = inject(ConfirmationDialog);

  readonly visible = input.required<boolean>();

  /**
   * When set, the dialog opens in edit mode: fields pre-fill from this bulletin,
   * the submit calls update() instead of create(), and ExpectedVersion is sent
   * so optimistic concurrency catches racing edits. Leave null for create mode.
   */
  readonly existing = input<BulletinDetailsResponse | null>(null);

  readonly closed = output<void>();
  readonly saved = output<void>();

  // Used to flush staged attachments after a create succeeds, or to read its
  // existing-attachments state in edit mode (it manages that itself).
  readonly attachments = viewChild(BulletinAttachments);

  readonly title = signal('');
  readonly detail = signal('');
  readonly categoryId = signal<string | null>(null);
  readonly isPinned = signal(false);
  readonly requiresAck = signal(false);
  readonly expiresAt = signal<Date | null>(null);
  readonly selectedAudienceKeys = signal<Set<string>>(new Set());
  readonly submitting = signal(false);
  readonly canPin = signal(false);
  // Server validator rejects past dates — mirror that on the picker so users
  // can't pick an instant we'd reject anyway. Refreshed each open() so the
  // floor tracks real time across long-lived sessions.
  readonly minExpiryDate = signal<Date>(new Date());
  // Categories live inside the dialog now rather than as an @Input. With OnPush
  // and the parent's signal-fed binding, the Select could latch onto an empty
  // array on first render and the "No results found" message stayed visible.
  // Owning the fetch here means the options array is always live when the panel
  // opens — and the shared shareReplay() in the data service avoids a second
  // round-trip when the feed has already loaded them.
  readonly categories = signal<BulletinCategoryResponse[]>([]);
  readonly allowedGroups = signal<BulletinAllowedGroupResponse[]>([]);

  // Manually track which fields the user has blurred. We can't rely on
  // ngModel's touched state for invalid styling because some PrimeNG inputs
  // (e.g. p-select) call onModelChange inside writeValue, which Angular
  // interprets as a user change and flips ng-dirty on mount — making the
  // field look invalid before the user has touched it. Reset on each dialog
  // open via reset().
  readonly touchedFields = signal<ReadonlySet<string>>(new Set());

  markTouched(field: string): void {
    if (this.touchedFields().has(field)) return;
    this.touchedFields.update(s => new Set(s).add(field));
  }

  wasTouched(field: string): boolean {
    return this.touchedFields().has(field);
  }

  readonly isEdit = computed(() => this.existing() !== null);

  readonly audienceChoices = computed<AudienceChoice[]>(() => {
    // Fixed audiences come first so they read like a sentence: "all staff / all
    // pupils / all parents", then any allowlisted student groups.
    const choices: AudienceChoice[] = [
      { key: 'all-staff',   labelKey: 'allStaff',   kind: BulletinAudienceKind.AllStaff },
      { key: 'all-pupils',  labelKey: 'allPupils',  kind: BulletinAudienceKind.AllPupils },
      { key: 'all-parents', labelKey: 'allParents', kind: BulletinAudienceKind.AllParents },
    ];

    // De-dup student-group entries by id: the allowlist is the canonical source,
    // but when editing we also surface any group the bulletin currently targets
    // even if the admin has since removed it from the allowlist — otherwise the
    // editor couldn't see (or untick) it.
    const groups = new Map<string, AudienceChoice>();
    for (const g of this.allowedGroups()) {
      groups.set(g.studentGroupId, {
        key: `sg-${g.studentGroupId}`,
        labelKey: 'group',
        fallbackLabel: g.name,
        kind: BulletinAudienceKind.StudentGroup,
        studentGroupId: g.studentGroupId,
      });
    }
    const existing = this.existing();
    if (existing) {
      for (const a of existing.audiences) {
        if (a.audienceKind === BulletinAudienceKind.StudentGroup
            && a.studentGroupId
            && !groups.has(a.studentGroupId)) {
          groups.set(a.studentGroupId, {
            key: `sg-${a.studentGroupId}`,
            labelKey: 'group',
            fallbackLabel: a.studentGroupName ?? undefined,
            kind: BulletinAudienceKind.StudentGroup,
            studentGroupId: a.studentGroupId,
          });
        }
      }
    }

    return [...choices, ...groups.values()];
  });

  readonly isValid = computed(() =>
    this.title().trim().length > 0 &&
    this.detail().trim().length > 0 &&
    this.categoryId() !== null &&
    this.selectedAudienceKeys().size > 0,
  );

  private readonly snapshot = signal<FormSnapshot | null>(null);

  private readonly currentForm = computed<FormSnapshot>(() => ({
    title: this.title(),
    detail: this.detail(),
    categoryId: this.categoryId(),
    isPinned: this.isPinned(),
    requiresAck: this.requiresAck(),
    expiresAt: this.expiresAt()?.toISOString() ?? null,
    audienceKeys: [...this.selectedAudienceKeys()].sort(),
  }));

  readonly isDirty = computed(() => {
    const s = this.snapshot();
    if (!s) return false;
    return JSON.stringify(s) !== JSON.stringify(this.currentForm());
  });

  constructor() {
    // Watch the open transition. Reads of `existing()` happen inside `untracked`
    // so an edit-mode parent that updates `existing` while the dialog is open
    // doesn't re-trigger the reset (matches the old ngOnChanges semantics).
    effect(() => {
      if (this.visible()) {
        untracked(() => {
          this.reset();
          this.loadDependencies();
        });
      }
    });
  }

  isSelected(key: string): boolean {
    return this.selectedAudienceKeys().has(key);
  }

  toggleAudience(key: string): void {
    this.selectedAudienceKeys.update(set => {
      const next = new Set(set);
      if (next.has(key)) next.delete(key);
      else next.add(key);
      return next;
    });
  }

  audienceLabel(choice: AudienceChoice): string {
    // Student groups use the live name from the API rather than a translation
    // (group names are tenant data, not UI copy). Fixed audiences resolve to
    // the per-locale "All staff/pupils/parents" string.
    return choice.fallbackLabel ?? this.transloco.translate(`bulletins.audience.${choice.labelKey}`);
  }

  async onCancel(): Promise<void> {
    await this.requestClose();
  }

  onHide(): void {
    // PrimeNG fires onHide both when the user triggers a close (Escape) and
    // when the parent flips `visible` to false in response to our own
    // `closed`/`saved` events. We can't re-prompt here without re-asking the
    // user who just clicked Discard — and Escape is gated by closeOnEscape so
    // it only fires on a clean form anyway. Just keep the parent in sync.
    this.closed.emit();
  }

  private async requestClose(): Promise<void> {
    if (this.isDirty()) {
      const ok = await this.confirmDialog.confirm({
        header: this.transloco.translate('common.discardChanges'),
        message: this.transloco.translate('common.discardConfirm'),
        acceptLabel: this.transloco.translate('common.discard'),
        acceptSeverity: 'danger',
      });
      if (!ok) return;
    }
    this.closed.emit();
  }

  publish(): void {
    if (!this.isValid() || this.submitting()) return;
    this.submitting.set(true);

    const audiences: BulletinAudienceRequest[] = this.audienceChoices()
      .filter(c => this.selectedAudienceKeys().has(c.key))
      .map(c => ({
        audienceKind: c.kind,
        studentGroupId: c.studentGroupId ?? null,
      }));

    const existing = this.existing();
    const payload: BulletinUpsertRequest = {
      title: this.title().trim(),
      detail: this.detail().trim(),
      categoryId: this.categoryId()!,
      isPinned: this.isPinned(),
      requiresAcknowledgement: this.requiresAck(),
      audiences,
      expiresAt: this.expiresAt()?.toISOString() ?? null,
      // ExpectedVersion is irrelevant on create (server ignores it on POST)
      // but required on update for the optimistic-concurrency guard.
      expectedVersion: existing?.version ?? 0,
    };

    const isEdit = existing !== null;
    const t = (key: string) => this.transloco.translate(`bulletins.form.${key}`);
    const finishOk = () => {
      this.submitting.set(false);
      // Re-baseline before emitting saved so the parent-driven close doesn't
      // see isDirty=true and prompt the user.
      this.snapshot.set(this.currentForm());
      this.notify.success(t(isEdit ? 'updatedToast' : 'publishedToast'));
      this.saved.emit();
    };
    const onError = () => {
      this.submitting.set(false);
      this.notify.error(
        t(isEdit ? 'errorUpdate' : 'errorPublish'),
        t('errorBody'),
      );
    };

    if (existing) {
      this.data.update(existing.id, payload).subscribe({ next: finishOk, error: onError });
      return;
    }

    // Create path: post the bulletin, then if files were staged we need the
    // new bulletin's directoryId to upload them — fetch the details and
    // hand the queue to the attachments component. The bulletin is already
    // published either way; failed attachment uploads don't roll it back.
    this.data.create(payload).subscribe({
      next: ({ id }) => {
        const attachments = this.attachments();
        if (attachments?.hasStaged()) {
          this.data.getById(id).subscribe({
            next: details => {
              attachments.uploadStaged(id, details.directoryId).finally(() => finishOk());
            },
            error: () => finishOk(),
          });
        } else {
          finishOk();
        }
      },
      error: onError,
    });
  }

  private reset(): void {
    this.minExpiryDate.set(new Date());
    const existing = this.existing();
    if (existing) {
      // Edit mode: hydrate every field from the bulletin we're editing.
      this.title.set(existing.title);
      this.detail.set(existing.detail);
      this.categoryId.set(existing.categoryId);
      this.isPinned.set(existing.pinnedAt !== null);
      this.requiresAck.set(existing.requiresAcknowledgement);
      this.expiresAt.set(existing.expiresAt ? new Date(existing.expiresAt) : null);
      this.selectedAudienceKeys.set(audienceKeysFor(existing.audiences));
    } else {
      // Create mode.
      this.title.set('');
      this.detail.set('');
      this.isPinned.set(false);
      this.requiresAck.set(false);
      this.expiresAt.set(null);
      // Default audience: All staff. Matches the mockup's pre-selected chip.
      this.selectedAudienceKeys.set(new Set(['all-staff']));
      // categoryId default is set after the categories call resolves (see
      // loadDependencies); setting it here from a possibly-empty cache would
      // leave the Select blank. loadDependencies re-baselines the snapshot
      // once the default lands.
      this.categoryId.set(null);
    }
    this.touchedFields.set(new Set());
    this.snapshot.set(this.currentForm());
  }

  private loadDependencies(): void {
    // Check pin permission once per open. Cheap — MeService caches.
    this.meService.me().subscribe(me => {
      this.canPin.set(!!me.permissions?.includes(Permissions.School.PinSchoolBulletins));
    });

    // Categories: the data service shareReplays this, so the feed's earlier call
    // serves the dialog without an extra round-trip. We still resubscribe here
    // because we own the dialog's category default (create mode only — edit
    // already has a categoryId set from the existing bulletin).
    this.data.listCategories(false).subscribe({
      next: cats => {
        this.categories.set(cats ?? []);
        if (cats?.length && this.categoryId() === null) {
          this.categoryId.set(cats[0].id);
          // Re-baseline now that the async default has landed; otherwise the
          // snapshot taken in reset() would see categoryId=null and the form
          // would read as dirty before the user touched anything.
          this.snapshot.set(this.currentForm());
        }
      },
      error: () => this.categories.set([]),
    });

    // Fetch the allowlist on every open in case the admin changed it since last
    // render. One extra request per dialog open — acceptable.
    this.data.getSettings().subscribe({
      next: s => this.allowedGroups.set(s.allowedAudienceGroups ?? []),
      error: () => this.allowedGroups.set([]),
    });
  }
}

function audienceKeysFor(audiences: BulletinAudienceResponse[]): Set<string> {
  const keys = new Set<string>();
  for (const a of audiences) {
    switch (a.audienceKind) {
      case BulletinAudienceKind.AllStaff:   keys.add('all-staff');   break;
      case BulletinAudienceKind.AllPupils:  keys.add('all-pupils');  break;
      case BulletinAudienceKind.AllParents: keys.add('all-parents'); break;
      case BulletinAudienceKind.StudentGroup:
        if (a.studentGroupId) keys.add(`sg-${a.studentGroupId}`);
        break;
    }
  }
  return keys;
}
