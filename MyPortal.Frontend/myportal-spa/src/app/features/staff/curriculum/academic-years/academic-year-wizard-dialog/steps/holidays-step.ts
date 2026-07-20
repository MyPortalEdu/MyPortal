import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MpSelect, MpDatePicker, MpInput, MpButton } from '@myportal/ui';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';

import {
  AcademicYearUpsertRequest,
  SchoolHolidayType,
  SchoolHolidayUpsertRequest,
} from '../../../../../../shared/types/academic-year';

interface LabelledOption<T> {
  label: string;
  value: T;
}

@Component({
  selector: 'mp-academic-year-wizard-holidays-step',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    MpInput,
    MpSelect,
    MpButton,
    MpDatePicker,
    TranslocoDirective,
  ],
  templateUrl: './holidays-step.html',
})
export class AcademicYearWizardHolidaysStep {
  private readonly transloco = inject(TranslocoService);

  readonly model = input.required<AcademicYearUpsertRequest>();
  readonly readOnly = input<boolean>(false);
  readonly modelChange = output<Partial<AcademicYearUpsertRequest>>();

  readonly holidayTypes: LabelledOption<SchoolHolidayType>[] = [
    {
      label: this.transloco.translate('academic-years.wizard.holidays.typeHalfTerm'),
      value: SchoolHolidayType.HalfTerm,
    },
    {
      label: this.transloco.translate('academic-years.wizard.holidays.typeTeacherTraining'),
      value: SchoolHolidayType.TeacherTraining,
    },
    {
      label: this.transloco.translate('academic-years.wizard.holidays.typePublicHoliday'),
      value: SchoolHolidayType.PublicHoliday,
    },
  ];

  readonly hasHolidays = computed(() => this.model().schoolHolidays.length > 0);

  addHoliday(): void {
    const newHoliday: SchoolHolidayUpsertRequest = {
      name: '',
      type: SchoolHolidayType.HalfTerm,
      startDate: null,
      endDate: null,
    };
    this.modelChange.emit({
      schoolHolidays: [...this.model().schoolHolidays, newHoliday],
    });
  }

  removeHoliday(index: number): void {
    const updated = this.model().schoolHolidays.filter((_, i) => i !== index);
    this.modelChange.emit({ schoolHolidays: updated });
  }

  updateHoliday(index: number, patch: Partial<SchoolHolidayUpsertRequest>): void {
    const updated = this.model().schoolHolidays.map((h, i) =>
      i === index ? { ...h, ...patch } : h,
    );
    this.modelChange.emit({ schoolHolidays: updated });
  }
}
