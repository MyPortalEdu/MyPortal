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
  MpInputNumber,
  MpSelect,
  MpTextarea,
} from '@myportal/ui';
import { TranslocoDirective, provideTranslocoScope } from '@jsverse/transloco';

import { Field } from '../../../../../../shared/components/field/field';
import { LookupResponse } from '../../../../../../shared/types/lookup';
import { SenNeedResponse, SenNeedUpsertRequest } from '../../../../../../shared/types/student-sen';

interface NeedModel {
  senTypeId: string | null;
  description: string;
  startDate: Date | null;
  endDate: Date | null;
  rank: number | null;
}

@Component({
  selector: 'mp-student-sen-need-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormField,
    MpButton,
    MpDatePicker,
    MpDialog,
    MpDialogFooter,
    MpInputNumber,
    MpSelect,
    MpTextarea,
    Field,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('students')],
  templateUrl: './student-sen-need-dialog.html',
})
export class StudentSenNeedDialog {
  readonly open = input.required<boolean>();
  readonly senTypes = input<LookupResponse[]>([]);
  readonly editTarget = input<SenNeedResponse | null>(null);
  readonly saving = input(false);

  readonly closed = output<void>();
  readonly save = output<SenNeedUpsertRequest>();

  protected readonly isEdit = computed(() => !!this.editTarget());

  protected readonly model = signal<NeedModel>({
    senTypeId: null,
    description: '',
    startDate: null,
    endDate: null,
    rank: 1,
  });
  protected readonly f = form(this.model, path => {
    validate(path.senTypeId, ({ value }) => (value() ? undefined : { kind: 'required' }));
    validate(path.startDate, ({ value }) => (value() ? undefined : { kind: 'required' }));
    validate(path.rank, ({ value }) => {
      const v = value();
      return v != null && v >= 1 ? undefined : { kind: 'min', message: 'students.sen.needs.rankMin' };
    });
    validate(path.endDate, ({ value, valueOf }) => {
      const start = valueOf(path.startDate);
      const end = value();
      return start && end && end.getTime() < start.getTime()
        ? { kind: 'range', message: 'students.sen.needs.endBeforeStart' }
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
        senTypeId: m.senTypeId as string,
        description: m.description.trim() || null,
        startDate: (m.startDate as Date).toISOString(),
        endDate: m.endDate?.toISOString() ?? null,
        rank: m.rank as number,
      });
    });
  }

  private reset(): void {
    const target = this.editTarget();
    this.model.set({
      senTypeId: target?.senTypeId ?? null,
      description: target?.description ?? '',
      startDate: target?.startDate ? new Date(target.startDate) : null,
      endDate: target?.endDate ? new Date(target.endDate) : null,
      rank: target?.rank ?? 1,
    });
    this.f().reset();
  }
}
