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
import { FormsModule } from '@angular/forms';
import { MpButton, MpDialog, MpDialogFooter, MpInput } from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { UsersDataService } from '../../../../../shared/services/users-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';

@Component({
  selector: 'mp-user-set-password-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpButton, MpDialog, MpDialogFooter, MpInput, TranslocoDirective, TranslocoPipe],
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

  readonly password = signal('');
  readonly confirm = signal('');
  readonly submitting = signal(false);

  readonly isValid = computed(() => this.password().length > 0 && this.password() === this.confirm());
  readonly mismatch = computed(() => this.confirm().length > 0 && this.password() !== this.confirm());
  readonly isDirty = computed(() => this.password().length > 0 || this.confirm().length > 0);

  constructor() {
    effect(() => {
      if (this.visible()) {
        untracked(() => this.reset());
      }
    });
  }

  async onCancel(): Promise<void> {
    if (this.isDirty()) {
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

  save(): void {
    if (!this.isValid() || this.submitting()) return;
    this.submitting.set(true);
    this.data.setPassword(this.userId(), { password: this.password() }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.password.set('');
        this.confirm.set('');
        this.notify.success(this.transloco.translate('users.password.savedToast'));
        this.closed.emit();
      },
      error: err => {
        this.submitting.set(false);
        this.notify.apiError(err, this.transloco.translate('users.password.error'));
      },
    });
  }

  private reset(): void {
    this.password.set('');
    this.confirm.set('');
    this.submitting.set(false);
  }
}
