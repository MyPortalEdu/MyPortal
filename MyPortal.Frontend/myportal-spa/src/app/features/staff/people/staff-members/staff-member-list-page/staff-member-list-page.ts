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
  MpTable,
  MpTableEmpty,
  MpCellDef,
  MpBadge,
  type MpColumn,
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
    MpTable,
    MpTableEmpty,
    MpCellDef,
    MpBadge,
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
      // Persist just the status to the URL (per-column text filters are transient).
      filters: (): Record<string, string> | undefined => {
        const status = this.table()?.filterValue('status');
        return status != null ? { status: String(status) } : undefined;
      },
    });

  private readonly initialStatus =
    (this.grid.initialState.filters?.['status'] ?? DEFAULT_STATUS) as StaffStatus | 'All';

  // Seeds the grid so it opens filtered to Active (the status column filter shows it).
  protected readonly initialFilters: Record<string, MpFilterMetadata> =
    this.initialStatus !== 'All' ? { status: { value: this.initialStatus, matchMode: 'equals' } } : {};

  // Any filter set beyond the default status → offer "clear filters" on an empty result. Reads the
  // table's active fields so it stays correct as columns are added.
  protected readonly hasFilter = computed(() => {
    const t = this.table();
    if (!t) return false;
    const status = t.filterValue('status');
    return (
      t.activeFilterFields().some(f => f !== 'status') ||
      (status != null && status !== DEFAULT_STATUS)
    );
  });

  protected readonly statusOptions = computed<{ label: string; value: StaffStatus | 'All' }[]>(() => [
    { label: this.transloco.translate('staff-members.statusFilter.all'), value: 'All' },
    { label: this.transloco.translate('staff-members.status.Active'), value: 'Active' },
    { label: this.transloco.translate('staff-members.status.Future'), value: 'Future' },
    { label: this.transloco.translate('staff-members.status.Leaver'), value: 'Leaver' },
    { label: this.transloco.translate('staff-members.status.None'), value: 'None' },
  ]);

  protected readonly genderOptions = computed<{ label: string; value: string }[]>(() => [
    { label: this.transloco.translate('staff-members.genderFilter.all'), value: 'All' },
    { label: this.transloco.translate('common.gender.M'), value: 'M' },
    { label: this.transloco.translate('common.gender.F'), value: 'F' },
    { label: this.transloco.translate('common.gender.U'), value: 'U' },
  ]);

  // The grid, described declaratively — header, sort and filters are generated from this; only the
  // custom cells (avatar, name link, gender label, date, status badge) need an mpCell template.
  protected readonly columns = computed<MpColumn[]>(() => {
    const t = (key: string) => this.transloco.translate(key);
    return [
      { field: 'avatar', width: '4rem' },
      {
        field: 'name', header: t('staff-members.columns.name'),
        sortable: true, sortField: 'lastName', filter: 'text', filterField: 'searchName'
      },
      {
        field: 'code', header: t('staff-members.columns.code'), sortable: true, filter: 'text',
        width: '10rem', cellClass: 'font-mono text-xs text-muted-foreground',
      },
      { field: 'role', header: t('staff-members.columns.role'), sortable: true, filter: 'text', hideBelow: 'lg' },
      {
        field: 'title', header: t('staff-members.columns.title'), sortable: true, filter: 'text',
        hideBelow: 'xl', cellClass: 'text-muted-foreground',
      },
      {
        field: 'gender', header: t('staff-members.columns.gender'), sortable: true, hideBelow: 'xl',
        width: '7rem', cellClass: 'text-muted-foreground',
        filter: { type: 'select', options: this.genderOptions(), clearValue: 'All' },
      },
      {
        field: 'startDate', header: t('staff-members.columns.startDate'),
        sortable: true, sortField: 'employmentStartDate', filter: 'date', filterField: 'startDateOnly',
        hideBelow: 'xl', cellClass: 'text-muted-foreground tabular-nums',
      },
      {
        field: 'status', header: t('staff-members.columns.status'), sortable: true, width: '9rem',
        filter: { type: 'select', options: this.statusOptions(), clearValue: 'All' },
      },
    ];
  });

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

  protected openDetails(row: StaffMemberSummaryResponse): void {
    this.router.navigate(['/staff/people/staff-members', row.id]);
  }

  protected clearFilters(): void {
    this.table()?.clearFilters();
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
    return last ? `${last}, ${first}`.trim() : first;
  }

  protected legalName(row: StaffMemberSummaryResponse): string | null {
    const legal = row.lastName ? `${row.lastName}, ${row.firstName}`.trim() : row.firstName;
    return legal === this.displayName(row) ? null : legal;
  }

  protected initials(row: StaffMemberSummaryResponse): string {
    const first = (row.preferredFirstName?.trim() || row.firstName).charAt(0);
    const last = (row.preferredLastName?.trim() || row.lastName).charAt(0);
    return (first + last).toUpperCase();
  }

  protected genderLabel(row: StaffMemberSummaryResponse): string {
    const key = (row.gender ?? '').trim().toUpperCase();
    return key === 'M' || key === 'F' || key === 'U'
      ? this.transloco.translate(`common.gender.${key}`)
      : (row.gender ?? '');
  }

  protected startDate(row: StaffMemberSummaryResponse): string {
    if (!row.employmentStartDate) return '';
    return new Date(row.employmentStartDate).toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    });
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
