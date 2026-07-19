import { Directive, Signal, inject, input } from '@angular/core';

/** The bit of MpTable that selectable rows + selection checkboxes talk to (token, to avoid a circular import). */
export abstract class MpSelectionHost {
  abstract readonly selectionEnabled: Signal<boolean>;
  /** Row click: selects (single) or toggles (multiple). */
  abstract selectRow(row: unknown): void;
  abstract isRowSelected(row: unknown): boolean;
  // --- multiple-selection (checkbox) support ---
  abstract toggleRow(row: unknown): void;
  abstract isRowDisabled(row: unknown): boolean;
  abstract toggleAll(): void;
  /** True when every selectable row is selected. */
  abstract readonly allSelected: Signal<boolean>;
  /** True when at least one selectable row is selected. */
  abstract readonly anySelected: Signal<boolean>;
}

/**
 * Selectable row — the design-system equivalent of `[pSelectableRow]`. Apply to a `<tr>` in the
 * body template; clicking selects the row (MpTable emits `rowSelect`) and marks it selected.
 */
@Directive({
  selector: '[mpSelectableRow]',
  standalone: true,
  host: {
    '[class.cursor-pointer]': 'true',
    '(click)': 'onClick()',
    '[attr.data-selected]': 'selected()',
  },
})
export class MpSelectableRow {
  private readonly host = inject(MpSelectionHost);
  readonly row = input.required<unknown>({ alias: 'mpSelectableRow' });

  protected selected(): boolean | null {
    return this.host.isRowSelected(this.row()) ? true : null;
  }

  protected onClick(): void {
    this.host.selectRow(this.row());
  }
}
