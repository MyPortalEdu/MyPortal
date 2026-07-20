import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import {
  BrnCalendar,
  BrnCalendarImports,
  injectBrnCalendarI18n,
  provideBrnCalendarI18n,
} from '@spartan-ng/brain/calendar';
import { injectDateAdapter, provideNativeDateAdapter } from '@spartan-ng/brain/date-time';

type CalendarView = 'days' | 'months' | 'years';

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

  readonly minYear = input<number>();
  readonly maxYear = input<number>();

  protected readonly view = signal<CalendarView>('days');

  protected readonly monthOptions = computed(() =>
    this.i18n.config().months().map((label, value) => ({ label, value, short: label.slice(0, 3) })),
  );

  protected readonly focusedMonth = computed(() => this.dateAdapter.getMonth(this.calendar.focusedDate()));
  protected readonly focusedYear = computed(() => this.dateAdapter.getYear(this.calendar.focusedDate()));
  protected readonly monthLabel = computed(() => this.monthOptions()[this.focusedMonth()]?.label ?? '');

  protected readonly yearOptions = computed(() => {
    const current = this.focusedYear();
    const thisYear = new Date().getFullYear();
    const max = Math.max(this.maxYear() ?? thisYear + 5, current);
    const min = Math.min(this.minYear() ?? thisYear - 100, current);
    const years: number[] = [];
    for (let y = max; y >= min; y--) years.push(y);
    return years;
  });

  protected toggleView(target: CalendarView): void {
    this.view.set(this.view() === target ? 'days' : target);
  }

  protected pickMonth(month: number): void {
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

  protected gridBtnClass(selected: boolean): string {
    return (
      'inline-flex h-9 items-center justify-center rounded-control text-sm outline-none transition-colors ' +
      (selected
        ? 'bg-primary text-primary-foreground font-semibold'
        : 'hover:bg-accent hover:text-accent-foreground')
    );
  }

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
