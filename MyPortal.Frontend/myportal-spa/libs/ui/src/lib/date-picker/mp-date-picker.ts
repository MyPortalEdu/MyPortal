import { ChangeDetectionStrategy, Component, computed, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { BrnPopoverImports } from '@spartan-ng/brain/popover';
import { MpInput } from '../input/mp-input';
import { MpPopover } from '../popover/mp-popover';
import { MpCalendar } from './mp-calendar';

/**
 * Data-entry date picker — the design-system equivalent of `p-datePicker`. A typeable text input
 * shows/accepts the date; a trailing calendar button opens MpCalendar in a CDK overlay via
 * brn-popover (the same overlay pattern MpSelect uses).
 *
 * Values are native `Date | null`, matching the app's existing p-datePicker contract, so
 * `[ngModel]`/`(ngModelChange)` migrations are mechanical. Display/parse format follows
 * p-datePicker's jQuery-UI tokens (`dd`, `mm`, `yy` = 4-digit year); only `dd/mm/yy` is used today.
 *
 * Typing: edit the input freely, then blur or press Enter to commit (parsed against `dateFormat`;
 * unparseable text reverts to the last valid value). The calendar header has month + year dropdowns
 * for fast decade jumps (birthdays).
 *
 * Not yet supported (kept on p-datePicker for now): `showTime` / `timeOnly` (time entry).
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
  // p-datePicker's `dateFormat` (jQuery-UI tokens). Default matches the app-wide `dd/mm/yy`.
  readonly dateFormat = input('dd/mm/yy');
  // Kept for API compatibility with p-datePicker; the calendar opener button is always shown.
  readonly showIcon = input(true);
  readonly minDate = input<Date | undefined>(undefined);
  readonly maxDate = input<Date | undefined>(undefined);

  protected readonly value = signal<Date | null>(null);
  // The input's live text — user-editable, so it's a separate signal we resync on value changes.
  protected readonly text = signal('');
  private readonly cvaDisabled = signal(false);
  protected readonly disabled = computed(() => this.disabledInput() || this.cvaDisabled());

  protected onChange: (value: Date | null) => void = () => {};
  protected onTouched: () => void = () => {};

  private setValue(next: Date | null): void {
    this.value.set(next);
    this.text.set(next ? formatDate(next, this.dateFormat()) : '');
  }

  protected onDatePicked(date: Date | undefined, popover: { toggle: () => void }): void {
    const next = date ?? null;
    this.setValue(next);
    this.onChange(next);
    this.onTouched();
    // Auto-close after a selection (brn-popover has no close(); toggle() closes while it's open).
    popover.toggle();
  }

  protected commitTyped(): void {
    this.onTouched();
    const parsed = parseDate(this.text(), this.dateFormat());
    // Unparseable, non-empty text reverts to the last valid value; empty clears.
    if (parsed === undefined) {
      this.text.set(this.value() ? formatDate(this.value() as Date, this.dateFormat()) : '');
      return;
    }
    if (sameDay(parsed, this.value())) {
      this.text.set(parsed ? formatDate(parsed, this.dateFormat()) : '');
      return;
    }
    this.setValue(parsed);
    this.onChange(parsed);
  }

  protected readonly contentClass =
    'z-50 w-auto rounded-surface border border-border bg-background p-2 shadow-overlay';

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

const sameDay = (a: Date | null, b: Date | null): boolean =>
  a !== null &&
  b !== null &&
  a.getFullYear() === b.getFullYear() &&
  a.getMonth() === b.getMonth() &&
  a.getDate() === b.getDate();

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

  // Order of the day/month/year fields as they appear in the format string.
  const order: string[] = [];
  format.replace(/y+|m+|d+/g, (token) => (order.push(token[0]), token));
  if (order.length < 3) return undefined;

  const fields: Record<string, number> = {};
  order.forEach((key, i) => (fields[key] = Number(parts[i])));

  let year = fields['y'];
  const month = fields['m'];
  const day = fields['d'];
  if ([year, month, day].some((n) => Number.isNaN(n))) return undefined;
  // 2-digit years: >30 → 19xx, else 20xx (matches typical DOB entry).
  if (year < 100) year += year > 30 ? 1900 : 2000;

  const date = new Date(year, month - 1, day);
  // Reject overflow (e.g. 31/02) — the constructed date's parts must match what was typed.
  if (date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day) {
    return undefined;
  }
  return date;
}
