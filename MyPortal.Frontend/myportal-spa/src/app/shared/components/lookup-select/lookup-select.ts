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

import { LookupResponse } from '../../types/lookup';

@Component({
  selector: 'mp-lookup-select',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpSelect],
  template: `
    <mp-select
      [inputId]="inputId()"
      [options]="options()"
      optionLabel="description"
      optionValue="id"
      [showClear]="!required()"
      [filter]="options().length > 8"
      filterBy="description"
      [placeholder]="placeholder() ?? defaultPlaceholder()"
      [ngModel]="value()"
      (ngModelChange)="value.set($event)"
      [invalid]="invalid()"
      [touched]="touched()"
      class="w-full"></mp-select>
  `,
})
export class LookupSelect {
  private readonly transloco = inject(TranslocoService);

  readonly value = model<string | null>(null);

  readonly options = input<LookupResponse[]>([]);

  readonly inputId = input<string>();

  readonly placeholder = input<string>();

  readonly required = input<boolean>(false);

  readonly invalid = input<boolean>(false);

  readonly touched = input<boolean>(true);

  protected readonly defaultPlaceholder = computed(() =>
    this.transloco.translate('common.select'),
  );
}
