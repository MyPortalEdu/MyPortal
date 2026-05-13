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
import { Button } from 'primeng/button';
import { Checkbox } from 'primeng/checkbox';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { BulletinsDataService } from '../../../../../shared/services/bulletins-data.service';
import { NotificationService } from '../../../../../shared/services/notification.service';
import {
  BulletinCategoryResponse,
  BulletinCategoryUpsertRequest,
} from '../../../../../shared/types/bulletin';

// Curated icon grid — Font Awesome Pro glyphs that read well as category
// markers in a school-bulletin context. Stored as `fa-regular fa-…` so the
// pills render in regular weight, matching the chip's quieter visual role.
// Free-text icon entry would invite visual chaos in the feed; admins pick
// from a known good set.
export const CATEGORY_ICONS: readonly string[] = [
  // Announcement / notice
  'fa-regular fa-megaphone',
  'fa-regular fa-bullhorn',
  'fa-regular fa-bell',
  'fa-regular fa-circle-info',
  'fa-regular fa-triangle-exclamation',
  'fa-regular fa-flag',
  // Safety / pastoral
  'fa-regular fa-shield-halved',
  'fa-regular fa-heart',
  'fa-regular fa-hand',
  'fa-regular fa-handshake',
  // Calendar / time
  'fa-regular fa-calendar',
  'fa-regular fa-calendar-day',
  'fa-regular fa-calendar-check',
  'fa-regular fa-clock',
  // Celebration
  'fa-regular fa-star',
  'fa-regular fa-trophy',
  'fa-regular fa-gift',
  'fa-regular fa-cake-candles',
  'fa-regular fa-thumbs-up',
  // Academic
  'fa-regular fa-graduation-cap',
  'fa-regular fa-book',
  'fa-regular fa-book-open',
  'fa-regular fa-pen-to-square',
  'fa-regular fa-microscope',
  'fa-regular fa-flask',
  'fa-regular fa-calculator',
  // Arts / activities
  'fa-regular fa-palette',
  'fa-regular fa-music',
  'fa-regular fa-camera',
  // Sport
  'fa-regular fa-futbol',
  'fa-regular fa-person-running',
  // School operations
  'fa-regular fa-bus',
  'fa-regular fa-utensils',
  'fa-regular fa-briefcase',
  'fa-regular fa-globe',
  'fa-regular fa-house',
  // People
  'fa-regular fa-users',
  'fa-regular fa-user',
  'fa-regular fa-id-card',
  // Docs / comms
  'fa-regular fa-clipboard',
  'fa-regular fa-file-lines',
  'fa-regular fa-folder',
  'fa-regular fa-envelope',
  'fa-regular fa-comment',
  'fa-regular fa-paperclip',
  // Generic
  'fa-regular fa-bookmark',
  'fa-regular fa-tag',
  'fa-regular fa-lightbulb',
  'fa-regular fa-bolt',
  'fa-regular fa-gear',
] as const;

// Curated swatches — primary indigo plus a spread of brand-safe accents. Hex
// is canonical because the API stores it as a fixed-length hex string. The
// active category chip in the feed tints from this via `${hex}1A` for the
// 10% background, so keep these as 7-char `#RRGGBB`.
export const CATEGORY_COLOURS: readonly string[] = [
  '#4f46e5', // indigo-600 (matches app primary)
  '#2563eb', // blue-600
  '#0891b2', // cyan-600
  '#059669', // emerald-600
  '#65a30d', // lime-600
  '#ca8a04', // yellow-600
  '#ea580c', // orange-600
  '#dc2626', // red-600
  '#db2777', // pink-600
  '#7c3aed', // violet-600
];

@Component({
  selector: 'mp-bulletin-category-form-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Button, Checkbox, Dialog, InputText, InputNumber, TranslocoDirective, TranslocoPipe],
  providers: [provideTranslocoScope('bulletin-settings')],
  templateUrl: './bulletin-category-form-dialog.html',
})
export class BulletinCategoryFormDialog {
  private readonly data = inject(BulletinsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly visible = input.required<boolean>();
  /** Pre-fills fields for edit mode; null = create. */
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

  readonly isEdit = computed(() => this.existing() !== null);

  readonly isValid = computed(() => this.name().trim().length > 0);

  constructor() {
    effect(() => {
      if (this.visible()) {
        untracked(() => this.reset());
      }
    });
  }

  onCancel(): void {
    this.closed.emit();
  }

  onHide(): void {
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
      return;
    }
    this.name.set('');
    this.icon.set(CATEGORY_ICONS[0]);
    this.colour.set(CATEGORY_COLOURS[0]);
    this.displayOrder.set(100);
    this.active.set(true);
  }
}
