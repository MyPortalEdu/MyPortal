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
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
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
  type MpTableLazyLoadEvent,
  type MpFilterMetadata,
} from '@myportal/ui';
import { Tag } from 'primeng/tag';
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
import {
  GridState,
  gridStateFromLazyLoadEvent,
  gridStateFromQueryParams,
  gridStateToQueryParams,
  toQueryKitParams,
} from '../../../../../shared/utils/primeng-querykit';
import { StaffMemberCreateDialog } from '../staff-member-create-dialog/staff-member-create-dialog';

// Columns the single search box matches against. Includes preferred names so a
// search hits whatever the grid actually displays, not just the legal name.
const SEARCH_FIELDS = ['firstName', 'lastName', 'preferredFirstName', 'preferredLastName', 'code'];

// Status defaults to Active — the common case is "who's on staff now" — so a URL
// with no `status` param restores an Active-filtered grid.
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
    Tag,
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
  private readonly route = inject(ActivatedRoute);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);

  protected readonly rows = signal<StaffMemberSummaryResponse[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly loading = signal(false);
  protected readonly createOpen = signal(false);

  protected readonly canCreate = signal(false);

  private readonly table = viewChild(MpTable);

  // Read the URL once from the snapshot rather than subscribing to queryParams:
  // load() writes the state back, and a subscription would turn that write into
  // another load → navigate feedback loop.
  protected readonly initialState = gridStateFromQueryParams(
    this.route.snapshot.queryParamMap,
    GRID_DEFAULTS,
  );

  // Status filter for the grid. Restored from the URL, defaulting to Active — the
  // user can switch to a single status or All to surface future starters and
  // leavers. 'All' clears the status predicate entirely.
  protected readonly statusFilter = signal<StaffStatus | 'All'>(
    (this.initialState.filters?.['status'] ?? DEFAULT_STATUS) as StaffStatus | 'All',
  );

  // Mirrors the search box so the clear affordance and the empty state can both
  // read it; the table's own `filters` stay the source of truth for the request.
  protected readonly searchTerm = signal(this.initialState.global ?? '');

  // A filtered-empty grid can be caused by either input, so both count.
  protected readonly hasFilter = computed(
    () => this.searchTerm().trim().length > 0 || this.statusFilter() !== DEFAULT_STATUS,
  );

  // Seeded onto the table's `filters` input so the one lazy-load it fires on init
  // already carries the restored search term and status — no second round-trip.
  protected readonly initialFilters: Record<string, MpFilterMetadata> = {
    ...(this.initialState.global
      ? { global: { value: this.initialState.global, matchMode: 'contains' } }
      : {}),
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

  load(event: MpTableLazyLoadEvent): void {
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
        this.notify.apiError(err, this.transloco.translate('staff-members.loadError'));
      },
    });
  }

  private syncUrl(event: MpTableLazyLoadEvent): void {
    // The dropdown, not the event, is the source of truth for status: 'All' is
    // absence-of-predicate in the event but still a value the URL must carry.
    const state = gridStateFromLazyLoadEvent(event, {
      defaultRows: GRID_DEFAULTS.rows,
      filters: { status: this.statusFilter() },
    });
    // replaceUrl: rewrite the list URL in place. Without it every keystroke and
    // page change would push a history entry and Back would walk through them.
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: gridStateToQueryParams(state, GRID_DEFAULTS),
      replaceUrl: true,
    });
  }

  openDetails(row: StaffMemberSummaryResponse): void {
    this.router.navigate(['/staff/people/staff-members', row.id]);
  }

  // Row click is a convenience over the name link; ignore clicks that started on
  // the link or a row action so they aren't handled twice.
  protected onRowClick(event: MouseEvent, row: StaffMemberSummaryResponse): void {
    if ((event.target as HTMLElement).closest('a,button')) return;
    this.openDetails(row);
  }

  // Drive the table's `status` column filter from the dropdown. 'All' clears it
  // (null value → toQueryKitParams drops the predicate); otherwise an equals
  // match against the computed Status column. p-table.filter() triggers a reload.
  protected onStatusFilterChange(value: StaffStatus | 'All'): void {
    this.statusFilter.set(value);
    this.table()?.filter(value === 'All' ? null : value, 'status', 'equals');
  }

  protected onSearch(value: string): void {
    this.searchTerm.set(value);
    this.table()?.filterGlobal(value, 'contains');
  }

  // Same path as any other search change: the table drops the `global` filter and
  // re-fires onLazyLoad, so load() → syncUrl() strips `q` from the URL itself.
  protected clearSearch(): void {
    this.onSearch('');
  }

  // Both inputs go back to their defaults. Each p-table filter call restarts the
  // shared filterDelay timer, so the two land as one lazy-load, not two — and the
  // status signal is set before it fires, so syncUrl() sees the reset value.
  protected clearFilters(): void {
    this.clearSearch();
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

  // Name shown in the grid: preferred name wins over legal where set.
  protected displayName(row: StaffMemberSummaryResponse): string {
    const first = row.preferredFirstName?.trim() || row.firstName;
    const last = row.preferredLastName?.trim() || row.lastName;
    return `${first} ${last}`.trim();
  }

  // Legal name, shown as a muted second line only when it differs from the
  // preferred name above — so a row never repeats the same name twice.
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
    // Land straight on the new record so HR can fill in the other areas
    // (equality, employment, professional, etc.) via their PUTs.
    this.router.navigate(['/staff/people/staff-members', staffMemberId]);
  }

  protected onOpenExisting(staffMemberId: string): void {
    // HR picked someone already on staff — jump to their existing profile rather
    // than creating a duplicate.
    this.createOpen.set(false);
    this.router.navigate(['/staff/people/staff-members', staffMemberId]);
  }
}
