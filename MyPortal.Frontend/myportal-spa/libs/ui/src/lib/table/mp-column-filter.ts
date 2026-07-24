import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MpInput } from '../input/mp-input';
import { MpInputNumber } from '../input-number/mp-input-number';
import { MpDatePicker } from '../date-picker/mp-date-picker';
import { MpSelect } from '../select/mp-select';
import { MpMenu, type MpMenuItem } from '../menu/mp-menu';
import { MpTable } from './mp-table';

export type MpColumnFilterType = 'text' | 'number' | 'date';

// Date filters carry a local date-only string ('YYYY-MM-DD'), not a Date — a Date JSON-serialises via
// toISOString (UTC) and shifts across midnight, landing the filter on the wrong day.
function toDateOnly(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

function parseDateOnly(value: unknown): Date | null {
  if (value instanceof Date) return value;
  if (typeof value !== 'string' || !value) return null;
  const [y, m, d] = value.split('-').map(Number);
  return y && m && d ? new Date(y, m - 1, d) : null;
}

interface ModeDef {
  mode: string;
  label: string;
  icon: string;
}

// Match modes per data type. The `mode` keys map to QueryKit operators in shared/utils/querykit.ts.
const TEXT_MODES: ModeDef[] = [
  { mode: 'contains', label: 'Contains', icon: 'fa-magnifying-glass' },
  { mode: 'notContains', label: 'Does not contain', icon: 'fa-magnifying-glass-minus' },
  { mode: 'startsWith', label: 'Starts with', icon: 'fa-align-left' },
  { mode: 'endsWith', label: 'Ends with', icon: 'fa-align-right' },
  { mode: 'equals', label: 'Equals', icon: 'fa-equals' },
  { mode: 'notEquals', label: 'Does not equal', icon: 'fa-not-equal' },
];

// Numbers and dates share comparison operators.
const COMPARE_MODES: ModeDef[] = [
  { mode: 'equals', label: 'Equals', icon: 'fa-equals' },
  { mode: 'notEquals', label: 'Does not equal', icon: 'fa-not-equal' },
  { mode: 'lt', label: 'Less than', icon: 'fa-less-than' },
  { mode: 'gt', label: 'Greater than', icon: 'fa-greater-than' },
  { mode: 'lte', label: 'Less than or equal to', icon: 'fa-less-than-equal' },
  { mode: 'gte', label: 'Greater than or equal to', icon: 'fa-greater-than-equal' },
  { mode: 'between', label: 'Between', icon: 'fa-arrows-left-right-to-line' },
];

function modesFor(type: MpColumnFilterType): ModeDef[] {
  return type === 'text' ? TEXT_MODES : COMPARE_MODES;
}

/**
 * A per-column filter for a filter row under the header. A small icon (left) opens a menu of match
 * modes for the column's data type; the input adapts to `type` (text / number / date), and "Between"
 * shows two inputs. Controlled by the parent table's filter state.
 */
@Component({
  selector: 'mp-column-filter',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpInput, MpInputNumber, MpDatePicker, MpMenu],
  templateUrl: './mp-column-filter.html',
})
export class MpColumnFilter {
  private readonly table = inject(MpTable);

  readonly field = input.required<string>();
  readonly type = input<MpColumnFilterType>('text');
  readonly placeholder = input('');
  /** Optional starting match mode; otherwise the first mode for the type (Contains / Equals). */
  readonly matchMode = input<string | null>(null);

  private readonly chosenMode = signal<string | null>(null);

  // Between keeps its two bounds locally — the table only holds a complete [from, to] pair.
  protected readonly betweenFrom = signal<unknown>(null);
  protected readonly betweenTo = signal<unknown>(null);

  protected readonly modes = computed(() => modesFor(this.type()));
  protected readonly defaultMode = computed(() => this.matchMode() ?? this.modes()[0].mode);
  protected readonly activeMode = computed(() => this.chosenMode() ?? this.defaultMode());
  protected readonly isBetween = computed(() => this.activeMode() === 'between');

  private readonly rawValue = computed(() => this.table.filterValue(this.field()));
  protected readonly textValue = computed(() => String(this.rawValue() ?? ''));
  protected readonly numberValue = computed(() =>
    typeof this.rawValue() === 'number' ? (this.rawValue() as number) : null,
  );
  protected readonly dateValue = computed(() => parseDateOnly(this.rawValue()));

  protected readonly fromNumber = computed(() =>
    typeof this.betweenFrom() === 'number' ? (this.betweenFrom() as number) : null,
  );
  protected readonly toNumber = computed(() =>
    typeof this.betweenTo() === 'number' ? (this.betweenTo() as number) : null,
  );
  protected readonly fromDate = computed(() => parseDateOnly(this.betweenFrom()));
  protected readonly toDate = computed(() => parseDateOnly(this.betweenTo()));

  protected readonly triggerIcon = computed(() => {
    const def = this.modes().find(m => m.mode === this.activeMode());
    return 'fa-solid ' + (def?.icon ?? 'fa-filter');
  });

  protected readonly triggerLabel = computed(
    () => this.modes().find(m => m.mode === this.activeMode())?.label ?? 'Filter',
  );

  protected readonly modeItems = computed<MpMenuItem[]>(() => [
    ...this.modes().map(m => ({
      label: m.label,
      icon: 'fa-solid ' + m.icon + ' text-muted-foreground',
      command: () => this.setMode(m.mode),
    })),
    { separator: true },
    { label: 'Reset', icon: 'fa-solid fa-rotate-left text-muted-foreground', command: () => this.reset() },
  ]);

  // --- single-value inputs ---
  protected onText(event: Event): void {
    this.table.filter((event.target as HTMLInputElement).value, this.field(), this.activeMode());
  }

  protected onValue(value: unknown): void {
    this.table.filter(this.coerce(value), this.field(), this.activeMode());
  }

  // --- between inputs ---
  protected onFrom(value: unknown): void {
    this.betweenFrom.set(this.coerce(value));
    this.applyBetween();
  }

  protected onTo(value: unknown): void {
    this.betweenTo.set(this.coerce(value));
    this.applyBetween();
  }

  private applyBetween(): void {
    const from = this.betweenFrom();
    const to = this.betweenTo();
    const complete = from != null && from !== '' && to != null && to !== '';
    this.table.filter(complete ? [from, to] : null, this.field(), 'between');
  }

  private coerce(value: unknown): unknown {
    if (value == null) return null;
    return this.type() === 'date' && value instanceof Date ? toDateOnly(value) : value;
  }

  private setMode(mode: string): void {
    const wasBetween = this.isBetween();
    const enteringBetween = mode === 'between';
    const current = this.rawValue();
    this.chosenMode.set(mode);

    if (enteringBetween) {
      if (!wasBetween) {
        this.betweenFrom.set(current ?? null);
        this.betweenTo.set(null);
      }
      this.applyBetween();
      return;
    }

    // Collapse to a single value under the new mode.
    const single = wasBetween ? this.betweenFrom() : current;
    this.table.filter(single != null && single !== '' ? single : null, this.field(), mode);
  }

  private reset(): void {
    this.chosenMode.set(null);
    this.betweenFrom.set(null);
    this.betweenTo.set(null);
    this.table.filter(null, this.field(), this.defaultMode());
  }
}

/**
 * A select filter for one column (status, gender, …). Selecting <c>clearValue</c> removes the
 * filter. Controlled by the parent table's filter state.
 */
@Component({
  selector: 'mp-column-select-filter',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpSelect],
  template: `
    <mp-select
      class="w-full"
      [options]="options()"
      [optionLabel]="optionLabel()"
      [optionValue]="optionValue()"
      [ariaLabel]="ariaLabel()"
      [ngModel]="current()"
      (ngModelChange)="onChange($event)"></mp-select>
  `,
})
export class MpColumnSelectFilter {
  private readonly table = inject(MpTable);
  readonly field = input.required<string>();
  readonly options = input.required<readonly unknown[]>();
  readonly matchMode = input('equals');
  readonly optionLabel = input('label');
  readonly optionValue = input('value');
  readonly ariaLabel = input('');
  readonly clearValue = input<unknown>(null);

  protected readonly current = computed(() => this.table.filterValue(this.field()) ?? this.clearValue());

  protected onChange(value: unknown): void {
    this.table.filter(value === this.clearValue() ? null : value, this.field(), this.matchMode());
  }
}
