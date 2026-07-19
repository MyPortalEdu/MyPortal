import { ChangeDetectionStrategy, Component, computed, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { BrnPopoverImports } from '@spartan-ng/brain/popover';
import { MpInput } from '../input/mp-input';
import { MpPopover } from '../popover/mp-popover';
import { MpCalendar } from './mp-calendar';

/**
 * Data-entry date picker — the design-system equivalent of `p-datePicker`. A typeable input shows/
 * accepts the value; a trailing button opens MpCalendar (and/or a time picker) in a CDK overlay via
 * MpPopover.
 *
 * Modes: date-only (default), `showTime` (calendar + 24h time selects, value = date & time), and
 * `timeOnly` (just the time selects, no calendar). Values are native `Date | null`, matching
 * p-datePicker. Display/parse uses jQuery-UI date tokens (`dd`/`mm`/`yy`=4-digit year) plus `HH:mm`
 * for the time part.
 */
@Component({
  selector: 'mp-date-picker',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [BrnPopoverImports, MpPopover, MpInput, MpCalendar],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MpDatePicker), multi: true }],
  host: { class: 'block' },
  templateUrl: './mp-date-picker.html',
})
export class MpDatePicker implements ControlValueAccessor {
  readonly placeholder = input<string | undefined>(undefined);
  readonly disabledInput = input(false, { alias: 'disabled' });
  readonly invalid = input<boolean | null | undefined>(false);
  readonly inputId = input<string | undefined>(undefined);
  readonly dateFormat = input('dd/mm/yy');
  readonly showIcon = input(true);
  readonly showTime = input(false);
  readonly timeOnly = input(false);
  readonly minDate = input<Date | undefined>(undefined);
  readonly maxDate = input<Date | undefined>(undefined);

  protected readonly value = signal<Date | null>(null);
  protected readonly text = signal('');
  private readonly cvaDisabled = signal(false);
  protected readonly disabled = computed(() => this.disabledInput() || this.cvaDisabled());

  protected readonly hours = Array.from({ length: 24 }, (_, i) => i);
  protected readonly minutes = Array.from({ length: 60 }, (_, i) => i);
  protected readonly selectedHour = computed(() => this.value()?.getHours() ?? 0);
  protected readonly selectedMinute = computed(() => this.value()?.getMinutes() ?? 0);

  protected onChange: (value: Date | null) => void = () => {};
  protected onTouched: () => void = () => {};

  private setValue(next: Date | null): void {
    this.value.set(next);
    this.text.set(next ? this.formatValue(next) : '');
  }

  private emit(next: Date | null): void {
    this.setValue(next);
    this.onChange(next);
    this.onTouched();
  }

  protected onDatePicked(date: Date | undefined, popover: { toggle: () => void }): void {
    let next = date ?? null;
    // In showTime mode, keep the currently-selected time when a new day is picked.
    if (next && this.showTime() && this.value()) {
      const prev = this.value() as Date;
      next = new Date(next);
      next.setHours(prev.getHours(), prev.getMinutes(), 0, 0);
    }
    this.emit(next);
    // Date-only auto-closes; showTime stays open so the user can still set the time.
    if (!this.showTime()) popover.toggle();
  }

  protected onHourChange(event: Event): void {
    this.applyTime(Number((event.target as HTMLSelectElement).value), this.selectedMinute());
  }
  protected onMinuteChange(event: Event): void {
    this.applyTime(this.selectedHour(), Number((event.target as HTMLSelectElement).value));
  }
  private applyTime(hours: number, minutes: number): void {
    const base = this.value() ? new Date(this.value() as Date) : new Date();
    base.setHours(hours, minutes, 0, 0);
    this.emit(base);
  }

  protected commitTyped(): void {
    this.onTouched();
    const parsed = this.parseValue(this.text());
    if (parsed === undefined) {
      this.text.set(this.value() ? this.formatValue(this.value() as Date) : '');
      return;
    }
    if (sameInstant(parsed, this.value())) {
      this.text.set(parsed ? this.formatValue(parsed) : '');
      return;
    }
    this.emit(parsed);
  }

  // --- formatting / parsing (mode-aware) ---
  private formatValue(date: Date): string {
    if (this.timeOnly()) return formatTime(date);
    if (this.showTime()) return `${formatDate(date, this.dateFormat())} ${formatTime(date)}`;
    return formatDate(date, this.dateFormat());
  }

  private parseValue(text: string): Date | null | undefined {
    const trimmed = text.trim();
    if (!trimmed) return null;
    if (this.timeOnly()) {
      const t = parseTime(trimmed);
      if (!t) return undefined;
      const base = this.value() ? new Date(this.value() as Date) : new Date();
      base.setHours(t.h, t.m, 0, 0);
      return base;
    }
    if (this.showTime()) {
      const m = trimmed.match(/(\d{1,2}):(\d{2})\s*$/);
      const time = m ? { h: Number(m[1]), m: Number(m[2]) } : { h: 0, m: 0 };
      if (m && (time.h > 23 || time.m > 59)) return undefined;
      const datePart = m ? trimmed.slice(0, m.index).trim() : trimmed;
      const date = parseDate(datePart, this.dateFormat());
      if (date == null) return date;
      date.setHours(time.h, time.m, 0, 0);
      return date;
    }
    return parseDate(trimmed, this.dateFormat());
  }

  protected readonly contentClass =
    'z-50 w-auto rounded-surface border border-border bg-background p-2 shadow-overlay';
  protected readonly timeSelectClass =
    'h-8 rounded-control border border-input bg-background px-1.5 text-sm outline-none cursor-pointer ' +
    'transition-colors hover:border-[var(--p-form-field-hover-border-color)] focus:border-ring focus:ring-2 focus:ring-ring/40 ' +
    'disabled:cursor-not-allowed disabled:opacity-50';

  protected pad(n: number): string {
    return String(n).padStart(2, '0');
  }

  writeValue(value: Date | string | null): void {
    this.setValue(value ? (value instanceof Date ? value : new Date(value)) : null);
  }
  registerOnChange(fn: (value: Date | null) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.cvaDisabled.set(isDisabled);
  }
}

const pad2 = (n: number): string => String(n).padStart(2, '0');

const formatTime = (date: Date): string => `${pad2(date.getHours())}:${pad2(date.getMinutes())}`;

/** Compare to the minute (time-picker granularity), so no-op edits don't churn the model. */
const sameInstant = (a: Date | null, b: Date | null): boolean =>
  a !== null &&
  b !== null &&
  a.getFullYear() === b.getFullYear() &&
  a.getMonth() === b.getMonth() &&
  a.getDate() === b.getDate() &&
  a.getHours() === b.getHours() &&
  a.getMinutes() === b.getMinutes();

/** `HH:mm` → {h, m}, validated; null if unparseable. */
function parseTime(text: string): { h: number; m: number } | null {
  const match = text.trim().match(/^(\d{1,2}):(\d{2})$/);
  if (!match) return null;
  const h = Number(match[1]);
  const m = Number(match[2]);
  return h <= 23 && m <= 59 ? { h, m } : null;
}

/**
 * Format a Date with p-datePicker's jQuery-UI tokens. `yy` is a 4-digit year (PrimeNG semantics),
 * `y` is 2-digit; `dd`/`mm` are zero-padded, `d`/`m` are not. Longest tokens are replaced first.
 */
function formatDate(date: Date, format: string): string {
  const day = date.getDate();
  const month = date.getMonth() + 1;
  const year = date.getFullYear();
  return format
    .replace(/yyyy/g, String(year))
    .replace(/yy/g, String(year))
    .replace(/y/g, String(year).slice(-2))
    .replace(/dd/g, pad2(day))
    .replace(/d/g, String(day))
    .replace(/mm/g, pad2(month))
    .replace(/m/g, String(month));
}

/**
 * Parse typed text against a format's token order (which of d/m/y comes first). Returns a Date on
 * success, `null` for empty input, or `undefined` when the text can't be parsed to a valid date.
 */
function parseDate(text: string, format: string): Date | null | undefined {
  const trimmed = text.trim();
  if (!trimmed) return null;

  const parts = trimmed.split(/\D+/).filter(Boolean);
  if (parts.length < 3) return undefined;

  const order: string[] = [];
  format.replace(/y+|m+|d+/g, (token) => (order.push(token[0]), token));
  if (order.length < 3) return undefined;

  const fields: Record<string, number> = {};
  order.forEach((key, i) => (fields[key] = Number(parts[i])));

  let year = fields['y'];
  const month = fields['m'];
  const day = fields['d'];
  if ([year, month, day].some((n) => Number.isNaN(n))) return undefined;
  if (year < 100) year += year > 30 ? 1900 : 2000;

  const date = new Date(year, month - 1, day);
  if (date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day) {
    return undefined;
  }
  return date;
}
