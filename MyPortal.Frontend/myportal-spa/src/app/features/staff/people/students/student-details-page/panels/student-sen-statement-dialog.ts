import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { FormField, form, submit, validate } from '@angular/forms/signals';
import {
  MpButton,
  MpCheckbox,
  MpDatePicker,
  MpDialog,
  MpDialogFooter,
  MpInput,
  MpSelect,
  MpTextarea,
} from '@myportal/ui';
import { TranslocoDirective, provideTranslocoScope } from '@jsverse/transloco';

import { Field } from '../../../../../../shared/components/field/field';
import { LocalAuthorityPicker } from '../../../../../../shared/components/pickers/local-authority-picker/local-authority-picker';
import { LookupResponse } from '../../../../../../shared/types/lookup';
import { LocalAuthoritySummaryResponse } from '../../../../../../shared/types/school';
import {
  SenStatementResponse,
  SenStatementUpsertRequest,
} from '../../../../../../shared/types/student-sen';

interface StatementModel {
  isEhcp: boolean;
  assessmentRequestDate: Date | null;
  parentConsultDate: Date | null;
  finalisedDate: Date | null;
  ceasedDate: Date | null;
  statutoryAssessmentAgreedId: string | null;
  statutoryAssessmentOutcomeId: string | null;
  subjectToTribunal: boolean;
  undergoingMediation: boolean;
  appealNotes: string;
  temporaryDisapplicationSubjects: string;
  permanentDisapplicationSubjects: string;
  localAuthorityId: string | null;
  comments: string;
}

@Component({
  selector: 'mp-student-sen-statement-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormField,
    MpButton,
    MpCheckbox,
    MpDatePicker,
    MpDialog,
    MpDialogFooter,
    MpInput,
    MpSelect,
    MpTextarea,
    Field,
    LocalAuthorityPicker,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('students')],
  templateUrl: './student-sen-statement-dialog.html',
})
export class StudentSenStatementDialog {
  readonly open = input.required<boolean>();
  readonly assessmentAgreedOptions = input<LookupResponse[]>([]);
  readonly assessmentOutcomeOptions = input<LookupResponse[]>([]);
  readonly editTarget = input<SenStatementResponse | null>(null);
  readonly saving = input(false);

  readonly closed = output<void>();
  readonly save = output<SenStatementUpsertRequest>();

  protected readonly isEdit = computed(() => !!this.editTarget());
  protected readonly localAuthorityName = signal<string | null>(null);

  protected readonly model = signal<StatementModel>(this.empty());
  protected readonly f = form(this.model, path => {
    validate(path.assessmentRequestDate, ({ value }) => (value() ? undefined : { kind: 'required' }));
    validate(path.parentConsultDate, ({ value, valueOf }) =>
      this.beforeRequest(value(), valueOf(path.assessmentRequestDate))
        ? { kind: 'range', message: 'students.sen.statements.beforeRequest' }
        : undefined,
    );
    validate(path.finalisedDate, ({ value, valueOf }) =>
      this.beforeRequest(value(), valueOf(path.assessmentRequestDate))
        ? { kind: 'range', message: 'students.sen.statements.beforeRequest' }
        : undefined,
    );
    validate(path.ceasedDate, ({ value, valueOf }) => {
      const ceased = value();
      if (!ceased) return undefined;
      if (this.beforeRequest(ceased, valueOf(path.assessmentRequestDate)))
        return { kind: 'range', message: 'students.sen.statements.beforeRequest' };
      const finalised = valueOf(path.finalisedDate);
      return finalised && ceased.getTime() < finalised.getTime()
        ? { kind: 'range', message: 'students.sen.statements.ceasedBeforeFinalised' }
        : undefined;
    });
  });

  constructor() {
    effect(() => {
      if (this.open()) untracked(() => this.reset());
    });
  }

  protected onLocalAuthorityPicked(la: LocalAuthoritySummaryResponse): void {
    this.localAuthorityName.set(la.name);
    this.model.update(m => ({ ...m, localAuthorityId: la.id }));
  }

  protected clearLocalAuthority(): void {
    this.localAuthorityName.set(null);
    this.model.update(m => ({ ...m, localAuthorityId: null }));
  }

  protected onClose(): void {
    if (this.saving()) return;
    this.closed.emit();
  }

  protected async submit(): Promise<void> {
    await submit(this.f, async () => {
      const m = this.model();
      this.save.emit({
        id: this.editTarget()?.id ?? null,
        isEhcp: m.isEhcp,
        assessmentRequestDate: (m.assessmentRequestDate as Date).toISOString(),
        parentConsultDate: m.parentConsultDate?.toISOString() ?? null,
        finalisedDate: m.finalisedDate?.toISOString() ?? null,
        ceasedDate: m.ceasedDate?.toISOString() ?? null,
        statutoryAssessmentAgreedId: m.statutoryAssessmentAgreedId,
        statutoryAssessmentOutcomeId: m.statutoryAssessmentOutcomeId,
        subjectToTribunal: m.subjectToTribunal,
        undergoingMediation: m.undergoingMediation,
        appealNotes: m.appealNotes.trim() || null,
        temporaryDisapplicationSubjects: m.temporaryDisapplicationSubjects.trim() || null,
        permanentDisapplicationSubjects: m.permanentDisapplicationSubjects.trim() || null,
        localAuthorityId: m.localAuthorityId,
        comments: m.comments.trim() || null,
      });
    });
  }

  private beforeRequest(value: Date | null, request: Date | null): boolean {
    return !!value && !!request && value.getTime() < request.getTime();
  }

  private reset(): void {
    const t = this.editTarget();
    this.localAuthorityName.set(null);
    this.model.set(
      t
        ? {
            isEhcp: t.isEhcp,
            assessmentRequestDate: new Date(t.assessmentRequestDate),
            parentConsultDate: t.parentConsultDate ? new Date(t.parentConsultDate) : null,
            finalisedDate: t.finalisedDate ? new Date(t.finalisedDate) : null,
            ceasedDate: t.ceasedDate ? new Date(t.ceasedDate) : null,
            statutoryAssessmentAgreedId: t.statutoryAssessmentAgreedId ?? null,
            statutoryAssessmentOutcomeId: t.statutoryAssessmentOutcomeId ?? null,
            subjectToTribunal: t.subjectToTribunal,
            undergoingMediation: t.undergoingMediation,
            appealNotes: t.appealNotes ?? '',
            temporaryDisapplicationSubjects: t.temporaryDisapplicationSubjects ?? '',
            permanentDisapplicationSubjects: t.permanentDisapplicationSubjects ?? '',
            localAuthorityId: t.localAuthorityId ?? null,
            comments: t.comments ?? '',
          }
        : this.empty(),
    );
    this.f().reset();
  }

  private empty(): StatementModel {
    return {
      isEhcp: false,
      assessmentRequestDate: null,
      parentConsultDate: null,
      finalisedDate: null,
      ceasedDate: null,
      statutoryAssessmentAgreedId: null,
      statutoryAssessmentOutcomeId: null,
      subjectToTribunal: false,
      undergoingMediation: false,
      appealNotes: '',
      temporaryDisapplicationSubjects: '',
      permanentDisapplicationSubjects: '',
      localAuthorityId: null,
      comments: '',
    };
  }
}
