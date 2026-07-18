import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MpButton, MpInput, MpSkeleton } from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../shared/components/page-header/page-header';
import { MeService } from '../../../core/services/me-service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../core/services/confirmation.service';
import { CanComponentDeactivate } from '../../../core/guards/can-deactivate.guard';
import { Me } from '../../../core/types/me';
import { UserType } from '../../../core/types/user-type';

/**
 * Self-service account settings for the signed-in user: read-only identity fields
 * plus a change-password form backed by PUT /api/me/password (which verifies the
 * current password server-side). Reachable by every user type via /portal/settings.
 */
@Component({
  selector: 'mp-account-settings-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpButton, MpInput, MpSkeleton, PageHeader, TranslocoDirective],
  providers: [provideTranslocoScope('account-settings')],
  templateUrl: './account-settings-page.html',
})
export class AccountSettingsPage implements OnInit, CanComponentDeactivate {
  private readonly meService = inject(MeService);
  private readonly notify = inject(NotificationService);
  private readonly confirmDialog = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly me = signal<Me | null>(null);
  readonly loading = signal(true);

  readonly currentPassword = signal('');
  readonly newPassword = signal('');
  readonly confirm = signal('');
  readonly submitting = signal(false);

  // Relative to the template's `account-settings` transloco prefix — the `t()` in
  // the template prepends the scope, so returning a fully-qualified key would double it.
  readonly userTypeKey = computed(() => {
    switch (this.me()?.userType) {
      case UserType.Staff: return 'userType.staff';
      case UserType.Student: return 'userType.student';
      case UserType.Parent: return 'userType.parent';
      default: return 'userType.unknown';
    }
  });

  readonly mismatch = computed(
    () => this.confirm().length > 0 && this.newPassword() !== this.confirm());

  // New password can't equal the current one — a no-op change that Identity would reject anyway.
  readonly sameAsCurrent = computed(
    () => this.newPassword().length > 0 && this.newPassword() === this.currentPassword());

  readonly isValid = computed(() =>
    this.currentPassword().length > 0 &&
    this.newPassword().length > 0 &&
    this.newPassword() === this.confirm() &&
    !this.sameAsCurrent());

  readonly isDirty = computed(() =>
    this.currentPassword().length > 0 ||
    this.newPassword().length > 0 ||
    this.confirm().length > 0);

  ngOnInit(): void {
    this.meService.me().subscribe({
      next: me => {
        this.me.set(me);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('account-settings.loadError'));
      },
    });
  }

  canDeactivate(): boolean | Promise<boolean> {
    if (!this.isDirty()) return true;
    return this.confirmDialog.confirm({
      header: this.transloco.translate('common.discardChanges'),
      message: this.transloco.translate('common.discardConfirm'),
      acceptLabel: this.transloco.translate('common.discard'),
      acceptSeverity: 'danger',
    });
  }

  save(): void {
    if (!this.isValid() || this.submitting()) return;
    this.submitting.set(true);
    this.meService
      .changePassword({ currentPassword: this.currentPassword(), password: this.newPassword() })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.resetForm();
          this.notify.success(this.transloco.translate('account-settings.password.savedToast'));
        },
        error: err => {
          this.submitting.set(false);
          this.notify.apiError(err, this.transloco.translate('account-settings.password.error'));
        },
      });
  }

  private resetForm(): void {
    this.currentPassword.set('');
    this.newPassword.set('');
    this.confirm.set('');
  }
}
