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
import { MpButton, MpDialog, MpDialogFooter, MpInput, MpSelect } from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { UserType } from '../../../../../core/types/user-type';

interface AudienceOption {
  label: string;
  value: UserType;
}

@Component({
  selector: 'mp-role-create-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpButton, MpDialog, MpDialogFooter, MpInput, MpSelect, TranslocoDirective, TranslocoPipe],
  providers: [provideTranslocoScope('roles')],
  templateUrl: './role-create-dialog.html',
})
export class RoleCreateDialog {
  private readonly data = inject(RolesDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly confirmDialog = inject(ConfirmationDialog);

  readonly visible = input.required<boolean>();
  readonly closed = output<void>();
  readonly created = output<string>();

  readonly name = signal('');
  readonly userType = signal<UserType>(UserType.Staff);
  readonly submitting = signal(false);

  readonly audienceOptions = computed<AudienceOption[]>(() => [
    { label: this.transloco.translate('roles.audience.staff'), value: UserType.Staff },
    { label: this.transloco.translate('roles.audience.student'), value: UserType.Student },
    { label: this.transloco.translate('roles.audience.parent'), value: UserType.Parent },
  ]);

  readonly isValid = computed(() => this.name().trim().length > 0);
  readonly isDirty = computed(() => this.name().trim().length > 0 || this.userType() !== UserType.Staff);

  constructor() {
    effect(() => {
      if (this.visible()) {
        untracked(() => this.reset());
      }
    });
  }

  async onCancel(): Promise<void> {
    await this.requestClose();
  }

  onHide(): void {
    this.closed.emit();
  }

  save(): void {
    if (!this.isValid() || this.submitting()) return;
    this.submitting.set(true);

    this.data
      .create({ name: this.name().trim(), description: null, userType: this.userType(), permissionIds: [] })
      .subscribe({
        next: res => {
          this.submitting.set(false);
          this.name.set('');
          this.notify.success(this.transloco.translate('roles.form.createdToast'));
          this.created.emit(res.id);
        },
        error: err => {
          this.submitting.set(false);
          this.notify.apiError(err, this.transloco.translate('roles.form.errorCreate'));
        },
      });
  }

  private reset(): void {
    this.name.set('');
    this.userType.set(UserType.Staff);
    this.submitting.set(false);
  }

  private async requestClose(): Promise<void> {
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
}
