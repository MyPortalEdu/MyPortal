import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  MpButton,
  MpCard,
  MpInput,
  MpTable,
  MpTableCaption,
  MpTableHeader,
  MpTableBody,
  MpTableEmpty,
  MpSortable,
  MpSortIcon,
  MpBadge,
} from '@myportal/ui';
import { MpTooltip } from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../../shared/components/empty-state/empty-state';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { RoleSummaryResponse } from '../../../../../shared/types/role';
import { UserType } from '../../../../../core/types/user-type';
import { GridState } from '../../../../../shared/utils/querykit';
import { injectGridList } from '../../../../../shared/utils/grid-list';
import { RoleCreateDialog } from '../role-create-dialog/role-create-dialog';

type AudienceSeverity = 'info' | 'success' | 'warn' | 'secondary';
type TypeSeverity = 'danger' | 'info' | 'secondary';

const SEARCH_FIELDS = ['name', 'description'];

const GRID_DEFAULTS: GridState = { first: 0, rows: 25 };

@Component({
  selector: 'mp-role-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MpButton,
    MpCard,
    MpInput,
    MpTable,
    MpTableCaption,
    MpTableHeader,
    MpTableBody,
    MpTableEmpty,
    MpSortable,
    MpSortIcon,
    MpBadge,
    MpTooltip,
    RouterLink,
    PageHeader,
    EmptyState,
    RoleCreateDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('roles')],
  templateUrl: './role-list-page.html',
})
export class RoleListPage implements OnInit {
  private readonly data = inject(RolesDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);
  private readonly router = inject(Router);

  readonly createOpen = signal(false);
  readonly canEdit = signal(false);

  private readonly table = viewChild(MpTable);

  protected readonly grid = injectGridList<RoleSummaryResponse>({
    list: params => this.data.list(params),
    searchFields: SEARCH_FIELDS,
    defaults: GRID_DEFAULTS,
    table: this.table,
    onError: err => this.notify.apiError(err, this.transloco.translate('roles.loadError')),
  });

  readonly headerActions = computed<HeaderAction[]>(() =>
    this.canEdit()
      ? [
          {
            label: this.transloco.translate('common.new'),
            icon: 'fa-solid fa-plus',
            severity: 'primary',
            command: () => this.openCreate(),
          },
        ]
      : [],
  );

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canEdit.set(me.permissions?.includes(Permissions.SystemAdmin.EditRoles) ?? false);
    });
  }

  openCreate(): void {
    this.createOpen.set(true);
  }

  openEdit(role: RoleSummaryResponse): void {
    void this.router.navigate(['/staff/system/roles', role.id]);
  }

  onRowClick(event: MouseEvent, role: RoleSummaryResponse): void {
    if ((event.target as HTMLElement).closest('a,button')) return;
    this.openEdit(role);
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
    if (!this.canEdit() || !this.canDelete(role)) return;

    const ok = await this.confirm.danger({
      message: this.transloco.translate('roles.deleteConfirm', { name: role.name }),
    });
    if (!ok) return;

    this.data.delete(role.id).subscribe({
      next: () => {
        this.notify.success(this.transloco.translate('roles.deletedToast'));
        this.grid.reload();
      },
      error: err => this.notify.apiError(err, this.transloco.translate('roles.deleteError')),
    });
  }
}
