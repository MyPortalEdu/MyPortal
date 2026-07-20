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
import { FormField, form, required, submit, validate } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { MpButton, MpDialog, MpDialogFooter, MpFormField, MpInput, MpSelect } from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { UserType } from '../../../../../core/types/user-type';

interface AudienceOption {
  label: string;
  value: UserType;
}

interface RoleFormModel {
  name: string;
  userType: UserType;
}

@Component({
  selector: 'mp-role-create-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField, MpButton, MpDialog, MpDialogFooter, MpFormField, MpInput, MpSelect, TranslocoDirective, TranslocoPipe],
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

  protected readonly model = signal<RoleFormModel>({ name: '', userType: UserType.Staff });
  protected readonly f = form(this.model, path => {
    required(path.name);
    validate(path.name, ({ value }) =>
      value().trim().length ? undefined : { kind: 'blank', message: 'roles.form.nameBlank' },
    );
  });

  readonly audienceOptions = computed<AudienceOption[]>(() => [
    { label: this.transloco.translate('roles.audience.staff'), value: UserType.Staff },
    { label: this.transloco.translate('roles.audience.student'), value: UserType.Student },
    { label: this.transloco.translate('roles.audience.parent'), value: UserType.Parent },
  ]);

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

  save(): Promise<boolean> {
    return submit(this.f, async () => {
      const { name, userType } = this.model();
      let res: { id: string };
      try {
        res = await firstValueFrom(
          this.data.create({ name: name.trim(), description: null, userType, permissionIds: [] }),
        );
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('roles.form.errorCreate'));
        return;
      }
      this.notify.success(this.transloco.translate('roles.form.createdToast'));
      this.created.emit(res.id);
    });
  }

  private reset(): void {
    this.model.set({ name: '', userType: UserType.Staff });
    this.f().reset();
  }

  private async requestClose(): Promise<void> {
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
}
