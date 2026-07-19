import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { cn } from '../utils/cn';
import { MpSelectionHost } from './mp-selectable-row';

const BOX = 'inline-flex h-4 w-4 shrink-0 items-center justify-center rounded-[3px] border align-middle transition-colors';

@Component({
  selector: 'mp-table-checkbox',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span
      role="checkbox"
      [attr.aria-checked]="checked()"
      [attr.aria-disabled]="disabled() || null"
      [class]="boxClass()"
      (click)="toggle($event)">
      @if (checked()) {
        <i class="fa-solid fa-check text-[0.6rem] text-primary-foreground"></i>
      }
    </span>
  `,
})
export class MpTableCheckbox {
  private readonly host = inject(MpSelectionHost);
  readonly row = input.required<unknown>({ alias: 'value' });
  readonly disabledInput = input(false, { alias: 'disabled' });

  protected readonly checked = computed(() => this.host.isRowSelected(this.row()));
  protected readonly disabled = computed(() => this.disabledInput() || this.host.isRowDisabled(this.row()));

  protected readonly boxClass = computed(() =>
    cn(
      BOX,
      this.checked() ? 'border-primary bg-primary' : 'border-input',
      this.disabled() ? 'pointer-events-none opacity-40' : 'cursor-pointer',
    ),
  );

  protected toggle(event: Event): void {
    event.stopPropagation();
    if (!this.disabled()) this.host.toggleRow(this.row());
  }
}

@Component({
  selector: 'mp-table-header-checkbox',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span
      role="checkbox"
      [attr.aria-checked]="checked() ? 'true' : indeterminate() ? 'mixed' : 'false'"
      [class]="boxClass()"
      (click)="toggle($event)">
      @if (checked()) {
        <i class="fa-solid fa-check text-[0.6rem] text-primary-foreground"></i>
      } @else if (indeterminate()) {
        <i class="fa-solid fa-minus text-[0.6rem] text-primary-foreground"></i>
      }
    </span>
  `,
})
export class MpTableHeaderCheckbox {
  private readonly host = inject(MpSelectionHost);

  protected readonly checked = computed(() => this.host.allSelected());
  protected readonly indeterminate = computed(() => this.host.anySelected() && !this.host.allSelected());

  protected readonly boxClass = computed(() =>
    cn(BOX, 'cursor-pointer', this.checked() || this.indeterminate() ? 'border-primary bg-primary' : 'border-input'),
  );

  protected toggle(event: Event): void {
    event.stopPropagation();
    this.host.toggleAll();
  }
}
