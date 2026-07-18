import { ChangeDetectionStrategy, Component, computed, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

/**
 * Binary checkbox — the design-system equivalent of `p-checkbox [binary]`. A native `<input>` (kept
 * in the tab order via `sr-only`, so it stays fully keyboard/screen-reader accessible) drives a
 * styled box. Implements ControlValueAccessor, so it works with `[(ngModel)]` / reactive forms and
 * the app's `[ngModel]` + `(ngModelChange)` pattern, plus a direct `[disabled]` input.
 *
 * A binary checkbox needs no headless machinery, so this is self-contained rather than wrapping
 * Spartan's brain checkbox — brain is reserved for the genuinely-headless components (select,
 * dialog, popover, datepicker).
 */
@Component({
  selector: 'mp-checkbox',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MpCheckbox), multi: true }],
  templateUrl: './mp-checkbox.html',
})
export class MpCheckbox implements ControlValueAccessor {
  readonly id = input<string | null>(null);
  readonly disabledInput = input(false, { alias: 'disabled' });
  readonly userClass = input<ClassValue>('', { alias: 'class' });

  protected readonly checked = signal(false);
  private readonly cvaDisabled = signal(false);
  protected readonly disabled = computed(() => this.disabledInput() || this.cvaDisabled());

  protected onChange: (value: boolean) => void = () => {};
  protected onTouched: () => void = () => {};

  protected readonly wrapperClass = computed(() =>
    cn('inline-flex items-center', this.disabled() ? 'cursor-not-allowed' : 'cursor-pointer', this.userClass()),
  );

  protected readonly boxClass = computed(() =>
    cn(
      'flex h-4 w-4 shrink-0 items-center justify-center rounded-[3px] border transition-colors ' +
        'peer-focus-visible:ring-2 peer-focus-visible:ring-ring peer-focus-visible:ring-offset-1',
      this.checked() ? 'border-primary bg-primary text-primary-foreground' : 'border-input bg-background',
      this.disabled() ? 'opacity-50' : '',
    ),
  );

  protected onToggle(value: boolean): void {
    this.checked.set(value);
    this.onChange(value);
  }

  writeValue(value: boolean): void {
    this.checked.set(!!value);
  }
  registerOnChange(fn: (value: boolean) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.cvaDisabled.set(isDisabled);
  }
}
