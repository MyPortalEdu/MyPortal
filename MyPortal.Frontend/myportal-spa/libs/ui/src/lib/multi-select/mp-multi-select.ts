import { ChangeDetectionStrategy, Component, computed, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { BrnPopoverImports } from '@spartan-ng/brain/popover';
import { cn } from '../utils/cn';
import { MpPopover } from '../popover/mp-popover';

/**
 * Multi-select dropdown — the design-system equivalent of `p-multiSelect` with `display="chip"`.
 * Data-driven (`[options]`/`optionLabel`/`optionValue`), CVA over an array of selected values.
 * Selected items show as removable chips in the trigger; the panel is a checkable list, opened in
 * the edge-aware MpPopover overlay.
 */
@Component({
  selector: 'mp-multi-select',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [BrnPopoverImports, MpPopover],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MpMultiSelect), multi: true }],
  host: { class: 'block' },
  templateUrl: './mp-multi-select.html',
})
export class MpMultiSelect implements ControlValueAccessor {
  readonly options = input<readonly unknown[]>([]);
  readonly optionLabel = input<string | undefined>(undefined);
  readonly optionValue = input<string | undefined>(undefined);
  readonly placeholder = input<string | undefined>(undefined);
  readonly invalid = input<boolean | null | undefined>(false);
  readonly inputId = input<string | undefined>(undefined);
  readonly disabledInput = input(false, { alias: 'disabled' });

  protected readonly value = signal<unknown[]>([]);
  private readonly cvaDisabled = signal(false);
  protected readonly disabled = computed(() => this.disabledInput() || this.cvaDisabled());

  protected onChange: (value: unknown[]) => void = () => {};
  protected onTouched: () => void = () => {};

  protected readonly labelOf = (opt: unknown): string => {
    const key = this.optionLabel();
    return key ? String((opt as Record<string, unknown>)?.[key] ?? '') : String(opt ?? '');
  };
  protected readonly valueOf = (opt: unknown): unknown => {
    const key = this.optionValue();
    return key ? (opt as Record<string, unknown>)?.[key] : opt;
  };

  protected readonly selectedOptions = computed(() =>
    this.options().filter((o) => this.value().includes(this.valueOf(o))),
  );

  protected isSelected(optValue: unknown): boolean {
    return this.value().includes(optValue);
  }

  protected toggle(optValue: unknown): void {
    if (this.disabled()) return;
    const next = this.isSelected(optValue)
      ? this.value().filter((v) => v !== optValue)
      : [...this.value(), optValue];
    this.commit(next);
  }

  protected remove(optValue: unknown, event: Event): void {
    event.stopPropagation();
    if (this.disabled()) return;
    this.commit(this.value().filter((v) => v !== optValue));
  }

  private commit(next: unknown[]): void {
    this.value.set(next);
    this.onChange(next);
    this.onTouched();
  }

  protected readonly triggerClass = computed(() =>
    cn(
      'flex min-h-8 w-full flex-wrap items-center gap-1 rounded-control border border-input bg-background ' +
        'px-2 py-1 text-sm outline-none transition-colors hover:border-[var(--p-form-field-hover-border-color)] ' +
        'focus:border-ring focus:ring-2 focus:ring-ring/40 aria-expanded:border-ring aria-expanded:ring-2 aria-expanded:ring-ring/40',
      this.invalid() ? 'border-destructive' : '',
      this.disabled() ? 'cursor-not-allowed opacity-50' : 'cursor-pointer',
    ),
  );

  protected readonly contentClass =
    'z-50 max-h-72 min-w-[14rem] max-w-[22rem] overflow-y-auto rounded-surface ' +
    'border border-border bg-background p-1 shadow-overlay';

  protected readonly itemClass =
    'flex w-full items-center gap-2 rounded-control px-2 py-1.5 text-sm text-left outline-none ' +
    'cursor-pointer hover:bg-accent hover:text-accent-foreground';

  writeValue(value: unknown[] | null): void {
    this.value.set(Array.isArray(value) ? value : []);
  }
  registerOnChange(fn: (value: unknown[]) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.cvaDisabled.set(isDisabled);
  }
}
