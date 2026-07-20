import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { FormField, form, required, submit, validate } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { MpButton, MpDialog, MpDialogFooter, MpFormField, MpInput } from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { UsersDataService } from '../../../../../shared/services/users-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';

@Component({
  selector: 'mp-user-set-password-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField, MpButton, MpDialog, MpDialogFooter, MpFormField, MpInput, TranslocoDirective, TranslocoPipe],
  providers: [provideTranslocoScope('users')],
  templateUrl: './user-set-password-dialog.html',
})
export class UserSetPasswordDialog {
  private readonly data = inject(UsersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly confirmDialog = inject(ConfirmationDialog);

  readonly visible = input.required<boolean>();
  readonly userId = input.required<string>();
  readonly username = input<string>('');

  readonly closed = output<void>();

  protected readonly model = signal({ password: '', confirm: '' });
  protected readonly f = form(this.model, path => {
    required(path.password);
    required(path.confirm);
    validate(path.confirm, ({ value, valueOf }) =>
      !value() || value() === valueOf(path.password)
        ? undefined
        : { kind: 'mismatch', message: 'users.password.mismatch' },
    );
  });

  constructor() {
    effect(() => {
      if (this.visible()) {
        untracked(() => this.reset());
      }
    });
  }

  async onCancel(): Promise<void> {
    if (this.f().dirty()) {
      const ok = await this.confirmDialog.confirm({
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

  save(): Promise<boolean> {
    return submit(this.f, async () => {
      try {
        await firstValueFrom(this.data.setPassword(this.userId(), { password: this.model().password }));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('users.password.error'));
        return;
      }
      this.notify.success(this.transloco.translate('users.password.savedToast'));
      this.closed.emit();
    });
  }

  private reset(): void {
    this.model.set({ password: '', confirm: '' });
    this.f().reset();
  }
}
