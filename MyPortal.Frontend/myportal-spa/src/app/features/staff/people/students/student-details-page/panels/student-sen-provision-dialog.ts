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
  MpInput,
  MpInputNumber,
  MpSelect,
  MpTextarea,
} from '@myportal/ui';
import { TranslocoDirective, provideTranslocoScope } from '@jsverse/transloco';

import { Field } from '../../../../../../shared/components/field/field';
import { LookupResponse } from '../../../../../../shared/types/lookup';
import {
  SenProvisionResponse,
  SenProvisionUpsertRequest,
} from '../../../../../../shared/types/student-sen';

interface ProvisionModel {
  senProvisionTypeId: string | null;
  startDate: Date | null;
  endDate: Date | null;
  frequency: string;
  cost: number | null;
  note: string;
}

@Component({
  selector: 'mp-student-sen-provision-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormField,
    MpButton,
    MpDatePicker,
    MpDialog,
    MpDialogFooter,
    MpInput,
    MpInputNumber,
    MpSelect,
    MpTextarea,
    Field,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('students')],
  templateUrl: './student-sen-provision-dialog.html',
})
export class StudentSenProvisionDialog {
  readonly open = input.required<boolean>();
  readonly provisionTypes = input<LookupResponse[]>([]);
  readonly editTarget = input<SenProvisionResponse | null>(null);
  readonly saving = input(false);

  readonly closed = output<void>();
  readonly save = output<SenProvisionUpsertRequest>();

  protected readonly isEdit = computed(() => !!this.editTarget());

  protected readonly model = signal<ProvisionModel>({
    senProvisionTypeId: null,
    startDate: null,
    endDate: null,
    frequency: '',
    cost: null,
    note: '',
  });
  protected readonly f = form(this.model, path => {
    validate(path.senProvisionTypeId, ({ value }) => (value() ? undefined : { kind: 'required' }));
    validate(path.startDate, ({ value }) => (value() ? undefined : { kind: 'required' }));
    validate(path.note, ({ value }) => (value().trim() ? undefined : { kind: 'required' }));
    validate(path.endDate, ({ value, valueOf }) => {
      const start = valueOf(path.startDate);
      const end = value();
      return start && end && end.getTime() < start.getTime()
        ? { kind: 'range', message: 'students.sen.provisions.endBeforeStart' }
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
        senProvisionTypeId: m.senProvisionTypeId as string,
        startDate: (m.startDate as Date).toISOString(),
        endDate: m.endDate?.toISOString() ?? null,
        frequency: m.frequency.trim() || null,
        cost: m.cost,
        note: m.note.trim(),
      });
    });
  }

  private reset(): void {
    const target = this.editTarget();
    this.model.set({
      senProvisionTypeId: target?.senProvisionTypeId ?? null,
      startDate: target?.startDate ? new Date(target.startDate) : null,
      endDate: target?.endDate ? new Date(target.endDate) : null,
      frequency: target?.frequency ?? '',
      cost: target?.cost ?? null,
      note: target?.note ?? '',
    });
    this.f().reset();
  }
}
