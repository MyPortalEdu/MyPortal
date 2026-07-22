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
import { FormField, form, maxLength, required, submit, validate } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { MpSelect, MpDialog, MpDialogFooter, MpButton, MpFormField, MpInput, MpTextarea, MpCheckbox, MpDatePicker } from '@myportal/ui';
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

interface BulletinFormModel {
  title: string;
  detail: string;
  categoryId: string | null;
  isPinned: boolean;
  requiresAck: boolean;
  expiresAt: Date | null;
  audienceKeys: string[];
}

@Component({
  selector: 'mp-bulletin-form-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField, MpDialog, MpDialogFooter, MpButton, MpFormField, MpInput, MpTextarea, MpSelect, MpCheckbox, MpDatePicker, TranslocoDirective, TranslocoPipe, BulletinAttachments],
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

  readonly canPin = signal(false);
  readonly minExpiryDate = signal<Date>(new Date());
  readonly categories = signal<BulletinCategoryResponse[]>([]);
  readonly allowedGroups = signal<BulletinAllowedGroupResponse[]>([]);

  protected readonly model = signal<BulletinFormModel>({
    title: '',
    detail: '',
    categoryId: null,
    isPinned: false,
    requiresAck: false,
    expiresAt: null,
    audienceKeys: ['all-staff'],
  });
  protected readonly f = form(this.model, path => {
    required(path.title);
    maxLength(path.title, 50);
    validate(path.title, ({ value }) =>
      value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
    );
    required(path.detail);
    maxLength(path.detail, 2000);
    validate(path.detail, ({ value }) =>
      value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
    );
    required(path.categoryId);
    validate(path.audienceKeys, ({ value }) =>
      value().length ? undefined : { kind: 'required', message: 'common.validation.required' },
    );
  });

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

  private readonly snapshot = signal<FormSnapshot | null>(null);

  private readonly formSnapshot = computed<FormSnapshot>(() => {
    const m = this.model();
    return {
      title: m.title,
      detail: m.detail,
      categoryId: m.categoryId,
      isPinned: m.isPinned,
      requiresAck: m.requiresAck,
      expiresAt: m.expiresAt?.toISOString() ?? null,
      audienceKeys: [...m.audienceKeys].sort(),
    };
  });

  readonly isDirty = computed(() => {
    const s = this.snapshot();
    if (!s) return false;
    return JSON.stringify(s) !== JSON.stringify(this.formSnapshot());
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
    return this.model().audienceKeys.includes(key);
  }

  toggleAudience(key: string): void {
    this.model.update(m => ({
      ...m,
      audienceKeys: m.audienceKeys.includes(key)
        ? m.audienceKeys.filter(k => k !== key)
        : [...m.audienceKeys, key],
    }));
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

  publish(): Promise<boolean> {
    return submit(this.f, async () => {
      const m = this.model();
      const existing = this.existing();
      const isEdit = existing !== null;
      const t = (key: string) => this.transloco.translate(`bulletins.form.${key}`);

      const audiences: BulletinAudienceRequest[] = this.audienceChoices()
        .filter(c => m.audienceKeys.includes(c.key))
        .map(c => ({
          audienceKind: c.kind,
          studentGroupId: c.studentGroupId ?? null,
        }));

      const payload: BulletinUpsertRequest = {
        title: m.title.trim(),
        detail: m.detail.trim(),
        categoryId: m.categoryId!,
        isPinned: m.isPinned,
        requiresAcknowledgement: m.requiresAck,
        audiences,
        expiresAt: m.expiresAt?.toISOString() ?? null,
        expectedVersion: existing?.version ?? 0,
      };

      try {
        if (existing) {
          await firstValueFrom(this.data.update(existing.id, payload));
        } else {
          const { id } = await firstValueFrom(this.data.create(payload));
          const attachments = this.attachments();
          if (attachments?.hasStaged()) {
            const details = await firstValueFrom(this.data.getById(id)).catch(() => null);
            if (details) await attachments.uploadStaged(id, details.directoryId);
          }
        }
      } catch {
        this.notify.error(t(isEdit ? 'errorUpdate' : 'errorPublish'), t('errorBody'));
        return;
      }

      this.snapshot.set(this.formSnapshot());
      this.notify.success(t(isEdit ? 'updatedToast' : 'publishedToast'));
      this.saved.emit();
    });
  }

  private reset(): void {
    this.minExpiryDate.set(new Date());
    const existing = this.existing();
    this.model.set(
      existing
        ? {
            title: existing.title,
            detail: existing.detail,
            categoryId: existing.categoryId,
            isPinned: existing.pinnedAt !== null,
            requiresAck: existing.requiresAcknowledgement,
            expiresAt: existing.expiresAt ? new Date(existing.expiresAt) : null,
            audienceKeys: [...audienceKeysFor(existing.audiences)],
          }
        : {
            title: '',
            detail: '',
            categoryId: null,
            isPinned: false,
            requiresAck: false,
            expiresAt: null,
            audienceKeys: ['all-staff'],
          },
    );
    this.f().reset();
    this.snapshot.set(this.formSnapshot());
  }

  private loadDependencies(): void {
    this.meService.me().subscribe(me => {
      this.canPin.set(!!me.permissions?.includes(Permissions.School.PinSchoolBulletins));
    });

    this.data.listCategories(false).subscribe({
      next: cats => {
        this.categories.set(cats ?? []);
        if (cats?.length && this.model().categoryId === null) {
          this.model.update(m => ({ ...m, categoryId: cats[0].id }));
          this.snapshot.set(this.formSnapshot());
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
