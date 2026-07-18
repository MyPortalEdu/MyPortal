import { ChangeDetectionStrategy, Component, Directive, Signal, inject, input } from '@angular/core';

/**
 * The bit of MpTable that sortable headers talk to. A token (not the concrete component) so the
 * `mpSortable` directive / `mp-sort-icon` — which render inside the *consumer's* projected header
 * template — can inject the table without a circular import. MpTable provides itself as this.
 */
export abstract class MpSortHost {
  abstract readonly activeSortField: Signal<string | null>;
  abstract readonly activeSortOrder: Signal<number>;
  abstract sort(field: string): void;
}

/**
 * Sortable column header — the design-system equivalent of `pSortableColumn`. Apply to a `<th>`;
 * clicking cycles ascending → descending → unsorted and asks the table to reload.
 */
@Directive({
  selector: '[mpSortable]',
  standalone: true,
  host: {
    class: 'cursor-pointer select-none',
    '(click)': 'onClick()',
    '[attr.aria-sort]': 'ariaSort()',
  },
})
export class MpSortable {
  private readonly host = inject(MpSortHost);
  readonly field = input.required<string>({ alias: 'mpSortable' });

  protected ariaSort(): 'ascending' | 'descending' | null {
    if (this.host.activeSortField() !== this.field()) return null;
    return this.host.activeSortOrder() === 1 ? 'ascending' : 'descending';
  }

  protected onClick(): void {
    this.host.sort(this.field());
  }
}

/**
 * Sort direction indicator — the design-system equivalent of `<p-sortIcon field="…">`. Place next
 * to a sortable column's label; shows neutral / up / down for the current sort state of `field`.
 */
@Component({
  selector: 'mp-sort-icon',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<i class="fa-solid text-xs" [class]="iconClass()"></i>`,
  host: { class: 'ml-1 inline-block' },
})
export class MpSortIcon {
  private readonly host = inject(MpSortHost);
  readonly field = input.required<string>();

  protected iconClass(): string {
    if (this.host.activeSortField() !== this.field()) return 'fa-sort opacity-40';
    return this.host.activeSortOrder() === 1 ? 'fa-sort-up' : 'fa-sort-down';
  }
}
