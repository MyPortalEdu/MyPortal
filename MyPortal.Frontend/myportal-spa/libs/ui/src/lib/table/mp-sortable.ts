import { ChangeDetectionStrategy, Component, Directive, Signal, inject, input } from '@angular/core';

export abstract class MpSortHost {
  abstract readonly activeSortField: Signal<string | null>;
  abstract readonly activeSortOrder: Signal<number>;
  abstract sort(field: string): void;
}

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
