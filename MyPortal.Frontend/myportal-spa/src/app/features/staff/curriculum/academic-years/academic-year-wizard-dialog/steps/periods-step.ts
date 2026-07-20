import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MpSelect, MpInput, MpButton, MpCheckbox, MpSelectButton, MpDatePicker } from '@myportal/ui';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';

import { AcademicYearsDataService } from '../../../../../../shared/services/academic-years-data.service';
import { ConfirmationDialog } from '../../../../../../core/services/confirmation.service';
import { AcademicYearSummary } from '../../../../../../core/types/academic-year-summary';
import {
  AcademicYearUpsertRequest,
  AttendancePeriodUpsertRequest,
} from '../../../../../../shared/types/academic-year';

const DAY_NAMES = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

type Mode = 'define' | 'copy';

interface LabelledOption<T> {
  label: string;
  value: T;
}

@Component({
  selector: 'mp-academic-year-wizard-periods-step',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    MpInput,
    MpSelect,
    MpSelectButton,
    MpButton,
    MpCheckbox,
    MpDatePicker,
    TranslocoDirective,
  ],
  templateUrl: './periods-step.html',
})
export class AcademicYearWizardPeriodsStep implements OnInit {
  private readonly data = inject(AcademicYearsDataService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly model = input.required<AcademicYearUpsertRequest>();
  readonly editMode = input<boolean>(false);
  readonly readOnly = input<boolean>(false);
  readonly modelChange = output<Partial<AcademicYearUpsertRequest>>();

  readonly priorYears = signal<AcademicYearSummary[]>([]);

  readonly selectedCycleDay = signal(0);

  readonly mode = signal<Mode>('define');

  readonly modeOptions: LabelledOption<Mode>[] = [
    { label: 'Define new periods', value: 'define' },
    { label: 'Copy from existing year', value: 'copy' },
  ];

  readonly cycleDayOptions = computed<LabelledOption<number>[]>(() => {
    const m = this.model();
    const swl = Math.max(1, m.schoolWeekLength);
    const cycle = m.timetableCycleLength;
    const weeks = Math.max(1, cycle / swl);
    return Array.from({ length: cycle }, (_, i) => {
      const dayInWeek = i % swl;
      const weekIdx = Math.floor(i / swl);
      const dayShort = (DAY_NAMES[dayInWeek] ?? `Day ${dayInWeek + 1}`).slice(0, 3);
      const label = weeks > 1
        ? `${dayShort} (${String.fromCharCode(65 + weekIdx)})`
        : dayShort;
      return { label, value: i };
    });
  });

  readonly displayedDay = computed(() => {
    const cycle = this.model().timetableCycleLength;
    return Math.min(this.selectedCycleDay(), Math.max(0, cycle - 1));
  });

  readonly displayedDayLabel = computed(() =>
    this.cycleDayOptions().find(o => o.value === this.displayedDay())?.label ?? '',
  );

  readonly periodsForDay = computed(() =>
    this.model().attendancePeriods
      .map((period, fullIndex) => ({ period, fullIndex }))
      .filter(x => x.period.cycleDayIndex === this.displayedDay()),
  );

  readonly priorYearOptions = computed<LabelledOption<string>[]>(() =>
    this.priorYears().map(y => ({ label: y.name, value: y.id })),
  );

  readonly canCopyToAllDays = computed(() =>
    this.periodsForDay().length > 0 && this.model().timetableCycleLength > 1,
  );

  readonly amPmIssues = computed<string[]>(() => {
    const m = this.model();
    if (m.attendancePeriods.length === 0) return [];
    const out: string[] = [];
    for (let d = 0; d < m.timetableCycleLength; d++) {
      let am = 0;
      let pm = 0;
      for (const p of m.attendancePeriods) {
        if (p.cycleDayIndex !== d) continue;
        if (p.isAmReg) am++;
        if (p.isPmReg) pm++;
      }
      if (am === 1 && pm === 1) continue;
      const problems: string[] = [];
      if (am === 0) problems.push('needs AM reg');
      else if (am > 1) problems.push(`${am} AM regs (only 1 allowed)`);
      if (pm === 0) problems.push('needs PM reg');
      else if (pm > 1) problems.push(`${pm} PM regs (only 1 allowed)`);
      const label = this.cycleDayOptions().find(o => o.value === d)?.label ?? `Day ${d + 1}`;
      out.push(`${label}: ${problems.join(', ')}`);
    }
    return out;
  });

  ngOnInit(): void {
    const m = this.model();

    if (m.copyPeriodsFromAcademicYearId != null) {
      this.mode.set('copy');
    } else if (m.attendancePeriods.length > 0) {
      this.mode.set('define');
    }

    const cycle = m.timetableCycleLength;
    const inRange = m.attendancePeriods.filter(p => p.cycleDayIndex < cycle);
    if (!this.readOnly() && inRange.length !== m.attendancePeriods.length) {
      this.modelChange.emit({ attendancePeriods: inRange });
    }

    this.data.list().subscribe({
      next: rows => this.priorYears.set(rows ?? []),
      error: () => this.priorYears.set([]),
    });
  }

  setMode(mode: Mode): void {
    if (mode === this.mode()) return;
    this.mode.set(mode);
    if (mode === 'copy') {
      if (this.model().attendancePeriods.length > 0) {
        this.modelChange.emit({ attendancePeriods: [] });
      }
    } else {
      if (this.model().copyPeriodsFromAcademicYearId != null) {
        this.modelChange.emit({ copyPeriodsFromAcademicYearId: null });
      }
    }
  }

  onCopyYearPicked(id: string | null): void {
    this.modelChange.emit({ copyPeriodsFromAcademicYearId: id });
  }

  addPeriod(): void {
    const day = this.displayedDay();
    const existing = this.model().attendancePeriods.filter(p => p.cycleDayIndex === day);
    const last = existing[existing.length - 1];
    const newPeriod: AttendancePeriodUpsertRequest = {
      cycleDayIndex: day,
      name: '',
      startTime: last?.endTime ?? null,
      endTime: null,
      isAmReg: false,
      isPmReg: false,
      isLesson: true,
    };
    this.modelChange.emit({
      attendancePeriods: [...this.model().attendancePeriods, newPeriod],
    });
  }

  removePeriod(fullIndex: number): void {
    const updated = this.model().attendancePeriods.filter((_, i) => i !== fullIndex);
    this.modelChange.emit({ attendancePeriods: updated });
  }

  updatePeriod(fullIndex: number, patch: Partial<AttendancePeriodUpsertRequest>): void {
    const updated = this.model().attendancePeriods.map((p, i) =>
      i === fullIndex ? { ...p, ...patch } : p,
    );
    this.modelChange.emit({ attendancePeriods: updated });
  }

  async copyToAllDays(): Promise<void> {
    if (!this.canCopyToAllDays()) return;

    const day = this.displayedDay();
    const cycle = this.model().timetableCycleLength;
    const sourcePeriods = this.model().attendancePeriods.filter(p => p.cycleDayIndex === day);

    const daysWithPeriods = new Set<number>();
    for (const p of this.model().attendancePeriods) {
      if (p.cycleDayIndex !== day) daysWithPeriods.add(p.cycleDayIndex);
    }

    if (daysWithPeriods.size > 0) {
      const ok = await this.confirm.danger({
        message: this.transloco.translate('academic-years.wizard.periods.copyConfirm', {
          count: daysWithPeriods.size,
        }),
        acceptLabel: this.transloco.translate('academic-years.wizard.periods.copyToAllDays'),
      });
      if (!ok) return;
    }

    const kept = this.model().attendancePeriods.filter(p => p.cycleDayIndex === day);
    const cloned: AttendancePeriodUpsertRequest[] = [];
    for (let d = 0; d < cycle; d++) {
      if (d === day) continue;
      for (const sp of sourcePeriods) {
        cloned.push({ ...sp, cycleDayIndex: d });
      }
    }
    this.modelChange.emit({ attendancePeriods: [...kept, ...cloned] });
  }
}
