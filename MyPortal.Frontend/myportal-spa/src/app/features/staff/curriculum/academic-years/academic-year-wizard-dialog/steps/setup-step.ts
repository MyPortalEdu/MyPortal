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
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { Button } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { TranslocoDirective } from '@jsverse/transloco';

import { AcademicYearsDataService } from '../../../../../../shared/services/academic-years-data.service';
import { AcademicYearSummary } from '../../../../../../core/types/academic-year-summary';
import {
  AcademicTermUpsertRequest,
  AcademicYearUpsertRequest,
} from '../../../../../../shared/types/academic-year';

interface LabelledOption<T> {
  label: string;
  value: T;
}

@Component({
  selector: 'mp-academic-year-wizard-setup-step',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    InputNumber,
    InputText,
    Select,
    Button,
    DatePicker,
    TranslocoDirective,
  ],
  templateUrl: './setup-step.html',
})
export class AcademicYearWizardSetupStep implements OnInit {
  private readonly data = inject(AcademicYearsDataService);

  readonly model = input.required<AcademicYearUpsertRequest>();
  // Hides copy-from controls when true; the server rejects copy-from on
  // update so we don't surface the option in edit mode at all.
  readonly editMode = input<boolean>(false);
  // Disables every control and drops the add/remove actions — the step becomes
  // a read-back of a year the server won't let us change.
  readonly readOnly = input<boolean>(false);
  readonly modelChange = output<Partial<AcademicYearUpsertRequest>>();

  readonly priorYears = signal<AcademicYearSummary[]>([]);

  // The model stores days (TimetableCycleLength) and a day-offset (FirstWeekOffset)
  // because that's what the backend's cycle maths consumes. The UI exposes the
  // friendlier week-based concepts and converts at the boundary — kept as computed
  // signals so the model is the single source of truth and the UI can't drift.
  readonly cycleWeeks = computed(() => {
    const m = this.model();
    const swl = Math.max(1, m.schoolWeekLength);
    return Math.max(1, Math.floor(m.timetableCycleLength / swl));
  });

  readonly firstWeekIndex = computed(() => {
    const m = this.model();
    const swl = Math.max(1, m.schoolWeekLength);
    return Math.floor(m.firstWeekOffset / swl);
  });

  readonly cycleWeekChoices: LabelledOption<number>[] = [
    { label: 'Weekly (1 week)', value: 1 },
    { label: 'Fortnightly (2 weeks)', value: 2 },
    { label: 'Three-week', value: 3 },
    { label: 'Four-week', value: 4 },
  ];

  // Map index 0..n-1 to Week A..Week n. Only meaningful when cycle > 1 week.
  readonly weekChoices = computed<LabelledOption<number>[]>(() =>
    Array.from({ length: this.cycleWeeks() }, (_, i) => ({
      label: `Week ${String.fromCharCode(65 + i)}`,
      value: i,
    })),
  );

  // Pastoral-copy options: null = don't copy, plus each prior year.
  readonly pastoralOptions = computed<LabelledOption<string | null>[]>(() => [
    { label: "Don't copy", value: null },
    ...this.priorYears().map(y => ({ label: `Copy from ${y.name}`, value: y.id })),
  ]);

  // Derived year name preview — mirrors the backend's BuildAcademicYearName
  // (min start year / max start year across terms). Hidden until at least one
  // term has a start date.
  readonly yearName = computed(() => {
    const years = this.model().academicTerms
      .map(t => (t.startDate ? t.startDate.getFullYear() : null))
      .filter((y): y is number => y !== null);
    if (years.length === 0) return null;
    return `${Math.min(...years)}/${Math.max(...years)}`;
  });

  ngOnInit(): void {
    // Used only for the "copy pastoral structure from…" picker. Fail-soft —
    // if the list call errors, the section just hides; the rest of step 1
    // (cycle, terms, etc.) is unaffected.
    this.data.list().subscribe({
      next: rows => this.priorYears.set(rows ?? []),
      error: () => this.priorYears.set([]),
    });
  }

  onSchoolWeekLengthChange(value: number | null | undefined): void {
    const swl = Math.max(1, Math.min(7, value ?? 1));
    // Recompute timetableCycleLength + firstWeekOffset from the user-facing
    // week-based concepts so the multiple-of-swl invariant is preserved when
    // days-per-week changes (otherwise the cycle/week ratio could drift).
    this.modelChange.emit({
      schoolWeekLength: swl,
      timetableCycleLength: this.cycleWeeks() * swl,
      firstWeekOffset: this.firstWeekIndex() * swl,
    });
  }

  onCycleWeeksChange(weeks: number): void {
    const swl = Math.max(1, this.model().schoolWeekLength);
    // Clamp firstWeekIndex when shrinking the cycle — going from 4 weeks with
    // "Week D" picked down to 2 weeks would otherwise leave a stale out-of-range
    // index, which the server's FirstWeekOffset < cycle check would reject.
    const idx = Math.min(this.firstWeekIndex(), weeks - 1);
    this.modelChange.emit({
      timetableCycleLength: weeks * swl,
      firstWeekOffset: idx * swl,
    });
  }

  onFirstWeekIndexChange(idx: number): void {
    const swl = Math.max(1, this.model().schoolWeekLength);
    this.modelChange.emit({ firstWeekOffset: idx * swl });
  }

  addTerm(): void {
    const terms: AcademicTermUpsertRequest[] = [
      ...this.model().academicTerms,
      { name: '', startDate: null, endDate: null },
    ];
    this.modelChange.emit({ academicTerms: terms });
  }

  removeTerm(index: number): void {
    const terms = this.model().academicTerms.filter((_, i) => i !== index);
    this.modelChange.emit({ academicTerms: terms });
  }

  updateTermName(index: number, name: string): void {
    const terms = this.model().academicTerms.map((t, i) =>
      i === index ? { ...t, name } : t,
    );
    this.modelChange.emit({ academicTerms: terms });
  }

  // Stores the Date instance the picker emitted verbatim — re-creating a Date
  // in a getter caused a render/emit feedback loop (fresh reference each CD →
  // PrimeNG saw a "new" value → ngModelChange fired → repeat).
  updateTermDate(index: number, field: 'startDate' | 'endDate', value: Date | null): void {
    const terms = this.model().academicTerms.map((t, i) =>
      i === index ? { ...t, [field]: value } : t,
    );
    this.modelChange.emit({ academicTerms: terms });
  }

  onPastoralYearPicked(id: string | null): void {
    this.modelChange.emit({ copyPastoralStructureFromAcademicYearId: id });
  }
}
