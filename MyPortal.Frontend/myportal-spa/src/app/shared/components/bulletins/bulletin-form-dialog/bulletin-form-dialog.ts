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
import { MpSelect, MpDialog, MpDialogFooter, MpButton, MpInput, MpTextarea, MpCheckbox, MpDatePicker } from '@myportal/ui';
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
  key: string;
  labelKey: string;
  fallbackLabel?: string;
  kind: BulletinAudienceKind;
  studentGroupId?: string;
}

type FormSnapshot = {
  title: string;
  detail: string;
  categoryId: string | null;
  isPinned: boolean;
  requiresAck: boolean;
  expiresAt: string | null;
  audienceKeys: string[];
};

@Component({
  selector: 'mp-bulletin-form-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpDialog, MpDialogFooter, MpButton, MpInput, MpTextarea, MpSelect, MpCheckbox, MpDatePicker, TranslocoDirective, TranslocoPipe, BulletinAttachments],
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

  readonly existing = input<BulletinDetailsResponse | null>(null);

  readonly closed = output<void>();
  readonly saved = output<void>();

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
  readonly minExpiryDate = signal<Date>(new Date());
  readonly categories = signal<BulletinCategoryResponse[]>([]);
  readonly allowedGroups = signal<BulletinAllowedGroupResponse[]>([]);

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
    const choices: AudienceChoice[] = [
      { key: 'all-staff',   labelKey: 'allStaff',   kind: BulletinAudienceKind.AllStaff },
      { key: 'all-pupils',  labelKey: 'allPupils',  kind: BulletinAudienceKind.AllPupils },
      { key: 'all-parents', labelKey: 'allParents', kind: BulletinAudienceKind.AllParents },
    ];

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
    return choice.fallbackLabel ?? this.transloco.translate(`bulletins.audience.${choice.labelKey}`);
  }

  async onCancel(): Promise<void> {
    await this.requestClose();
  }

  onHide(): void {
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
      expectedVersion: existing?.version ?? 0,
    };

    const isEdit = existing !== null;
    const t = (key: string) => this.transloco.translate(`bulletins.form.${key}`);
    const finishOk = () => {
      this.submitting.set(false);
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
      this.title.set(existing.title);
      this.detail.set(existing.detail);
      this.categoryId.set(existing.categoryId);
      this.isPinned.set(existing.pinnedAt !== null);
      this.requiresAck.set(existing.requiresAcknowledgement);
      this.expiresAt.set(existing.expiresAt ? new Date(existing.expiresAt) : null);
      this.selectedAudienceKeys.set(audienceKeysFor(existing.audiences));
    } else {
      this.title.set('');
      this.detail.set('');
      this.isPinned.set(false);
      this.requiresAck.set(false);
      this.expiresAt.set(null);
      this.selectedAudienceKeys.set(new Set(['all-staff']));
      this.categoryId.set(null);
    }
    this.touchedFields.set(new Set());
    this.snapshot.set(this.currentForm());
  }

  private loadDependencies(): void {
    this.meService.me().subscribe(me => {
      this.canPin.set(!!me.permissions?.includes(Permissions.School.PinSchoolBulletins));
    });

    this.data.listCategories(false).subscribe({
      next: cats => {
        this.categories.set(cats ?? []);
        if (cats?.length && this.categoryId() === null) {
          this.categoryId.set(cats[0].id);
          this.snapshot.set(this.currentForm());
        }
      },
      error: () => this.categories.set([]),
    });

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
