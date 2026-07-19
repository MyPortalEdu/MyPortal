import { ChangeDetectionStrategy, Component, computed, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

@Component({
  selector: 'mp-input-number',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MpInputNumber), multi: true }],
  host: { class: 'block' },
  templateUrl: './mp-input-number.html',
})
export class MpInputNumber implements ControlValueAccessor {
  readonly mode = input<'decimal' | 'currency'>('decimal');
  readonly currency = input('GBP');
  readonly locale = input('en-GB');
  readonly useGrouping = input(true);
  readonly min = input<number | undefined>(undefined);
  readonly max = input<number | undefined>(undefined);
  readonly minFractionDigits = input<number | undefined>(undefined);
  readonly maxFractionDigits = input<number | undefined>(undefined);
  readonly step = input(1);
  readonly showButtons = input(false);
  readonly invalid = input<boolean | null | undefined>(false);
  readonly touched = input<boolean>(true);
  readonly inputId = input<string | undefined>(undefined);
  readonly placeholder = input<string | undefined>(undefined);
  readonly disabledInput = input(false, { alias: 'disabled' });
  readonly userClass = input<ClassValue>('', { alias: 'class' });

  protected readonly value = signal<number | null>(null);
  protected readonly text = signal('');
  protected readonly focused = signal(false);
  private readonly cvaDisabled = signal(false);
  protected readonly disabled = computed(() => this.disabledInput() || this.cvaDisabled());

  protected onChange: (value: number | null) => void = () => {};
  protected onTouched: () => void = () => {};

  private readonly formatter = computed(
    () =>
      new Intl.NumberFormat(this.locale(), {
        style: this.mode() === 'currency' ? 'currency' : 'decimal',
        currency: this.currency(),
        useGrouping: this.useGrouping(),
        minimumFractionDigits: this.minFractionDigits(),
        maximumFractionDigits: this.maxFractionDigits() ?? Math.max(this.minFractionDigits() ?? 0, 20),
      }),
  );

  private syncText(): void {
    const v = this.value();
    if (this.focused()) this.text.set(v == null ? '' : String(v));
    else this.text.set(v == null ? '' : this.formatter().format(v));
  }

  protected onFocus(): void {
    this.focused.set(true);
    this.syncText();
  }

  protected onInput(event: Event): void {
    this.text.set((event.target as HTMLInputElement).value);
    this.commit(false);
  }

  protected onBlur(): void {
    this.focused.set(false);
    this.commit(true);
    this.syncText();
    this.onTouched();
  }

  private commit(clamp: boolean): void {
    let parsed = parseNumber(this.text());
    if (parsed !== null && clamp) parsed = this.clamp(parsed);
    if (parsed !== this.value()) {
      this.value.set(parsed);
      this.onChange(parsed);
    }
  }

  private clamp(n: number): number {
    const lo = this.min();
    const hi = this.max();
    if (lo != null && n < lo) return lo;
    if (hi != null && n > hi) return hi;
    return n;
  }

  protected spin(delta: number): void {
    if (this.disabled()) return;
    const next = this.clamp((this.value() ?? 0) + delta * this.step());
    this.value.set(next);
    this.onChange(next);
    this.onTouched();
    this.syncText();
  }

  protected readonly showInvalid = computed(() => !!this.invalid() && this.touched());

  protected readonly fieldClass = computed(() =>
    cn(
      'flex h-8 w-full rounded-control border bg-background px-2.5 py-1 text-sm outline-none transition-colors ' +
        'placeholder:text-muted-foreground hover:border-input-hover ' +
        'focus:border-ring focus:ring-2 focus:ring-ring/40 disabled:cursor-not-allowed disabled:opacity-50',
      this.showButtons() ? 'rounded-r-none' : '',
      this.showInvalid() ? 'border-destructive' : 'border-input',
    ),
  );

  protected readonly stepBtnClass =
    'flex h-4 flex-1 items-center justify-center border border-l-0 border-input text-[0.6rem] ' +
    'text-muted-foreground outline-none hover:bg-accent hover:text-accent-foreground ' +
    'disabled:pointer-events-none disabled:opacity-50';

  writeValue(value: number | null): void {
    this.value.set(value ?? null);
    this.syncText();
  }
  registerOnChange(fn: (value: number | null) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.cvaDisabled.set(isDisabled);
  }
}

function parseNumber(text: string): number | null {
  const cleaned = text.replace(/[^0-9.,-]/g, '').replace(/,/g, '');
  if (cleaned === '' || cleaned === '-' || cleaned === '.') return null;
  const n = Number(cleaned);
  return Number.isNaN(n) ? null : n;
}
