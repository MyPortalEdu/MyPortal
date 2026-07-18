import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import {
  BrnCalendar,
  BrnCalendarImports,
  injectBrnCalendarI18n,
  provideBrnCalendarI18n,
} from '@spartan-ng/brain/calendar';
import { injectDateAdapter, provideNativeDateAdapter } from '@spartan-ng/brain/date-time';

type CalendarView = 'days' | 'months' | 'years';

/**
 * Inline month calendar — the grid behind MpDatePicker, usable on its own for always-visible
 * pickers. Wraps Spartan's headless `brnCalendar` (keyboard nav, focus management, min/max and
 * per-date disabling all come from the brain layer) and styles the cells with design tokens.
 *
 * The header's month/year are buttons that swap the body to an in-panel month grid or a scrollable,
 * height-capped year grid (the Material/p-calendar pattern) — self-contained, so no nested CDK
 * overlay to fight the date picker's popover, and no runaway-height native `<select>` dropdown.
 * The year range defaults to a DOB-friendly ~100 years back; override with `minYear`/`maxYear`.
 * (For a far-back date, typing `dd/mm/yyyy` straight into MpDatePicker's input is the fast path.)
 *
 * Values are native `Date`s (via `provideNativeDateAdapter`), matching the app's existing
 * p-datePicker contract. Two-way bind the selection with `[date]` / `(dateChange)`.
 */
@Component({
  selector: 'mp-calendar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [BrnCalendarImports],
  providers: [provideNativeDateAdapter(), provideBrnCalendarI18n()],
  hostDirectives: [
    {
      directive: BrnCalendar,
      inputs: ['min', 'max', 'disabled', 'date', 'dateDisabled', 'weekStartsOn', 'defaultFocusedDate'],
      outputs: ['dateChange'],
    },
  ],
  host: { class: 'block' },
  templateUrl: './mp-calendar.html',
})
export class MpCalendar {
  protected readonly i18n = injectBrnCalendarI18n();
  protected readonly dateAdapter = injectDateAdapter<Date>();
  private readonly calendar = inject(BrnCalendar);

  /** Bounds for the year grid. Default: ~100 years back to +5, always including the focused year. */
  readonly minYear = input<number>();
  readonly maxYear = input<number>();

  protected readonly view = signal<CalendarView>('days');

  protected readonly monthOptions = computed(() =>
    this.i18n.config().months().map((label, value) => ({ label, value, short: label.slice(0, 3) })),
  );

  // brn's focusedDate (a linkedSignal off `date`) tracks the visible month; drive the header off it.
  protected readonly focusedMonth = computed(() => this.dateAdapter.getMonth(this.calendar.focusedDate()));
  protected readonly focusedYear = computed(() => this.dateAdapter.getYear(this.calendar.focusedDate()));
  protected readonly monthLabel = computed(() => this.monthOptions()[this.focusedMonth()]?.label ?? '');

  protected readonly yearOptions = computed(() => {
    const current = this.focusedYear();
    const thisYear = new Date().getFullYear();
    const max = Math.max(this.maxYear() ?? thisYear + 5, current);
    const min = Math.min(this.minYear() ?? thisYear - 100, current);
    const years: number[] = [];
    for (let y = max; y >= min; y--) years.push(y); // newest first — current year near the top
    return years;
  });

  protected toggleView(target: CalendarView): void {
    this.view.set(this.view() === target ? 'days' : target);
  }

  protected pickMonth(month: number): void {
    // day:1 avoids month-length overflow (e.g. 31 Jan → Feb) shifting the visible month.
    this.calendar.focusedDate.set(this.dateAdapter.set(this.calendar.focusedDate(), { month, day: 1 }));
    this.view.set('days');
  }

  protected pickYear(year: number): void {
    this.calendar.focusedDate.set(this.dateAdapter.set(this.calendar.focusedDate(), { year, day: 1 }));
    this.view.set('months');
  }

  protected readonly navBtnClass =
    'inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-control text-muted-foreground ' +
    'outline-none transition-colors hover:bg-accent hover:text-accent-foreground ' +
    'aria-disabled:pointer-events-none aria-disabled:opacity-40';

  protected readonly headerBtnClass =
    'inline-flex h-7 items-center rounded-control px-2 text-sm font-medium outline-none transition-colors ' +
    'hover:bg-accent hover:text-accent-foreground focus:ring-2 focus:ring-ring/40';

  // Month/year grid cell — highlighted when it's the focused month/year.
  protected gridBtnClass(selected: boolean): string {
    return (
      'inline-flex h-9 items-center justify-center rounded-control text-sm outline-none transition-colors ' +
      (selected
        ? 'bg-primary text-primary-foreground font-semibold'
        : 'hover:bg-accent hover:text-accent-foreground')
    );
  }

  // brn sets data-[selected-single|today|outside|disabled|focused]="true" on the cell button
  // (single-date selection is data-selected-single, NOT data-selected — that's for range mode).
  protected readonly cellBtnClass =
    'mx-auto inline-flex h-8 w-8 items-center justify-center rounded-control text-sm outline-none ' +
    'transition-colors hover:bg-accent hover:text-accent-foreground ' +
    'data-[today=true]:bg-muted data-[today=true]:font-semibold ' +
    'data-[selected-single=true]:bg-primary data-[selected-single=true]:text-primary-foreground ' +
    'data-[selected-single=true]:font-semibold data-[selected-single=true]:hover:bg-primary ' +
    'data-[outside=true]:opacity-40 ' +
    'data-[disabled=true]:pointer-events-none data-[disabled=true]:opacity-30 ' +
    'data-[focused=true]:ring-2 data-[focused=true]:ring-ring/40';
}
