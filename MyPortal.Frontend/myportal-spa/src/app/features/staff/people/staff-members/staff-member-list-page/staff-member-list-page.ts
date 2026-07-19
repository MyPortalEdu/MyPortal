import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  MpButton,
  MpCard,
  MpInput,
  MpSelect,
  MpTable,
  MpTableCaption,
  MpTableHeader,
  MpTableBody,
  MpTableEmpty,
  MpSortable,
  MpSortIcon,
  MpBadge,
  type MpFilterMetadata,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { StaffStatus } from '../../../../../shared/types/staff-member-header';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../../shared/components/empty-state/empty-state';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { StaffMembersDataService } from '../../../../../shared/services/staff-members-data.service';
import { StaffMemberSummaryResponse } from '../../../../../shared/types/staff-member';
import { NotificationService } from '../../../../../core/services/notification.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { GridState } from '../../../../../shared/utils/querykit';
import { GridListController, injectGridList } from '../../../../../shared/utils/grid-list';
import { StaffMemberCreateDialog } from '../staff-member-create-dialog/staff-member-create-dialog';

const SEARCH_FIELDS = ['firstName', 'lastName', 'preferredFirstName', 'preferredLastName', 'code'];

const DEFAULT_STATUS: StaffStatus | 'All' = 'Active';

const GRID_DEFAULTS: GridState = { first: 0, rows: 25, filters: { status: DEFAULT_STATUS } };

@Component({
  selector: 'mp-staff-member-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MpButton,
    MpCard,
    MpInput,
    MpSelect,
    MpTable,
    MpTableCaption,
    MpTableHeader,
    MpTableBody,
    MpTableEmpty,
    MpSortable,
    MpSortIcon,
    MpBadge,
    FormsModule,
    RouterLink,
    PageHeader,
    EmptyState,
    StaffMemberCreateDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-members')],
  templateUrl: './staff-member-list-page.html',
})
export class StaffMemberListPage implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly router = inject(Router);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);

  protected readonly createOpen = signal(false);
  protected readonly canCreate = signal(false);

  private readonly table = viewChild(MpTable);

  protected readonly grid: GridListController<StaffMemberSummaryResponse> =
    injectGridList<StaffMemberSummaryResponse>({
      list: params => this.data.list(params),
      searchFields: SEARCH_FIELDS,
      defaults: GRID_DEFAULTS,
      table: this.table,
      onError: err => this.notify.apiError(err, this.transloco.translate('staff-members.loadError')),
      filters: () => ({ status: this.statusFilter() }),
    });

  protected readonly statusFilter = signal<StaffStatus | 'All'>(
    (this.grid.initialState.filters?.['status'] ?? DEFAULT_STATUS) as StaffStatus | 'All',
  );

  protected readonly hasFilter = computed(
    () => this.grid.hasFilter() || this.statusFilter() !== DEFAULT_STATUS,
  );

  protected readonly initialFilters: Record<string, MpFilterMetadata> = {
    ...this.grid.initialFilters,
    ...(this.statusFilter() !== 'All'
      ? { status: { value: this.statusFilter(), matchMode: 'equals' } }
      : {}),
  };

  protected readonly statusOptions = computed<{ label: string; value: StaffStatus | 'All' }[]>(() => [
    { label: this.transloco.translate('staff-members.statusFilter.all'), value: 'All' },
    { label: this.transloco.translate('staff-members.status.Active'), value: 'Active' },
    { label: this.transloco.translate('staff-members.status.Future'), value: 'Future' },
    { label: this.transloco.translate('staff-members.status.Leaver'), value: 'Leaver' },
    { label: this.transloco.translate('staff-members.status.None'), value: 'None' },
  ]);

  protected readonly headerActions = computed<HeaderAction[]>(() =>
    this.canCreate()
      ? [
          {
            label: this.transloco.translate('staff-members.new'),
            icon: 'fa-solid fa-plus',
            severity: 'primary',
            command: () => this.createOpen.set(true),
          },
        ]
      : [],
  );

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canCreate.set(me.permissions?.includes(Permissions.Staff.EditAllStaffBasicDetails) ?? false);
    });
  }

  openDetails(row: StaffMemberSummaryResponse): void {
    this.router.navigate(['/staff/people/staff-members', row.id]);
  }

  protected onRowClick(event: MouseEvent, row: StaffMemberSummaryResponse): void {
    if ((event.target as HTMLElement).closest('a,button')) return;
    this.openDetails(row);
  }

  protected onStatusFilterChange(value: StaffStatus | 'All'): void {
    this.statusFilter.set(value);
    this.table()?.filter(value === 'All' ? null : value, 'status', 'equals');
  }

  protected clearFilters(): void {
    this.grid.clearSearch();
    this.onStatusFilterChange(DEFAULT_STATUS);
  }

  protected statusSeverity(
    status: StaffStatus,
  ): 'success' | 'info' | 'warn' | 'secondary' | 'contrast' {
    switch (status) {
      case 'Active':
        return 'success';
      case 'Future':
        return 'info';
      case 'Leaver':
        return 'warn';
      case 'Archived':
        return 'contrast';
      default:
        return 'secondary';
    }
  }

  protected displayName(row: StaffMemberSummaryResponse): string {
    const first = row.preferredFirstName?.trim() || row.firstName;
    const last = row.preferredLastName?.trim() || row.lastName;
    return `${first} ${last}`.trim();
  }

  protected legalName(row: StaffMemberSummaryResponse): string | null {
    const legal = `${row.firstName} ${row.lastName}`.trim();
    return legal === this.displayName(row) ? null : legal;
  }

  protected initials(row: StaffMemberSummaryResponse): string {
    const first = (row.preferredFirstName?.trim() || row.firstName).charAt(0);
    const last = (row.preferredLastName?.trim() || row.lastName).charAt(0);
    return (first + last).toUpperCase();
  }

  protected onCreateClosed(): void {
    this.createOpen.set(false);
  }

  protected onCreated(staffMemberId: string): void {
    this.createOpen.set(false);
    this.router.navigate(['/staff/people/staff-members', staffMemberId]);
  }

  protected onOpenExisting(staffMemberId: string): void {
    this.createOpen.set(false);
    this.router.navigate(['/staff/people/staff-members', staffMemberId]);
  }
}
