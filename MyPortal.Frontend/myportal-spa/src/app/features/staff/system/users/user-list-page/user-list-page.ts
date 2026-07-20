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
import { UsersDataService } from '../../../../../shared/services/users-data.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { UserSummaryResponse } from '../../../../../shared/types/user';
import { UserType } from '../../../../../core/types/user-type';
import { GridState } from '../../../../../shared/utils/querykit';
import { injectGridList } from '../../../../../shared/utils/grid-list';

type AudienceSeverity = 'info' | 'success' | 'warn' | 'secondary';

const SEARCH_FIELDS = ['username', 'email', 'personFullName'];

const GRID_DEFAULTS: GridState = { first: 0, rows: 25 };

@Component({
  selector: 'mp-user-list-page',
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
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('users')],
  templateUrl: './user-list-page.html',
})
export class UserListPage implements OnInit {
  private readonly data = inject(UsersDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);
  private readonly router = inject(Router);

  readonly canEdit = signal(false);

  private readonly table = viewChild(MpTable);

  protected readonly grid = injectGridList<UserSummaryResponse>({
    list: params => this.data.list(params),
    searchFields: SEARCH_FIELDS,
    defaults: GRID_DEFAULTS,
    table: this.table,
    onError: err => this.notify.apiError(err, this.transloco.translate('users.loadError')),
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
      this.canEdit.set(me.permissions?.includes(Permissions.SystemAdmin.EditUsers) ?? false);
    });
  }

  openCreate(): void {
    void this.router.navigate(['/staff/system/users/new']);
  }

  openEdit(user: UserSummaryResponse): void {
    void this.router.navigate(['/staff/system/users', user.id]);
  }

  onRowClick(event: MouseEvent, user: UserSummaryResponse): void {
    if ((event.target as HTMLElement).closest('a,button')) return;
    this.openEdit(user);
  }

  audienceLabel(userType: UserType): string {
    return this.transloco.translate('users.userType.' + UserType[userType].toLowerCase());
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

  async deleteUser(user: UserSummaryResponse): Promise<void> {
    if (!this.canEdit() || user.isSystem) return;

    const ok = await this.confirm.danger({
      message: this.transloco.translate('users.deleteConfirm', { name: user.username }),
    });
    if (!ok) return;

    this.data.delete(user.id).subscribe({
      next: () => {
        this.notify.success(this.transloco.translate('users.deletedToast'));
        this.grid.reload();
      },
      error: err => this.notify.apiError(err, this.transloco.translate('users.deleteError')),
    });
  }
}
