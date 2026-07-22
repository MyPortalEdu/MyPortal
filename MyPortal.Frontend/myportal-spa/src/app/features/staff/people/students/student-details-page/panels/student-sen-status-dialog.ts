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
import { MpButton, MpDatePicker, MpDialog, MpDialogFooter, MpSelect } from '@myportal/ui';
import { TranslocoDirective, provideTranslocoScope } from '@jsverse/transloco';

import { Field } from '../../../../../../shared/components/field/field';
import { LookupResponse } from '../../../../../../shared/types/lookup';
import { SetSenStatusRequest } from '../../../../../../shared/types/student-sen';

interface StatusModel {
  senStatusId: string | null;
  startDate: Date | null;
}

@Component({
  selector: 'mp-student-sen-status-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField, MpButton, MpDatePicker, MpDialog, MpDialogFooter, MpSelect, Field, TranslocoDirective],
  providers: [provideTranslocoScope('students')],
  templateUrl: './student-sen-status-dialog.html',
})
export class StudentSenStatusDialog {
  readonly open = input.required<boolean>();
  readonly senStatuses = input<LookupResponse[]>([]);
  readonly currentSenStatusId = input<string | null>(null);
  readonly currentStartDate = input<string | null>(null);
  readonly saving = input(false);

  readonly closed = output<void>();
  readonly save = output<SetSenStatusRequest>();

  protected readonly model = signal<StatusModel>({ senStatusId: null, startDate: null });
  protected readonly f = form(this.model, path => {
    validate(path.senStatusId, ({ value }) => (value() ? undefined : { kind: 'required' }));
    validate(path.startDate, ({ value }) => (value() ? undefined : { kind: 'required' }));
  });

  protected readonly submitted = computed(() => this.f().submitting());

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
        senStatusId: m.senStatusId as string,
        startDate: (m.startDate as Date).toISOString(),
      });
    });
  }

  private reset(): void {
    this.model.set({
      senStatusId: this.currentSenStatusId() ?? null,
      startDate: this.currentStartDate() ? new Date(this.currentStartDate() as string) : new Date(),
    });
    this.f().reset();
  }
}
