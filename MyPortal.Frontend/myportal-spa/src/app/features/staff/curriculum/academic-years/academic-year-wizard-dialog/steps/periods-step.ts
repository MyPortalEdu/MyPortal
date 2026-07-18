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
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { SelectButton } from 'primeng/selectbutton';
import { Button } from 'primeng/button';
import { Checkbox } from 'primeng/checkbox';
import { DatePicker } from 'primeng/datepicker';
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
    InputText,
    Select,
    SelectButton,
    Button,
    Checkbox,
    DatePicker,
    TranslocoDirective,
  ],
  templateUrl: './periods-step.html',
})
export class AcademicYearWizardPeriodsStep implements OnInit {
  private readonly data = inject(AcademicYearsDataService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly model = input.required<AcademicYearUpsertRequest>();
  // Hides the define/copy mode switch and forces 'define' when true. The
  // server rejects copy-from on update so edit mode only ever submits inline
  // periods.
  readonly editMode = input<boolean>(false);
  // Disables every control and drops the add/remove/copy actions — the step
  // becomes a read-back of a year the server won't let us change.
  readonly readOnly = input<boolean>(false);
  readonly modelChange = output<Partial<AcademicYearUpsertRequest>>();

  readonly priorYears = signal<AcademicYearSummary[]>([]);

  // Local UI state — which cycle day is being edited. Independent of the model
  // (the model is a flat list keyed by cycleDayIndex; the day switcher is UX).
  readonly selectedCycleDay = signal(0);

  // Local mode toggle. The server's validator rejects both inline AND copy
  // being set, so the user has to pick one. Initialised from the model in
  // ngOnInit so navigating back into step 2 preserves the previous choice.
  readonly mode = signal<Mode>('define');

  readonly modeOptions: LabelledOption<Mode>[] = [
    { label: 'Define new periods', value: 'define' },
    { label: 'Copy from existing year', value: 'copy' },
  ];

  // Cycle-day labels. For a multi-week cycle the day name gets a "Week A/B/…"
  // suffix so the user can tell which week of the cycle each tab represents.
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

  // If the user shrank the cycle in step 1, the previously-selected day may
  // now be out of range. Clamp at read time so the UI never shows a non-day.
  readonly displayedDay = computed(() => {
    const cycle = this.model().timetableCycleLength;
    return Math.min(this.selectedCycleDay(), Math.max(0, cycle - 1));
  });

  readonly displayedDayLabel = computed(() =>
    this.cycleDayOptions().find(o => o.value === this.displayedDay())?.label ?? '',
  );

  // Periods for the currently-shown cycle day, with their index in the flat
  // attendancePeriods array attached so update/remove handlers can locate the
  // row without re-scanning.
  readonly periodsForDay = computed(() =>
    this.model().attendancePeriods
      .map((period, fullIndex) => ({ period, fullIndex }))
      .filter(x => x.period.cycleDayIndex === this.displayedDay()),
  );

  readonly priorYearOptions = computed<LabelledOption<string>[]>(() =>
    this.priorYears().map(y => ({ label: y.name, value: y.id })),
  );

  // Copy-to-all only makes sense when there's actually something to copy AND
  // somewhere to copy it to.
  readonly canCopyToAllDays = computed(() =>
    this.periodsForDay().length > 0 && this.model().timetableCycleLength > 1,
  );

  // Surfaces the AM/PM rule violations so the user knows WHY Next is disabled
  // rather than having to guess. Only computed when in define mode and the
  // user has actually started defining periods — empty define-mode is its own
  // (more obvious) blocking state covered by the "Add period" CTA.
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

    // Restore mode from the model so users who navigate back into this step
    // don't lose their previous choice.
    if (m.copyPeriodsFromAcademicYearId != null) {
      this.mode.set('copy');
    } else if (m.attendancePeriods.length > 0) {
      this.mode.set('define');
    }

    // Drop any periods that point at a cycle-day no longer in range (e.g. the
    // user shrank the cycle on step 1 after defining periods on a now-removed
    // day). Otherwise they'd be invisible in the UI but flunk the server's
    // CycleDayIndex < cycle check on submit.
    // Read-only never submits, so leave the loaded data exactly as the server
    // returned it.
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
    // Server-side validator rejects both inline AND copy being set. Switching
    // modes clears whichever data is no longer relevant so the user can't
    // accidentally submit an ambiguous payload.
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
    // Chain the new period's startTime to the previous period's endTime when
    // one exists — most schools' periods run back-to-back, so this saves a
    // click. New default: IsLesson=true (everything is a taught period unless
    // the user says otherwise).
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

    // Only prompt when overwriting something — copying onto empty days is
    // a safe, expected operation. Shared Date references in the cloned
    // periods are fine: the model only ever replaces Date instances, never
    // mutates them in place.
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

    // Keep current day's rows as-is, then synthesise a copy for every other
    // cycle day. Builds a fresh array so signal change detection picks it up.
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
