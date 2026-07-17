import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { Router } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
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
import { toQueryKitParams } from '../../../../../shared/utils/primeng-querykit';
import { RoleCreateDialog } from '../role-create-dialog/role-create-dialog';

type AudienceSeverity = 'info' | 'success' | 'warn' | 'secondary';
type TypeSeverity = 'danger' | 'info' | 'secondary';

const SEARCH_FIELDS = ['name', 'description'];

@Component({
  selector: 'mp-role-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Button, Card, InputText, TableModule, Skeleton, Tag, Tooltip, PageHeader, RoleCreateDialog, TranslocoDirective],
  providers: [provideTranslocoScope('roles')],
  templateUrl: './role-list-page.html',
})
export class RoleListPage {
  private readonly data = inject(RolesDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);
  private readonly router = inject(Router);

  readonly rows = signal<RoleSummaryResponse[]>([]);
  readonly totalRecords = signal(0);
  readonly loading = signal(false);
  readonly createOpen = signal(false);

  private lastEvent: TableLazyLoadEvent | null = null;

  readonly headerActions = computed<HeaderAction[]>(() => [
    {
      label: this.transloco.translate('common.new'),
      icon: 'fa-solid fa-plus',
      severity: 'primary',
      command: () => this.openCreate(),
    },
  ]);

  load(event: TableLazyLoadEvent): void {
    this.lastEvent = event;
    this.loading.set(true);
    this.data.list(toQueryKitParams(event, { globalFields: SEARCH_FIELDS })).subscribe({
      next: page => {
        this.rows.set(page.items ?? []);
        this.totalRecords.set(page.totalItems ?? 0);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('roles.loadError'));
      },
    });
  }

  reload(): void {
    if (this.lastEvent) this.load(this.lastEvent);
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
    void this.router.navigate(['/staff/system/roles', id]);
  }

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
        this.reload();
      },
      error: err => this.notify.apiError(err, this.transloco.translate('roles.deleteError')),
    });
  }
}
