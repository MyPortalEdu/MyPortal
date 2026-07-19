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
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MpButton, MpCheckbox, MpDialog, MpDialogFooter, MpInput, MpInputNumber } from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { BulletinsDataService } from '../../../../../shared/services/bulletins-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import {
  BulletinCategoryResponse,
  BulletinCategoryUpsertRequest,
} from '../../../../../shared/types/bulletin';

type FormSnapshot = {
  name: string;
  icon: string;
  colour: string;
  displayOrder: number;
  active: boolean;
};

export const CATEGORY_ICONS: readonly string[] = [
  'fa-regular fa-megaphone',
  'fa-regular fa-bullhorn',
  'fa-regular fa-bell',
  'fa-regular fa-circle-info',
  'fa-regular fa-triangle-exclamation',
  'fa-regular fa-flag',
  'fa-regular fa-shield-halved',
  'fa-regular fa-heart',
  'fa-regular fa-hand',
  'fa-regular fa-handshake',
  'fa-regular fa-calendar',
  'fa-regular fa-calendar-day',
  'fa-regular fa-calendar-check',
  'fa-regular fa-clock',
  'fa-regular fa-star',
  'fa-regular fa-trophy',
  'fa-regular fa-gift',
  'fa-regular fa-cake-candles',
  'fa-regular fa-thumbs-up',
  'fa-regular fa-graduation-cap',
  'fa-regular fa-book',
  'fa-regular fa-book-open',
  'fa-regular fa-pen-to-square',
  'fa-regular fa-microscope',
  'fa-regular fa-flask',
  'fa-regular fa-calculator',
  'fa-regular fa-palette',
  'fa-regular fa-music',
  'fa-regular fa-camera',
  'fa-regular fa-futbol',
  'fa-regular fa-person-running',
  'fa-regular fa-bus',
  'fa-regular fa-utensils',
  'fa-regular fa-briefcase',
  'fa-regular fa-globe',
  'fa-regular fa-house',
  'fa-regular fa-users',
  'fa-regular fa-user',
  'fa-regular fa-id-card',
  'fa-regular fa-clipboard',
  'fa-regular fa-file-lines',
  'fa-regular fa-folder',
  'fa-regular fa-envelope',
  'fa-regular fa-comment',
  'fa-regular fa-paperclip',
  'fa-regular fa-bookmark',
  'fa-regular fa-tag',
  'fa-regular fa-lightbulb',
  'fa-regular fa-bolt',
  'fa-regular fa-gear',
] as const;

export const CATEGORY_COLOURS: readonly string[] = [
  '#4f46e5',
  '#2563eb',
  '#0891b2',
  '#059669',
  '#65a30d',
  '#ca8a04',
  '#ea580c',
  '#dc2626',
  '#db2777',
  '#7c3aed',
];

@Component({
  selector: 'mp-bulletin-category-form-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpButton, MpCheckbox, MpDialog, MpDialogFooter, MpInput, MpInputNumber, TranslocoDirective, TranslocoPipe],
  providers: [provideTranslocoScope('bulletin-settings')],
  templateUrl: './bulletin-category-form-dialog.html',
})
export class BulletinCategoryFormDialog {
  private readonly data = inject(BulletinsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly confirmDialog = inject(ConfirmationDialog);

  readonly visible = input.required<boolean>();
  readonly existing = input<BulletinCategoryResponse | null>(null);

  readonly closed = output<void>();
  readonly saved = output<void>();

  readonly icons = CATEGORY_ICONS;
  readonly colours = CATEGORY_COLOURS;

  readonly name = signal('');
  readonly icon = signal<string>(CATEGORY_ICONS[0]);
  readonly colour = signal<string>(CATEGORY_COLOURS[0]);
  readonly displayOrder = signal<number>(100);
  readonly active = signal(true);
  readonly submitting = signal(false);

  readonly touchedFields = signal<ReadonlySet<string>>(new Set());

  markTouched(field: string): void {
    if (this.touchedFields().has(field)) return;
    this.touchedFields.update(s => new Set(s).add(field));
  }

  wasTouched(field: string): boolean {
    return this.touchedFields().has(field);
  }

  readonly isEdit = computed(() => this.existing() !== null);

  readonly isValid = computed(() => this.name().trim().length > 0);

  private readonly snapshot = signal<FormSnapshot | null>(null);

  private readonly currentForm = computed<FormSnapshot>(() => ({
    name: this.name(),
    icon: this.icon(),
    colour: this.colour(),
    displayOrder: this.displayOrder(),
    active: this.active(),
  }));

  readonly isDirty = computed(() => {
    const s = this.snapshot();
    if (!s) return false;
    return JSON.stringify(s) !== JSON.stringify(this.currentForm());
  });

  constructor() {
    effect(() => {
      if (this.visible()) {
        untracked(() => this.reset());
      }
    });
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

  save(): void {
    if (!this.isValid() || this.submitting()) return;
    this.submitting.set(true);

    const existing = this.existing();
    const payload: BulletinCategoryUpsertRequest = {
      name: this.name().trim(),
      icon: this.icon(),
      colourCode: this.colour(),
      displayOrder: this.displayOrder(),
      active: this.active(),
      expectedVersion: existing?.version ?? 0,
    };

    const t = (key: string) => this.transloco.translate(`bulletin-settings.form.${key}`);
    const onSuccess = () => {
      this.submitting.set(false);
      this.snapshot.set(this.currentForm());
      this.notify.success(t(existing ? 'updatedToast' : 'createdToast'));
      this.saved.emit();
    };
    const onError = (err: unknown) => {
      this.submitting.set(false);
      this.notify.apiError(err, t(existing ? 'errorUpdate' : 'errorCreate'));
    };

    if (existing) {
      this.data.updateCategory(existing.id, payload).subscribe({ next: onSuccess, error: onError });
    } else {
      this.data.createCategory(payload).subscribe({ next: onSuccess, error: onError });
    }
  }

  private reset(): void {
    const existing = this.existing();
    if (existing) {
      this.name.set(existing.name);
      this.icon.set(existing.icon || CATEGORY_ICONS[0]);
      this.colour.set(existing.colourCode || CATEGORY_COLOURS[0]);
      this.displayOrder.set(existing.displayOrder);
      this.active.set(existing.active);
    } else {
      this.name.set('');
      this.icon.set(CATEGORY_ICONS[0]);
      this.colour.set(CATEGORY_COLOURS[0]);
      this.displayOrder.set(100);
      this.active.set(true);
    }
    this.touchedFields.set(new Set());
    this.snapshot.set(this.currentForm());
  }
}
