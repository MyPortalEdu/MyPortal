import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { Button } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
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
    InputText,
    Select,
    Button,
    DatePicker,
    TranslocoDirective,
  ],
  templateUrl: './holidays-step.html',
})
export class AcademicYearWizardHolidaysStep {
  private readonly transloco = inject(TranslocoService);

  readonly model = input.required<AcademicYearUpsertRequest>();
  // Disables every control and drops the add/remove actions — the step becomes
  // a read-back of a year the server won't let us change.
  readonly readOnly = input<boolean>(false);
  readonly modelChange = output<Partial<AcademicYearUpsertRequest>>();

  // Built once from the i18n bundle. Labels don't re-translate on a language
  // change but that's a non-issue today — only English is shipped, and the
  // wizard is short-lived (re-mounts on each launch from a fresh translation).
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
    // Default type = HalfTerm — by far the most common holiday a UK school
    // adds, so it saves a click. The user changes it via the type select if
    // they're adding a training day or public holiday.
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
