import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { Skeleton } from 'primeng/skeleton';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { RoleSummaryResponse } from '../../../../../shared/types/role';
import { UserType } from '../../../../../core/types/user-type';
import { RoleFormDialog } from '../role-form-dialog/role-form-dialog';

type AudienceSeverity = 'info' | 'success' | 'warn' | 'secondary';
type TypeSeverity = 'danger' | 'info' | 'secondary';

@Component({
  selector: 'mp-role-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Button, Card, TableModule, Skeleton, Tag, Tooltip, PageHeader, RoleFormDialog, TranslocoDirective],
  providers: [provideTranslocoScope('roles')],
  templateUrl: './role-list-page.html',
})
export class RoleListPage implements OnInit {
  private readonly data = inject(RolesDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly roles = signal<RoleSummaryResponse[]>([]);
  readonly loading = signal(false);
  readonly dialogOpen = signal(false);
  // Non-null when the dialog is editing an existing role; null = create.
  readonly editRoleId = signal<string | null>(null);

  readonly skeletonRows = [1, 2, 3, 4, 5];

  readonly headerActions = computed<HeaderAction[]>(() => [
    {
      label: this.transloco.translate('common.new'),
      icon: 'fa-solid fa-plus',
      severity: 'primary',
      command: () => this.openCreate(),
    },
  ]);

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.data.list().subscribe({
      next: page => {
        this.roles.set(page?.items ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('roles.loadError'));
      },
    });
  }

  openCreate(): void {
    this.editRoleId.set(null);
    this.dialogOpen.set(true);
  }

  openEdit(role: RoleSummaryResponse): void {
    this.editRoleId.set(role.id);
    this.dialogOpen.set(true);
  }

  closeDialog(): void {
    this.dialogOpen.set(false);
    this.editRoleId.set(null);
  }

  onSaved(): void {
    this.dialogOpen.set(false);
    this.editRoleId.set(null);
    this.refresh();
  }

  // Both IsSystem and IsDefault roles are protected from deletion (see RoleService).
  canDelete(role: RoleSummaryResponse): boolean {
    return !role.isSystem && !role.isDefault;
  }

  audienceLabel(userType: UserType): string {
    return this.transloco.translate('roles.audience.' + UserType[userType].toLowerCase());
  }

  audienceSeverity(userType: UserType): AudienceSeverity {
    switch (userType) {
      case UserType.Staff:
        return 'info';
      case UserType.Student:
        return 'success';
      case UserType.Parent:
        return 'warn';
      default:
        return 'secondary';
    }
  }

  typeLabel(role: RoleSummaryResponse): string {
    if (role.isSystem) return this.transloco.translate('roles.type.system');
    if (role.isDefault) return this.transloco.translate('roles.type.default');
    return this.transloco.translate('roles.type.custom');
  }

  typeSeverity(role: RoleSummaryResponse): TypeSeverity {
    if (role.isSystem) return 'danger';
    if (role.isDefault) return 'info';
    return 'secondary';
  }

  async deleteRole(role: RoleSummaryResponse): Promise<void> {
    if (!this.canDelete(role)) return;

    const ok = await this.confirm.danger({
      message: this.transloco.translate('roles.deleteConfirm', { name: role.name }),
    });
    if (!ok) return;

    this.data.delete(role.id).subscribe({
      next: () => {
        this.notify.success(this.transloco.translate('roles.deletedToast'));
        this.refresh();
      },
      error: err => this.notify.apiError(err, this.transloco.translate('roles.deleteError')),
    });
  }
}
