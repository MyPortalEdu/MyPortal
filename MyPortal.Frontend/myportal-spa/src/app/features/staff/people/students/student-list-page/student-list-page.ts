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

import { StudentStatus } from '../../../../../shared/types/student-header';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../../shared/components/empty-state/empty-state';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { StudentsDataService } from '../../../../../shared/services/students-data.service';
import { StudentSummaryResponse } from '../../../../../shared/types/student-summary';
import { NotificationService } from '../../../../../core/services/notification.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { GridState } from '../../../../../shared/utils/querykit';
import { GridListController, injectGridList } from '../../../../../shared/utils/grid-list';
import { StudentCreateDialog } from '../student-create-dialog/student-create-dialog';

const SEARCH_FIELDS = ['firstName', 'lastName', 'preferredFirstName', 'preferredLastName'];

const DEFAULT_STATUS: StudentStatus | 'All' = 'Active';

const GRID_DEFAULTS: GridState = { first: 0, rows: 25, filters: { status: DEFAULT_STATUS } };

@Component({
  selector: 'mp-student-list-page',
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
    StudentCreateDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('students')],
  templateUrl: './student-list-page.html',
})
export class StudentListPage implements OnInit {
  private readonly data = inject(StudentsDataService);
  private readonly router = inject(Router);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);

  protected readonly createOpen = signal(false);
  protected readonly canCreate = signal(false);

  private readonly table = viewChild(MpTable);

  protected readonly grid: GridListController<StudentSummaryResponse> =
    injectGridList<StudentSummaryResponse>({
      list: params => this.data.list(params),
      searchFields: SEARCH_FIELDS,
      defaults: GRID_DEFAULTS,
      table: this.table,
      onError: err => this.notify.apiError(err, this.transloco.translate('students.loadError')),
      filters: () => ({ status: this.statusFilter() }),
    });

  protected readonly statusFilter = signal<StudentStatus | 'All'>(
    (this.grid.initialState.filters?.['status'] ?? DEFAULT_STATUS) as StudentStatus | 'All',
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

  protected readonly statusOptions = computed<{ label: string; value: StudentStatus | 'All' }[]>(() => [
    { label: this.transloco.translate('students.statusFilter.all'), value: 'All' },
    { label: this.transloco.translate('students.status.Active'), value: 'Active' },
    { label: this.transloco.translate('students.status.Future'), value: 'Future' },
    { label: this.transloco.translate('students.status.Leaver'), value: 'Leaver' },
    { label: this.transloco.translate('students.status.None'), value: 'None' },
  ]);

  protected readonly headerActions = computed<HeaderAction[]>(() =>
    this.canCreate()
      ? [
          {
            label: this.transloco.translate('students.new'),
            icon: 'fa-solid fa-plus',
            severity: 'primary',
            command: () => this.createOpen.set(true),
          },
        ]
      : [],
  );

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canCreate.set(me.permissions?.includes(Permissions.Student.EditStudentBasicDetails) ?? false);
    });
  }

  openDetails(row: StudentSummaryResponse): void {
    this.router.navigate(['/staff/people/students', row.id]);
  }

  protected onRowClick(event: MouseEvent, row: StudentSummaryResponse): void {
    if ((event.target as HTMLElement).closest('a,button')) return;
    this.openDetails(row);
  }

  protected onStatusFilterChange(value: StudentStatus | 'All'): void {
    this.statusFilter.set(value);
    this.table()?.filter(value === 'All' ? null : value, 'status', 'equals');
  }

  protected clearFilters(): void {
    this.grid.clearSearch();
    this.onStatusFilterChange(DEFAULT_STATUS);
  }

  protected statusSeverity(
    status: StudentStatus,
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

  protected displayName(row: StudentSummaryResponse): string {
    const first = row.preferredFirstName?.trim() || row.firstName;
    const last = row.preferredLastName?.trim() || row.lastName;
    return `${first} ${last}`.trim();
  }

  protected legalName(row: StudentSummaryResponse): string | null {
    const legal = `${row.firstName} ${row.lastName}`.trim();
    return legal === this.displayName(row) ? null : legal;
  }

  protected initials(row: StudentSummaryResponse): string {
    const first = (row.preferredFirstName?.trim() || row.firstName).charAt(0);
    const last = (row.preferredLastName?.trim() || row.lastName).charAt(0);
    return (first + last).toUpperCase();
  }

  protected onCreateClosed(): void {
    this.createOpen.set(false);
  }

  protected onCreated(studentId: string): void {
    this.createOpen.set(false);
    this.router.navigate(['/staff/people/students', studentId]);
  }

  protected onOpenExisting(studentId: string): void {
    this.createOpen.set(false);
    this.router.navigate(['/staff/people/students', studentId]);
  }
}
