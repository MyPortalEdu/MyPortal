import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  NgZone,
  OnDestroy,
  ViewChild,
  inject,
  input,
  signal,
} from '@angular/core';
import { Calendar, EventInput, EventSourceFuncArg } from '@fullcalendar/core';
import allLocales from '@fullcalendar/core/locales-all';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import listPlugin from '@fullcalendar/list';
import { TranslocoService } from '@jsverse/transloco';

import { StaffMembersDataService } from '../../services/staff-members-data.service';
import {
  StaffCalendarCategory,
  StaffCalendarEvent,
} from '../../types/staff-timetable';

const CATEGORY_COLOUR: Record<StaffCalendarCategory, string> = {
  Lesson: '#4f46e5',
  Cover: '#7c3aed',
  Detention: '#dc2626',
  Event: '#0891b2',
  Holiday: '#16a34a',
  NonContact: '#64748b',
  ParentEvening: '#d97706',
  Absence: '#9ca3af',
};

// Per-kind colours for diary-sourced events (keyed by DiaryEventKind). Colour is a UI concern, so it
// lives here rather than in SQL. Kinds without an entry fall back to the category colour.
const KIND_COLOUR: Record<number, string> = {
  1: '#06b6d4', // ECActivity
  3: '#7c3aed', // Cover
  4: '#dc2626', // Detention
  5: '#14b8a6', // NCC
  6: '#16a34a', // PPA
  7: '#9333ea', // SchoolHoliday
  8: '#a21caf', // PublicHoliday
  9: '#ea580c', // TeacherTraining (INSET)
  10: '#d97706', // ParentEvening
  11: '#0ea5e9', // TrainingEvent (staff training)
};

const MOBILE_MAX = 639;

@Component({
  selector: 'mp-staff-timetable',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mp-staff-timetable">
      @if (errored()) {
        <div class="text-sm text-red-600 dark:text-red-400 mb-2">
          {{ transloco.translate('common.timetable.loadError') }}
        </div>
      }
      <div #cal></div>
    </div>
  `,
  styles: [
    `
      /* FullCalendar ships hard-coded light defaults (#fff page, #ddd borders, #f5f5f5 list hover,
         hsl-grey neutrals), so anything left unmapped stays light in dark mode — most visibly the
         sticky header, the more-link popover and the list view, which all paint --fc-page-bg-color.
         Map every one to a PrimeNG *semantic* token: those flip with the theme, whereas the numbered
         --p-surface-N ramp does NOT invert (Aura defines surface.50 as a light slate/zinc in both
         colour schemes), so it must never be used here. */
      :host ::ng-deep .fc {
        --fc-page-bg-color: var(--background);
        --fc-border-color: var(--border);
        --fc-neutral-bg-color: var(--muted);
        --fc-neutral-text-color: var(--muted-foreground);
        --fc-today-bg-color: color-mix(in srgb, var(--primary) 8%, transparent);
        --fc-now-indicator-color: var(--primary);
        --fc-highlight-color: color-mix(in srgb, var(--primary) 12%, transparent);
        --fc-non-business-color: color-mix(in srgb, var(--muted-foreground) 8%, transparent);
        --fc-list-event-hover-bg-color: var(--muted);
        --fc-more-link-bg-color: var(--muted);
        --fc-more-link-text-color: var(--foreground);
        --fc-event-bg-color: var(--primary);
        --fc-event-border-color: var(--primary);
        --fc-event-text-color: var(--primary-foreground, #fff);
        --fc-event-selected-overlay-color: color-mix(in srgb, var(--foreground) 20%, transparent);
        font-size: 0.8125rem;
      }
      :host ::ng-deep .fc .fc-toolbar.fc-header-toolbar {
        margin-bottom: 0.75rem;
        gap: 0.5rem;
        flex-wrap: wrap;
      }
      :host ::ng-deep .fc .fc-toolbar-title {
        font-size: 1rem;
        font-weight: 600;
      }
      :host ::ng-deep .fc .fc-button {
        padding: 0.3rem 0.7rem;
        font-size: 0.8125rem;
        font-weight: 500;
        text-transform: capitalize;
        box-shadow: none;
      }
      /* Bind FullCalendar's button skin to the app's theme: subtle by default,
         indigo for the active view and the pressed state. */
      :host ::ng-deep .fc .fc-button-primary {
        --fc-button-bg-color: var(--background);
        --fc-button-border-color: var(--border);
        --fc-button-text-color: var(--foreground);
        --fc-button-hover-bg-color: var(--muted);
        --fc-button-hover-border-color: var(--border);
        --fc-button-active-bg-color: var(--primary);
        --fc-button-active-border-color: var(--primary);
        border-radius: var(--radius-control);
      }
      :host ::ng-deep .fc .fc-button-primary:not(:disabled).fc-button-active,
      :host ::ng-deep .fc .fc-button-primary:not(:disabled):active {
        color: var(--primary-foreground, #fff);
      }
      :host ::ng-deep .fc .fc-button-primary:disabled {
        --fc-button-bg-color: var(--background);
        --fc-button-border-color: var(--border);
        --fc-button-text-color: var(--muted-foreground);
        opacity: 1;
      }
      :host ::ng-deep .fc .fc-button-primary:focus {
        box-shadow: none;
      }
      :host ::ng-deep .fc .fc-button-primary:focus-visible {
        outline: 2px solid var(--primary);
        outline-offset: 1px;
      }
      :host ::ng-deep .fc-event {
        cursor: default;
        font-size: 0.75rem;
      }
    `,
  ],
})
export class StaffTimetable implements AfterViewInit, OnDestroy {
  protected readonly transloco = inject(TranslocoService);
  private readonly data = inject(StaffMembersDataService);
  private readonly zone = inject(NgZone);

  readonly staffMemberId = input<string>();

  readonly self = input<boolean>(false);

  @ViewChild('cal', { static: true }) private readonly calEl!: ElementRef<HTMLDivElement>;

  protected readonly errored = signal(false);
  private calendar?: Calendar;

  ngAfterViewInit(): void {
    const isMobile = window.innerWidth <= MOBILE_MAX;
    this.zone.runOutsideAngular(() => {
      this.calendar = new Calendar(this.calEl.nativeElement, {
        plugins: [timeGridPlugin, dayGridPlugin, listPlugin],
        locales: allLocales,
        locale: navigator.language,
        initialView: isMobile ? 'listWeek' : 'timeGridWeek',
        firstDay: 1,
        nowIndicator: true,
        weekends: true,
        allDaySlot: true,
        slotMinTime: '07:00:00',
        slotMaxTime: '19:00:00',
        expandRows: true,
        height: 'auto',
        stickyHeaderDates: true,
        headerToolbar: {
          left: 'prev,next today',
          center: 'title',
          right: 'timeGridWeek,timeGridDay,listWeek',
        },
        buttonText: {
          today: this.transloco.translate('common.timetable.today'),
          week: this.transloco.translate('common.timetable.week'),
          day: this.transloco.translate('common.timetable.day'),
          list: this.transloco.translate('common.timetable.list'),
        },
        noEventsContent: () => this.transloco.translate('common.timetable.noEvents'),
        events: (arg, success, failure) => this.loadEvents(arg, success, failure),
        windowResize: () => {
          const next = window.innerWidth <= MOBILE_MAX ? 'listWeek' : 'timeGridWeek';
          if (this.calendar && this.calendar.view.type !== next) {
            this.calendar.changeView(next);
          }
        },
      });
      this.calendar.render();
    });
  }

  ngOnDestroy(): void {
    this.calendar?.destroy();
  }

  private loadEvents(
    arg: EventSourceFuncArg,
    success: (events: EventInput[]) => void,
    failure: (error: Error) => void,
  ): void {
    const from = arg.start.toISOString();
    const to = arg.end.toISOString();
    const id = this.staffMemberId();

    if (!this.self() && !id) {
      success([]);
      return;
    }

    const feed$ = this.self()
      ? this.data.getMyTimetable(from, to)
      : this.data.getTimetable(id!, from, to);

    feed$.subscribe({
      next: res => {
        success(res.events.map(e => this.toInput(e)));
        this.zone.run(() => this.errored.set(false));
      },
      error: () => {
        this.zone.run(() => this.errored.set(true));
        failure(new Error('Failed to load timetable'));
      },
    });
  }

  private toInput(e: StaffCalendarEvent): EventInput {
    const colour =
      (e.kind != null ? KIND_COLOUR[e.kind] : null) ||
      e.colourCode ||
      CATEGORY_COLOUR[e.category] ||
      CATEGORY_COLOUR.Event;
    return {
      id: e.id,
      title: e.location ? `${e.title} · ${e.location}` : e.title,
      start: e.start,
      end: e.end,
      allDay: e.allDay,
      backgroundColor: colour,
      borderColor: colour,
      display: e.category === 'Absence' ? 'background' : 'auto',
      extendedProps: { category: e.category },
    };
  }
}
