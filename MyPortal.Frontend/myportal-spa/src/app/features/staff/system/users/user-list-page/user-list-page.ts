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
import { UsersDataService } from '../../../../../shared/services/users-data.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { UserSummaryResponse } from '../../../../../shared/types/user';
import { UserType } from '../../../../../core/types/user-type';
import { toQueryKitParams } from '../../../../../shared/utils/primeng-querykit';

type AudienceSeverity = 'info' | 'success' | 'warn' | 'secondary';

const SEARCH_FIELDS = ['username', 'email', 'personFullName'];

@Component({
  selector: 'mp-user-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Button, Card, InputText, TableModule, Skeleton, Tag, Tooltip, PageHeader, TranslocoDirective],
  providers: [provideTranslocoScope('users')],
  templateUrl: './user-list-page.html',
})
export class UserListPage {
  private readonly data = inject(UsersDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);
  private readonly router = inject(Router);

  readonly rows = signal<UserSummaryResponse[]>([]);
  readonly totalRecords = signal(0);
  readonly loading = signal(false);

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
        this.notify.apiError(err, this.transloco.translate('users.loadError'));
      },
    });
  }

  reload(): void {
    if (this.lastEvent) this.load(this.lastEvent);
  }

  openCreate(): void {
    void this.router.navigate(['/staff/system/users/new']);
  }

  openEdit(user: UserSummaryResponse): void {
    void this.router.navigate(['/staff/system/users', user.id]);
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
    if (user.isSystem) return;

    const ok = await this.confirm.danger({
      message: this.transloco.translate('users.deleteConfirm', { name: user.username }),
    });
    if (!ok) return;

    this.data.delete(user.id).subscribe({
      next: () => {
        this.notify.success(this.transloco.translate('users.deletedToast'));
        this.reload();
      },
      error: err => this.notify.apiError(err, this.transloco.translate('users.deleteError')),
    });
  }
}
