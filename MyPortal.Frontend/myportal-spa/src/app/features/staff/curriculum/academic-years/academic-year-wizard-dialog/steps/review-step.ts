import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';

import { AcademicYearsDataService } from '../../../../../../shared/services/academic-years-data.service';
import { AcademicYearSummary } from '../../../../../../core/types/academic-year-summary';
import {
  AcademicYearUpsertRequest,
  AttendancePeriodUpsertRequest,
  SchoolHolidayType,
} from '../../../../../../shared/types/academic-year';

const DAY_NAMES = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

interface DaySummary {
  day: string;
  count: number;
  firstStart: Date | null;
  lastEnd: Date | null;
  amCount: number;
  pmCount: number;
  lessonCount: number;
}

@Component({
  selector: 'mp-academic-year-wizard-review-step',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, TranslocoDirective],
  templateUrl: './review-step.html',
})
export class AcademicYearWizardReviewStep implements OnInit {
  private readonly data = inject(AcademicYearsDataService);
  private readonly transloco = inject(TranslocoService);

  readonly model = input.required<AcademicYearUpsertRequest>();

  // Used to resolve the "copy from" id back to a human name for the review.
  // Same call the other steps make; not cached cross-step today.
  readonly priorYears = signal<AcademicYearSummary[]>([]);

  ngOnInit(): void {
    this.data.list().subscribe({
      next: rows => this.priorYears.set(rows ?? []),
      error: () => this.priorYears.set([]),
    });
  }

  readonly yearName = computed(() => {
    const years = this.model().academicTerms
      .map(t => t.startDate?.getFullYear())
      .filter((y): y is number => y !== undefined);
    if (years.length === 0) return null;
    return `${Math.min(...years)}/${Math.max(...years)}`;
  });

  readonly cycleSummary = computed(() => {
    const m = this.model();
    const swl = Math.max(1, m.schoolWeekLength);
    const weeks = Math.max(1, m.timetableCycleLength / swl);
    const weekIdx = Math.floor(m.firstWeekOffset / swl);
    return {
      daysPerWeek: swl,
      cycleWeeks: weeks,
      firstWeekLetter: String.fromCharCode(65 + weekIdx),
    };
  });

  readonly isCopyingPeriods = computed(() =>
    this.model().copyPeriodsFromAcademicYearId != null,
  );

  readonly copyPeriodsSourceName = computed(() => {
    const id = this.model().copyPeriodsFromAcademicYearId;
    if (id == null) return null;
    return this.priorYears().find(y => y.id === id)?.name ?? id;
  });

  readonly copyPastoralSourceName = computed(() => {
    const id = this.model().copyPastoralStructureFromAcademicYearId;
    if (id == null) return null;
    return this.priorYears().find(y => y.id === id)?.name ?? id;
  });

  // Per-day period rollup. Sorted by cycle-day index and again by start time
  // within each day so first/last reflect actual chronological bounds rather
  // than insertion order.
  readonly periodDaySummaries = computed<DaySummary[]>(() => {
    const m = this.model();
    if (m.copyPeriodsFromAcademicYearId != null) return [];
    const byDay = new Map<number, AttendancePeriodUpsertRequest[]>();
    for (const p of m.attendancePeriods) {
      const list = byDay.get(p.cycleDayIndex) ?? [];
      list.push(p);
      byDay.set(p.cycleDayIndex, list);
    }
    return Array.from(byDay.entries())
      .sort(([a], [b]) => a - b)
      .map(([d, periods]) => {
        const sorted = [...periods].sort(
          (a, b) => (a.startTime?.getTime() ?? 0) - (b.startTime?.getTime() ?? 0),
        );
        return {
          day: this.dayLabel(d),
          count: periods.length,
          firstStart: sorted[0]?.startTime ?? null,
          lastEnd: sorted[sorted.length - 1]?.endTime ?? null,
          amCount: periods.filter(p => p.isAmReg).length,
          pmCount: periods.filter(p => p.isPmReg).length,
          lessonCount: periods.filter(p => p.isLesson).length,
        };
      });
  });

  holidayTypeLabel(type: SchoolHolidayType): string {
    switch (type) {
      case SchoolHolidayType.HalfTerm:
        return this.transloco.translate('academic-years.wizard.holidays.typeHalfTerm');
      case SchoolHolidayType.TeacherTraining:
        return this.transloco.translate('academic-years.wizard.holidays.typeTeacherTraining');
      case SchoolHolidayType.PublicHoliday:
        return this.transloco.translate('academic-years.wizard.holidays.typePublicHoliday');
    }
  }

  // Day-of-cycle label — same scheme as the periods step (short day name with
  // optional Week A/B/… suffix for multi-week cycles).
  private dayLabel(cycleDayIndex: number): string {
    const m = this.model();
    const swl = Math.max(1, m.schoolWeekLength);
    const weeks = Math.max(1, m.timetableCycleLength / swl);
    const dayInWeek = cycleDayIndex % swl;
    const weekIdx = Math.floor(cycleDayIndex / swl);
    const dayShort = (DAY_NAMES[dayInWeek] ?? `Day ${dayInWeek + 1}`).slice(0, 3);
    return weeks > 1
      ? `${dayShort} (${String.fromCharCode(65 + weekIdx)})`
      : dayShort;
  }
}
