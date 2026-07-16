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

// Colour per source bucket. A diary event may override this with its own type colour; everything
// else is keyed here so lessons, cover, detentions etc. stay visually distinct and consistent.
const CATEGORY_COLOUR: Record<StaffCalendarCategory, string> = {
  Lesson: '#4f46e5', // indigo — the app's brand colour; lessons fill most of the grid
  Cover: '#7c3aed', // violet — a covered lesson reads as "not your usual"
  Detention: '#dc2626', // red
  Event: '#0891b2', // cyan
  Holiday: '#16a34a', // green
  NonContact: '#64748b', // slate
  ParentEvening: '#d97706', // amber
  Absence: '#9ca3af', // grey, rendered as a background band
};

const MOBILE_MAX = 639;

/**
 * Read-only week calendar for a staff member, built directly on the FullCalendar core API (no
 * Angular wrapper, so it's free of the wrapper's Angular-version peer constraints). Defaults to
 * the current week; FullCalendar refetches the feed on every navigation via the events callback.
 */
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
      :host ::ng-deep .fc {
        --fc-border-color: var(--p-surface-200);
        --fc-today-bg-color: color-mix(in srgb, var(--p-primary-color) 8%, transparent);
        --fc-now-indicator-color: var(--p-primary-color);
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
        --fc-button-bg-color: var(--p-surface-100);
        --fc-button-border-color: var(--p-surface-200);
        --fc-button-text-color: var(--p-text-color);
        --fc-button-hover-bg-color: var(--p-surface-200);
        --fc-button-hover-border-color: var(--p-surface-200);
        --fc-button-active-bg-color: var(--p-primary-color);
        --fc-button-active-border-color: var(--p-primary-color);
        border-radius: var(--p-button-border-radius);
      }
      :host ::ng-deep .fc .fc-button-primary:not(:disabled).fc-button-active,
      :host ::ng-deep .fc .fc-button-primary:not(:disabled):active {
        color: var(--p-primary-contrast-color, #fff);
      }
      :host ::ng-deep .fc .fc-button-primary:disabled {
        --fc-button-bg-color: var(--p-surface-100);
        --fc-button-border-color: var(--p-surface-200);
        --fc-button-text-color: var(--p-text-muted-color);
        opacity: 1;
      }
      :host ::ng-deep .fc .fc-button-primary:focus {
        box-shadow: none;
      }
      :host ::ng-deep .fc .fc-button-primary:focus-visible {
        outline: 2px solid var(--p-primary-color);
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

  // Profile mode: the staff member whose timetable to show. Ignored in self mode.
  readonly staffMemberId = input<string>();

  // Self mode: show the *current user's* own calendar via /me/timetable instead of a specific
  // member. Used on the home dashboard, where the viewer may not even be a staff member.
  readonly self = input<boolean>(false);

  @ViewChild('cal', { static: true }) private readonly calEl!: ElementRef<HTMLDivElement>;

  protected readonly errored = signal(false);
  private calendar?: Calendar;

  ngAfterViewInit(): void {
    const isMobile = window.innerWidth <= MOBILE_MAX;
    // FullCalendar churns the DOM outside Angular — keep it out of change detection.
    this.zone.runOutsideAngular(() => {
      this.calendar = new Calendar(this.calEl.nativeElement, {
        plugins: [timeGridPlugin, dayGridPlugin, listPlugin],
        // Follow the browser locale so date formats match the user's region
        // (e.g. en-GB renders D/M, not FullCalendar's default American M/D).
        locales: allLocales,
        locale: navigator.language,
        initialView: isMobile ? 'listWeek' : 'timeGridWeek',
        firstDay: 1, // Monday
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
        // Drop to the list view on small screens, back to the week grid when there's room.
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
      // Profile mode with no id yet — nothing to fetch.
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
    const colour = e.colourCode || CATEGORY_COLOUR[e.category] || CATEGORY_COLOUR.Event;
    return {
      id: e.id,
      title: e.location ? `${e.title} · ${e.location}` : e.title,
      start: e.start,
      end: e.end,
      allDay: e.allDay,
      backgroundColor: colour,
      borderColor: colour,
      // Absences sit behind the day as a context band rather than a solid block.
      display: e.category === 'Absence' ? 'background' : 'auto',
      extendedProps: { category: e.category },
    };
  }
}
