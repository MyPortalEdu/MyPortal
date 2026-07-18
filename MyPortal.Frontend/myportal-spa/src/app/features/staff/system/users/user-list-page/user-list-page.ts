import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { FilterMetadata } from 'primeng/api';
import { Table, TableLazyLoadEvent, TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
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
import {
  GridState,
  gridStateFromLazyLoadEvent,
  gridStateFromQueryParams,
  gridStateToQueryParams,
  toQueryKitParams,
} from '../../../../../shared/utils/primeng-querykit';

type AudienceSeverity = 'info' | 'success' | 'warn' | 'secondary';

const SEARCH_FIELDS = ['username', 'email', 'personFullName'];

const GRID_DEFAULTS: GridState = { first: 0, rows: 25 };

@Component({
  selector: 'mp-user-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Button, Card, InputText, TableModule, Tag, Tooltip, RouterLink, PageHeader, EmptyState, TranslocoDirective],
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
  private readonly route = inject(ActivatedRoute);

  readonly rows = signal<UserSummaryResponse[]>([]);
  readonly totalRecords = signal(0);
  readonly loading = signal(false);
  readonly canEdit = signal(false);

  // Read the URL once from the snapshot rather than subscribing to queryParams:
  // load() writes the state back, and a subscription would turn that write into
  // another load → navigate feedback loop.
  readonly initialState = gridStateFromQueryParams(this.route.snapshot.queryParamMap, GRID_DEFAULTS);

  // Seeded onto the table's `filters` input so the one lazy-load it fires on init
  // already carries the restored search term — no second, redundant request.
  readonly initialFilters: Record<string, FilterMetadata> = this.initialState.global
    ? { global: { value: this.initialState.global, matchMode: 'contains' } }
    : {};

  private readonly table = viewChild<Table>('dt');

  // Mirrors the search box so the clear affordance and the empty state can both
  // read it; the table's own `filters` stay the source of truth for the request.
  readonly searchTerm = signal(this.initialState.global ?? '');

  readonly hasFilter = computed(() => this.searchTerm().trim().length > 0);

  private lastEvent: TableLazyLoadEvent | null = null;

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

  load(event: TableLazyLoadEvent): void {
    this.lastEvent = event;
    this.syncUrl(event);
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

  onSearch(value: string): void {
    this.searchTerm.set(value);
    this.table()?.filterGlobal(value, 'contains');
  }

  // Same path as any other search change: the table drops the `global` filter and
  // re-fires onLazyLoad, so load() → syncUrl() strips `q` from the URL itself.
  clearSearch(): void {
    this.onSearch('');
  }

  private syncUrl(event: TableLazyLoadEvent): void {
    const state = gridStateFromLazyLoadEvent(event, { defaultRows: GRID_DEFAULTS.rows });
    // replaceUrl: rewrite the list URL in place. Without it every keystroke and
    // page change would push a history entry and Back would walk through them.
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: gridStateToQueryParams(state, GRID_DEFAULTS),
      replaceUrl: true,
    });
  }

  openCreate(): void {
    void this.router.navigate(['/staff/system/users/new']);
  }

  openEdit(user: UserSummaryResponse): void {
    void this.router.navigate(['/staff/system/users', user.id]);
  }

  // Row click is a convenience over the username link; ignore clicks that started
  // on the link or a row action so they aren't handled twice.
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
        this.reload();
      },
      error: err => this.notify.apiError(err, this.transloco.translate('users.deleteError')),
    });
  }
}
