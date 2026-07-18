import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { MpInput } from '../input/mp-input';
import { MpTable } from './mp-table';

/**
 * Inline column filter — the design-system equivalent of `<p-columnFilter type="text" display="row">`.
 * Drop it in a header filter cell; typing sets that column's filter on the parent table (debounced by
 * the table's `filterDelay`) and re-fires `lazyLoad`.
 *
 * Injects MpTable directly (resolved through the projected-header content injector, like mpSortable).
 * Only the text/row variant is implemented — the menu/popover and custom-template variants aren't
 * needed by the current screens.
 */
@Component({
  selector: 'mp-column-filter',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MpInput],
  template: `
    <input
      mpInput
      type="text"
      class="h-7 w-full text-xs font-normal"
      [value]="value()"
      [placeholder]="placeholder()"
      (input)="onInput($event)" />
  `,
})
export class MpColumnFilter {
  private readonly table = inject(MpTable);
  readonly field = input.required<string>();
  readonly matchMode = input('contains');
  readonly placeholder = input('');

  protected readonly value = signal('');

  protected onInput(event: Event): void {
    const next = (event.target as HTMLInputElement).value;
    this.value.set(next);
    this.table.filter(next, this.field(), this.matchMode());
  }
}
