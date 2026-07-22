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
  MpDatePicker,
  MpDialog,
  MpDialogFooter,
  MpSelect,
  MpTextarea,
} from '@myportal/ui';
import { TranslocoDirective, provideTranslocoScope } from '@jsverse/transloco';

import { Field } from '../../../../../../shared/components/field/field';
import { LookupResponse } from '../../../../../../shared/types/lookup';
import {
  ChildProtectionPlanResponse,
  ChildProtectionPlanUpsertRequest,
} from '../../../../../../shared/types/student-welfare';

interface CpModel {
  localAuthorityId: string | null;
  startDate: Date | null;
  endDate: Date | null;
  comment: string;
}

@Component({
  selector: 'mp-student-welfare-cp-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormField,
    MpButton,
    MpDatePicker,
    MpDialog,
    MpDialogFooter,
    MpSelect,
    MpTextarea,
    Field,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('students')],
  templateUrl: './student-welfare-cp-dialog.html',
})
export class StudentWelfareCpDialog {
  readonly open = input.required<boolean>();
  readonly authorities = input<LookupResponse[]>([]);
  readonly editTarget = input<ChildProtectionPlanResponse | null>(null);
  readonly saving = input(false);

  readonly closed = output<void>();
  readonly save = output<ChildProtectionPlanUpsertRequest>();

  protected readonly isEdit = computed(() => !!this.editTarget());

  protected readonly model = signal<CpModel>(this.empty());
  protected readonly f = form(this.model, path => {
    validate(path.startDate, ({ value }) => (value() ? undefined : { kind: 'required' }));
    validate(path.endDate, ({ value, valueOf }) => {
      const start = valueOf(path.startDate);
      const end = value();
      return start && end && end.getTime() < start.getTime()
        ? { kind: 'range', message: 'students.welfare.childProtectionPlans.endBeforeStart' }
        : undefined;
    });
  });

  constructor() {
    effect(() => {
      if (this.open()) untracked(() => this.reset());
    });
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
        localAuthorityId: m.localAuthorityId,
        startDate: (m.startDate as Date).toISOString(),
        endDate: m.endDate?.toISOString() ?? null,
        comment: m.comment.trim() || null,
      });
    });
  }

  private reset(): void {
    const t = this.editTarget();
    this.model.set(
      t
        ? {
            localAuthorityId: t.localAuthorityId ?? null,
            startDate: new Date(t.startDate),
            endDate: t.endDate ? new Date(t.endDate) : null,
            comment: t.comment ?? '',
          }
        : this.empty(),
    );
    this.f().reset();
  }

  private empty(): CpModel {
    return {
      localAuthorityId: null,
      startDate: null,
      endDate: null,
      comment: '',
    };
  }
}
