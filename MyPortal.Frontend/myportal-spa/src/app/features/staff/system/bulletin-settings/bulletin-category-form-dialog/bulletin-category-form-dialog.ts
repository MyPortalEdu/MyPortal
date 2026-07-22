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
import { FormField, form, maxLength, required, submit, validate } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import {
  MpButton,
  MpCheckbox,
  MpDialog,
  MpDialogFooter,
  MpFormField,
  MpInput,
  MpInputNumber,
} from '@myportal/ui';
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
  imports: [FormField, MpButton, MpCheckbox, MpDialog, MpDialogFooter, MpFormField, MpInput, MpInputNumber, TranslocoDirective, TranslocoPipe],
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
  readonly existingNames = input<string[]>([]);

  readonly closed = output<void>();
  readonly saved = output<void>();

  readonly icons = CATEGORY_ICONS;
  readonly colours = CATEGORY_COLOURS;

  protected readonly model = signal<FormSnapshot>({
    name: '',
    icon: CATEGORY_ICONS[0],
    colour: CATEGORY_COLOURS[0],
    displayOrder: 100,
    active: true,
  });
  protected readonly f = form(this.model, path => {
    required(path.name);
    maxLength(path.name, 50);
    validate(path.name, ({ value }) =>
      value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
    );
    validate(path.name, ({ value }) => {
      const name = value().trim().toLowerCase();
      return name && this.existingNames().some(n => n.trim().toLowerCase() === name)
        ? { kind: 'taken', message: 'bulletin-settings.form.nameTaken' }
        : undefined;
    });
  });

  setIcon(icon: string): void {
    this.model.update(m => ({ ...m, icon }));
  }

  setColour(colour: string): void {
    this.model.update(m => ({ ...m, colour }));
  }

  readonly isEdit = computed(() => this.existing() !== null);

  private readonly snapshot = signal<FormSnapshot | null>(null);

  readonly isDirty = computed(() => {
    const s = this.snapshot();
    if (!s) return false;
    return JSON.stringify(s) !== JSON.stringify(this.model());
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

  save(): Promise<boolean> {
    return submit(this.f, async () => {
      const existing = this.existing();
      const m = this.model();
      const payload: BulletinCategoryUpsertRequest = {
        name: m.name.trim(),
        icon: m.icon,
        colourCode: m.colour,
        displayOrder: m.displayOrder,
        active: m.active,
        expectedVersion: existing?.version ?? 0,
      };

      const t = (key: string) => this.transloco.translate(`bulletin-settings.form.${key}`);
      try {
        if (existing) {
          await firstValueFrom(this.data.updateCategory(existing.id, payload));
        } else {
          await firstValueFrom(this.data.createCategory(payload));
        }
      } catch (err) {
        this.notify.apiError(err, t(existing ? 'errorUpdate' : 'errorCreate'));
        return;
      }

      this.snapshot.set(this.model());
      this.notify.success(t(existing ? 'updatedToast' : 'createdToast'));
      this.saved.emit();
    });
  }

  private reset(): void {
    const existing = this.existing();
    this.model.set(
      existing
        ? {
            name: existing.name,
            icon: existing.icon || CATEGORY_ICONS[0],
            colour: existing.colourCode || CATEGORY_COLOURS[0],
            displayOrder: existing.displayOrder,
            active: existing.active,
          }
        : { name: '', icon: CATEGORY_ICONS[0], colour: CATEGORY_COLOURS[0], displayOrder: 100, active: true },
    );
    this.f().reset();
    this.snapshot.set(this.model());
  }
}
