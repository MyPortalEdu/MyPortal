import { Directive, Signal, inject, input } from '@angular/core';

export abstract class MpSelectionHost {
  abstract readonly selectionEnabled: Signal<boolean>;
  abstract selectRow(row: unknown): void;
  abstract isRowSelected(row: unknown): boolean;
  abstract toggleRow(row: unknown): void;
  abstract isRowDisabled(row: unknown): boolean;
  abstract toggleAll(): void;
  abstract readonly allSelected: Signal<boolean>;
  abstract readonly anySelected: Signal<boolean>;
}

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
