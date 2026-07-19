import { ChangeDetectionStrategy, Component, computed, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'mp-select-button',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MpSelectButton), multi: true }],
  host: { class: 'inline-block' },
  templateUrl: './mp-select-button.html',
})
export class MpSelectButton implements ControlValueAccessor {
  readonly options = input<readonly unknown[]>([]);
  readonly optionLabel = input<string | undefined>(undefined);
  readonly optionValue = input<string | undefined>(undefined);
  readonly allowEmpty = input(true);
  readonly disabledInput = input(false, { alias: 'disabled' });

  protected readonly value = signal<unknown>(null);
  private readonly cvaDisabled = signal(false);
  protected readonly disabled = computed(() => this.disabledInput() || this.cvaDisabled());

  protected onChange: (value: unknown) => void = () => {};
  protected onTouched: () => void = () => {};

  protected readonly labelOf = (opt: unknown): string => {
    const key = this.optionLabel();
    return key ? String((opt as Record<string, unknown>)?.[key] ?? '') : String(opt ?? '');
  };
  protected readonly valueOf = (opt: unknown): unknown => {
    const key = this.optionValue();
    return key ? (opt as Record<string, unknown>)?.[key] : opt;
  };

  protected select(optValue: unknown): void {
    if (this.disabled()) return;
    const next = this.allowEmpty() && optValue === this.value() ? null : optValue;
    this.value.set(next);
    this.onChange(next);
    this.onTouched();
  }

  protected btnClass(active: boolean): string {
    return (
      'inline-flex items-center justify-center rounded-[3px] px-3 py-1 text-sm font-medium outline-none ' +
      'transition-colors disabled:pointer-events-none disabled:opacity-50 ' +
      (active
        ? 'bg-primary text-primary-foreground shadow-sm'
        : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground')
    );
  }

  writeValue(value: unknown): void {
    this.value.set(value ?? null);
  }
  registerOnChange(fn: (value: unknown) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.cvaDisabled.set(isDisabled);
  }
}
