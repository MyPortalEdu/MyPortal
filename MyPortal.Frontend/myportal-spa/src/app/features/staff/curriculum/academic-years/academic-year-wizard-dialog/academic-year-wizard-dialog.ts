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
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { AcademicYearsDataService } from '../../../../../shared/services/academic-years-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { AcademicYearService } from '../../../../../core/services/academic-year-service';
import { AcademicYearUpsertRequest } from '../../../../../shared/types/academic-year';
import { AcademicYearWizardSetupStep } from './steps/setup-step';
import { AcademicYearWizardPeriodsStep } from './steps/periods-step';
import { AcademicYearWizardHolidaysStep } from './steps/holidays-step';
import { AcademicYearWizardReviewStep } from './steps/review-step';

// Step ordering is positional throughout the wizard — currentStep is an index
// into this list. The labels here drive the header chips; the per-step
// component renders the body.
const STEP_KEYS = ['setup', 'periods', 'holidays', 'review'] as const;
type StepKey = (typeof STEP_KEYS)[number];

function emptyRequest(): AcademicYearUpsertRequest {
  return {
    // Default to weekly 5-day Mon-Fri — overridden in step 1. Picking 0 here
    // would let the wizard render with an invalid cycle/week ratio before the
    // user has even touched the field.
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
    Button,
    Dialog,
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

  readonly closed = output<void>();
  readonly saved = output<void>();

  readonly stepKeys = STEP_KEYS;
  readonly currentStep = signal<number>(0);
  readonly submitting = signal<boolean>(false);

  // Single source of truth for the accumulated request as the user works
  // through the steps. Each step receives it as input and proposes changes
  // back through modelChange; the dialog owns the merge.
  readonly model = signal<AcademicYearUpsertRequest>(emptyRequest());

  readonly currentStepKey = computed<StepKey>(() => STEP_KEYS[this.currentStep()]);
  readonly isLastStep = computed(() => this.currentStep() === STEP_KEYS.length - 1);

  // Per-step gate. Derived from the accumulated model so each step's "ready
  // to advance?" question lives in one place. Steps currently scaffold to
  // permissive checks; the real per-field validation will tighten these as
  // each step gains its form. Step 3 (review) is purely a read-back, so it
  // doesn't need its own entry — the Create button replaces Next there.
  readonly canAdvance = computed(() => {
    const m = this.model();
    switch (this.currentStep()) {
      case 0: // setup
        return m.timetableCycleLength > 0
          && m.schoolWeekLength > 0
          && m.timetableCycleLength % m.schoolWeekLength === 0
          && m.firstWeekOffset >= 0
          && m.firstWeekOffset < m.timetableCycleLength
          && m.academicTerms.length > 0
          // Per-term sanity: server-side validator does the same plus overlap
          // detection; doing the easy checks here lets Next light up the moment
          // the user has typed valid values, rather than waiting for a 400.
          && m.academicTerms.every(term =>
              term.name.trim().length > 0
              && term.startDate !== null
              && term.endDate !== null
              && term.endDate > term.startDate);
      case 1: { // periods — exclusively copy-from OR inline (server rejects both)
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
              // BE CHECK constraint: a period must serve at least one role.
              && (p.isAmReg || p.isPmReg || p.isLesson)
              && p.cycleDayIndex >= 0
              && p.cycleDayIndex < cycle);
        if (!perPeriodValid) return false;
        // Statutory: every cycle day needs exactly one AM and one PM reg.
        // Lessons are unconstrained.
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
      case 2: // holidays — list is optional, but every row that exists must be complete
        return m.schoolHolidays.every(h =>
          h.name.trim().length > 0
          && h.startDate !== null
          && h.endDate !== null
          // Single-day holidays are legal (server's validator uses >=), so don't
          // require strict ordering — the user shouldn't have to enter the same
          // date twice for an INSET day.
          && h.endDate >= h.startDate);
      case 3: // review
        return true;
      default:
        return false;
    }
  });

  // Reset state every time the dialog opens — without this, re-opening after
  // a cancel would resume the previous wizard's progress.
  constructor() {
    effect(() => {
      if (this.visible()) {
        untracked(() => this.reset());
      }
    });
  }

  patchModel(patch: Partial<AcademicYearUpsertRequest>): void {
    this.model.update(m => ({ ...m, ...patch }));
  }

  next(): void {
    if (!this.canAdvance() || this.isLastStep()) return;
    this.currentStep.update(s => s + 1);
  }

  back(): void {
    if (this.currentStep() === 0) return;
    this.currentStep.update(s => s - 1);
  }

  async onCancel(): Promise<void> {
    // Only prompt if the user has made progress past step 1 — a blank wizard
    // that hasn't been touched is fine to dismiss silently.
    const dirty = this.currentStep() > 0;
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
    // PrimeNG fires onHide when the parent flips `visible` to false in
    // response to our own closed/saved emissions. The cancel-dirty prompt
    // lives in onCancel, so onHide just keeps the parent state in sync.
    this.closed.emit();
  }

  create(): void {
    if (!this.isLastStep() || this.submitting()) return;
    this.submitting.set(true);

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

  private reset(): void {
    this.currentStep.set(0);
    this.submitting.set(false);
    this.model.set(emptyRequest());
  }
}
