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
import { MpSelect, MpDatePicker, MpInputNumber, MpInput, MpButton } from '@myportal/ui';
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
    MpInputNumber,
    MpInput,
    MpSelect,
    MpButton,
    MpDatePicker,
    TranslocoDirective,
  ],
  templateUrl: './setup-step.html',
})
export class AcademicYearWizardSetupStep implements OnInit {
  private readonly data = inject(AcademicYearsDataService);

  readonly model = input.required<AcademicYearUpsertRequest>();
  readonly editMode = input<boolean>(false);
  readonly readOnly = input<boolean>(false);
  readonly modelChange = output<Partial<AcademicYearUpsertRequest>>();

  readonly priorYears = signal<AcademicYearSummary[]>([]);

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

  readonly weekChoices = computed<LabelledOption<number>[]>(() =>
    Array.from({ length: this.cycleWeeks() }, (_, i) => ({
      label: `Week ${String.fromCharCode(65 + i)}`,
      value: i,
    })),
  );

  readonly pastoralOptions = computed<LabelledOption<string | null>[]>(() => [
    { label: "Don't copy", value: null },
    ...this.priorYears().map(y => ({ label: `Copy from ${y.name}`, value: y.id })),
  ]);

  readonly yearName = computed(() => {
    const years = this.model().academicTerms
      .map(t => (t.startDate ? t.startDate.getFullYear() : null))
      .filter((y): y is number => y !== null);
    if (years.length === 0) return null;
    return `${Math.min(...years)}/${Math.max(...years)}`;
  });

  ngOnInit(): void {
    this.data.list().subscribe({
      next: rows => this.priorYears.set(rows ?? []),
      error: () => this.priorYears.set([]),
    });
  }

  onSchoolWeekLengthChange(value: number | null | undefined): void {
    const swl = Math.max(1, Math.min(7, value ?? 1));
    this.modelChange.emit({
      schoolWeekLength: swl,
      timetableCycleLength: this.cycleWeeks() * swl,
      firstWeekOffset: this.firstWeekIndex() * swl,
    });
  }

  onCycleWeeksChange(weeks: number): void {
    const swl = Math.max(1, this.model().schoolWeekLength);
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
