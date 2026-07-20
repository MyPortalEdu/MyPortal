import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { MpDialog, MpButton, MpSpinner } from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { AcademicYearsDataService } from '../../../../../shared/services/academic-years-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { AcademicYearService } from '../../../../../core/services/academic-year-service';
import {
  AcademicTermUpsertRequest,
  AcademicYearUpsertRequest,
  SchoolHolidayUpsertRequest,
} from '../../../../../shared/types/academic-year';
import { AcademicYearDetailsResponse } from '../../../../../shared/types/academic-year-details';
import { Callout } from '../../../../../shared/components/callout/callout';
import { AcademicYearWizardSetupStep } from './steps/setup-step';
import { AcademicYearWizardPeriodsStep } from './steps/periods-step';
import { AcademicYearWizardHolidaysStep } from './steps/holidays-step';
import { AcademicYearWizardReviewStep } from './steps/review-step';

const STEP_KEYS = ['setup', 'periods', 'holidays', 'review'] as const;
type StepKey = (typeof STEP_KEYS)[number];

function emptyRequest(): AcademicYearUpsertRequest {
  return {
    timetableCycleLength: 5,
    schoolWeekLength: 5,
    firstWeekOffset: 0,
    copyPeriodsFromAcademicYearId: null,
    copyPastoralStructureFromAcademicYearId: null,
    academicTerms: [],
    attendancePeriods: [],
    schoolHolidays: [],
  };
}

@Component({
  selector: 'mp-academic-year-wizard-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MpButton,
    MpDialog,
    MpSpinner,
    Callout,
    TranslocoDirective,
    TranslocoPipe,
    AcademicYearWizardSetupStep,
    AcademicYearWizardPeriodsStep,
    AcademicYearWizardHolidaysStep,
    AcademicYearWizardReviewStep,
  ],
  providers: [provideTranslocoScope('academic-years')],
  templateUrl: './academic-year-wizard-dialog.html',
})
export class AcademicYearWizardDialog {
  private readonly data = inject(AcademicYearsDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);
  private readonly currentYearCache = inject(AcademicYearService);

  readonly visible = input.required<boolean>();
  readonly editYearId = input<string | null>(null);
  readonly readOnly = input<boolean>(false);
  readonly readOnlyReason = input<string>('');

  readonly closed = output<void>();
  readonly saved = output<void>();

  readonly stepKeys = STEP_KEYS;
  readonly currentStep = signal<number>(0);
  readonly submitting = signal<boolean>(false);
  readonly loading = signal<boolean>(false);

  readonly model = signal<AcademicYearUpsertRequest>(emptyRequest());

  readonly currentStepKey = computed<StepKey>(() => STEP_KEYS[this.currentStep()]);
  readonly isLastStep = computed(() => this.currentStep() === STEP_KEYS.length - 1);
  readonly isEditMode = computed(() => this.editYearId() != null);

  readonly canAdvance = computed(() => {
    const m = this.model();
    switch (this.currentStep()) {
      case 0:
        return m.timetableCycleLength > 0
          && m.schoolWeekLength > 0
          && m.timetableCycleLength % m.schoolWeekLength === 0
          && m.firstWeekOffset >= 0
          && m.firstWeekOffset < m.timetableCycleLength
          && m.academicTerms.length > 0
          && m.academicTerms.every(term =>
              term.name.trim().length > 0
              && term.startDate !== null
              && term.endDate !== null
              && term.endDate > term.startDate)
          && termsDoNotOverlap(m.academicTerms);
      case 1: {
        if (m.copyPeriodsFromAcademicYearId != null) {
          return m.attendancePeriods.length === 0;
        }
        const cycle = m.timetableCycleLength;
        const perPeriodValid = m.attendancePeriods.length > 0
          && m.attendancePeriods.every(p =>
              p.name.trim().length > 0
              && p.startTime !== null
              && p.endTime !== null
              && p.endTime > p.startTime
              && (p.isAmReg || p.isPmReg || p.isLesson)
              && p.cycleDayIndex >= 0
              && p.cycleDayIndex < cycle);
        if (!perPeriodValid) return false;
        for (let d = 0; d < cycle; d++) {
          let am = 0;
          let pm = 0;
          for (const p of m.attendancePeriods) {
            if (p.cycleDayIndex !== d) continue;
            if (p.isAmReg) am++;
            if (p.isPmReg) pm++;
          }
          if (am !== 1 || pm !== 1) return false;
        }
        return true;
      }
      case 2:
        return m.schoolHolidays.every(h =>
          h.name.trim().length > 0
          && h.startDate !== null
          && h.endDate !== null
          && h.endDate >= h.startDate)
          && holidaysWithinTermSpan(m.academicTerms, m.schoolHolidays);
      case 3:
        return true;
      default:
        return false;
    }
  });

  readonly canGoNext = computed(() => this.readOnly() || this.canAdvance());

  readonly termsOverlap = computed(
    () => this.currentStep() === 0 && !termsDoNotOverlap(this.model().academicTerms),
  );
  readonly holidaysOutOfSpan = computed(
    () =>
      this.currentStep() === 2 &&
      !holidaysWithinTermSpan(this.model().academicTerms, this.model().schoolHolidays),
  );

  constructor() {
    effect(() => {
      if (this.visible()) {
        untracked(() => {
          this.reset();
          const id = this.editYearId();
          if (id) {
            this.loadForEdit(id);
          }
        });
      }
    });
  }

  patchModel(patch: Partial<AcademicYearUpsertRequest>): void {
    this.model.update(m => ({ ...m, ...patch }));
  }

  next(): void {
    if (!this.canGoNext() || this.isLastStep()) return;
    this.currentStep.update(s => s + 1);
  }

  back(): void {
    if (this.currentStep() === 0) return;
    this.currentStep.update(s => s - 1);
  }

  async onCancel(): Promise<void> {
    const dirty = !this.readOnly() && (this.currentStep() > 0 || this.isEditMode());
    if (dirty) {
      const ok = await this.confirm.confirm({
        header: this.transloco.translate('common.discardChanges'),
        message: this.transloco.translate('common.discardConfirm'),
        acceptLabel: this.transloco.translate('common.discard'),
        acceptSeverity: 'danger',
      });
      if (!ok) return;
    }
    this.closed.emit();
  }

  onHide(): void {
    this.closed.emit();
  }

  save(): void {
    if (this.readOnly() || !this.isLastStep() || this.submitting()) return;
    this.submitting.set(true);

    const id = this.editYearId();
    if (id) {
      const payload: AcademicYearUpsertRequest = {
        ...this.model(),
        copyPeriodsFromAcademicYearId: null,
        copyPastoralStructureFromAcademicYearId: null,
      };
      this.data.update(id, payload).subscribe({
        next: () => {
          this.submitting.set(false);
          this.currentYearCache.clearCache();
          this.notify.success(this.transloco.translate('academic-years.wizard.updatedToast'));
          this.saved.emit();
        },
        error: err => {
          this.submitting.set(false);
          this.notify.apiError(err, this.transloco.translate('academic-years.wizard.errorUpdate'));
        },
      });
    } else {
      this.data.create(this.model()).subscribe({
        next: () => {
          this.submitting.set(false);
          this.currentYearCache.clearCache();
          this.notify.success(this.transloco.translate('academic-years.wizard.createdToast'));
          this.saved.emit();
        },
        error: err => {
          this.submitting.set(false);
          this.notify.apiError(err, this.transloco.translate('academic-years.wizard.errorCreate'));
        },
      });
    }
  }

  private reset(): void {
    this.currentStep.set(0);
    this.submitting.set(false);
    this.loading.set(false);
    this.model.set(emptyRequest());
  }

  private loadForEdit(id: string): void {
    this.loading.set(true);
    this.data.getById(id).subscribe({
      next: details => {
        this.model.set(detailsToUpsert(details));
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('academic-years.wizard.errorLoad'));
        this.closed.emit();
      },
    });
  }
}

function detailsToUpsert(d: AcademicYearDetailsResponse): AcademicYearUpsertRequest {
  return {
    timetableCycleLength: d.timetableCycleLength,
    schoolWeekLength: d.schoolWeekLength,
    firstWeekOffset: 0,
    copyPeriodsFromAcademicYearId: null,
    copyPastoralStructureFromAcademicYearId: null,
    academicTerms: d.terms.map(t => ({
      academicTermId: t.id,
      name: t.name,
      startDate: parseLocalDate(t.startDate),
      endDate: parseLocalDate(t.endDate),
    })),
    attendancePeriods: d.attendancePeriods.map(p => ({
      attendancePeriodId: p.id,
      cycleDayIndex: p.cycleDayIndex,
      name: p.name,
      startTime: parseLocalTime(p.startTime),
      endTime: parseLocalTime(p.endTime),
      isAmReg: p.isAmReg,
      isPmReg: p.isPmReg,
      isLesson: p.isLesson,
    })),
    schoolHolidays: d.schoolHolidays.map(h => ({
      schoolHolidayId: h.id,
      name: h.name,
      type: h.type,
      startDate: parseLocalDate(h.startDate),
      endDate: parseLocalDate(h.endDate),
    })),
  };
}

function parseLocalDate(iso: string): Date {
  return new Date(iso);
}

function parseLocalTime(hms: string): Date {
  const [h, m, s] = hms.split(':').map(Number);
  const d = new Date();
  d.setHours(h, m, s ?? 0, 0);
  return d;
}

function termsDoNotOverlap(terms: AcademicTermUpsertRequest[]): boolean {
  const ordered = terms
    .filter(t => t.startDate && t.endDate)
    .slice()
    .sort((a, b) => a.startDate!.getTime() - b.startDate!.getTime());
  for (let i = 1; i < ordered.length; i++) {
    if (ordered[i].startDate!.getTime() <= ordered[i - 1].endDate!.getTime()) {
      return false;
    }
  }
  return true;
}

function holidaysWithinTermSpan(
  terms: AcademicTermUpsertRequest[],
  holidays: SchoolHolidayUpsertRequest[],
): boolean {
  const starts = terms.filter(t => t.startDate).map(t => t.startDate!.getTime());
  const ends = terms.filter(t => t.endDate).map(t => t.endDate!.getTime());
  if (!starts.length || !ends.length) return true;
  const yearStart = Math.min(...starts);
  const yearEnd = Math.max(...ends);
  return holidays.every(
    h =>
      !h.startDate ||
      !h.endDate ||
      (h.startDate.getTime() >= yearStart && h.endDate.getTime() <= yearEnd),
  );
}
