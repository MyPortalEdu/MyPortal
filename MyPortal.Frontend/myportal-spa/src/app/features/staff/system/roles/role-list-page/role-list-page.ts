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
import { Router } from '@angular/router';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { RoleSummaryResponse } from '../../../../../shared/types/role';
import { UserType } from '../../../../../core/types/user-type';
import { RoleCreateDialog } from '../role-create-dialog/role-create-dialog';

type AudienceSeverity = 'info' | 'success' | 'warn' | 'secondary';
type TypeSeverity = 'danger' | 'info' | 'secondary';

@Component({
  selector: 'mp-role-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Button, Card, TableModule, Skeleton, Tag, Tooltip, PageHeader, RoleCreateDialog, TranslocoDirective],
  providers: [provideTranslocoScope('roles')],
  templateUrl: './role-list-page.html',
})
export class RoleListPage implements OnInit {
  private readonly data = inject(RolesDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);
  private readonly router = inject(Router);

  readonly roles = signal<RoleSummaryResponse[]>([]);
  readonly loading = signal(false);
  readonly createOpen = signal(false);

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
    this.createOpen.set(true);
  }

  openEdit(role: RoleSummaryResponse): void {
    void this.router.navigate(['/staff/system/roles', role.id]);
  }

  closeCreate(): void {
    this.createOpen.set(false);
  }

  onCreated(id: string): void {
    this.createOpen.set(false);
    // Straight into the new role's editor to assign permissions.
    void this.router.navigate(['/staff/system/roles', id]);
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
