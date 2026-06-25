import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  model,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Select } from 'primeng/select';

import { GENDER_CODES } from '../../constants/gender';

/**
 * Reusable gender dropdown for any Person form (staff, students, contacts…).
 * Two-way bind the single-char code via `[value]`/`(valueChange)`. Options and
 * placeholder come from the shared `common.gender.*` i18n keys. For read-only
 * display use the `genderLabel` pipe instead.
 */
@Component({
  selector: 'mp-gender-select',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Select],
  template: `
    <p-select
      [inputId]="inputId()"
      [options]="options()"
      optionLabel="label"
      optionValue="value"
      [placeholder]="placeholder() ?? defaultPlaceholder()"
      [ngModel]="value()"
      (ngModelChange)="value.set($event)"></p-select>
  `,
})
export class GenderSelect {
  private readonly transloco = inject(TranslocoService);

  /** The selected single-char gender code (M/F/X), two-way bound. */
  readonly value = model<string | null>(null);

  /** Forwarded to the native control for label association. */
  readonly inputId = input<string>();

  /** Optional placeholder override; defaults to "Select gender". */
  readonly placeholder = input<string>();

  protected readonly defaultPlaceholder = computed(() =>
    this.transloco.translate('common.gender.select'),
  );

  protected readonly options = computed(() =>
    GENDER_CODES.map(code => ({
      value: code,
      label: this.transloco.translate(`common.gender.${code}`),
    })),
  );
}
