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

import { LookupResponse } from '../../types/lookup';

/**
 * Reusable single-select over any `LookupResponse[]` catalogue (ethnicity,
 * religion, marital status…). Two-way bind the selected id via
 * `[value]`/`(valueChange)`. Optional selects show a clear affordance; the panel
 * appends to body so it never clips inside a dialog/card. For read-only display
 * resolve the id against the same list yourself.
 */
@Component({
  selector: 'mp-lookup-select',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Select],
  template: `
    <p-select
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
      appendTo="body"
      styleClass="w-full"></p-select>
  `,
})
export class LookupSelect {
  private readonly transloco = inject(TranslocoService);

  /** The selected lookup id, two-way bound. */
  readonly value = model<string | null>(null);

  /** The catalogue to choose from. */
  readonly options = input<LookupResponse[]>([]);

  /** Forwarded to the native control for label association. */
  readonly inputId = input<string>();

  /** Optional placeholder override; defaults to "Select…". */
  readonly placeholder = input<string>();

  /** When false (default), a clear affordance lets the user blank the value. */
  readonly required = input<boolean>(false);

  /** Paints the invalid state; drive it from the owning form's validity. */
  readonly invalid = input<boolean>(false);

  protected readonly defaultPlaceholder = computed(() =>
    this.transloco.translate('common.select'),
  );
}
