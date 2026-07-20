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
import { MpSelect } from '@myportal/ui';

import { GENDER_CODES } from '../../constants/gender';

@Component({
  selector: 'mp-gender-select',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpSelect],
  template: `
    <mp-select
      [inputId]="inputId()"
      [options]="options()"
      optionLabel="label"
      optionValue="value"
      [placeholder]="placeholder() ?? defaultPlaceholder()"
      [ngModel]="value()"
      (ngModelChange)="value.set($event)"
      [invalid]="invalid()"
      class="w-full"></mp-select>
  `,
})
export class GenderSelect {
  private readonly transloco = inject(TranslocoService);

  readonly value = model<string | null>(null);

  readonly inputId = input<string>();

  readonly placeholder = input<string>();

  readonly invalid = input<boolean>(false);

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
